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

        private Vector3 lookUpOffset;

        private Matrix cameraRotation = Matrix.Identity, 
            cameraTranslation = Matrix.Identity,
            extraOffset = Matrix.Identity;

        public override Matrix View { get; protected set; }

        public ITargetNode TargetNode { get; set; }

        public ThirdPersonCamera(ITargetNode targetNode, Vector3 lookUpOffset, InputComponent input)
        {
            this.TargetNode = targetNode;
            Zoom = 300;
            RotationSpeed = .05f;
            this.lookUpOffset = lookUpOffset;
            this.input = input;
        }

        public override void Update(GameTime gameTime)
        {
            // Get X relative to lookAt.
            Vector3 localX = Vector3.Cross(offset, Vector3.Up);

            Vector2 movement = RotationSpeed * new Vector2(input.GetState(Input.CameraX), input.GetState(Input.CameraY));

            extraOffset *= Matrix.CreateFromAxisAngle(localX, movement.Y) * Matrix.CreateRotationY(movement.X);
            extraOffset = Matrix.Lerp(extraOffset, Matrix.Identity, .1f);

            cameraRotation = Matrix.Lerp(cameraRotation, TargetNode.RotationMatrix, .075f);
            cameraTranslation = Matrix.Lerp(cameraTranslation, TargetNode.TranslationMatrix, .98f);
            //cameraTranslation = TargetNode.TranslationMatrix;
            View = Matrix.CreateLookAt(Vector3.Transform(Zoom * offset, extraOffset * cameraRotation * cameraTranslation),
                Vector3.Transform(lookUpOffset, cameraRotation * cameraTranslation), Vector3.Up);
        }

    }
}
