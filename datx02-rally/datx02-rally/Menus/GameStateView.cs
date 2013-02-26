using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.Menus
{
    public abstract class GameStateView : DrawableGameComponent
    {
        public Game1 game;

        public GameStateView(Game game) : base(game)
        {
            this.game = game as Game1;
        }

        /// <summary>
        /// Updates the state.
        /// </summary>
        /// <returns>Next game state</returns>
        public abstract GameState UpdateState(GameTime gameTime);
    }
}
