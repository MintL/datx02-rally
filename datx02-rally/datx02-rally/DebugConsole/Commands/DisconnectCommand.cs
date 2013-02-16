using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Microsoft.Xna.Framework;

namespace datx02_rally.DebugConsole.Commands
{
    class DisconnectCommand : ICommand
    {
        public string Name
        {
            get { return "disconnect"; }
        }

        public string[] Description
        {
            get { return new string[] {"Disconnect from the connected server"}; }
        }

        private ServerClient client;

        public DisconnectCommand(ServerClient client)
        {
            this.client = client;
        }

        public string[] Execute(string[] arguments)
        {
            client.Disconnect();
            return new string[] {"Disconnected!"};
        }
    }
}
