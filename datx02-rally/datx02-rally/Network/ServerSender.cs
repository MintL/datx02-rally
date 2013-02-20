using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Lidgren.Network;

namespace datx02_rally
{
    class ServerSender
    {
        NetClient ServerThread;
        ServerClient ServerHandler;

        public ServerSender(NetClient serverThread, ServerClient handler)
        {
            this.ServerThread = serverThread;
            this.ServerHandler = handler;
        }

        public void SendTestData()
        {
            NetOutgoingMessage msg = ServerThread.CreateMessage();
            msg.Write((byte)MessageType.Debug);
            msg.Write("Hello world! I'm connected.");
            ServerThread.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
        }

        public void SendPlayerPosition(Vector3 position, float rotation, double ms)
        {
            NetOutgoingMessage msg = ServerThread.CreateMessage();
            msg.Write((byte)MessageType.PlayerPos);
            msg.Write(ms);
            msg.Write(position.X);
            msg.Write(position.Y);
            msg.Write(position.Z);
            msg.Write(rotation);
            ServerThread.SendMessage(msg, NetDeliveryMethod.Unreliable);
        }

        public void SendChatMessage(string textMsg)
        {
            NetOutgoingMessage msg = ServerThread.CreateMessage();
            msg.Write((byte)MessageType.Chat);
            msg.Write(textMsg);
            ServerThread.SendMessage(msg, NetDeliveryMethod.Unreliable);
        }

        public void SendPlayerInfo()
        {
            NetOutgoingMessage msg = ServerThread.CreateMessage();
            msg.Write((byte)MessageType.PlayerInfo);
            msg.Write(ServerHandler.LocalPlayer.PlayerName);
            ServerThread.SendMessage(msg, NetDeliveryMethod.Unreliable);
        }
    }
}
