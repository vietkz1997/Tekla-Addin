using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.Collaboration;
using GeometryHelper;
using GeometryHelper.Geometry;
using GeometryHelper.Spatial;
using GeometryHelper.Core;
using GeometryHelper.TeklaConvert;

namespace BimCommands.Tekla.ClashCheck
{
    /// <summary>
    /// Geometric container holding high-performance GeometryHelper representations of an IFC element.
    /// Supports exact oriented sub-component OBBs (flanges, web, hollow walls) and triangulated BVH mesh trees.
    /// </summary>
    public class IfcGeometryRepresentation
    {
        public List<GeoSolid3> ExactSolids { get; set; } = new List<GeoSolid3>();
        public List<GeoObb3> SolidParts { get; set; } = new List<GeoObb3>();
        public GeoSolid3 BrepSolid { get; set; }
        public GeoBvh3 MeshBvh { get; set; }
        public bool IsHollowSection { get; set; }
        public bool HasExactSolids => ExactSolids != null && ExactSolids.Count > 0;
        public bool HasParametricGeometry => SolidParts.Count > 0;
    }

    /// <summary>
    /// Bridge connecting Tekla IFC Reference Models to GeometryHelper geometric entities.
    /// Extracts exact 3D parametric components from Tekla Collaboration Open API and B-Rep meshes.
    /// </summary>
    public static class IfcGeometryBridge
    {
        private static readonly ConcurrentDictionary<long, IfcGeometryRepresentation> _geoCache = new ConcurrentDictionary<long, IfcGeometryRepresentation>();

        /// <summary>
        /// Clears memory cache of parsed IFC geometries.
        /// </summary>
        public static void ClearCache()
        {
            _geoCache.Clear();
        }

