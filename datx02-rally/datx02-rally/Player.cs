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
        public float Speed { get; set; }
        public PositionMessage LastReceived { get; private set; }
        public TimeSpan RaceTime { get; set; }
        public int Lap { get; set; }

        public byte ID;
        public readonly bool LOCAL_PLAYER;
        private string playerName;
        public string PlayerName
        {
            get { 
                if (LOCAL_PLAYER)
                    return GameSettings.Default.PlayerName;
                else
                    return playerName;
            }
            set { playerName = value; }
        }

        public Player(GameManager game) : base(game)
        {
            PlayerName = GameSettings.Default.PlayerName;
            LOCAL_PLAYER = true;
            LastReceived = new PositionMessage();
            LastReceived.Sequence = byte.MinValue;
            RaceTime = TimeSpan.MaxValue;
            Lap = 0;
        }

        public Player(GameManager game, byte id, string name) : base(game)
        {
            ID = id;
            PlayerName = name;
            LOCAL_PLAYER = false;
            LastReceived = new PositionMessage();
            LastReceived.Sequence = byte.MinValue;
            RaceTime = TimeSpan.MaxValue;
            Lap = 0;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// Gets 
        /// </summary>
        /// <returns></returns>
        public void SetPosition(float x, float y, float z, float rotation, float velocity, byte sequence, DateTime sentTime) 
        {
            PositionMessage newMessage = new PositionMessage();
            newMessage.Position = new Vector3(x, y, z);
            newMessage.Rotation = rotation;
            newMessage.Sequence = sequence;
            newMessage.TimeSent = sentTime;
            newMessage.Velocity = velocity;

            LastReceived = newMessage;
        }

        public Vector3 GetPosition()
        {
            return Game.GetService<CarControlComponent>().Cars[this].Position;
        }

        public class PositionMessage
        {
            public Vector3 Position { get; set; }
            public float Rotation { get; set; }
            public byte Sequence { get; set; }
            public float Velocity { get; set; }
            public DateTime TimeSent { get; set; }
        }

    }

}
