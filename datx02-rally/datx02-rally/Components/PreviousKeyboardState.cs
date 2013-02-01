using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace datx02_rally
{
    class PreviousKeyboardState : GameComponent
    {
        public KeyboardState State { get; private set; }

        public PreviousKeyboardState(Game game) : base(game)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            State = Keyboard.GetState();
        }
    }
}
