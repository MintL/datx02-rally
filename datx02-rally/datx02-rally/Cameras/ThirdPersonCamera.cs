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


namespace datx02_rally
{
    class ThirdPersonCamera : Camera
    {
        protected Vector3 offset = new Vector3(0, .3f, 1);

        // Input inversion
        protected Vector2 inputLookInversion = Vector2.One;

        public float RotationSpeed { get; set; }

        public float Zoom { get; set; }

        private Matrix cameraRotation = Matrix.Identity, 
            cameraTranslation = Matrix.Identity,
            extraOffset = Matrix.Identity;

        public override Matrix View
        {
            get
            {
                cameraRotation = Matrix.Lerp(cameraRotation, TargetNode.RotationMatrix, .075f);
                //cameraTranslation = Matrix.Lerp(cameraTranslation, TargetNode.TranslationMatrix, 1);
                cameraTranslation = TargetNode.TranslationMatrix;
                return Matrix.CreateLookAt(Vector3.Transform(Zoom * offset, extraOffset * cameraRotation * cameraTranslation),
                    Vector3.Transform(50 * Vector3.Up, cameraRotation * cameraTranslation), Vector3.Up);
            }
        }

        public ITargetNode TargetNode { get; set; }

        public ThirdPersonCamera(ITargetNode targetNode)
        {
            this.TargetNode = targetNode;
            Zoom = 250;
            RotationSpeed = .05f;
        }

        public override void Update(GameTime gameTime)
        {
            // Get X relative to lookAt.
            Vector3 localX = Vector3.Cross(offset, Vector3.Up);

            Vector2 input = RotationSpeed * new Vector2(K(Keys.Right) - K(Keys.Left), K(Keys.Up) - K(Keys.Down));

            extraOffset *= Matrix.CreateFromAxisAngle(localX, input.Y) * Matrix.CreateRotationY(input.X);
            extraOffset = Matrix.Lerp(extraOffset, Matrix.Identity, .1f);
        }

    }
}
