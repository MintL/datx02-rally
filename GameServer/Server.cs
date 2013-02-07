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
        ServerPlayer[] Players;
        readonly int Port;
        NetServer serverThread;
        Boolean running;

        public Server()
        {
            ServerName = "Test server";
            MaxPlayers = 4;
            Players = new ServerPlayer[4];
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
            Console.Write("Received data from player " + msg.SenderEndPoint.Address + ": " + msg.ReadString());
        }

        public void Start()
        {
            Console.WriteLine("Started server: "+ServerName+".");

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
        }

        public void PlayerDisconnected(IPEndPoint playerIP)
        {
            // TODO
        }
    }
}
