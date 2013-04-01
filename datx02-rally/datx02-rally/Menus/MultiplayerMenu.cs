using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally.Menus
{
    class MultiplayerMenu : OverlayView
    {

        public MultiplayerMenu(Game game) : base(game, GameState.MultiplayerMenu) { }

        protected override void LoadContent()
        {
            base.LoadContent();

            Vector2 size = GetScreenPosition(new Vector2(0.5f, 0.6f));
            Bounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);

            TextInputMenuItem serverIP = new TextInputMenuItem("Server", "server");
            serverIP.Bounds = Bounds;
            serverIP.Font = MenuFont;
            serverIP.Background = OptionSelected;
            serverIP.FontColor = ItemColor;
            serverIP.FontColorSelected = Color.Black;
            AddMenuItem(serverIP);

            MenuItem item = new ActionMenuItem("Connect", ConnectToServer);
            item.Background = ButtonBackground;
            item.Font = MenuFont;
            item.FontColor = ItemColor;
            item.FontColorSelected = ItemColorSelected;
            item.SetWidth(Bounds.Width);
            AddMenuItem(item);

            item = new StateActionMenuItem("Cancel", GameState.MainMenu);
            item.Background = ButtonBackground;
            item.Font = MenuFont;
            item.FontColor = ItemColor;
            item.FontColorSelected = ItemColorSelected;
            item.SetWidth(Bounds.Width);
            AddMenuItem(item);

            item = new StateActionMenuItem("Not Connected...", GameState.Gameplay, "start");
            item.Background = ButtonBackground;
            item.Font = MenuFont;
            item.FontColor = ItemColor;
            item.FontColorSelected = ItemColorSelected;
            item.SetWidth(Bounds.Width);
            item.Enabled = false;
            AddMenuItem(item);
        }

        private void ConnectToServer()
        {
            string server = (GetMenuItem("server") as TextInputMenuItem).GetEnteredText();
            gameInstance.GetService<ServerClient>().Connect(System.Net.IPAddress.Parse(server));
        }

        public override GameState UpdateState(GameTime gameTime)
        {
            if (gameInstance.GetService<ServerClient>().connected)
            {
                MenuItem startButton = GetMenuItem("start");
                startButton.Text = "Start Game!";
                startButton.Enabled = true;
            }
            return base.UpdateState(gameTime);
        }
    }
}
