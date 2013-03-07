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
        public abstract Vector3 Position { get; protected set; }
        public abstract Matrix View { get; protected set; }

        protected bool ShakeEnabled { get; private set; }
        protected Matrix ShakeTransfomation { get; private set; }
        protected float shake;

        public Camera()
        {
            ShakeTransfomation = Matrix.Identity;
            ShakeEnabled = true;
        }

        public virtual void Update(GameTime gameTime)
        {
            shake *= .9f;

            if (shake > 0.01)
                ShakeTransfomation *= Matrix.CreateTranslation(
                    20 * shake * (float)(UniversalRandom.GetInstance().NextDouble() - .5),
                    20 * shake * (float)(UniversalRandom.GetInstance().NextDouble() - .5), 0);
            else
                ShakeTransfomation = Matrix.Identity;

            View *= ShakeTransfomation;
        }

        protected InputComponent input;

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

        internal void Shake()
        {
            if (ShakeEnabled)
                shake = 1;
        }
    }
}
