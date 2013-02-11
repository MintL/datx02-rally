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
        SpriteFont Font;
        KeyboardState PrevKeyState;
        Boolean connected = false;
        Boolean enabled = false;

        public HUDConsoleComponent(Game1 game) : base(game)
        {
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

        public override void Draw(GameTime gameTime)
        {
            if (enabled)
            {
                Game1 game = Game as Game1;
                Vector2 topLeft = new Vector2(0, 0);
                Vector2 bottomLeft = new Vector2(0, game.GraphicsDevice.Viewport.Height - Font.LineSpacing);

                game.spriteBatch.Begin();
                game.spriteBatch.DrawString(Font, "DATX02 Rally", topLeft, Color.White);
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
            else if (key == Keys.Back)
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
            EnteredCommand.Clear();
            switch (command[0])
            {
                case "CONNECT":
                    IPAddress ip;
                    if (!connected && IPAddress.TryParse(command[1], out ip))
                    {
                        Console.WriteLine("Connecting to server: " + command[1]);
                        System.Threading.ThreadPool.QueueUserWorkItem(delegate
                        {
                            ServerClient client = new ServerClient();
                            client.Connect(ip);
                            System.Threading.Thread.Sleep(5000);
                            client.SendTestData();
                            Console.WriteLine("Sent test data!");
                        }, null);
                        connected = true;
                    }
                    else
                    {
                        Console.WriteLine("Retard! Cannot connect to server " + command[1]);
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