        /// <summary>
        /// Builds an optimized GeometryHelper representation for a Tekla ReferenceModelObject.
        /// </summary>
        public static IfcGeometryRepresentation ExtractGeometry(
            ReferenceModelObject refObj, 
            Point bMin, 
            Point bMax, 
            Model teklaModel = null,
            IEnumerable<string> skipNames = null)
        {
            var rep = new IfcGeometryRepresentation();
            if (refObj == null) return rep;

            long objId = refObj.Identifier.ID;
            if (_geoCache.TryGetValue(objId, out var cachedRep))
            {
                return cachedRep;
            }

            // Step 0: Priority #1 - Extract 100% exact B-Rep solids directly via GeometryHelper.TeklaConvert & IfcConvert
            try
            {
                var opts = new GeometryHelper.IfcConvert.Core.IfcConvertOptions
                {
                    CoordinateSpace = GeometryHelper.IfcConvert.Core.CoordinateSpace.Global,
                    TargetUnit = GeometryHelper.IfcConvert.Core.LengthUnit.Millimeters,
                    ApplyVoids = false, // Critical performance boost: skips heavy CSG booleans
                    TessellateNonPlanarFaces = true
                };

                if (skipNames != null)
                {
                    foreach (var s in skipNames)
                    {
                        if (!string.IsNullOrWhiteSpace(s))
                            opts.AddSkipNames(s.Trim());
                    }
                }

                var solids = refObj.ToGeoSolids(opts);
                if (solids != null && solids.Length > 0)
                {
                    rep.ExactSolids = new List<GeoSolid3>(solids);
                    if (rep.ExactSolids.Count == 1)
                    {
                        rep.BrepSolid = rep.ExactSolids[0];
                    }
                    _geoCache[objId] = rep;
                    return rep;
                }
            }
            catch { }

            // Step 1: Attempt exact Parametric Profile Extraction from Tekla Collaboration API
            try
            {
                var attrEnum = new ReferenceModelObjectAttributeEnumerator(refObj);
                while (attrEnum.MoveNext())
                {
                    var attr = attrEnum.Current as ReferenceModelObjectAttribute;
                    if (attr == null || attr.Origin == null || attr.xDir == null || attr.Extrusion == null)
                        continue;

                    Vector vExt = attr.Extrusion;
                    double len = vExt.GetLength();
                    if (len <= 1.0) continue;

                    Vector vX = attr.xDir;
                    double lenX = vX.GetLength();
                    if (lenX <= 1e-4) continue;

                    // Orthonormal local basis vectors: Z = Extrusion, X = Local X, Y = Z x X
                    Vector uZ = new Vector(vExt.X / len, vExt.Y / len, vExt.Z / len);
                    Vector uX = new Vector(vX.X / lenX, vX.Y / lenX, vX.Z / lenX);
                    Vector uY = uZ.Cross(uX);
                    double lenY = uY.GetLength();
                    if (lenY <= 1e-4) continue;
                    uY = new Vector(uY.X / lenY, uY.Y / lenY, uY.Z / lenY);

                    GeoVector3 gvx = new GeoVector3(uX.X, uX.Y, uX.Z);
                    GeoVector3 gvy = new GeoVector3(uY.X, uY.Y, uY.Z);

                    Point origin = attr.Origin;

                    // Case A: I-Shape Profile (Composite Beam / Column)
                    if (attr is IFC2X3_ParametricObject_IShapeProfile iProf)
                    {
                        double width = iProf.OverallWidth > 0 ? iProf.OverallWidth : 200.0;
                        double depth = iProf.OverallDepth > 0 ? iProf.OverallDepth : 400.0;
                        double tf = iProf.FlangeThickness > 0 ? iProf.FlangeThickness : 20.0;
                        double tw = iProf.WebThickness > 0 ? iProf.WebThickness : 12.0;

                        // Top Flange
                        Point pTop = new Point(
                            origin.X + uZ.X * (len / 2.0) + uY.X * (depth / 2.0 - tf / 2.0),
                            origin.Y + uZ.Y * (len / 2.0) + uY.Y * (depth / 2.0 - tf / 2.0),
                            origin.Z + uZ.Z * (len / 2.0) + uY.Z * (depth / 2.0 - tf / 2.0)
                        );
                        rep.SolidParts.Add(new GeoObb3(new GeoPoint3(pTop.X, pTop.Y, pTop.Z), width, tf, len, gvx, gvy));

                        // Bottom Flange
                        Point pBot = new Point(
                            origin.X + uZ.X * (len / 2.0) - uY.X * (depth / 2.0 - tf / 2.0),
                            origin.Y + uZ.Y * (len / 2.0) - uY.Y * (depth / 2.0 - tf / 2.0),
                            origin.Z + uZ.Z * (len / 2.0) - uY.Z * (depth / 2.0 - tf / 2.0)
                        );
                        rep.SolidParts.Add(new GeoObb3(new GeoPoint3(pBot.X, pBot.Y, pBot.Z), width, tf, len, gvx, gvy));

                        // Web (spanning between flanges, leaves central bays hollow)
                        double webH = Math.Max(1.0, depth - 2.0 * tf);
                        Point pWeb = new Point(
                            origin.X + uZ.X * (len / 2.0),
                            origin.Y + uZ.Y * (len / 2.0),
                            origin.Z + uZ.Z * (len / 2.0)
                        );
                        rep.SolidParts.Add(new GeoObb3(new GeoPoint3(pWeb.X, pWeb.Y, pWeb.Z), tw, webH, len, gvx, gvy));

                        rep.IsHollowSection = true;
                        _geoCache[objId] = rep;
                        return rep;
                    }

                    // Case B: Rectangular Hollow Section (Box Column / Tube)
                    if (attr is IFC2X3_ParametricObject_RectangleHollowProfile rectHollow)
                    {
                        double width = rectHollow.XDim > 0 ? rectHollow.XDim : 200.0;
                        double depth = rectHollow.YDim > 0 ? rectHollow.YDim : 200.0;
                        double wallThk = rectHollow.WallThickness > 0 ? rectHollow.WallThickness : 12.0;

                        // 2 Vertical walls
                        double hWall = Math.Max(1.0, depth - 2.0 * wallThk);
                        Point pLeft = new Point(
                            origin.X + uZ.X * (len / 2.0) - uX.X * (width / 2.0 - wallThk / 2.0),
                            origin.Y + uZ.Y * (len / 2.0) - uX.Y * (width / 2.0 - wallThk / 2.0),
                            origin.Z + uZ.Z * (len / 2.0) - uX.Z * (width / 2.0 - wallThk / 2.0)
                        );
                        Point pRight = new Point(
                            origin.X + uZ.X * (len / 2.0) + uX.X * (width / 2.0 - wallThk / 2.0),
                            origin.Y + uZ.Y * (len / 2.0) + uX.Y * (width / 2.0 - wallThk / 2.0),
                            origin.Z + uZ.Z * (len / 2.0) + uX.Z * (width / 2.0 - wallThk / 2.0)
                        );
                        rep.SolidParts.Add(new GeoObb3(new GeoPoint3(pLeft.X, pLeft.Y, pLeft.Z), wallThk, hWall, len, gvx, gvy));
                        rep.SolidParts.Add(new GeoObb3(new GeoPoint3(pRight.X, pRight.Y, pRight.Z), wallThk, hWall, len, gvx, gvy));

                        // 2 Horizontal walls
                        Point pTop = new Point(
                            origin.X + uZ.X * (len / 2.0) + uY.X * (depth / 2.0 - wallThk / 2.0),
                            origin.Y + uZ.Y * (len / 2.0) + uY.Y * (depth / 2.0 - wallThk / 2.0),
                            origin.Z + uZ.Z * (len / 2.0) + uY.Z * (depth / 2.0 - wallThk / 2.0)
                        );
                        Point pBot = new Point(
                            origin.X + uZ.X * (len / 2.0) - uY.X * (depth / 2.0 - wallThk / 2.0),
                            origin.Y + uZ.Y * (len / 2.0) - uY.Y * (depth / 2.0 - wallThk / 2.0),
                            origin.Z + uZ.Z * (len / 2.0) - uY.Z * (depth / 2.0 - wallThk / 2.0)
                        );
                        rep.SolidParts.Add(new GeoObb3(new GeoPoint3(pTop.X, pTop.Y, pTop.Z), width, wallThk, len, gvx, gvy));
                        rep.SolidParts.Add(new GeoObb3(new GeoPoint3(pBot.X, pBot.Y, pBot.Z), width, wallThk, len, gvx, gvy));

                        rep.IsHollowSection = true;
                        _geoCache[objId] = rep;
                        return rep;
                    }

                    // Case C: Solid Rectangular Profile (Plate, Beam, Column, Wall)
                    if (attr is IFC2X3_ParametricObject_RectangleProfile rectProf)
                    {
                        double width = rectProf.XDim > 0 ? rectProf.XDim : 150.0;
                        double depth = rectProf.YDim > 0 ? rectProf.YDim : 150.0;
                        Point pCenter = new Point(
                            origin.X + uZ.X * (len / 2.0),
                            origin.Y + uZ.Y * (len / 2.0),
                            origin.Z + uZ.Z * (len / 2.0)
                        );
                        rep.SolidParts.Add(new GeoObb3(new GeoPoint3(pCenter.X, pCenter.Y, pCenter.Z), width, depth, len, gvx, gvy));
                        _geoCache[objId] = rep;
                        return rep;
                    }

                    // Case D: L-Shape Profile
                    if (attr is IFC2X3_ParametricObject_LShapeProfile lProf)
                    {
                        double width = lProf.Width > 0 ? lProf.Width : 100.0;
                        double depth = lProf.Depth > 0 ? lProf.Depth : 100.0;
                        double thk = lProf.Thickness > 0 ? lProf.Thickness : 10.0;

                        Point pV = new Point(
                            origin.X + uZ.X * (len / 2.0),
                            origin.Y + uZ.Y * (len / 2.0),
                            origin.Z + uZ.Z * (len / 2.0)
                        );
                        rep.SolidParts.Add(new GeoObb3(new GeoPoint3(pV.X, pV.Y, pV.Z), thk, depth, len, gvx, gvy));
                        rep.SolidParts.Add(new GeoObb3(new GeoPoint3(pV.X, pV.Y, pV.Z), width, thk, len, gvx, gvy));
                        _geoCache[objId] = rep;
                        return rep;
                    }

                    // Case E: ParametricObject_ObjectBoundingBox or any oriented attribute
                    // Extracts exact oriented bounding box from local axes (Origin = center, xDir & Extrusion = local half-axes)
                    if (attr.Origin != null && attr.xDir != null && attr.Extrusion != null)
                    {
                        Vector v1 = attr.xDir;
                        Vector v2 = attr.Extrusion;
                        double len1 = v1.GetLength();
                        double len2 = v2.GetLength();

                        if (len1 > 1e-3 && len2 > 1e-3)
                        {
                            Vector ax1 = new Vector(v1.X / len1, v1.Y / len1, v1.Z / len1);
                            Vector ax2 = new Vector(v2.X / len2, v2.Y / len2, v2.Z / len2);
                            Vector ax3 = ax1.Cross(ax2);
                            double len3 = ax3.GetLength();

                            if (len3 > 1e-3)
                            {
                                ax3 = new Vector(ax3.X / len3, ax3.Y / len3, ax3.Z / len3);

                                double h1 = len1;
                                double h2 = len2;

                                // Project AABB extents onto axis 3 to get exact longitudinal half-extent
                                double dX_box = bMax.X - bMin.X;
                                double dY_box = bMax.Y - bMin.Y;
                                double dZ_box = bMax.Z - bMin.Z;
                                double h3 = (Math.Abs(ax3.X) * dX_box + Math.Abs(ax3.Y) * dY_box + Math.Abs(ax3.Z) * dZ_box) / 2.0;
                                if (h3 < 1.0) h3 = Math.Max(h1, h2);

                                GeoPoint3 cPt = new GeoPoint3(attr.Origin.X, attr.Origin.Y, attr.Origin.Z);

                                string rmoName = string.Empty;
                                try
                                {
                                    refObj.GetReportProperty("REFERENCE_MODEL_OBJECT.NAME", ref rmoName);
                                    if (string.IsNullOrEmpty(rmoName)) refObj.GetReportProperty("NAME", ref rmoName);
                                }
                                catch { }

                                string tag = (rmoName + " " + attr.ProfileName + " " + attr.Name);

                                // Attempt to decompose into accurate I-section components (flanges + web)
                                if (TryDecomposeIBeam(cPt, 2.0 * h1, 2.0 * h2, 2.0 * h3, ax1, ax2, ax3, tag, rep))
                                {
                                    _geoCache[objId] = rep;
                                    return rep;
                                }

                                GeoVector3 gv1 = new GeoVector3(ax1.X, ax1.Y, ax1.Z);
                                GeoVector3 gv2 = new GeoVector3(ax2.X, ax2.Y, ax2.Z);

                                rep.SolidParts.Add(new GeoObb3(cPt, 2.0 * h1, 2.0 * h2, 2.0 * h3, gv1, gv2));
                                _geoCache[objId] = rep;
                                return rep;
                            }
                        }
                    }
                }
            }
            catch { }

            // Step 2: Attempt xBIM Brep Mesh Extraction for complex geometric shapes
            if (teklaModel != null)
            {
                try
                {
                    var dummyObs = new IfcBoxInfo
                    {
                        ModelObject = refObj,
                        MinPoint = bMin,
                        MaxPoint = bMax
                    };
                    var exactSolids = XbimGeometryManager.GetExactSolids(dummyObs, teklaModel);
                    if (exactSolids != null && exactSolids.Count > 0)
                    {
                        rep.BrepSolid = exactSolids[0];
                        rep.MeshBvh = GeoBvh3.FromSolid(rep.BrepSolid);
                        _geoCache[objId] = rep;
                        return rep;
                    }
                }
                catch { }
            }

            // Step 3: Structural fallback - Oriented bounding box or decomposition
            double dX = bMax.X - bMin.X;
            double dY = bMax.Y - bMin.Y;
            double dZ = bMax.Z - bMin.Z;
            GeoPoint3 center = new GeoPoint3((bMin.X + bMax.X) / 2.0, (bMin.Y + bMax.Y) / 2.0, (bMin.Z + bMax.Z) / 2.0);

            string fbName = string.Empty;
            try
            {
                refObj.GetReportProperty("REFERENCE_MODEL_OBJECT.NAME", ref fbName);
                if (string.IsNullOrEmpty(fbName)) refObj.GetReportProperty("NAME", ref fbName);
            }
            catch { }

            // If the element is slanted diagonally in the XY plane (e.g. beam/girder at 45 degrees):
            // construct an oriented OBB along the diagonal to prevent enclosing huge triangle airspace!
            if (dX > 400.0 && dY > 400.0 && dZ < Math.Min(dX, dY))
            {
                double diagLen = Math.Sqrt(dX * dX + dY * dY);
                double invDiag = 1.0 / diagLen;
                Vector vLong = new Vector(dX * invDiag, dY * invDiag, 0.0);
                Vector vTrans = new Vector(-dY * invDiag, dX * invDiag, 0.0);
                Vector vUp = new Vector(0.0, 0.0, 1.0);

                double beamWidth = Math.Min(350.0, Math.Min(dX, dY));
                double beamHeight = Math.Max(1.0, dZ);

                if (TryDecomposeIBeam(center, beamWidth, beamHeight, diagLen, vTrans, vUp, vLong, fbName, rep))
                {
                    _geoCache[objId] = rep;
                    return rep;
                }

                GeoVector3 gvTrans = new GeoVector3(vTrans.X, vTrans.Y, vTrans.Z);
                GeoVector3 gvUp = new GeoVector3(vUp.X, vUp.Y, vUp.Z);
                rep.SolidParts.Add(new GeoObb3(center, beamWidth, beamHeight, diagLen, gvTrans, gvUp));
                _geoCache[objId] = rep;
                return rep;
            }

            // Check if orthogonal member (beam along X or Y)
            Vector stdX = new Vector(1, 0, 0);
            Vector stdY = new Vector(0, 1, 0);
            Vector stdZ = new Vector(0, 0, 1);
            if (TryDecomposeIBeam(center, Math.Max(1.0, dX), Math.Max(1.0, dY), Math.Max(1.0, dZ), stdX, stdY, stdZ, fbName, rep))
            {
                _geoCache[objId] = rep;
                return rep;
            }

            rep.SolidParts.Add(new GeoObb3(center, Math.Max(1.0, dX), Math.Max(1.0, dY), Math.Max(1.0, dZ)));
            _geoCache[objId] = rep;
            return rep;
        }

