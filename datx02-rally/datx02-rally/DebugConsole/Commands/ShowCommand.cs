using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Microsoft.Xna.Framework;
using datx02_rally.Components;

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
                Game1 game = Game1.GetInstance();
                switch (arguments[1].ToLower())
                {
                    case "players":
                        if (client.connected)
                        {
                            string playerList = String.Join(", ", client.Players.Values.Select(p => p.PlayerName)); 
                            output.Add("Remote players: " + playerList);
                        }
                        else
                        {
                            output.Add("Not connected to a server");
                        }
                        break;
                    case "chat":
                        if (client.connected)
                        {
                            var lastChatMsg = client.ChatHistory.Last;
                            if (lastChatMsg != null)
                            {
                                output.Add("(" + lastChatMsg.Value.Item3.TimeOfDay + ") " + lastChatMsg.Value.Item1 + ": " + lastChatMsg.Value.Item2);
                            }
                            else
                            {
                                output.Add("No chat messages to show.");
                            }
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
                                output.Add(arguments[2] + ": position " + matchPlayer.GetPosition());
                            }
                            else
                            {
                                output.Add("No player named " + arguments[2] + " found!");
                            }
                        }
                        else
                        {
                            output.Add(client.LocalPlayer.PlayerName+ ": position " + client.LocalPlayer.GetPosition());
                        }
                        break;
                    case "name":
                        output.Add("Name: " + client.LocalPlayer.PlayerName);
                        break;
                    case "fps":
                        game.GetService<HUDComponent>().ConsoleEnabled = true;
                        break;
                    case "velocity":
                        game.GetService<HUDComponent>().SpeedEnabled = true;
                        break;
                    default:
                        output.Add("Unknown show command: "+String.Join(" ",arguments, 1, arguments.Length-1));
                        break;
                }
            }
            return output.ToArray();
        }
    }
}
