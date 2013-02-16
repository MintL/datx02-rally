using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Microsoft.Xna.Framework;

namespace datx02_rally.DebugConsole.Commands
{
    class ConnectCommand : ICommand
    {
        public string Name
        {
            get { return "connect"; }
        }

        public string[] Description
        {
            get { return new string[] {"Connect to a server"}; }
        }

        private ServerClient client;

        public ConnectCommand(ServerClient client)
        {
            this.client = client;
        }

        public string[] Execute(string[] arguments)
        {
            IPAddress ip;
            List<string> output = new List<string>();
            if (!client.connected && arguments.Length > 1 && IPAddress.TryParse(arguments[1], out ip))
            {
                client.Connect(ip);
                output.Add("Connecting to server " + arguments[1] + "...");
            }
            else
            {
                if (arguments.Length == 1)
                {
                    output.Add("Can't connect to server.");
                }
                else
                {
                    output.Add("Can't connect to server " + arguments[1] + ".");
                    output.Add("You may be already connected or entered a faulty IP");
                }
            }
            return output.ToArray();
        }
    }
}
