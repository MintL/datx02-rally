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
    class ThirdPersonCamera : FirstPersonCamera
    {
        public override Matrix View
        {
            get
            {
                return Matrix.CreateLookAt(Zoom * lookat + position, 
                    position + OrbitZOffset * Vector3.Up, Vector3.Up);
            }
        }

        public Vector3 Position { get { return position; } set { position = value; } }

        public float Zoom { get; set; }

        public float OrbitZOffset { get; set; }

        public ThirdPersonCamera() : this(Vector3.Zero)
        {
        }

        public ThirdPersonCamera(Vector3 position) : base(position)
        {
            Zoom = 500;
            OrbitZOffset = 20;
            MovementSpeed *= -.09f;
            inputLookInversion.Y *= -1;
        }

    }
}
