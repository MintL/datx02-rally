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

        public Vector3 Normal;

        private Matrix normalMatrix = Matrix.Identity;

        private float wheelRadius;
        private float wheelRotationX;

        // Distance between wheelaxis.
        private float L = 40.197f;

        #endregion

        #region Constant properties

        public float MaxSpeed { get; protected set; }
        public float Acceleration { get; protected set; }
        public float MaxWheelTurn { get; protected set; }
        public float TurnSpeed { get; protected set; }

        #endregion

        #region Car Controlling Properties

        /// <summary>
        /// Set only for repositioning, not driving
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Set only for repositioning, not driving
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// This is set from outside to make the car go forward or backward.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// This is set from outside to make the car turn.
        /// </summary>
        public float WheelRotationY { get; set; }

        #endregion

        #region ITargetNode used by Camera

        public Matrix TranslationMatrix { get { return Matrix.CreateTranslation(Position); } }
        public Matrix RotationMatrix { get { return Matrix.CreateRotationY(Rotation) * normalMatrix; } }

        #endregion

        #region IMovingObject used by TriggerManager

        // Forward direction calculated to know where the car is heading. Not for set outside.
        public Vector3 Heading { get; set; }

        #endregion

        #region Initialization

        // Constructor
        private Car(Game game, Model model, float wheelRadius) : base(game)
        {
            this.model = model;

            this.Position = Vector3.Zero;

            // TODO: Move
            // Set Constants
            Acceleration = .15f;
            MaxSpeed = 50;
            MaxWheelTurn = MathHelper.PiOver4 / 0.7f;
            TurnSpeed = .005f;

            Normal = Vector3.Up;

            this.wheelRadius = wheelRadius;
        }

        #endregion

        #region Static factory

        private static Model carModel;
        private static float carWheelRadius = 13.4631138f;

        private static Effect 
            mainEffect,
            wheelEffect,
            glassEffect,
            otherEffect;

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
                foreach (var mesh in carModel.Meshes)
                {
                    foreach (var part in mesh.MeshParts)
                    {
                        var basicEffect = part.Effect as BasicEffect;

                        part.Effect = meshEffect[mesh.Name].Clone();

                        if (part.Effect.Parameters["DiffuseMap"] != null)
                            part.Effect.Parameters["DiffuseMap"].SetValue(basicEffect.Texture);

                        if (part.Effect.Parameters["MaterialDiffuse"] != null)
                            part.Effect.Parameters["MaterialDiffuse"].SetValue(basicEffect.DiffuseColor);

                        if (part.Effect.Parameters["MaterialAmbient"] != null)
                            part.Effect.Parameters["MaterialAmbient"].SetValue(basicEffect.DiffuseColor);

                        if (part.Effect.Parameters["MaterialSpecular"] != null)
                            part.Effect.Parameters["MaterialSpecular"].SetValue(basicEffect.DiffuseColor);
                    }

                    if (mesh.Name.StartsWith("wheel"))
                    {
                        if (mesh.Name.EndsWith("001") || mesh.Name.EndsWith("002"))
                            mesh.Tag = 2;
                        else
                            mesh.Tag = 1;
                    }
                    else
                        mesh.Tag = 0;
                }
            }

            var car = new Car(game, carModel, carWheelRadius);
            car.Initialize();
            return car;
        }

        private static void InitializeEffects(ContentManager content)
        {
            mainEffect = content.Load<Effect>(@"Effects/Car/CarBodyEffect");
            wheelEffect = content.Load<Effect>(@"Effects/Car/OtherCarEffect");
            glassEffect = content.Load<Effect>(@"Effects/Car/GlassEffect");
            otherEffect = content.Load<Effect>(@"Effects/Car/OtherCarEffect");

            meshEffect = new Dictionary<string, Effect>()
            {
                { "main", mainEffect },
                { "wheel001", wheelEffect },
                { "wheel002", wheelEffect },
                { "wheel003", wheelEffect },
                { "wheel004", wheelEffect },
                { "glasswindows", glassEffect },
                { "frontlightglass", glassEffect },
                { "lights", otherEffect },
                { "interior", mainEffect },
                { "realmirrors", otherEffect },
                { "registerplate", otherEffect },
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

        public void Update()
        {
            var previousPos = Position;
            var previousRotation = Rotation;

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
            wheelRotationX += (Speed < 0 ? 1 : -1) * (Position - oldPos).Length() / wheelRadius;

            normalMatrix = Matrix.Lerp(normalMatrix, Vector3.Up.GetRotationMatrix(Normal), .2f);

        }

        public Matrix View { get; set; }
        public Matrix Projection { get; set; }

        public DirectionalLight DirectionalLight { get; set; }

        public override void Draw(GameTime gameTime)
        {
            var meshes = model.Meshes.OrderBy(m => m.Name.ToLower().Contains("glass") ? 1 : -1).ToList();

            foreach (var mesh in meshes)
            {
                var world = Matrix.Identity;

                // Wheel transformation
                if ((int)mesh.Tag > 0)
                {
                    world *= Matrix.CreateRotationX(wheelRotationX);
                    if ((int)mesh.Tag > 1)
                        world *= Matrix.CreateRotationY(WheelRotationY);
                }

                // Local modelspace
                world *= mesh.ParentBone.Transform;

                // World
                world *= RotationMatrix * TranslationMatrix;

                foreach (Effect effect in mesh.Effects)
                {
                    EffectParameterCollection param = effect.Parameters;

                    param["World"].SetValue(world);
                    param["View"].SetValue(View);
                    param["Projection"].SetValue(Projection);

                    // TODO: CARMOVE
                    if (mesh.Name.Equals("main"))
                    {
                        param["MaterialReflection"].SetValue(.9f);
                        param["MaterialShininess"].SetValue(10);

                        param["NormalMatrix"].SetValue(Matrix.Invert(Matrix.Transpose(world)));

                        param["EyePosition"].SetValue(Game.GetService<CameraComponent>().Position);

                        if (DirectionalLight != null)
                        {
                            param["DirectionalLightDirection"].SetValue(DirectionalLight.Direction);
                            param["DirectionalLightDiffuse"].SetValue(DirectionalLight.Diffuse);
                            param["DirectionalLightAmbient"].SetValue(DirectionalLight.Ambient);
                        }
                    }
                }

                mesh.Draw();
            }

        }
    }
}