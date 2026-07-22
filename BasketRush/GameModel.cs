using System;
using System.Drawing;

namespace BasketballGame
{
    // Перечисление состояний игры
    public enum GameState
    {
        Menu,      
        Selection,  
        Playing,    
        GameOver,   
        Shop  
    }

    // Вся игровая логика
    public class GameModel
    {
        public const float Gravity = 0.8f;   
        public const float Bounce = 0.5f;   
        private Random rnd = new Random();
        public PointF BallPos;               
        public PointF BallVelocity;          
        public float BallRadius = 24;
        public RectangleF BasketRect;        
        public RectangleF BoardRect;         
        public int Score { get; set; }
        public double TimeLeft { get; set; } 
        public PointF PreviousBallPos { get; set; } 
        public bool HasScoredThisThrow { get; set; } = false; 
        public bool WasRimHit { get; set; } = false;          
        public bool IsBallFlying { get; set; }                 
        public PointF PlayerPos { get; set; }
        public RectangleF RimEllipse { get; set; }   
        public string ComboText { get; set; } = "";  
        public float ComboOpacity { get; set; } = 0f;
        public int FloorBounceCount { get; set; } = 0; 
        public float GroundY { get; set; } = 920f;    
        public float GroundBounce { get; set; } = 0.6f;
        public PointF ComboPos { get; set; }            
        public bool IsBlockedBySide { get; set; } = false; 
        public float BallRotationAngle { get; set; } = 0f; 

        private float idleAnimTime = 0f;
        private bool isIdleDribbling = true;  
        private float idleTimer = 0f;
        private const float AnimInterval = 5f;   
        private const float AnimDuration = 0.4f;

        public bool IsGameOver => TimeLeft <= 0;

        // Позиция кольца и щита
        public void SetLevel(string fileName)
        {
            BasketRect = new RectangleF(1145, 400, 55, 5);
            BoardRect = new RectangleF(1200, 218, 10, 145);
        }

        // Инициализация модели
        public GameModel(double startingTime, int screenWidth, int screenHeight)
        {
            TimeLeft = startingTime;
            Score = 0;
            SpawnPlayer(screenWidth, screenHeight);
            ResetBall();
            RimEllipse = new RectangleF(screenWidth - 398, screenHeight - 620, 60f, 20f);
        }

        // Случайный спавн игрока
        public void SpawnPlayer(int screenWidth, int screenHeight)
        {
            var minX = (int)(screenWidth * 0.22f);
            var maxX = (int)(screenWidth * 0.55f);
            PlayerPos = new PointF(rnd.Next(minX, maxX), GroundY - rnd.Next(180, 320));
        }

        // Сброс мяча в руки игрока
        public void ResetBall()
        {
            BallPos = PlayerPos;
            BallVelocity = new PointF(0, 0);
            IsBlockedBySide = false;
            WasRimHit = false;
            HasScoredThisThrow = false;
            IsBallFlying = false;
            FloorBounceCount = 0;
            BallRotationAngle = 0f;
            isIdleDribbling = true;
        }

        public void StartAiming() => isIdleDribbling = false;

