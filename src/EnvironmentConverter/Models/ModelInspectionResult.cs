using System;
using System.Collections.Generic;

namespace BimCommands.EnvironmentConverter.Models
{
    /// <summary>
    /// Holds the inspection details of a Tekla Structures model.
    /// </summary>
    public class ModelInspectionResult
    {
        public string ModelPath { get; set; }
        public string ModelName { get; set; }
        public string DetectedEnvironment { get; set; } = "Unknown";
        public string DetectedRole { get; set; } = "Unknown";
        public bool IsOpenInActiveTekla { get; set; }

        // Existing catalog files inside the model directory
        public bool HasLocalProfileDb { get; set; }
        public bool HasLocalMaterialDb { get; set; }
        public bool HasLocalBoltDb { get; set; }
        public bool HasLocalBoltAssDb { get; set; }
        public bool HasLocalRebarDb { get; set; }
        public bool HasLocalRebarRules { get; set; }
        public bool HasLocalShapeCatalog { get; set; }

        // Model statistics (from Open API if connected)
        public int PartCount { get; set; }
        public int RebarCount { get; set; }
        public int BoltCount { get; set; }

        public HashSet<string> UsedProfiles { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> UsedMaterials { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> UsedRebarGrades { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Detected options from options.ini
        public Dictionary<string, string> DetectedOptions { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
