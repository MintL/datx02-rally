using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Lidgren.Network;
using System.Net;

namespace datx02_rally
{
    public enum MessageType { 
        PlayerPos, Chat, Debug, // game info exchange stuff
        LobbyUpdate, PlayerInfo, OK // handshake-y stuff    
    }
    class ServerClient : GameComponent
    {
        NetClient ServerThread;
        ServerSender Sender;
        ServerReceiver Receiver;
        Game1 Game;
        public readonly Player LocalPlayer;
        public Dictionary<byte,Player> Players = new Dictionary<byte, Player>();
        readonly int PORT = 19283;
        public bool connected = false;

        private DateTime TryConnectedTime = DateTime.MinValue;
        private TimeSpan WaitConnect = new TimeSpan(0, 0, 3); // 3 second wait
        
        public ServerClient(Game1 game) : base(game)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("DATX02");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            
            ServerThread = new NetClient(config);
            ServerThread.Start();

            Sender = new ServerSender(ServerThread, this);
            Receiver = new ServerReceiver(ServerThread, this);
            Game = game;
            LocalPlayer = new Player(game);
        }

        public void Connect(IPAddress IP) {
            ServerThread.Connect(new IPEndPoint(IP, PORT));
            TryConnectedTime = DateTime.Now;
        }

        public override void Update(GameTime gameTime)
        {
            // If not received handshake, has tried to connect, and has waited 3 secs, try again
            if (!connected)
            {
                if (TryConnectedTime != DateTime.MinValue && DateTime.Now - TryConnectedTime >= WaitConnect)
                {
                    Sender.SendPlayerInfo();
                    TryConnectedTime = DateTime.Now;
                }
                return;
            }

            Sender.SendPlayerPosition(Game.car.Position, gameTime.TotalGameTime.TotalMilliseconds);
            Receiver.ReceiveMessages();
            base.Update(gameTime);
        }

        public void Chat(string msg)
        {
            Sender.SendChatMessage(msg);
        }

        public void SetPlayerName(string name)
        {
            LocalPlayer.PlayerName = name;
        }

    }
}
