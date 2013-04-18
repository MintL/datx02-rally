using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.Cameras
{
    class CarChooserCamera : Camera
    {
        private float speed;

        public override Vector3 Position { get; protected set; }

        public override Matrix View { get; protected set; }

        public CarChooserCamera(float zoom, float speed)
        {
            this.speed = speed;
            Position = new Vector3(0,
                zoom * (float)Math.Sin(MathHelper.PiOver2 / 3), 
                zoom);
        }

        public override void Update(GameTime gameTime)
        {
            Position = Vector3.Transform(Position, 
                Matrix.CreateRotationY(
                (float)gameTime.ElapsedGameTime.TotalSeconds * speed));
            View = Matrix.CreateLookAt(Position, Vector3.Zero, Vector3.Up);
            base.Update(gameTime);
        }

    }
}
