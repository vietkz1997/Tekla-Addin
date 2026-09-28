using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BimCommands.EnvironmentConverter.Models;

namespace BimCommands.EnvironmentConverter.Core
{
    /// <summary>
    /// Handles synchronization and merging of catalogs, rebar shape rules, attributes, and environment settings.
    /// </summary>
    public static class CatalogManager
    {
        public delegate void SyncLogHandler(string message, bool isSuccess = true);

        /// <summary>
        /// Synchronizes profile, material, bolt, and rebar databases from target environment to model folder.
        /// </summary>
        public static void SyncCatalogs(string modelPath, TeklaEnvironmentInfo targetEnv, SyncLogHandler logger)
        {
            if (string.IsNullOrEmpty(modelPath) || !Directory.Exists(modelPath)) return;
            if (targetEnv == null) return;

            logger?.Invoke("--- Bắt đầu đồng bộ cơ sở dữ liệu Catalogs ---");

            // 1. Profile catalog (.bin or .lis)
            if (!string.IsNullOrEmpty(targetEnv.ProfileCatalogPath) && File.Exists(targetEnv.ProfileCatalogPath))
            {
                string ext = Path.GetExtension(targetEnv.ProfileCatalogPath);
                string targetName = ext.Equals(".lis", StringComparison.OrdinalIgnoreCase) ? "profdb.lis" : "profdb.bin";
                SyncCatalogFile(modelPath, targetEnv.ProfileCatalogPath, targetName, logger);
            }

            // 2. Material catalog (.bin or .lis)
            if (!string.IsNullOrEmpty(targetEnv.MaterialCatalogPath) && File.Exists(targetEnv.MaterialCatalogPath))
            {
                string ext = Path.GetExtension(targetEnv.MaterialCatalogPath);
                string targetName = ext.Equals(".lis", StringComparison.OrdinalIgnoreCase) ? "matdb.lis" : "matdb.bin";
                SyncCatalogFile(modelPath, targetEnv.MaterialCatalogPath, targetName, logger);
            }

            // 3. Bolt catalog
            SyncCatalogFile(modelPath, targetEnv.BoltCatalogPath, "screwdb.db", logger);
            SyncCatalogFile(modelPath, targetEnv.BoltAssemblyCatalogPath, "assdb.db", logger);

            // 4. Rebar catalog
            SyncCatalogFile(modelPath, targetEnv.RebarDatabasePath, "rebar_database.inp", logger);
            if (!string.IsNullOrEmpty(targetEnv.RootPath))
            {
                string meshInp = Path.Combine(targetEnv.RootPath, "inp", "mesh_database.inp");
                if (File.Exists(meshInp))
                {
                    SyncCatalogFile(modelPath, meshInp, "mesh_database.inp", logger);
                }
            }

            logger?.Invoke("Đồng bộ Catalogs hoàn tất.", true);
        }

        /// <summary>
        /// Synchronizes RebarShapeRules.xml and ShapeCatalog.xml to prevent Unknown Shape issues.
        /// </summary>
        public static void SyncRebarShapeRules(string modelPath, TeklaEnvironmentInfo targetEnv, SyncLogHandler logger)
        {
            if (string.IsNullOrEmpty(modelPath) || !Directory.Exists(modelPath)) return;
            if (targetEnv == null) return;

            logger?.Invoke("--- Bắt đầu đồng bộ Rebar Shape Manager & Rules ---");

            // RebarShapeRules.xml
            if (!string.IsNullOrEmpty(targetEnv.RebarShapeRulesPath) && File.Exists(targetEnv.RebarShapeRulesPath))
            {
                SyncCatalogFile(modelPath, targetEnv.RebarShapeRulesPath, "RebarShapeRules.xml", logger);
            }
            else
            {
                logger?.Invoke("Cảnh báo: Không tìm thấy RebarShapeRules.xml trong Environment đích, bỏ qua.", false);
            }

            // ShapeCatalog.xml
            if (!string.IsNullOrEmpty(targetEnv.ShapeCatalogPath) && File.Exists(targetEnv.ShapeCatalogPath))
            {
                SyncCatalogFile(modelPath, targetEnv.ShapeCatalogPath, "ShapeCatalog.xml", logger);
            }

            // Shapes folder (if exists in environment)
            if (!string.IsNullOrEmpty(targetEnv.RootPath))
            {
                string envShapesDir = Path.Combine(targetEnv.RootPath, "system", "shapes");
                if (!Directory.Exists(envShapesDir))
                {
                    envShapesDir = Path.Combine(targetEnv.RootPath, "shapes");
                }

                if (Directory.Exists(envShapesDir))
                {
                    string modelShapesDir = Path.Combine(modelPath, "shapes");
                    if (!Directory.Exists(modelShapesDir))
                    {
                        Directory.CreateDirectory(modelShapesDir);
                    }

                    int copiedShapes = 0;
                    foreach (var shapeFile in Directory.GetFiles(envShapesDir, "*.*"))
                    {
                        string dest = Path.Combine(modelShapesDir, Path.GetFileName(shapeFile));
                        if (!File.Exists(dest))
                        {
                            File.Copy(shapeFile, dest, false);
                            copiedShapes++;
                        }
                    }

                    if (copiedShapes > 0)
                    {
                        logger?.Invoke($"Đã sao chép {copiedShapes} file định nghĩa hình dạng thép (shapes).", true);
                    }
                }
            }

            logger?.Invoke("Đồng bộ Rebar Shape Rules hoàn tất.", true);
        }

