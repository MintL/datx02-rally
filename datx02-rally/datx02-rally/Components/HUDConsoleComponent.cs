using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Net;

namespace datx02_rally
{
    class HUDConsoleComponent : DrawableGameComponent
    {
        private StringBuilder EnteredCommand = new StringBuilder();
        private readonly string CONSOLE_HEADING = "DATX02-Racing";
        private string ReceivedOutput;
        SpriteFont Font;
        KeyboardState PrevKeyState;
        Boolean enabled = false;
        Game1 Game;

        public HUDConsoleComponent(Game1 game) : base(game)
        {
            Game = game;
        }

        protected override void LoadContent()
        {
            Font = Game.Content.Load<SpriteFont>(@"ConsoleFont");
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
                        AddKey(key);
                }
                PrevKeyState = keyState;
            }

            base.Update(gameTime);
        }

        public void WriteOutput(string output) 
        {
            ReceivedOutput = output;
        }

        public override void Draw(GameTime gameTime)
        {
            if (enabled)
            {
                Game1 game = Game as Game1;
                Vector2 topLeft = new Vector2(0, 0);
                Vector2 bottomLeft = new Vector2(0, game.GraphicsDevice.Viewport.Height - Font.LineSpacing);

                game.spriteBatch.Begin();
                game.spriteBatch.DrawString(Font, CONSOLE_HEADING+"\n"+ReceivedOutput, topLeft, Color.White);
                game.spriteBatch.DrawString(Font, EnteredCommand.ToString(), bottomLeft, Color.White);
                game.spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private void AddKey(Keys key)
        {
            String sKey = key.ToString();
            if (key == Keys.Space)
            {
                EnteredCommand.Append(" ");
            }
            else if (sKey.Length == 1 && Char.IsLetter(sKey[0]))
            {
                EnteredCommand.Append(sKey[0]);
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
        }

        private void ParseCommand()
        {
            string[] command = EnteredCommand.ToString().Split(" ".ToCharArray());
            ServerClient client = Game.GetService<ServerClient>();
            EnteredCommand.Clear();
            switch (command[0])
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
                default:
                    WriteOutput("Unknown command: " + String.Join(" ", command));
                    break;
            }
            
        }

        public void Toggle()
        {
            enabled = !enabled;
        }
    }
}
