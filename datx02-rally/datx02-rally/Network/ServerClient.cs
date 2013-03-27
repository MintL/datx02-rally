using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Lidgren.Network;
using System.Net;
using datx02_rally.Menus;

namespace datx02_rally
{
    public enum MessageType { 
        PlayerPos, Chat, Debug, StateChange, // game info exchange stuff
        LobbyUpdate, PlayerInfo, OK // handshake-y stuff    
    }
    public enum ServerState { Lobby, Gameplay, Ended }
    class ServerClient : GameComponent
    {
        NetClient ServerThread;
        ServerSender Sender;
        ServerReceiver Receiver;
        Game1 Game;
        public GamePlayView GamePlay { set; get; }
        public readonly Player LocalPlayer;
        public LinkedList<Tuple<string, string, DateTime>> ChatHistory = new LinkedList<Tuple<string, string, DateTime>>();
        public Dictionary<byte,Player> Players = new Dictionary<byte, Player>();
        readonly int PORT = 19283;
        public bool connected = false;
        public ServerState State { get; set; } 

        private DateTime TryConnectedTime = DateTime.MinValue;
        private TimeSpan WaitConnect = new TimeSpan(0, 0, 1); // 3 second wait
        private int ConnectTryCount = 0;
        private int MAX_CONNECT_TRIES = 5;

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
            State = ServerState.Lobby;
        }

        public void Connect(IPAddress IP) {
            ServerThread.Connect(new IPEndPoint(IP, PORT));
            TryConnectedTime = DateTime.Now;
            ConnectTryCount = 0;
        }

        public void Disconnect()
        {
            connected = false;
            TryConnectedTime = DateTime.MinValue;
            ConnectTryCount = 0;
            ServerThread.Disconnect("");
        }

        public override void Update(GameTime gameTime)
        {
            Receiver.ReceiveMessages();
            // If not received handshake, has tried to connect, and has waited 1 sec, try again
            if (!connected)
            {
                bool connectionTimeOut = ConnectTryCount > MAX_CONNECT_TRIES;
                if (connectionTimeOut)
                {
                    Game.GetService<HUDConsoleComponent>().WriteOutput("Could not connect to server");
                    Disconnect();
                }
                else if (TryConnectedTime != DateTime.MinValue && DateTime.Now - TryConnectedTime >= WaitConnect)
                {
                    Sender.SendPlayerInfo();
                    TryConnectedTime = DateTime.Now;
                    ConnectTryCount++;
                }
                return;
            }

            Sender.SendPlayerPosition(GamePlay.Car.Position, GamePlay.Car.Rotation, gameTime.TotalGameTime.TotalMilliseconds);
            base.Update(gameTime);
        }

        public void Chat(string msg)
        {
            Sender.SendChatMessage(msg);
        }

        public void SetPlayerName(string name)
        {
            LocalPlayer.PlayerName = name;
            if (connected)
                Sender.SendPlayerInfo();
        }

    }
}
