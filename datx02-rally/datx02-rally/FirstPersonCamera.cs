using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Test
{
    class FirstPersonCamera : Camera
    {
        protected Vector3 position, 
            lookat = Vector3.Forward;

        // Input inversion
        protected Vector2 inputLookInversion = Vector2.One, 
            inputWalkInversion = Vector2.One;

        public override Matrix View
        {
            get { return Matrix.CreateLookAt(position, position + lookat, Vector3.Up); }
        }

        public float MovementSpeed { set; get; }
        public float RotationSpeed { set; get; }

        public bool IsMoving { set; get; }

        /// <summary>
        /// Constructs a new first person camera.
        /// </summary>
        /// <param name="position">Recommended to specify a position a persons length above some ground.</param>
        public FirstPersonCamera(Vector3 position)
        {
            this.position = position;

            MovementSpeed = .5f;
            RotationSpeed = .0035f;
        }

        public override void Update(GamePadState gamePadState)
        {
            lookat = Vector3.Transform(lookat, Matrix.CreateFromAxisAngle(Vector3.Cross(lookat, Vector3.Up), gamePadState.ThumbSticks.Right.Y) *
                Matrix.CreateFromAxisAngle(Vector3.Up, gamePadState.ThumbSticks.Right.X));

            //position += MovementSpeed * Vector3.Transform(Vector3.Transform(new Vector3(gps.ThumbSticks.Left, 0),
            //    Matrix.CreateRotationX(-MathHelper.PiOver2)), rotation);
        }

        public override void Update(KeyboardState keyboardState, MouseState mouseState, Vector2 screenCenter)
        {
            Vector2 mouseDelta = inputLookInversion * RotationSpeed * 
                (screenCenter - new Vector2(mouseState.X, mouseState.Y));

            lookat = Vector3.Transform(lookat, Matrix.CreateFromAxisAngle(Vector3.Cross(lookat, Vector3.Up), mouseDelta.Y) * 
                Matrix.CreateFromAxisAngle(Vector3.Up, mouseDelta.X));

            Vector2 walkdirection = Vector2.Zero;
            if (keyboardState.IsKeyDown(Keys.W))
                walkdirection.Y += inputWalkInversion.Y;
            if (keyboardState.IsKeyDown(Keys.A))
                walkdirection.X -= inputWalkInversion.X;
            if (keyboardState.IsKeyDown(Keys.S))
                walkdirection.Y -= inputWalkInversion.Y;
            if (keyboardState.IsKeyDown(Keys.D))
                walkdirection.X += inputWalkInversion.X;

            //if (IsMoving = (walkdirection.Y != 0 || walkdirection.X != 0))
            //{
            //    walkdirection.Normalize();
            //    position += MovementSpeed * (walkdirection.Y * VectorExtension.GroundPlane * lookat +
            //        walkdirection.X * Vector3.Cross(lookat, Vector3.Up));
            //}
            
            Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);
        }

    }
}
