using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using datx02_rally.Menus;


namespace datx02_rally.Components
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class HUDComponent : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private TimeSpan timeSinceLastFPS;
        private List<TextNotification> notifications = new List<TextNotification>();

        private int frameCount = 0;
        private int currentFps;

        private int playerPosition = 1;

        public bool ConsoleEnabled { get; set; }
        public bool SpeedEnabled { get; set; }
        public bool PlayerPlaceEnabled { get; set; }
        public bool TimeEnabled { get; set; }

        public HUDComponent(Game game)
            : base(game)
        {
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            timeSinceLastFPS = new TimeSpan(0);
            font = Game.Content.Load<SpriteFont>(@"Menu/FPSFont");

            ConsoleEnabled = false;
            SpeedEnabled = true;
            PlayerPlaceEnabled = true;
            TimeEnabled = true;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        public void SetPlayerPosition(int position) 
        {
            playerPosition = position;
        }

        public void ShowTextNotification(Color c, string text)
        {
            ShowTextNotification(c, text, TimeSpan.FromSeconds(3));
        }

        public void ShowTextNotification(Color c, string text, TimeSpan time)
        {
            TextNotification notification = new TextNotification();
            notification.DisplayedTime = TimeSpan.Zero;
            notification.Timeout = time;
            notification.FadeTime = new TimeSpan((long)(notification.Timeout.Ticks * 0.1));
            notification.Color = c;
            notification.Text = text;

            notifications.Add(notification);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            timeSinceLastFPS = timeSinceLastFPS.Add(gameTime.ElapsedGameTime);
            if (ConsoleEnabled)
            {
                if (timeSinceLastFPS.Ticks >= TimeSpan.TicksPerSecond)
                {
                    currentFps = ++frameCount;
                    timeSinceLastFPS = new TimeSpan(0);
                    frameCount = 0;
                }
                else
                {
                    frameCount++;
                }

                Vector2 topLeft = new Vector2(0, 0);
                spriteBatch.DrawString(font, currentFps == 0 ? "" : currentFps.ToString(), topLeft, Color.Moccasin);
            }
            if (SpeedEnabled)
            {
                double carSpeed = Math.Abs(Math.Round(((Game as Game1).currentView as GamePlayView).Car.Speed, 1));
                Vector2 bottomLeft = new Vector2(0, Game.GraphicsDevice.Viewport.Height - font.LineSpacing);
                spriteBatch.DrawString(font, carSpeed.ToString(), bottomLeft, Color.Moccasin);
            }
            if (PlayerPlaceEnabled)
            {
                int totalPlayers = Game.GetService<CarControlComponent>().Cars.Count;
                string positionText = playerPosition + "/" + totalPlayers;
                Vector2 topRight = new Vector2(Game.GraphicsDevice.Viewport.Width - font.MeasureString(positionText).X, 0);
                spriteBatch.DrawString(font, positionText, topRight, Color.Moccasin);
            }
            if (TimeEnabled)
            {
                string totalGameTime = gameTime.TotalGameTime.ToString(@"ss\:ff");
                string lapTime = gameTime.TotalGameTime.ToString(@"m\:ss\:ff");
                Vector2 topCenter1 = new Vector2((Game.GraphicsDevice.Viewport.Width / 2) - (font.MeasureString(lapTime).X / 2), 0);
                Vector2 topCenter2 = new Vector2((Game.GraphicsDevice.Viewport.Width / 2) - (font.MeasureString(totalGameTime).X / 2), font.MeasureString(totalGameTime).Y);
                spriteBatch.DrawString(font, lapTime, topCenter1, Color.Red);
                spriteBatch.DrawString(font, totalGameTime, topCenter2, Color.Red);
            }

            var notificationPosition = new Vector2(Game.GraphicsDevice.Viewport.Width / 2.0f, Game.GraphicsDevice.Viewport.Height * 0.2f);  
            foreach (var notification in notifications)
            {
                // fade in/out
                float fadeProgress;
                if (notification.FadeTime > notification.DisplayedTime)
                    fadeProgress = (float)notification.DisplayedTime.Ticks / notification.FadeTime.Ticks;
                else if (notification.DisplayedTime > notification.Timeout - notification.FadeTime)
                    fadeProgress = ((notification.Timeout.Ticks - notification.DisplayedTime.Ticks) / (float)notification.FadeTime.Ticks);
                else
                    fadeProgress = 1.0f;

                Vector2 textSize = font.MeasureString(notification.Text) * fadeProgress;
                Vector2 thisNotificationPosition = new Vector2(
                    notificationPosition.X - textSize.X / 2,
                    notificationPosition.Y);

                spriteBatch.DrawString(font, notification.Text, thisNotificationPosition, Color.Lerp(Color.Transparent, notification.Color, fadeProgress), 
                    0.0f, Vector2.Zero, fadeProgress, SpriteEffects.None, 0);

                notificationPosition.Y += textSize.Y;
                notification.DisplayedTime += gameTime.ElapsedGameTime;
            }
            notifications.RemoveAll(notification => notification.DisplayedTime > notification.Timeout);

            spriteBatch.End();

            base.Draw(gameTime);
        }


    }

    class TextNotification
    {
        public TimeSpan Timeout { get; set; }
        public TimeSpan FadeTime { get; set; }
        public TimeSpan DisplayedTime { get; set; }
        public Color Color { get; set; }
        public string Text { get; set; }
    }
}
