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
        public readonly int ID;
        public readonly int LOCAL_PLAYER = 0;

        public Player(Game1 game) : base(game)
        {
            // Local player
            ID = LOCAL_PLAYER;
            // TODO
        }

        public Player(Game1 game, int id) : base(game)
        {
            ID = id;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// Gets 
        /// </summary>
        /// <returns></returns>
        public void SetPosition(float x, float y, float z) 
        {
            Position.X = x;
            Position.Y = y;
            Position.Z = z;
        }

    }
}
