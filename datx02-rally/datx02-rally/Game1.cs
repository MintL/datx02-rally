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
using BulletSharp;
using Test;

namespace datx02_rally
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Model model;
        Vector3 modelPosition = Vector3.Zero;
        float modelRotation = 0.0f;
        float lightRotation;

        ThirdPersonCamera camera;
        Vector2 screenCenter;

        Matrix projection;

        Vector3 lightPosition;

        Effect effect;

        Car car;

        PlaneModel plane;
        PlaneModel tree;
        List<Matrix> treeTransforms = new List<Matrix>();

        Random random = new Random();

        #region SkySphere

        Model skySphereModel;
        Effect skySphereEffect;

        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 800;// 1920;
            graphics.PreferredBackBufferHeight = 600; // 1080;
            graphics.ApplyChanges();

            //graphics.ToggleFullScreen();

            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            var debugCamera = new DebugCameraComponent(this);
            Components.Add(debugCamera);
            Services.AddService(typeof(DebugCameraComponent), debugCamera);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            camera = new ThirdPersonCamera();
            screenCenter = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) / 2f;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                GraphicsDevice.Viewport.AspectRatio, .1f, 10000f);
            
            effect = Content.Load<Effect>(@"Effects/BasicShading");
            model = Content.Load<Model>(@"Models/porsche");
            
            // Light specific parameters
            lightPosition = new Vector3(-200.0f, 200.0f, 0.0f);
            effect.Parameters["LightAmbient"].SetValue(Color.Black.ToVector3() * 0.2f);
            effect.Parameters["LightDiffuse"].SetValue(Color.White.ToVector3() * 1.0f);
            effect.CurrentTechnique = effect.Techniques["BasicShading"];

            // Initialize the material settings
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    BasicEffect basicEffect = (BasicEffect)part.Effect;
                    part.Effect = effect.Clone();
                    part.Effect.Parameters["MaterialAmbient"].SetValue(basicEffect.AmbientLightColor);
                    part.Effect.Parameters["MaterialDiffuse"].SetValue(basicEffect.DiffuseColor);
                    part.Effect.Parameters["MaterialSpecular"].SetValue(basicEffect.SpecularColor);
                }
            }

            car = new Car(Content.Load<Model>(@"Models/porsche"), 10.4725f);
            plane = new PlaneModel(new Vector2(-10000), new Vector2(10000), 1, GraphicsDevice, null, projection, Matrix.Identity);

            Vector2 treePlaneSizeStart = new Vector2(-50, -250),
                treePlaneSizeEnd = new Vector2(50, 0);

            tree = new PlaneModel(treePlaneSizeStart, treePlaneSizeEnd, 1, GraphicsDevice,
                    Content.Load<Texture2D>("spruce"), projection, Matrix.Identity);

            for (int i = 99; i >= 0; i--)
            {
                int side = 2 * random.Next(2) - 1;
                treeTransforms.Add(Matrix.CreateRotationX(MathHelper.PiOver2) *
                    Matrix.CreateRotationY(-side * (MathHelper.PiOver4 + (float)(random.NextDouble() / 4))) *
                    Matrix.CreateTranslation(new Vector3(side * (140 + random.Next(30)), 0, i * -100)));
            }

            #region SkySphere

            skySphereModel = Content.Load<Model>(@"Models/skysphere");
            skySphereEffect = Content.Load<Effect>(@"Effects/SkySphere");
            
            TextureCube cubeMap = new TextureCube(GraphicsDevice, 2048, false, SurfaceFormat.Color);

            string[] cubemapfaces = { @"SkyBoxes/PurpleSky/skybox_right1", 
@"SkyBoxes/PurpleSky/skybox_left2", 
@"SkyBoxes/PurpleSky/skybox_top3", 
@"SkyBoxes/PurpleSky/skybox_bottom4", 
@"SkyBoxes/PurpleSky/skybox_front5", 
@"SkyBoxes/PurpleSky/skybox_back6_2" 
                                    };

            for (int i = 0; i < cubemapfaces.Length; i++)
                LoadCubemapFace(cubeMap, cubemapfaces[i], (CubeMapFace)i);

            skySphereEffect.Parameters["SkyboxTexture"].SetValue(cubeMap);

            foreach (var mesh in skySphereModel.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = skySphereEffect;
                    
                }
            }

            #endregion
        }

        /// <summary>
        /// Loads a texture from Content and asign it to the cubemaps face.
        /// </summary>
        /// <param name="cubeMap"></param>
        /// <param name="filepath"></param>
        /// <param name="face"></param>
        private void LoadCubemapFace(TextureCube cubeMap, string filepath, CubeMapFace face)
        {
            Texture2D texture = Content.Load<Texture2D>(filepath);
            byte[] data = new byte[texture.Width * texture.Height * 4];
            texture.GetData<byte>(data);
            cubeMap.SetData<byte>(face, data);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();


            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                modelRotation += (float)gameTime.ElapsedGameTime.Milliseconds * MathHelper.ToRadians(0.05f);
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                modelRotation -= (float)gameTime.ElapsedGameTime.Milliseconds * MathHelper.ToRadians(0.05f);
            }

            lightRotation += (float)gameTime.ElapsedGameTime.Milliseconds * MathHelper.ToRadians(0.05f);
            lightPosition = Vector3.Transform(new Vector3(0.0f, 0.0f, 0.0f),
                Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, 300.0f)) * Matrix.CreateRotationY(lightRotation));
            Console.WriteLine(lightPosition);

            #region Car control

            //Accelerate
            car.Speed = Math.Min(car.Speed + car.Acceleration *
                ((Keyboard.GetState().IsKeyDown(Keys.W) ? 1 : 0) +
                GamePad.GetState(PlayerIndex.One).Triggers.Right -
                (Keyboard.GetState().IsKeyDown(Keys.S) ? 1 : 0) -
                GamePad.GetState(PlayerIndex.One).Triggers.Left), car.MaxSpeed);
            // Turn Wheel
            car.WheelRotationY += (Keyboard.GetState().IsKeyDown(Keys.A) ? car.TurnSpeed : 0) -
                (Keyboard.GetState().IsKeyDown(Keys.D) ? car.TurnSpeed : 0);
            car.WheelRotationY = MathHelper.Clamp(car.WheelRotationY, -car.MaxWheelTurn, car.MaxWheelTurn);
            if (Math.Abs(car.WheelRotationY) > MathHelper.Pi / 720)
                car.WheelRotationY *= .9f;
            else
                car.WheelRotationY = 0;

            //Apply changes to car
            car.Update();

            //Friktion if is not driving
            float friction = .97f; // 0.995f;
            if (!Keyboard.GetState().IsKeyDown(Keys.W) ||
                !GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.RightTrigger) && GamePad.GetState(PlayerIndex.One).IsConnected)
                car.Speed *= friction;

            #endregion

            //camera.Update(Keyboard.GetState(), Mouse.GetState(), screenCenter);

            camera.Position = car.Position;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Honeydew);

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            Matrix view = this.GetService<DebugCameraComponent>().View;

            // Model specific parameters
            //effect.Parameters["MaterialAmbient"].SetValue(Color.White.ToVector3() * 0.5f);
            //effect.Parameters["MaterialDiffuse"].SetValue(Color.DarkGreen.ToVector3() * 0.9f);
            //effect.Parameters["MaterialSpecular"].SetValue(Color.White.ToVector3() * 0.2f);

            //foreach (ModelMesh mesh in model.Meshes)
            //{
            //    foreach (Effect currentEffect in mesh.Effects)
            //    {
            //        currentEffect.Parameters["MaterialShininess"].SetValue(10.0f);

            //        Matrix worldMatrix = transforms[mesh.ParentBone.Index] *
            //                    Matrix.CreateRotationY(modelRotation) *
            //                    Matrix.CreateTranslation(modelPosition);

            //        Matrix normalMatrix = Matrix.Invert(Matrix.Transpose(view * worldMatrix));
                    
            //        currentEffect.Parameters["World"].SetValue(worldMatrix);
            //        currentEffect.Parameters["View"].SetValue(view);
            //        currentEffect.Parameters["Projection"].SetValue(projection);
            //        currentEffect.Parameters["NormalMatrix"].SetValue(normalMatrix);
            //        currentEffect.Parameters["LightPosition"].SetValue(lightPosition);
            //    }
            //    mesh.Draw();
            //}
            //foreach (ModelMesh mesh in model.Meshes)
            //{
            //    foreach (Effect currentEffect in mesh.Effects)
            //    {
            //        currentEffect.Parameters["MaterialShininess"].SetValue(10.0f);

            //        currentEffect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] *
            //                    Matrix.CreateRotationY(modelRotation) *
            //                    Matrix.CreateTranslation(lightPosition));
            //        currentEffect.Parameters["View"].SetValue(view);
            //        currentEffect.Parameters["Projection"].SetValue(projection);
            //        currentEffect.Parameters["LightPosition"].SetValue(lightPosition);
            //    }
            //    mesh.Draw();
            //}


            plane.Draw(view, Color.Gray);


            // Draw car
            foreach (var mesh in car.Model.Meshes) // 5 meshes
            {
                foreach (Effect effect in mesh.Effects) // 5 effects for main, 1 for each wheel
                {
                    effect.Parameters["MaterialShininess"].SetValue(10.0f);

                    Matrix world = Matrix.Identity;
                    // If this mesh is a wheel, apply rotation
                    if (mesh.Name.StartsWith("wheel"))
                    {
                        world *= Matrix.CreateRotationX(car.WheelRotationX);

                        if (mesh.Name.EndsWith("001") || mesh.Name.EndsWith("002"))
                            world *= Matrix.CreateRotationY(car.WheelRotationY);
                    }
                    // Local morldspace, due to bad .X-file/exporter
                    world *= car.Model.Bones[1 + car.Model.Meshes.IndexOf(mesh) * 2].Transform;
                    world *= Matrix.CreateRotationY(car.Rotation) *
                        Matrix.CreateTranslation(car.Position);

                    effect.Parameters["World"].SetValue(world);

                    Matrix normalMatrix = Matrix.Invert(Matrix.Transpose(view * world));
                    effect.Parameters["NormalMatrix"].SetValue(normalMatrix);

                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);

                    effect.Parameters["LightPosition"].SetValue(lightPosition);
                }
                mesh.Draw();
            }


            #region SkySphere

            skySphereEffect.Parameters["View"].SetValue(view);
            skySphereEffect.Parameters["Projection"].SetValue(projection);
            foreach (var mesh in skySphereModel.Meshes)
            {
                mesh.Draw();
            }

            #endregion


            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            foreach (var world in treeTransforms)
            {
                tree.World = world;
                tree.Draw(view);
            }

            base.Draw(gameTime);
        }
    }
}
