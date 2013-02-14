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
        public readonly byte ID;
        public readonly byte LOCAL_PLAYER = 0;
        public string PlayerName;

        public Player(Game1 game) : base(game)
        {
            // Local discoveredPlayer
            ID = LOCAL_PLAYER;
            PlayerName = "LOCAL";
            // TODO
        }

        public Player(Game1 game, byte id, string name) : base(game)
        {
            ID = id;
            PlayerName = name;
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
