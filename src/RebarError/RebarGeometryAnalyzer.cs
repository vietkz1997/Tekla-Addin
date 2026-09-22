using System;
using System.Collections;
using System.Collections.Generic;
using Tekla.Structures;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;

namespace BimCommands.RebarErrorChecker
{
    public class RebarGeometryData
    {
        public Reinforcement RebarObject { get; set; }
        public int Id { get; set; }
        public string Size { get; set; } = "";
        public string Grade { get; set; } = "";
        public double Diameter { get; set; } = 10.0;
        public int BarCount { get; set; }
        public List<PolyLine> Centerlines { get; set; } = new List<PolyLine>();
        public Point CenterPoint { get; set; } = new Point();
        public Point MinPoint { get; set; } = new Point();
        public Point MaxPoint { get; set; } = new Point();
        public Vector MainDirection { get; set; } = new Vector(1, 0, 0);
        public List<Point> StartPoints { get; set; } = new List<Point>();
        public List<Point> EndPoints { get; set; } = new List<Point>();
        
        // Distribution range vector (StartPoint -> EndPoint of RebarGroup)
        public Vector RangeVector { get; set; }
        public Point RangeStart { get; set; }
        public Point RangeEnd { get; set; }
        public bool HasRange { get; set; }

        public bool IsCorrupted { get; set; }
        public string CorruptReason { get; set; } = "";
    }

    public class AnalysisSettings
    {
        public double SearchRadius { get; set; } = 1000.0;    // mm: Bounding search radius
        public double AngleTolerance { get; set; } = 3.0;     // degrees: Max allowable angle deviation
        public double OffsetTolerance { get; set; } = 40.0;   // mm: Max allowable perpendicular offset
        public double MaxEndGap { get; set; } = 800.0;        // mm: Max distance between closest bar ends
        public bool CheckSpliceSimulation { get; set; } = true; // Attempt RebarSplice.Insert()
        public bool CheckBarCountMismatch { get; set; } = true;
        public bool CheckRangeDirection { get; set; } = true;
        public bool CheckCorruptedBars { get; set; } = true;
    }

    public class SpatialGrid
    {
        private readonly double _cellSize;
        private readonly Dictionary<long, List<RebarGeometryData>> _cells = new Dictionary<long, List<RebarGeometryData>>();

        public SpatialGrid(double cellSize)
        {
            _cellSize = cellSize > 100.0 ? cellSize : 1500.0;
        }

        private static long HashCoordinate(int x, int y, int z)
        {
            long hx = (long)(x & 0x1FFFFF);
            long hy = (long)(y & 0x1FFFFF);
            long hz = (long)(z & 0x1FFFFF);
            return (hx << 42) | (hy << 21) | hz;
        }

