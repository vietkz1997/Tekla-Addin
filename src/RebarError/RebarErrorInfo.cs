using System;
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace BimCommands.RebarErrorChecker
{
    public enum RebarErrorCategory
    {
        SpliceFailed,       // Tekla RebarSplice.Insert() returns false
        AngleDeviation,     // Misaligned orientation vector between bars
        EccentricityOffset, // Perpendicular offset too large between bar axes
        BarCountMismatch,   // Unequal number of bars between groups
        EndGapTooLarge,     // Distance between bar ends exceeds lap reach
        CorruptedRebar,     // Zero-length, empty geometry, invalid shape
        OrphanSplice        // Existing RebarSplice with missing parent rebars
    }

    public class RebarErrorInfo
    {
        public int Index { get; set; }
        public bool IsSelected { get; set; } = true;
        public RebarErrorCategory Category { get; set; }
        public string CategoryDisplayName { get; set; } = "";
        public int Id1 { get; set; }
        public int Id2 { get; set; }
        public ModelObject ModelObject1 { get; set; }
        public ModelObject ModelObject2 { get; set; }
        public string BarSize1 { get; set; } = "";
        public string BarSize2 { get; set; } = "";
        public string ErrorDetails { get; set; } = "";
        public Point CenterPoint { get; set; } = new Point();
        public Point MinPoint { get; set; } = new Point();
        public Point MaxPoint { get; set; } = new Point();
        public double DeviationValue { get; set; }

        public string LocationString
        {
            get
            {
                if (CenterPoint == null) return "N/A";
                return string.Format("X:{0:F0} Y:{1:F0} Z:{2:F0}", CenterPoint.X, CenterPoint.Y, CenterPoint.Z);
            }
        }

        public string ObjectIdsString
        {
            get
            {
                if (Id2 > 0)
                    return string.Format("{0} - {1}", Id1, Id2);
                return Id1.ToString();
            }
        }
    }
}
