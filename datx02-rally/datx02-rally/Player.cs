using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally
{
    class Player : GameComponent
    {
        Vector3 Position;

        public Player(Game1 game) : base(game)
        {
            // TODO
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// Gets 
        /// </summary>
        /// <returns></returns>
        private Vector3 GetPosition(GameTime gameTime) 
        {
            return new Vector3();
        }
    }
}
