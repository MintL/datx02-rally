using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace datx02_rally
{
    class DebugCamera : Camera
    {
        private float speed;
        private Vector3 lookAt = Vector3.Forward;

        private ButtonState oldButtonState = ButtonState.Released;
        private int oldX, oldY;

        public float MinSpeed { get; set; }
        public float MaxSpeed { get; set; }

        public override Matrix View { get; protected set; }

        public override Vector3 Position { get; protected set; }

        public DebugCamera(Vector3 position, InputComponent input) : this(input)
        {
            this.Position = position;
        }

        public DebugCamera(InputComponent input)
        {
            speed = MinSpeed = 3f;
            MaxSpeed = 50f;
            this.input = input;
        }

        public override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.R))
                Position = Vector3.Zero;

            // Get X relative to lookAt.
            Vector3 localX = Vector3.Cross(lookAt, Vector3.Up);

            // Lookdirection
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                if (oldButtonState == ButtonState.Pressed)
                {
                    lookAt = Vector3.Transform(lookAt, Matrix.CreateFromAxisAngle(localX, -.0035f * (Mouse.GetState().Y - oldY)) *
                        Matrix.CreateRotationY(-.0035f * (Mouse.GetState().X - oldX)));
                }
                oldX = Mouse.GetState().X;
                oldY = Mouse.GetState().Y;
            }
            oldButtonState = Mouse.GetState().LeftButton;

            // Movement
            Vector3 delta = speed * (lookAt * (K(Keys.W) - K(Keys.S)) +
                localX * (K(Keys.D) - K(Keys.A)) + Vector3.Up * (K(Keys.PageUp) - K(Keys.PageDown)));

            if (delta.LengthSquared() > 0)
                speed = Math.Min(MaxSpeed, speed * 1.02f);
            else
                speed = MinSpeed;

            Position += delta;

            View = Matrix.CreateLookAt(Position, Position + lookAt, Vector3.Up);
        }
    }
}
