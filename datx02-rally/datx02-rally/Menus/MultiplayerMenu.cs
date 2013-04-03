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

            Vector2 size = GetScreenPosition(new Vector2(0.5f, 0.5f));
            Bounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);

            TextInputMenuItem playerName = new TextInputMenuItem("Server IP", "server");
            playerName.Bounds = Bounds;
            playerName.Font = MenuFont;
            playerName.Background = OptionSelected;
            playerName.FontColor = ItemColor;
            playerName.FontColorSelected = Color.Black;
            AddMenuItem(playerName);

            MenuItem item = new StateActionMenuItem("Cancel", GameState.MainMenu);
            item.Background = ButtonBackground;
            item.Font = MenuFont;
            item.FontColor = ItemColor;
            item.FontColorSelected = ItemColorSelected;
            item.SetWidth(Bounds.Width);
            AddMenuItem(item);

            item = new StateActionMenuItem("Apply", GameState.MainMenu);
            item.Background = ButtonBackground;
            item.Font = MenuFont;
            item.FontColor = ItemColor;
            item.FontColorSelected = ItemColorSelected;
            item.SetWidth(Bounds.Width);
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
