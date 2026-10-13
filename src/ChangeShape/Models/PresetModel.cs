using System;
using System.Collections.Generic;

namespace ChangeShape.Models
{
    public class PresetModel
    {
        public string Name { get; set; } = string.Empty;
        public string ShapeId { get; set; } = "56";
        public string Category { get; set; } = "Dầm"; // Dầm, Cột, Móng, Tùy chỉnh
        public List<double> Values { get; set; } = new List<double>();
        public string Description { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Name} (Shape {ShapeId})";
        }
    }
}
