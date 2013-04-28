using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Net;
using System.Timers;

namespace GameServer
{
    class Server
    {
        int genSeed;
        byte playerIdCounter = 0;
        string serverName;
        readonly int maxPlayers;
        Dictionary<IPAddress, ServerPlayer> players;
        readonly int port;
        NetServer serverThread;
        Boolean running;
        ServerState state;
        bool started = false;
        
        //Debug stuff
        Boolean DbgPlayerPos = false;

        public enum MessageType
        {
            PlayerPos, Chat, Debug, StateChange, // game info exchange stuff
            LobbyUpdate, PlayerInfo, OK, Countdown // handshake-y stuff    
        }
        public enum ServerState { Lobby, Gameplay, Ended }

        public Server(int genSeed)
        {
            this.genSeed = genSeed;
            serverName = "Test server";
            maxPlayers = 4;
            players = new Dictionary<IPAddress, ServerPlayer>();
            port = 19283;
            state = ServerState.Lobby;

            Start();
        }

        public Server() : this( (int) DateTime.Now.Ticks)
        { }

        private void Run()
        {
            NetIncomingMessage msg;
            while (running)
            {
                while ((msg = serverThread.ReadMessage()) != null)
                {
                    HandleIncomingPacket(msg);

                    if (!started && players.Count > 0 && players.Values.All(c => c.Ready))
                    {
                        Console.WriteLine("All players ready, starting countdown!");
                        DistributeCountdown();
                        started = true;
                    }
                }
            }
        }

