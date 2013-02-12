using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer
{
    struct Position
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }
    class ServerPlayer
    {
        public string PlayerName { get; set; }
        readonly int PlayerID;
        public Position PlayerPos;

        public ServerPlayer(int id)
        {
            PlayerID = id;
            PlayerName = "Player "+id;
            PlayerPos = new Position();
        }

        public void UpdatePosition(float x, float y, float z)
        {
            PlayerPos.x = x;
            PlayerPos.y = y;
            PlayerPos.z = z;
        }

    }
}
