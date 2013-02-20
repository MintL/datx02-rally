using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace GameServer
{
    struct Position
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float rotation { get; set; }
    }
    class ServerPlayer
    {
        public string PlayerName { get; set; }
        public readonly byte PlayerID;
        public Position PlayerPos;
        public NetConnection Connection { get; set; }
        public bool connected = false;

        public ServerPlayer(byte id, NetConnection connection)
        {
            PlayerID = id;
            PlayerName = "Unnamed Player "+id;
            Connection = connection;
            PlayerPos = new Position();
        }

        public void UpdatePosition(float x, float y, float z, float rotation)
        {
            PlayerPos.x = x;
            PlayerPos.y = y;
            PlayerPos.z = z;
            PlayerPos.rotation = rotation;
        }

    }
}