        private void HandleIncomingPacket(NetIncomingMessage msg)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.DiscoveryRequest:
                    Console.WriteLine("Server discovered by new player " + msg.SenderEndPoint.Address);
                    break;
                case NetIncomingMessageType.StatusChanged:
                    NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                    switch (status)
                    {
                        case NetConnectionStatus.Connected:
                            PlayerConnected(msg.SenderConnection);
                            break;
                        case NetConnectionStatus.Disconnected:
                            PlayerDisconnected(msg.SenderConnection);
                            break;
                        default:
                            Console.WriteLine("Received unhandled status message " + status + " from player " + msg.SenderEndPoint.Address);
                            break;
                    }
                    break;
                case NetIncomingMessageType.Data:
                    ProcessDataMessage(msg);
                    break;
                default:
                    Console.WriteLine("Received unhandled message type '" + msg.MessageType + "' from player " + msg.SenderEndPoint.Address);
                    break;
            }
        }

        private void ProcessDataMessage(NetIncomingMessage msg)
        {
            //Console.Write("Received data from player " + msg.SenderEndPoint.Address);
            
            ServerPlayer player;
            if (!players.TryGetValue(msg.SenderEndPoint.Address, out player)) 
            {
                Console.WriteLine("Player not found!");
                return;
            }

            MessageType type = (MessageType)msg.ReadByte();
            switch (type)
            {
                case MessageType.PlayerPos:
                    float x = msg.ReadFloat(); float y = msg.ReadFloat(); float z = msg.ReadFloat();
                    float rotation = msg.ReadFloat(); float velocity = msg.ReadFloat();
                    if (DbgPlayerPos) 
                        Console.WriteLine("PlayerPos: X:{0}, Y:{1}, Z:{2}",x,y,z);
                    player.UpdatePosition(x, y, z, rotation, velocity);
                    DistributePlayerPosition(player);
                    break;
                case MessageType.Chat:
                    string chatMsg = msg.ReadString();
                    string chatSender = player.PlayerName;
                    Console.WriteLine("Chat message: '{0}: {1}", chatSender, chatMsg);
                    DistributeChatMessage(player, chatMsg);
                    break;
                case MessageType.Debug:
                    string debugMsg = msg.ReadString();
                    Console.WriteLine(" DEBUG: " + debugMsg);
                    break;
                case MessageType.PlayerInfo:
                    string playerName = msg.ReadString();
                    player.PlayerName = playerName;
                    Console.WriteLine("PlayerInfo: " + playerName);
                    if (!player.connected)
                    {
                        SendOKHandshake(player);
                        player.connected = true;
                    }
                    DistributeLobbyUpdate();
                    break;
                case MessageType.OK:
                    player.Ready = true;
                    break;
                default:
                    Console.WriteLine(" of unknown type!");
                    break;
            }
        }

        private void DistributeLobbyUpdate()
        {
            NetOutgoingMessage msg = serverThread.CreateMessage();
            msg.Write((byte)MessageType.LobbyUpdate);
            msg.Write((byte)players.Values.Count);
            foreach (var player in players.Values)
            {
                msg.Write(player.PlayerID);
                msg.Write(player.PlayerName);
            }
            serverThread.SendToAll(msg, NetDeliveryMethod.Unreliable);
        }

        private void DistributeCountdown()
        {
            for (int i = 1; i <= 4; i++)
            {
                Timer timer = new Timer(i * 1000);
                timer.Elapsed += (s, e) =>
                {
                    NetOutgoingMessage msg = serverThread.CreateMessage();
                    msg.Write((byte)MessageType.Countdown);
                    msg.Write((byte)i);
                    serverThread.SendToAll(msg, NetDeliveryMethod.Unreliable);
                };
                timer.Start();
            }
        }

        private void DistributeChatMessage(ServerPlayer player, string chatMsg)
        {
            NetOutgoingMessage msg = serverThread.CreateMessage();
            msg.Write((byte)MessageType.Chat);
            msg.Write(player.PlayerID);
            msg.Write(chatMsg);
            SendToAllOtherPlayers(msg, player.Connection);   
        }

        private void DistributePlayerPosition(ServerPlayer player)
        {
            NetOutgoingMessage msg = serverThread.CreateMessage();
            msg.Write((byte)MessageType.PlayerPos);
            msg.Write(player.PlayerID);
            msg.Write((byte)(++player.SequenceNo));
            msg.Write(player.PlayerPos.x);
            msg.Write(player.PlayerPos.y);
            msg.Write(player.PlayerPos.z);
            msg.Write(player.PlayerPos.rotation);
            msg.Write(player.PlayerPos.velocity);
            msg.Write(DateTime.UtcNow.Ticks);
            SendToAllOtherPlayers(msg, player.Connection);
        }

        private void SendOKHandshake(ServerPlayer player)
        {
            NetOutgoingMessage msg = serverThread.CreateMessage();
            msg.Write((byte)MessageType.OK);
            msg.Write(player.PlayerID);
            msg.Write(genSeed);
            serverThread.SendMessage(msg, player.Connection, NetDeliveryMethod.Unreliable);
            Console.WriteLine("Sent OK handshake to player " + player.Connection.RemoteEndPoint.Address);
        }

        private void SendToAllOtherPlayers(NetOutgoingMessage msg, NetConnection exceptPlayer)
        {
            List<NetConnection> otherPlayers = serverThread.Connections.Where(c => c != exceptPlayer).ToList();
            if (otherPlayers.Count > 0)
                serverThread.SendMessage(msg, otherPlayers, NetDeliveryMethod.Unreliable, 0);
        }

        public void Start()
        {
            Console.WriteLine("Started server: " + serverName + ".");

            NetPeerConfiguration config = new NetPeerConfiguration("DATX02");
            config.Port = port;

            serverThread = new NetServer(config);
            serverThread.Start();
            running = true;

            Run();
        }

        public void Shutdown()
        {
            Console.WriteLine("Shutting down server...");
        }

        public void PlayerConnected(NetConnection connection)
        {
            if (state == ServerState.Lobby)
            {
                serverThread.SendDiscoveryResponse(null, connection.RemoteEndPoint);
                Console.WriteLine("Player " + connection.RemoteEndPoint.Address + " connected!");
                Console.WriteLine("Current players: " + serverThread.ConnectionsCount);
                foreach (NetConnection conn in serverThread.Connections)
                    Console.WriteLine(conn.RemoteEndPoint.Address);

                ServerPlayer player = new ServerPlayer(++playerIdCounter, connection);
                players[connection.RemoteEndPoint.Address] = player;
            }
        }

        public void PlayerDisconnected(NetConnection connection)
        {
            Console.WriteLine("Player " + connection.RemoteEndPoint.Address + " disconnected!");
            Console.WriteLine("Current players: " + serverThread.ConnectionsCount);
            foreach (NetConnection conn in serverThread.Connections)
                Console.WriteLine(conn.RemoteEndPoint.Address);

            players.Remove(connection.RemoteEndPoint.Address);
            DistributeLobbyUpdate();
        }
    }
}
