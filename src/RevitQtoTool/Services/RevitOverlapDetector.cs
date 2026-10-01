using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using RevitQtoTool.Models;

namespace RevitQtoTool.Services
{
    public class RevitOverlapDetector
    {
        private const double CubicFeetToCubicMeters = 0.028316846592;
        private const double MinSolidVolumeFt3 = 0.0001;
        private const double MinOverlapVolumeFt3 = 0.00035;
        private const double DuplicateThreshold = 0.95;

        /// <summary>
        /// Detects overlapping/duplicate structural elements in the document.
        /// Optimized using AABB pre-filter and Solid caching to avoid O(N*FilteredElementCollector).
        /// </summary>
        public List<OverlapIssueItem> DetectOverlaps(Document doc, IEnumerable<BuiltInCategory> targetCategories = null)
        {
            var issues = new List<OverlapIssueItem>();
            if (doc == null) return issues;

            var catList = targetCategories?.ToList() ?? new List<BuiltInCategory>
            {
                BuiltInCategory.OST_StructuralColumns,
                BuiltInCategory.OST_StructuralFraming,
                BuiltInCategory.OST_Floors,
                BuiltInCategory.OST_Walls
            };

            // Step 1: Collect all elements once
            var filter = new ElementMulticategoryFilter(catList);
            var elements = new FilteredElementCollector(doc)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElements();

            if (elements.Count < 2) return issues;

            // Step 2: Pre-cache BoundingBox and Solid for each element (avoids redundant geometry calls)
            var bboxCache = new Dictionary<long, BoundingBoxXYZ>();
            var solidCache = new Dictionary<long, Solid>();

            foreach (var elem in elements)
            {
                long id = elem.Id.Value;

                BoundingBoxXYZ bbox = elem.get_BoundingBox(null);
                if (bbox == null) continue;
                bboxCache[id] = bbox;

                Solid solid = RevitElementHelper.GetBestSolid(elem);
                if (solid != null && solid.Volume > MinSolidVolumeFt3)
                {
                    solidCache[id] = solid;
                }
            }

            // Step 3: Pairwise comparison with AABB pre-filter (only upper triangle i < j)
            var elemArray = elements.Where(e => solidCache.ContainsKey(e.Id.Value)).ToArray();

            for (int i = 0; i < elemArray.Length; i++)
            {
                Element elemA = elemArray[i];
                long idA = elemA.Id.Value;
                BoundingBoxXYZ bboxA = bboxCache[idA];
                Solid solidA = solidCache[idA];

                for (int j = i + 1; j < elemArray.Length; j++)
                {
                    Element elemB = elemArray[j];
                    long idB = elemB.Id.Value;

                    // Fast rejection: AABB test before expensive Boolean operations
                    if (!bboxCache.TryGetValue(idB, out BoundingBoxXYZ bboxB))
                        continue;

                    if (!RevitElementHelper.BoundingBoxesIntersect(bboxA, bboxB))
                        continue;

                    Solid solidB = solidCache[idB];

                    try
                    {
                        Solid intersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                            solidA, solidB, BooleanOperationsType.Intersect);

                        if (intersection != null && intersection.Volume > MinOverlapVolumeFt3)
                        {
                            double overlapM3 = intersection.Volume * CubicFeetToCubicMeters;
                            bool areJoined = JoinGeometryUtils.AreElementsJoined(doc, elemA, elemB);
                            bool isDuplicate = (intersection.Volume / Math.Min(solidA.Volume, solidB.Volume)) > DuplicateThreshold;

                            issues.Add(new OverlapIssueItem
                            {
                                ElementIdA = idA,
                                CategoryA = elemA.Category?.Name ?? "Unknown",
                                ElementIdB = idB,
                                CategoryB = elemB.Category?.Name ?? "Unknown",
                                LevelName = RevitElementHelper.GetElementLevelName(doc, elemA),
                                ClashingVolumeM3 = Math.Round(overlapM3, 4),
                                IssueType = isDuplicate
                                    ? "Trùng lặp 100% (Duplicate)"
                                    : (areJoined ? "Giao cắt (Đã Join)" : "Chưa Join (Nguy cơ tính thừa)"),
                                IsJoined = areJoined
                            });
                        }
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
                    {
                        // Non-manifold geometry cannot perform Boolean — skip gracefully
                        Debug.WriteLine($"[Overlap Skip] Elements {idA}-{idB}: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[Overlap Error] Elements {idA}-{idB}: {ex.Message}");
                    }
                }
            }

            return issues;
        }

        /// <summary>
        /// Automatically joins two overlapping elements using JoinGeometryUtils.
        /// Wraps the operation in a Transaction for safe execution.
        /// </summary>
        public bool AutoJoinElements(Document doc, long idA, long idB)
        {
            try
            {
                using (var t = new Transaction(doc, "Auto-Join Overlapping Elements"))
                {
                    t.Start();
                    Element elemA = doc.GetElement(new ElementId(idA));
                    Element elemB = doc.GetElement(new ElementId(idB));

                    if (elemA != null && elemB != null && !JoinGeometryUtils.AreElementsJoined(doc, elemA, elemB))
                    {
                        JoinGeometryUtils.JoinGeometry(doc, elemA, elemB);
                        t.Commit();
                        return true;
                    }
                    t.RollBack();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Join Error]: {ex.Message}");
            }
            return false;
        }
    }
}
