using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.DB;

namespace RevitQtoTool.Services
{
    public class RevitFormworkCalculator
    {
        private const double SqFeetToSqMeters = 0.09290304;

        public double CalculateNetFormworkArea(Document doc, Element element, IEnumerable<Element> intersectingElements)
        {
            if (element == null || !element.IsValidObject)
                return 0.0;

            Solid mainSolid = RevitElementHelper.GetBestSolid(element);
            if (mainSolid == null || mainSolid.Volume <= 0.0001)
                return 0.0;

            BuiltInCategory category = (BuiltInCategory)(element.Category?.Id?.Value ?? -1);

            // Clone the solid to avoid mutating the original geometry during Boolean Difference.
            // If Clone fails, re-extract from geometry to ensure a fresh copy.
            Solid netSolid;
            try
            {
                netSolid = SolidUtils.Clone(mainSolid);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FormworkCalc] Clone failed for Element {element.Id}: {ex.Message}. Re-extracting geometry.");
                // Re-extract geometry instead of reusing potentially mutable mainSolid reference
                netSolid = RevitElementHelper.GetBestSolid(element);
                if (netSolid == null) return 0.0;
            }

            if (intersectingElements != null)
            {
                foreach (Element neighbor in intersectingElements)
                {
                    if (neighbor == null || neighbor.Id == element.Id)
                        continue;

                    Solid neighborSolid = RevitElementHelper.GetBestSolid(neighbor);
                    if (neighborSolid == null || neighborSolid.Volume <= 0.0001)
                        continue;

                    try
                    {
                        Solid diffResult = BooleanOperationsUtils.ExecuteBooleanOperation(
                            netSolid, 
                            neighborSolid, 
                            BooleanOperationsType.Difference
                        );

                        if (diffResult != null && diffResult.Volume > 0.0001)
                        {
                            netSolid = diffResult;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Non-manifold intersections or degenerate geometry — skip this neighbor
                        Debug.WriteLine($"[FormworkCalc] Boolean Difference failed for {element.Id} - {neighbor.Id}: {ex.Message}");
                    }
                }
            }

            double totalFormworkSqFt = 0.0;
            foreach (Face face in netSolid.Faces)
            {
                if (face == null || face.Area <= 0.0001)
                    continue;

                if (IsFormworkSurface(face, category))
                {
                    totalFormworkSqFt += face.Area;
                }
            }

            return Math.Round(totalFormworkSqFt * SqFeetToSqMeters, 2);
        }

        private bool IsFormworkSurface(Face face, BuiltInCategory category)
        {
            BoundingBoxUV bboxUV = face.GetBoundingBox();
            UV midUV = (bboxUV.Min + bboxUV.Max) * 0.5;
            XYZ normal = face.ComputeNormal(midUV).Normalize();

            // Top face (Z > 0.707 ≈ 45°) — not formwork
            if (normal.Z > 0.707)
                return false;

            // Bottom face (Z < -0.707 ≈ 45°) — depends on category
            if (normal.Z < -0.707)
            {
                if (category == BuiltInCategory.OST_StructuralColumns ||
                    category == BuiltInCategory.OST_Walls ||
                    category == BuiltInCategory.OST_StructuralFoundation)
                {
                    return false;
                }
                return true; // Slabs/beams need bottom formwork
            }

            return true; // Side faces are always formwork
        }

        public IList<Element> GetNearbyIntersectingElements(Document doc, Element targetElem)
        {
            BoundingBoxXYZ bbox = targetElem.get_BoundingBox(null);
            if (bbox == null) return new List<Element>();

            // Expand BoundingBox by ~5mm tolerance for edge-touching elements
            Outline outline = new Outline(
                new XYZ(bbox.Min.X - 0.016, bbox.Min.Y - 0.016, bbox.Min.Z - 0.016),
                new XYZ(bbox.Max.X + 0.016, bbox.Max.Y + 0.016, bbox.Max.Z + 0.016)
            );

            var targetCategories = new List<BuiltInCategory>
            {
                BuiltInCategory.OST_StructuralColumns,
                BuiltInCategory.OST_StructuralFraming,
                BuiltInCategory.OST_Floors,
                BuiltInCategory.OST_Walls
            };

            var catFilter = new ElementMulticategoryFilter(targetCategories);
            var bboxFilter = new BoundingBoxIntersectsFilter(outline);

            return new FilteredElementCollector(doc)
                .WherePasses(catFilter)
                .WherePasses(bboxFilter)
                .WhereElementIsNotElementType()
                .Excluding(new List<ElementId> { targetElem.Id })
                .ToElements();
        }

        // GetBestSolid is now delegated to RevitElementHelper.GetBestSolid()
        // Kept as wrapper for backward compatibility
        public Solid GetBestSolid(Element element) => RevitElementHelper.GetBestSolid(element);
    }
}
