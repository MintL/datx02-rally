using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer
{
    class Server
    {
        string ServerName;
        readonly int MaxPlayers;
        ServerPlayer[] Players;

        public Server()
        {
            ServerName = "Test server";
            MaxPlayers = 4;
            Players = new ServerPlayer[4];

            Start();
        }

        public void Start()
        {
            Console.WriteLine("Started server: "+ServerName+".");
        }

        public void Shutdown()
        {
            Console.WriteLine("Shutting down server...");
        }

        public void PlayerConnected()
        {
            // TODO
        }

        public void PlayerDisconnected()
        {
            // TODO
        }
    }
}
