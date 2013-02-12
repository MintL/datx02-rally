using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Lidgren.Network;
using System.Net;

namespace datx02_rally
{
    public enum MessageType { PlayerPos, Chat, Debug }
    class ServerClient : GameComponent
    {
        NetClient Client;
        ServerSender Sender;
        ServerReceiver Receiver;
        readonly int PORT = 19283;
        
        public ServerClient(Game1 game) : base(game)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("DATX02");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            
            Client = new NetClient(config);
            Client.Start();

            Sender = new ServerSender(Client);
            Receiver = new ServerReceiver(Client);
        }

        public void Connect(IPAddress IP) {
            Client.Connect(new IPEndPoint(IP, PORT));
        }

        public void SendTestData()
        {
            NetOutgoingMessage msg = Client.CreateMessage();
            msg.Write((byte)MessageType.Debug);
            msg.Write("Hello world! I'm connected.");
            Client.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
        }

    }
}
