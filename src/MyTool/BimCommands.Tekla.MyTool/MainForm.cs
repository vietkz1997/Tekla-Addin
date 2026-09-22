using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Tekla.Structures.Model;

namespace BimCommands.Tekla.MyTool;

public class MainForm : Form
{
	private Model _model;

	private SplitSlabDialog _splitSlabDlg;

	private string _appDir = "D:\\Tekla_\\v20\\application";

	[STAThread]
	public static void Main(string[] args)
	{
		if (args != null && args.Length > 0)
		{
			string arg = args[0].ToLowerInvariant();
			if (arg.Contains("clash"))
			{
				LaunchClashCheckApp();
				return;
			}
			if (arg.Contains("error") || arg.Contains("rebar"))
			{
				LaunchRebarErrorApp();
				return;
			}
		}
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		Application.Run(new MainForm());
	}

	public MainForm()
	{
		InitializeToolbar();
		ConnectTekla();
	}

	private void InitializeToolbar()
	{
		Text = "My-tool";
		base.Size = new Size(1115, 116);
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = true;
		base.TopMost = true;
		base.StartPosition = FormStartPosition.CenterScreen;
		BackColor = Color.FromArgb(240, 242, 245);
		Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
		int num = 6;
		int num2 = 104;
		int h = 34;
		int num3 = 5;
		int num4 = 6;
		Button button = CreateToolButton("✂\ufe0f Split-Slab", num4, num, num2, h, Color.FromArgb(220, 252, 231), Color.FromArgb(22, 101, 52));
		button.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
		button.Click += delegate
		{
			OpenSplitSlabDialog();
		};
		base.Controls.Add(button);
		num4 += num2 + num3;
		Button button2 = CreateToolButton("⚠\ufe0f Overlap", num4, num, num2, h, Color.FromArgb(254, 243, 199), Color.FromArgb(146, 64, 14));
		button2.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
		button2.Click += delegate
		{
			LaunchOverlapChecker();
		};
		base.Controls.Add(button2);
		num4 += num2 + num3;
		Button buttonRebarErr = CreateToolButton("❌ Rebar-Error", num4, num, num2, h, Color.FromArgb(254, 226, 226), Color.FromArgb(185, 28, 28));
		buttonRebarErr.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
		buttonRebarErr.Click += delegate
		{
			LaunchRebarErrorApp();
		};
		base.Controls.Add(buttonRebarErr);
		num4 += num2 + num3;
		base.Controls.Add(CreateAppButton("\ud83c\udfe2 Slab Deck", "AppSlabDeck.exe", num4, num, num2, h));
		num4 += num2 + num3;
		base.Controls.Add(CreateAppButton("\ud83d\udce6 Formwork", "AppFormwork.exe", num4, num, num2, h));
		num4 += num2 + num3;
		base.Controls.Add(CreateAppButton("\ud83d\udcdd Report UDA", "AppGetUDA.exe", num4, num, num2, h));
		num4 += num2 + num3;
		base.Controls.Add(CreateAppButton("\ud83d\udd29 Coupler", "AppCoupler.exe", num4, num, num2, h));
		num4 += num2 + num3;
		base.Controls.Add(CreateAppButton("\ud83d\udd04 To IFC", "AppConvertToIFC.exe", num4, num, num2, h));
		num4 += num2 + num3;
		base.Controls.Add(CreateAppButton("\ud83d\udd32 Sel Filter", "AppSelectionFillter.exe", num4, num, num2, h));
		num4 += num2 + num3;
		base.Controls.Add(CreateAppButton("\ud83d\udccb Check List", "AppCheckList.exe", num4, num, num2, h));
		int num5 = 44;
		num4 = 6;
		base.Controls.Add(CreateAppButton("↔\ufe0f Split Rebar", "AppSplitRebar.exe", num4, num5, num2, h));
		num4 += num2 + num3;
		base.Controls.Add(CreateAppButton("➗ Divide Rebar", "AppDivideRebar.exe", num4, num5, num2, h));
		num4 += num2 + num3;
		base.Controls.Add(CreateAppButton("\ud83e\ude9c Stair Rebar", "StairRebar.exe", num4, num5, num2, h));
		num4 += num2 + num3;
		base.Controls.Add(CreateAppButton("\ud83e\uddf1 Rebar Wall", "AppRebarWall.exe", num4, num5, num2, h));
		num4 += num2 + num3;
		base.Controls.Add(CreateAppButton("\ud83c\udfdb\ufe0f Rebar Column", "AppRebarHoopColumn.exe", num4, num5, num2, h));
		num4 += num2 + num3;
		base.Controls.Add(CreateAppButton("\ud83d\udd0d Model Filter", "AppBimFilter.exe", num4, num5, num2, h));
		num4 += num2 + num3;
		base.Controls.Add(CreateAppButton("\ud83d\udcd0 RoundUp", "AppRoundUp.exe", num4, num5, num2, h));
		num4 += num2 + num3;
		Button buttonClash = CreateToolButton("💥 Clash-Check", num4, num5, num2, h, Color.FromArgb(224, 231, 255), Color.FromArgb(67, 56, 202));
		buttonClash.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
		buttonClash.Click += delegate
		{
			LaunchClashCheckApp();
		};
		base.Controls.Add(buttonClash);
		num4 += num2 + num3;
		base.Controls.Add(CreateAppButton("⚙\ufe0f Preference", "AppPreference.exe", num4, num5, num2, h));
	}

