using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.IO;
using System.Media;

namespace PomodoroSharp
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer workTimer;
        private System.Windows.Forms.Timer breakTimer;
        private int workTimeRemaining;
        private int breakTimeRemaining;
        private int defaultWorkTime = 30 * 60; // 秒
        private int defaultBreakTime = 10 * 60; // 秒
        private SoundPlayer startWorkSound;
        private SoundPlayer pauseSound;
        private SoundPlayer workEndSound;
        private SoundPlayer resumeSound;
        private SoundPlayer startBreakSound;
        private SoundPlayer stopSound;
        private SoundPlayer breakEndSound;

        // UI 元件
        private Panel mainPanel;
        private Panel workPanel;
        private Panel breakPanel;
        private Panel controlPanel;

        private Label lblWorkTime;
        private Label lblWorkTitle;
        private RoundedButton btnStartWork;
        private RoundedButton btnPauseWork;
        private RoundedButton btnStopWork;
        private NumericUpDown numWorkMinutes;
        private Label lblWorkInput;

        private Label lblBreakTime;
        private Label lblBreakTitle;
        private RoundedButton btnStartBreak;
        private RoundedButton btnPauseBreak;
        private RoundedButton btnStopBreak;
        private NumericUpDown numBreakMinutes;
        private Label lblBreakInput;

        private Button btnMinimize;
        private Button btnClose;

        private bool isWorkPaused = false;
        private bool isBreakPaused = false;
        private CheckBox chkMute; // 新增靜音checkbox
        private bool isMuted = false; // 新增靜音狀態追踪

        // 現代化顏色配置
        private Color primaryColor = Color.FromArgb(74, 126, 180);
        private Color secondaryColor = Color.FromArgb(52, 73, 94);
        private Color accentColor = Color.FromArgb(231, 76, 60);
        private Color successColor = Color.FromArgb(46, 150, 30);
        private Color warningColor = Color.FromArgb(241, 176, 15);
        private Color backgroundColor = Color.FromArgb(236, 240, 241);
        private Color panelColor = Color.White;
        private Color textColor = Color.FromArgb(44, 62, 80);


        public Form1()
        {
            InitializeComponent();
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "PomodoroClock.ico");
            if (File.Exists(iconPath))
            {
                this.Icon = new Icon(iconPath);
            }
            InitializeSounds();
            SetupModernUI();
            InitializeTimers();
        }

        private void InitializeSounds()
        {
            string startWorkSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Effect1.wav");
            string pauseSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Effect8.wav");
            string workEndSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Effect5.wav");
            string resumeSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Effect9.wav");
            string startBreakSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Effect3.wav");
            string stopSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Effect6.wav");
            string breakEndSoundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Effect7.wav");


            if (File.Exists(startWorkSoundPath))
            {
                startWorkSound = new SoundPlayer(startWorkSoundPath);
            }
            if (File.Exists(pauseSoundPath))
            {
                pauseSound = new SoundPlayer(pauseSoundPath);
            }
            if (File.Exists(workEndSoundPath))
            {
                workEndSound = new SoundPlayer(workEndSoundPath);
            }
            if (File.Exists(resumeSoundPath))
            {
                resumeSound = new SoundPlayer(resumeSoundPath);
            }
            if (File.Exists(startBreakSoundPath))
            {
                startBreakSound = new SoundPlayer(startBreakSoundPath);
            }
            if (File.Exists(stopSoundPath))
            {
                stopSound = new SoundPlayer(stopSoundPath);
            }
            if (File.Exists(breakEndSoundPath))
            {
                breakEndSound = new SoundPlayer(breakEndSoundPath);
            }
        }

        private void SetupModernUI()
        {
            // 主視窗設定
            this.Text = "Pomodoro Timer";
            this.Size = new Size(800, 420);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = backgroundColor;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // 主面板
            mainPanel = new Panel
            {
                Size = new Size(780, 580),
                Location = new Point(10, 10),
                BackColor = panelColor,
                BorderStyle = BorderStyle.None
            };
            mainPanel.Paint += (s, e) => DrawRoundedPanel(e.Graphics, mainPanel, 15);

            // 標題欄
            CreateTitleBar();

            // 工作區域面板
            workPanel = CreateModernPanel(50, 80, 330, 420, "工作計時器");
            breakPanel = CreateModernPanel(400, 80, 330, 420, "休息計時器");

            // 工作區域控制項
            SetupWorkControls();
            SetupBreakControls();
            SetupMuteControl();

            this.Controls.Add(mainPanel);
            mainPanel.Controls.AddRange(new Control[] { workPanel, breakPanel });
        }

        private void CreateTitleBar()
        {
            Panel titlePanel = new Panel
            {
                Size = new Size(780, 50),
                Location = new Point(0, 0),
                BackColor = primaryColor
            };

            Label titleLabel = new Label
            {
                Text = "🍅 Pomodoro Timer",
                Location = new Point(20, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White
            };

            btnClose = CreateTitleButton("✕", 730, accentColor);
            btnMinimize = CreateTitleButton("－", 690, warningColor);

            btnClose.Click += (s, e) => this.Close();
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            titlePanel.Controls.AddRange(new Control[] { titleLabel, btnClose, btnMinimize });
            titlePanel.Paint += (s, e) => DrawRoundedPanelTop(e.Graphics, titlePanel, 15);

            // 讓標題欄可以拖拽視窗
            titlePanel.MouseDown += TitlePanel_MouseDown;
            titleLabel.MouseDown += TitlePanel_MouseDown;

            mainPanel.Controls.Add(titlePanel);
        }

        private Button CreateTitleButton(string text, int x, Color color)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(x, 10),
                Size = new Size(30, 30),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private Panel CreateModernPanel(int x, int y, int width, int height, string title)
        {
            Panel panel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.None
            };

            Label titleLabel = new Label
            {
                Text = title,
                Location = new Point(20, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = textColor
            };

            panel.Controls.Add(titleLabel);
            panel.Paint += (s, e) => DrawRoundedPanel(e.Graphics, panel, 10);

            return panel;
        }

        private void SetupWorkControls()
        {
            lblWorkTime = new Label
            {
                Text = FormatTime(defaultWorkTime),
                Location = new Point(20, 60),
                Size = new Size(290, 60),
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = primaryColor,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            lblWorkInput = new Label
            {
                Text = "工作時間（分鐘）",
                Location = new Point(20, 140),
                AutoSize = true,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10F)
            };

            numWorkMinutes = new NumericUpDown
            {
                Location = new Point(200, 135),
                Width = 100,
                Height = 30,
                Minimum = 1,
                Maximum = 240,
                Value = 30,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnStartWork = new RoundedButton("開始工作", 20, 180, 90, 40, successColor);
            btnPauseWork = new RoundedButton("暫停", 120, 180, 90, 40, warningColor);
            btnStopWork = new RoundedButton("停止", 220, 180, 90, 40, accentColor);

            // 初始禁用暫停和停止按鈕
            btnPauseWork.Enabled = false;
            btnStopWork.Enabled = false;
            btnPauseWork.BackColor = Color.Gray;
            btnStopWork.BackColor = Color.Gray;

            btnStartWork.Click += (s, e) => StartWork();
            btnPauseWork.Click += (s, e) => PauseWork();
            btnStopWork.Click += (s, e) => StopWork();

            workPanel.Controls.AddRange(new Control[] {
                lblWorkTime, lblWorkInput, numWorkMinutes,
                btnStartWork, btnPauseWork, btnStopWork
            });
        }

        private void SetupBreakControls()
        {
            lblBreakTime = new Label
            {
                Text = FormatTime(defaultBreakTime),
                Location = new Point(20, 60),
                Size = new Size(290, 60),
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = successColor,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            lblBreakInput = new Label
            {
                Text = "休息時間（分鐘）",
                Location = new Point(20, 140),
                AutoSize = true,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10F)
            };

            numBreakMinutes = new NumericUpDown
            {
                Location = new Point(200, 135),
                Width = 100,
                Height = 30,
                Minimum = 1,
                Maximum = 120,
                Value = 10,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnStartBreak = new RoundedButton("開始休息", 20, 180, 90, 40, primaryColor);
            btnPauseBreak = new RoundedButton("暫停", 120, 180, 90, 40, warningColor);
            btnStopBreak = new RoundedButton("停止", 220, 180, 90, 40, accentColor);

            // 初始禁用暫停和停止按鈕
            btnPauseBreak.Enabled = false;
            btnStopBreak.Enabled = false;
            btnPauseBreak.BackColor = Color.Gray;
            btnStopBreak.BackColor = Color.Gray;

            btnStartBreak.Click += (s, e) => StartBreak();
            btnPauseBreak.Click += (s, e) => PauseBreak();
            btnStopBreak.Click += (s, e) => StopBreak();

            breakPanel.Controls.AddRange(new Control[] {
                lblBreakTime, lblBreakInput, numBreakMinutes,
                btnStartBreak, btnPauseBreak, btnStopBreak
            });
        }

        private void SetupMuteControl()
        {
            chkMute = new CheckBox
            {
                Text = "靜音",
                Location = new Point(252, 240),  // 移動到休息面板的右下方
                Size = new Size(70, 30),
                Font = new Font("Segoe UI", 10F),
                ForeColor = textColor,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            chkMute.CheckedChanged += (s, e) =>
            {
                isMuted = chkMute.Checked;
            };

            breakPanel.Controls.Add(chkMute);  // 改為加入到breakPanel
        }

        private void PlaySound(SoundPlayer sound)
        {
            if (!isMuted && sound != null)
            {
                sound.Play();
            }
        }

        // 自定義圓角按鈕類別
        public class RoundedButton : Button
        {
            public RoundedButton(string text, int x, int y, int width, int height, Color backColor)
            {
                this.Text = text;
                this.Location = new Point(x, y);
                this.Size = new Size(width, height);
                this.BackColor = backColor;
                this.ForeColor = Color.White;
                this.FlatStyle = FlatStyle.Flat;
                this.FlatAppearance.BorderSize = 0;
                this.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                this.Cursor = Cursors.Hand;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                GraphicsPath path = new GraphicsPath();
                path.AddRoundedRectangle(new Rectangle(0, 0, Width, Height), 8);
                this.Region = new Region(path);
                base.OnPaint(e);
            }
        }

        // 繪製圓角面板
        private void DrawRoundedPanel(Graphics g, Control panel, int radius)
        {
            Rectangle rect = new Rectangle(0, 0, panel.Width, panel.Height);
            GraphicsPath path = new GraphicsPath();
            path.AddRoundedRectangle(rect, radius);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillPath(new SolidBrush(panel.BackColor), path);
            g.DrawPath(new Pen(Color.FromArgb(220, 220, 220), 1), path);
        }

        private void DrawRoundedPanelTop(Graphics g, Control panel, int radius)
        {
            Rectangle rect = new Rectangle(0, 0, panel.Width, panel.Height);
            GraphicsPath path = new GraphicsPath();
            path.AddRoundedRectangleTop(rect, radius);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillPath(new SolidBrush(panel.BackColor), path);
        }

        // 拖拽視窗功能
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private void TitlePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private void InitializeTimers()
        {
            workTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            breakTimer = new System.Windows.Forms.Timer { Interval = 1000 };

            workTimer.Tick += (s, e) => {
                if (--workTimeRemaining <= 0)
                {
                    workTimer.Stop();
                    PlaySound(workEndSound);
                    ShowBreakNotification();
                    workTimeRemaining = defaultWorkTime;
                    lblWorkTime.Text = FormatTime(workTimeRemaining);

                    // 啟用開始按鈕，禁用暫停和停止按鈕
                    btnStartWork.Enabled = true;
                    btnStartWork.BackColor = successColor;
                    btnPauseWork.Enabled = false;
                    btnStopWork.Enabled = false;
                    btnPauseWork.BackColor = Color.Gray;
                    btnStopWork.BackColor = Color.Gray;
                }
                else
                {
                    lblWorkTime.Text = FormatTime(workTimeRemaining);
                }
            };

            breakTimer.Tick += (s, e) => {
                if (--breakTimeRemaining <= 0)
                {
                    breakTimer.Stop();
                    PlaySound(breakEndSound);
                    ShowWorkNotification();
                    breakTimeRemaining = defaultBreakTime;
                    lblBreakTime.Text = FormatTime(breakTimeRemaining);

                    // 啟用開始按鈕，禁用暫停和停止按鈕
                    btnStartBreak.Enabled = true;
                    btnStartBreak.BackColor = primaryColor;
                    btnPauseBreak.Enabled = false;
                    btnStopBreak.Enabled = false;
                    btnPauseBreak.BackColor = Color.Gray;
                    btnStopBreak.BackColor = Color.Gray;
                }
                else
                {
                    lblBreakTime.Text = FormatTime(breakTimeRemaining);
                }
            };
        }

        private string FormatTime(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }

        private void ShowBreakNotification()
        {
            var popup = new PopupForm("工作時間結束，要休息了嗎？", () => StartBreak());
            popup.Location = new Point(Screen.PrimaryScreen.WorkingArea.Right - popup.Width - 20,
                                       Screen.PrimaryScreen.WorkingArea.Bottom - popup.Height - 20);
            popup.Show();
        }

        private void ShowWorkNotification()
        {
            var popup = new PopupForm("休息時間結束，要開始工作了嗎？", () => StartWork());
            popup.Location = new Point(Screen.PrimaryScreen.WorkingArea.Right - popup.Width - 20,
                                       Screen.PrimaryScreen.WorkingArea.Bottom - popup.Height - 20);
            popup.Show();
        }

        private void StartWork()
        {
            defaultWorkTime = (int)numWorkMinutes.Value * 60;
            workTimeRemaining = defaultWorkTime;
            workTimer.Start();
            isWorkPaused = false;
            btnPauseWork.Text = "暫停";
            PlaySound(startWorkSound);

            // 禁用開始按鈕，啟用暫停和停止按鈕
            btnStartWork.Enabled = false;
            btnStartWork.BackColor = Color.Gray;
            btnPauseWork.Enabled = true;
            btnStopWork.Enabled = true;
            btnPauseWork.BackColor = warningColor;
            btnStopWork.BackColor = accentColor;
        }

        private void PauseWork()
        {
            if (isWorkPaused)
            {
                workTimer.Start();
                btnPauseWork.Text = "暫停";
                PlaySound(resumeSound);
            }
            else
            {
                workTimer.Stop();
                btnPauseWork.Text = "繼續";
                PlaySound(pauseSound);
            }
            isWorkPaused = !isWorkPaused;
        }

        private void StopWork()
        {
            workTimer.Stop();
            isWorkPaused = false;
            btnPauseWork.Text = "暫停";
            workTimeRemaining = defaultWorkTime;
            lblWorkTime.Text = FormatTime(workTimeRemaining);
            PlaySound(stopSound);

            // 啟用開始按鈕，禁用暫停和停止按鈕
            btnStartWork.Enabled = true;
            btnStartWork.BackColor = successColor;
            btnPauseWork.Enabled = false;
            btnStopWork.Enabled = false;
            btnPauseWork.BackColor = Color.Gray;
            btnStopWork.BackColor = Color.Gray;
        }

        private void StartBreak()
        {
            defaultBreakTime = (int)numBreakMinutes.Value * 60;
            breakTimeRemaining = defaultBreakTime;
            breakTimer.Start();
            isBreakPaused = false;
            btnPauseBreak.Text = "暫停";
            PlaySound(startBreakSound);

            // 禁用開始按鈕，啟用暫停和停止按鈕
            btnStartBreak.Enabled = false;
            btnStartBreak.BackColor = Color.Gray;
            btnPauseBreak.Enabled = true;
            btnStopBreak.Enabled = true;
            btnPauseBreak.BackColor = warningColor;
            btnStopBreak.BackColor = accentColor;
        }

        private void PauseBreak()
        {
            if (isBreakPaused)
            {
                breakTimer.Start();
                btnPauseBreak.Text = "暫停";
                PlaySound(resumeSound);
            }
            else
            {
                breakTimer.Stop();
                btnPauseBreak.Text = "繼續";
                PlaySound(pauseSound);
            }
            isBreakPaused = !isBreakPaused;
        }

        private void StopBreak()
        {
            breakTimer.Stop();
            isBreakPaused = false;
            btnPauseBreak.Text = "暫停";
            breakTimeRemaining = defaultBreakTime;
            lblBreakTime.Text = FormatTime(breakTimeRemaining);
            PlaySound(stopSound);

            // 啟用開始按鈕，禁用暫停和停止按鈕
            btnStartBreak.Enabled = true;
            btnStartBreak.BackColor = primaryColor;
            btnPauseBreak.Enabled = false;
            btnStopBreak.Enabled = false;
            btnPauseBreak.BackColor = Color.Gray;
            btnStopBreak.BackColor = Color.Gray;
        }
    }

    // GraphicsPath 擴充功能
    public static class GraphicsPathExtensions
    {
        public static void AddRoundedRectangle(this GraphicsPath path, Rectangle rect, int radius)
        {
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
        }

        public static void AddRoundedRectangleTop(this GraphicsPath path, Rectangle rect, int radius)
        {
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
            path.CloseFigure();
        }
    }
}