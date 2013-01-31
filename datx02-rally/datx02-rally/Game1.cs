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
        Model lightModel;
        Vector3 modelPosition = Vector3.Zero;
        float modelRotation = 0.0f;
        float lightRotation;
        float lightDistance = 700.0f;

        ThirdPersonCamera camera;
        Vector2 screenCenter;

        Matrix projection;

        List<PointLight> pointLights;

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
            pointLights = new List<PointLight>();
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
            lightModel = Content.Load<Model>(@"Models/light");

            // Light specific parameters
            for (int i = 0; i < 10; i++)
            {
                //float x = MathHelper.Lerp(-500.0f, 500.0f, (float)random.NextDouble());
                float z = MathHelper.Lerp(-5000.0f, 0.0f, (float)random.NextDouble());
                Vector3 color = new Vector3(
                    MathHelper.Lerp(0.0f, 1.0f, (float)random.NextDouble()),
                    MathHelper.Lerp(0.0f, 1.0f, (float)random.NextDouble()),
                    MathHelper.Lerp(0.0f, 1.0f, (float)random.NextDouble()));
                Console.WriteLine(color);
                pointLights.Add(new PointLight(new Vector3(0.0f, 100.0f, z), color * 0.8f, 600.0f));
            }

            effect.CurrentTechnique = effect.Techniques["BasicShading"];

            // Initialize the material settings
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    BasicEffect basicEffect = (BasicEffect)part.Effect;
                    part.Effect = effect.Clone();
                    part.Effect.Parameters["MaterialAmbient"].SetValue(basicEffect.DiffuseColor * 0.5f);
                    part.Effect.Parameters["MaterialDiffuse"].SetValue(basicEffect.DiffuseColor);
                    part.Effect.Parameters["MaterialSpecular"].SetValue(Color.White.ToVector3());//basicEffect.SpecularColor
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

            KeyboardState keyboard = Keyboard.GetState();
            float millis = (float)gameTime.ElapsedGameTime.Milliseconds;
            if (keyboard.IsKeyDown(Keys.Left))
            {
                modelRotation += millis * MathHelper.ToRadians(0.05f);
            }
            else if (keyboard.IsKeyDown(Keys.Right))
            {
                modelRotation -= millis * MathHelper.ToRadians(0.05f);
            }

            if (keyboard.IsKeyDown(Keys.Subtract))
            {
                lightDistance -= millis * 1.0f;
            }
            else if (keyboard.IsKeyDown(Keys.Add))
            {
                lightDistance += millis * 1.0f;
            }


            /*lightRotation += (float)gameTime.ElapsedGameTime.Milliseconds * MathHelper.ToRadians(0.05f);

            PointLight pointLight = pointLights.First<PointLight>();
            pointLight.Position = Vector3.Transform(new Vector3(0.0f, 100.0f, 0.0f),
                Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, lightDistance)) * Matrix.CreateRotationY(lightRotation));
            */

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

            camera.Update(Keyboard.GetState(), Mouse.GetState(), screenCenter);

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

            Matrix view = camera.View;

            foreach (PointLight light in pointLights)
            {
                light.Draw(lightModel, view, projection);
            }


            plane.Draw(view, Color.Gray);


            // Draw car
            foreach (var mesh in car.Model.Meshes) // 5 meshes
            {
                foreach (Effect effect in mesh.Effects) // 5 effects for main, 1 for each wheel
                {
                    EffectParameterCollection parameters = effect.Parameters;
                    parameters["MaterialShininess"].SetValue(10.0f);

                    Matrix world = Matrix.Identity;
                    // If this mesh is a wheel, apply rotation
                    if (mesh.Name.StartsWith("wheel"))
                    {
                        world *= Matrix.CreateRotationX(car.WheelRotationX);

                        if (mesh.Name.EndsWith("001") || mesh.Name.EndsWith("002"))
                            world *= Matrix.CreateRotationY(car.WheelRotationY);
                    }
                    // Local worldspace, due to bad .X-file/exporter
                    world *= car.Model.Bones[1 + car.Model.Meshes.IndexOf(mesh) * 2].Transform;
                    world *= Matrix.CreateRotationY(car.Rotation) *
                        Matrix.CreateTranslation(car.Position);

                    parameters["World"].SetValue(world);

                    Matrix normalMatrix = Matrix.Invert(Matrix.Transpose(world));
                    parameters["NormalMatrix"].SetValue(normalMatrix);

                    parameters["View"].SetValue(view);
                    parameters["Projection"].SetValue(projection);
                    parameters["EyePosition"].SetValue(camera.Position);

                    Vector3[] positions = new Vector3[pointLights.Count];
                    Vector3[] diffuses = new Vector3[pointLights.Count];
                    float[] ranges = new float[pointLights.Count];
                    for (int i=0; i<pointLights.Count; i++) {
                        positions[i] = pointLights[i].Position;
                        diffuses[i] = pointLights[i].Diffuse;
                        ranges[i] = pointLights[i].Range;
                    }
                    parameters["LightPosition"].SetValue(positions);
                    parameters["LightDiffuse"].SetValue(diffuses);
                    parameters["LightRange"].SetValue(ranges);
                    parameters["NumLights"].SetValue(pointLights.Count);

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