        public void Update(float dt, int screenWidth, int screenHeight)
        {
            if (TimeLeft > 0)
                TimeLeft -= dt;

            if (ComboOpacity > 0)
            {
                ComboOpacity -= 1.75f * dt;
                ComboPos = new PointF(ComboPos.X, ComboPos.Y - 125f * dt);
            }

            if (!IsBallFlying)
            {
                // Анимация дриблинга
                if (isIdleDribbling)
                {
                    idleTimer += dt;

                    if (idleTimer % AnimInterval < AnimDuration)
                    {
                        idleAnimTime += dt * 8f;
                        var bounceDistance = 180f - BallRadius;
                        var offset = (float)Math.Abs(Math.Sin(idleAnimTime)) * bounceDistance;
                        BallPos = new PointF(PlayerPos.X, PlayerPos.Y + offset);
                        BallRotationAngle += 5f;
                    }
                    else
                    {
                        BallPos = PlayerPos;
                        idleAnimTime = 0f;
                    }
                }
                else
                    BallPos = PlayerPos;
                PreviousBallPos = BallPos;
                return;
            }

            // Физика полёта 
            PreviousBallPos = BallPos;
            BallVelocity = new PointF(BallVelocity.X, BallVelocity.Y + Gravity);
            BallPos = new PointF(BallPos.X + BallVelocity.X, BallPos.Y + BallVelocity.Y);
            BallRotationAngle += BallVelocity.X * 1.5f; 

            // Отскок от пола
            if (BallPos.Y + BallRadius > GroundY)
            {
                FloorBounceCount++;

                if (FloorBounceCount >= 2)
                {
                    ResetBall();
                    return;
                }

                BallPos = new PointF(BallPos.X, GroundY - BallRadius);
                BallVelocity = new PointF(BallVelocity.X * 0.9f, -Math.Abs(BallVelocity.Y) * GroundBounce);
                WasRimHit = true;
            }

            // Отскок от щита
            if (BallPos.X + BallRadius > BoardRect.Left && BallPos.X - BallRadius < BoardRect.Right &&
                BallPos.Y > BoardRect.Top && BallPos.Y < BoardRect.Bottom)
            {
                BallVelocity = new PointF(BallVelocity.X * -Bounce, BallVelocity.Y);
                BallPos = new PointF(BoardRect.Left - BallRadius, BallPos.Y);
                WasRimHit = true;
            }

            PointF leftRim = new PointF(RimEllipse.Left, RimEllipse.Top + RimEllipse.Height / 2);
            PointF rightRim = new PointF(RimEllipse.Right, RimEllipse.Top + RimEllipse.Height / 2);
            CheckRimCollision(leftRim);
            CheckRimCollision(rightRim);

            var targetY = RimEllipse.Y + (3 * 7f);
            var targetWidth = RimEllipse.Width * (float)Math.Pow(0.95f, 3);
            var targetX = RimEllipse.X + (RimEllipse.Width - targetWidth) / 2;

            // Блокировка сбоку
            if (!IsBlockedBySide)
            {
                var lineTop = new PointF(targetX - 10, targetY - 10);
                var lineBottom = new PointF(targetX + 10, targetY + 40);

                if (Intersects(PreviousBallPos, BallPos, lineTop, lineBottom))
                {
                    if (BallPos.X > PreviousBallPos.X)
                        IsBlockedBySide = true;
                }
            }

            // Проверка попадания
            if (!HasScoredThisThrow && BallVelocity.Y > 0 && !IsBlockedBySide)
            {
                var isInsideX = BallPos.X > targetX && BallPos.X < (targetX + targetWidth);
                var crossedLineDown = PreviousBallPos.Y <= targetY && BallPos.Y >= targetY;
                var isInFallbackZone = BallPos.Y > targetY && BallPos.Y < (targetY + 40f); 

                if (isInsideX && (crossedLineDown || isInFallbackZone))
                {
                    Score++;
                    ComboPos = new PointF(BallPos.X, BallPos.Y - 50);
                    ComboOpacity = 1.0f;

                    if (!WasRimHit)
                    {
                        TimeLeft += 5.0;
                        ComboText = "+5с";
                    }
                    else
                    {
                        TimeLeft += 3.0;
                        ComboText = "+3с";
                    }
                    HasScoredThisThrow = true;
                    SpawnPlayer(screenWidth, screenHeight);
                    ResetBall();
                    return;
                }
            }

            if (BallPos.X > RimEllipse.Left && BallPos.X < RimEllipse.Right &&
                BallPos.Y > RimEllipse.Top && BallPos.Y < RimEllipse.Bottom + 100)
                BallVelocity = new PointF(BallVelocity.X * 0.8f, BallVelocity.Y * 0.85f);

            // Сброс мяча
            if (BallPos.Y > 1080 || BallPos.X < -100 || BallPos.X > 2000)
                ResetBall();
        }

        private bool Intersects(PointF a, PointF b, PointF c, PointF d)
        {
            var denominator = ((b.X - a.X) * (d.Y - c.Y)) - ((b.Y - a.Y) * (d.X - c.X));
            if (denominator == 0)
                return false;
            var ta = (((c.X - a.X) * (d.Y - c.Y)) - ((c.Y - a.Y) * (d.X - c.X))) / denominator;
            var tb = (((c.X - a.X) * (b.Y - a.Y)) - ((c.Y - a.Y) * (b.X - a.X))) / denominator;
            return (ta >= 0 && ta <= 1) && (tb >= 0 && tb <= 1);
        }

        // Столкновение с кольцом
        private void CheckRimCollision(PointF rimPoint)
        {
            var dx = BallPos.X - rimPoint.X;
            var dy = BallPos.Y - rimPoint.Y;
            var distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance < BallRadius)
            {
                if (distance < BallRadius - 5f)
                    WasRimHit = true;

                var normalX = dx / distance;
                var normalY = dy / distance;
                var dot = BallVelocity.X * normalX + BallVelocity.Y * normalY;

                BallVelocity = new PointF(
                    (BallVelocity.X - 2 * dot * normalX) * 0.7f,
                    (BallVelocity.Y - 2 * dot * normalY) * 0.7f
                );

                // Выталкиваем мяч за пределы
                BallPos = new PointF(
                    rimPoint.X + normalX * BallRadius,
                    rimPoint.Y + normalY * BallRadius
                );
            }
        }

        // Вычисляет точку на траектории броска
        public PointF GetTrajectoryPoint(PointF startPos, PointF velocity, float time)
        {
            var t = time * 20;
            var x = startPos.X + velocity.X * t;
            var y = startPos.Y + velocity.Y * t + 0.5f * Gravity * t * t;
            return new PointF(x, y);
        }
    }
}