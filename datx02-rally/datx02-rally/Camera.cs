using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Test
{
    abstract class Camera
    {
        public abstract Matrix View { get; }

        public abstract void Update(GamePadState gamePadState) ;

        public abstract void Update(KeyboardState keyboardState, MouseState mouseState, Vector2 screenCenter);
    }
}
