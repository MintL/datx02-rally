using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer
{
    class ServerPlayer
    {
        string PlayerName;
        readonly int PlayerID;

        public ServerPlayer(int id)
        {
            PlayerID = id;
            PlayerName = "Player "+id;
        }
    }
}
