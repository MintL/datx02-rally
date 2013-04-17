using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally
{
    class Player : GameComponent
    {
        public Vector3 Position { get; set; }
        public float Rotation { get; set; }
        public byte ID;
        public readonly bool LOCAL_PLAYER;
        public string PlayerName;
        public Player(Game1 game) : base(game)
        {
            PlayerName = "UnnamedPlayer";
            LOCAL_PLAYER = true;
        }

        public Player(Game1 game, byte id, string name) : base(game)
        {
            ID = id;
            PlayerName = name;
            LOCAL_PLAYER = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// Gets 
        /// </summary>
        /// <returns></returns>
        public void SetPosition(float x, float y, float z, float rotation) 
        {
            Position = new Vector3(x, y, z);
            Rotation = rotation;
        }

        public Vector3 GetPosition()
        {
            return Game.GetService<CarControlComponent>().Cars[this].Position;
        }


    }
}
