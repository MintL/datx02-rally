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
        public float velocity { get; set; }
    }
    struct Color
    {
        public byte r { get; set; }
        public byte g { get; set; }
        public byte b { get; set; }
    }
    class ServerPlayer
    {
        public string PlayerName { get; set; }
        public bool Ready { get; set; }
        public byte SequenceNo = 0;
        public readonly byte PlayerID;
        public Position PlayerPos;
        public Color CarColor;
        public NetConnection Connection { get; set; }
        public long RaceTime { get; set; }
        public bool connected = false;

        public ServerPlayer(byte id, NetConnection connection)
        {
            PlayerID = id;
            Ready = false;
            PlayerName = "Unnamed Player "+id;
            Connection = connection;
            PlayerPos = new Position();

            // default to white cars
            CarColor = new Color();
            CarColor.r = byte.MaxValue;
            CarColor.g = byte.MaxValue;
            CarColor.b = byte.MaxValue;
        }

        public void UpdatePosition(float x, float y, float z, float rotation, float velocity)
        {
            PlayerPos.x = x;
            PlayerPos.y = y;
            PlayerPos.z = z;
            PlayerPos.rotation = rotation;
            PlayerPos.velocity = velocity;
        }


    }
}
