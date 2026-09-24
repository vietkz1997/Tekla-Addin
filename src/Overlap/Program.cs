using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace BimCommands.Tekla.OverlapChecker
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                RunApp();
            }
            catch (Exception ex)
            {
                File.WriteAllText("overlap_error.log", ex.ToString());
                MessageBox.Show("Error launching Overlap Checker: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                string reqName = new AssemblyName(args.Name).Name;
                string appDir = AppDomain.CurrentDomain.BaseDirectory;

                // 1. Check local application directory
                string localTarget = Path.Combine(appDir, reqName + ".dll");
                if (File.Exists(localTarget))
                {
                    return Assembly.LoadFrom(localTarget);
                }

                // 2. Check running TeklaStructures process directory
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

                // 3. Check XBIN environment variable
                string xbin = Environment.GetEnvironmentVariable("XBIN");
                if (!string.IsNullOrEmpty(xbin) && Directory.Exists(xbin))
                {
                    string candidate = Path.Combine(xbin, reqName + ".dll");
                    if (File.Exists(candidate)) return Assembly.LoadFrom(candidate);
                }

                // 4. Check common Tekla installation roots
                string[] searchRoots = new string[]
                {
                    @"C:\TeklaStructures",
                    @"C:\Program Files\Trimble\Tekla Structures",
                    @"D:\TeklaStructures",
                    @"D:\Program Files\Trimble\Tekla Structures"
                };

                foreach (var root in searchRoots)
                {
                    if (!Directory.Exists(root)) continue;
                    try
                    {
                        var dlls = Directory.GetFiles(root, reqName + ".dll", SearchOption.AllDirectories);
                        if (dlls.Length > 0)
                        {
                            return Assembly.LoadFrom(dlls[0]);
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
