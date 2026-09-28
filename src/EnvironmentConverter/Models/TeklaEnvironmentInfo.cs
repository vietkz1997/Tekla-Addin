using System;
using System.Collections.Generic;
using System.IO;

namespace BimCommands.EnvironmentConverter.Models
{
    /// <summary>
    /// Represents an installed or configured Tekla Structures environment.
    /// </summary>
    public class TeklaEnvironmentInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string RootPath { get; set; }
        public string IniFilePath { get; set; }
        public bool IsCustom { get; set; }

        public List<string> SystemFolders { get; set; } = new List<string>();
        public List<string> RoleIniFiles { get; set; } = new List<string>();

        // Catalog file paths in this environment
        public string ProfileCatalogPath { get; set; }
        public string MaterialCatalogPath { get; set; }
        public string BoltCatalogPath { get; set; }
        public string BoltAssemblyCatalogPath { get; set; }
        public string RebarDatabasePath { get; set; }
        public string RebarShapeRulesPath { get; set; }
        public string ShapeCatalogPath { get; set; }
        public string AttributesFolderPath { get; set; }

        public bool HasProfiles => !string.IsNullOrEmpty(ProfileCatalogPath) && File.Exists(ProfileCatalogPath);
        public bool HasMaterials => !string.IsNullOrEmpty(MaterialCatalogPath) && File.Exists(MaterialCatalogPath);
        public bool HasRebarRules => !string.IsNullOrEmpty(RebarShapeRulesPath) && File.Exists(RebarShapeRulesPath);
        public bool HasAttributes => !string.IsNullOrEmpty(AttributesFolderPath) && Directory.Exists(AttributesFolderPath);

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Version))
                return Name;
            return $"{Name} (Tekla {Version})";
        }
    }
}
