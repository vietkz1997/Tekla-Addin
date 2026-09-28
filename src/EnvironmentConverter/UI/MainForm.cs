using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BimCommands.EnvironmentConverter.Core;
using BimCommands.EnvironmentConverter.Models;

namespace BimCommands.EnvironmentConverter.UI
{
    public class MainForm : Form
    {
        // UI Controls
        private Panel headerPanel;
        private Label lblTitle;
        private Label lblSubtitle;

        private GroupBox grpModel;
        private TextBox txtModelPath;
        private Button btnGetActiveModel;
        private Button btnBrowseModel;
        private Label lblModelStatus;

        private GroupBox grpTargetEnv;
        private ComboBox cboTargetEnv;
        private Button btnRefreshEnvs;
        private Button btnBrowseTargetEnv;
        private Label lblEnvStatus;

        private GroupBox grpOptions;
        private CheckBox chkBackup;
        private CheckBox chkSyncCatalogs;
        private CheckBox chkSyncRebarRules;
        private CheckBox chkUpdateEnvIni;
        private CheckBox chkSyncAttributes;
        private CheckBox chkAutoMapMaterials;

        private GroupBox grpLog;
        private RichTextBox rtbLog;
        private ProgressBar progressBar;
        private Button btnInspect;
        private Button btnConvert;

        // State data
        private List<TeklaEnvironmentInfo> _availableEnvironments = new List<TeklaEnvironmentInfo>();
        private ModelInspectionResult _currentModelInspection;
        private TeklaEnvironmentInfo _selectedTargetEnv;
        private List<MaterialMappingRule> _currentMappingRules = new List<MaterialMappingRule>();
        private Button btnConfigureMapping;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Tekla Structures - Environment Converter";
            this.Size = new Size(820, 720);
            this.MinimumSize = new Size(780, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.ShowInTaskbar = true;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.BackColor = Color.FromArgb(245, 247, 250);

            // 1. Header Panel
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(24, 43, 73)
            };

            lblTitle = new Label
            {
                Text = "TEKLA ENVIRONMENT CONVERTER",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(16, 12),
                AutoSize = true
            };

            lblSubtitle = new Label
            {
                Text = "Chuyển đổi Environment Model (Ví dụ: Korea → Vietnam) an toàn, chuẩn hóa catalogs & thuộc tính",
                ForeColor = Color.FromArgb(180, 205, 237),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                Location = new Point(17, 36),
                AutoSize = true
            };

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            this.Controls.Add(headerPanel);

            // Container Panel with AutoScroll
            var bodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(12)
            };
            this.Controls.Add(bodyPanel);
            bodyPanel.BringToFront();

            int currentY = 10;

