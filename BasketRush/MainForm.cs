using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BasketballGame
{
    public partial class MainForm : Form
    {
        private GameState currentState = GameState.Menu; 
        private GameModel game;                          
        private System.Windows.Forms.Timer gameTimer;   

        private string lastArenaPath = "";               
        private string[] backgroundFiles = { "backgroundArena.png", "backgroundArena2.png", "backgroundArena3.png", "backgroundArena4.png", "backgroundArena5.png", "backgroundArena6.png", "backgroundArena7.png", "backgroundArena8.png" };
        private string[] arenaNames = { "Дворовой корт", "Крыша небоскреба", "Азиатский турнир", "Задний двор во Франции", "Пустыня", "Ночь в парке", "Профессиональная арена", "Пляж" };
        private string currentBallPath = "ballSprite-1.png";  
        private string currentPlayerPath = "player1.png";      
        private readonly Dictionary<string, string> playerSpritesMap = new Dictionary<string, string>
        {
            { "Lebron.png", "player1.png" },
            { "ChaunceyBillups.png", "player2.png" },
            { "JarrettAllen.png", "player3.png" }
        };

        private Image ballSprite;               
        private Label titleLabel;               
        private Button startBtn, setBtn, shopBtn, exitBtn; 
        private List<Panel> gridPanels;         
        private Panel arenaSelectionPanel;       
        private Panel gameOverPanel;             
        private Panel shopPanel;                 
        private Panel settingsPanel;             
        private BackArrowButton globalBackBtn;   
        private bool isDragging = false;         
        private PointF mouseCurrent;    

        // Пиксельные шрифты цифр 0–9 в виде битовых масок (5×7 пикселей)
        // Каждое число — строка, каждый бит — пиксель
        public static int[][] PixelDigits = new int[][]
        {
            new int[] { 0b01110, 0b10001, 0b10011, 0b10101, 0b11001, 0b10001, 0b01110 }, // 0
            new int[] { 0b00100, 0b01100, 0b00100, 0b00100, 0b00100, 0b00100, 0b01110 }, // 1
            new int[] { 0b01110, 0b10001, 0b00001, 0b00110, 0b01000, 0b10000, 0b11111 }, // 2
            new int[] { 0b11110, 0b00001, 0b00001, 0b01110, 0b00001, 0b00001, 0b11110 }, // 3
            new int[] { 0b00010, 0b00110, 0b01010, 0b10010, 0b11111, 0b00010, 0b00010 }, // 4
            new int[] { 0b11111, 0b10000, 0b10000, 0b11110, 0b00001, 0b00001, 0b11110 }, // 5
            new int[] { 0b01110, 0b10000, 0b10000, 0b11110, 0b10001, 0b10001, 0b01110 }, // 6
            new int[] { 0b11111, 0b00001, 0b00010, 0b00100, 0b01000, 0b01000, 0b01000 }, // 7
            new int[] { 0b01110, 0b10001, 0b10001, 0b01110, 0b10001, 0b10001, 0b01110 }, // 8
            new int[] { 0b01110, 0b10001, 0b10001, 0b01111, 0b00001, 0b00001, 0b01110 }, // 9
        };

        public MainForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;  
            this.BackColor = Color.FromArgb(15, 15, 25);
            Rectangle bounds = Screen.PrimaryScreen.Bounds;

            game = new GameModel(15, bounds.Width, bounds.Height); 
            gameTimer = new System.Windows.Forms.Timer { Interval = 20 }; 
            try { ballSprite = Image.FromFile("ballSprite-1.png"); }
            catch { }

            gameTimer.Tick += (s, e) => {
                game.Update(0.02f, bounds.Width, bounds.Height);
                if (game.IsGameOver) 
                    EndGame();
                this.Invalidate();
            };

            arenaSelectionPanel = new BufferedPanel
            {
                Size = bounds.Size,
                BackColor = Color.Black,
                Visible = false
            };

            this.Controls.Add(arenaSelectionPanel);

            // Создаём элементы главного меню 
            titleLabel = MenuUI.CreateTitle("", bounds);
            startBtn = MenuUI.CreateMenuButton("ИГРАТЬ", -150, bounds);
            setBtn = MenuUI.CreateMenuButton("НАСТРОЙКИ", -50, bounds);
            shopBtn = MenuUI.CreateMenuButton("МАГАЗИН", 50, bounds);
            exitBtn = MenuUI.CreateMenuButton("ВЫХОД", 150, bounds);

            startBtn.Click += (s, e) => ShowSelection();
            setBtn.Click += (s, e) => ShowSettings();
            shopBtn.Click += (s, e) => ShowShop();
            exitBtn.Click += (s, e) => Application.Exit();

            // Создаём сетку арен
            gridPanels = MenuUI.CreateSelectionGrid(backgroundFiles, arenaNames, bounds, StartGame);
            gridPanels.ForEach(p => arenaSelectionPanel.Controls.Add(p));

            gameOverPanel = MenuUI.CreateGameOverPanel(bounds, () => StartGame(lastArenaPath), BackToMenu);
            shopPanel = MenuUI.CreateShopPanel(bounds, BackToMenu);

            foreach (Control control in shopPanel.Controls)
            {
                if (control is ShopItemPanel itemPanel)
                    AttachShopItemClick(itemPanel);
            }
            settingsPanel = SettingsUI.CreateSettingsPanel(bounds, BackToMenu);

            // Кнопка «назад»
            globalBackBtn = MenuUI.CreateBackButton(this.ClientRectangle, BackToMenu);
            arenaSelectionPanel.Controls.Add(globalBackBtn);
            globalBackBtn.BringToFront();

            this.Controls.AddRange(new Control[] { titleLabel, startBtn, setBtn, shopBtn, exitBtn, gameOverPanel, shopPanel, settingsPanel, globalBackBtn });
            gridPanels.ForEach(p => this.Controls.Add(p));

            this.MouseDown += OnMouseDown;
            this.MouseMove += OnMouseMove;
            this.MouseUp += OnMouseUp;
            this.KeyDown += OnKeyDown;
        }

        // Выбор в магазине
        private void AttachShopItemClick(ShopItemPanel item)
        {
            item.ItemClicked += (clickedItem) => {
                var path = clickedItem.ItemPath;

                if (string.IsNullOrEmpty(path))
                    return;

                if (path.Contains("ball"))
                { 
                    currentBallPath = path;
                    try { ballSprite = Image.FromFile(currentBallPath); }
                    catch { }
                }
                else
                {
                    if (playerSpritesMap.TryGetValue(path, out var gameSpritePath))
                        currentPlayerPath = gameSpritePath;
                    else
                        currentPlayerPath = "player1.png";
                }
            };
        }

        // Выбор арены
        private void ShowSelection()
        {
            currentState = GameState.Selection;
            ToggleMenu(false); 
            titleLabel.Text = "ВЫБЕРИТЕ АРЕНУ";
            titleLabel.Size = new Size(Screen.PrimaryScreen.Bounds.Width, 150);
            titleLabel.Location = new Point(0, 50);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Visible = true;
            gridPanels.ForEach(p => { p.Visible = true; p.BringToFront(); });
            globalBackBtn.Visible = true;
            globalBackBtn.BringToFront();
            this.Invalidate();
        }

        private void ShowSettings()
        {
            ToggleMenu(false);
            settingsPanel.Visible = true;
            settingsPanel.BringToFront();
        }

        private void ShowShop()
        {
            ToggleMenu(false);
            shopPanel.Visible = true;
            shopPanel.BringToFront();
        }

        private void StartGame(string path)
        {
            lastArenaPath = path;
            currentState = GameState.Playing;
            gridPanels.ForEach(p => p.Visible = false);
            gameOverPanel.Visible = titleLabel.Visible = globalBackBtn.Visible = false;
            ToggleMenu(false);
            game = new GameModel(15, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            game.SetLevel(path);
            gameTimer.Start();
        }

        private void EndGame()
        {
            gameTimer.Stop();
            currentState = GameState.GameOver;
            var scoreLbl = gameOverPanel.Controls.Find("FinalScore", true).FirstOrDefault() as Label;
            if (scoreLbl != null)
                scoreLbl.Text = game.Score.ToString();

            gameOverPanel.Parent = this;
            gameOverPanel.BackColor = Color.FromArgb(240, 20, 20, 30);
            gameOverPanel.Visible = true;
            gameOverPanel.BringToFront();
            gameOverPanel.Invalidate(); 
        }

        private void BackToMenu()
        {
            currentState = GameState.Menu;
            gameOverPanel.Visible = shopPanel.Visible = settingsPanel.Visible = globalBackBtn.Visible = false;
            gridPanels.ForEach(p => p.Visible = false);

            titleLabel.Text = "";
            titleLabel.Font = new Font("Impact", 85, FontStyle.Bold);
            titleLabel.Size = new Size(800, 180);
            titleLabel.Location = new Point(Screen.PrimaryScreen.Bounds.Width - 850, (Screen.PrimaryScreen.Bounds.Height / 2) - 400);
            titleLabel.TextAlign = ContentAlignment.MiddleRight;
            ToggleMenu(true);
            this.Invalidate();
        }

        private void ToggleMenu(bool v) => startBtn.Visible = setBtn.Visible = exitBtn.Visible = shopBtn.Visible = titleLabel.Visible = v;

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (currentState == GameState.Menu)
            {
                Image menuBg = ResourceManager.GetImage("mainMenuBg.jpeg");
                if (menuBg != null)
                    g.DrawImage(menuBg, 0, 0, this.Width, this.Height);
            }

            if (currentState == GameState.Playing)
            {
                // Фон арены
                g.DrawImage(ResourceManager.GetImage(lastArenaPath), 0, 0, this.Width, this.Height);

                // Игрок
                Image playerSprite = ResourceManager.GetImage(currentPlayerPath);
                if (playerSprite is Bitmap bmp)
                    bmp.MakeTransparent(Color.White);

                var pWidth = 200;
                var pHeight = 295;
                var handOffsetX = pWidth * 0.87f;
                var handOffsetY = pHeight * 0.4f;
                var playerDrawX = game.PlayerPos.X - handOffsetX;
                var playerDrawY = game.PlayerPos.Y - handOffsetY;

                g.DrawImage(playerSprite, playerDrawX, playerDrawY, pWidth, pHeight);

                // Отрисовка линии прицела 
                if (isDragging)
                {
                    var dx = mouseCurrent.X - game.BallPos.X;
                    var dy = mouseCurrent.Y - game.BallPos.Y;
                    var dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    var maxDist = 500f;
                    if (dist > maxDist)
                    {
                        dx = (dx / dist) * maxDist;
                        dy = (dy / dist) * maxDist;
                    }

                    PointF v = new PointF(dx * 0.12f, dy * 0.12f);
                    PointF lastPoint = game.BallPos;
                    var passedApex = false; 
                    var pointsAfterApex = 0;

                    using (Pen trajPen = new Pen(Color.FromArgb(180, Color.White), 6))
                    {
                        trajPen.DashStyle = DashStyle.Dash;
                        for (var i = 1; i <= 60; i++)
                        {
                            var t = i * 0.12f;
                            PointF nextPoint = game.GetTrajectoryPoint(game.BallPos, v, t);

                            if (game.BoardRect.Contains((int)nextPoint.X, (int)nextPoint.Y) && i > 2)
                            {
                                g.DrawLine(trajPen, lastPoint, nextPoint);
                                break;
                            }
                            if (nextPoint.Y > lastPoint.Y && i > 1)
                                passedApex = true;

                            g.DrawLine(trajPen, lastPoint, nextPoint);
                            lastPoint = nextPoint;

                            if (passedApex && ++pointsAfterApex >= 2)
                                break;
                            if (lastPoint.Y > this.Height || lastPoint.X < 0 || lastPoint.X > this.Width)
                                break;
                        }
                    }
                }

                // Отрисовка мяча
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                if (ballSprite != null)
                {
                    var targetHeight = game.BallRadius * 2.2f;
                    var ratio = (float)ballSprite.Width / ballSprite.Height;
                    var targetWidth = targetHeight * ratio;
                    g.TranslateTransform(game.BallPos.X, game.BallPos.Y);
                    g.RotateTransform(game.BallRotationAngle);
                    g.DrawImage(ballSprite, -(targetWidth / 2), -(targetHeight / 2), targetWidth, targetHeight);
                    g.ResetTransform();
                }
                else
                    g.FillEllipse(Brushes.OrangeRed, game.BallPos.X - game.BallRadius, game.BallPos.Y - game.BallRadius, game.BallRadius * 2, game.BallRadius * 2);
                g.InterpolationMode = InterpolationMode.Default;

                // Отрисовка интерфейса (Счет и Таймер)
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(200, 10, 10, 10)))
                {
                    var panelW = 350;
                    var panelH = 120;
                    Rectangle scoreboard = new Rectangle(this.Width / 2 - panelW / 2, 30, panelW, panelH);

                    g.FillRectangle(bgBrush, scoreboard);
                    g.DrawRectangle(new Pen(Color.DarkOrange, 4), scoreboard);

                    // Пиксельный счёт слева
                    var pixelSize = 7;
                    var centerX = scoreboard.X + (scoreboard.Width / 2);
                    var gapFromCenter = 30;
                    var s1 = (game.Score / 10) % 10; 
                    var s2 = game.Score % 10; 
                    var scoreX = centerX - gapFromCenter - (pixelSize * 12);
                    DrawPixelDigit(g, s1, scoreX, scoreboard.Y + 35, pixelSize, Brushes.DarkOrange);
                    DrawPixelDigit(g, s2, scoreX + (pixelSize * 6), scoreboard.Y + 35, pixelSize, Brushes.DarkOrange);

                    using (Pen linePen = new Pen(Color.DarkOrange, 2))
                        g.DrawLine(linePen, centerX, scoreboard.Y + 20, centerX, scoreboard.Bottom - 20);

                    var seconds = (int)Math.Ceiling(Math.Max(0, game.TimeLeft));
                    var t1 = (seconds / 10) % 10;
                    var t2 = seconds % 10;
                    var timeBrush = game.TimeLeft <= 5.0 ? Brushes.Red : Brushes.Orange;
                    var timerX = centerX + gapFromCenter;
                    DrawPixelDigit(g, t1, timerX, scoreboard.Y + 35, pixelSize, timeBrush);
                    DrawPixelDigit(g, t2, timerX + (pixelSize * 6), scoreboard.Y + 35, pixelSize, timeBrush);
                }

                if (game.ComboOpacity > 0)
                {
                    var alpha = (int)(255 * game.ComboOpacity);
                    using (Font comboFont = new Font("Impact", 36, FontStyle.Italic))
                    using (SolidBrush comboBrush = new SolidBrush(Color.FromArgb(alpha, Color.DarkOrange)))
                    using (Pen outlinePen = new Pen(Color.FromArgb(alpha, Color.Black), 2))
                    {
                        GraphicsPath path = new GraphicsPath();
                        path.AddString(game.ComboText, comboFont.FontFamily, (int)comboFont.Style, comboFont.Size, game.ComboPos, StringFormat.GenericDefault);
                        g.DrawPath(outlinePen, path);
                        g.FillPath(comboBrush, path);
                    }
                }
            }
        }

        public static void DrawPixelDigit(Graphics g, int digit, int x, int y, int pixelSize, Brush brush)
        {
            if (digit < 0 || digit > 9)
                return;
            var rows = PixelDigits[digit];

            for (var row = 0; row < rows.Length; row++)
            {
                for (var col = 0; col < 5; col++)
                {
                    if (((rows[row] >> (4 - col)) & 1) == 1)
                        g.FillRectangle(brush, x + col * pixelSize, y + row * pixelSize, pixelSize - 1, pixelSize - 1);
                }
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (currentState == GameState.Playing && !game.IsBallFlying)
            {
                var dist = (float)Math.Sqrt(Math.Pow(e.X - game.BallPos.X, 2) + Math.Pow(e.Y - game.BallPos.Y, 2));
                if (dist < 80)
                {
                    isDragging = true;
                    mouseCurrent = e.Location;
                    game.StartAiming();
                }
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
                mouseCurrent = e.Location;
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                var dx = e.X - game.BallPos.X;
                var dy = e.Y - game.BallPos.Y;
                var dist = (float)Math.Sqrt(dx * dx + dy * dy);
                var maxDist = 500f;

                if (dist > maxDist)
                {
                    dx = (dx / dist) * maxDist;
                    dy = (dy / dist) * maxDist;
                }

                game.BallVelocity = new PointF(dx * 0.12f, dy * 0.12f);
                game.IsBallFlying = true;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Application.Exit();
        }
    }
}