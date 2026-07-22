using System;
using System.Drawing;
using System.Windows.Forms;

namespace BasketballGame
{
    // Экран настроек 
    public static class SettingsUI
    {
        public static Panel CreateSettingsPanel(Rectangle bounds, Action onBack)
        {
            BufferedPanel panel = new BufferedPanel
            {
                Size = bounds.Size,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(240, 20, 20, 30),
                Visible = false 
            };

            // Заголовок экрана
            Label title = new Label
            {
                Text = "НАСТРОЙКИ",
                Font = new Font("Impact", 60),
                ForeColor = Color.Orange,
                Size = new Size(bounds.Width, 120),
                Location = new Point(0, 100),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            panel.Controls.Add(title);

            // Информационная надпись
            Label infoLabel = new Label
            {
                Text = "Здесь скоро появятся новые опции",
                Font = new Font("Arial", 14),
                ForeColor = Color.Gray,
                Size = new Size(bounds.Width, 40),
                Location = new Point(0, 300),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            panel.Controls.Add(infoLabel);

            // Кнопка «назад» в левом верхнем углу
            var backBtn = new BackArrowButton { Location = new Point(25, 25) };
            backBtn.Click += (s, e) => onBack();
            panel.Controls.Add(backBtn);
            backBtn.BringToFront(); 

            return panel;
        }
    }
}