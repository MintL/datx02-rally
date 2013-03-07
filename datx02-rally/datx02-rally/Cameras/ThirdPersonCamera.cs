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
        // Input inversion
        protected Vector2 inputLookInversion = Vector2.One;

        public float Zoom { get; set; }
        public float RotationSpeed { get; set; }
        public float RotationMagnet { get; set; }
        public float TranslationMagnet { get; set; }

        private Vector3 lookUpOffset = Vector3.Zero;
        protected Vector3 translationOffset;

        private Matrix cameraRotation = Matrix.Identity, 
            cameraTranslation = Matrix.Identity,
            localOffset = Matrix.Identity;


        public ITargetNode TargetNode { get; set; }

        public override Vector3 Position { get; protected set; }
        public override Matrix View { get; protected set; }


        public ThirdPersonCamera(ITargetNode targetNode, InputComponent input) 
            : base()
        {
            this.TargetNode = targetNode;
            this.input = input;
            
            // Constants

            Zoom = 300;
            RotationSpeed = .05f;
            RotationMagnet = .075f;
            TranslationMagnet = .98f;

            lookUpOffset = Zoom * new Vector3(0, .25f, 0);
            translationOffset = Zoom * new Vector3(0, .2f, 1);

            cameraRotation = TargetNode.RotationMatrix;
            cameraTranslation = TargetNode.TranslationMatrix;

            Position = Vector3.Transform(translationOffset, cameraRotation * cameraTranslation);
            View = Matrix.CreateLookAt(Position,
                Vector3.Transform(lookUpOffset, cameraRotation * cameraTranslation), Vector3.Up);
        }

        public override void Update(GameTime gameTime)
        {
            // Get X relative to lookAt.
            Vector3 localX = Vector3.Cross(translationOffset, Vector3.Up);

            Vector2 movement = RotationSpeed * new Vector2(input.GetState(Input.CameraX), input.GetState(Input.CameraY));

            // Offset with user controlled camera
            localOffset *= Matrix.CreateFromAxisAngle(localX, movement.Y) * Matrix.CreateRotationY(movement.X);
            localOffset = Matrix.Lerp(localOffset, Matrix.Identity, .1f);

            // Follow targetnode
            cameraRotation = Matrix.Lerp(cameraRotation, TargetNode.RotationMatrix, RotationMagnet);
            cameraTranslation = Matrix.Lerp(cameraTranslation, TargetNode.TranslationMatrix, TranslationMagnet);
            
            Position = Vector3.Transform(translationOffset, localOffset * cameraRotation * cameraTranslation);
            View = Matrix.CreateLookAt(Position,
                Vector3.Transform(lookUpOffset, cameraRotation * cameraTranslation), Vector3.Up);

            base.Update(gameTime);
        }

    }
}
