using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace BimCommands.Tekla.ClashCheck
{
    /// <summary>
    /// Điểm khởi nhập chính của ứng dụng kiểm tra va chạm (Clash-check).
    /// Thiết lập bộ phân giải Assembly động từ Tekla Structures trước khi khởi tạo UI.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Điểm vào (Entry point) chính của ứng dụng.
        /// Cấu hình sự kiện nạp thư viện động trước khi gọi giao diện MainForm.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            try
            {
                // Đăng ký bộ phân giải Assembly trước khi JIT compiler phân giải các kiểu dữ liệu Tekla
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                RunApp();
            }
            catch (Exception ex)
            {
                File.WriteAllText("clash_error.log", ex.ToString());
                MessageBox.Show("Lỗi khởi chạy Clash-check: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Tự động phân giải các file DLL phụ thuộc khi ứng dụng không tìm thấy trong thư mục mặc định:
        /// 1. Kiểm tra trong thư mục ứng dụng hiện tại (thư mục release/).
        /// 2. Tìm trong thư mục cài đặt bin của tiến trình TeklaStructures.exe đang chạy trên máy.
        /// </summary>
        /// <param name="sender">Nguồn phát sự kiện.</param>
        /// <param name="args">Thông tin Assembly cần tìm kiếm.</param>
        /// <returns>Assembly đã nạp thành công hoặc null nếu không tìm thấy.</returns>
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

        /// <summary>
        /// Khởi chạy giao diện chính Windows Forms.
        /// Phải tách biệt trong hàm NoInlining để tránh việc CLR nạp trước các kiểu dữ liệu của Tekla
        /// khi hàm Main() chưa kịp đăng ký sự kiện AssemblyResolve.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RunApp()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}

