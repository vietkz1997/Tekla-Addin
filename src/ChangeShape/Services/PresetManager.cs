using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ChangeShape.Models;

namespace ChangeShape.Services
{
    public class PresetManager
    {
        private static readonly string PresetFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "presets.json");
        private readonly List<PresetModel> _presets = new();

        public IReadOnlyList<PresetModel> Presets => _presets;

        public PresetManager()
        {
            LoadPresets();
        }

        public void LoadPresets()
        {
            _presets.Clear();

            // Default built-in presets
            _presets.Add(new PresetModel
            {
                Name = "Dầm 300×500 (Đai 240×440)",
                Category = "Dầm",
                ShapeId = "56",
                Values = new List<double> { 240, 440, 120 },
                Description = "Dầm rộng 300, cao 500, lớp BV 30mm, móc 120mm"
            });

            _presets.Add(new PresetModel
            {
                Name = "Dầm 300×600 (Đai 240×540)",
                Category = "Dầm",
                ShapeId = "56",
                Values = new List<double> { 240, 540, 120 },
                Description = "Dầm rộng 300, cao 600, lớp BV 30mm, móc 120mm"
            });

            _presets.Add(new PresetModel
            {
                Name = "Dầm 200×350 (Đai 140×290)",
                Category = "Dầm",
                ShapeId = "56",
                Values = new List<double> { 140, 290, 100 },
                Description = "Dầm phụ 200×350, lớp BV 30mm, móc 100mm"
            });

            _presets.Add(new PresetModel
            {
                Name = "Dầm 400×700 (Đai 340×640)",
                Category = "Dầm",
                ShapeId = "56",
                Values = new List<double> { 340, 640, 150 },
                Description = "Dầm chính 400×700, lớp BV 30mm, móc 150mm"
            });

            _presets.Add(new PresetModel
            {
                Name = "Cột 400×400 (Đai 340×340)",
                Category = "Cột",
                ShapeId = "56",
                Values = new List<double> { 340, 340, 120 },
                Description = "Cột 400×400, lớp BV 30mm, móc 120mm"
            });

            _presets.Add(new PresetModel
            {
                Name = "Cột 500×500 (Đai 440×440)",
                Category = "Cột",
                ShapeId = "56",
                Values = new List<double> { 440, 440, 140 },
                Description = "Cột 500×500, lớp BV 30mm, móc 140mm"
            });

            _presets.Add(new PresetModel
            {
                Name = "Cột 600×600 (Đai 540×540)",
                Category = "Cột",
                ShapeId = "56",
                Values = new List<double> { 540, 540, 150 },
                Description = "Cột 600×600, lớp BV 30mm, móc 150mm"
            });

            _presets.Add(new PresetModel
            {
                Name = "Chữ U 400×500 (Shape 51)",
                Category = "Chữ U",
                ShapeId = "51",
                Values = new List<double> { 400, 500 },
                Description = "Thép chữ U đáy 400, cánh 500"
            });

            _presets.Add(new PresetModel
            {
                Name = "C-Clip Giằng 200 (Shape 370)",
                Category = "Giằng",
                ShapeId = "370",
                Values = new List<double> { 600, 200, 100 },
                Description = "Giằng chống uốn kẹp sườn dầm"
            });

            // Load user presets from file if exists
            try
            {
                if (File.Exists(PresetFilePath))
                {
                    string json = File.ReadAllText(PresetFilePath);
                    var userPresets = JsonSerializer.Deserialize<List<PresetModel>>(json);
                    if (userPresets != null)
                    {
                        foreach (var up in userPresets)
                        {
                            if (!_presets.Exists(p => p.Name.Equals(up.Name, StringComparison.OrdinalIgnoreCase)))
                            {
                                _presets.Add(up);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignore file read error and keep built-ins
            }
        }

        public void SaveCustomPreset(string name, string shapeId, List<double> values, string category = "Tùy chỉnh")
        {
            var existing = _presets.Find(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.ShapeId = shapeId;
                existing.Values = new List<double>(values);
                existing.Category = category;
            }
            else
            {
                _presets.Add(new PresetModel
                {
                    Name = name,
                    ShapeId = shapeId,
                    Category = category,
                    Values = new List<double>(values),
                    Description = $"Tạo bởi người dùng lúc {DateTime.Now:dd/MM/yyyy HH:mm}"
                });
            }

            try
            {
                string json = JsonSerializer.Serialize(_presets, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(PresetFilePath, json);
            }
            catch
            {
                // Ignore file save error
            }
        }

        /// <summary>
        /// Calculate stirrup dimensions from concrete section and cover.
        /// </summary>
        public static (double A, double B, double C) CalculateFromConcreteSection(double concreteWidth, double concreteHeight, double cover, int rebarDia = 10)
        {
            double a = Math.Max(50, concreteWidth - 2.0 * cover);
            double b = Math.Max(50, concreteHeight - 2.0 * cover);
            double c = Math.Max(100, rebarDia * 10.0); // Minimum 100mm or 10*diameter
            return (a, b, c);
        }
    }
}
