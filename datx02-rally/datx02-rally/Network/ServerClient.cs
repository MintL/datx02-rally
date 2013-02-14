using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Lidgren.Network;
using System.Net;

namespace datx02_rally
{
    public enum MessageType { PlayerPos, Chat, Debug, LobbyUpdate }
    class ServerClient : GameComponent
    {
        NetClient ServerThread;
        ServerSender Sender;
        ServerReceiver Receiver;
        Game1 Game;
        public Dictionary<byte,Player> Players = new Dictionary<byte, Player>();
        readonly int PORT = 19283;
        
        public ServerClient(Game1 game) : base(game)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("DATX02");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            
            ServerThread = new NetClient(config);
            ServerThread.Start();

            Sender = new ServerSender(ServerThread, this);
            Receiver = new ServerReceiver(ServerThread, this);
            Game = game;
        }

        public void Connect(IPAddress IP) {
            ServerThread.Connect(new IPEndPoint(IP, PORT));
        }

        public override void Update(GameTime gameTime)
        {
            Sender.SendPlayerPosition(Game.car.Position, gameTime.TotalGameTime.TotalMilliseconds);
            Receiver.ReceiveMessages();
            base.Update(gameTime);
        }

        public void Chat(string msg)
        {
            Sender.SendChatMessage(msg);
        }
    }
}
