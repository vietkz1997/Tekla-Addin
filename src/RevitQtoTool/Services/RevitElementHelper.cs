using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.DB;

namespace RevitQtoTool.Services
{
    /// <summary>
    /// Shared utility methods for Revit element geometry extraction and level resolution.
    /// Eliminates duplication across Collector/Detector services.
    /// </summary>
    public static class RevitElementHelper
    {
        private static readonly BuiltInParameter[] LevelParamCandidates = new[]
        {
            BuiltInParameter.FAMILY_BASE_LEVEL_PARAM,
            BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM,
            BuiltInParameter.WALL_BASE_CONSTRAINT,
            BuiltInParameter.LEVEL_PARAM,
            BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM,
            BuiltInParameter.SCHEDULE_LEVEL_PARAM
        };

        /// <summary>
        /// Resolves the level name of a Revit element using multiple fallback strategies.
        /// Checks element.LevelId first, then scans common BuiltInParameters.
        /// </summary>
        public static string GetElementLevelName(Document doc, Element elem)
        {
            if (elem == null || doc == null) return "Unassigned";

            // Strategy 1: Direct LevelId
            if (elem.LevelId != null && elem.LevelId != ElementId.InvalidElementId)
            {
                Level lvl = doc.GetElement(elem.LevelId) as Level;
                if (lvl != null) return lvl.Name;
            }

            // Strategy 2: Scan known Level parameters
            foreach (var bip in LevelParamCandidates)
            {
                Parameter p = elem.get_Parameter(bip);
                if (p != null && p.HasValue)
                {
                    ElementId id = p.AsElementId();
                    if (id != null && id != ElementId.InvalidElementId)
                    {
                        Level lvl = doc.GetElement(id) as Level;
                        if (lvl != null) return lvl.Name;
                    }
                }
            }

            return "Unassigned";
        }

        /// <summary>
        /// Extracts the largest Solid from an element's geometry.
        /// Handles GeometryInstance recursion for family-based elements.
        /// </summary>
        public static Solid GetBestSolid(Element element)
        {
            if (element == null || !element.IsValidObject) return null;

            var opt = new Options
            {
                ComputeReferences = false,
                DetailLevel = ViewDetailLevel.Fine,
                IncludeNonVisibleObjects = false
            };

            GeometryElement geomElem = element.get_Geometry(opt);
            if (geomElem == null) return null;

            return ExtractLargestSolid(geomElem);
        }

        private static Solid ExtractLargestSolid(GeometryElement geomElem)
        {
            Solid bestSolid = null;
            double maxVolume = 0.0;

            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid solid && solid.Volume > 0.0001 && solid.Faces.Size > 0)
                {
                    if (solid.Volume > maxVolume)
                    {
                        maxVolume = solid.Volume;
                        bestSolid = solid;
                    }
                }
                else if (geomObj is GeometryInstance geomInst)
                {
                    GeometryElement instanceGeom = geomInst.GetInstanceGeometry();
                    if (instanceGeom != null)
                    {
                        Solid candidate = ExtractLargestSolid(instanceGeom);
                        if (candidate != null && candidate.Volume > maxVolume)
                        {
                            maxVolume = candidate.Volume;
                            bestSolid = candidate;
                        }
                    }
                }
            }

            return bestSolid;
        }

        /// <summary>
        /// Fast AABB (Axis-Aligned Bounding Box) intersection test.
        /// Used as pre-filter before expensive Boolean operations.
        /// Returns true if the two bounding boxes overlap in 3D space.
        /// </summary>
        public static bool BoundingBoxesIntersect(BoundingBoxXYZ a, BoundingBoxXYZ b)
        {
            if (a == null || b == null) return false;

            return a.Min.X <= b.Max.X && a.Max.X >= b.Min.X
                && a.Min.Y <= b.Max.Y && a.Max.Y >= b.Min.Y
                && a.Min.Z <= b.Max.Z && a.Max.Z >= b.Min.Z;
        }
    }
}
