using System;
using System.Collections;
using System.Collections.Generic;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Color = System.Drawing.Color;
using TeklaUiSelector = Tekla.Structures.Model.UI.ModelObjectSelector;
using TeklaViewHandler = Tekla.Structures.Model.UI.ViewHandler;
using TeklaPicker = Tekla.Structures.Model.UI.Picker;

namespace BimCommands.ConvertRebar
{
    public class RebarConverter
    {
        private readonly Model _model;

        public RebarConverter(Model model)
        {
            _model = model;
        }

        /// <summary>
        /// Collects all Reinforcement objects from the current Tekla selection.
        /// If concrete parts (Beams, Columns, Slabs) are selected, extracts their reinforcement children.
        /// </summary>
        public List<Reinforcement> GetSelectedReinforcements()
        {
            var result = new List<Reinforcement>();
            var seenIds = new HashSet<int>();

            try
            {
                var selector = new TeklaUiSelector();
                var enumerator = selector.GetSelectedObjects();

                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (current == null) continue;

                    if (current is Reinforcement rebar)
                    {
                        if (seenIds.Add(rebar.Identifier.ID))
                        {
                            result.Add(rebar);
                        }
                    }
                    else if (current is Part part)
                    {
                        var children = part.GetChildren();
                        while (children.MoveNext())
                        {
                            if (children.Current is Reinforcement childRebar)
                            {
                                if (seenIds.Add(childRebar.Identifier.ID))
                                {
                                    result.Add(childRebar);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetSelectedReinforcements error: " + ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Prompts the user to pick one or more reinforcement objects directly in the Tekla 3D model.
        /// </summary>
        public List<Reinforcement> PickReinforcementsFromModel(string prompt = "Pick rebar(s) to convert, then click middle mouse button")
        {
            var result = new List<Reinforcement>();
            var seenIds = new HashSet<int>();

            try
            {
                var picker = new TeklaPicker();
                var pickedObjects = picker.PickObjects(TeklaPicker.PickObjectsEnum.PICK_N_REINFORCEMENTS, prompt);

                while (pickedObjects.MoveNext())
                {
                    var current = pickedObjects.Current;
                    if (current is Reinforcement rebar)
                    {
                        if (seenIds.Add(rebar.Identifier.ID))
                        {
                            result.Add(rebar);
                        }
                    }
                    else if (current is Part part)
                    {
                        var children = part.GetChildren();
                        while (children.MoveNext())
                        {
                            if (children.Current is Reinforcement childRebar)
                            {
                                if (seenIds.Add(childRebar.Identifier.ID))
                                {
                                    result.Add(childRebar);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PickReinforcementsFromModel: " + ex.Message);
            }

            return result;
        }

        public static string GetRebarSize(Reinforcement rebar)
        {
            if (rebar is SingleRebar sr) return sr.Size ?? "";
            if (rebar is RebarGroup rg) return rg.Size ?? "";
            string s = "";
            if (rebar.GetReportProperty("SIZE", ref s)) return s;
            return "";
        }

        /// <summary>
        /// Analyzes a reinforcement object to inspect its point count and shape without modifying it.
        /// </summary>
        public RebarConversionRecord AnalyzeRebar(Reinforcement rebar)
        {
            if (rebar == null) return null;

            var record = new RebarConversionRecord
            {
                Id = rebar.Identifier.ID,
                Name = rebar.Name ?? "",
                Size = GetRebarSize(rebar),
                Grade = rebar.Grade ?? "",
                ModelObject = rebar
            };

            if (rebar is SingleRebar sr)
            {
                record.RebarType = "SingleRebar";
                record.OriginalPointCount = sr.Polygon?.Points != null ? sr.Polygon.Points.Count : 0;
            }
            else if (rebar is RebarGroup rg)
            {
                record.RebarType = "RebarGroup";
                if (rg.Polygons != null && rg.Polygons.Count > 0 && rg.Polygons[0] is Polygon p)
                {
                    record.OriginalPointCount = p.Points != null ? p.Points.Count : 0;
                }
            }
            else
            {
                record.RebarType = rebar.GetType().Name;
                record.OriginalPointCount = 0;
            }

            if (record.OriginalPointCount == 6)
            {
                record.Status = "Sẵn sàng (6 điểm ➔ 4 điểm)";
                record.StatusColor = Color.FromArgb(37, 99, 235); // Blue
                record.NewPointCount = 4;
            }
            else if (record.OriginalPointCount == 4)
            {
                record.Status = "Đã là thép 4 điểm (Không đổi)";
                record.StatusColor = Color.FromArgb(100, 116, 139); // Slate
                record.NewPointCount = 4;
            }
            else
            {
                record.Status = $"Bỏ qua: Có {record.OriginalPointCount} điểm (yêu cầu 6 điểm)";
                record.StatusColor = Color.FromArgb(217, 119, 6); // Amber
                record.NewPointCount = record.OriginalPointCount;
            }

            return record;
        }

        /// <summary>
        /// Converts a 6-point rebar into a 4-point rebar by trimming the two end legs (P0 and P5),
        /// keeping internal points [P1, P2, P3, P4].
        /// </summary>
        public bool ConvertRebar(RebarConversionRecord record, bool onlySixPoints = true)
        {
            if (record == null || record.ModelObject == null) return false;

            if (onlySixPoints && record.OriginalPointCount != 6)
            {
                record.Status = $"Bỏ qua: Thép có {record.OriginalPointCount} điểm (chỉ xử lý 6 điểm)";
                record.StatusColor = Color.FromArgb(217, 119, 6);
                return false;
            }

            try
            {
                if (record.ModelObject is SingleRebar sr)
                {
                    return ConvertSingleRebar(sr, record);
                }
                else if (record.ModelObject is RebarGroup rg)
                {
                    return ConvertRebarGroup(rg, record);
                }
                else
                {
                    record.Status = $"Không hỗ trợ loại rebar: {record.RebarType}";
                    record.StatusColor = Color.Red;
                    return false;
                }
            }
            catch (Exception ex)
            {
                record.Status = "Lỗi chuyển đổi: " + ex.Message;
                record.StatusColor = Color.Red;
                return false;
            }
        }

        private bool ConvertSingleRebar(SingleRebar sr, RebarConversionRecord record)
        {
            var poly = sr.Polygon;
            if (poly?.Points == null || poly.Points.Count < 5)
            {
                record.Status = "Không đủ điểm polygon để cắt 2 đầu";
                record.StatusColor = Color.Red;
                return false;
            }

            // Backup original polygon points for Undo
            var backupPoints = new ArrayList();
            foreach (Point pt in poly.Points)
            {
                backupPoints.Add(new Point(pt.X, pt.Y, pt.Z));
            }
            record.BackupPolygons.Clear();
            record.BackupPolygons.Add(backupPoints);

            // Backup radii
            if (sr.RadiusValues != null)
            {
                record.BackupRadiusValues = new ArrayList(sr.RadiusValues);
            }

            // Trim two ends: Keep points [1, 2, 3, ..., Count - 2]
            // For 6 points: keeps index 1, 2, 3, 4 (exactly 4 points)
            var newPoints = new ArrayList();
            for (int i = 1; i < poly.Points.Count - 1; i++)
            {
                Point originalPt = (Point)poly.Points[i];
                newPoints.Add(new Point(originalPt.X, originalPt.Y, originalPt.Z));
            }

            sr.Polygon = new Polygon { Points = newPoints };

            // Adjust RadiusValues for the new interior corners
            if (sr.RadiusValues != null)
            {
                if (sr.RadiusValues.Count >= 4)
                {
                    var newRadii = new ArrayList();
                    newRadii.Add(sr.RadiusValues[1]);
                    newRadii.Add(sr.RadiusValues[2]);
                    sr.RadiusValues = newRadii;
                }
                else if (sr.RadiusValues.Count == 1)
                {
                    sr.RadiusValues = new ArrayList { sr.RadiusValues[0] };
                }
            }

            bool modified = sr.Modify();
            if (modified)
            {
                record.NewPointCount = newPoints.Count;
                record.Status = $"Thành công: 6 điểm ➔ {newPoints.Count} điểm (Cắt 2 chân)";
                record.StatusColor = Color.FromArgb(5, 150, 105); // Emerald Green
                return true;
            }
            else
            {
                record.Status = "Tekla từ chối Modify() SingleRebar";
                record.StatusColor = Color.Red;
                return false;
            }
        }

        private bool ConvertRebarGroup(RebarGroup rg, RebarConversionRecord record)
        {
            if (rg.Polygons == null || rg.Polygons.Count == 0)
            {
                record.Status = "RebarGroup không có Polygons";
                record.StatusColor = Color.Red;
                return false;
            }

            // Backup original polygons
            record.BackupPolygons.Clear();
            foreach (Polygon poly in rg.Polygons)
            {
                var bp = new ArrayList();
                if (poly.Points != null)
                {
                    foreach (Point pt in poly.Points)
                    {
                        bp.Add(new Point(pt.X, pt.Y, pt.Z));
                    }
                }
                record.BackupPolygons.Add(bp);
            }

            // Backup radii
            if (rg.RadiusValues != null)
            {
                record.BackupRadiusValues = new ArrayList(rg.RadiusValues);
            }

            var newPolygons = new ArrayList();
            int modifiedCount = 0;
            int newPtsCount = 4;

            foreach (Polygon poly in rg.Polygons)
            {
                if (poly.Points != null && poly.Points.Count >= 5)
                {
                    var newPoints = new ArrayList();
                    for (int i = 1; i < poly.Points.Count - 1; i++)
                    {
                        Point originalPt = (Point)poly.Points[i];
                        newPoints.Add(new Point(originalPt.X, originalPt.Y, originalPt.Z));
                    }
                    newPolygons.Add(new Polygon { Points = newPoints });
                    newPtsCount = newPoints.Count;
                    modifiedCount++;
                }
                else
                {
                    newPolygons.Add(poly);
                }
            }

            if (modifiedCount == 0)
            {
                record.Status = "Không có polygon nào đủ điều kiện để cắt 2 chân";
                record.StatusColor = Color.DarkOrange;
                return false;
            }

            rg.Polygons = newPolygons;

            // Adjust RadiusValues for new interior corners
            if (rg.RadiusValues != null)
            {
                if (rg.RadiusValues.Count >= 4)
                {
                    var newRadii = new ArrayList();
                    newRadii.Add(rg.RadiusValues[1]);
                    newRadii.Add(rg.RadiusValues[2]);
                    rg.RadiusValues = newRadii;
                }
                else if (rg.RadiusValues.Count == 1)
                {
                    rg.RadiusValues = new ArrayList { rg.RadiusValues[0] };
                }
            }

            bool modified = rg.Modify();
            if (modified)
            {
                record.NewPointCount = newPtsCount;
                record.Status = $"Thành công: 6 điểm ➔ {newPtsCount} điểm ({modifiedCount} polygons)";
                record.StatusColor = Color.FromArgb(5, 150, 105); // Emerald Green
                return true;
            }
            else
            {
                record.Status = "Tekla từ chối Modify() RebarGroup";
                record.StatusColor = Color.Red;
                return false;
            }
        }

        /// <summary>
        /// Undoes the conversion and restores the original geometry.
        /// </summary>
        public bool UndoRecord(RebarConversionRecord record)
        {
            if (record == null || record.ModelObject == null || record.BackupPolygons.Count == 0)
            {
                return false;
            }

            try
            {
                if (record.ModelObject is SingleRebar sr)
                {
                    sr.Polygon = new Polygon { Points = record.BackupPolygons[0] };
                    if (record.BackupRadiusValues != null)
                    {
                        sr.RadiusValues = record.BackupRadiusValues;
                    }
                    bool ok = sr.Modify();
                    if (ok)
                    {
                        record.Status = "Đã hoàn tác (Khôi phục hình dạng gốc)";
                        record.StatusColor = Color.FromArgb(37, 99, 235);
                        record.NewPointCount = record.OriginalPointCount;
                    }
                    return ok;
                }
                else if (record.ModelObject is RebarGroup rg)
                {
                    var restoredPolygons = new ArrayList();
                    foreach (var bp in record.BackupPolygons)
                    {
                        restoredPolygons.Add(new Polygon { Points = bp });
                    }
                    rg.Polygons = restoredPolygons;
                    if (record.BackupRadiusValues != null)
                    {
                        rg.RadiusValues = record.BackupRadiusValues;
                    }
                    bool ok = rg.Modify();
                    if (ok)
                    {
                        record.Status = "Đã hoàn tác (Khôi phục hình dạng gốc)";
                        record.StatusColor = Color.FromArgb(37, 99, 235);
                        record.NewPointCount = record.OriginalPointCount;
                    }
                    return ok;
                }
            }
            catch (Exception ex)
            {
                record.Status = "Lỗi khi hoàn tác: " + ex.Message;
                record.StatusColor = Color.Red;
            }

            return false;
        }

        /// <summary>
        /// Zooms to the specified rebar in Tekla 3D model and selects it.
        /// </summary>
        public void ZoomToRebar(Reinforcement rebar)
        {
            if (rebar == null) return;

            try
            {
                var selectList = new ArrayList { rebar };
                var selector = new TeklaUiSelector();
                selector.Select(selectList);

                // Calculate bounding box from polygon points
                Point minPt = new Point(double.MaxValue, double.MaxValue, double.MaxValue);
                Point maxPt = new Point(double.MinValue, double.MinValue, double.MinValue);

                void Expand(Point pt)
                {
                    minPt.X = Math.Min(minPt.X, pt.X);
                    minPt.Y = Math.Min(minPt.Y, pt.Y);
                    minPt.Z = Math.Min(minPt.Z, pt.Z);
                    maxPt.X = Math.Max(maxPt.X, pt.X);
                    maxPt.Y = Math.Max(maxPt.Y, pt.Y);
                    maxPt.Z = Math.Max(maxPt.Z, pt.Z);
                }

                if (rebar is SingleRebar sr && sr.Polygon?.Points != null)
                {
                    foreach (Point pt in sr.Polygon.Points) Expand(pt);
                }
                else if (rebar is RebarGroup rg && rg.Polygons != null)
                {
                    foreach (Polygon poly in rg.Polygons)
                    {
                        if (poly.Points != null)
                        {
                            foreach (Point pt in poly.Points) Expand(pt);
                        }
                    }
                }

                if (minPt.X < double.MaxValue && maxPt.X > double.MinValue)
                {
                    double margin = 250.0;
                    var boxMin = new Point(minPt.X - margin, minPt.Y - margin, minPt.Z - margin);
                    var boxMax = new Point(maxPt.X + margin, maxPt.Y + margin, maxPt.Z + margin);

                    TeklaViewHandler.ZoomToBoundingBox(new AABB(boxMin, boxMax));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ZoomToRebar error: " + ex.Message);
            }
        }

        /// <summary>
        /// Redraws all currently visible Tekla 3D views.
        /// </summary>
        public static void RedrawVisibleViews()
        {
            try
            {
                var views = TeklaViewHandler.GetVisibleViews();
                while (views.MoveNext())
                {
                    TeklaViewHandler.RedrawView(views.Current);
                }
            }
            catch { }
        }
    }
}
