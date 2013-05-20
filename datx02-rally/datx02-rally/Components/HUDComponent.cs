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
    /// 
    /// Speedometer adopted from
    /// https://code.google.com/p/riot2021/
    /// and
    /// http://en.wikibooks.org/wiki/Game_Creation_with_XNA/2D_Development/Heads-Up-Display (original source)
    /// </summary>
    public class HUDComponent : Microsoft.Xna.Framework.DrawableGameComponent
    {
        // speedometer
        private enum SpeedHUD { Graphic, Text, None }
        private SpeedHUD speedRepresentation = SpeedHUD.Graphic;
        private Vector2 speedometerPosition;
        private Vector2 needleOrigin;
        private Vector2 needlePosition;
        private float MAX_NEEDLE_ANGLE = 240;
        private Texture2D speedometerTexture;
        private Texture2D needleTexture;

        // other stuff
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private TimeSpan timeSinceLastFPS;
        private List<TextNotification> notifications = new List<TextNotification>();
        private DateTime lastPlacementNotification = DateTime.MinValue;

        private int frameCount = 0;
        private int currentFps;

        private int playerPosition = 1;

        public bool ConsoleEnabled { get; set; }
        public bool SpeedEnabled { get; set; }
        public bool PlayerPlaceEnabled { get; set; }
        public bool TimeEnabled { get; set; }
        public bool PlacementNotificationsEnabled { get; set; }
        private bool playerPlaceTimeOut = true;

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
            PlacementNotificationsEnabled = true; // seriously, don't set this to true, its done externally
            DrawOrder = 100;

        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            // speedometer
            speedometerTexture = Game.Content.Load<Texture2D>(@"HUD/speedometer2");
            needleTexture = Game.Content.Load<Texture2D>(@"HUD/speedometer-needle");
            speedometerPosition = new Vector2(5, GraphicsDevice.Viewport.Height - speedometerTexture.Height - 5);
            needlePosition = new Vector2(speedometerPosition.X + speedometerTexture.Width / 2, speedometerPosition.Y + speedometerTexture.Height / 2);
            needleOrigin = new Vector2(50, 18);
        }

        public void SetPlayerPosition(int position, TimeSpan notificationTime) 
        {
            if (TimeSpan.FromSeconds(3) > DateTime.Now - lastPlacementNotification)
                return;
            if (PlacementNotificationsEnabled && position != playerPosition && !playerPlaceTimeOut)
            {
                lastPlacementNotification = DateTime.Now;
                string[] suffixes = { "th", "st", "nd", "rd", "th", "th", "th", "th", "th", "th" };
                string text = position.ToString();
                if (position > 9 && (position % 10) % 10 == 1)
                    text += "th place!";
                else
                    text += (suffixes[position % 10] + " place!");
                ShowTextNotification(Color.Aqua, text, notificationTime);
            }

            playerPosition = position;
        }

        public void SetPlayerPosition(int position)
        {
            SetPlayerPosition(position, TimeSpan.FromSeconds(2));
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
                DrawOutlinedString(spriteBatch, currentFps == 0 ? "" : currentFps.ToString(), Color.Black, Color.Moccasin, 1, topLeft);
            }
            if (SpeedEnabled)
            {
                var car = Game.GetService<Car>();
                float carSpeed = Math.Abs(car.Speed);
                switch (speedRepresentation)
                {
                    case SpeedHUD.Graphic:
                        float rotation = ((carSpeed / car.MaxSpeed) * MAX_NEEDLE_ANGLE);
                        spriteBatch.Draw(speedometerTexture, speedometerPosition, Color.White);
                        spriteBatch.Draw(needleTexture, needlePosition, null, Color.White, MathHelper.ToRadians(rotation), needleOrigin, 1, SpriteEffects.None, 0);
                        break;
                    case SpeedHUD.Text:
                        carSpeed = (float)Math.Round(carSpeed * 2.35f, 1);
                        Vector2 bottomLeft = new Vector2(0, Game.GraphicsDevice.Viewport.Height - font.LineSpacing);
                        DrawOutlinedString(spriteBatch, carSpeed.ToString(), Color.Black, Color.Moccasin, 1, bottomLeft);
                        break;
                    case SpeedHUD.None:
                        break;
                    default:
                        break;
                }

            }
            if (PlayerPlaceEnabled)
            {
                int totalPlayers = Game.GetService<CarControlComponent>().Cars.Count;
                string positionText = playerPosition + "/" + totalPlayers;
                Vector2 topRight = new Vector2(Game.GraphicsDevice.Viewport.Width - font.MeasureString(positionText).X, 0);
                DrawOutlinedString(spriteBatch, positionText, Color.Black, Color.Azure, 1, topRight);
            }
            if (TimeEnabled)
            {
                TimeSpan startTime = Game.GetService<GameplayMode>().StartTime;
                if (startTime != TimeSpan.Zero)
                {
                    TimeSpan totalRaceTime = gameTime.TotalGameTime - startTime;
                    playerPlaceTimeOut = totalRaceTime < TimeSpan.FromSeconds(1);

                    string timeText = totalRaceTime.ToString(@"m\:ss\:ff");
                    Vector2 topCenter1 = new Vector2((Game.GraphicsDevice.Viewport.Width / 2) - (font.MeasureString(timeText).X / 2), 0);
                    DrawOutlinedString(spriteBatch, timeText, Color.Black, Color.SpringGreen, 1, topCenter1);
                }
            }

            var notificationPosition = new Vector2(Game.GraphicsDevice.Viewport.Width / 2.0f, Game.GraphicsDevice.Viewport.Height * 0.2f);  
            foreach (var notification in new List<TextNotification>(notifications)) //thread safety
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

                //spriteBatch.DrawString(font, notification.Text, thisNotificationPosition, Color.Lerp(Color.Transparent, notification.Color, fadeProgress), 
                //    0.0f, Vector2.Zero, fadeProgress, SpriteEffects.None, 0);
                DrawOutlinedString(spriteBatch, notification.Text,
                    Color.Lerp(Color.Transparent, Color.Black, fadeProgress),
                    Color.Lerp(Color.Transparent, notification.Color, fadeProgress), 
                    fadeProgress, thisNotificationPosition);
                notificationPosition.Y += textSize.Y;
                notification.DisplayedTime += gameTime.ElapsedGameTime;
            }
            notifications.RemoveAll(notification => notification.DisplayedTime > notification.Timeout);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        // Applied from http://erikskoglund.wordpress.com/2009/09/10/super-simple-text-outlining-in-xna/
        private void DrawOutlinedString(SpriteBatch sb, string text, Color backColor, Color frontColor, float scale, Vector2 position)
        {
            Vector2 origin = Vector2.Zero;//new Vector2(font.MeasureString(text).X / 2, font.MeasureString(text).Y / 2);
            float rotation = 0.0f;
            int thickness = 3; // default 1

            //Outline
            sb.DrawString(font, text, position + new Vector2(thickness * scale, thickness * scale), backColor, rotation, origin, scale, SpriteEffects.None, 0);
            sb.DrawString(font, text, position + new Vector2(-thickness * scale, -thickness * scale), backColor, rotation, origin, scale, SpriteEffects.None, 0);
            sb.DrawString(font, text, position + new Vector2(-thickness * scale, thickness * scale), backColor, rotation, origin, scale, SpriteEffects.None, 0);
            sb.DrawString(font, text, position + new Vector2(thickness * scale, -thickness * scale), backColor, rotation, origin, scale, SpriteEffects.None, 0);

            //Text
            sb.DrawString(font, text, position, frontColor, rotation, origin, scale, SpriteEffects.None, 0);
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
