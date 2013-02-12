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
        string ServerName;
        readonly int MaxPlayers;
        Dictionary<IPAddress, ServerPlayer> Players;
        readonly int Port;
        NetServer serverThread;
        Boolean running;

        public enum MessageType { PlayerPos, Chat, Debug }

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
                            PlayerConnected(msg.SenderEndPoint);
                            break;
                        case NetConnectionStatus.Disconnected:
                            PlayerDisconnected(msg.SenderEndPoint);
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
                    double msGameTime = msg.ReadFloat();
                    float x = msg.ReadFloat(); float y = msg.ReadFloat(); float z = msg.ReadFloat();
                    Console.WriteLine(" of type PlayerPos: {0},{1},{2}",x,y,z);
                    player.UpdatePosition(x, y, z);
                    DistributePlayerPosition(player);
                    break;
                case MessageType.Chat:
                    string chatMsg = msg.ReadString();
                    string chatSender = player.PlayerName;
                    Console.WriteLine(" of type chat message: '{0}: {1}", chatSender, chatMsg);
                    DistributeChatMessage(chatSender, chatMsg);
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

        private void DistributeChatMessage(string chatSender, string chatMsg)
        {
            NetOutgoingMessage msg = serverThread.CreateMessage();
            msg.Write((byte)MessageType.Chat);
            msg.Write(chatSender);
            msg.Write(chatMsg);
            serverThread.SendToAll(msg, NetDeliveryMethod.Unreliable);
        }

        private void DistributePlayerPosition(ServerPlayer player)
        {
            NetOutgoingMessage msg = serverThread.CreateMessage();
            msg.Write((byte)MessageType.PlayerPos);
            msg.Write(player.PlayerPos.x);
            msg.Write(player.PlayerPos.y);
            msg.Write(player.PlayerPos.z);
            serverThread.SendToAll(msg, NetDeliveryMethod.Unreliable);
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

        public void PlayerConnected(IPEndPoint playerIP)
        {
            serverThread.SendDiscoveryResponse(null, playerIP);
            Console.WriteLine("Player " + playerIP.Address + " connected!");
            Console.WriteLine("Current players: " + serverThread.ConnectionsCount);
            foreach (NetConnection connection in serverThread.Connections)
                Console.WriteLine(connection.RemoteEndPoint.Address);

            Players[playerIP.Address] = new ServerPlayer(serverThread.ConnectionsCount);
        }

        public void PlayerDisconnected(IPEndPoint playerIP)
        {
            // TODO
        }
    }
}
