using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.Menus
{
    class LoadingMenu : OverlayView
    {
        private StateActionMenuItem item = new StateActionMenuItem("Loading:\nInitializing", GameState.None);
        public LoadingMenu(Game game)
            : base(game, GameState.None) 
        {
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Vector2 size = GetScreenPosition(new Vector2(0.3f, 0.6f));
            Bounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);

            item.Background = ButtonBackground;
            item.Font = MenuFont;
            item.SetWidth(Bounds.Width);
            AddMenuItem(item);
        }

        public void SetProgress(string text) 
        {
            item.Text = "Loading:\n" + text;
        }
    }

}
