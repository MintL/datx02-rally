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
        Game1 Game;
        readonly int PORT = 19283;
        
        public ServerClient(Game1 game) : base(game)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("DATX02");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            
            Client = new NetClient(config);
            Client.Start();

            Sender = new ServerSender(Client);
            Receiver = new ServerReceiver(Client);
            Game = game;
        }

        public void Connect(IPAddress IP) {
            Client.Connect(new IPEndPoint(IP, PORT));
        }

        public override void Update(GameTime gameTime)
        {
            Sender.SendPlayerPosition(Game.car.Position, gameTime.TotalGameTime.TotalMilliseconds);
            base.Update(gameTime);
        }

    }
}
