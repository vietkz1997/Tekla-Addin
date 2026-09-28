using System;
using System.Collections.Generic;
using System.Linq;
using BimCommands.EnvironmentConverter.Models;
using Tekla.Structures.Model;

namespace BimCommands.EnvironmentConverter.Core
{
    /// <summary>
    /// Handles batch scanning, rule matching, and updating of materials and rebar grades across a Tekla Structures model.
    /// </summary>
    public static class MaterialMapper
    {
        public delegate void MappingLogHandler(string message, bool isSuccess = true);

        /// <summary>
        /// Returns default dictionary mapping from Korea standards to Vietnam / TCVN standards.
        /// </summary>
        public static List<MaterialMappingRule> GetDefaultKoreaToVietnamRules()
        {
            var rules = new List<MaterialMappingRule>
            {
                // Rebar grades: Korea (KS D 3504) -> Vietnam (TCVN 1651:2018)
                new MaterialMappingRule(MappingCategory.Rebar, "SD300", "CB300-V", "Thép gân mác SD300 sang CB300-V"),
                new MaterialMappingRule(MappingCategory.Rebar, "SD400", "CB400-V", "Thép gân mác SD400 sang CB400-V (phổ biến nhất)"),
                new MaterialMappingRule(MappingCategory.Rebar, "SD500", "CB500-V", "Thép gân cường độ cao SD500 sang CB500-V"),
                new MaterialMappingRule(MappingCategory.Rebar, "SD600", "CB600-V", "Thép gân cường độ cao SD600 sang CB600-V"),
                new MaterialMappingRule(MappingCategory.Rebar, "HD300", "CB300-V", "Thép HD300 sang CB300-V"),
                new MaterialMappingRule(MappingCategory.Rebar, "HD400", "CB400-V", "Thép HD400 sang CB400-V"),
                new MaterialMappingRule(MappingCategory.Rebar, "HD500", "CB500-V", "Thép HD500 sang CB500-V"),
                new MaterialMappingRule(MappingCategory.Rebar, "SR24", "CB240-T", "Thép tròn trơn SR24 sang CB240-T"),

                // Structural Steel: Korea (KS D 3503 / 3515 / 3861) -> Vietnam / TCVN / Standard
                new MaterialMappingRule(MappingCategory.SteelPart, "SS400", "SS400", "Thép kết cấu SS400 giữ nguyên hoặc map Q235B"),
                new MaterialMappingRule(MappingCategory.SteelPart, "SM490", "Q345B", "Thép hàn SM490 sang Q345B"),
                new MaterialMappingRule(MappingCategory.SteelPart, "SM490A", "Q345B", "Thép SM490A sang Q345B"),
                new MaterialMappingRule(MappingCategory.SteelPart, "SM490B", "Q345B", "Thép SM490B sang Q345B"),
                new MaterialMappingRule(MappingCategory.SteelPart, "SHN400", "SS400", "Thép hình cán nóng SHN400 sang SS400"),
                new MaterialMappingRule(MappingCategory.SteelPart, "SHN490", "Q345B", "Thép hình cán nóng SHN490 sang Q345B"),
                new MaterialMappingRule(MappingCategory.SteelPart, "SN400", "SS400", "Thép xây dựng chịu chấn SN400 sang SS400"),
                new MaterialMappingRule(MappingCategory.SteelPart, "SN490", "Q345B", "Thép xây dựng chịu chấn SN490 sang Q345B"),
                new MaterialMappingRule(MappingCategory.SteelPart, "S275", "SS400", "Thép S275 sang SS400"),
                new MaterialMappingRule(MappingCategory.SteelPart, "S355", "Q345B", "Thép S355 sang Q345B"),

                // Concrete grades: Korea (C21..C40) -> Vietnam (B20..B40)
                new MaterialMappingRule(MappingCategory.ConcretePart, "C21", "B20", "Bê tông C21 sang cấp độ bền B20"),
                new MaterialMappingRule(MappingCategory.ConcretePart, "C24", "B25", "Bê tông C24 sang cấp độ bền B25"),
                new MaterialMappingRule(MappingCategory.ConcretePart, "C27", "B25", "Bê tông C27 sang cấp độ bền B25"),
                new MaterialMappingRule(MappingCategory.ConcretePart, "C30", "B30", "Bê tông C30 sang cấp độ bền B30"),
                new MaterialMappingRule(MappingCategory.ConcretePart, "C35", "B35", "Bê tông C35 sang cấp độ bền B35"),
                new MaterialMappingRule(MappingCategory.ConcretePart, "C40", "B40", "Bê tông C40 sang cấp độ bền B40")
            };

            return rules;
        }

