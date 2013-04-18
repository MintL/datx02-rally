using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using datx02_rally.Entities;
using Microsoft.Xna.Framework.Content;

namespace datx02_rally
{
    public class Car : DrawableGameComponent, ITargetNode, IMovingObject
    {

        #region Field

        private Model model;

        private Vector3 normal;

        private Matrix normalMatrix = Matrix.Identity;

        #endregion

        #region Car Controlling Properties

        /// <summary>
        /// Set only for repositioning, not driving
        /// </summary>
        public float Rotation { get; set; }


        #endregion


        
        /// <summary>
        /// Set only for repositioning, not driving
        /// </summary>
        public Vector3 Position { get; set; }

        private float wheelRadius;
        public float WheelRotationX { get; private set; }

        public Matrix TranslationMatrix { get { return Matrix.CreateTranslation(Position); } }
        public Matrix RotationMatrix { get { return Matrix.CreateRotationY(Rotation) * normalMatrix; } }

        /// <summary>
        /// This is set from outside to make the car go forward or backward.
        /// </summary>
        public float Speed { get; set; }
        /// <summary>
        /// This is set from outside to make the car turn.
        /// </summary>
        public float WheelRotationY { get; set; }

        // Constants
        public float MaxSpeed { get; protected set; }
        public float Acceleration { get; protected set; }
        public float MaxWheelTurn { get; protected set; }
        public float TurnSpeed { get; protected set; }
        
        // Distance between wheelaxis.
        private float L = 40.197f;

        // Forward direction calculated to know where the car is heading. Not for set outside.
        public Vector3 Heading { get; set; }

        // Constructor
        private Car(Game game, Model model, float wheelRadius) : base(game)
        {
            this.Model = model;

            this.Position = Vector3.Zero;

            // Constants
            Acceleration = .15f;
            MaxSpeed = 50;
            MaxWheelTurn = MathHelper.PiOver4 / 0.7f;
            TurnSpeed = .005f;

            Normal = Vector3.Up;

            this.wheelRadius = wheelRadius;
        }

        #region Static

        private static Model carModel;
        private static float carWheelRadius = 13.4631138f;

        private static Effect 
            mainEffect,
            wheelEffect,
            glassEffect,
            otherEfect;

        private static Dictionary<string, Effect> meshEffect;

        /// <summary>
        /// Our porsche factory.
        /// </summary>
        /// <param name="game"></param>
        /// <returns>A brand new porsche.</returns>
        public static Car CreateCar(Game game)
        {
            if (carModel == null)
            {
                carModel = game.Content.Load<Model>(@"Models/Cars/porsche");
                InitializeEffects(game.Content);

                // Keep some parameters from imported modeleffect
                foreach (ModelMesh mesh in carModel.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        var basicEffect = part.Effect as BasicEffect;

                        part.Effect = meshEffect[mesh.Name];

                        if (mesh.Name.Equals("main"))
                        {
                            part.Effect.Parameters["DiffuseMap"].SetValue(basicEffect.Texture);
                            part.Effect.Parameters["MaterialDiffuse"].SetValue(basicEffect.DiffuseColor);
                            part.Effect.Parameters["MaterialAmbient"].SetValue(basicEffect.DiffuseColor);
                            part.Effect.Parameters["MaterialSpecular"].SetValue(basicEffect.DiffuseColor);
                        }

                    }
                }

            }
            return new Car(game, carModel, carWheelRadius);
        }

        private static void InitializeEffects(ContentManager content)
        {
            mainEffect = content.Load<Effect>(@"Effects/Car/CarBodyEffect");
            wheelEffect = content.Load<Effect>(@"Effects/Car/OtherCarEffect");
            glassEffect = content.Load<Effect>(@"Effects/Car/OtherCarEffect");
            otherEfect = content.Load<Effect>(@"Effects/Car/OtherCarEffect");

            meshEffect = new Dictionary<string, Effect>()
            {
                { "main", mainEffect },
                { "wheel001", wheelEffect },
                { "wheel002", wheelEffect },
                { "wheel003", wheelEffect },
                { "wheel004", wheelEffect },
                { "glasswindows", glassEffect },
                { "frontlightglass", glassEffect },
                { "lights", otherEfect },
                { "interior", otherEfect },
                { "realmirrors", otherEfect },
                { "registerplate", otherEfect },
            };
        }


        #endregion

        /*
        protected BoundingBox GetBoundingBox(Model model)
        {
            // Initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            // For each mesh of the model
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Vertex buffer parameters
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    // Get vertex data as float
                    float[] vertexData = new float[vertexBufferSize / sizeof(float)];
                    meshPart.VertexBuffer.GetData<float>(vertexData);

                    // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        Vector3 transformedPosition = new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]);

                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);
                    }
                }
            }

            // Create and return bounding box
            return new BoundingBox(min, max);
        }
        */

        public Vector3 previousPos;
        public float previousRotation;

        public void Update()
        {
            previousPos = Position;
            previousRotation = Rotation;

            Heading = Vector3.Transform(Vector3.Forward,
                Matrix.CreateRotationY(Rotation));
            Vector3 axisOffset = L * Heading;

            Vector3 front = Position + axisOffset;
            Vector3 back = Position - axisOffset;

            front += Speed * Vector3.Transform(Vector3.Forward,
                Matrix.CreateRotationY(Rotation + WheelRotationY));
            back += Speed * Heading;

            Vector3 oldPos = Position;
            Position = (front + back) / 2;
            Rotation = (float)Math.Atan2(back.X - front.X, back.Z - front.Z);
            WheelRotationX += (Speed < 0 ? 1 : -1) * (Position - oldPos).Length() / wheelRadius;

            normalMatrix = Matrix.Lerp(normalMatrix, Vector3.Up.GetRotationMatrix(Normal), .2f);

        }

        public Matrix View { get; set; }
        public Matrix Projection { get; set; }

        public DirectionalLight DirectionalLight { get; set; }

        public override void Draw(GameTime gameTime)
        {

            foreach (var mesh in Model.Meshes)
            {
                var world = Matrix.Identity;

                // Wheel transformation
                if ((int)mesh.Tag > 0)
                {
                    world *= Matrix.CreateRotationX(WheelRotationX);
                    if ((int)mesh.Tag > 1)
                        world *= Matrix.CreateRotationY(WheelRotationY);
                }

                // Local modelspace
                world *= mesh.ParentBone.Transform;

                // World
                world *= RotationMatrix * TranslationMatrix;

                foreach (Effect effect in mesh.Effects) // 5 effects for main, 1 for each wheel
                {
                    EffectParameterCollection param = effect.Parameters;

                    if (mesh.Name.Equals("main"))
                    {
                        param["MaterialReflection"].SetValue(.9f);
                        param["MaterialShininess"].SetValue(10);
                    }

                    param["World"].SetValue(world);
                    param["View"].SetValue(View);
                    param["Projection"].SetValue(Projection);

                    if (mesh.Name.Equals("main"))
                    {

                        param["NormalMatrix"].SetValue(Matrix.Invert(Matrix.Transpose(world)));

                        param["EyePosition"].SetValue(Game.GetService<CameraComponent>().Position);

                        param["DirectionalLightDirection"].SetValue(DirectionalLight.Direction);
                        param["DirectionalLightDiffuse"].SetValue(DirectionalLight.Diffuse);
                        param["DirectionalLightAmbient"].SetValue(DirectionalLight.Ambient);
                    }
                }

                mesh.Draw();
            }

        }
    }
}