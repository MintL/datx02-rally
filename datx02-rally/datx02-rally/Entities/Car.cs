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
        private List<ModelMesh> meshes;

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
        public float Deacceleration { get; protected set; }

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
            if (model != null)
                this.meshes = model.Meshes.OrderBy(m => m.Name.ToLower().Contains("glass") ? 1 : -1).ToList();

            this.Position = Vector3.Zero;

            // TODO: Move
            // Set Constants
            Acceleration = .15f;
            Deacceleration = .35f;
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

        /// <summary>
        /// Factory for creating a simulated car, i.e. a car that is not drawn.
        /// </summary>
        /// <param name="game"></param>
        /// <returns>A brand new porsche.</returns>
        public static Car CreateSimulatedCar(Game game)
        {
            var car = new Car(game, null, carWheelRadius);
            car.Visible = false;
            return car;
        }

        private static void InitializeEffects(ContentManager content)
        {
            mainEffect = content.Load<Effect>(@"Effects/Car/CarBodyEffect");
            wheelEffect = content.Load<Effect>(@"Effects/Car/OtherCarEffect");
            glassEffect = content.Load<Effect>(@"Effects/Car/GlassEffect");
            otherEffect = content.Load<Effect>(@"Effects/Car/OtherCarEffect");

            mainEffect.Parameters["MaterialReflection"].SetValue(.7f);
            mainEffect.Parameters["MaterialShininess"].SetValue(10);


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

            // Zoom the thirdperson camera with the car speed
            var camera = Game.GetService<CameraComponent>().CurrentCamera;
            if (camera is ThirdPersonCamera)
            {
                (camera as ThirdPersonCamera).Zoom = 330 + Speed * 1.5f;
            }
        }

        public Matrix View { get; set; }
        public Matrix Projection { get; set; }

        public DirectionalLight DirectionalLight { get; set; }

        public override void Draw(GameTime gameTime)
        {
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
                        if (param["NormalMatrix"] != null)
                            param["NormalMatrix"].SetValue(Matrix.Invert(Matrix.Transpose(world)));

                        if (param["EyePosition"] != null) 
                            param["EyePosition"].SetValue(Game.GetService<CameraComponent>().Position);

                        if (DirectionalLight != null)
                        {
                            if (param["DirectionalLightDirection"] != null)
                                param["DirectionalLightDirection"].SetValue(DirectionalLight.Direction);
                            if (param["DirectionalLightDiffuse"] != null)
                                param["DirectionalLightDiffuse"].SetValue(DirectionalLight.Diffuse);
                            if (param["DirectionalLightAmbient"] != null)
                                param["DirectionalLightAmbient"].SetValue(DirectionalLight.Ambient);
                        }
                    }
                }

                mesh.Draw();
            }

        }

        public TextureCube EnvironmentMap
        {
            set
            {
                foreach (ModelMesh mesh in model.Meshes)
                    foreach (ModelMeshPart part in mesh.MeshParts)
                        if (part.Effect.Parameters["EnvironmentMap"] != null)
                            part.Effect.Parameters["EnvironmentMap"].SetValue(value);
            }
        }

        public Vector3 MaterialDiffuse
        {
            set
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    if (mesh.Name == "main")
                    {
                        foreach (ModelMeshPart part in mesh.MeshParts)
                        {
                            if (part.Effect.Parameters["MaterialDiffuse"] != null)
                                part.Effect.Parameters["MaterialDiffuse"].SetValue(value);
                        }
                    }
                }
            }
        }

        // To be used by PrelightingRenderer.
        public void PreparePrelighting(RenderTarget2D lightTarget, Matrix lightProjection, int width, int height)
        {
            //foreach (ModelMesh mesh in model.Meshes)
            {
                var mesh = model.Meshes["main"];
                //if (mesh.Name.Equals("main"))
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect.Parameters["LightMap"].SetValue(lightTarget);
                        part.Effect.Parameters["PrelightProjection"].SetValue(lightProjection);
                        part.Effect.Parameters["viewportWidth"].SetValue(width);
                        part.Effect.Parameters["viewportHeight"].SetValue(height);
                    }
                }
            }
        }

        // Draw the main body of the car with a special effect to draw depth and normal buffer.
        // To be used by PrelightingRenderer.
        public void DrawDepthNormal(Effect depthNormalEffect, Matrix lightProjection)
        {
            Dictionary<ModelMeshPart, Effect> oldEffects = new Dictionary<ModelMeshPart,Effect>();

            Matrix oldProjection = Projection;
            Projection = lightProjection;

            //foreach (ModelMesh mesh in model.Meshes)
            {
                var mesh = model.Meshes["main"];
                //if (mesh.Name.Equals("main"))
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        oldEffects.Add(part, part.Effect);
                        part.Effect = depthNormalEffect;
                    }
                }
            }
            var tmpMeshes = meshes;
            this.meshes = new List<ModelMesh>{ carModel.Meshes["main"] };
            Draw(null);
            this.meshes = tmpMeshes;

            // Reset the effect and projection
            Projection = oldProjection;            
            var mainMesh = model.Meshes["main"];
            foreach (ModelMeshPart part in mainMesh.MeshParts)
            {
                part.Effect = oldEffects[part];
            }   
            
        }
    }
}