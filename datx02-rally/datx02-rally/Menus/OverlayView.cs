using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.Menus
{
    public abstract class OverlayView : GameStateView
    {
        public OverlayView(Game game) : base(game)
        {

        }
        public override GameState UpdateState(Microsoft.Xna.Framework.GameTime gameTime)
        {
            
            throw new NotImplementedException();
        }
    }
}
