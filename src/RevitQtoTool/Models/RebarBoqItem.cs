using System;

namespace RevitQtoTool.Models
{
    public class RebarBoqItem
    {
        public long ElementId { get; set; }
        public string HostCategory { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public string RebarType { get; set; } = string.Empty;
        public double DiameterMm { get; set; }
        public int Quantity { get; set; }
        public double SingleLengthM { get; set; }
        public double TotalLengthM { get; set; }
        public double TotalWeightKg { get; set; }

        public string DiameterGroup
        {
            get
            {
                if (DiameterMm <= 10.01) return "D <= 10";
                if (DiameterMm <= 18.01) return "10 < D <= 18";
                return "D > 18";
            }
        }
    }
}
