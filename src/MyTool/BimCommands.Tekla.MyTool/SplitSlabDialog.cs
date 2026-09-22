using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace BimCommands.Tekla.MyTool;

public class SplitSlabDialog : Form
{
	private Model _model;

	private GroupBox grpParams;

	private Label lblGap;

	private NumericUpDown numGap;

	private Label lblNewClass;

	private ComboBox cboNewClass;

	private CheckBox chkSelectNew;

	private CheckBox chkAutoOrtho;

	private GroupBox grpActions;

	private Button btnSplitMulti;

	private Button btnSplitSingle;

	private Button btnSplit1Point;

	private Button btnSplitContinuous;

	private Label lblTip;

	private GroupBox grpLog;

	private ListBox lstLog;

	private Button btnClearLog;

	public SplitSlabDialog(Model model)
	{
		_model = model;
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		this.Text = "My-tool  ➤  Split-Slab (Tách sàn bê tông)";
		base.Size = new System.Drawing.Size(460, 670);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.TopMost = true;
		this.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
		this.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular);
		this.grpParams = new System.Windows.Forms.GroupBox
		{
			Text = " ⚙️ THIẾT LẬP CẮT SÀN ",
			Location = new System.Drawing.Point(14, 12),
			Size = new System.Drawing.Size(416, 150),
			Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(30, 41, 59)
		};
		this.lblGap = new System.Windows.Forms.Label
		{
			Text = "Khe co giãn (Joint Gap mm):",
			Location = new System.Drawing.Point(16, 26),
			AutoSize = true,
			Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular),
			ForeColor = System.Drawing.Color.Black
		};
		this.numGap = new System.Windows.Forms.NumericUpDown
		{
			Location = new System.Drawing.Point(230, 23),
			Width = 165,
			Minimum = 0m,
			Maximum = 10000m,
			Value = 0m,
			DecimalPlaces = 1,
			TextAlign = System.Windows.Forms.HorizontalAlignment.Right,
			Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold)
		};
		this.lblNewClass = new System.Windows.Forms.Label
		{
			Text = "Class cho sàn mới:",
			Location = new System.Drawing.Point(16, 60),
			AutoSize = true,
			Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular),
			ForeColor = System.Drawing.Color.Black
		};
		this.cboNewClass = new System.Windows.Forms.ComboBox
		{
			Location = new System.Drawing.Point(230, 57),
			Width = 165,
			DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList,
			Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Regular)
		};
		this.cboNewClass.Items.Add("Giữ nguyên Class gốc");
		this.cboNewClass.Items.Add("Tự động tăng (+1)");
		this.cboNewClass.Items.Add("Class 6 (Màu cam)");
		this.cboNewClass.Items.Add("Class 3 (Màu xanh)");
		this.cboNewClass.Items.Add("Class 1 (Màu đỏ)");
		this.cboNewClass.SelectedIndex = 0;
		this.chkSelectNew = new System.Windows.Forms.CheckBox
		{
			Text = "Tự động chọn sàn sau khi tách (Highlight)",
			Location = new System.Drawing.Point(19, 91),
			AutoSize = true,
			Checked = true,
			Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Regular),
			ForeColor = System.Drawing.Color.FromArgb(71, 85, 105)
		};
		this.chkAutoOrtho = new System.Windows.Forms.CheckBox
		{
			Text = "🎯 Tự động nắn thẳng (Auto-Ortho / Cạnh sàn / Trục X-Y)",
			Location = new System.Drawing.Point(19, 116),
			AutoSize = true,
			Checked = true,
			Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(2, 132, 199)
		};
		this.grpParams.Controls.Add(this.lblGap);
		this.grpParams.Controls.Add(this.numGap);
		this.grpParams.Controls.Add(this.lblNewClass);
		this.grpParams.Controls.Add(this.cboNewClass);
		this.grpParams.Controls.Add(this.chkSelectNew);
		this.grpParams.Controls.Add(this.chkAutoOrtho);
		this.grpActions = new System.Windows.Forms.GroupBox
		{
			Text = " 🎯 THAO TÁC ",
			Location = new System.Drawing.Point(14, 168),
			Size = new System.Drawing.Size(416, 230),
			Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(30, 41, 59)
		};
		this.btnSplitMulti = new System.Windows.Forms.Button
		{
			Text = "🎯  TÁCH ĐA ĐIỂM  (Pick các điểm ➔ Chuột giữa / Enter)",
			Location = new System.Drawing.Point(16, 23),
			Size = new System.Drawing.Size(378, 40),
			BackColor = System.Drawing.Color.FromArgb(16, 185, 129),
			ForeColor = System.Drawing.Color.White,
			Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold),
			FlatStyle = System.Windows.Forms.FlatStyle.Flat,
			Cursor = System.Windows.Forms.Cursors.Hand
		};
		this.btnSplitMulti.FlatAppearance.BorderSize = 0;
		this.btnSplitMulti.Click += delegate
		{
			this.RunSplitMultiProcess(false);
		};
		this.btnSplit1Point = new System.Windows.Forms.Button
		{
			Text = "📐  TÁCH 1 ĐIỂM  (Cắt vuông góc cạnh mép sàn)",
			Location = new System.Drawing.Point(16, 68),
			Size = new System.Drawing.Size(378, 36),
			BackColor = System.Drawing.Color.FromArgb(124, 58, 237),
			ForeColor = System.Drawing.Color.White,
			Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			FlatStyle = System.Windows.Forms.FlatStyle.Flat,
			Cursor = System.Windows.Forms.Cursors.Hand
		};
		this.btnSplit1Point.FlatAppearance.BorderSize = 0;
		this.btnSplit1Point.Click += delegate
		{
			this.RunSplitProcess(false, true);
		};
		this.btnSplitSingle = new System.Windows.Forms.Button
		{
			Text = "✂️  TÁCH 2 ĐIỂM  (Pick 2 điểm cắt ngay)",
			Location = new System.Drawing.Point(16, 110),
			Size = new System.Drawing.Size(378, 36),
			BackColor = System.Drawing.Color.FromArgb(37, 99, 235),
			ForeColor = System.Drawing.Color.White,
			Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			FlatStyle = System.Windows.Forms.FlatStyle.Flat,
			Cursor = System.Windows.Forms.Cursors.Hand
		};
		this.btnSplitSingle.FlatAppearance.BorderSize = 0;
		this.btnSplitSingle.Click += delegate
		{
			this.RunSplitProcess(false, false);
		};
		this.btnSplitContinuous = new System.Windows.Forms.Button
		{
			Text = "⚡  TÁCH LIÊN TỤC  (Multi-Slabs Split)",
			Location = new System.Drawing.Point(16, 152),
			Size = new System.Drawing.Size(378, 34),
			BackColor = System.Drawing.Color.FromArgb(79, 70, 229),
			ForeColor = System.Drawing.Color.White,
			Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			FlatStyle = System.Windows.Forms.FlatStyle.Flat,
			Cursor = System.Windows.Forms.Cursors.Hand
		};
		this.btnSplitContinuous.FlatAppearance.BorderSize = 0;
		this.btnSplitContinuous.Click += delegate
		{
			this.RunSplitMultiProcess(true);
		};
		this.lblTip = new System.Windows.Forms.Label
		{
			Text = "💡 Pick các điểm cắt rồi click Chuột giữa (hoặc Enter) để tách sàn.",
			Location = new System.Drawing.Point(16, 196),
			AutoSize = true,
			Font = new System.Drawing.Font("Segoe UI", 7.8f, System.Drawing.FontStyle.Italic),
			ForeColor = System.Drawing.Color.FromArgb(100, 116, 139)
		};
		this.grpActions.Controls.Add(this.btnSplitMulti);
		this.grpActions.Controls.Add(this.btnSplit1Point);
		this.grpActions.Controls.Add(this.btnSplitSingle);
		this.grpActions.Controls.Add(this.btnSplitContinuous);
		this.grpActions.Controls.Add(this.lblTip);
		this.grpLog = new System.Windows.Forms.GroupBox
		{
			Text = " 📋 NHẬT KÝ XỬ LÝ ",
			Location = new System.Drawing.Point(14, 404),
			Size = new System.Drawing.Size(416, 215),
			Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
			ForeColor = System.Drawing.Color.FromArgb(30, 41, 59)
		};
		this.lstLog = new System.Windows.Forms.ListBox
		{
			Location = new System.Drawing.Point(14, 25),
			Size = new System.Drawing.Size(388, 145),
			Font = new System.Drawing.Font("Consolas", 8.25f, System.Drawing.FontStyle.Regular),
			BackColor = System.Drawing.Color.White,
			BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle,
			HorizontalScrollbar = true
		};
		this.btnClearLog = new System.Windows.Forms.Button
		{
			Text = "Xóa Log",
			Location = new System.Drawing.Point(320, 178),
			Size = new System.Drawing.Size(82, 26),
			Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Regular),
			BackColor = System.Drawing.Color.FromArgb(241, 245, 249),
			FlatStyle = System.Windows.Forms.FlatStyle.Flat,
			Cursor = System.Windows.Forms.Cursors.Hand
		};
		this.btnClearLog.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 213, 225);
		this.btnClearLog.Click += delegate
		{
			this.lstLog.Items.Clear();
		};
		this.grpLog.Controls.Add(this.lstLog);
		this.grpLog.Controls.Add(this.btnClearLog);
		base.Controls.Add(this.grpParams);
		base.Controls.Add(this.grpActions);
		base.Controls.Add(this.grpLog);
		this.Log("Split-Slab sẵn sàng. Chọn chế độ cắt phía trên.");
	}

	private void Log(string msg)
	{
		lstLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] {msg}");
		lstLog.SelectedIndex = lstLog.Items.Count - 1;
	}

	private void RunSplitProcess(bool continuous, bool onePointMode = false)
	{
		if (_model == null)
		{
			_model = new Model();
		}
		if (!_model.GetConnectionStatus())
		{
			MessageBox.Show("Chưa kết nối mô hình Tekla. Vui lòng mở model dự án trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		double gap = (double)numGap.Value;
		int selectedIndex = cboNewClass.SelectedIndex;
		Picker picker = new Picker();
		int num = 0;
		do
		{
			try
			{
				Log("👉 Vui lòng pick chọn tấm sàn (ContourPlate)...");
				ModelObject modelObject = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick sàn (ContourPlate) cần tách:");
				if (!(modelObject is ContourPlate contourPlate))
				{
					Log("❌ Đối tượng chọn không phải ContourPlate!");
					if (!continuous)
					{
						break;
					}
					continue;
				}
				Log($"Đã chọn sàn ID: {contourPlate.Identifier.ID} ({contourPlate.Profile.ProfileString})");

				global::Tekla.Structures.Geometry3d.Point point = null;
				global::Tekla.Structures.Geometry3d.Point point2 = null;

				if (onePointMode)
				{
					Log("👉 Pick 1 điểm trên mép sàn để cắt vuông góc...");
					global::Tekla.Structures.Geometry3d.Point pickPt = picker.PickPoint("Pick điểm cắt trên mép sàn:");
					string edgeInfo;
					if (!GetPerpendicularCutPointsFrom1Point(contourPlate, pickPt, out point, out point2, out edgeInfo))
					{
						Log("⚠ Không xác định được mép sàn gần điểm pick. Vui lòng thử lại.");
						if (!continuous) break;
						continue;
					}
					Log("📐 [1-Point Cut] Tự động cắt vuông góc: " + edgeInfo);
				}
				else
				{
					Log("👉 Pick điểm thứ 1 của đường cắt...");
					point = picker.PickPoint("Pick điểm thứ 1:");
					Log("👉 Pick điểm thứ 2 của đường cắt...");
					point2 = picker.PickPoint("Pick điểm thứ 2:");

					if (Distance(point, point2) < 1.0)
					{
						Log("⚠️ Hai điểm cắt quá gần nhau (< 1mm). Đã hủy.");
						if (!continuous)
						{
							break;
						}
						continue;
					}

					// Auto-Ortho: Straighten line if enabled
					if (chkAutoOrtho.Checked)
					{
						string alignReason;
						point2 = StraightenCutLine(contourPlate, point, point2, out alignReason);
						if (!string.IsNullOrEmpty(alignReason))
						{
							Log("🎯 [Auto-Snap] Đã nắn thẳng theo: " + alignReason);
						}
					}
				}

				ContourPlate newSlab = null;
				if (SplitContourPlate(contourPlate, point, point2, gap, selectedIndex, out newSlab))
				{
					_model.CommitChanges();
					num++;
					Log(string.Format("✅ TÁCH SÀN THÀNH CÔNG! (Sàn gốc ID: {0}, Sàn mới ID: {1})", contourPlate.Identifier.ID, (newSlab != null) ? newSlab.Identifier.ID.ToString() : "N/A"));
					if (chkSelectNew.Checked && newSlab != null)
					{
						try
						{
							ArrayList arrayList = new ArrayList();
							arrayList.Add(contourPlate);
							arrayList.Add(newSlab);
							global::Tekla.Structures.Model.UI.ModelObjectSelector modelObjectSelector = new global::Tekla.Structures.Model.UI.ModelObjectSelector();
							modelObjectSelector.Select(arrayList);
						}
						catch
						{
						}
					}
				}
				else
				{
					Log("⚠️ Đường cắt không đi xuyên qua chu vi tấm sàn.");
				}
				if (!continuous)
				{
					break;
				}
			}
			catch
			{
				Log("Thao tác kết thúc hoặc đã nhấn phím Esc.");
				break;
			}
		}
		while (continuous);
		if (num > 0)
		{
			Log($"Hoàn tất. Số lần tách thành công: {num}");
		}
	}

	private void RunSplitMultiProcess(bool continuous)
	{
		if (_model == null)
		{
			_model = new Model();
		}
		if (!_model.GetConnectionStatus())
		{
			MessageBox.Show("Chưa kết nối mô hình Tekla. Vui lòng mở model dự án trước!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		double gap = (double)numGap.Value;
		int selectedIndex = cboNewClass.SelectedIndex;
		Picker picker = new Picker();
		int num = 0;
		do
		{
			try
			{
				Log("👉 Vui lòng pick chọn tấm sàn (ContourPlate)...");
				ModelObject modelObject = picker.PickObject(Picker.PickObjectEnum.PICK_ONE_PART, "Pick sàn (ContourPlate) cần tách:");
				if (!(modelObject is ContourPlate contourPlate))
				{
					Log("❌ Đối tượng chọn không phải ContourPlate!");
					if (!continuous) break;
					continue;
				}
				Log($"Đã chọn sàn ID: {contourPlate.Identifier.ID} ({contourPlate.Profile.ProfileString})");

				Log("👉 Pick các điểm của đường cắt, sau đó nhấn CHUỘT GIỮA (hoặc Enter) để hoàn tất:");
				ArrayList rawPoints = picker.PickPoints(Picker.PickPointEnum.PICK_POLYGON, "Pick các điểm cắt, click chuột giữa (hoặc Enter) khi xong:");

				if (rawPoints == null || rawPoints.Count < 2)
				{
					Log("⚠️ Cần pick ít nhất 2 điểm để xác định đường cắt. Đã hủy.");
					if (!continuous) break;
					continue;
				}

				List<global::Tekla.Structures.Geometry3d.Point> cutPoints = new List<global::Tekla.Structures.Geometry3d.Point>();
				foreach (object obj in rawPoints)
				{
					if (obj is global::Tekla.Structures.Geometry3d.Point pt) cutPoints.Add(pt);
				}

				if (cutPoints.Count < 2)
				{
					Log("⚠️ Số điểm hợp lệ < 2. Đã hủy.");
					if (!continuous) break;
					continue;
				}

				// If Tekla PICK_POLYGON closed the polygon by adding start point at the end, remove it
				if (cutPoints.Count >= 3 && Distance(cutPoints[0], cutPoints[cutPoints.Count - 1]) < 15.0)
				{
					cutPoints.RemoveAt(cutPoints.Count - 1);
				}

				// Remove adjacent duplicate points
				for (int i = cutPoints.Count - 1; i >= 1; i--)
				{
					if (Distance(cutPoints[i], cutPoints[i - 1]) < 2.0)
					{
						cutPoints.RemoveAt(i);
					}
				}

				if (cutPoints.Count < 2)
				{
					Log("⚠️ Số điểm hợp lệ < 2. Đã hủy.");
					if (!continuous) break;
					continue;
				}

				Log($"-> Nhận diện {cutPoints.Count} điểm = {cutPoints.Count - 1} đường cắt độc lập.");

				List<ContourPlate> allResultSlabs = new List<ContourPlate>();

				if (cutPoints.Count == 2)
				{
					// Exactly 1 cut line (Point 1 -> Point 2)
					global::Tekla.Structures.Geometry3d.Point p1 = cutPoints[0];
					global::Tekla.Structures.Geometry3d.Point p2 = cutPoints[1];

					if (chkAutoOrtho.Checked)
					{
						string alignReason;
						p2 = StraightenCutLine(contourPlate, p1, p2, out alignReason);
						if (!string.IsNullOrEmpty(alignReason))
						{
							Log("🎯 [Auto-Snap] Đã nắn thẳng theo: " + alignReason);
						}
					}

					ContourPlate newSlab = null;
					if (SplitContourPlate(contourPlate, p1, p2, gap, selectedIndex, out newSlab))
					{
						_model.CommitChanges();
						num++;
						allResultSlabs.Add(contourPlate);
						if (newSlab != null) allResultSlabs.Add(newSlab);
						Log(string.Format("✅ TÁCH SÀN THÀNH CÔNG! (Sàn gốc ID: {0}, Sàn mới ID: {1})", contourPlate.Identifier.ID, (newSlab != null) ? newSlab.Identifier.ID.ToString() : "N/A"));
					}
					else
					{
						Log("⚠️ Đường cắt không đi xuyên qua chu vi tấm sàn.");
					}
				}
				else
				{
					if (AreIntermediatePointsInside(contourPlate, cutPoints))
					{
						// Polyline / V-notch / Ziczac cut (point 2 is inside slab) -> Splits into 2 slabs
						ContourPlate newSlab = null;
						if (SplitContourPlateByPolyline(contourPlate, cutPoints, gap, selectedIndex, out newSlab))
						{
							_model.CommitChanges();
							num++;
							allResultSlabs.Add(contourPlate);
							if (newSlab != null) allResultSlabs.Add(newSlab);
							Log(string.Format("✅ TÁCH SÀN ZICZAC THÀNH CÔNG! (Sàn gốc ID: {0}, Sàn mới ID: {1})", contourPlate.Identifier.ID, (newSlab != null) ? newSlab.Identifier.ID.ToString() : "N/A"));
						}
						else
						{
							Log("⚠️ Tách sàn ziczac không thành công. Hãy đảm bảo điểm đầu và cuối nằm trên mép sàn.");
						}
					}
					else
					{
						// Multiple cut lines: e.g. Point 1->2 is Line 1, Point 2->3 is Line 2 -> Splits into 3+ slabs
						List<ContourPlate> multiSlabs;
						if (SplitSlabByMultiSegments(contourPlate, cutPoints, gap, selectedIndex, out multiSlabs))
						{
							_model.CommitChanges();
							num++;
							allResultSlabs.AddRange(multiSlabs);
							Log($"✅ TÁCH ĐA ĐIỂM THÀNH CÔNG! Đã chia thành {multiSlabs.Count} tấm sàn:");
							for (int k = 0; k < multiSlabs.Count; k++)
							{
								Log($"   ➔ Sàn #{k + 1}: ID {multiSlabs[k].Identifier.ID} (Class {multiSlabs[k].Class})");
							}
						}
						else
						{
							Log("⚠️ Tách đa điểm không thành công. Hãy kiểm tra các đoạn cắt nối xuyên qua mép sàn.");
						}
					}
				}

				if (chkSelectNew.Checked && allResultSlabs.Count > 0)
				{
					try
					{
						ArrayList arrayList = new ArrayList();
						foreach (var slab in allResultSlabs) arrayList.Add(slab);
						global::Tekla.Structures.Model.UI.ModelObjectSelector modelObjectSelector = new global::Tekla.Structures.Model.UI.ModelObjectSelector();
						modelObjectSelector.Select(arrayList);
					}
					catch { }
				}

				if (!continuous) break;
			}
			catch
			{
				Log("Thao tác kết thúc hoặc đã nhấn phím Esc.");
				break;
			}
		}
		while (continuous);

		if (num > 0)
		{
			Log($"Hoàn tất. Số lần tách thành công: {num}");
		}
	}

	private bool SplitSlabByMultiSegments(
		ContourPlate initialSlab,
		List<global::Tekla.Structures.Geometry3d.Point> cutPoints,
		double gap,
		int selectedIndex,
		out List<ContourPlate> resultSlabs)
	{
		resultSlabs = new List<ContourPlate> { initialSlab };
		if (cutPoints == null || cutPoints.Count < 2) return false;

		string origClass = initialSlab.Class;
		int totalSegments = cutPoints.Count - 1;
		int successCuts = 0;

		for (int segIdx = 0; segIdx < totalSegments; segIdx++)
		{
			global::Tekla.Structures.Geometry3d.Point pA = cutPoints[segIdx];
			global::Tekla.Structures.Geometry3d.Point pB = cutPoints[segIdx + 1];

			Vector segVec = new Vector(pB.X - pA.X, pB.Y - pA.Y, pB.Z - pA.Z);
			if (segVec.GetLength() < 10.0) continue;

			// Find candidate slab containing this cut segment
			ContourPlate targetSlab = FindTargetSlabForSegment(resultSlabs, pA, pB);
			ContourPlate newSlab = null;
			bool splitOk = false;

			if (targetSlab != null)
			{
				if (SplitContourPlate(targetSlab, pA, pB, gap, selectedIndex, out newSlab))
				{
					splitOk = true;
				}
			}

			// Fallback: if targetSlab didn't split, try all other current slabs
			if (!splitOk)
			{
				foreach (var slab in resultSlabs.ToArray())
				{
					if (slab == targetSlab) continue;
					if (SplitContourPlate(slab, pA, pB, gap, selectedIndex, out newSlab))
					{
						targetSlab = slab;
						splitOk = true;
						break;
					}
				}
			}

			if (splitOk && newSlab != null)
			{
				ApplyClassToSlab(newSlab, origClass, selectedIndex, successCuts);
				newSlab.Modify();
				resultSlabs.Add(newSlab);
				_model.CommitChanges();
				successCuts++;
				Log($"-> Đoạn cắt #{segIdx + 1} ({pA.X:F0},{pA.Y:F0} ➔ {pB.X:F0},{pB.Y:F0}): Tách thành công! (Sàn mới ID: {newSlab.Identifier.ID})");
			}
			else
			{
				Log($"⚠️ Đoạn cắt #{segIdx + 1} ({pA.X:F0},{pA.Y:F0} ➔ {pB.X:F0},{pB.Y:F0}) không thể cắt qua sàn nào.");
			}
		}

		return successCuts > 0;
	}

	private bool AreIntermediatePointsInside(ContourPlate slab, List<global::Tekla.Structures.Geometry3d.Point> cutPoints)
	{
		if (cutPoints == null || cutPoints.Count <= 2) return false;
		List<global::Tekla.Structures.Geometry3d.Point> poly = GetSlabPolygon(slab);
		if (poly == null || poly.Count < 3) return false;

		for (int i = 1; i < cutPoints.Count - 1; i++)
		{
			global::Tekla.Structures.Geometry3d.Point pt = cutPoints[i];
			double distToBoundary = MinDistanceToPolygonBoundary(poly, pt);
			if (distToBoundary > 30.0 && IsPointInsideSlab(poly, pt))
			{
				return true;
			}
		}
		return false;
	}

	private double MinDistanceToPolygonBoundary(List<global::Tekla.Structures.Geometry3d.Point> poly, global::Tekla.Structures.Geometry3d.Point pt)
	{
		if (poly == null || poly.Count < 2) return double.MaxValue;
		double minDist = double.MaxValue;
		int n = poly.Count;
		for (int i = 0; i < n; i++)
		{
			global::Tekla.Structures.Geometry3d.Point a = poly[i];
			global::Tekla.Structures.Geometry3d.Point b = poly[(i + 1) % n];
			Vector ab = new Vector(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
			double len = ab.Normalize();
			if (len < 0.001)
			{
				double d0 = Distance(pt, a);
				if (d0 < minDist) minDist = d0;
				continue;
			}
			Vector ap = new Vector(pt.X - a.X, pt.Y - a.Y, pt.Z - a.Z);
			double t = ap.Dot(ab);
			if (t < 0.0) t = 0.0;
			if (t > len) t = len;
			global::Tekla.Structures.Geometry3d.Point proj = new global::Tekla.Structures.Geometry3d.Point(
				a.X + ab.X * t, a.Y + ab.Y * t, a.Z + ab.Z * t);
			double d = Distance(pt, proj);
			if (d < minDist) minDist = d;
		}
		return minDist;
	}

	private bool SplitContourPlateByPolyline(
		ContourPlate slab,
		List<global::Tekla.Structures.Geometry3d.Point> cutPoints,
		double gap,
		int selectedIndex,
		out ContourPlate newSlab)
	{
		newSlab = null;
		if (slab == null || cutPoints == null || cutPoints.Count < 2) return false;

		// Method 1: Try Tekla native Operation.Split with extended endpoints
		try
		{
			Polygon splitPoly = new Polygon();
			global::Tekla.Structures.Geometry3d.Point p0 = cutPoints[0];
			global::Tekla.Structures.Geometry3d.Point p1 = cutPoints[1];
			Vector vStart = new Vector(p0.X - p1.X, p0.Y - p1.Y, p0.Z - p1.Z);
			if (vStart.Normalize() > 0.001)
			{
				splitPoly.Points.Add(new global::Tekla.Structures.Geometry3d.Point(p0.X + vStart.X * 1000.0, p0.Y + vStart.Y * 1000.0, p0.Z + vStart.Z * 1000.0));
			}

			foreach (var pt in cutPoints)
			{
				splitPoly.Points.Add(pt);
			}

			int m = cutPoints.Count;
			global::Tekla.Structures.Geometry3d.Point pLast = cutPoints[m - 1];
			global::Tekla.Structures.Geometry3d.Point pPenultimate = cutPoints[m - 2];
			Vector vEnd = new Vector(pLast.X - pPenultimate.X, pLast.Y - pPenultimate.Y, pLast.Z - pPenultimate.Z);
			if (vEnd.Normalize() > 0.001)
			{
				splitPoly.Points.Add(new global::Tekla.Structures.Geometry3d.Point(pLast.X + vEnd.X * 1000.0, pLast.Y + vEnd.Y * 1000.0, pLast.Z + vEnd.Z * 1000.0));
			}

			newSlab = global::Tekla.Structures.Model.Operations.Operation.Split(slab, splitPoly);
			if (newSlab != null)
			{
				ApplyClassToSlab(newSlab, slab.Class, selectedIndex, 0);
				newSlab.Modify();
				_model.CommitChanges();
				Log(string.Format("✅ TÁCH SÀN ZICZAC THÀNH CÔNG! (Sàn gốc ID: {0}, Sàn mới ID: {1})", slab.Identifier.ID, newSlab.Identifier.ID));
				return true;
			}
		}
		catch { }

		// Method 2: Pure geometric boundary chain splitting
		return SplitContourPlateByGeometryChains(slab, cutPoints, gap, selectedIndex, out newSlab);
	}

	private bool SplitContourPlateByGeometryChains(
		ContourPlate slab,
		List<global::Tekla.Structures.Geometry3d.Point> cutPoints,
		double gap,
		int selectedIndex,
		out ContourPlate newSlab)
	{
		newSlab = null;
		ArrayList contourPoints = slab.Contour.ContourPoints;
		if (contourPoints == null || contourPoints.Count < 3) return false;

		List<global::Tekla.Structures.Geometry3d.Point> poly = new List<global::Tekla.Structures.Geometry3d.Point>();
		foreach (ContourPoint cp in contourPoints)
		{
			poly.Add(new global::Tekla.Structures.Geometry3d.Point(cp.X, cp.Y, cp.Z));
		}

		int n = poly.Count;
		global::Tekla.Structures.Geometry3d.Point pStart = cutPoints[0];
		global::Tekla.Structures.Geometry3d.Point pEnd = cutPoints[cutPoints.Count - 1];

		int edgeStartIdx = -1;
		global::Tekla.Structures.Geometry3d.Point qStart = ProjectPointToClosestEdge(poly, pStart, out edgeStartIdx);

		int edgeEndIdx = -1;
		global::Tekla.Structures.Geometry3d.Point qEnd = ProjectPointToClosestEdge(poly, pEnd, out edgeEndIdx);

		if (edgeStartIdx < 0 || edgeEndIdx < 0 || edgeStartIdx == edgeEndIdx)
		{
			return false;
		}

		// Build Loop 1: from qStart along poly boundary to qEnd, then back through cutPoints reverse
		List<global::Tekla.Structures.Geometry3d.Point> loop1 = new List<global::Tekla.Structures.Geometry3d.Point>();
		loop1.Add(qStart);
		int curr = (edgeStartIdx + 1) % n;
		while (curr != (edgeEndIdx + 1) % n)
		{
			loop1.Add(poly[curr]);
			curr = (curr + 1) % n;
		}
		loop1.Add(qEnd);
		for (int k = cutPoints.Count - 2; k >= 1; k--)
		{
			loop1.Add(cutPoints[k]);
		}

		// Build Loop 2: from qEnd along poly boundary to qStart, then through cutPoints forward
		List<global::Tekla.Structures.Geometry3d.Point> loop2 = new List<global::Tekla.Structures.Geometry3d.Point>();
		loop2.Add(qEnd);
		curr = (edgeEndIdx + 1) % n;
		while (curr != (edgeStartIdx + 1) % n)
		{
			loop2.Add(poly[curr]);
			curr = (curr + 1) % n;
		}
		loop2.Add(qStart);
		for (int k = 1; k <= cutPoints.Count - 2; k++)
		{
			loop2.Add(cutPoints[k]);
		}

		loop1 = RemoveCollinearPoints(loop1);
		loop2 = RemoveCollinearPoints(loop2);

		if (loop1.Count < 3 || loop2.Count < 3) return false;

		Contour c1 = new Contour();
		foreach (var pt in loop1)
		{
			Chamfer ch = FindOriginalChamfer(contourPoints, pt);
			c1.AddContourPoint(new ContourPoint(pt, ch));
		}
		slab.Contour = c1;
		slab.Modify();

		newSlab = new ContourPlate();
		newSlab.Profile.ProfileString = slab.Profile.ProfileString;
		newSlab.Material.MaterialString = slab.Material.MaterialString;
		newSlab.Finish = slab.Finish;
		newSlab.Position.Plane = slab.Position.Plane;
		newSlab.Position.Depth = slab.Position.Depth;
		newSlab.Position.PlaneOffset = slab.Position.PlaneOffset;
		newSlab.Position.DepthOffset = slab.Position.DepthOffset;
		newSlab.Name = slab.Name;
		ApplyClassToSlab(newSlab, slab.Class, selectedIndex, 0);

		Contour c2 = new Contour();
		foreach (var pt in loop2)
		{
			Chamfer ch = FindOriginalChamfer(contourPoints, pt);
			c2.AddContourPoint(new ContourPoint(pt, ch));
		}
		newSlab.Contour = c2;
		newSlab.Insert();

		_model.CommitChanges();
		Log($"✅ TÁCH SÀN ZICZAC (HÌNH HỌC) THÀNH CÔNG! Sàn 1: {loop1.Count} đỉnh | Sàn 2: {loop2.Count} đỉnh");
		return true;
	}

	private global::Tekla.Structures.Geometry3d.Point ProjectPointToClosestEdge(
		List<global::Tekla.Structures.Geometry3d.Point> poly,
		global::Tekla.Structures.Geometry3d.Point p,
		out int bestEdgeIdx)
	{
		bestEdgeIdx = -1;
		double minDist = double.MaxValue;
		global::Tekla.Structures.Geometry3d.Point bestProj = p;
		int n = poly.Count;

		for (int i = 0; i < n; i++)
		{
			global::Tekla.Structures.Geometry3d.Point a = poly[i];
			global::Tekla.Structures.Geometry3d.Point b = poly[(i + 1) % n];
			Vector ab = new Vector(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
			double len = ab.Normalize();
			if (len < 0.001) continue;

			Vector ap = new Vector(p.X - a.X, p.Y - a.Y, p.Z - a.Z);
			double t = ap.Dot(ab);
			if (t < 0.0) t = 0.0;
			if (t > len) t = len;

			global::Tekla.Structures.Geometry3d.Point proj = new global::Tekla.Structures.Geometry3d.Point(
				a.X + ab.X * t, a.Y + ab.Y * t, a.Z + ab.Z * t);
			double d = Distance(p, proj);
			if (d < minDist)
			{
				minDist = d;
				bestProj = proj;
				bestEdgeIdx = i;
			}
		}

		return bestProj;
	}

	private ContourPlate FindTargetSlabForSegment(
		List<ContourPlate> slabs,
		global::Tekla.Structures.Geometry3d.Point pA,
		global::Tekla.Structures.Geometry3d.Point pB)
	{
		if (slabs == null || slabs.Count == 0) return null;
		if (slabs.Count == 1) return slabs[0];

		// Sample multiple points along segment (t = 0.5, 0.3, 0.7, 0.2, 0.8)
		double[] sampleT = new double[] { 0.5, 0.3, 0.7, 0.2, 0.8 };
		ContourPlate bestSlab = null;
		int maxHits = -1;

		foreach (var slab in slabs)
		{
			List<global::Tekla.Structures.Geometry3d.Point> poly = GetSlabPolygon(slab);
			if (poly == null || poly.Count < 3) continue;

			int hits = 0;
			for (int i = 0; i < sampleT.Length; i++)
			{
				double t = sampleT[i];
				global::Tekla.Structures.Geometry3d.Point pt = new global::Tekla.Structures.Geometry3d.Point(
					pA.X + t * (pB.X - pA.X),
					pA.Y + t * (pB.Y - pA.Y),
					pA.Z + t * (pB.Z - pA.Z)
				);
				if (IsPointInsideSlab(poly, pt))
				{
					hits++;
				}
			}

			if (hits > maxHits)
			{
				maxHits = hits;
				bestSlab = slab;
			}
		}

		if (maxHits > 0) return bestSlab;

		// Fallback: return the latest created slab
		return slabs[slabs.Count - 1];
	}

	private List<global::Tekla.Structures.Geometry3d.Point> GetSlabPolygon(ContourPlate slab)
	{
		if (slab == null || slab.Contour == null || slab.Contour.ContourPoints == null) return null;
		List<global::Tekla.Structures.Geometry3d.Point> list = new List<global::Tekla.Structures.Geometry3d.Point>();
		foreach (ContourPoint cp in slab.Contour.ContourPoints)
		{
			list.Add(new global::Tekla.Structures.Geometry3d.Point(cp.X, cp.Y, cp.Z));
		}
		return list;
	}

	private bool IsPointInsideSlab(List<global::Tekla.Structures.Geometry3d.Point> poly, global::Tekla.Structures.Geometry3d.Point pt)
	{
		if (poly == null || poly.Count < 3) return false;

		// Find polygon normal to project to 2D
		Vector normal = GetPolygonNormal(poly);

		// Project to dominant 2D plane
		double absX = Math.Abs(normal.X);
		double absY = Math.Abs(normal.Y);
		double absZ = Math.Abs(normal.Z);

		int count = poly.Count;
		bool inside = false;

		if (absZ >= absX && absZ >= absY)
		{
			// Project to XY plane
			for (int i = 0, j = count - 1; i < count; j = i++)
			{
				double xi = poly[i].X, yi = poly[i].Y;
				double xj = poly[j].X, yj = poly[j].Y;
				if (((yi > pt.Y) != (yj > pt.Y)) &&
					(pt.X < (xj - xi) * (pt.Y - yi) / (yj - yi) + xi))
				{
					inside = !inside;
				}
			}
		}
		else if (absX >= absY && absX >= absZ)
		{
			// Project to YZ plane
			for (int i = 0, j = count - 1; i < count; j = i++)
			{
				double yi = poly[i].Y, zi = poly[i].Z;
				double yj = poly[j].Y, zj = poly[j].Z;
				if (((zi > pt.Z) != (zj > pt.Z)) &&
					(pt.Y < (yj - yi) * (pt.Z - zi) / (zj - zi) + yi))
				{
					inside = !inside;
				}
			}
		}
		else
		{
			// Project to XZ plane
			for (int i = 0, j = count - 1; i < count; j = i++)
			{
				double xi = poly[i].X, zi = poly[i].Z;
				double xj = poly[j].X, zj = poly[j].Z;
				if (((zi > pt.Z) != (zj > pt.Z)) &&
					(pt.X < (xj - xi) * (pt.Z - zi) / (zj - zi) + xi))
				{
					inside = !inside;
				}
			}
		}

		return inside;
	}

	private Vector GetPolygonNormal(List<global::Tekla.Structures.Geometry3d.Point> poly)
	{
		if (poly == null || poly.Count < 3) return new Vector(0, 0, 1);
		for (int i = 0; i < poly.Count - 2; i++)
		{
			Vector vA = new Vector(poly[i + 1].X - poly[i].X, poly[i + 1].Y - poly[i].Y, poly[i + 1].Z - poly[i].Z);
			Vector vB = new Vector(poly[i + 2].X - poly[i].X, poly[i + 2].Y - poly[i].Y, poly[i + 2].Z - poly[i].Z);
			Vector cross = vA.Cross(vB);
			if (cross.GetLength() > 0.0001)
			{
				cross.Normalize();
				return cross;
			}
		}
		return new Vector(0, 0, 1);
	}

	private void ApplyClassToSlab(ContourPlate newSlab, string origClass, int classOption, int stepIndex = 0)
	{
		switch (classOption)
		{
			case 1:
				if (int.TryParse(origClass, out var res))
					newSlab.Class = (res + 1 + stepIndex).ToString();
				else
					newSlab.Class = origClass;
				break;
			case 2: newSlab.Class = "6"; break;
			case 3: newSlab.Class = "3"; break;
			case 4: newSlab.Class = "1"; break;
			default: newSlab.Class = origClass; break;
		}
	}

	private global::Tekla.Structures.Geometry3d.Point StraightenCutLine(ContourPlate slab, global::Tekla.Structures.Geometry3d.Point p1, global::Tekla.Structures.Geometry3d.Point p2, out string alignReason)
	{
		alignReason = "";
		Vector rawVec = new Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
		double len = rawVec.GetLength();
		if (len < 1.0) return p2;

		Vector uRaw = new Vector(rawVec);
		uRaw.Normalize();

		ArrayList pts = slab.Contour.ContourPoints;
		if (pts == null || pts.Count < 3) return p2;

		List<global::Tekla.Structures.Geometry3d.Point> poly = new List<global::Tekla.Structures.Geometry3d.Point>();
		foreach (ContourPoint cp in pts) poly.Add(new global::Tekla.Structures.Geometry3d.Point(cp.X, cp.Y, cp.Z));

		Vector vA = new Vector(poly[1].X - poly[0].X, poly[1].Y - poly[0].Y, poly[1].Z - poly[0].Z);
		Vector vB = new Vector(poly[2].X - poly[0].X, poly[2].Y - poly[0].Y, poly[2].Z - poly[0].Z);
		Vector nSlab = vA.Cross(vB);
		if (nSlab.GetLength() < 0.0001) nSlab = new Vector(0, 0, 1);
		nSlab.Normalize();

		var candidates = new List<Tuple<Vector, string>>();
		candidates.Add(Tuple.Create(new Vector(1, 0, 0), "Trục X"));
		candidates.Add(Tuple.Create(new Vector(0, 1, 0), "Trục Y"));

		for (int i = 0; i < poly.Count; i++)
		{
			global::Tekla.Structures.Geometry3d.Point pt1 = poly[i];
			global::Tekla.Structures.Geometry3d.Point pt2 = poly[(i + 1) % poly.Count];
			Vector edgeVec = new Vector(pt2.X - pt1.X, pt2.Y - pt1.Y, pt2.Z - pt1.Z);
			if (edgeVec.Normalize() > 0.001)
			{
				candidates.Add(Tuple.Create(new Vector(edgeVec), $"Song song Cạnh #{i + 1}"));
				Vector perpVec = edgeVec.Cross(nSlab);
				if (perpVec.Normalize() > 0.001)
				{
					candidates.Add(Tuple.Create(new Vector(perpVec), $"Vuông góc Cạnh #{i + 1}"));
				}
			}
		}

		double maxDot = -1.0;
		Vector bestDir = null;
		string bestName = "";

		foreach (var cand in candidates)
		{
			Vector d = cand.Item1;
			double dot = uRaw.Dot(d);
			double absDot = Math.Abs(dot);
			if (absDot > maxDot)
			{
				maxDot = absDot;
				bestDir = dot >= 0 ? d : new Vector(0.0 - d.X, 0.0 - d.Y, 0.0 - d.Z);
				bestName = cand.Item2;
			}
		}

		if (bestDir != null && maxDot > 0.7071)
		{
			double angleDeg = Math.Acos(Math.Min(1.0, maxDot)) * (180.0 / Math.PI);
			alignReason = $"{bestName} (Lệch nắn: {angleDeg:F1}°)";
			return new global::Tekla.Structures.Geometry3d.Point(p1.X + bestDir.X * len, p1.Y + bestDir.Y * len, p1.Z + bestDir.Z * len);
		}

		return p2;
	}

	private bool GetPerpendicularCutPointsFrom1Point(ContourPlate slab, global::Tekla.Structures.Geometry3d.Point pickPt, out global::Tekla.Structures.Geometry3d.Point p1, out global::Tekla.Structures.Geometry3d.Point p2, out string edgeInfo)
	{
		p1 = null;
		p2 = null;
		edgeInfo = "";

		ArrayList pts = slab.Contour.ContourPoints;
		if (pts == null || pts.Count < 3) return false;

		List<global::Tekla.Structures.Geometry3d.Point> poly = new List<global::Tekla.Structures.Geometry3d.Point>();
		foreach (ContourPoint cp in pts) poly.Add(new global::Tekla.Structures.Geometry3d.Point(cp.X, cp.Y, cp.Z));

		Vector vA = new Vector(poly[1].X - poly[0].X, poly[1].Y - poly[0].Y, poly[1].Z - poly[0].Z);
		Vector vB = new Vector(poly[2].X - poly[0].X, poly[2].Y - poly[0].Y, poly[2].Z - poly[0].Z);
		Vector nSlab = vA.Cross(vB);
		if (nSlab.GetLength() < 0.0001) nSlab = new Vector(0, 0, 1);
		nSlab.Normalize();

		double minDistance = double.MaxValue;
		Vector bestEdgeDir = null;
		global::Tekla.Structures.Geometry3d.Point bestProjPt = null;
		int bestEdgeIdx = 0;

		for (int i = 0; i < poly.Count; i++)
		{
			global::Tekla.Structures.Geometry3d.Point eA = poly[i];
			global::Tekla.Structures.Geometry3d.Point eB = poly[(i + 1) % poly.Count];
			Vector edgeVec = new Vector(eB.X - eA.X, eB.Y - eA.Y, eB.Z - eA.Z);
			double edgeLen = edgeVec.Normalize();
			if (edgeLen < 1.0) continue;

			Vector toPt = new Vector(pickPt.X - eA.X, pickPt.Y - eA.Y, pickPt.Z - eA.Z);
			double t = toPt.Dot(edgeVec);
			global::Tekla.Structures.Geometry3d.Point proj = new global::Tekla.Structures.Geometry3d.Point(eA.X + edgeVec.X * t, eA.Y + edgeVec.Y * t, eA.Z + edgeVec.Z * t);

			double dist = Distance(pickPt, proj);
			if (dist < minDistance)
			{
				minDistance = dist;
				bestEdgeDir = edgeVec;
				bestProjPt = proj;
				bestEdgeIdx = i + 1;
			}
		}

		if (bestEdgeDir == null) return false;

		Vector cutDir = bestEdgeDir.Cross(nSlab);
		cutDir.Normalize();

		global::Tekla.Structures.Geometry3d.Point center = (minDistance < 600.0 && bestProjPt != null) ? bestProjPt : pickPt;
		double ext = 15000.0;
		p1 = new global::Tekla.Structures.Geometry3d.Point(center.X - cutDir.X * ext, center.Y - cutDir.Y * ext, center.Z - cutDir.Z * ext);
		p2 = new global::Tekla.Structures.Geometry3d.Point(center.X + cutDir.X * ext, center.Y + cutDir.Y * ext, center.Z + cutDir.Z * ext);
		edgeInfo = $"Vuông góc Cạnh #{bestEdgeIdx}";
		return true;
	}

	private bool SplitContourPlate(ContourPlate slab, global::Tekla.Structures.Geometry3d.Point p1, global::Tekla.Structures.Geometry3d.Point p2, double gap, int classOption, out ContourPlate newSlab)
	{
		newSlab = null;
		ArrayList contourPoints = slab.Contour.ContourPoints;
		if (contourPoints == null || contourPoints.Count < 3)
		{
			return false;
		}
		List<global::Tekla.Structures.Geometry3d.Point> list = new List<global::Tekla.Structures.Geometry3d.Point>();
		foreach (ContourPoint item in contourPoints)
		{
			list.Add(new global::Tekla.Structures.Geometry3d.Point(item.X, item.Y, item.Z));
		}
		Vector vector3 = GetPolygonNormal(list);
		Vector vector4 = new Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
		vector4.Normalize();
		Vector vector5 = vector4.Cross(vector3);
		vector5.Normalize();
		double offset = gap / 2.0;
		List<global::Tekla.Structures.Geometry3d.Point> list2 = ClipPolygonByPlane(list, p1, vector5, offset);
		Vector normal = new Vector(0.0 - vector5.X, 0.0 - vector5.Y, 0.0 - vector5.Z);
		List<global::Tekla.Structures.Geometry3d.Point> list3 = ClipPolygonByPlane(list, p1, normal, offset);

		list2 = RemoveCollinearPoints(list2);
		list3 = RemoveCollinearPoints(list3);

		if (list2.Count < 3 || list3.Count < 3)
		{
			return false;
		}
		Contour contour = new Contour();
		foreach (global::Tekla.Structures.Geometry3d.Point item2 in list2)
		{
			Chamfer ch = FindOriginalChamfer(contourPoints, item2);
			contour.AddContourPoint(new ContourPoint(item2, ch));
		}
		slab.Contour = contour;
		slab.Modify();
		newSlab = new ContourPlate();
		newSlab.Profile.ProfileString = slab.Profile.ProfileString;
		newSlab.Material.MaterialString = slab.Material.MaterialString;
		newSlab.Finish = slab.Finish;
		newSlab.Position.Plane = slab.Position.Plane;
		newSlab.Position.Depth = slab.Position.Depth;
		newSlab.Position.PlaneOffset = slab.Position.PlaneOffset;
		newSlab.Position.DepthOffset = slab.Position.DepthOffset;
		newSlab.Name = slab.Name;
		string text = slab.Class;
		switch (classOption)
		{
		case 1:
		{
			if (int.TryParse(text, out var result))
			{
				newSlab.Class = (result + 1).ToString();
			}
			else
			{
				newSlab.Class = text;
			}
			break;
		}
		case 2:
			newSlab.Class = "6";
			break;
		case 3:
			newSlab.Class = "3";
			break;
		case 4:
			newSlab.Class = "1";
			break;
		default:
			newSlab.Class = text;
			break;
		}
		Contour contour2 = new Contour();
		foreach (global::Tekla.Structures.Geometry3d.Point item3 in list3)
		{
			Chamfer ch = FindOriginalChamfer(contourPoints, item3);
			contour2.AddContourPoint(new ContourPoint(item3, ch));
		}
		newSlab.Contour = contour2;
		newSlab.Insert();
		Log($"-> Sàn 1: {list2.Count} đỉnh | Sàn 2: {list3.Count} đỉnh");
		return true;
	}

	private Chamfer FindOriginalChamfer(ArrayList origPoints, global::Tekla.Structures.Geometry3d.Point pt)
	{
		if (origPoints == null) return null;
		foreach (object obj in origPoints)
		{
			if (obj is ContourPoint cp && cp.Chamfer != null && cp.Chamfer.Type != Chamfer.ChamferTypeEnum.CHAMFER_NONE)
			{
				double d = Distance(new global::Tekla.Structures.Geometry3d.Point(cp.X, cp.Y, cp.Z), pt);
				if (d < 1.0) return cp.Chamfer;
			}
		}
		return null;
	}

	private List<global::Tekla.Structures.Geometry3d.Point> RemoveCollinearPoints(List<global::Tekla.Structures.Geometry3d.Point> pts)
	{
		if (pts == null || pts.Count < 3) return pts;

		// Pass 1: Remove adjacent duplicate points safely (keep exactly one copy)
		List<global::Tekla.Structures.Geometry3d.Point> uniquePts = new List<global::Tekla.Structures.Geometry3d.Point>();
		for (int i = 0; i < pts.Count; i++)
		{
			global::Tekla.Structures.Geometry3d.Point curr = pts[i];
			if (uniquePts.Count == 0 || Distance(uniquePts[uniquePts.Count - 1], curr) >= 1.0)
			{
				uniquePts.Add(curr);
			}
		}
		if (uniquePts.Count >= 3 && Distance(uniquePts[0], uniquePts[uniquePts.Count - 1]) < 1.0)
		{
			uniquePts.RemoveAt(uniquePts.Count - 1);
		}

		if (uniquePts.Count < 4) return uniquePts.Count >= 3 ? uniquePts : pts;

		// Pass 2: Remove redundant collinear points along straight lines
		List<global::Tekla.Structures.Geometry3d.Point> result = new List<global::Tekla.Structures.Geometry3d.Point>();
		int n = uniquePts.Count;
		for (int i = 0; i < n; i++)
		{
			global::Tekla.Structures.Geometry3d.Point prev = uniquePts[(i + n - 1) % n];
			global::Tekla.Structures.Geometry3d.Point curr = uniquePts[i];
			global::Tekla.Structures.Geometry3d.Point next = uniquePts[(i + 1) % n];
			Vector v1 = new Vector(curr.X - prev.X, curr.Y - prev.Y, curr.Z - prev.Z);
			Vector v2 = new Vector(next.X - curr.X, next.Y - curr.Y, next.Z - curr.Z);
			if (v1.Normalize() < 0.001 || v2.Normalize() < 0.001) continue;
			Vector cross = v1.Cross(v2);
			if (cross.GetLength() < 0.001 && v1.Dot(v2) > 0.999)
			{
				continue;
			}
			result.Add(curr);
		}
		return result.Count >= 3 ? result : uniquePts;
	}

	private List<global::Tekla.Structures.Geometry3d.Point> ClipPolygonByPlane(List<global::Tekla.Structures.Geometry3d.Point> poly, global::Tekla.Structures.Geometry3d.Point origin, Vector normal, double offset)
	{
		List<global::Tekla.Structures.Geometry3d.Point> list = new List<global::Tekla.Structures.Geometry3d.Point>();
		int count = poly.Count;
		for (int i = 0; i < count; i++)
		{
			global::Tekla.Structures.Geometry3d.Point point = poly[i];
			global::Tekla.Structures.Geometry3d.Point point2 = poly[(i + count - 1) % count];
			double num = DistanceToPlane(point, origin, normal) - offset;
			double num2 = DistanceToPlane(point2, origin, normal) - offset;
			if (num >= 0.0)
			{
				if (num2 < 0.0)
				{
					global::Tekla.Structures.Geometry3d.Point item = LinePlaneIntersection(point2, point, origin, normal, offset);
					list.Add(item);
				}
				list.Add(point);
			}
			else if (num2 >= 0.0)
			{
				global::Tekla.Structures.Geometry3d.Point item = LinePlaneIntersection(point2, point, origin, normal, offset);
				list.Add(item);
			}
		}
		return list;
	}

	private double DistanceToPlane(global::Tekla.Structures.Geometry3d.Point pt, global::Tekla.Structures.Geometry3d.Point origin, Vector normal)
	{
		return (pt.X - origin.X) * normal.X + (pt.Y - origin.Y) * normal.Y + (pt.Z - origin.Z) * normal.Z;
	}

	private global::Tekla.Structures.Geometry3d.Point LinePlaneIntersection(global::Tekla.Structures.Geometry3d.Point p1, global::Tekla.Structures.Geometry3d.Point p2, global::Tekla.Structures.Geometry3d.Point origin, Vector normal, double offset)
	{
		Vector vector = new Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
		double num = vector.X * normal.X + vector.Y * normal.Y + vector.Z * normal.Z;
		if (Math.Abs(num) < 1E-07)
		{
			return p1;
		}
		double num2 = (offset - DistanceToPlane(p1, origin, normal)) / num;
		return new global::Tekla.Structures.Geometry3d.Point(p1.X + num2 * vector.X, p1.Y + num2 * vector.Y, p1.Z + num2 * vector.Z);
	}

	private double Distance(global::Tekla.Structures.Geometry3d.Point a, global::Tekla.Structures.Geometry3d.Point b)
	{
		double num = a.X - b.X;
		double num2 = a.Y - b.Y;
		double num3 = a.Z - b.Z;
		return Math.Sqrt(num * num + num2 * num2 + num3 * num3);
	}
}
