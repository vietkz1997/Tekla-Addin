using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using RevitQtoTool.Models;

namespace RevitQtoTool.Services
{
    public class RevitRebarCollector
    {
        private const double FeetToMm = 304.8;
        private const double FeetToMeter = 0.3048;

        /// <summary>
        /// TCVN standard unit weight formula: W = D² / 162 (kg/m)
        /// where D is diameter in mm, density = 7850 kg/m³.
        /// Using 162.0 as the industry-standard divisor in Vietnam.
        /// </summary>
        private const double TcvnDivisor = 162.0;

        public List<RebarBoqItem> CollectRebars(Document doc)
        {
            var results = new List<RebarBoqItem>();
            if (doc == null) return results;

            var rebarCollector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rebar)
                .WhereElementIsNotElementType();

            foreach (Element elem in rebarCollector)
            {
                if (elem is Rebar rebar)
                {
                    try
                    {
                        RebarBarType barType = doc.GetElement(rebar.GetTypeId()) as RebarBarType;
                        if (barType == null) continue;

                        double diameterFt = barType.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER)?.AsDouble() ?? barType.BarModelDiameter;
                        double diameterMm = Math.Round(diameterFt * FeetToMm, 1);
                        int qty = rebar.Quantity;
                        if (qty <= 0) qty = 1;

                        double totalLengthM = rebar.TotalLength * FeetToMeter;
                        double singleLengthM = qty > 0 ? (totalLengthM / qty) : 0;

                        double unitWeightKgPerM = (diameterMm * diameterMm) / TcvnDivisor;
                        double totalWeightKg = Math.Round(totalLengthM * unitWeightKgPerM, 2);

                        Element hostElem = null;
                        ElementId hostId = rebar.GetHostId();
                        if (hostId != null && hostId != ElementId.InvalidElementId)
                        {
                            hostElem = doc.GetElement(hostId);
                        }
                        string hostCategory = hostElem?.Category?.Name ?? "General";
                        string levelName = RevitElementHelper.GetElementLevelName(doc, hostElem ?? elem);

                        results.Add(new RebarBoqItem
                        {
                            ElementId = rebar.Id.Value,
                            HostCategory = hostCategory,
                            LevelName = levelName,
                            RebarType = barType.Name,
                            DiameterMm = diameterMm,
                            Quantity = qty,
                            SingleLengthM = Math.Round(singleLengthM, 3),
                            TotalLengthM = Math.Round(totalLengthM, 2),
                            TotalWeightKg = totalWeightKg
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Rebar QTO Error Id={elem.Id}]: {ex.Message}");
                    }
                }
            }

            return results;
        }
    }
}
