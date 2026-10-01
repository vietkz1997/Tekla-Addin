using System;

namespace RevitQtoTool.Models
{
    /// <summary>
    /// Represents an extracted Bill of Quantities item for concrete elements.
    /// </summary>
    public class BoqItem
    {
        public long ElementId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        
        /// <summary>
        /// Net volume in cubic meters (m3)
        /// </summary>
        public double NetVolumeM3 { get; set; }

        /// <summary>
        /// Formwork surface area in square meters (m2)
        /// </summary>
        public double FormworkAreaM2 { get; set; }

        public string Comments { get; set; } = string.Empty;
        public string Mark { get; set; } = string.Empty;
    }
}
