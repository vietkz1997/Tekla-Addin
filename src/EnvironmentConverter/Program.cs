using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using BimCommands.EnvironmentConverter.UI;

namespace BimCommands.EnvironmentConverter
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += (s, e) =>
                {
                    HandleFatalException(e.Exception);
                };
                AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    if (e.ExceptionObject is Exception ex)
                    {
                        HandleFatalException(ex);
                    }
                };

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                RunApp();
            }
            catch (Exception ex)
            {
                HandleFatalException(ex);
            }
        }

        private static void HandleFatalException(Exception ex)
        {
            try
            {
                string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "environment_converter_error.log");
                File.WriteAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]\n{ex}");
            }
            catch { }

            MessageBox.Show("Lỗi khởi chạy Environment Converter:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                string reqName = new AssemblyName(args.Name).Name;
                if (string.IsNullOrEmpty(reqName) || reqName.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                string appDir = AppDomain.CurrentDomain.BaseDirectory;

                // 1. Check current directory
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

                // 4. Check common Tekla installation paths (check nt\bin and bin)
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
                        foreach (var verDir in Directory.GetDirectories(root))
                        {
                            // Standard Tekla Structures binary directory is nt\bin
                            string ntBinDir = Path.Combine(verDir, "nt", "bin");
                            if (Directory.Exists(ntBinDir))
                            {
                                string candidate = Path.Combine(ntBinDir, reqName + ".dll");
                                if (File.Exists(candidate)) return Assembly.LoadFrom(candidate);
                            }

                            string binDir = Path.Combine(verDir, "bin");
                            if (Directory.Exists(binDir))
                            {
                                string candidate = Path.Combine(binDir, reqName + ".dll");
                                if (File.Exists(candidate)) return Assembly.LoadFrom(candidate);
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
