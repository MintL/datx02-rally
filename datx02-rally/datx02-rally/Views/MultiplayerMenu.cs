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
        List<MenuItem> lobbyItems = new List<MenuItem>();
        TextInputMenuItem playerNameInput;

        public MultiplayerMenu(Game game) : base(game, GameState.MultiplayerMenu) 
        {
            MenuTitle = "Multiplayer Game";
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Vector2 size = GetScreenPosition(new Vector2(0.5f, 0.5f));
            Bounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);

            playerNameInput = new TextInputMenuItem("Server IP", "server");
            playerNameInput.Bounds = Bounds;
            playerNameInput.Font = MenuFont;
            playerNameInput.Background = OptionSelected;
            playerNameInput.FontColor = ItemColor;
            playerNameInput.FontColorSelected = Color.Black;
            AddMenuItem(playerNameInput);

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

            MenuItems.Insert(0, playerNameInput);

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
            var serverClient = gameInstance.GetService<ServerClient>();
            // prepare lobby (once)
            if (waitingForConnection && serverClient.connected)
            {
                waitingForConnection = false;

                MenuItem startButton = GetMenuItem("start");
                startButton.Text = "Start Game!";

                Vector2 size = GetScreenPosition(new Vector2(1.5f, 1.5f));
                Bounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);

                MenuItems.Remove(playerNameInput);

                MenuItem item = new TextMenuItem("Players", null, "playerHeading");
                item.Background = OptionSelected;
                item.Font = MenuHeaderFont;
                item.FontColor = ItemColor;
                item.FontColorSelected = ItemColor;
                MenuItems.Insert(0, item);
                lobbyItems.Add(item);

                foreach (var menuItem in MenuItems)
                {
                    menuItem.SetWidth(Bounds.Width);
                }
                
                startButton.Enabled = true;
            }

            // update lobby
            if (serverClient.connected)
            {
                var fullPlayerList = new List<Player>(serverClient.Players.Values);
                fullPlayerList.Add(serverClient.LocalPlayer);
                fullPlayerList.OrderBy(p => p.ID);

                foreach (var player in fullPlayerList)
                {
                    if (!lobbyItems.Exists(item => item.Identifier == player.PlayerName + player.ID))
                    {
                        MenuItem item = new TextMenuItem(player.ID+". "+player.PlayerName, null, player.PlayerName+player.ID);
                        item.Background = OptionSelected;
                        item.Font = MenuFont;
                        item.FontColor = ItemColor;
                        item.FontColorSelected = ItemColor;
                        item.SetWidth(Bounds.Width);

                        lobbyItems.Add(item);
                        MenuItems.Insert(MenuItems.Count - 2, item);
                    }
                }
            }
            return base.UpdateState(gameTime);
        }
    }
}
