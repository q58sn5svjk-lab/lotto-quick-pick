using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace DaLeTou
{
    public class LottoForm : Form
    {
        // All 7 ball labels (indices 0-4 = front, 5-6 = back)
        private Label[] ballLabels = new Label[7];
        private Button generateButton;
        private Label timeLabel;
        // Store numbers separately — Label.Text is left empty so its built-in
        // text rendering (which fires BEFORE the Paint event) doesn't get
        // overwritten by our custom circle drawing.
        private string[] ballTexts = new string[7];

        public LottoForm()
        {
            InitializeComponent();
            GenerateNumbers();
        }

        private void InitializeComponent()
        {
            this.Text = "大乐透 随机选号器";
            this.Size = new Size(640, 330);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.BackColor = Color.FromArgb(0x1a, 0x1a, 0x2e);
            this.Font = new Font("Microsoft YaHei UI", 12F);

            // ── Title ──
            var titleLabel = new Label
            {
                Text = "大乐透 随机选号器",
                Font = new Font("Microsoft YaHei UI", 18F, FontStyle.Bold),
                ForeColor = Color.Gold,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(600, 40),
                Location = new Point(20, 15)
            };

            // ── Subtitle: rules ──
            var subtitleLabel = new Label
            {
                Text = "前区 (1-35) 选 5 个  +  后区 (1-12) 选 2 个",
                Font = new Font("Microsoft YaHei UI", 11F),
                ForeColor = Color.LightGray,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(600, 25),
                Location = new Point(20, 58)
            };

            // ── 7 balls in ONE row, centered ──
            int ballSize = 62;
            int spacing = 76;                // center-to-center
            int totalWidth = spacing * 6 + ballSize;
            int startX = (this.ClientSize.Width - totalWidth) / 2;
            int ballY = 95;

            for (int i = 0; i < 5; i++)
                ballLabels[i] = CreateBallLabel(startX + i * spacing, ballY, Color.DodgerBlue, i);

            for (int i = 0; i < 2; i++)
                ballLabels[5 + i] = CreateBallLabel(startX + (5 + i) * spacing, ballY, Color.Crimson, 5 + i);

            // ── Small zone labels under the last front ball and last back ball ──
            var zoneFront = new Label
            {
                Text = "前区",
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                ForeColor = Color.LightSteelBlue,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(62, 20),
                Location = new Point(startX + 4 * spacing, ballY + ballSize + 4)
            };
            var zoneBack = new Label
            {
                Text = "后区",
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                ForeColor = Color.IndianRed,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(62, 20),
                Location = new Point(startX + 6 * spacing, ballY + ballSize + 4)
            };

            // ── Generate button (NO custom Paint — standard FlatStyle) ──
            generateButton = new Button
            {
                Text = "随 机 选 号",
                Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold),
                Size = new Size(180, 40),
                Location = new Point((this.ClientSize.Width - 180) / 2, 210),
                BackColor = Color.FromArgb(0xff, 0x8c, 0x00),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                UseVisualStyleBackColor = false,
                Cursor = Cursors.Hand
            };
            generateButton.Click += (s, e) => GenerateNumbers();

            // ── Time label ──
            timeLabel = new Label
            {
                Font = new Font("Microsoft YaHei UI", 9F),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(600, 20),
                Location = new Point(20, 270)
            };

            // Add controls
            this.Controls.Add(titleLabel);
            this.Controls.Add(subtitleLabel);
            foreach (var lbl in ballLabels) this.Controls.Add(lbl);
            this.Controls.Add(zoneFront);
            this.Controls.Add(zoneBack);
            this.Controls.Add(generateButton);
            this.Controls.Add(timeLabel);
        }

        /// <summary>
        /// Creates a ball-style Label that draws everything (circle + text)
        /// in the Paint event. Label.Text is intentionally left empty because
        /// Label's built-in text rendering fires BEFORE the Paint event and
        /// would be overwritten by our circle drawing.
        /// </summary>
        private Label CreateBallLabel(int x, int y, Color baseColor, int index)
        {
            var lbl = new Label
            {
                Size = new Size(62, 62),
                Location = new Point(x, y),
                BackColor = Color.Transparent,
                Text = "",
                Tag = index
            };

            lbl.Paint += (sender, e) =>
            {
                var ctrl = (Label)sender;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                int w = ctrl.Width;
                int h = ctrl.Height;

                // Shadow
                using (var shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                    e.Graphics.FillEllipse(shadowBrush, 3, 5, w - 6, h - 6);

                // Main circle
                using (var brush = new SolidBrush(baseColor))
                    e.Graphics.FillEllipse(brush, 2, 2, w - 5, h - 5);

                using (var pen = new Pen(Color.FromArgb(180, 255, 255, 255), 2))
                    e.Graphics.DrawEllipse(pen, 2, 2, w - 5, h - 5);

                // Highlight (top-left shine)
                using (var shineBrush = new SolidBrush(Color.FromArgb(70, 255, 255, 255)))
                    e.Graphics.FillEllipse(shineBrush, 10, 6, 18, 12);

                // Draw number text (from our array, not Label.Text)
                int idx = (int)ctrl.Tag;
                string text = ballTexts[idx];
                if (!string.IsNullOrEmpty(text))
                {
                    using (var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    })
                    using (var textBrush = new SolidBrush(Color.White))
                    using (var textFont = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold))
                    {
                        e.Graphics.DrawString(text, textFont, textBrush,
                            new RectangleF(2, 2, w - 5, h - 5), sf);
                    }
                }
            };

            return lbl;
        }

        private void GenerateNumbers()
        {
            var (front, back) = LottoGenerator.Generate();

            for (int i = 0; i < 5; i++)
            {
                ballTexts[i] = front[i].ToString("D2");
                ballLabels[i].Invalidate();
            }
            for (int i = 0; i < 2; i++)
            {
                ballTexts[5 + i] = back[i].ToString("D2");
                ballLabels[5 + i].Invalidate();
            }

            timeLabel.Text = $"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
    }

    /// <summary>
    /// Cryptographically secure random number generator (CSPRNG).
    /// Uses System.Security.Cryptography.RandomNumberGenerator
    /// instead of System.Random, matching the "加密级伪随机" standard
    /// referenced in the user's material.
    /// </summary>
    public static class CryptoRandom
    {
        public static int Next(int min, int max)
        {
            uint range = (uint)(max - min);
            // Rejection sampling to eliminate modulo bias
            uint maxAcceptable = uint.MaxValue - (uint.MaxValue % range) - 1;

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[4];
                while (true)
                {
                    rng.GetBytes(bytes);
                    uint value = BitConverter.ToUInt32(bytes, 0);
                    if (value <= maxAcceptable)
                    {
                        return (int)(min + value % range);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generates lottery numbers matching 大乐透 rules:
    /// - Front area: 5 unique numbers from 1-35
    /// - Back area:  2 unique numbers from 1-12
    /// - Front and back may have overlapping numbers
    /// </summary>
    public static class LottoGenerator
    {
        public static (int[] front, int[] back) Generate()
        {
            // Front: 5 unique numbers from 1-35
            var frontSet = new HashSet<int>();
            while (frontSet.Count < 5)
                frontSet.Add(CryptoRandom.Next(1, 36));

            var front = new List<int>(frontSet);
            front.Sort();

            // Back: 2 unique numbers from 1-12
            var backSet = new HashSet<int>();
            while (backSet.Count < 2)
                backSet.Add(CryptoRandom.Next(1, 13));

            var back = new List<int>(backSet);
            back.Sort();

            return (front.ToArray(), back.ToArray());
        }
    }
}
