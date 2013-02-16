using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Microsoft.Xna.Framework;

namespace datx02_rally.DebugConsole.Commands
{
    class ShowCommand : ICommand
    {
        public string Name
        {
            get { return "show"; }
        }

        public string[] Description
        {
            get { return new string[] {"Show details about the game status"}; }
        }

        private ServerClient client;

        public ShowCommand(ServerClient client)
        {
            this.client = client;
        }

        public string[] Execute(string[] arguments)
        {
            List<string> output = new List<string>();
            if (arguments.Length > 1)
            {
                switch (arguments[1].ToLower())
                {
                    //case "playerpos":
                    //    output.Add("Player position: " + client.LocalPlayer.Position.ToString());
                    //    break;
                    case "players":
                        if (client.connected)
                        {
                            StringBuilder outputBuilder = new StringBuilder("Remote players: \n");
                            if (client.Players.Values.Count > 1)
                            {
                                for (int i = 0; i < client.Players.Values.Count - 1; i++)
                                {
                                    var player = client.Players.Values.ElementAt(i);
                                    outputBuilder.Append(player.PlayerName).Append(", ");
                                }
                            }
                            if (client.Players.Values.Count > 0)
                            {
                                outputBuilder.Append(client.Players.Values.Last().PlayerName);
                            }
                            output.Add(outputBuilder.ToString());
                        }
                        else
                        {
                            output.Add("Not connected to a server");
                        }
                        break;
                    case "chat":
                        if (client.connected)
                        {
                            var lastChatMsg = client.ChatHistory.Last.Value;
                            if (lastChatMsg == null)
                                output.Add("(" + lastChatMsg.Item3.TimeOfDay + ") " + lastChatMsg.Item1 + ": " + lastChatMsg.Item2);
                        }
                        else
                        {
                            output.Add("Not connected to a server");
                        }
                        break;
                    case "player":
                        if (arguments.Length > 2 && client.connected)
                        {
                            Player matchPlayer = client.Players.Values.ToList().Find(c => c.PlayerName.Equals(arguments[2]));
                            if (matchPlayer != null)
                            {
                                output.Add(arguments[2] + ": position " + matchPlayer.Position);
                            }
                            else
                            {
                                output.Add("No player named " + arguments[2] + " found!");
                            }
                        }
                        else
                        {
                            output.Add(client.LocalPlayer.PlayerName+ ": position " + client.LocalPlayer.Position);
                        }
                        break;
                }
            }
            return output.ToArray();
        }
    }
}
