using System;
using System.Collections.Generic;
using GeometryHelper.Geometry;
using Tekla.Structures.Model;
using TeklaPoint = global::Tekla.Structures.Geometry3d.Point;

namespace BimCommands.Tekla.OverlapChecker
{
    /// <summary>
    /// Represents an overlap/duplicate pair with comprehensive structural metadata for side-by-side inspection.
    /// Enhanced with GeometryHelper 3D clash metrics (exact volume, overlap percentage, 3D solids).
    /// </summary>
    public class OverlapItem
    {
        public bool IsSelected { get; set; } = true;

        // Model Object References
        public ModelObject OriginalObject { get; set; }
        public ModelObject DuplicateObject { get; set; }

        // Primary Identifiers
        public int OriginalId { get; set; }
        public int DuplicateId { get; set; }
        public string OriginalGuid { get; set; } = "";
        public string DuplicateGuid { get; set; } = "";

        // Common Classification
        public string TypeName { get; set; } = "";
        public string OverlapType { get; set; } = "100% Duplicate"; // "100% Duplicate", "Volume Overlap"
        public string RetentionReason { get; set; } = "Default order";

        // Original (Kept) Attributes
        public string OriginalName { get; set; } = "";
        public string OriginalProfile { get; set; } = "";
        public string OriginalMaterial { get; set; } = "";
        public string OriginalClass { get; set; } = "";
        public int OriginalPhase { get; set; }
        public string OriginalMark { get; set; } = ""; // Assembly/Part Mark

        // Duplicate (To-Delete) Attributes
        public string DuplicateName { get; set; } = "";
        public string DuplicateProfile { get; set; } = "";
        public string DuplicateMaterial { get; set; } = "";
        public string DuplicateClass { get; set; } = "";
        public int DuplicatePhase { get; set; }
        public string DuplicateMark { get; set; } = "";

        // Geometry Metrics
        public TeklaPoint CenterPoint { get; set; }
        public string CenterStr { get; set; } = "";
        public TeklaPoint MinPoint { get; set; }
        public TeklaPoint MaxPoint { get; set; }
        public double OriginalLength { get; set; }
        public double DuplicateLength { get; set; }
        public double OriginalVolume { get; set; }
        public double DuplicateVolume { get; set; }

        // GeometryHelper 3D Clash Metrics
        public double ClashVolume { get; set; } // in mm3
        public double OverlapRatio { get; set; } // 0.0 to 1.0 (e.g. 0.98 = 98%)
        public string ClashVolumeFormatted => ClashVolume > 1e6 ? $"{ClashVolume / 1e6:0.00} dm³" : (ClashVolume > 1e3 ? $"{ClashVolume / 1e3:0.0} cm³" : $"{ClashVolume:0} mm³");
        public string OverlapPercentageFormatted => $"{OverlapRatio * 100.0:0.0}%";
        public List<GeoSolid3> ClashSolids { get; set; } = new List<GeoSolid3>();

        // Compatibility Properties
        public string Name => DuplicateName;
        public string Profile => DuplicateProfile;

        /// <summary>
        /// Swaps which object is kept and which object is marked as duplicate.
        /// </summary>
        public void Swap()
        {
            // Swap Objects & IDs
            var tempObj = OriginalObject;
            OriginalObject = DuplicateObject;
            DuplicateObject = tempObj;

            int tempId = OriginalId;
            OriginalId = DuplicateId;
            DuplicateId = tempId;

            string tempGuid = OriginalGuid;
            OriginalGuid = DuplicateGuid;
            DuplicateGuid = tempGuid;

            // Swap metadata
            string tempName = OriginalName;
            OriginalName = DuplicateName;
            DuplicateName = tempName;

            string tempProf = OriginalProfile;
            OriginalProfile = DuplicateProfile;
            DuplicateProfile = tempProf;

            string tempMat = OriginalMaterial;
            OriginalMaterial = DuplicateMaterial;
            DuplicateMaterial = tempMat;

            string tempCls = OriginalClass;
            OriginalClass = DuplicateClass;
            DuplicateClass = tempCls;

            int tempPhase = OriginalPhase;
            OriginalPhase = DuplicatePhase;
            DuplicatePhase = tempPhase;

            string tempMark = OriginalMark;
            OriginalMark = DuplicateMark;
            DuplicateMark = tempMark;

            double tempLen = OriginalLength;
            OriginalLength = DuplicateLength;
            DuplicateLength = tempLen;

            double tempVol = OriginalVolume;
            OriginalVolume = DuplicateVolume;
            DuplicateVolume = tempVol;

            RetentionReason = "Manually swapped by user";
        }
    }
}
