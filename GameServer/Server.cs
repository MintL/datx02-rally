using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Net;

namespace GameServer
{
    class Server
    {
        byte PlayerIdCounter = 0;
        string ServerName;
        readonly int MaxPlayers;
        Dictionary<IPAddress, ServerPlayer> Players;
        readonly int Port;
        NetServer serverThread;
        Boolean running;

        public enum MessageType { PlayerPos, Chat, Debug, LobbyUpdate }

        public Server()
        {
            ServerName = "Test server";
            MaxPlayers = 4;
            Players = new Dictionary<IPAddress, ServerPlayer>();
            Port = 19283;

            Start();
        }

        private void Run()
        {
            NetIncomingMessage msg;
            while (running)
            {
                while ((msg = serverThread.ReadMessage()) != null)
                {
                    HandleIncomingPacket(msg);
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
            Console.Write("Received data from player " + msg.SenderEndPoint.Address);
            
            ServerPlayer player;
            if (!Players.TryGetValue(msg.SenderEndPoint.Address, out player)) 
            {
                Console.WriteLine("Player not found!");
                return;
            }

            MessageType type = (MessageType)msg.ReadByte();
            switch (type)
            {
                case MessageType.PlayerPos:
                    double msGameTime = msg.ReadDouble();
                    float x = msg.ReadFloat(); float y = msg.ReadFloat(); float z = msg.ReadFloat();
                    Console.WriteLine(" of type PlayerPos: X:{0}, Y:{1}, Z:{2}",x,y,z);
                    player.UpdatePosition(x, y, z);
                    DistributePlayerPosition(player);
                    break;
                case MessageType.Chat:
                    string chatMsg = msg.ReadString();
                    string chatSender = player.PlayerName;
                    Console.WriteLine(" of type chat message: '{0}: {1}", chatSender, chatMsg);
                    DistributeChatMessage(player, chatMsg);
                    break;
                case MessageType.Debug:
                    string debugMsg = msg.ReadString();
                    Console.WriteLine(" DEBUG: " + debugMsg);
                    break;
                default:
                    Console.WriteLine(" of unknown type!");
                    break;
            }
        }

        private void DistributeChatMessage(ServerPlayer player, string chatMsg)
        {
            NetOutgoingMessage msg = serverThread.CreateMessage();
            msg.Write((byte)MessageType.Chat);
            msg.Write(player.PlayerName);
            msg.Write(chatMsg);
            SendToAllOtherPlayers(msg, player.Connection);            
        }

        private void DistributePlayerPosition(ServerPlayer player)
        {
            NetOutgoingMessage msg = serverThread.CreateMessage();
            msg.Write((byte)MessageType.PlayerPos);
            msg.Write(player.PlayerPos.x);
            msg.Write(player.PlayerPos.y);
            msg.Write(player.PlayerPos.z);
            SendToAllOtherPlayers(msg, player.Connection);
        }

        private void SendToAllOtherPlayers(NetOutgoingMessage msg, NetConnection exceptPlayer)
        {
            foreach (var playerConnection in serverThread.Connections)
            {
                if (playerConnection != exceptPlayer)
                    serverThread.SendMessage(msg, playerConnection, NetDeliveryMethod.Unreliable);
            }
        }

        public void Start()
        {
            Console.WriteLine("Started server: " + ServerName + ".");

            NetPeerConfiguration config = new NetPeerConfiguration("DATX02");
            config.Port = Port;

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
            serverThread.SendDiscoveryResponse(null, connection.RemoteEndPoint);
            Console.WriteLine("Player " + connection.RemoteEndPoint.Address + " connected!");
            Console.WriteLine("Current players: " + serverThread.ConnectionsCount);
            foreach (NetConnection conn in serverThread.Connections)
                Console.WriteLine(conn.RemoteEndPoint.Address);

            Players[connection.RemoteEndPoint.Address] = new ServerPlayer(++PlayerIdCounter, connection);
        }

        public void PlayerDisconnected(NetConnection connection)
        {
            // TODO
        }
    }
}
