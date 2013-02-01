using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace datx02_rally
{
    abstract class Camera : IUpdateable
    {
        public abstract Matrix View { get; }

        public abstract void Update(GameTime gameTime);

        protected int K(Keys k)
        {
            return Keyboard.GetState().IsKeyDown(k) ? 1 : 0;
        }

        public bool Enabled
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<EventArgs> EnabledChanged;

        public int UpdateOrder
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<EventArgs> UpdateOrderChanged;
    }
}
