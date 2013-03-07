using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.Menus
{
    public class OverlayView : MenuView
    {
        public OverlayView(Game game) : base(game, GameState.None)
        {

        }

        public override GameState UpdateState(Microsoft.Xna.Framework.GameTime gameTime)
        {
            
            throw new NotImplementedException();
        }

        public override void Draw(GameTime gameTime)
        {
            
            base.Draw(gameTime);
        }
    }
}
