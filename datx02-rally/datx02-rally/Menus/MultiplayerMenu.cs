using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.Menus
{
    class MultiplayerMenu : MenuView
    {
        bool connected = false;
        OverlayView connectOverlay;

        public MultiplayerMenu(Game game) : base(game, GameState.MultiplayerMenu) { }

        protected override void LoadContent()
        {
            connectOverlay = new OverlayView(gameInstance);
            base.LoadContent();
        }
    }
}
