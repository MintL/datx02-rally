using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace datx02_rally.DebugConsole.Commands
{
    class NameCommand : ICommand
    {
        public string Name
        {
            get { return "name"; }
        }

        public string[] Description
        {
            get { return new string[] {"Set the name of the local player"}; }
        }

        private ServerClient Client;

        public NameCommand(ServerClient client)
        {
            Client = client;
        }

        public string[] Execute(string[] arguments)
        {
            List<string> output = new List<string>();
            if (arguments.Length > 1)
            {
                string playerName = arguments[1];
                Client.SetPlayerName(playerName);
                return new string[] { "New name: " + playerName };
            }
            else
            {
                return new string[] { "Name not changed: name must be specified" };
            }
        }
    }
}
