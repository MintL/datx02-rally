﻿using System;
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
    }
    class ServerPlayer
    {
        public string PlayerName { get; set; }
        readonly int PlayerID;
        public Position PlayerPos;
        public NetConnection Connection { get; set; }

        public ServerPlayer(int id, NetConnection connection)
        {
            PlayerID = id;
            PlayerName = "Player "+id;
            Connection = connection;
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
