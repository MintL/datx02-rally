﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using datx02_rally.DebugConsole.Commands;
using datx02_rally.DebugConsole;

namespace datx02_rally
{
    class HUDConsoleComponent : DrawableGameComponent
    {
        private StringBuilder EnteredCommand = new StringBuilder();
        private readonly string CONSOLE_HEADING = "DATX02-Racing";
        
        private List<string> OutputHistory = new List<string>();
        public int MaxHistory = 11;

        public List<ICommand> Commands = new List<ICommand>();

        SpriteFont Font;
        Texture2D Background;
        KeyboardState PrevKeyState;
        Boolean enabled = false;
        Game1 Game1;

        public HUDConsoleComponent(Game1 game)
            : base(game)
        {
            Game1 = game;
        }

        protected override void LoadContent()
        {
            Font = Game.Content.Load<SpriteFont>(@"ConsoleFont");
            Background = Game.Content.Load<Texture2D>("Console");
            WriteOutput(CONSOLE_HEADING);

            ServerClient client = Game.GetService<ServerClient>();
            Commands.Add(new ConnectCommand(client));
            Commands.Add(new DisconnectCommand(client));
            Commands.Add(new ChatCommand(client));
            Commands.Add(new ShowCommand(client));
            Commands.Add(new ClearCommand(this));
            Commands.Add(new ExitCommand(Game));
            Commands.Add(new HelpCommand(this));
            
            base.LoadContent();
        }
        
        public override void Update(GameTime gameTime)
        {
            if (enabled)
            {
                KeyboardState keyState = Keyboard.GetState();
                foreach (var key in keyState.GetPressedKeys())
                {
                    if (PrevKeyState.IsKeyUp(key))
                        AddKey(key, keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift) );
                }
                PrevKeyState = keyState;
            }

            base.Update(gameTime);
        }

        public void WriteOutput(string output) 
        {
            OutputHistory.Add(output);
            if (OutputHistory.Count > MaxHistory)
            {
                OutputHistory.RemoveAt(0);
            }
        }

        public void ClearOutput()
        {
            OutputHistory.Clear();
        }

        public override void Draw(GameTime gameTime)
        {
            if (enabled)
            {
                Vector2 topLeft = new Vector2(15, 10);
                Vector2 commandPosition = new Vector2(5, 251 - Font.LineSpacing);

                Game1.spriteBatch.Begin();
                Game1.spriteBatch.Draw(Background, new Rectangle(0, 0, Game1.GraphicsDevice.Viewport.Width, 256), Color.White);

                for (int i = 0; i < OutputHistory.Count; i++)
                {
                    Game1.spriteBatch.DrawString(Font, OutputHistory[i], topLeft + new Vector2(0, Font.LineSpacing * i), Color.White);
                }
                Game1.spriteBatch.DrawString(Font, ">" + EnteredCommand.ToString(), commandPosition, Color.White);
                Game1.spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private void AddKey(Keys key, bool modifier)
        {
            String sKey = key.ToString();
            if (key == Keys.Space)
            {
                EnteredCommand.Append(" ");
            }
            else if (sKey.Length == 1 && Char.IsLetter(sKey[0]))
            {
                if (modifier)
                    EnteredCommand.Append(Char.ToUpper(sKey[0]));
                else
                    EnteredCommand.Append(Char.ToLower(sKey[0]));
            }
            else if (key == Keys.OemPeriod)
            {
                EnteredCommand.Append(".");
            }
            else if (key >= Keys.D0 && key <= Keys.D9)
            {
                EnteredCommand.Append(sKey[1]);
            }
            else if (key == Keys.Back && EnteredCommand.Length > 0)
            {
                EnteredCommand.Remove(EnteredCommand.Length - 1, 1);
            }
            else if (key == Keys.Enter)
            {
                ParseCommand();
            }
            else if (key == Keys.Tab)
            {
                AutoComplete();
            }
        }

        private void ParseCommand()
        {
            string[] arguments = EnteredCommand.ToString().Split(" ".ToCharArray());
            ServerClient client = Game.GetService<ServerClient>();
            EnteredCommand.Clear();

            ICommand command = Commands.Find(x => x.Name.ToLower().Equals(arguments[0]));
            if (command != null)
            {
                string[] outputs = command.Execute(arguments);
                foreach (string output in outputs)
                {
                    WriteOutput(output);
                }
            }
            else
            {
                WriteOutput("Unknown command: " + String.Join(" ", arguments));
            }
            /*switch (command[0])
            {
                case "CONNECT":
                    IPAddress ip;
                    if (!client.connected && IPAddress.TryParse(command[1], out ip))
                    {
                        WriteOutput("Connecting to server " + command[1] + "...");
                        client.Connect(ip);
                    }
                    else
                    {
                        WriteOutput("Cannot connect to server " + command[1] + ". You may be already connected or entered a faulty IP");
                    }
                    break;
                case "DISCONNECT":
                    Game.GetService<ServerClient>().Disconnect();
                    WriteOutput("Disconnected!");
                    break;
                case "CHAT":
                    if (client.connected)
                    {
                        string chatMsg = String.Join(" ", command, 1, command.Length - 1);
                        client.Chat(chatMsg);
                    }
                    break;
                case "SHOW":
                    if (command.Length > 1)
                        HandleShowCommand(command);
                    break;
                case "CLEAR":
                    WriteOutput("");
                    break;
                default:
                    WriteOutput("Unknown command: " + String.Join(" ", command));
                    break;
            }*/
            
        }

        private void AutoComplete()
        {
            string command = EnteredCommand.ToString().Split(" ".ToCharArray()).LastOrDefault();
            if (command.TrimEnd().Equals("")) return;
            var match = Commands.Where(x => x.Name.StartsWith(command)).FirstOrDefault();

            if (match != null)
            {
                string restOfTheCommand = match.Name.Substring(command.Length);
                EnteredCommand.Append(restOfTheCommand).Append(" ");
            }
        }

        private void HandleShowCommand(string[] command) 
        {
            ServerClient client = Game.GetService<ServerClient>();
            switch (command[1])
            {
                case "PLAYERPOS":
                    WriteOutput("Player position: " + Game1.car.Position.ToString());
                    break;
                case "PLAYERS":
                    StringBuilder output = new StringBuilder("Remote players: \n");
                    foreach (var player in client.Players.Values)
                        output.Append(player.PlayerName);
                    WriteOutput(output.ToString());
                    break;
                case "CHAT":
                    var lastChatMsg = client.ChatHistory.Last.Value;
                    if (lastChatMsg == null)
                        WriteOutput("(" + lastChatMsg.Item3.TimeOfDay + ") " + lastChatMsg.Item1 + ": " + lastChatMsg.Item2);
                    break;
                case "PLAYER":
                    int playerIndex;
                    if (int.TryParse(command[2], out playerIndex))
                    {
                        if (client.Players.Values.Count >= playerIndex - 1)
                        {
                            Player player = client.Players.Values.ElementAt(playerIndex);
                            WriteOutput("Player name: " + player.PlayerName + "\nPosition: " + player.Position);
                        }
                        else
                        {
                            WriteOutput("No player with index " + playerIndex + " found!");
                        }
                        
                    }
                    break;
                default:
                    break;
            }
        }

        public void Toggle()
        {
            enabled = !enabled;
        }
    }
}
