using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using RevitQtoTool.Views;

namespace RevitQtoTool
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                RunStandaloneApp();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi động ứng dụng: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RunStandaloneApp()
        {
            var app = new Application();
            var window = new QtoWindow(null);
            app.Run(window);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                string reqName = new AssemblyName(args.Name).Name;
                string appDir = AppDomain.CurrentDomain.BaseDirectory;

                string localTarget = Path.Combine(appDir, reqName + ".dll");
                if (File.Exists(localTarget))
                {
                    return Assembly.LoadFrom(localTarget);
                }

                string binTarget = Path.Combine(appDir, "Bin", reqName + ".dll");
                if (File.Exists(binTarget))
                {
                    return Assembly.LoadFrom(binTarget);
                }

                string r26Target = Path.Combine(appDir, "Revit_2026", reqName + ".dll");
                if (File.Exists(r26Target))
                {
                    return Assembly.LoadFrom(r26Target);
                }

                string r24Target = Path.Combine(appDir, "Revit_2024", reqName + ".dll");
                if (File.Exists(r24Target))
                {
                    return Assembly.LoadFrom(r24Target);
                }
            }
            catch { }

            return null;
        }
    }
}
