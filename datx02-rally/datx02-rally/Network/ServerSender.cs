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
        NetClient Client;

        public ServerSender(NetClient client)
        {
            this.Client = client;
        }

        public void SendTestData()
        {
            NetOutgoingMessage msg = Client.CreateMessage();
            msg.Write((byte)MessageType.Debug);
            msg.Write("Hello world! I'm connected.");
            Client.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
        }

        public void SendPlayerPosition(Vector3 position)
        {
            NetOutgoingMessage msg = Client.CreateMessage();
            msg.Write((byte)MessageType.PlayerPos);
            msg.Write(position.X);
            msg.Write(position.Y);
            msg.Write(position.Z);
            Client.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
        }
    }
}