            // 2. Group Model
            grpModel = new GroupBox
            {
                Text = "1. Mô hình Tekla cần chuyển đổi (Source Model)",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(12, currentY),
                Size = new Size(765, 115),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblPath = new Label
            {
                Text = "Đường dẫn Model:",
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(15, 27),
                AutoSize = true
            };

            txtModelPath = new TextBox
            {
                Location = new Point(125, 24),
                Size = new Size(420, 25),
                Font = new Font("Segoe UI", 9F),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            btnGetActiveModel = new Button
            {
                Text = "Model đang mở",
                Location = new Point(555, 23),
                Size = new Size(105, 27),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                BackColor = Color.FromArgb(235, 243, 254),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnGetActiveModel.Click += BtnGetActiveModel_Click;

            btnBrowseModel = new Button
            {
                Text = "Chọn thư mục...",
                Location = new Point(665, 23),
                Size = new Size(90, 27),
                Font = new Font("Segoe UI", 8.5F),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnBrowseModel.Click += BtnBrowseModel_Click;

            lblModelStatus = new Label
            {
                Text = "Chưa nạp thông tin mô hình.",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.DimGray,
                Location = new Point(15, 58),
                Size = new Size(735, 48),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            grpModel.Controls.Add(lblPath);
            grpModel.Controls.Add(txtModelPath);
            grpModel.Controls.Add(btnGetActiveModel);
            grpModel.Controls.Add(btnBrowseModel);
            grpModel.Controls.Add(lblModelStatus);
            bodyPanel.Controls.Add(grpModel);

            currentY += 125;

            // 3. Group Target Environment
            grpTargetEnv = new GroupBox
            {
                Text = "2. Môi trường đích (Target Environment)",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(12, currentY),
                Size = new Size(765, 110),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblEnv = new Label
            {
                Text = "Chọn Environment:",
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(15, 27),
                AutoSize = true
            };

            cboTargetEnv = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(125, 24),
                Size = new Size(420, 25),
                Font = new Font("Segoe UI", 9F),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            cboTargetEnv.SelectedIndexChanged += CboTargetEnv_SelectedIndexChanged;

            btnRefreshEnvs = new Button
            {
                Text = "Làm mới",
                Location = new Point(555, 23),
                Size = new Size(75, 27),
                Font = new Font("Segoe UI", 8.5F),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnRefreshEnvs.Click += (s, e) => RefreshEnvironmentList();

            btnBrowseTargetEnv = new Button
            {
                Text = "Thư mục khác...",
                Location = new Point(635, 23),
                Size = new Size(120, 27),
                Font = new Font("Segoe UI", 8.5F),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnBrowseTargetEnv.Click += BtnBrowseTargetEnv_Click;

            lblEnvStatus = new Label
            {
                Text = "Chưa chọn Environment đích.",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.DimGray,
                Location = new Point(15, 58),
                Size = new Size(735, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            grpTargetEnv.Controls.Add(lblEnv);
            grpTargetEnv.Controls.Add(cboTargetEnv);
            grpTargetEnv.Controls.Add(btnRefreshEnvs);
            grpTargetEnv.Controls.Add(btnBrowseTargetEnv);
            grpTargetEnv.Controls.Add(lblEnvStatus);
            bodyPanel.Controls.Add(grpTargetEnv);

            currentY += 120;

            // 4. Group Options
            grpOptions = new GroupBox
            {
                Text = "3. Tùy chọn xử lý & Chuẩn hóa (Conversion Tasks)",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(12, currentY),
                Size = new Size(765, 110),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chkBackup = new CheckBox
            {
                Text = "Tự động sao lưu (Backup) Model trước khi chuyển đổi",
                Checked = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                Location = new Point(18, 25),
                Size = new Size(350, 22)
            };

            chkSyncCatalogs = new CheckBox
            {
                Text = "Đồng bộ Catalogs (Profile, Material, Bolt) từ Environment đích",
                Checked = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                Location = new Point(18, 50),
                Size = new Size(350, 22)
            };

            chkSyncRebarRules = new CheckBox
            {
                Text = "Đồng bộ Rebar Shape Rules & ShapeCatalog (Tránh lỗi Unknown Shape)",
                Checked = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                Location = new Point(18, 75),
                Size = new Size(350, 22)
            };

            chkUpdateEnvIni = new CheckBox
            {
                Text = "Cập nhật options.ini & model.ini trỏ sang Environment đích",
                Checked = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                Location = new Point(390, 25),
                Size = new Size(360, 22)
            };

            chkSyncAttributes = new CheckBox
            {
                Text = "Đồng bộ Attributes chuẩn (Bản vẽ, bộ lọc filter, view settings)",
                Checked = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                Location = new Point(390, 50),
                Size = new Size(360, 22)
            };

            chkAutoMapMaterials = new CheckBox
            {
                Text = "Ánh xạ mác thép/vật liệu (Ví dụ: SD400 -> CB400-V, SM490 -> Q345B)",
                Checked = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                Location = new Point(390, 75),
                Size = new Size(245, 22)
            };

            btnConfigureMapping = new Button
            {
                Text = "Cấu hình Ánh xạ...",
                Location = new Point(640, 72),
                Size = new Size(115, 26),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                BackColor = Color.FromArgb(235, 243, 254),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnConfigureMapping.Click += BtnConfigureMapping_Click;

            grpOptions.Controls.Add(chkBackup);
            grpOptions.Controls.Add(chkSyncCatalogs);
            grpOptions.Controls.Add(chkSyncRebarRules);
            grpOptions.Controls.Add(chkUpdateEnvIni);
            grpOptions.Controls.Add(chkSyncAttributes);
            grpOptions.Controls.Add(chkAutoMapMaterials);
            grpOptions.Controls.Add(btnConfigureMapping);
            bodyPanel.Controls.Add(grpOptions);

            currentY += 120;

            // 5. Group Log & Execution
            grpLog = new GroupBox
            {
                Text = "4. Nhật ký xử lý & Thực thi",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(12, currentY),
                Size = new Size(765, 230),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            rtbLog = new RichTextBox
            {
                Location = new Point(15, 24),
                Size = new Size(735, 125),
                ReadOnly = true,
                BackColor = Color.FromArgb(250, 250, 252),
                Font = new Font("Consolas", 8.5F),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            progressBar = new ProgressBar
            {
                Location = new Point(15, 158),
                Size = new Size(735, 18),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            btnInspect = new Button
            {
                Text = "Kiểm tra trước khi đổi (Inspect)",
                Location = new Point(15, 185),
                Size = new Size(210, 32),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(235, 243, 254),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnInspect.Click += BtnInspect_Click;

            btnConvert = new Button
            {
                Text = "BẮT ĐẦU CHUYỂN ĐỔI ENVIRONMENT",
                Location = new Point(480, 185),
                Size = new Size(270, 32),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                BackColor = Color.FromArgb(24, 119, 242),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnConvert.Click += BtnConvert_Click;

            grpLog.Controls.Add(rtbLog);
            grpLog.Controls.Add(progressBar);
            grpLog.Controls.Add(btnInspect);
            grpLog.Controls.Add(btnConvert);
            bodyPanel.Controls.Add(grpLog);

            this.Shown += MainForm_Shown;
        }

        private async void MainForm_Shown(object sender, EventArgs e)
        {
            try
            {
                this.Activate();
                this.BringToFront();
            }
            catch { }

            Log("Khởi động Tekla Structures Environment Converter...", Color.Black, true);
            await InitializeDataAsync();
        }

        private async Task InitializeDataAsync()
        {
            try
            {
                // 1. Quét danh sách Environment đích (chạy ngầm, không khóa UI)
                await Task.Run(() =>
                {
                    _availableEnvironments = TeklaEnvironmentScanner.ScanAllEnvironments();
                });

                cboTargetEnv.Items.Clear();
                if (_availableEnvironments.Count == 0)
                {
                    Log("Cảnh báo: Không tự động tìm thấy Environment nào trong các thư mục cài đặt tiêu chuẩn.", Color.DarkOrange);
                    lblEnvStatus.Text = "Không tìm thấy Environment. Vui lòng bấm 'Thư mục khác...' để chọn thủ công.";
                }
                else
                {
                    foreach (var env in _availableEnvironments)
                    {
                        cboTargetEnv.Items.Add(env);
                    }

                    // Prefer Vietnam or South_East_Asia if available
                    int preferIdx = -1;
                    for (int i = 0; i < _availableEnvironments.Count; i++)
                    {
                        var env = _availableEnvironments[i];
                        if (env.Name.IndexOf("vietnam", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            env.Name.IndexOf("south_east_asia", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            preferIdx = i;
                            break;
                        }
                    }

                    if (preferIdx >= 0)
                    {
                        cboTargetEnv.SelectedIndex = preferIdx;
                    }
                    else if (cboTargetEnv.Items.Count > 0)
                    {
                        cboTargetEnv.SelectedIndex = 0;
                    }

                    Log($"Đã nạp thành công {_availableEnvironments.Count} Environment cài đặt trên hệ thống.", Color.DarkGreen);
                }

                // 2. Tự động nhận diện model đang mở trong Tekla (chỉ đọc thông tin ban đầu, không quét chi tiết live objects ngay)
                if (ModelEnvironmentDetector.IsTeklaConnected(out string path, out string name))
                {
                    txtModelPath.Text = path;
                    Log($"Phát hiện phiên Tekla đang mở Model: {name} ({path})", Color.DarkBlue, true);
                    await InspectSelectedModelAsync(scanLiveObjects: false);
                    Log("Gợi ý: Bấm '1. Kiểm tra Model' nếu bạn muốn quét chi tiết toàn bộ vật liệu và mác thép thực tế trong Model.", Color.Gray);
                }
                else
                {
                    Log("Tekla Structures chưa mở model nào hoặc đang đóng. Bạn có thể chọn Model từ thư mục đĩa.", Color.Gray);
                }
            }
            catch (Exception ex)
            {
                Log("Lỗi khởi tạo ứng dụng: " + ex.Message, Color.Red);
            }
        }

        private async void RefreshEnvironmentList()
        {
            try
            {
                cboTargetEnv.Items.Clear();
                await Task.Run(() =>
                {
                    _availableEnvironments = TeklaEnvironmentScanner.ScanAllEnvironments();
                });

                if (_availableEnvironments.Count == 0)
                {
                    Log("Cảnh báo: Không tự động tìm thấy Environment nào trong các thư mục cài đặt tiêu chuẩn.", Color.DarkOrange);
                    lblEnvStatus.Text = "Không tìm thấy Environment. Vui lòng bấm 'Thư mục khác...' để chọn thủ công.";
                    return;
                }

                foreach (var env in _availableEnvironments)
                {
                    cboTargetEnv.Items.Add(env);
                }

                // Prefer Vietnam or South_East_Asia if available
                int preferIdx = -1;
                for (int i = 0; i < _availableEnvironments.Count; i++)
                {
                    var env = _availableEnvironments[i];
                    if (env.Name.IndexOf("vietnam", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        env.Name.IndexOf("south_east_asia", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        preferIdx = i;
                        break;
                    }
                }

                if (preferIdx >= 0)
                {
                    cboTargetEnv.SelectedIndex = preferIdx;
                }
                else if (cboTargetEnv.Items.Count > 0)
                {
                    cboTargetEnv.SelectedIndex = 0;
                }

                Log($"Đã quét thành công {_availableEnvironments.Count} Environment cài đặt trên hệ thống.", Color.DarkGreen);
            }
            catch (Exception ex)
            {
                Log("Lỗi khi quét Environment: " + ex.Message, Color.Red);
            }
        }

        private async void BtnGetActiveModel_Click(object sender, EventArgs e)
        {
            if (ModelEnvironmentDetector.IsTeklaConnected(out string path, out string name))
            {
                txtModelPath.Text = path;
                Log($"Phát hiện phiên Tekla đang mở Model: {name} ({path})", Color.DarkBlue, true);
                await InspectSelectedModelAsync(scanLiveObjects: false);
            }
            else
            {
                MessageBox.Show("Không tìm thấy phiên Tekla Structures nào đang mở model!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void BtnBrowseModel_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Chọn thư mục Model Tekla cần chuyển đổi:";
                if (!string.IsNullOrEmpty(txtModelPath.Text) && Directory.Exists(txtModelPath.Text))
                {
                    dlg.SelectedPath = txtModelPath.Text;
                }

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtModelPath.Text = dlg.SelectedPath;
                    await InspectSelectedModelAsync(scanLiveObjects: false);
                }
            }
        }

        private void BtnBrowseTargetEnv_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Chọn thư mục Environment đích (chứa env_*.ini hoặc catalogs):";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string dir = dlg.SelectedPath;
                    string name = Path.GetFileName(dir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                    var customEnv = TeklaEnvironmentScanner.ParseEnvironmentInfo(dir, name, "Custom");

                    if (customEnv != null)
                    {
                        _availableEnvironments.Insert(0, customEnv);
                        cboTargetEnv.Items.Insert(0, customEnv);
                        cboTargetEnv.SelectedIndex = 0;
                        Log($"Đã nạp Environment tùy chỉnh: {name} ({dir})", Color.DarkGreen);
                    }
                }
            }
        }

        private void CboTargetEnv_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboTargetEnv.SelectedItem is TeklaEnvironmentInfo env)
            {
                _selectedTargetEnv = env;
                string status = $"Target: {env.Name} | Profiles: {(env.HasProfiles ? "Có" : "Không")} | Materials: {(env.HasMaterials ? "Có" : "Không")} | RebarRules: {(env.HasRebarRules ? "Có" : "Không")} | Attributes: {(env.HasAttributes ? "Có" : "Không")}\nĐường dẫn: {env.RootPath}";
                lblEnvStatus.Text = status;
                lblEnvStatus.ForeColor = Color.DarkSlateBlue;
            }
        }

        private async void BtnInspect_Click(object sender, EventArgs e)
        {
            await InspectSelectedModelAsync(scanLiveObjects: true);
        }

        private async Task InspectSelectedModelAsync(bool scanLiveObjects)
        {
            string path = txtModelPath.Text.Trim();
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                MessageBox.Show("Vui lòng chọn đường dẫn thư mục Model hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                btnInspect.Enabled = false;
                btnConvert.Enabled = false;
                progressBar.Style = ProgressBarStyle.Marquee;

                Log("----------------------------------------------------------------", Color.Gray);
                Log($"Bắt đầu kiểm tra Model: {path} {(scanLiveObjects ? "(Đang quét đối tượng trong Tekla...)" : "(Đọc cấu hình file trên đĩa)...")}", Color.Black, true);

                _currentModelInspection = ModelEnvironmentDetector.InspectModelFolder(path, scanLiveObjects);

                string statusText = $"Model: {_currentModelInspection.ModelName} | Env phát hiện: {_currentModelInspection.DetectedEnvironment} | Role: {_currentModelInspection.DetectedRole}\n" +
                                   $"Cấu kiện: {_currentModelInspection.PartCount} Parts, {_currentModelInspection.RebarCount} Rebars, {_currentModelInspection.BoltCount} Bolts | Đang mở trong Tekla: {(_currentModelInspection.IsOpenInActiveTekla ? "Có" : "Không")}\n" +
                                   $"Local DBs: Prof={(_currentModelInspection.HasLocalProfileDb ? "Có" : "Không")}, Mat={(_currentModelInspection.HasLocalMaterialDb ? "Có" : "Không")}, Bolt={(_currentModelInspection.HasLocalBoltDb ? "Có" : "Không")}, RebarRules={(_currentModelInspection.HasLocalRebarRules ? "Có" : "Không")}";

                lblModelStatus.Text = statusText;
                lblModelStatus.ForeColor = Color.DarkSlateBlue;

                Log($"Environment gốc phát hiện: {_currentModelInspection.DetectedEnvironment}", Color.DarkBlue);
                if (scanLiveObjects)
                {
                    Log($"Thống kê mô hình: {_currentModelInspection.PartCount} Parts, {_currentModelInspection.RebarCount} Rebars, {_currentModelInspection.BoltCount} Bolts", Color.Black);

                    if (_currentModelInspection.UsedMaterials.Count > 0)
                    {
                        Log($"Các loại vật liệu đang dùng ({_currentModelInspection.UsedMaterials.Count}): {string.Join(", ", _currentModelInspection.UsedMaterials.Take(8))}{(_currentModelInspection.UsedMaterials.Count > 8 ? "..." : "")}", Color.DimGray);
                    }

                    if (_currentModelInspection.UsedRebarGrades.Count > 0)
                    {
                        Log($"Các mác thép cốt đang dùng ({_currentModelInspection.UsedRebarGrades.Count}): {string.Join(", ", _currentModelInspection.UsedRebarGrades)}", Color.DimGray);
                    }
                }

                // Build or update customized mapping rules for this model
                _currentMappingRules = MaterialMapper.BuildTailoredRules(_currentModelInspection);

                Log($"Đã nạp sẵn {_currentMappingRules.Count} quy tắc ánh xạ vật liệu & mác thép (bấm 'Cấu hình Ánh xạ...' để xem chi tiết).", Color.DarkSlateBlue);
                Log("Kiểm tra mô hình hoàn tất. Sẵn sàng cho quá trình chuyển đổi.", Color.DarkGreen);
            }
            catch (Exception ex)
            {
                Log("Lỗi kiểm tra Model: " + ex.Message, Color.Red);
            }
            finally
            {
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 0;
                btnInspect.Enabled = true;
                btnConvert.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void BtnConfigureMapping_Click(object sender, EventArgs e)
        {
            if (_currentMappingRules == null || _currentMappingRules.Count == 0)
            {
                _currentMappingRules = MaterialMapper.BuildTailoredRules(_currentModelInspection);
            }

            using (var dlg = new MappingDialog(_currentMappingRules))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _currentMappingRules = dlg.CurrentRules;
                    int activeCount = _currentMappingRules.Count(r => r.IsEnabled);
                    Log($"Đã cập nhật bảng quy tắc ánh xạ: {activeCount} quy tắc đang bật.", Color.DarkGreen);
                }
            }
        }

        private async void BtnConvert_Click(object sender, EventArgs e)
        {
            if (_currentModelInspection == null || string.IsNullOrEmpty(_currentModelInspection.ModelPath))
            {
                MessageBox.Show("Vui lòng kiểm tra (Inspect) Model trước khi chuyển đổi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_selectedTargetEnv == null)
            {
                MessageBox.Show("Vui lòng chọn Environment đích!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Xác nhận chuyển đổi Model:\n'{_currentModelInspection.ModelName}'\n\nTừ Environment: '{_currentModelInspection.DetectedEnvironment}'\nSang Environment: '{_selectedTargetEnv.Name}'?\n\nBạn có muốn tiếp tục?",
                "Xác nhận chuyển đổi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            btnConvert.Enabled = false;
            btnInspect.Enabled = false;
            progressBar.Value = 0;

            try
            {
                Log("================================================================", Color.Navy);
                Log($"BẮT ĐẦU CHUYỂN ĐỔI: {_currentModelInspection.DetectedEnvironment} → {_selectedTargetEnv.Name}", Color.Navy, true);
                Log($"Thư mục Model: {_currentModelInspection.ModelPath}", Color.Navy);
                Log($"Environment đích: {_selectedTargetEnv.RootPath}", Color.Navy);

                string modelPath = _currentModelInspection.ModelPath;

                // Task 1: Backup
                if (chkBackup.Checked)
                {
                    Log("\n--- BƯỚC 1: TIẾN HÀNH SAO LƯU MODEL DỰ PHÒNG ---", Color.Black, true);
                    string backupDir = await ModelBackupManager.CreateModelBackupAsync(
                        modelPath,
                        (item, pct, msg) =>
                        {
                            if (pct >= 0 && pct <= 100) progressBar.Value = pct / 3; // 0 - 33%
                            if (pct % 50 == 0 || pct == 100) Log(msg, Color.DimGray);
                        });
                    Log($"Sao lưu an toàn thành công tại: {backupDir}", Color.DarkGreen, true);
                }

                progressBar.Value = 35;

                // Task 2: Catalogs
                if (chkSyncCatalogs.Checked)
                {
                    Log("\n--- BƯỚC 2: ĐỒNG BỘ CƠ SỞ DỮ LIỆU CATALOGS ---", Color.Black, true);
                    CatalogManager.SyncCatalogs(modelPath, _selectedTargetEnv, (msg, success) =>
                    {
                        Log(msg, success ? Color.DarkSlateGray : Color.DarkOrange);
                    });
                }

                progressBar.Value = 55;

                // Task 3: Rebar Shape Rules
                if (chkSyncRebarRules.Checked)
                {
                    Log("\n--- BƯỚC 3: ĐỒNG BỘ REBAR SHAPE RULES & SHAPE CATALOG ---", Color.Black, true);
                    CatalogManager.SyncRebarShapeRules(modelPath, _selectedTargetEnv, (msg, success) =>
                    {
                        Log(msg, success ? Color.DarkSlateGray : Color.DarkOrange);
                    });
                }

                progressBar.Value = 70;

                // Task 4: Attributes
                if (chkSyncAttributes.Checked)
                {
                    Log("\n--- BƯỚC 4: ĐỒNG BỘ THUỘC TÍNH BẢN VẼ & VIEW (ATTRIBUTES) ---", Color.Black, true);
                    CatalogManager.SyncAttributes(modelPath, _selectedTargetEnv, (msg, success) =>
                    {
                        Log(msg, success ? Color.DarkSlateGray : Color.DarkOrange);
                    });
                }

                progressBar.Value = 85;

                // Task 5: Options.ini and Model.ini
                if (chkUpdateEnvIni.Checked)
                {
                    Log("\n--- BƯỚC 5: CẬP NHẬT CẤU HÌNH OPTIONS.INI & FONT CHỮ ---", Color.Black, true);
                    CatalogManager.UpdateEnvironmentOptions(modelPath, _selectedTargetEnv, (msg, success) =>
                    {
                        Log(msg, success ? Color.DarkSlateGray : Color.DarkOrange);
                    });
                }

                progressBar.Value = 90;

                // Task 6: Batch Mapping of Materials & Rebars
                if (chkAutoMapMaterials.Checked)
                {
                    Log("\n--- BƯỚC 6: ÁNH XẠ VẬT LIỆU & MÁC THÉP (KOREA → VIETNAM) ---", Color.Black, true);
                    if (_currentMappingRules == null || _currentMappingRules.Count == 0)
                    {
                        _currentMappingRules = MaterialMapper.BuildTailoredRules(_currentModelInspection);
                    }

                    var activeModel = new Tekla.Structures.Model.Model();
                    if (activeModel.GetConnectionStatus())
                    {
                        string livePath = activeModel.GetInfo().ModelPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        string currentPath = modelPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                        if (string.Equals(livePath, currentPath, StringComparison.OrdinalIgnoreCase))
                        {
                            MaterialMapper.ExecuteBatchMapping(
                                activeModel,
                                _currentMappingRules,
                                (msg, success) => Log(msg, success ? Color.DarkGreen : Color.DarkOrange),
                                out int partsModified,
                                out int rebarsModified);

                            Log($"Tổng kết: Đã chuyển đổi {partsModified} Parts và {rebarsModified} Rebars sang mác chuẩn!", Color.DarkGreen, true);
                        }
                        else
                        {
                            Log("Model đang xử lý khác với Model đang mở trong Tekla Structures.", Color.DarkOrange);
                            Log("Cơ sở dữ liệu Catalogs đã được cập nhật. Bạn hãy mở Model này trong Tekla để chạy Ánh xạ mác đối tượng.", Color.DarkSlateGray);
                        }
                    }
                    else
                    {
                        Log("Tekla Structures chưa mở Model này. Các cơ sở dữ liệu (Catalogs & Rules) đã sẵn sàng.", Color.DarkSlateGray);
                        Log("Sau khi mở Model trong Tekla, bạn có thể bấm chuyển đổi lại để tự động cập nhật mác cho từng cấu kiện.", Color.DarkSlateGray);
                    }
                }

                progressBar.Value = 100;
                Log("\n================================================================", Color.DarkGreen);
                Log($"HOÀN TẤT CHUYỂN ĐỔI TOÀN DIỆN ENVIRONMENT SANG {_selectedTargetEnv.Name.ToUpper()}!", Color.DarkGreen, true);

                MessageBox.Show(
                    $"Quá trình chuyển đổi Model sang Environment '{_selectedTargetEnv.Name}' đã hoàn tất thành công!\n\n- Bản sao lưu an toàn đã được tạo.\n- Cơ sở dữ liệu Catalogs & Rebar Shape Rules đã đồng bộ.\n- File cấu hình & Font chữ đã được chuẩn hóa.",
                    "Chuyển đổi hoàn tất",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log("Lỗi trong quá trình chuyển đổi: " + ex.Message, Color.Red, true);
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnConvert.Enabled = true;
                btnInspect.Enabled = true;
            }
        }

        private void Log(string message, Color color, bool isBold = false)
        {
            if (rtbLog.InvokeRequired)
            {
                rtbLog.Invoke(new Action(() => Log(message, color, isBold)));
                return;
            }

            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = color;
            rtbLog.SelectionFont = new Font(rtbLog.Font, isBold ? FontStyle.Bold : FontStyle.Regular);
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            rtbLog.ScrollToCaret();
        }
    }
}
