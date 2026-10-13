using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using ChangeShape.Models;

namespace ChangeShape.Services
{
    public class AutoCadBridge
    {
        [DllImport("ole32.dll")]
        private static extern int CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string lpszProgID, out Guid lpclsid);

        [DllImport("oleaut32.dll", PreserveSig = false)]
        private static extern void GetActiveObject(ref Guid rclsid, IntPtr pvReserved, [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private static readonly string BridgeJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cad_bridge.json");

        public static object? GetAutoCadApp()
        {
            try
            {
                int hr = CLSIDFromProgID("AutoCAD.Application", out Guid clsid);
                if (hr < 0) return null;

                GetActiveObject(ref clsid, IntPtr.Zero, out object obj);
                return obj;
            }
            catch
            {
                return null;
            }
        }

        public bool IsAutoCadAvailable()
        {
            var app = GetAutoCadApp();
            return app != null;
        }

        /// <summary>
        /// Focuses AutoCAD window.
        /// </summary>
        public void FocusAutoCad(dynamic acadApp)
        {
            try
            {
                long hwndLong = (long)acadApp.HWND;
                SetForegroundWindow(new IntPtr(hwndLong));
            }
            catch
            {
                // Fallback if HWND property access differs
            }
        }

        /// <summary>
        /// Interactively pick an MText rebar tag in AutoCAD.
        /// </summary>
        public CadRebarTag? PickMTextFromCad(out string errorMessage)
        {
            errorMessage = string.Empty;
            dynamic? acad = GetAutoCadApp();
            if (acad == null)
            {
                errorMessage = "Không tìm thấy phiên bản AutoCAD đang mở! Hãy chắc chắn AutoCAD đang hoạt động.";
                return CheckBridgeFileFallback();
            }

            try
            {
                FocusAutoCad(acad);
                dynamic doc = acad.ActiveDocument;
                dynamic util = doc.Utility;

                object pickedObj;
                object pickPoint;
                util.GetEntity(out pickedObj, out pickPoint, "\n[Change-Shape] Quét chọn thanh MText cốt thép cần đổi Shape: ");

                if (pickedObj == null)
                {
                    errorMessage = "Bạn chưa chọn đối tượng nào trong CAD.";
                    return null;
                }

                dynamic ent = pickedObj;
                string objName = ent.ObjectName;
                if (!objName.Contains("MText", StringComparison.OrdinalIgnoreCase) &&
                    !objName.Contains("Text", StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = $"Đối tượng chọn là '{objName}', vui lòng chọn đúng chữ MText hoặc Text thanh thép!";
                    return null;
                }

                string textString = ent.TextString;
                string handle = ent.Handle;

                var tag = CadRebarTag.Parse(textString, handle);
                return tag;
            }
            catch (COMException cex)
            {
                errorMessage = $"Lỗi giao tiếp AutoCAD: {cex.Message}\n(Mẹo: Nhấn ESC trong CAD để hủy lệnh dở dang rồi thử lại)";
                return CheckBridgeFileFallback();
            }
            catch (Exception ex)
            {
                errorMessage = $"Lỗi: {ex.Message}";
                return CheckBridgeFileFallback();
            }
        }

        /// <summary>
        /// Interactively measure distance between 2 points in AutoCAD.
        /// </summary>
        public double? MeasureDistanceInCad(string prompt, out string errorMessage)
        {
            errorMessage = string.Empty;
            dynamic? acad = GetAutoCadApp();
            if (acad == null)
            {
                errorMessage = "Không tìm thấy AutoCAD đang mở!";
                return null;
            }

            try
            {
                FocusAutoCad(acad);
                dynamic doc = acad.ActiveDocument;
                dynamic util = doc.Utility;

                object pt1 = util.GetPoint(Type.Missing, $"\n[Change-Shape] {prompt} - Chọn điểm 1: ");
                object pt2 = util.GetPoint(pt1, "\n[Change-Shape] Chọn điểm 2: ");

                double[] p1 = (double[])pt1;
                double[] p2 = (double[])pt2;

                double dx = p2[0] - p1[0];
                double dy = p2[1] - p1[1];
                double dist = Math.Sqrt(dx * dx + dy * dy);

                return Math.Round(dist);
            }
            catch (Exception ex)
            {
                errorMessage = $"Đo khoảng cách bị hủy: {ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Update MText in AutoCAD by entity handle or active document.
        /// </summary>
        public bool UpdateMTextInCad(string handle, string newFullText, out string errorMessage)
        {
            errorMessage = string.Empty;
            dynamic? acad = GetAutoCadApp();
            if (acad == null)
            {
                errorMessage = "AutoCAD không hoạt động.";
                SaveToBridgeFile(handle, newFullText);
                return false;
            }

            try
            {
                dynamic doc = acad.ActiveDocument;
                dynamic ent;

                if (!string.IsNullOrEmpty(handle))
                {
                    ent = doc.HandleToObject(handle);
                }
                else
                {
                    // If handle missing, prompt pick
                    FocusAutoCad(acad);
                    object pickedObj;
                    object pt;
                    doc.Utility.GetEntity(out pickedObj, out pt, "\nChọn MText cần áp dụng mã Shape mới: ");
                    ent = pickedObj;
                }

                if (ent != null)
                {
                    ent.TextString = newFullText;
                    ent.Update();
                    doc.Utility.Prompt("\n[Change-Shape] Đã cập nhật mã Shape và kích thước thành công!");
                    SaveToBridgeFile(handle, newFullText);
                    return true;
                }

                errorMessage = "Không tìm thấy đối tượng trong CAD để cập nhật.";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Lỗi cập nhật CAD: {ex.Message}";
                SaveToBridgeFile(handle, newFullText);
                return false;
            }
        }

        /// <summary>
        /// Batch updates multiple MTexts selected by user in AutoCAD.
        /// </summary>
        public int BatchUpdateMTextsInCad(string newShapeId, double newTotalLength, string tagParamString, out string errorMessage)
        {
            errorMessage = string.Empty;
            dynamic? acad = GetAutoCadApp();
            if (acad == null)
            {
                errorMessage = "AutoCAD không hoạt động!";
                return 0;
            }

            try
            {
                FocusAutoCad(acad);
                dynamic doc = acad.ActiveDocument;

                // Create or reuse temporary selection set
                string ssName = "CS_BATCH_SS_" + DateTime.Now.Ticks % 10000;
                dynamic ss;
                try
                {
                    ss = doc.SelectionSets.Add(ssName);
                }
                catch
                {
                    ss = doc.SelectionSets.Item(ssName);
                    ss.Clear();
                }

                doc.Utility.Prompt("\n[Change-Shape] Quét chọn các MText cốt thép cần đổi sang Shape mới:");
                ss.SelectOnScreen();

                int count = ss.Count;
                int updatedCount = 0;

                for (int i = 0; i < count; i++)
                {
                    dynamic ent = ss.Item(i);
                    string objName = ent.ObjectName;
                    if (objName.Contains("MText", StringComparison.OrdinalIgnoreCase) ||
                        objName.Contains("Text", StringComparison.OrdinalIgnoreCase))
                    {
                        string raw = ent.TextString;
                        var tag = CadRebarTag.Parse(raw, ent.Handle);
                        string updatedText = tag.GenerateMText(newShapeId, newTotalLength, tagParamString);
                        ent.TextString = updatedText;
                        ent.Update();
                        updatedCount++;
                    }
                }

                ss.Delete();
                doc.Utility.Prompt($"\n[Change-Shape] Đã cập nhật thành công {updatedCount} thanh MText sang Shape {newShapeId}!");
                return updatedCount;
            }
            catch (Exception ex)
            {
                errorMessage = $"Lỗi chọn hàng loạt: {ex.Message}";
                return 0;
            }
        }

        /// <summary>
        /// Draw a 2D polyline rebar geometry in AutoCAD.
        /// </summary>
        public bool DrawPolylineInCad(List<Point> localPoints, bool isClosed, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (localPoints == null || localPoints.Count < 2)
            {
                errorMessage = "Hình vẽ không có đủ tọa độ đỉnh!";
                return false;
            }

            dynamic? acad = GetAutoCadApp();
            if (acad == null)
            {
                errorMessage = "AutoCAD không hoạt động!";
                return false;
            }

            try
            {
                FocusAutoCad(acad);
                dynamic doc = acad.ActiveDocument;
                dynamic util = doc.Utility;

                object insertPt = util.GetPoint(Type.Missing, "\n[Change-Shape] Chọn điểm đặt (Insertion Point) để vẽ thanh thép: ");
                double[] basePt = (double[])insertPt;

                // Flatten coordinates
                int count = localPoints.Count;
                double[] coords = new double[count * 2];
                for (int i = 0; i < count; i++)
                {
                    coords[i * 2] = basePt[0] + localPoints[i].X;
                    coords[i * 2 + 1] = basePt[1] + localPoints[i].Y;
                }

                dynamic space = doc.ModelSpace;
                dynamic poly = space.AddLightWeightPolyline(coords);
                poly.Closed = isClosed;

                // Try setting layer
                try
                {
                    poly.Layer = "0-BIM-REBAR";
                }
                catch
                {
                    // Layer might not exist, keep current layer
                }

                poly.Update();
                util.Prompt("\n[Change-Shape] Đã vẽ đường Polyline thanh thép thành công!");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Lỗi vẽ Polyline: {ex.Message}";
                return false;
            }
        }

        private void SaveToBridgeFile(string handle, string text)
        {
            try
            {
                var data = new Dictionary<string, string>
                {
                    { "handle", handle },
                    { "text", text },
                    { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                };
                File.WriteAllText(BridgeJsonPath, JsonSerializer.Serialize(data));
            }
            catch { }
        }

        private CadRebarTag? CheckBridgeFileFallback()
        {
            try
            {
                if (File.Exists(BridgeJsonPath))
                {
                    string json = File.ReadAllText(BridgeJsonPath);
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("text", out var textElem))
                    {
                        string handle = doc.RootElement.TryGetProperty("handle", out var hElem) ? hElem.GetString() ?? "" : "";
                        return CadRebarTag.Parse(textElem.GetString() ?? "", handle);
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
