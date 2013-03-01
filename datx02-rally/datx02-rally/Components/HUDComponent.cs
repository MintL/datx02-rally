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
        private TimeSpan timeSinceLastDraw;

        private int frameCount = 0;
        private int currentFps;

        /*private float distance = 0;
        private float prevPosition;
        private float currentSpeed;*/

        public bool ConsoleEnabled { get; set; }
        public bool SpeedEnabled { get; set; }

        public HUDComponent(Game game)
            : base(game)
        {
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            timeSinceLastDraw = new TimeSpan(0);
            font = Game.Content.Load<SpriteFont>(@"Menu/FPSFont");

            ConsoleEnabled = false;
            SpeedEnabled = true;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Draw(GameTime gameTime)
        {
            timeSinceLastDraw = timeSinceLastDraw.Add(gameTime.ElapsedGameTime);
            if (ConsoleEnabled)
            {
                if (timeSinceLastDraw.Ticks >= TimeSpan.TicksPerSecond)
                {
                    currentFps = ++frameCount;
                    frameCount = 0;
                    timeSinceLastDraw = new TimeSpan(0);
                }
                else
                {
                    frameCount++;
                }

                spriteBatch.Begin();
                Vector2 topLeft = new Vector2(0, 0);
                spriteBatch.DrawString(font, currentFps == 0 ? "" : currentFps.ToString(), topLeft, Color.Moccasin);
                spriteBatch.End();
            }
            if (SpeedEnabled)
            {
                if (timeSinceLastDraw.Ticks >= TimeSpan.TicksPerSecond/10)
                {
                    spriteBatch.Begin();
                    double carSpeed = Math.Abs(Math.Round(((Game as Game1).currentView as GamePlayView).Car.Speed, 1));
                    Vector2 bottomLeft = new Vector2(0, Game.GraphicsDevice.Viewport.Height - font.LineSpacing);
                    spriteBatch.DrawString(font, carSpeed.ToString(), bottomLeft, Color.Moccasin);
                    spriteBatch.End();
                }
            }
            base.Draw(gameTime);
        }
    }
}
