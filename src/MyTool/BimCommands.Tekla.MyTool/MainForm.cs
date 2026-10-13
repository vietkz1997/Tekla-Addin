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
	private ChangeShapeDialog _changeShapeDlg;

	[STAThread]
	public static void Main(string[] args)
	{
		AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

		// 1. Direct launching if exe name indicates Split-Slab or Change-Shape
		string friendlyName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName ?? "").ToLowerInvariant();
		if (friendlyName.Contains("change") || friendlyName.Contains("shape"))
		{
			RunChangeShapeApp();
			return;
		}
		if (friendlyName.Contains("split") || friendlyName.Contains("slab"))
		{
			RunSplitSlabApp();
			return;
		}

		// 2. Command-line argument routing
		if (args != null && args.Length > 0)
		{
			string arg = args[0].ToLowerInvariant();
			if (arg.Contains("change") || arg.Contains("shape"))
			{
				RunChangeShapeApp();
				return;
			}
			if (arg.Contains("split") || arg.Contains("slab"))
			{
				RunSplitSlabApp();
				return;
			}
			if (arg.Contains("clash"))
			{
				LaunchClashCheckApp();
				return;
			}
			if (arg.Contains("convert"))
			{
				LaunchConvertRebarApp();
				return;
			}
			if (arg.Contains("error") || arg.Contains("rebar"))
			{
				LaunchRebarErrorApp();
				return;
			}
			if (arg.Contains("overlap"))
			{
				LaunchOverlapChecker();
				return;
			}
		}
		RunApp();
	}

	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
	public static void RunChangeShapeApp()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		Model model = new Model();
		Application.Run(new ChangeShapeDialog(model));
	}

	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
	public static void RunSplitSlabApp()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		Model model = new Model();
		Application.Run(new SplitSlabDialog(model));
	}

	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
	private static void RunApp()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		Application.Run(new MainForm());
	}

	private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
	{
		try
		{
			string reqName = new System.Reflection.AssemblyName(args.Name).Name;
			string appDir = AppDomain.CurrentDomain.BaseDirectory;

			// 1. Check local application directory (release/)
			string localTarget = Path.Combine(appDir, reqName + ".dll");
			if (File.Exists(localTarget)) return System.Reflection.Assembly.LoadFrom(localTarget);

			// 2. Find from running TeklaStructures.exe process
			var procs = Process.GetProcessesByName("TeklaStructures");
			if (procs.Length > 0)
			{
				try
				{
					string teklaBin = Path.GetDirectoryName(procs[0].MainModule.FileName);
					if (!string.IsNullOrEmpty(teklaBin))
					{
						string candidate = Path.Combine(teklaBin, reqName + ".dll");
						if (File.Exists(candidate)) return System.Reflection.Assembly.LoadFrom(candidate);
					}
				}
				catch { }
			}
		}
		catch { }
		return null;
	}

	public MainForm()
	{
		InitializeToolbar();
		ConnectTekla();
	}

	private void InitializeToolbar()
	{
		Text = "My-tool";
		base.ClientSize = new Size(1215, 86);
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

		// Row 1 Buttons
		Button button = CreateToolButton("\u2702\ufe0f Split-Slab", num4, num, num2, h, Color.FromArgb(220, 252, 231), Color.FromArgb(22, 101, 52));
		button.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
		button.Click += delegate
		{
			OpenSplitSlabDialog();
		};
		base.Controls.Add(button);
		num4 += num2 + num3;

		Button button2 = CreateToolButton("\u26a0\ufe0f Overlap", num4, num, num2, h, Color.FromArgb(254, 243, 199), Color.FromArgb(146, 64, 14));
		button2.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
		button2.Click += delegate
		{
			LaunchOverlapChecker();
		};
		base.Controls.Add(button2);
		num4 += num2 + num3;

		Button buttonRebarErr = CreateToolButton("\u274c Rebar-Error", num4, num, num2, h, Color.FromArgb(254, 226, 226), Color.FromArgb(185, 28, 28));
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
		num4 += num2 + num3;

		// New Change-Shape Button in Row 1
		Button btnChangeShape = CreateToolButton("Change-Shape", num4, num, num2, h, Color.FromArgb(237, 233, 254), Color.FromArgb(109, 40, 217));
		btnChangeShape.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
		btnChangeShape.Click += delegate
		{
			OpenChangeShapeDialog();
		};
		base.Controls.Add(btnChangeShape);

		// Row 2 Buttons
		int num5 = 44;
		num4 = 6;
		base.Controls.Add(CreateAppButton("\u2194\ufe0f Split Rebar", "AppSplitRebar.exe", num4, num5, num2, h));
		num4 += num2 + num3;
		base.Controls.Add(CreateAppButton("\u2797 Divide Rebar", "AppDivideRebar.exe", num4, num5, num2, h));
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

		Button buttonClash = CreateToolButton("\ud83d\udca5 Clash-Check", num4, num5, num2, h, Color.FromArgb(224, 231, 255), Color.FromArgb(67, 56, 202));
		buttonClash.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
		buttonClash.Click += delegate
		{
			LaunchClashCheckApp();
		};
		base.Controls.Add(buttonClash);
		num4 += num2 + num3;

		Button buttonConvert = CreateToolButton("Rebar-Convert", num4, num5, num2, h, Color.FromArgb(224, 242, 254), Color.FromArgb(3, 105, 161));
		buttonConvert.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
		buttonConvert.Click += delegate
		{
			LaunchConvertRebarApp();
		};
		base.Controls.Add(buttonConvert);
		num4 += num2 + num3;

		base.Controls.Add(CreateAppButton("\u2699\ufe0f Preference", "AppPreference.exe", num4, num5, num2, h));
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

	private static string FindExecutable(string exeName, string subFolder = null)
	{
		var candidates = new System.Collections.Generic.List<string>();
		string baseDir = AppDomain.CurrentDomain.BaseDirectory;

		candidates.Add(Path.Combine(baseDir, exeName));
		if (!string.IsNullOrEmpty(subFolder))
		{
			candidates.Add(Path.Combine(baseDir, subFolder, exeName));
		}

		string[] fixedRoots = new string[]
		{
			@"C:\tekla\My-tool",
			@"C:\tekla\MyTool",
			@"D:\tekla\My-tool",
			@"C:\Users\BIM\Documents\Github\Tekla-Addin\dist\MyTool",
			@"C:\Users\BIM\Documents\Github\Tekla-Addin\release"
		};

		foreach (var r in fixedRoots)
		{
			candidates.Add(Path.Combine(r, exeName));
			if (!string.IsNullOrEmpty(subFolder))
			{
				candidates.Add(Path.Combine(r, subFolder, exeName));
			}
		}

		try
		{
			var procs = Process.GetProcessesByName("TeklaStructures");
			if (procs.Length > 0)
			{
				string bin = Path.GetDirectoryName(procs[0].MainModule.FileName);
				string app1 = Path.Combine(bin, "applications", exeName);
				string app2 = Path.Combine(bin, "nt", "bin", "applications", "My-tool", exeName);
				candidates.Add(app1);
				candidates.Add(app2);
			}
		}
		catch { }

		foreach (var c in candidates)
		{
			if (File.Exists(c)) return c;
		}

		return null;
	}

	private static void LaunchExternalApp(string exeName, string subFolder = null)
	{
		string target = FindExecutable(exeName, subFolder);
		if (!string.IsNullOrEmpty(target))
		{
			try
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = target,
					WorkingDirectory = Path.GetDirectoryName(target),
					UseShellExecute = true
				});
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error starting " + exeName + ":\n" + ex.Message, "My-tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}
		MessageBox.Show("Tool not found: " + exeName + "\nPlease place it in tool directory, release folder, or C:\\tekla\\My-tool\\", "My-tool", MessageBoxButtons.OK, MessageBoxIcon.Information);
	}

	private static void LaunchOverlapChecker()
	{
		LaunchExternalApp("Overlap.exe", "Overlap");
	}

	private static void LaunchClashCheckApp()
	{
		LaunchExternalApp("Clash-check.exe", "ClashCheck");
	}

	private static void LaunchRebarErrorApp()
	{
		LaunchExternalApp("Rebar-error.exe", "RebarError");
	}

	private static void LaunchConvertRebarApp()
	{
		LaunchExternalApp("Convert-rebar.exe", "ConvertRebar");
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
				Text = "My-tool (Tekla Not Connected)";
			}
		}
		catch
		{
			Text = "My-tool (Offline)";
		}
	}

	private void OpenChangeShapeDialog()
	{
		if (_changeShapeDlg == null || _changeShapeDlg.IsDisposed)
		{
			_changeShapeDlg = new ChangeShapeDialog(_model);
			_changeShapeDlg.StartPosition = FormStartPosition.Manual;
			_changeShapeDlg.Location = new Point(base.Location.X, base.Location.Y + base.Height + 5);
			_changeShapeDlg.Show(this);
		}
		else
		{
			_changeShapeDlg.BringToFront();
			_changeShapeDlg.Focus();
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
