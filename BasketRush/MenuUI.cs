using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml.Linq;

namespace BasketballGame
{
    public static class MenuUI
    {
        public static Label CreateTitle(string text, Rectangle bounds) => new Label
        {
            Text = text,
            Font = new Font("Impact", 85, FontStyle.Bold),
            ForeColor = Color.Orange,
            TextAlign = ContentAlignment.MiddleRight,
            Size = new Size(800, 180),
            Location = new Point(bounds.Width - 850, (bounds.Height / 2) - 400),
            BackColor = Color.Transparent
        };

        public static Button CreateMenuButton(string text, int yOffset, Rectangle bounds) => new NeonButton
        {
            Text = text,
            Size = new Size(380, 75),
            Location = new Point(180, bounds.Height - 330 + yOffset)
        };

        public static BackArrowButton CreateBackButton(Rectangle bounds, Action onClick)
        {
            BackArrowButton btn = new BackArrowButton
            {
                Location = new Point(25, 25),
                Visible = false
            };

            btn.Click += (s, e) => onClick();
            return btn;
        }


        public static List<Panel> CreateSelectionGrid(string[] files, string[] names, Rectangle bounds, Action<string> onArenaClick)
        {
            var panels = new List<Panel>();
            int columns = 4, spacingX = 45, spacingY = 110;
            var availableWidth = (int)(bounds.Width * 0.85);
            var imgWidth = (availableWidth - (spacingX * (columns - 1))) / columns;
            var imgHeight = (int)(imgWidth * 0.5625);
            var startX = (bounds.Width - ((imgWidth * columns) + (spacingX * (columns - 1)))) / 2;
            var startY = (bounds.Height / 2) - 130;

            for (var i = 0; i < files.Length; i++)
            {
                var path = files[i];
                var panel = new Panel
                {
                    Size = new Size(imgWidth, imgHeight + 100),
                    Location = new Point(startX + (i % columns) * (imgWidth + spacingX), startY + (i / columns) * (imgHeight + spacingY + 40)),
                    Visible = false,
                    BackColor = Color.Transparent
                };

                var btn = new Button
                {
                    Size = new Size(imgWidth, imgHeight),
                    BackgroundImageLayout = ImageLayout.Stretch,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;

                btn.MouseEnter += (s, e) => {
                    btn.FlatAppearance.BorderSize = 3;
                    btn.FlatAppearance.BorderColor = Color.Orange;
                    btn.Size = new Size(imgWidth + 10, imgHeight + 10);
                    btn.Location = new Point(-5, -3);
                };

                btn.MouseLeave += (s, e) => {
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Size = new Size(imgWidth, imgHeight);
                    btn.Location = new Point(0, 0);
                };

                btn.Click += (s, e) => onArenaClick(path);
                try { btn.BackgroundImage = Image.FromFile(path); }
                catch { btn.BackColor = Color.FromArgb(40, 40, 55); }

                panel.Controls.Add(btn);
                panel.Controls.Add(new Label
                {
                    Text = names[i].ToUpper(),
                    Size = new Size(imgWidth, 60),
                    Location = new Point(0, imgHeight + 25),
                    ForeColor = Color.AntiqueWhite,
                    TextAlign = ContentAlignment.TopCenter,
                    Font = new Font("Impact", 18),
                    BackColor = Color.Transparent
                });
                panels.Add(panel);
            }
            return panels;
        }

        public static Panel CreateGameOverPanel(Rectangle bounds, Action onRestart, Action onExitToMenu)
        {
            var panel = new BufferedPanel
            {
                Size = new Size(500, 450),
                Location = new Point(bounds.Width / 2 - 250, bounds.Height / 2 - 225),
                BackColor = Color.FromArgb(240, 20, 20, 30),
                Visible = false
            };

            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var br = new SolidBrush(Color.FromArgb(230, 15, 15, 25)))
                    g.FillRoundedRectangle(br, new Rectangle(0, 0, panel.Width - 1, panel.Height - 1), 15);

                using (var p = new Pen(Color.Orange, 4f))
                    g.DrawRoundedRectangle(p, new Rectangle(2, 2, panel.Width - 5, panel.Height - 5), 15);

                var scoreLbl = panel.Controls.Find("FinalScore", true).FirstOrDefault() as Label;
                if (scoreLbl != null && int.TryParse(scoreLbl.Text, out int score))
                {
                    var pixelSize = 10;
                    var s1 = (score / 10) % 10;
                    var s2 = score % 10;
                    var startX = (panel.Width - (pixelSize * 6 * 2)) / 2;
                    MainForm.DrawPixelDigit(g, s1, startX, 60, pixelSize, Brushes.Orange);
                    MainForm.DrawPixelDigit(g, s2, startX + (pixelSize * 6), 60, pixelSize, Brushes.Orange);
                }
            };

            var btnRestart = new NeonButton { Text = "ИГРАТЬ СНОВА", Size = new Size(320, 70), Location = new Point(90, 230) };
            var btnMenu = new NeonButton { Text = "В ГЛАВНОЕ МЕНЮ", Size = new Size(320, 70), Location = new Point(90, 320) };

