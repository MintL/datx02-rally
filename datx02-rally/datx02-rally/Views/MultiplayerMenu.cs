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
        enum State { Connected, Unconnected }
        private State state = State.Unconnected;
        List<MenuItem> lobbyItems = new List<MenuItem>();
        TextInputMenuItem serverInput;

        public MultiplayerMenu(Game game) : base(game, GameState.MultiplayerMenu) 
        {
            MenuTitle = "Multiplayer Game";
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Vector2 size = GetScreenPosition(new Vector2(0.5f, 0.5f));
            Bounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);

            serverInput = new TextInputMenuItem("Server IP", "server");
            serverInput.Bounds = Bounds;
            serverInput.Font = MenuFont;
            serverInput.Background = OptionSelected;
            serverInput.FontColor = ItemColor;
            serverInput.FontColorSelected = Color.Black;
            AddMenuItem(serverInput);

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
            var serverClient = gameInstance.GetService<ServerClient>();
            // prepare lobby (once)
            if (state == State.Unconnected && serverClient.connected)
            {
                state = State.Connected;

                MenuItem startButton = GetMenuItem("start");
                startButton.Text = "Start Game!";

                Vector2 size = GetScreenPosition(new Vector2(1f, 1.6f));
                Bounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);

                MenuItems.Remove(serverInput);

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
            else if (state == State.Connected && !serverClient.connected)
            {
                state = State.Unconnected;

                Vector2 size = GetScreenPosition(new Vector2(1f, 0.625f));
                Bounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);

                MenuItems.RemoveAll(i => lobbyItems.Exists(i2 => i == i2));
                lobbyItems.Clear();

                MenuItems.Insert(0, serverInput);
            }

            // update lobby if connected
            if (state == State.Connected)
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
