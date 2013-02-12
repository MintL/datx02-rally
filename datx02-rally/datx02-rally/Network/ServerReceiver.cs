using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Lidgren.Network;

namespace datx02_rally
{
    class ServerReceiver
    {
        NetClient Client;

        public ServerReceiver(NetClient Client)
        {
            this.Client = Client;
        }


    }
}