            btnRestart.Click += (s, e) => onRestart();
            btnMenu.Click += (s, e) => onExitToMenu();

            panel.Controls.Add(new Label { Name = "FinalScore", Text = "0", Visible = false });
            panel.Controls.Add(btnRestart); panel.Controls.Add(btnMenu);
            return panel;
        }

        public static Panel CreateShopPanel(Rectangle bounds, Action onBack)
        {
            var shop = new BufferedPanel
            {
                Size = bounds.Size,
                BackColor = Color.FromArgb(240, 20, 20, 30),
                Visible = false
            };

            shop.Controls.Add(new Label
            {
                Text = "МАГАЗИН",
                Font = new Font("Impact", 60),
                ForeColor = Color.Orange,
                Size = new Size(bounds.Width, 100),
                Location = new Point(0, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            });

            AddShopSection(shop, new[] { "ballSprite-1.png", "ballSprite-2.png", "ballSprite-3.png" }, new[] { "Классический", "Черный", "Серый" }, "МЯЧИ", bounds, 200);
            AddShopSection(shop, new[] { "Lebron.png", "ChaunceyBillups.png", "JarrettAllen.png" }, new[] { "Леброн", "Чонси", "Джарретт" }, "ИГРОКИ", bounds, 600);

            var back = new BackArrowButton { Location = new Point(25, 25) };
            back.Click += (s, e) => onBack();
            shop.Controls.Add(back);
            back.BringToFront();

            return shop;
        }

        private static void AddShopSection(Panel parent, string[] files, string[] names, string cat, Rectangle bounds, int y)
        {
            int w = 220, h = 320, sp = 40;
            int totalW = (w * 3) + (sp * 2), x = (bounds.Width - totalW) / 2;
            parent.Controls.Add(new Label { Text = cat, Font = new Font("Impact", 26), ForeColor = Color.White, Size = new Size(totalW, 40), Location = new Point(x, y - 60), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent });

            for (var i = 0; i < 3; i++)
            {
                var item = new ShopItemPanel(files[i], names[i], w, h) { Location = new Point(x + i * (w + sp), y) };
                item.ItemClicked += (clicked) =>
                {
                    foreach (var c in parent.Controls)
                        if (c is ShopItemPanel sip)
                            sip.SetSelected(false);
                    clicked.SetSelected(true);
                };
                parent.Controls.Add(item);
            }
        }
    }


    public class NeonButton : Button
    {
        private float alpha = 0;
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 16 };

