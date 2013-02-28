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


namespace datx02_rally.Components
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class FPSComponent : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private int currentFps;
        private TimeSpan timeSinceLastFPS;
        private int frameCount = 0;

        public FPSComponent(Game game)
            : base(game)
        {
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            timeSinceLastFPS = new TimeSpan(0);
            font = Game.Content.Load<SpriteFont>(@"Menu/FPSFont");
            // TODO: Construct any child components here
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
            timeSinceLastFPS = timeSinceLastFPS.Add(gameTime.ElapsedGameTime);
            if (timeSinceLastFPS.Ticks >= TimeSpan.TicksPerSecond)
            {
                currentFps = ++frameCount;
                frameCount = 0;
                timeSinceLastFPS = new TimeSpan(0);
            }
            else
            {
                frameCount++;
            }

            spriteBatch.Begin();
            Vector2 topLeft = new Vector2(0, 0);
            spriteBatch.DrawString(font, currentFps == 0 ? "" : currentFps.ToString(), topLeft, Color.Moccasin);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