        /// <summary>
        /// Copies standard drawing and modeling attributes from target environment to model/attributes.
        /// </summary>
        public static void SyncAttributes(string modelPath, TeklaEnvironmentInfo targetEnv, SyncLogHandler logger)
        {
            if (string.IsNullOrEmpty(modelPath) || !Directory.Exists(modelPath)) return;
            if (targetEnv == null || string.IsNullOrEmpty(targetEnv.AttributesFolderPath) || !Directory.Exists(targetEnv.AttributesFolderPath))
            {
                logger?.Invoke("Cảnh báo: Không tìm thấy thư mục Attributes trong Environment đích, bỏ qua.", false);
                return;
            }

            logger?.Invoke("--- Bắt đầu đồng bộ file thuộc tính (Attributes) chuẩn ---");

            string modelAttrDir = Path.Combine(modelPath, "attributes");
            if (!Directory.Exists(modelAttrDir))
            {
                Directory.CreateDirectory(modelAttrDir);
            }

            int addedCount = 0;
            var attrFiles = Directory.GetFiles(targetEnv.AttributesFolderPath, "*.*", SearchOption.AllDirectories);

            foreach (var srcFile in attrFiles)
            {
                string fileName = Path.GetFileName(srcFile);
                string destFile = Path.Combine(modelAttrDir, fileName);

                // If file does not exist, or is standard.*, copy safely
                if (!File.Exists(destFile) || fileName.StartsWith("standard.", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        File.Copy(srcFile, destFile, true);
                        addedCount++;
                    }
                    catch { }
                }
            }

            logger?.Invoke($"Đã cập nhật/bổ sung {addedCount} file thuộc tính chuẩn (.clm, .prt, .dia, filters...).", true);
        }

        /// <summary>
        /// Updates options.ini and model.ini in the model directory to reflect the new target environment.
        /// </summary>
        public static void UpdateEnvironmentOptions(string modelPath, TeklaEnvironmentInfo targetEnv, SyncLogHandler logger)
        {
            if (string.IsNullOrEmpty(modelPath) || !Directory.Exists(modelPath)) return;
            if (targetEnv == null) return;

            logger?.Invoke("--- Bắt đầu cập nhật file cấu hình options.ini & model.ini ---");

            string optionsFile = Path.Combine(modelPath, "options.ini");
            var updatedLines = new List<string>();
            bool envKeySet = false;

            if (File.Exists(optionsFile))
            {
                // Backup original options.ini
                string bakFile = Path.Combine(modelPath, "options.ini.orig_bak");
                if (!File.Exists(bakFile))
                {
                    File.Copy(optionsFile, bakFile, true);
                }

                var existingLines = File.ReadAllLines(optionsFile);
                foreach (var line in existingLines)
                {
                    string trimmed = line.Trim();

                    // Check environment name declaration
                    if (trimmed.StartsWith("set XS_ENVIRONMENT_NAME", StringComparison.OrdinalIgnoreCase) ||
                        trimmed.StartsWith("set XS_KEY_ENVIRONMENT_NAME", StringComparison.OrdinalIgnoreCase))
                    {
                        updatedLines.Add($"set XS_ENVIRONMENT_NAME={targetEnv.Name}");
                        envKeySet = true;
                        continue;
                    }

                    // Fix Korean font settings to standard Arial/Tahoma if present
                    if (trimmed.IndexOf("Gulim", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        trimmed.IndexOf("Dotum", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        trimmed.IndexOf("Batang", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string replaced = trimmed
                            .Replace("Gulim", "Arial")
                            .Replace("Dotum", "Arial")
                            .Replace("Batang", "Times New Roman");
                        updatedLines.Add(replaced);
                        continue;
                    }

                    updatedLines.Add(line);
                }
            }

            if (!envKeySet)
            {
                updatedLines.Add($"\n// Converted by Tekla Environment Converter on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                updatedLines.Add($"set XS_ENVIRONMENT_NAME={targetEnv.Name}");
                updatedLines.Add($"set XS_KEY_ENVIRONMENT_NAME={targetEnv.Name}");
            }

            File.WriteAllLines(optionsFile, updatedLines, Encoding.UTF8);
            logger?.Invoke($"Đã cập nhật options.ini: XS_ENVIRONMENT_NAME={targetEnv.Name}", true);

            // Update model.ini if exists
            string modelIni = Path.Combine(modelPath, "model.ini");
            if (File.Exists(modelIni))
            {
                try
                {
                    string iniContent = File.ReadAllText(modelIni);
                    // Replace references like Korea to TargetEnv
                    iniContent = $"// Updated by Tekla Environment Converter\n" + iniContent;
                    File.WriteAllText(modelIni, iniContent, Encoding.UTF8);
                    logger?.Invoke("Đã cập nhật model.ini.", true);
                }
                catch { }
            }
        }

        private static void SyncCatalogFile(string modelPath, string sourceFilePath, string targetFileName, SyncLogHandler logger)
        {
            if (string.IsNullOrEmpty(sourceFilePath) || !File.Exists(sourceFilePath)) return;

            string destPath = Path.Combine(modelPath, targetFileName);

            try
            {
                // Create backup if target already exists
                if (File.Exists(destPath))
                {
                    string bakPath = Path.Combine(modelPath, $"{targetFileName}.orig_bak");
                    if (!File.Exists(bakPath))
                    {
                        File.Copy(destPath, bakPath, true);
                    }
                }

                File.Copy(sourceFilePath, destPath, true);
                logger?.Invoke($"Đã đồng bộ {targetFileName} từ: {Path.GetFileName(sourceFilePath)}", true);
            }
            catch (Exception ex)
            {
                logger?.Invoke($"Lỗi khi đồng bộ {targetFileName}: {ex.Message}", false);
            }
        }
    }
}