	private Button CreateToolButton(string text, int x, int y, int w, int h, Color bg, Color fg)
	{
		Button button = new Button();
		button.Text = text;
		button.Location = new Point(x, y);
		button.Size = new Size(w, h);
		button.BackColor = bg;
		button.ForeColor = fg;
		button.FlatStyle = FlatStyle.Flat;
		button.TextAlign = ContentAlignment.MiddleCenter;
		button.Cursor = Cursors.Hand;
		Button button2 = button;
		button2.FlatAppearance.BorderColor = Color.FromArgb(180, 190, 205);
		button2.FlatAppearance.BorderSize = 1;
		return button2;
	}

	private Button CreateAppButton(string text, string exeName, int x, int y, int w, int h)
	{
		Button button = CreateToolButton(text, x, y, w, h, Color.White, Color.FromArgb(30, 41, 59));
		button.Click += delegate
		{
			LaunchExternalApp(exeName);
		};
		return button;
	}

	private void LaunchExternalApp(string exeName)
	{
		string text = Path.Combine(_appDir, exeName);
		if (File.Exists(text))
		{
			try
			{
				Process.Start(text);
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Lỗi khởi chạy: " + ex.Message);
				return;
			}
		}
		MessageBox.Show("Không tìm thấy công cụ: " + text, "Thông báo My-tool", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	private void LaunchOverlapChecker()
	{
		string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Overlap.exe");
		if (!File.Exists(text))
		{
			text = "D:\\tekla\\My-tool\\Overlap.exe";
		}
		if (File.Exists(text))
		{
			try
			{
				Process.Start(text);
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Lỗi: " + ex.Message);
				return;
			}
		}
		MessageBox.Show("Không tìm thấy: " + text, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	private static void LaunchClashCheckApp()
	{
		string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Clash-check.exe");
		if (!File.Exists(text))
		{
			text = "D:\\tekla\\My-tool\\Clash-check.exe";
		}
		if (!File.Exists(text))
		{
			text = "D:\\Tekla_\\My-tool\\Clash-check.exe";
		}
		if (!File.Exists(text))
		{
			text = "D:\\Tekla_\\v20\\application\\AppClashCheck.exe";
		}
		if (File.Exists(text))
		{
			try
			{
				Process.Start(text);
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Lỗi khởi chạy Clash-check: " + ex.Message);
				return;
			}
		}
		MessageBox.Show("Không tìm thấy công cụ: " + text, "Thông báo My-tool", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	private static void LaunchRebarErrorApp()
	{
		string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Rebar-error.exe");
		if (!File.Exists(text))
		{
			text = "D:\\tekla\\My-tool\\Rebar-error.exe";
		}
		if (!File.Exists(text))
		{
			text = "D:\\Tekla_\\My-tool\\Rebar-error.exe";
		}
		if (File.Exists(text))
		{
			try
			{
				Process.Start(text);
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Lỗi khởi chạy Rebar-error: " + ex.Message);
				return;
			}
		}
		MessageBox.Show("Không tìm thấy công cụ: " + text, "Thông báo My-tool", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	private void ConnectTekla()
	{
		try
		{
			_model = new Model();
			if (_model.GetConnectionStatus())
			{
				string modelName = _model.GetInfo().ModelName;
				Text = "My-tool (" + modelName + ")";
			}
			else
			{
				Text = "My-tool (Chưa kết nối Tekla)";
			}
		}
		catch
		{
			Text = "My-tool";
		}
	}

	private void OpenSplitSlabDialog()
	{
		if (_splitSlabDlg == null || _splitSlabDlg.IsDisposed)
		{
			_splitSlabDlg = new SplitSlabDialog(_model);
			_splitSlabDlg.StartPosition = FormStartPosition.Manual;
			_splitSlabDlg.Location = new Point(base.Location.X, base.Location.Y + base.Height + 5);
			_splitSlabDlg.Show(this);
		}
		else
		{
			_splitSlabDlg.BringToFront();
			_splitSlabDlg.Focus();
		}
	}
}
