using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Microsoft.Xna.Framework;

namespace datx02_rally.DebugConsole.Commands
{
    class ChatCommand : ICommand
    {
        public string Name
        {
            get { return "chat"; }
        }

        public string[] Description
        {
            get { return new string[] {"Send a message to the other players connected to the server"}; }
        }

        private ServerClient client;

        public ChatCommand(ServerClient client)
        {
            this.client = client;
        }

        public string[] Execute(string[] arguments)
        {
            List<string> output = new List<string>();
            if (client.connected)
            {
                string chatMsg = String.Join(" ", arguments, 1, arguments.Length - 1);
                client.Chat(chatMsg);
                output.Add(client.LocalPlayer.PlayerName + ": " + chatMsg);
            }
            else
            {
                output.Add("Not connected to a server");
            }
            return output.ToArray();
        }
    }
}