        public void Insert(RebarGeometryData data)
        {
            int minX = (int)Math.Floor(data.MinPoint.X / _cellSize);
            int maxX = (int)Math.Floor(data.MaxPoint.X / _cellSize);
            int minY = (int)Math.Floor(data.MinPoint.Y / _cellSize);
            int maxY = (int)Math.Floor(data.MaxPoint.Y / _cellSize);
            int minZ = (int)Math.Floor(data.MinPoint.Z / _cellSize);
            int maxZ = (int)Math.Floor(data.MaxPoint.Z / _cellSize);

            if (maxX - minX > 8) maxX = minX + 8;
            if (maxY - minY > 8) maxY = minY + 8;
            if (maxZ - minZ > 8) maxZ = minZ + 8;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        long key = HashCoordinate(x, y, z);
                        List<RebarGeometryData> list;
                        if (!_cells.TryGetValue(key, out list))
                        {
                            list = new List<RebarGeometryData>(4);
                            _cells[key] = list;
                        }
                        list.Add(data);
                    }
                }
            }
        }

        public HashSet<RebarGeometryData> QueryNearby(RebarGeometryData data, double radius)
        {
            var candidates = new HashSet<RebarGeometryData>();
            int minX = (int)Math.Floor((data.MinPoint.X - radius) / _cellSize);
            int maxX = (int)Math.Floor((data.MaxPoint.X + radius) / _cellSize);
            int minY = (int)Math.Floor((data.MinPoint.Y - radius) / _cellSize);
            int maxY = (int)Math.Floor((data.MaxPoint.Y + radius) / _cellSize);
            int minZ = (int)Math.Floor((data.MinPoint.Z - radius) / _cellSize);
            int maxZ = (int)Math.Floor((data.MaxPoint.Z + radius) / _cellSize);

            if (maxX - minX > 8) maxX = minX + 8;
            if (maxY - minY > 8) maxY = minY + 8;
            if (maxZ - minZ > 8) maxZ = minZ + 8;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        long key = HashCoordinate(x, y, z);
                        List<RebarGeometryData> list;
                        if (_cells.TryGetValue(key, out list))
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                var other = list[i];
                                if (other.Id > data.Id && !other.IsCorrupted)
                                {
                                    candidates.Add(other);
                                }
                            }
                        }
                    }
                }
            }
            return candidates;
        }
    }

    public static class RebarGeometryAnalyzer
    {
        public static string GetRebarSize(Reinforcement rebar)
        {
            if (rebar is RebarGroup rg) return rg.Size ?? "";
            if (rebar is SingleRebar sr) return sr.Size ?? "";

            string sizeStr = "";
            if (rebar.GetReportProperty("SIZE", ref sizeStr))
            {
                return sizeStr;
            }
            return "";
        }

        public static RebarGeometryData ExtractRebarData(Reinforcement rebar)
        {
            if (rebar == null) return null;

            var data = new RebarGeometryData
            {
                RebarObject = rebar,
                Id = rebar.Identifier.ID,
                Size = GetRebarSize(rebar),
                Grade = rebar.Grade ?? ""
            };

            double dia = 10.0;
            if (!string.IsNullOrEmpty(data.Size))
            {
                string cleanSize = data.Size.Replace("D", "").Replace("d", "").Replace("T", "").Replace("t", "").Trim();
                double.TryParse(cleanSize, out dia);
            }
            data.Diameter = dia > 0 ? dia : 10.0;

            // Extract Range information if it's a RebarGroup
            if (rebar is RebarGroup rebarGroup)
            {
                if (rebarGroup.StartPoint != null && rebarGroup.EndPoint != null)
                {
                    data.RangeStart = rebarGroup.StartPoint;
                    data.RangeEnd = rebarGroup.EndPoint;
                    Vector rv = new Vector(rebarGroup.EndPoint.X - rebarGroup.StartPoint.X,
                                           rebarGroup.EndPoint.Y - rebarGroup.StartPoint.Y,
                                           rebarGroup.EndPoint.Z - rebarGroup.StartPoint.Z);
                    if (rv.GetLength() > 1.0)
                    {
                        data.RangeVector = rv;
                        data.HasRange = true;
                    }
                }
            }

            try
            {
                ArrayList geoms = rebar.GetRebarGeometries(false);
                if (geoms == null || geoms.Count == 0)
                {
                    geoms = rebar.GetRebarGeometries(true);
                }

                if (geoms == null || geoms.Count == 0)
                {
                    data.IsCorrupted = true;
                    data.CorruptReason = "Rebar does not contain any geometry instances (empty group).";
                    return data;
                }

                data.BarCount = geoms.Count;
                double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
                double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;
                double sumX = 0, sumY = 0, sumZ = 0;
                int totalPoints = 0;

                Vector accumulatedDir = new Vector(0, 0, 0);

                foreach (object obj in geoms)
                {
                    RebarGeometry rg = obj as RebarGeometry;
                    if (rg == null || rg.Shape == null || rg.Shape.Points == null || rg.Shape.Points.Count < 2)
                        continue;

                    PolyLine poly = rg.Shape;
                    data.Centerlines.Add(poly);

                    Point pStart = poly.Points[0] as Point;
                    Point pEnd = poly.Points[poly.Points.Count - 1] as Point;

                    if (pStart != null) data.StartPoints.Add(pStart);
                    if (pEnd != null) data.EndPoints.Add(pEnd);

                    // Find primary orientation segment
                    double maxSegLen = 0;
                    Vector segDir = new Vector(1, 0, 0);

                    for (int i = 0; i < poly.Points.Count - 1; i++)
                    {
                        Point ptA = poly.Points[i] as Point;
                        Point ptB = poly.Points[i + 1] as Point;
                        if (ptA == null || ptB == null) continue;

                        double len = Distance.PointToPoint(ptA, ptB);
                        if (len > maxSegLen)
                        {
                            maxSegLen = len;
                            Vector v = new Vector(ptB.X - ptA.X, ptB.Y - ptA.Y, ptB.Z - ptA.Z);
                            if (v.Normalize() > 0.0001)
                            {
                                segDir = v;
                            }
                        }
                    }

                    if (maxSegLen > 0)
                    {
                        accumulatedDir.X += segDir.X;
                        accumulatedDir.Y += segDir.Y;
                        accumulatedDir.Z += segDir.Z;
                    }

                    // Bounding coordinates
                    foreach (object ptObj in poly.Points)
                    {
                        Point pt = ptObj as Point;
                        if (pt == null) continue;

                        minX = Math.Min(minX, pt.X);
                        minY = Math.Min(minY, pt.Y);
                        minZ = Math.Min(minZ, pt.Z);
                        maxX = Math.Max(maxX, pt.X);
                        maxY = Math.Max(maxY, pt.Y);
                        maxZ = Math.Max(maxZ, pt.Z);

                        sumX += pt.X;
                        sumY += pt.Y;
                        sumZ += pt.Z;
                        totalPoints++;
                    }
                }

                if (totalPoints > 0)
                {
                    data.MinPoint = new Point(minX, minY, minZ);
                    data.MaxPoint = new Point(maxX, maxY, maxZ);
                    data.CenterPoint = new Point(sumX / totalPoints, sumY / totalPoints, sumZ / totalPoints);
                }
                else
                {
                    data.IsCorrupted = true;
                    data.CorruptReason = "Polyline points are invalid or null.";
                    return data;
                }

                if (accumulatedDir.Normalize() > 0.0001)
                {
                    data.MainDirection = accumulatedDir;
                }
            }
            catch (Exception ex)
            {
                data.IsCorrupted = true;
                data.CorruptReason = "Exception reading geometry: " + ex.Message;
            }

            return data;
        }

        public static List<RebarErrorInfo> AnalyzeRebars(List<Reinforcement> rebars, AnalysisSettings settings, Action<int, int> progressCallback = null)
        {
            var results = new List<RebarErrorInfo>();
            if (rebars == null || rebars.Count == 0)
                return results;

            // Step 1: Rapid Extraction & Spatial Grid indexing
            double cellSize = Math.Max(1200.0, settings.SearchRadius * 1.5);
            var grid = new SpatialGrid(cellSize);
            var dataList = new List<RebarGeometryData>(rebars.Count);

            int totalRebars = rebars.Count;
            for (int i = 0; i < totalRebars; i++)
            {
                var data = ExtractRebarData(rebars[i]);
                if (data != null)
                {
                    dataList.Add(data);

                    if (!data.IsCorrupted)
                    {
                        grid.Insert(data);
                    }
                }

                if (totalRebars <= 100 || (i & 63) == 0 || i == totalRebars - 1)
                {
                    progressCallback?.Invoke(i + 1, totalRebars * 2);
                }
            }

            // Step 2: High-speed Spatial Pair Matching
            int processedCount = 0;
            var evaluatedPairs = new HashSet<string>();

            for (int i = 0; i < dataList.Count; i++)
            {
                var d1 = dataList[i];
                if (d1.IsCorrupted) continue;

                // For small selection sets (e.g. <= 150 rebars), query directly to ensure zero false-negatives
                IEnumerable<RebarGeometryData> candidatePool;
                double effectiveSearchRadius = Math.Max(settings.SearchRadius, 2000.0);

                if (dataList.Count <= 150)
                {
                    candidatePool = dataList;
                }
                else
                {
                    candidatePool = grid.QueryNearby(d1, effectiveSearchRadius);
                }

                foreach (var d2 in candidatePool)
                {
                    if (d2.Id <= d1.Id || d2.IsCorrupted) continue;

                    string pairKey = d1.Id + "_" + d2.Id;
                    if (evaluatedPairs.Contains(pairKey)) continue;

                    // Fast AABB distance check with generous margin for overlapping lap spans
                    if (!AreBoundingBoxesNear(d1.MinPoint, d1.MaxPoint, d2.MinPoint, d2.MaxPoint, effectiveSearchRadius))
                        continue;

                    // Check parallel orientation (bars must be roughly aligned along the line of placement)
                    double angleDev = CalculateAngleDegrees(d1.MainDirection, d2.MainDirection);
                    if (angleDev > 30.0)
                    {
                        // Bars are perpendicular or skew, not meant to be spliced
                        continue;
                    }

                    // Check distance between ends
                    Point endPt1, endPt2;
                    double minEndGap = FindMinimumEndDistance(d1, d2, out endPt1, out endPt2);

                    // If bounding boxes overlap significantly, bars are already adjacent or overlapping in 3D
                    bool boxesOverlap = DoBoundingBoxesOverlap(d1.MinPoint, d1.MaxPoint, d2.MinPoint, d2.MaxPoint, 150.0);
                    if (!boxesOverlap && minEndGap > effectiveSearchRadius)
                    {
                        // Too far apart to be a splice candidate
                        continue;
                    }

                    // Calculate axis offset between bars to filter out rebars in completely different structural elements
                    double offset = CalculatePerpendicularOffset(endPt1, d1.MainDirection, endPt2);
                    if (offset > 400.0)
                    {
                        // Rebars are in different beams or far apart laterally
                        continue;
                    }

                    evaluatedPairs.Add(pairKey);

                    Point midPoint = new Point(
                        (endPt1.X + endPt2.X) * 0.5,
                        (endPt1.Y + endPt2.Y) * 0.5,
                        (endPt1.Z + endPt2.Z) * 0.5
                    );

                    Point pairMin = new Point(
                        Math.Min(d1.MinPoint.X, d2.MinPoint.X) - 200,
                        Math.Min(d1.MinPoint.Y, d2.MinPoint.Y) - 200,
                        Math.Min(d1.MinPoint.Z, d2.MinPoint.Z) - 200
                    );
                    Point pairMax = new Point(
                        Math.Max(d1.MaxPoint.X, d2.MaxPoint.X) + 200,
                        Math.Max(d1.MaxPoint.Y, d2.MaxPoint.Y) + 200,
                        Math.Max(d1.MaxPoint.Z, d2.MaxPoint.Z) + 200
                    );

                    // --- DIAGNOSIS CHECKS: EXCLUSIVELY FOR 'Lỗi tạo Splice (Tekla Warning)' ---

                    // Check 1: Reversed range direction (Root cause of Tekla placement mismatch)
                    bool hasReversedRange = d1.HasRange && d2.HasRange && d1.RangeVector.Dot(d2.RangeVector) < -0.01;

                    // Check 2: Bar count mismatch between groups
                    bool hasCountMismatch = d1.BarCount != d2.BarCount;

                    // Check 3: Direct Tekla Engine Splice Test
                    string spliceErr;
                    bool canSplice = TestRebarSplice(d1.RebarObject, d2.RebarObject, Math.Max(d1.Diameter, d2.Diameter), out spliceErr);

                    // A pair is flagged as Splice Error if:
                    // 1) Tekla Splice insertion fails, OR
                    // 2) The range direction is reversed, OR
                    // 3) Bar counts mismatch for adjacent parallel groups
                    if (!canSplice || hasReversedRange || hasCountMismatch)
                    {
                        string placementDiagnosis = DiagnosePlacementReason(d1, d2, angleDev, minEndGap, hasReversedRange, hasCountMismatch);

                        results.Add(new RebarErrorInfo
                        {
                            Category = RebarErrorCategory.SpliceFailed,
                            CategoryDisplayName = "Lỗi tạo Splice (Tekla Warning)",
                            Id1 = d1.Id,
                            Id2 = d2.Id,
                            ModelObject1 = d1.RebarObject,
                            ModelObject2 = d2.RebarObject,
                            BarSize1 = d1.Size,
                            BarSize2 = d2.Size,
                            DeviationValue = angleDev,
                            ErrorDetails = string.Format("Rebar splice could not be created! {0}", placementDiagnosis),
                            CenterPoint = midPoint,
                            MinPoint = pairMin,
                            MaxPoint = pairMax
                        });
                    }
                }

                processedCount++;
                if (dataList.Count <= 100 || (processedCount & 63) == 0 || processedCount == dataList.Count)
                {
                    progressCallback?.Invoke(totalRebars + processedCount, totalRebars * 2);
                }
            }

            // Assign sequential indices
            for (int i = 0; i < results.Count; i++)
            {
                results[i].Index = i + 1;
            }

            return results;
        }

        private static string DiagnosePlacementReason(RebarGeometryData d1, RebarGeometryData d2, double angleDev, double endGap, bool reversedRange, bool countMismatch)
        {
            if (reversedRange)
            {
                return "Hướng dải rải thép (StartPoint -> EndPoint) của 2 nhóm bị ngược chiều nhau. Tekla ghép chéo mút thanh khiến nối thất bại.";
            }
            if (countMismatch)
            {
                return string.Format("Số thanh không khớp nhau (Nhóm 1: {0} thanh, Nhóm 2: {1} thanh). Tekla không thể ghép đối ứng 1-1.", d1.BarCount, d2.BarCount);
            }
            if (angleDev > 3.0)
            {
                return string.Format("Hướng trục 2 nhóm thép bị lệch góc {0:F1}° (vượt quá dung sai nối thẳng).", angleDev);
            }
            if (endGap > 1500.0)
            {
                return string.Format("Khoảng hở giữa 2 đầu thanh quá lớn ({0:F0}mm) vượt quá phạm vi nối cho phép.", endGap);
            }
            return "Vị trí phân bố từng thanh con không đối ứng (Placements mismatch) hoặc Tekla Engine từ chối kiểu nối.";
        }

        private static bool TestRebarSplice(Reinforcement r1, Reinforcement r2, double barDia, out string errorReason)
        {
            errorReason = "";
            try
            {
                // Try combinations of Lap types
                RebarSplice.RebarSpliceTypeEnum[] spliceTypes = new RebarSplice.RebarSpliceTypeEnum[]
                {
                    RebarSplice.RebarSpliceTypeEnum.SPLICE_TYPE_LAP_RIGHT,
                    RebarSplice.RebarSpliceTypeEnum.SPLICE_TYPE_LAP_LEFT,
                    RebarSplice.RebarSpliceTypeEnum.SPLICE_TYPE_LAP_BOTH
                };

                RebarSplice.RebarSpliceBarPositionsEnum[] barPositions = new RebarSplice.RebarSpliceBarPositionsEnum[]
                {
                    RebarSplice.RebarSpliceBarPositionsEnum.SPLICE_BAR_PARALLEL,
                    RebarSplice.RebarSpliceBarPositionsEnum.SPLICE_BAR_ON_TOP
                };

                double[] testLapLengths = new double[]
                {
                    0.0,                              // Keep existing overlap
                    Math.Max(300.0, barDia * 40.0),   // Standard 40d
                    900.0                             // Typical job lap
                };

                foreach (var st in spliceTypes)
                {
                    foreach (var bp in barPositions)
                    {
                        foreach (var lap in testLapLengths)
                        {
                            RebarSplice splice = new RebarSplice();
                            splice.RebarGroup1 = r1;
                            splice.RebarGroup2 = r2;
                            splice.Type = st;
                            splice.LapLength = lap;
                            splice.Offset = 0.0;
                            splice.Clearance = 0.0;
                            splice.BarPositions = bp;

                            if (splice.Insert())
                            {
                                splice.Delete();
                                return true; // Splicing works!
                            }
                        }
                    }
                }

                errorReason = "Rebar splice could not be created. Check rebar type and rebar group placements.";
                return false;
            }
            catch (Exception ex)
            {
                errorReason = ex.Message;
                return false;
            }
        }

        private static bool AreBoundingBoxesNear(Point min1, Point max1, Point min2, Point max2, double tol)
        {
            if (min1.X - tol > max2.X || min2.X - tol > max1.X) return false;
            if (min1.Y - tol > max2.Y || min2.Y - tol > max1.Y) return false;
            if (min1.Z - tol > max2.Z || min2.Z - tol > max1.Z) return false;
            return true;
        }

        private static bool DoBoundingBoxesOverlap(Point min1, Point max1, Point min2, Point max2, double margin)
        {
            if (min1.X - margin > max2.X || min2.X - margin > max1.X) return false;
            if (min1.Y - margin > max2.Y || min2.Y - margin > max1.Y) return false;
            if (min1.Z - margin > max2.Z || min2.Z - margin > max1.Z) return false;
            return true;
        }

        private static double FindMinimumEndDistance(RebarGeometryData d1, RebarGeometryData d2, out Point pt1, out Point pt2)
        {
            double minGap = double.MaxValue;
            pt1 = d1.CenterPoint;
            pt2 = d2.CenterPoint;

            var ends1 = new List<Point>();
            ends1.AddRange(d1.StartPoints);
            ends1.AddRange(d1.EndPoints);

            var ends2 = new List<Point>();
            ends2.AddRange(d2.StartPoints);
            ends2.AddRange(d2.EndPoints);

            foreach (var pA in ends1)
            {
                foreach (var pB in ends2)
                {
                    double dist = Distance.PointToPoint(pA, pB);
                    if (dist < minGap)
                    {
                        minGap = dist;
                        pt1 = pA;
                        pt2 = pB;
                    }
                }
            }

            return minGap;
        }

        private static double CalculateAngleDegrees(Vector v1, Vector v2)
        {
            Vector n1 = new Vector(v1);
            Vector n2 = new Vector(v2);
            if (n1.Normalize() <= 0.0001 || n2.Normalize() <= 0.0001) return 0;

            double dot = Math.Abs(n1.Dot(n2));
            if (dot > 1.0) dot = 1.0;
            double rad = Math.Acos(dot);
            return rad * (180.0 / Math.PI);
        }

        private static double CalculatePerpendicularOffset(Point linePt, Vector lineDir, Point testPt)
        {
            Vector vLine = new Vector(lineDir);
            if (vLine.Normalize() <= 0.0001) return 0;

            Vector vToPt = new Vector(testPt.X - linePt.X, testPt.Y - linePt.Y, testPt.Z - linePt.Z);
            Vector cross = vToPt.Cross(vLine);
            return cross.GetLength();
        }
    }
}
