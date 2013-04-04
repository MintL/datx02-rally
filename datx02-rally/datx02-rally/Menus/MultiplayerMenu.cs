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
        bool waitingForConnection = false;

        public MultiplayerMenu(Game game) : base(game, GameState.MultiplayerMenu) 
        {
            MenuTitle = "Multiplayer Game";
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Vector2 size = GetScreenPosition(new Vector2(0.5f, 0.5f));
            Bounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);

            TextInputMenuItem playerName = new TextInputMenuItem("Server IP", "server");
            playerName.Bounds = Bounds;
            playerName.Font = MenuFont;
            playerName.Background = OptionSelected;
            playerName.FontColor = ItemColor;
            playerName.FontColorSelected = Color.Black;
            AddMenuItem(playerName);

            MenuItem item = new ActionMenuItem("Connect", ConnectToServer, "connect");
            item.Background = ButtonBackground;
            item.Font = MenuFont;
            item.FontColor = ItemColor;
            item.FontColorSelected = ItemColorSelected;
            item.SetWidth(Bounds.Width);
            AddMenuItem(item);

            item = new StateActionMenuItem("Cancel", GameState.MainMenu, "cancel");
            item.Background = ButtonBackground;
            item.Font = MenuFont;
            item.FontColor = ItemColor;
            item.FontColorSelected = ItemColorSelected;
            item.SetWidth(Bounds.Width);
            AddMenuItem(item);
        }

        private void ConnectToServer()
        {
            string server = (GetMenuItem("server") as TextInputMenuItem).EnteredText;
            gameInstance.GetService<ServerClient>().Connect(System.Net.IPAddress.Parse(server));
            waitingForConnection = true;

            MenuItem start = new StateActionMenuItem("Connecting...", GameState.Gameplay, "start");
            start.Background = ButtonBackground;
            start.Font = MenuFont;
            start.Enabled = false;
            start.FontColor = ItemColor;
            start.FontColorSelected = ItemColorSelected;
            start.SetWidth(Bounds.Width);
            SetMenuItem("connect", start);

            MenuItem disconnect = new ActionMenuItem("Disconnect", Disconnect, "disconnect");
            disconnect.Background = ButtonBackground;
            disconnect.Font = MenuFont;
            disconnect.FontColor = ItemColor;
            disconnect.FontColorSelected = ItemColorSelected;
            disconnect.SetWidth(Bounds.Width);
            SetMenuItem("cancel", disconnect);
        }

        private void Disconnect()
        {
            gameInstance.GetService<ServerClient>().Disconnect();

            MenuItem item = new ActionMenuItem("Connect", ConnectToServer, "connect");
            item.Background = ButtonBackground;
            item.Font = MenuFont;
            item.FontColor = ItemColor;
            item.FontColorSelected = ItemColorSelected;
            item.SetWidth(Bounds.Width);
            SetMenuItem("start", item);

            item = new StateActionMenuItem("Cancel", GameState.MainMenu, "cancel");
            item.Background = ButtonBackground;
            item.Font = MenuFont;
            item.FontColor = ItemColor;
            item.FontColorSelected = ItemColorSelected;
            item.SetWidth(Bounds.Width);
            SetMenuItem("disconnect", item);
        }

        public override GameState UpdateState(GameTime gameTime)
        {
            if (waitingForConnection && gameInstance.GetService<ServerClient>().connected)
            {
                waitingForConnection = false;

                MenuItem startButton = GetMenuItem("start");
                startButton.Text = "Start Game!";
                startButton.Enabled = true;
            }
            return base.UpdateState(gameTime);
        }
    }
}
