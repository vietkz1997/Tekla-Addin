using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BimCommands.EnvironmentConverter.Models;
using Microsoft.Win32;

namespace BimCommands.EnvironmentConverter.Core
{
    /// <summary>
    /// Scans the operating system and filesystem to discover installed Tekla Structures versions and environments.
    /// </summary>
    public static class TeklaEnvironmentScanner
    {
        private static readonly string[] CommonDriveLetters = new string[] { @"C:\", @"D:\", @"E:\" };

        /// <summary>
        /// Discovers all available Tekla environments from installed versions and file system.
        /// </summary>
        public static List<TeklaEnvironmentInfo> ScanAllEnvironments()
        {
            var results = new List<TeklaEnvironmentInfo>();
            var visitedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Scan from Windows Registry
            ScanRegistryEnvironments(results, visitedPaths);

            // 2. Scan standard Tekla folder trees (e.g., C:\TeklaStructures\<version>\Environments)
            ScanStandardFolderTrees(results, visitedPaths);

            // Sort alphabetically by version and name
            return results.OrderByDescending(e => e.Version).ThenBy(e => e.Name).ToList();
        }

        /// <summary>
        /// Reads registry keys to find installed Tekla versions and their environment paths.
        /// </summary>
        private static void ScanRegistryEnvironments(List<TeklaEnvironmentInfo> results, HashSet<string> visitedPaths)
        {
            try
            {
                string[] regBases = new string[]
                {
                    @"SOFTWARE\Trimble\Tekla Structures",
                    @"SOFTWARE\WOW6432Node\Trimble\Tekla Structures",
                    @"SOFTWARE\Tekla\Structures"
                };

                foreach (var baseKey in regBases)
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(baseKey))
                    {
                        if (key == null) continue;

                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            using (var versionKey = key.OpenSubKey(subKeyName))
                            {
                                if (versionKey == null) continue;

                                string envPath = versionKey.GetValue("EnvironmentsPath") as string;
                                if (string.IsNullOrEmpty(envPath))
                                {
                                    string xdatadir = versionKey.GetValue("XDATADIR") as string;
                                    if (!string.IsNullOrEmpty(xdatadir))
                                    {
                                        envPath = Path.Combine(xdatadir, "Environments");
                                    }
                                }

                                if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
                                {
                                    ScanEnvironmentDirectory(envPath, subKeyName, results, visitedPaths);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignore registry access permission issues
            }
        }

        /// <summary>
        /// Scans disk paths for TeklaStructures installation trees.
        /// </summary>
        private static void ScanStandardFolderTrees(List<TeklaEnvironmentInfo> results, HashSet<string> visitedPaths)
        {
            foreach (var drive in CommonDriveLetters)
            {
                string root = Path.Combine(drive, "TeklaStructures");
                if (Directory.Exists(root))
                {
                    try
                    {
                        foreach (var verDir in Directory.GetDirectories(root))
                        {
                            string verName = Path.GetFileName(verDir);
                            string envDir = Path.Combine(verDir, "Environments");
                            if (Directory.Exists(envDir))
                            {
                                ScanEnvironmentDirectory(envDir, verName, results, visitedPaths);
                            }
                        }
                    }
                    catch { }
                }

                // ProgramData paths
                string programData = Path.Combine(drive, "ProgramData", "Trimble", "Tekla Structures");
                if (Directory.Exists(programData))
                {
                    try
                    {
                        foreach (var verDir in Directory.GetDirectories(programData))
                        {
                            string verName = Path.GetFileName(verDir);
                            string envDir = Path.Combine(verDir, "Environments");
                            if (Directory.Exists(envDir))
                            {
                                ScanEnvironmentDirectory(envDir, verName, results, visitedPaths);
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Discovers all sub-environments inside a specific Environments directory.
        /// </summary>
        public static void ScanEnvironmentDirectory(string envDirPath, string version, List<TeklaEnvironmentInfo> results, HashSet<string> visitedPaths)
        {
            if (!Directory.Exists(envDirPath)) return;

            try
            {
                var subDirs = Directory.GetDirectories(envDirPath);
                foreach (var dir in subDirs)
                {
                    string envName = Path.GetFileName(dir);
                    if (visitedPaths.Contains(dir)) continue;

                    // Exclude "common" or "blank_project" if desired, but we can keep common as reference
                    var envInfo = ParseEnvironmentInfo(dir, envName, version);
                    if (envInfo != null)
                    {
                        visitedPaths.Add(dir);
                        results.Add(envInfo);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Inspects a given directory to construct TeklaEnvironmentInfo.
        /// </summary>
        public static TeklaEnvironmentInfo ParseEnvironmentInfo(string dirPath, string envName, string version = "Custom")
        {
            if (!Directory.Exists(dirPath)) return null;

            var env = new TeklaEnvironmentInfo
            {
                Name = envName,
                Version = version,
                RootPath = dirPath,
                IsCustom = (version == "Custom")
            };

            // Locate env_<name>.ini or any *.ini
            string specificIni = Path.Combine(dirPath, $"env_{envName}.ini");
            if (File.Exists(specificIni))
            {
                env.IniFilePath = specificIni;
            }
            else
            {
                var iniFiles = Directory.GetFiles(dirPath, "env_*.ini");
                if (iniFiles.Length > 0)
                {
                    env.IniFilePath = iniFiles[0];
                }
            }

            // Find role inis
            try
            {
                env.RoleIniFiles.AddRange(Directory.GetFiles(dirPath, "role_*.ini"));
            }
            catch { }

            // Find system folders
            try
            {
                var sysDir = Path.Combine(dirPath, "system");
                if (Directory.Exists(sysDir))
                {
                    env.SystemFolders.Add(sysDir);
                }
            }
            catch { }

            // Locate Catalogs
            FindCatalogsInEnvironment(env);

            return env;
        }

        /// <summary>
        /// Searches for catalog files inside the environment folder structure.
        /// </summary>
        private static void FindCatalogsInEnvironment(TeklaEnvironmentInfo env)
        {
            if (string.IsNullOrEmpty(env.RootPath) || !Directory.Exists(env.RootPath)) return;

            try
            {
                // Profile catalog: profdb.bin or profdb.lis
                env.ProfileCatalogPath = FindFirstFile(env.RootPath, "profdb.bin", "profdb.lis");

                // Material catalog: matdb.bin or matdb.lis
                env.MaterialCatalogPath = FindFirstFile(env.RootPath, "matdb.bin", "matdb.lis");

                // Bolt catalog: screwdb.db
                env.BoltCatalogPath = FindFirstFile(env.RootPath, "screwdb.db");

                // Bolt assembly catalog: assdb.db
                env.BoltAssemblyCatalogPath = FindFirstFile(env.RootPath, "assdb.db");

                // Rebar database: rebar_database.inp
                env.RebarDatabasePath = FindFirstFile(env.RootPath, "rebar_database.inp");

                // Rebar shape rules: RebarShapeRules.xml
                env.RebarShapeRulesPath = FindFirstFile(env.RootPath, "RebarShapeRules.xml");

                // Shape catalog: ShapeCatalog.xml
                env.ShapeCatalogPath = FindFirstFile(env.RootPath, "ShapeCatalog.xml");

                // Attributes folder
                string[] candidateAttrDirs = new string[]
                {
                    Path.Combine(env.RootPath, "General", "attributes"),
                    Path.Combine(env.RootPath, "Steel", "attributes"),
                    Path.Combine(env.RootPath, "system", "attributes"),
                    Path.Combine(env.RootPath, "model_settings"),
                    Path.Combine(env.RootPath, "attributes")
                };

                foreach (var attrDir in candidateAttrDirs)
                {
                    if (Directory.Exists(attrDir))
                    {
                        env.AttributesFolderPath = attrDir;
                        break;
                    }
                }
            }
            catch { }
        }

        private static string FindFirstFile(string rootDir, params string[] fileNames)
        {
            try
            {
                foreach (var fn in fileNames)
                {
                    // Check direct or immediate subfolders first for speed
                    string direct = Path.Combine(rootDir, fn);
                    if (File.Exists(direct)) return direct;

                    string profDir = Path.Combine(rootDir, "profil", fn);
                    if (File.Exists(profDir)) return profDir;

                    string sysDir = Path.Combine(rootDir, "system", fn);
                    if (File.Exists(sysDir)) return sysDir;

                    string inpDir = Path.Combine(rootDir, "inp", fn);
                    if (File.Exists(inpDir)) return inpDir;

                    // Deep recursive search if not found
                    var matches = Directory.GetFiles(rootDir, fn, SearchOption.AllDirectories);
                    if (matches.Length > 0) return matches[0];
                }
            }
            catch { }

            return null;
        }
    }
}
