using System;
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace BimCommands.Tekla.ClashCheck
{
    public enum ClashSeverity
    {
        Minor,    // < 10mm
        Medium,   // 10mm - 50mm
        Severe    // > 50mm
    }

    public class ClashResultItem
    {
        public int Index { get; set; }
        
        // Rebar Info
        public long RebarId { get; set; }
        public string RebarGuid { get; set; } = string.Empty;
        public string RebarName { get; set; } = string.Empty;
        public string RebarSize { get; set; } = string.Empty;
        public string RebarGrade { get; set; } = string.Empty;
        public string RebarPos { get; set; } = string.Empty;
        public string HostPartName { get; set; } = string.Empty;
        public double RebarLength { get; set; }
        public Reinforcement RebarObject { get; set; }

        // IFC Info
        public long IfcObjectId { get; set; }
        public string IfcGuid { get; set; } = string.Empty;
        public string IfcFileName { get; set; } = string.Empty;
        public string IfcEntityName { get; set; } = string.Empty;
        public ReferenceModelObject IfcObject { get; set; }

        // Clash Details
        public double OverlapMm { get; set; }
        public Point ClashPoint { get; set; }
        public Point MinPoint { get; set; }
        public Point MaxPoint { get; set; }
        
        public ClashSeverity Severity
        {
            get
            {
                if (OverlapMm > 50.0) return ClashSeverity.Severe;
                if (OverlapMm > 10.0) return ClashSeverity.Medium;
                return ClashSeverity.Minor;
            }
        }

        public string ClashPointDisplay
        {
            get
            {
                if (ClashPoint == null) return "N/A";
                return string.Format("({0:F1}, {1:F1}, {2:F1})", ClashPoint.X, ClashPoint.Y, ClashPoint.Z);
            }
        }
    }
}
