using System;
using System.Windows.Forms;

namespace BimCommands.Tekla.ClashCheck
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    try
                    {
                        var reqName = new System.Reflection.AssemblyName(args.Name).Name;
                        string appDir = AppDomain.CurrentDomain.BaseDirectory;
                        string target = System.IO.Path.Combine(appDir, reqName + ".dll");
                        if (System.IO.File.Exists(target))
                        {
                            return System.Reflection.Assembly.LoadFrom(target);
                        }
                    }
                    catch { }
                    return null;
                };

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("clash_error.log", ex.ToString());
                MessageBox.Show("Lỗi khởi chạy Clash-check: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