        /// <summary>
        /// Generates a tailored list of mapping rules based on actual materials and rebars found in the inspected model.
        /// </summary>
        public static List<MaterialMappingRule> BuildTailoredRules(ModelInspectionResult inspection)
        {
            var defaultRules = GetDefaultKoreaToVietnamRules();
            var result = new List<MaterialMappingRule>();

            if (inspection == null) return defaultRules;

            // 1. Process Rebar Grades
            foreach (var grade in inspection.UsedRebarGrades)
            {
                var matched = defaultRules.FirstOrDefault(r => r.Category == MappingCategory.Rebar &&
                                                               r.SourceValue.Equals(grade, StringComparison.OrdinalIgnoreCase));
                if (matched != null)
                {
                    result.Add(new MaterialMappingRule(MappingCategory.Rebar, grade, matched.TargetValue, matched.Description));
                }
                else
                {
                    // Suggest target name or keep identical if already Vietnam
                    string suggested = grade.StartsWith("SD", StringComparison.OrdinalIgnoreCase)
                        ? "CB" + grade.Substring(2) + "-V"
                        : grade;
                    result.Add(new MaterialMappingRule(MappingCategory.Rebar, grade, suggested, "Phát hiện trong Model"));
                }
            }

            // 2. Process Part Materials
            foreach (var mat in inspection.UsedMaterials)
            {
                var matched = defaultRules.FirstOrDefault(r => r.Category != MappingCategory.Rebar &&
                                                               r.SourceValue.Equals(mat, StringComparison.OrdinalIgnoreCase));
                if (matched != null)
                {
                    result.Add(new MaterialMappingRule(matched.Category, mat, matched.TargetValue, matched.Description));
                }
                else
                {
                    bool isConcrete = mat.StartsWith("C", StringComparison.OrdinalIgnoreCase) && mat.Length <= 4 && char.IsDigit(mat[1]);
                    var cat = isConcrete ? MappingCategory.ConcretePart : MappingCategory.SteelPart;
                    result.Add(new MaterialMappingRule(cat, mat, mat, "Phát hiện trong Model"));
                }
            }

            // Also append any unused default rules so user can still see them
            foreach (var def in defaultRules)
            {
                if (!result.Any(r => r.Category == def.Category && r.SourceValue.Equals(def.SourceValue, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Add(def);
                }
            }

            return result;
        }

        /// <summary>
        /// Executes batch update of materials and rebar grades on an active Tekla model.
        /// </summary>
        public static bool ExecuteBatchMapping(Model model, List<MaterialMappingRule> rules, MappingLogHandler logger, out int partsModified, out int rebarsModified)
        {
            partsModified = 0;
            rebarsModified = 0;

            if (model == null || !model.GetConnectionStatus())
            {
                logger?.Invoke("Lỗi: Không thể kết nối tới Tekla Structures để thực hiện ánh xạ.", false);
                return false;
            }

            // Separate active lookup dictionaries for fast mapping
            var partMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var rebarMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var r in rules.Where(x => x.IsEnabled && !string.IsNullOrEmpty(x.TargetValue) && !x.SourceValue.Equals(x.TargetValue, StringComparison.OrdinalIgnoreCase)))
            {
                if (r.Category == MappingCategory.Rebar)
                {
                    rebarMap[r.SourceValue] = r.TargetValue;
                }
                else
                {
                    partMap[r.SourceValue] = r.TargetValue;
                }
            }

            if (partMap.Count == 0 && rebarMap.Count == 0)
            {
                logger?.Invoke("Không có quy tắc ánh xạ nào có sự thay đổi giá trị (Source != Target). Bỏ qua bước cập nhật đối tượng.", true);
                return true;
            }

            logger?.Invoke($"Bắt đầu quét và cập nhật đối tượng trong Model: {partMap.Count} quy tắc vật liệu, {rebarMap.Count} quy tắc mác thép...");

            try
            {
                var selector = model.GetModelObjectSelector();
                var targetTypes = new Type[] { typeof(Part), typeof(Reinforcement) };
                var enumerator = selector.GetAllObjectsWithType(targetTypes);

                int totalScanned = 0;

                while (enumerator.MoveNext())
                {
                    totalScanned++;
                    var current = enumerator.Current;
                    if (current == null) continue;

                    // 1. Check Part Material
                    if (current is Part part)
                    {
                        if (part.Material != null && !string.IsNullOrEmpty(part.Material.MaterialString))
                        {
                            if (partMap.TryGetValue(part.Material.MaterialString, out string newMat))
                            {
                                string oldMat = part.Material.MaterialString;
                                part.Material.MaterialString = newMat;
                                if (part.Modify())
                                {
                                    partsModified++;
                                }
                            }
                        }
                    }
                    // 2. Check Reinforcement Grade
                    else if (current is Reinforcement rebar)
                    {
                        if (!string.IsNullOrEmpty(rebar.Grade))
                        {
                            if (rebarMap.TryGetValue(rebar.Grade, out string newGrade))
                            {
                                string oldGrade = rebar.Grade;
                                rebar.Grade = newGrade;
                                if (rebar.Modify())
                                {
                                    rebarsModified++;
                                }
                            }
                        }
                    }
                }

                if (partsModified > 0 || rebarsModified > 0)
                {
                    bool committed = model.CommitChanges();
                    logger?.Invoke($"CommitChanges: {(committed ? "Thành công" : "Đã lưu")}. Đã cập nhật {partsModified} Parts và {rebarsModified} Rebars sang mác chuẩn Vietnam!", true);
                }
                else
                {
                    logger?.Invoke($"Đã quét qua {totalScanned} đối tượng, không phát hiện đối tượng nào cần đổi mác.", true);
                }

                return true;
            }
            catch (Exception ex)
            {
                logger?.Invoke("Lỗi trong quá trình cập nhật đối tượng: " + ex.Message, false);
                return false;
            }
        }
    }
}
