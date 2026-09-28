using System;
using System.Collections.Generic;
using System.IO;
using BimCommands.EnvironmentConverter.Models;
using Tekla.Structures.Model;

namespace BimCommands.EnvironmentConverter.Core
{
    /// <summary>
    /// Analyzes a Tekla Structures model folder and/or live Tekla session to detect environment settings and contents.
    /// </summary>
    public static class ModelEnvironmentDetector
    {
        /// <summary>
        /// Checks if Tekla Structures is running and connected to an active model.
        /// </summary>
        public static bool IsTeklaConnected(out string modelPath, out string modelName)
        {
            modelPath = string.Empty;
            modelName = string.Empty;

            try
            {
                var model = new Model();
                if (model.GetConnectionStatus())
                {
                    var info = model.GetInfo();
                    if (info != null && !string.IsNullOrEmpty(info.ModelPath))
                    {
                        modelPath = info.ModelPath;
                        modelName = info.ModelName;
                        return true;
                    }
                }
            }
            catch
            {
                // Tekla API not available or no active model
            }

            return false;
        }

        /// <summary>
        /// Inspects a model folder on disk and parses its options and catalog state.
        /// </summary>
        public static ModelInspectionResult InspectModelFolder(string modelDir, bool scanLiveObjectsIfConnected = true)
        {
            if (string.IsNullOrEmpty(modelDir) || !Directory.Exists(modelDir))
            {
                throw new DirectoryNotFoundException($"Thư mục model không tồn tại: {modelDir}");
            }

            var result = new ModelInspectionResult
            {
                ModelPath = modelDir,
                ModelName = Path.GetFileName(modelDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            };

            // 1. Check local catalog files in model folder
            result.HasLocalProfileDb = File.Exists(Path.Combine(modelDir, "profdb.bin"));
            result.HasLocalMaterialDb = File.Exists(Path.Combine(modelDir, "matdb.bin"));
            result.HasLocalBoltDb = File.Exists(Path.Combine(modelDir, "screwdb.db"));
            result.HasLocalBoltAssDb = File.Exists(Path.Combine(modelDir, "assdb.db"));
            result.HasLocalRebarDb = File.Exists(Path.Combine(modelDir, "rebar_database.inp"));
            result.HasLocalRebarRules = File.Exists(Path.Combine(modelDir, "RebarShapeRules.xml"));
            result.HasLocalShapeCatalog = File.Exists(Path.Combine(modelDir, "ShapeCatalog.xml"));

            // 2. Parse options.ini and model.ini
            ParseModelOptions(modelDir, result);

            // 3. Scan live model objects if Tekla is open and currently pointing to this model
            if (scanLiveObjectsIfConnected)
            {
                TryScanLiveModelObjects(modelDir, result);
            }

            return result;
        }

        private static void ParseModelOptions(string modelDir, ModelInspectionResult result)
        {
            string optionsFile = Path.Combine(modelDir, "options.ini");
            if (File.Exists(optionsFile))
            {
                try
                {
                    var lines = File.ReadAllLines(optionsFile);
                    foreach (var line in lines)
                    {
                        string trimmed = line.Trim();
                        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//") || trimmed.StartsWith("#") || trimmed.StartsWith("rem", StringComparison.OrdinalIgnoreCase))
                            continue;

                        // Example: set XS_ENVIRONMENT_NAME=Korea
                        int eqIdx = trimmed.IndexOf('=');
                        if (eqIdx > 0)
                        {
                            string keyPart = trimmed.Substring(0, eqIdx).Trim();
                            string valPart = trimmed.Substring(eqIdx + 1).Trim();

                            if (keyPart.StartsWith("set ", StringComparison.OrdinalIgnoreCase))
                            {
                                keyPart = keyPart.Substring(4).Trim();
                            }

                            result.DetectedOptions[keyPart] = valPart;

                            if (keyPart.Equals("XS_ENVIRONMENT_NAME", StringComparison.OrdinalIgnoreCase) ||
                                keyPart.Equals("XS_KEY_ENVIRONMENT_NAME", StringComparison.OrdinalIgnoreCase))
                            {
                                result.DetectedEnvironment = valPart;
                            }
                            else if (keyPart.Equals("XS_ROLE_NAME", StringComparison.OrdinalIgnoreCase) ||
                                     keyPart.Equals("XS_KEY_ROLE_NAME", StringComparison.OrdinalIgnoreCase))
                            {
                                result.DetectedRole = valPart;
                            }
                        }
                    }
                }
                catch { }
            }

            // Fallback: If still unknown, check model.ini or environment.db
            if (result.DetectedEnvironment == "Unknown")
            {
                string modelIni = Path.Combine(modelDir, "model.ini");
                if (File.Exists(modelIni))
                {
                    try
                    {
                        var content = File.ReadAllText(modelIni);
                        if (content.IndexOf("korea", StringComparison.OrdinalIgnoreCase) >= 0)
                            result.DetectedEnvironment = "Korea";
                        else if (content.IndexOf("vietnam", StringComparison.OrdinalIgnoreCase) >= 0)
                            result.DetectedEnvironment = "Vietnam";
                        else if (content.IndexOf("south_east_asia", StringComparison.OrdinalIgnoreCase) >= 0)
                            result.DetectedEnvironment = "South_East_Asia";
                    }
                    catch { }
                }
            }
        }

        private static void TryScanLiveModelObjects(string modelDir, ModelInspectionResult result)
        {
            try
            {
                var model = new Model();
                if (!model.GetConnectionStatus()) return;

                var info = model.GetInfo();
                if (info == null || string.IsNullOrEmpty(info.ModelPath)) return;

                // Ensure the active model matches the inspected directory
                string activeDir = info.ModelPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string targetDir = modelDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                if (!string.Equals(activeDir, targetDir, StringComparison.OrdinalIgnoreCase))
                    return;

                result.IsOpenInActiveTekla = true;

                // Enumerate model objects using targeted types (Part, Reinforcement, BoltGroup)
                // to avoid pulling welds, cuts, fittings, planes, and grids across IPC
                var modelObjectSelector = model.GetModelObjectSelector();
                var targetTypes = new Type[] { typeof(Part), typeof(Reinforcement), typeof(BoltGroup) };
                var enumerator = modelObjectSelector.GetAllObjectsWithType(targetTypes);

                int partCount = 0;
                int rebarCount = 0;
                int boltCount = 0;

                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (current is Part part)
                    {
                        partCount++;
                        if (!string.IsNullOrEmpty(part.Profile?.ProfileString))
                            result.UsedProfiles.Add(part.Profile.ProfileString);
                        if (!string.IsNullOrEmpty(part.Material?.MaterialString))
                            result.UsedMaterials.Add(part.Material.MaterialString);
                    }
                    else if (current is Reinforcement rebar)
                    {
                        rebarCount++;
                        if (!string.IsNullOrEmpty(rebar.Grade))
                            result.UsedRebarGrades.Add(rebar.Grade);
                    }
                    else if (current is BoltGroup)
                    {
                        boltCount++;
                    }
                }

                result.PartCount = partCount;
                result.RebarCount = rebarCount;
                result.BoltCount = boltCount;
            }
            catch
            {
                // Non-fatal, simply skip live scanning
            }
        }
    }
}
