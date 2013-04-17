using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace datx02_rally.Menus
{
    class GameOverMenu : OverlayView
    {
        private bool populated = false;

		public GameState NextState { get; private set; }
        public GameOverMenu(Game game)
            : base(game, GameState.GameOver) 
        {
            UpdateOrder = DrawOrder = 1;
            MenuTitle = "Game over";
			NextState = GameState.GameOver;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Vector2 size = GetScreenPosition(new Vector2(0.7f, 0.7f));
            Bounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);
            
        }

        public override GameState UpdateState(GameTime gameTime)
        {
            var state = base.UpdateState(gameTime);
            if (gameInstance.GetService<InputComponent>().GetKey(Keys.Enter))
                return GameState.MainMenu;
            return state;
        }

        public override void Update(GameTime gameTime)
        {
            var mode = gameInstance.GetService<GameplayMode>();
            if (mode.GameOver && !populated)
            {
                if (mode.Statistics.Won)
                    this.MenuTitle = "You won!";
                else
                    this.MenuTitle = "You lost!";
                foreach (var heading in mode.Statistics.CategorizedItems)
	            {
                    if (heading.Title != null)
                        AddTextItem(true, heading.Title, null);
                    foreach (var item in heading.Items)
                        AddTextItem(false, item.Key, item.Value);
	            }
                populated = true;

                AddTextItem(false, "\nPress enter to return to Main Menu!", null);
            }

            NextState = UpdateState(gameTime);

            base.Update(gameTime);
        }

        public void AddTextItem(bool heading, string columnOne, string columnTwo)
        {
            if (heading) // some spacing before headings
                AddTextItem(false, " ", null);
            MenuItem item = new TextMenuItem(columnOne, columnTwo);
            item.Bounds = Bounds;
            item.Font = heading ? MenuHeaderFont : MenuFont;
            item.Background = OptionSelected;
            item.FontColor = ItemColor;
            item.FontColorSelected = ItemColor;
            AddMenuItem(item);
        }
    }
}
