using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace PomodoroSharp
{
    public class PopupForm : Form
    {
        private System.Windows.Forms.Timer fadeTimer;
        private System.Windows.Forms.Timer autoCloseTimer;
        private System.Windows.Forms.Timer slideTimer;
        private float opacityStep = 0.02f;
        private int totalDuration = 60000; // 1 minute
        private int slidePosition;
        private int targetPosition;

        // 現代化顏色配置（與主程式一致）
        private Color primaryColor = Color.FromArgb(74, 144, 226);
        private Color successColor = Color.FromArgb(46, 204, 113);
        private Color accentColor = Color.FromArgb(231, 76, 60);
        private Color backgroundColor = Color.White;
        private Color textColor = Color.FromArgb(44, 62, 80);
        private Color shadowColor = Color.FromArgb(50, 0, 0, 0);

        public PopupForm(string message, Action onYes)
        {
            InitializeModernPopup(message, onYes);
            SetupAnimation();
        }

        private void InitializeModernPopup(string message, Action onYes)
        {
            // 基本視窗設定
            this.StartPosition = FormStartPosition.Manual;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(400, 180);
            this.TopMost = true;
            this.BackColor = backgroundColor;
            this.Opacity = 0.0;
            this.ShowInTaskbar = false;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            // 設定初始位置（螢幕右下角外側）
            targetPosition = Screen.PrimaryScreen.WorkingArea.Bottom - this.Height - 20;
            slidePosition = Screen.PrimaryScreen.WorkingArea.Bottom + 50;
            this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Right - this.Width - 20, slidePosition);

            // 主容器面板
            Panel mainPanel = new Panel
            {
                Size = new Size(380, 160),
                Location = new Point(10, 10),
                BackColor = backgroundColor
            };
            mainPanel.Paint += (s, e) => DrawModernPanel(e.Graphics, mainPanel);

            // 圖示區域
            Label iconLabel = new Label
            {
                Text = "🔔",
                Font = new Font("Segoe UI", 24F),
                Size = new Size(60, 60),
                Location = new Point(20, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // 訊息標題
            Label titleLabel = new Label
            {
                Text = "計時提醒",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = textColor,
                Location = new Point(90, 20),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // 訊息內容
            Label messageLabel = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(127, 140, 141),
                Location = new Point(90, 45),
                Size = new Size(270, 40),
                BackColor = Color.Transparent,
                AutoSize = false
            };

            // 按鈕容器
            Panel buttonPanel = new Panel
            {
                Size = new Size(360, 50),
                Location = new Point(10, 100),
                BackColor = Color.Transparent
            };

            // 現代化按鈕
            var btnYes = CreateModernButton("開始", 180, 10, successColor);
            var btnNo = CreateModernButton("稍後", 260, 10, Color.FromArgb(149, 165, 166));

            btnYes.Click += (s, e) => { CloseWithAnimation(); onYes?.Invoke(); };
            btnNo.Click += (s, e) => { CloseWithAnimation(); };

            // 進度條（顯示剩餘時間）
            var progressBar = new ProgressBar
            {
                Location = new Point(20, 140),
                Size = new Size(340, 4),
                Style = ProgressBarStyle.Continuous,
                MarqueeAnimationSpeed = 0
            };

            // 設定進度條更新
            var progressTimer = new System.Windows.Forms.Timer { Interval = 100 };
            int progressValue = 100;
            progressTimer.Tick += (s, e) =>
            {
                progressValue -= 100000 / totalDuration; // 每100ms減少的百分比
                if (progressValue <= 0)
                {
                    progressValue = 0;
                    progressTimer.Stop();
                }
                progressBar.Value = Math.Max(0, progressValue);
            };
            progressTimer.Start();

            // 添加控制項
            buttonPanel.Controls.AddRange(new Control[] { btnYes, btnNo });
            mainPanel.Controls.AddRange(new Control[] {
                iconLabel, titleLabel, messageLabel, buttonPanel, progressBar
            });
            this.Controls.Add(mainPanel);

            // 設定自動關閉計時器
            autoCloseTimer = new System.Windows.Forms.Timer();
            autoCloseTimer.Interval = totalDuration;
            autoCloseTimer.Tick += (s, e) =>
            {
                autoCloseTimer.Stop();
                progressTimer.Stop();
                StartFadeOut();
            };
            autoCloseTimer.Start();

            // 設定淡出計時器
            fadeTimer = new System.Windows.Forms.Timer();
            fadeTimer.Interval = 50;
            fadeTimer.Tick += (s, e) =>
            {
                if (this.Opacity > 0)
                {
                    this.Opacity -= opacityStep;
                }
                else
                {
                    fadeTimer.Stop();
                    this.Close();
                }
            };

            // 添加陰影效果
            this.Paint += (s, e) => DrawShadow(e.Graphics);
        }

        private Button CreateModernButton(string text, int x, int y, Color backColor)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(70, 30),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderSize = 0;

            // 添加懸停效果
            button.MouseEnter += (s, e) =>
            {
                button.BackColor = ControlPaint.Light(backColor, 0.2f);
            };

            button.MouseLeave += (s, e) =>
            {
                button.BackColor = backColor;
            };

            // 圓角效果
            button.Paint += (s, e) =>
            {
                GraphicsPath path = new GraphicsPath();
                path.AddRoundedRectangle(new Rectangle(0, 0, button.Width, button.Height), 6);
                button.Region = new Region(path);
            };

            return button;
        }

        private void SetupAnimation()
        {
            // 滑入動畫
            slideTimer = new System.Windows.Forms.Timer { Interval = 20 };
            slideTimer.Tick += (s, e) =>
            {
                if (slidePosition > targetPosition)
                {
                    slidePosition -= 8;
                    this.Location = new Point(this.Location.X, slidePosition);

                    // 同時淡入
                    if (this.Opacity < 0.95)
                        this.Opacity += 0.05;
                }
                else
                {
                    slideTimer.Stop();
                    this.Location = new Point(this.Location.X, targetPosition);
                    this.Opacity = 0.95;
                }
            };

            // 顯示時啟動滑入動畫
            this.Shown += (s, e) => slideTimer.Start();
        }

        private void DrawModernPanel(Graphics g, Panel panel)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 主要背景
            Rectangle rect = new Rectangle(0, 0, panel.Width, panel.Height);
            GraphicsPath path = new GraphicsPath();
            path.AddRoundedRectangle(rect, 12);

            g.FillPath(new SolidBrush(backgroundColor), path);

            // 邊框
            using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
            {
                g.DrawPath(pen, path);
            }

            // 頂部裝飾條
            Rectangle topRect = new Rectangle(0, 0, panel.Width, 4);
            GraphicsPath topPath = new GraphicsPath();
            topPath.AddRoundedRectangleTop(topRect, 12);
            g.FillPath(new SolidBrush(primaryColor), topPath);
        }

        private void DrawShadow(Graphics g)
        {
            // 繪製陰影效果
            Rectangle shadowRect = new Rectangle(5, 5, this.Width - 10, this.Height - 10);
            GraphicsPath shadowPath = new GraphicsPath();
            shadowPath.AddRoundedRectangle(shadowRect, 12);

            using (PathGradientBrush brush = new PathGradientBrush(shadowPath))
            {
                brush.CenterColor = shadowColor;
                brush.SurroundColors = new Color[] { Color.Transparent };
                g.FillPath(brush, shadowPath);
            }
        }

        private void StartFadeOut()
        {
            fadeTimer.Start();
        }

        private void CloseWithAnimation()
        {
            autoCloseTimer.Stop();

            // 滑出動畫
            var slideOutTimer = new System.Windows.Forms.Timer { Interval = 20 };
            slideOutTimer.Tick += (s, e) =>
            {
                slidePosition += 12;
                this.Location = new Point(this.Location.X, slidePosition);
                this.Opacity -= 0.08;

                if (this.Opacity <= 0 || slidePosition > Screen.PrimaryScreen.WorkingArea.Bottom)
                {
                    slideOutTimer.Stop();
                    this.Close();
                }
            };
            slideOutTimer.Start();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80000; // WS_EX_LAYERED
                return cp;
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, width, height, specified);
        }
    }
}