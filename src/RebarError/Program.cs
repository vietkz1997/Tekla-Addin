using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace BimCommands.RebarErrorChecker
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                RunApp();
            }
            catch (Exception ex)
            {
                File.WriteAllText("rebar_error.log", ex.ToString());
                MessageBox.Show("Lỗi khởi chạy Rebar-error: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                string reqName = new AssemblyName(args.Name).Name;
                string appDir = AppDomain.CurrentDomain.BaseDirectory;

                // 1. Kiểm tra thư mục ứng dụng hiện tại (release/)
                string localTarget = Path.Combine(appDir, reqName + ".dll");
                if (File.Exists(localTarget))
                {
                    return Assembly.LoadFrom(localTarget);
                }

                // 2. Tìm từ tiến trình TeklaStructures.exe đang chạy
                var procs = Process.GetProcessesByName("TeklaStructures");
                if (procs.Length > 0)
                {
                    try
                    {
                        string teklaBin = Path.GetDirectoryName(procs[0].MainModule.FileName);
                        if (!string.IsNullOrEmpty(teklaBin))
                        {
                            string candidate = Path.Combine(teklaBin, reqName + ".dll");
                            if (File.Exists(candidate))
                            {
                                return Assembly.LoadFrom(candidate);
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RunApp()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
