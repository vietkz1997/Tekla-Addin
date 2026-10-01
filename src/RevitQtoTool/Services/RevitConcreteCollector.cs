using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using RevitQtoTool.Models;

namespace RevitQtoTool.Services
{
    public class RevitConcreteCollector
    {
        private const double CubicFeetToCubicMeters = 0.028316846592;

        public List<BoqItem> CollectConcreteElements(Document doc)
        {
            var results = new List<BoqItem>();
            if (doc == null) return results;

            try
            {
                var categories = new List<BuiltInCategory>
                {
                    BuiltInCategory.OST_StructuralColumns,
                    BuiltInCategory.OST_StructuralFraming,
                    BuiltInCategory.OST_Floors,
                    BuiltInCategory.OST_Walls,
                    BuiltInCategory.OST_StructuralFoundation
                };

                var categoryFilter = new ElementMulticategoryFilter(categories);
                var collector = new FilteredElementCollector(doc)
                    .WherePasses(categoryFilter)
                    .WhereElementIsNotElementType();

                foreach (Element elem in collector)
                {
                    if (elem == null || !elem.IsValidObject) continue;

                    if (!IsConcreteElement(doc, elem, out string materialName, out double netVolumeM3))
                        continue;

                    string levelName = RevitElementHelper.GetElementLevelName(doc, elem);
                    ElementType elemType = doc.GetElement(elem.GetTypeId()) as ElementType;

                    var item = new BoqItem
                    {
                        ElementId = elem.Id.Value,
                        CategoryName = elem.Category?.Name ?? "Unknown",
                        FamilyName = elemType?.FamilyName ?? string.Empty,
                        TypeName = elemType?.Name ?? elem.Name,
                        LevelName = levelName,
                        MaterialName = materialName,
                        NetVolumeM3 = Math.Round(netVolumeM3, 3),
                        Comments = elem.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString() ?? string.Empty,
                        Mark = elem.get_Parameter(BuiltInParameter.ALL_MODEL_MARK)?.AsString() ?? string.Empty
                    };

                    results.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error in CollectConcreteElements]: {ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Determines if the element is a concrete element by checking its materials.
        /// Returns the material name with the largest concrete volume (fixes multi-material issue).
        /// </summary>
        private bool IsConcreteElement(Document doc, Element elem, out string materialName, out double netVolumeM3)
        {
            materialName = "Concrete";
            netVolumeM3 = 0.0;

            ICollection<ElementId> materialIds = elem.GetMaterialIds(false);
            if (materialIds != null && materialIds.Count > 0)
            {
                // Track which concrete material has the highest volume
                string bestConcreteName = null;
                double bestConcreteVolFt3 = 0.0;

                foreach (ElementId matId in materialIds)
                {
                    Material mat = doc.GetElement(matId) as Material;
                    if (mat == null) continue;

                    bool isConcrete = (mat.MaterialClass != null && mat.MaterialClass.IndexOf("Concrete", StringComparison.OrdinalIgnoreCase) >= 0)
                                      || mat.Name.IndexOf("Concrete", StringComparison.OrdinalIgnoreCase) >= 0
                                      || mat.Name.IndexOf("Bê tông", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (isConcrete)
                    {
                        double volFt3 = elem.GetMaterialVolume(matId);
                        netVolumeM3 += volFt3 * CubicFeetToCubicMeters;

                        // Keep the name of the concrete material with the largest volume
                        if (volFt3 > bestConcreteVolFt3)
                        {
                            bestConcreteVolFt3 = volFt3;
                            bestConcreteName = mat.Name;
                        }
                    }
                }

                if (netVolumeM3 > 0.0001 && bestConcreteName != null)
                {
                    materialName = bestConcreteName;
                    return true;
                }
            }

            // Fallback: Use HOST_VOLUME_COMPUTED for structural elements without explicit material
            Parameter volParam = elem.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED);
            if (volParam != null && volParam.HasValue && volParam.AsDouble() > 0.0001)
            {
                Parameter structParam = elem.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT)
                                      ?? elem.get_Parameter(BuiltInParameter.FLOOR_PARAM_IS_STRUCTURAL);

                if (structParam != null && structParam.HasValue && structParam.AsInteger() == 0)
                    return false;

                netVolumeM3 = volParam.AsDouble() * CubicFeetToCubicMeters;
                return true;
            }

            return false;
        }
    }
}
