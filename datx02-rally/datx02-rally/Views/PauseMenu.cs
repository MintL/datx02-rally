using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.Menus
{
    class PauseMenu : OverlayView
    {
		public GameState NextState { get; private set; } 
        public PauseMenu(Game game) : base(game, GameState.PausedGameplay) 
        {
            UpdateOrder = DrawOrder = 1;
            MenuTitle = "Pause";
			NextState = GameState.PausedGameplay;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Vector2 size = GetScreenPosition(new Vector2(0.3f, 0.6f));
            Bounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);

            List<Tuple<String, GameState>> itemInfo = new List<Tuple<string, GameState>>();
            itemInfo.Add(new Tuple<String, GameState>("Resume", GameState.Gameplay));
            itemInfo.Add(new Tuple<String, GameState>("Main menu", GameState.MainMenu));
            itemInfo.Add(new Tuple<String, GameState>("Quit", GameState.Exiting));
            
            foreach (var info in itemInfo) 
            {
                MenuItem item = new StateActionMenuItem(info.Item1, info.Item2);
                item.Background = ButtonBackground;
                item.Font = MenuFont;
                item.FontColorSelected = ItemColorSelected;
                item.SetWidth(Bounds.Width);
                AddMenuItem(item);
            }
        }

        public override void Update(GameTime gameTime)
        {
            NextState = UpdateState(gameTime); 

            base.Update(gameTime);
        }
    }

}