        public NeonButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Cursor = Cursors.Hand;
            Font = new Font("Impact", 20);
            ForeColor = Color.White;
            timer.Tick += (s, e) => {
                alpha += ((IsMouseOver() ? 1f : 0f) - alpha) * 0.2f;
                Invalidate();
            };
        }

        private bool IsMouseOver() => !IsDisposed && !Disposing && ClientRectangle.Contains(PointToClient(Cursor.Position));

        protected override void OnMouseEnter(EventArgs e)
        {
            timer.Start();
            base.OnMouseEnter(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);

            if (alpha > 0.01f)
            {
                using (var br = new LinearGradientBrush(r, Color.Transparent, Color.Transparent, 90f))
                {
                    ColorBlend cb = new ColorBlend();
                    cb.Positions = new[] { 0f, 0.6f, 1f };
                    cb.Colors = new[] {

                Interpolate(Color.FromArgb(210, 8, 12, 28), Color.FromArgb(255, 140, 0), alpha),
                Interpolate(Color.FromArgb(210, 18, 26, 52), Color.Yellow, alpha),
                Interpolate(Color.FromArgb(210, 8, 12, 28), Color.FromArgb(255, 100, 0), alpha)
            };
                    br.InterpolationColors = cb;
                    g.FillRoundedRectangle(br, r, 8);
                }
            }
            else
            {
                using (var br = new SolidBrush(Color.FromArgb(210, 8, 12, 28)))
                    g.FillRoundedRectangle(br, r, 8);
            }

            using (var p = new Pen(Interpolate(Color.FromArgb(100, 60, 60, 60), Color.DarkOrange, alpha), 2.5f))
                g.DrawRoundedRectangle(p, r, 8);

            var textColor = Interpolate(Color.White, Color.Black, alpha);
            TextRenderer.DrawText(g, Text, Font, r, textColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }



        private Color Interpolate(Color a, Color b, float t) =>
            Color.FromArgb((int)(a.A + (b.A - a.A) * t), (int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
    }

    public class BackArrowButton : Button
    {
        private float arrow = 0;
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 16 };


        public BackArrowButton()
        {
            Size = new Size(160, 60);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Cursor = Cursors.Hand;
            timer.Tick += (s, e) => {
                if (!IsDisposed && !Disposing)
                    arrow += ((ClientRectangle.Contains(PointToClient(Cursor.Position)) ? 1f : 0f) - arrow) * 0.2f;
                Invalidate();
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer?.Stop();
                timer?.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            timer.Start();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            timer.Stop();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var br = new SolidBrush(Color.FromArgb((int)(160 + 60 * this.arrow), 8, 12, 28))) g.FillRoundedRectangle(br, r, 8);
            if (this.arrow > 0.01f)
                using (var h = new SolidBrush(Color.FromArgb((int)(100 * this.arrow), 255, 140, 0))) g.FillRoundedRectangle(h, r, 8);

            var cx = Width / 5;
            var cy = Height / 2;
            var arrowLength = (int)(Width * 0.5f);
            var arrowWidth = (int)(Height * 0.4f);

            PointF[] arrow = {
                 new PointF(cx, cy - arrowWidth/2),
                 new PointF(cx - arrowWidth/2, cy),
                 new PointF(cx, cy + arrowWidth/2),
                 new PointF(cx, cy + arrowWidth/4),
                 new PointF(cx + arrowLength, cy + arrowWidth/4),
                 new PointF(cx + arrowLength, cy - arrowWidth/4),
                 new PointF(cx, cy - arrowWidth/4)
             };
            using (var ab = new SolidBrush(Color.FromArgb((int)(200 + 55 * this.arrow), (int)(200 + 55 * this.arrow), (int)(50 + 80 * this.arrow)))) g.FillPolygon(ab, arrow);
            using (var p = new Pen(Color.FromArgb(180, 212, 160, 30), 1.5f)) g.DrawRoundedRectangle(p, r, 8);
        }
    }

    public class ShopItemPanel : Panel
    {
        public bool IsSelected { get; private set; }
        public string ItemPath { get; private set; }
        private float panel = 0;
        private int direction = 1;
        private float hover = 0;
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 25 };
        private Image img;
        private string txt;
        public event Action<ShopItemPanel> ItemClicked;


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer?.Stop();
                timer?.Dispose();
                img?.Dispose();
            }
            base.Dispose(disposing);
        }


        public ShopItemPanel(string path, string label, int w, int h)
        {
            Size = new Size(w, h);
            txt = label;
            ItemPath = path;

            try { img = Image.FromFile(path); }
            catch { }

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;

            timer.Interval = 16;
            timer.Tick += (s, e) => {
                var needRedraw = false;

                if (IsSelected)
                {
                    panel += 0.05f * direction;
                    if (panel >= 1 || panel <= 0)
                        direction *= -1;
                    needRedraw = true;
                }
                else if (panel != 0)
                {
                    panel = 0;
                    needRedraw = true;
                }

                if (!IsDisposed && !Disposing)
                {
                    var isHovered = ClientRectangle.Contains(PointToClient(Cursor.Position));
                    var targetH = isHovered ? 1f : 0f;

                    if (Math.Abs(hover - targetH) > 0.01f)
                    {
                        hover += (targetH - hover) * 0.2f;
                        needRedraw = true;
                    }
                }

                if (needRedraw && !IsDisposed && !Disposing)
                    Invalidate();
            };

            timer.Start();
            this.Click += (s, e) => ItemClicked?.Invoke(this);

        }


        public void SetSelected(bool selected) => IsSelected = selected;

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var br = new SolidBrush(Color.FromArgb(220, 15, 15, 30)))
                g.FillRoundedRectangle(br, r, 12);

            if (IsSelected)
            {

                using (var p = new Pen(Color.Orange, 3f)) g.DrawRoundedRectangle(p, r, 12);
                Rectangle badgeRect = new Rectangle(Width / 2 - 55, Height - 35, 110, 25);
                using (var b = new SolidBrush(Color.Orange))
                    g.FillRoundedRectangle(b, badgeRect, 5);
                TextRenderer.DrawText(g, "ВЫБРАНО", new Font("Impact", 10),
                    badgeRect, Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            if (img != null)
            {
                var reservedBottom = 80;
                Rectangle imgArea = new Rectangle(10, 10, Width - 20, Height - reservedBottom - 10);
                var ratio = Math.Min((float)imgArea.Width / img.Width, (float)imgArea.Height / img.Height);
                var nw = (int)(img.Width * ratio);
                var nh = (int)(img.Height * ratio);
                g.DrawImage(img, imgArea.X + (imgArea.Width - nw) / 2, imgArea.Y + (imgArea.Height - nh) / 2, nw, nh);
            }

            Rectangle nameRect = new Rectangle(0, Height - 70, Width, 30);
            TextRenderer.DrawText(g, txt.ToUpper(), new Font("Impact", 14),
                nameRect, IsSelected ? Color.Gold : Color.Gray, TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
        }
    }

    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush b, Rectangle r, int rad)
        {
            using (var p = GetPath(r, rad)) g.FillPath(b, p);
        }

        public static void DrawRoundedRectangle(this Graphics g, Pen p, Rectangle r, int rad)
        {
            using (var path = GetPath(r, rad)) g.DrawPath(p, path);
        }

        private static GraphicsPath GetPath(Rectangle r, int rad)
        {
            var path = new GraphicsPath(); int d = rad * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90); path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure(); return path;
        }
    }

    public class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
    }
}