        /// <summary>
        /// Highly optimized zero-allocation test: checks distance & overlap between a 3D rebar line segment
        /// and a single GeoObb3 component.
        /// </summary>
        public static bool TestSegmentVsObb(
            GeoPoint3 p1, 
            GeoPoint3 p2, 
            double rebarRadius, 
            double clearance, 
            double tolerance, 
            GeoObb3 obb, 
            out double overlap, 
            out Point clashPt)
        {
            overlap = 0.0;
            clashPt = null;
            if (obb == null) return false;

            double effRadius = rebarRadius + clearance;

            // Transform segment into local coordinate system of the OBB
            // In local coordinates, the OBB is an axis-aligned box centered at (0, 0, 0)
            // with half-extents (hx, hy, hz).
            GeoPoint3 c = obb.Center;
            GeoVector3 vx = obb.AxisX;
            GeoVector3 vy = obb.AxisY;
            GeoVector3 vz = obb.AxisZ;

            double hx = obb.SizeX / 2.0;
            double hy = obb.SizeY / 2.0;
            double hz = obb.SizeZ / 2.0;

            // Vector from center to p1 and p2
            double dx1 = p1.X - c.X, dy1 = p1.Y - c.Y, dz1 = p1.Z - c.Z;
            double dx2 = p2.X - c.X, dy2 = p2.Y - c.Y, dz2 = p2.Z - c.Z;

            // Project onto local axes
            double lx1 = dx1 * vx.X + dy1 * vx.Y + dz1 * vx.Z;
            double ly1 = dx1 * vy.X + dy1 * vy.Y + dz1 * vy.Z;
            double lz1 = dx1 * vz.X + dy1 * vz.Y + dz1 * vz.Z;

            double lx2 = dx2 * vx.X + dy2 * vx.Y + dz2 * vx.Z;
            double ly2 = dx2 * vy.X + dy2 * vy.Y + dz2 * vy.Z;
            double lz2 = dx2 * vz.X + dy2 * vz.Y + dz2 * vz.Z;

            // Check ray/segment penetration against local box [-hx, hx], [-hy, hy], [-hz, hz]
            double slx = lx2 - lx1;
            double sly = ly2 - ly1;
            double slz = lz2 - lz1;

            double tMin = 0.0;
            double tMax = 1.0;

            // X slab
            if (Math.Abs(slx) < 1e-9)
            {
                if (lx1 < -hx || lx1 > hx) goto OutsideCheck;
            }
            else
            {
                double t0 = (-hx - lx1) / slx;
                double t1 = (hx - lx1) / slx;
                if (t0 > t1) { double t = t0; t0 = t1; t1 = t; }
                tMin = Math.Max(tMin, t0);
                tMax = Math.Min(tMax, t1);
                if (tMin > tMax) goto OutsideCheck;
            }

            // Y slab
            if (Math.Abs(sly) < 1e-9)
            {
                if (ly1 < -hy || ly1 > hy) goto OutsideCheck;
            }
            else
            {
                double t0 = (-hy - ly1) / sly;
                double t1 = (hy - ly1) / sly;
                if (t0 > t1) { double t = t0; t0 = t1; t1 = t; }
                tMin = Math.Max(tMin, t0);
                tMax = Math.Min(tMax, t1);
                if (tMin > tMax) goto OutsideCheck;
            }

            // Z slab
            if (Math.Abs(slz) < 1e-9)
            {
                if (lz1 < -hz || lz1 > hz) goto OutsideCheck;
            }
            else
            {
                double t0 = (-hz - lz1) / slz;
                double t1 = (hz - lz1) / slz;
                if (t0 > t1) { double t = t0; t0 = t1; t1 = t; }
                tMin = Math.Max(tMin, t0);
                tMax = Math.Min(tMax, t1);
                if (tMin > tMax) goto OutsideCheck;
            }

            // If segment penetrates the box interior
            if (tMax >= 0.0 && tMin <= 1.0 && tMin <= tMax)
            {
                double tIn = Math.Max(0.0, tMin);
                double tOut = Math.Min(1.0, tMax);
                double penLen = Math.Sqrt((tOut - tIn) * (tOut - tIn) * (slx * slx + sly * sly + slz * slz));

                if (penLen >= tolerance)
                {
                    double tMid = (tIn + tOut) / 2.0;
                    double midLx = lx1 + tMid * slx;
                    double midLy = ly1 + tMid * sly;
                    double midLz = lz1 + tMid * slz;

                    // Transform back to world coordinates
                    Point wClash = new Point(
                        c.X + midLx * vx.X + midLy * vy.X + midLz * vz.X,
                        c.Y + midLx * vx.Y + midLy * vy.Y + midLz * vz.Y,
                        c.Z + midLx * vx.Z + midLy * vy.Z + midLz * vz.Z
                    );

                    double minBoxDim = Math.Min(obb.SizeX, Math.Min(obb.SizeY, obb.SizeZ));
                    overlap = Math.Min(penLen, minBoxDim) + rebarRadius;
                    clashPt = wClash;
                    return true;
                }
            }

        OutsideCheck:
            // Segment did not penetrate solid core: check if cylindrical skin breaches clearance or touches face
            // Find closest point on local segment to local box [-hx, hx], [-hy, hy], [-hz, hz]
            double bestT = 0.5;
            double low = 0.0, high = 1.0;

            double EvalDeriv(double t)
            {
                double x = lx1 + t * slx;
                double y = ly1 + t * sly;
                double z = lz1 + t * slz;

                double gx = x < -hx ? (x + hx) : (x > hx ? (x - hx) : 0.0);
                double gy = y < -hy ? (y + hy) : (y > hy ? (y - hy) : 0.0);
                double gz = z < -hz ? (z + hz) : (z > hz ? (z - hz) : 0.0);

                return 2.0 * (gx * slx + gy * sly + gz * slz);
            }

            if (EvalDeriv(0.0) >= 0.0) bestT = 0.0;
            else if (EvalDeriv(1.0) <= 0.0) bestT = 1.0;
            else
            {
                for (int iter = 0; iter < 10; iter++)
                {
                    double mid = 0.5 * (low + high);
                    if (EvalDeriv(mid) < 0.0) low = mid;
                    else high = mid;
                }
                bestT = 0.5 * (low + high);
            }

            double qx = lx1 + bestT * slx;
            double qy = ly1 + bestT * sly;
            double qz = lz1 + bestT * slz;

            double bx = Math.Max(-hx, Math.Min(qx, hx));
            double by = Math.Max(-hy, Math.Min(qy, hy));
            double bz = Math.Max(-hz, Math.Min(qz, hz));

            double distSq = (qx - bx) * (qx - bx) + (qy - by) * (qy - by) + (qz - bz) * (qz - bz);
            if (distSq <= effRadius * effRadius)
            {
                double dist = Math.Sqrt(distSq);
                overlap = effRadius - dist;
                if (overlap >= tolerance)
                {
                    clashPt = new Point(
                        c.X + bx * vx.X + by * vy.X + bz * vz.X,
                        c.Y + bx * vx.Y + by * vy.Y + bz * vz.Y,
                        c.Z + bx * vx.Z + by * vy.Z + bz * vz.Z
                    );
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// High-speed test using GeometryHelper GeoBvh3 surface mesh.
        /// </summary>
        public static bool TestSegmentVsMeshBvh(
            GeoPoint3 p1, 
            GeoPoint3 p2, 
            double rebarRadius, 
            double clearance, 
            double tolerance, 
            GeoBvh3 bvh, 
            out double overlap, 
            out Point clashPt)
        {
            overlap = 0.0;
            clashPt = null;
            if (bvh == null) return false;

            double effRadius = rebarRadius + clearance;

            // 1. Ray intersection check along segment
            GeoVector3 dir = p1.GetVectorTo(p2);
            double segLen = p1.DistanceTo(p2);
            if (segLen > 1e-4)
            {
                GeoRay3 ray = new GeoRay3(p1, dir);
                var hits = bvh.GetIntersections(ray);
                if (hits != null && hits.Length > 0)
                {
                    foreach (var hit in hits)
                    {
                        double distFromStart = p1.DistanceTo(hit);
                        if (distFromStart <= segLen + tolerance)
                        {
                            overlap = rebarRadius;
                            clashPt = new Point(hit.X, hit.Y, hit.Z);
                            return true;
                        }
                    }
                }
            }

            // 2. Sample points along segment to query closest point on mesh surface
            int samples = Math.Max(2, (int)(segLen / 200.0) + 1);
            double minSurfDist = double.MaxValue;
            GeoPoint3 bestSurfPt = p1;

            for (int i = 0; i <= samples; i++)
            {
                double t = (double)i / samples;
                GeoPoint3 samplePt = new GeoPoint3(
                    p1.X + t * (p2.X - p1.X),
                    p1.Y + t * (p2.Y - p1.Y),
                    p1.Z + t * (p2.Z - p1.Z)
                );

                double d = bvh.DistanceTo(samplePt);
                if (d < minSurfDist)
                {
                    minSurfDist = d;
                    bestSurfPt = bvh.GetClosestPoint(samplePt);
                }
            }

            if (minSurfDist <= effRadius)
            {
                overlap = effRadius - minSurfDist;
                if (overlap >= tolerance)
                {
                    clashPt = new Point(bestSurfPt.X, bestSurfPt.Y, bestSurfPt.Z);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to decompose an oriented structural box (Girder, Beam, Column) into 3 accurate GeoObb3 parts:
        /// Top Flange, Bottom Flange, and Web. This leaves the side bays hollow, avoiding false clashes with rebars.
        /// </summary>
        private static bool TryDecomposeIBeam(
            GeoPoint3 cPt, 
            double dim1, 
            double dim2, 
            double dim3, 
            Vector ax1, 
            Vector ax2, 
            Vector ax3, 
            string tag, 
            IfcGeometryRepresentation rep)
        {
            if (rep == null) return false;

            // Find longitudinal axis (largest dimension)
            double maxDim = dim1;
            Vector vLong = ax1;
            Vector vCrossA = ax2;
            Vector vCrossB = ax3;
            double dimA = dim2;
            double dimB = dim3;

            if (dim2 > maxDim && dim2 > dim3)
            {
                maxDim = dim2;
                vLong = ax2;
                vCrossA = ax1;
                vCrossB = ax3;
                dimA = dim1;
                dimB = dim3;
            }
            else if (dim3 > maxDim)
            {
                maxDim = dim3;
                vLong = ax3;
                vCrossA = ax1;
                vCrossB = ax2;
                dimA = dim1;
                dimB = dim2;
            }

            // Cross section: depth is typically the larger dimension, width is the smaller
            double depth = Math.Max(dimA, dimB);
            double width = Math.Min(dimA, dimB);
            Vector vDepth = (dimA >= dimB) ? vCrossA : vCrossB;
            Vector vWidth = (dimA >= dimB) ? vCrossB : vCrossA;

            double length = maxDim;

            // Check if this object is a plate (one dimension very thin <= 40mm)
            if (width <= 40.0 && depth <= 120.0)
            {
                return false;
            }

            string upperTag = !string.IsNullOrEmpty(tag) ? tag.ToUpperInvariant() : string.Empty;
            bool isExplicitIBeam = upperTag.Contains("GIRDER") || upperTag.Contains("BEAM") || upperTag.Contains("COLUMN") ||
                                   upperTag.Contains("UB") || upperTag.Contains("UC") || upperTag.Contains("IPE") ||
                                   upperTag.Contains("HE") || upperTag.Contains("H-") || upperTag.Contains("H_") ||
                                   upperTag.Contains("W-") || upperTag.Contains("W_") || upperTag.Contains("W ") ||
                                   upperTag.Contains("JOIST") || upperTag.Contains("STEEL");

            bool isGeometricIBeam = (length >= 1.2 * depth) && (depth >= 150.0) && (width >= 80.0);

            if (!isExplicitIBeam && !isGeometricIBeam)
            {
                return false;
            }

            // Flange and web thicknesses (conservative estimates)
            double tf = Math.Max(12.0, Math.Min(35.0, depth * 0.08));
            double tw = Math.Max(8.0, Math.Min(25.0, width * 0.08));
            double webH = Math.Max(10.0, depth - 2.0 * tf);

            // GeoVectors for orientation
            GeoVector3 gvWidth = new GeoVector3(vWidth.X, vWidth.Y, vWidth.Z);
            GeoVector3 gvDepth = new GeoVector3(vDepth.X, vDepth.Y, vDepth.Z);

            double offsetFlange = (depth - tf) / 2.0;

            // Top Flange
            GeoPoint3 pTop = new GeoPoint3(
                cPt.X + vDepth.X * offsetFlange,
                cPt.Y + vDepth.Y * offsetFlange,
                cPt.Z + vDepth.Z * offsetFlange
            );
            rep.SolidParts.Add(new GeoObb3(pTop, width, tf, length, gvWidth, gvDepth));

            // Bottom Flange
            GeoPoint3 pBot = new GeoPoint3(
                cPt.X - vDepth.X * offsetFlange,
                cPt.Y - vDepth.Y * offsetFlange,
                cPt.Z - vDepth.Z * offsetFlange
            );
            rep.SolidParts.Add(new GeoObb3(pBot, width, tf, length, gvWidth, gvDepth));

            // Web
            rep.SolidParts.Add(new GeoObb3(cPt, tw, webH, length, gvWidth, gvDepth));

            rep.IsHollowSection = true;
            return true;
        }

        /// <summary>
        /// Tests a full rebar polyline against an IFC geometry representation using GeometryHelper's TrySplitBy.
        /// Accurately detects penetrations and clearance breaches without false positives in hollow/I-beam bays.
        /// </summary>
        public static bool TestPolylineVsGeometry(
            GeoPolyline3 rebarPolyline,
            double rebarRadius,
            double clearance,
            double tolerance,
            IfcGeometryRepresentation geomRep,
            out double overlap,
            out Point clashPt)
        {
            overlap = 0.0;
            clashPt = null;
            if (rebarPolyline == null || geomRep == null) return false;

            double effRadius = rebarRadius + clearance;
            Tolerance tol = new Tolerance(0.01, 0.01, Math.PI / 180.0, 0.01);

            // 0. Exact 100% B-Rep Solid from GeometryHelper.TeklaConvert.ToGeoSolids()
            if (geomRep.HasExactSolids)
            {
                var solidArr = geomRep.ExactSolids.ToArray();
                if (rebarPolyline.TrySplitBy(solidArr, out GeoPolyline3[] inside, out GeoPolyline3[] outside, tol))
                {
                    if (inside != null && inside.Length > 0)
                    {
                        double totalInsideLen = 0.0;
                        GeoPoint3 bestPt = inside[0].StartPoint;
                        double maxPieceLen = 0.0;

                        foreach (var piece in inside)
                        {
                            if (piece != null && piece.Length > 0.0)
                            {
                                totalInsideLen += piece.Length;
                                if (piece.Length > maxPieceLen)
                                {
                                    maxPieceLen = piece.Length;
                                    if (piece.VertexCount >= 2)
                                    {
                                        var p0 = piece[0];
                                        var p1 = piece[piece.VertexCount - 1];
                                        bestPt = new GeoPoint3((p0.X + p1.X) / 2.0, (p0.Y + p1.Y) / 2.0, (p0.Z + p1.Z) / 2.0);
                                    }
                                    else
                                    {
                                        bestPt = piece.StartPoint;
                                    }
                                }
                            }
                        }

                        if (totalInsideLen >= tolerance || maxPieceLen >= tolerance)
                        {
                            overlap = totalInsideLen + rebarRadius;
                            clashPt = new Point(bestPt.X, bestPt.Y, bestPt.Z);
                            return true;
                        }
                    }
                }

                // Skin / Clearance check: If centerline doesn't penetrate, check if rebar skin touches within effRadius
                if (effRadius > 0.0)
                {
                    for (int eIdx = 0; eIdx < rebarPolyline.EdgeCount; eIdx++)
                    {
                        var edge = rebarPolyline.GetEdgeAt(eIdx);
                        double eMinX = Math.Min(edge.StartPoint.X, edge.EndPoint.X) - effRadius;
                        double eMaxX = Math.Max(edge.StartPoint.X, edge.EndPoint.X) + effRadius;
                        double eMinY = Math.Min(edge.StartPoint.Y, edge.EndPoint.Y) - effRadius;
                        double eMaxY = Math.Max(edge.StartPoint.Y, edge.EndPoint.Y) + effRadius;
                        double eMinZ = Math.Min(edge.StartPoint.Z, edge.EndPoint.Z) - effRadius;
                        double eMaxZ = Math.Max(edge.StartPoint.Z, edge.EndPoint.Z) + effRadius;

                        foreach (var solid in geomRep.ExactSolids)
                        {
                            if (solid == null) continue;
                            var sAabb = solid.GetAabb();
                            if (eMinX > sAabb.Max.X || eMaxX < sAabb.Min.X ||
                                eMinY > sAabb.Max.Y || eMaxY < sAabb.Min.Y ||
                                eMinZ > sAabb.Max.Z || eMaxZ < sAabb.Min.Z)
                            {
                                continue;
                            }

                            double dist = Distance3.DistanceTo(edge, solid, tol);
                            if (dist <= effRadius)
                            {
                                double skinOverlap = effRadius - dist;
                                if (skinOverlap > overlap)
                                {
                                    overlap = skinOverlap;
                                    var mid = new GeoPoint3((edge.StartPoint.X + edge.EndPoint.X) / 2.0,
                                                           (edge.StartPoint.Y + edge.EndPoint.Y) / 2.0,
                                                           (edge.StartPoint.Z + edge.EndPoint.Z) / 2.0);
                                    clashPt = new Point(mid.X, mid.Y, mid.Z);
                                }
                            }
                        }
                    }

                    if (overlap >= tolerance && clashPt != null)
                    {
                        return true;
                    }
                }

                return false;
            }

            // 1. Exact Parametric Sub-components (Flanges, Web, Walls, Plates)
            if (geomRep.SolidParts != null && geomRep.SolidParts.Count > 0)
            {
                // Inflate each OBB by effRadius so centerline intersection corresponds to cylinder skin collision
                GeoObb3[] cutters = new GeoObb3[geomRep.SolidParts.Count];
                for (int i = 0; i < geomRep.SolidParts.Count; i++)
                {
                    var obb = geomRep.SolidParts[i];
                    cutters[i] = new GeoObb3(
                        obb.Center,
                        obb.SizeX + 2.0 * effRadius,
                        obb.SizeY + 2.0 * effRadius,
                        obb.SizeZ + 2.0 * effRadius,
                        obb.AxisX,
                        obb.AxisY
                    );
                }

                if (rebarPolyline.TrySplitBy(cutters, out GeoPolyline3[] inside, out GeoPolyline3[] outside, tol))
                {
                    if (inside != null && inside.Length > 0)
                    {
                        double totalInsideLen = 0.0;
                        GeoPoint3 bestPt = inside[0].StartPoint;
                        double maxPieceLen = 0.0;

                        foreach (var piece in inside)
                        {
                            if (piece != null && piece.Length > 0.0)
                            {
                                totalInsideLen += piece.Length;
                                if (piece.Length > maxPieceLen)
                                {
                                    maxPieceLen = piece.Length;
                                    if (piece.VertexCount >= 2)
                                    {
                                        var p0 = piece[0];
                                        var p1 = piece[piece.VertexCount - 1];
                                        bestPt = new GeoPoint3((p0.X + p1.X) / 2.0, (p0.Y + p1.Y) / 2.0, (p0.Z + p1.Z) / 2.0);
                                    }
                                    else
                                    {
                                        bestPt = piece.StartPoint;
                                    }
                                }
                            }
                        }

                        if (totalInsideLen >= tolerance || maxPieceLen >= tolerance)
                        {
                            overlap = totalInsideLen + rebarRadius;
                            clashPt = new Point(bestPt.X, bestPt.Y, bestPt.Z);
                            return true;
                        }
                    }
                }

                return false;
            }

            // 2. Exact B-Rep Solid (if available from xBIM)
            if (geomRep.BrepSolid != null)
            {
                if (rebarPolyline.TrySplitBy(geomRep.BrepSolid, out GeoPolyline3[] inside, out GeoPolyline3[] outside, tol))
                {
                    if (inside != null && inside.Length > 0)
                    {
                        double totalLen = 0.0;
                        GeoPoint3 bestPt = inside[0].StartPoint;
                        foreach (var piece in inside)
                        {
                            if (piece != null)
                            {
                                totalLen += piece.Length;
                                bestPt = piece.StartPoint;
                            }
                        }

                        if (totalLen >= tolerance)
                        {
                            overlap = totalLen + rebarRadius;
                            clashPt = new Point(bestPt.X, bestPt.Y, bestPt.Z);
                            return true;
                        }
                    }
                }
            }

            // 3. Triangulated Mesh BVH Tree
            if (geomRep.MeshBvh != null)
            {
                for (int i = 0; i < rebarPolyline.EdgeCount; i++)
                {
                    var edge = rebarPolyline.GetEdgeAt(i);
                    if (TestSegmentVsMeshBvh(edge.StartPoint, edge.EndPoint, rebarRadius, clearance, tolerance, geomRep.MeshBvh, out double bvhOverlap, out Point bvhPt))
                    {
                        if (bvhOverlap > overlap)
                        {
                            overlap = bvhOverlap;
                            clashPt = bvhPt;
                        }
                    }
                }

                if (overlap >= tolerance && clashPt != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
