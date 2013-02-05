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
using Particle3DSample;

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

        float lightDistance = 700.0f;
        float lightRotation;
        
        /*
        float[,] heightMap;
        int mapSize;*/
        Model terrain;

        ThirdPersonCamera camera;
        Vector2 screenCenter;

        Matrix projection;

        List<PointLight> pointLights;
        DirectionalLight directionalLight;

        Effect effect;

        Car car;
        ParticleSystem smoke;
        ParticleEmitter particleEmitter;

        ParticleSystem plasmaSystem;

        PlaneModel plane;
        PlaneModel tree;
        List<Matrix> treeTransforms = new List<Matrix>();

        Random random = new Random();

        #region SkySphere

        Model skySphereModel;
        Effect skySphereEffect;
        TextureCube cubeMap;

        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1366;
            graphics.PreferredBackBufferHeight = 768;
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

            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            // Components
            var inputComponent = new InputComponent(this);
            //inputComponent.CurrentController = Controller.GamePad;
            Components.Add(inputComponent);
            Services.AddService(typeof(InputComponent), inputComponent);
            
            var debugCamera = new CameraComponent(this);
            Components.Add(debugCamera);
            Services.AddService(typeof(CameraComponent), debugCamera);

            var previousKeyboardStateComponent = new PreviousKeyboardState(this);
            Components.Add(previousKeyboardStateComponent);
            Services.AddService(typeof(PreviousKeyboardState), previousKeyboardStateComponent);

            plasmaSystem = new PlasmaParticleSystem(this, Content);
            Components.Add(plasmaSystem);

            var carControlComponent = new CarControlComponent(this);
            Components.Add(carControlComponent);
            Services.AddService(typeof(CarControlComponent), carControlComponent);

            Console.WriteLine("isConnected " + GamePad.GetState(PlayerIndex.One).IsConnected);

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
                pointLights.Add(new PointLight(new Vector3(0.0f, 100.0f, z), color * 0.8f, 400.0f));
            }
            directionalLight = new DirectionalLight(new Vector3(-0.6f, -1.0f, 1.0f), new Vector3(1.0f, 0.6f, 1.0f) * 0.2f, Color.White.ToVector3() * 0.3f);

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
                    part.Effect.Parameters["MaterialSpecular"].SetValue(basicEffect.DiffuseColor * 0.3f);//basicEffect.SpecularColor
                }
            }

            car = new Car(Content.Load<Model>(@"Models/porsche"), 10.4725f);
            this.GetService<CarControlComponent>().Car = car;

            plane = new PlaneModel(new Vector2(-10000), new Vector2(10000), 1, GraphicsDevice, null, projection, Matrix.Identity);

            #region MapGeneration
            /*

            MapGeneration.HeightMap hmGenerator = new MapGeneration.HeightMap(mapSize);

            heightMap = hmGenerator.Generate();

            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    Console.Write(heightMap[i,j]+" ");
                }
                Console.WriteLine();
            }*/

            terrain = Content.Load<Model>("heightmap");



            #endregion

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
            
            cubeMap = new TextureCube(GraphicsDevice, 2048, false, SurfaceFormat.Color);

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

            var input = this.GetService<InputComponent>();
            camera = new ThirdPersonCamera(car, Vector3.Up * 50, input);
            this.GetService<CameraComponent>().AddCamera(camera);
            this.GetService<CameraComponent>().AddCamera(new DebugCamera(new Vector3(0, 200, 100), input));
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
            base.Update(gameTime);
            
            InputComponent input = this.GetService<InputComponent>();

            if (input.GetPressed(Input.ChangeController))
            {
                if (input.CurrentController == Controller.Keyboard)
                {
                    input.CurrentController = Controller.GamePad;
                    Console.WriteLine("CurrentController equals GamePad");
                }
                else
                {
                    input.CurrentController = Controller.Keyboard;
                    Console.WriteLine("CurrentController equals Keyboard");
                }
            }
            // Allows the game to exit
            if (input.GetPressed(Input.Exit))
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

            plasmaSystem.AddParticle(new Vector3(200,50,-500), Vector3.Zero);

            //Apply changes to car
            car.Update();

            base.Update(gameTime);

            input.UpdatePreviousState();

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

            Matrix view = this.GetService<CameraComponent>().View;

            //smoke.SetCamera(view, projection);
            plasmaSystem.SetCamera(view, projection);

            foreach (PointLight light in pointLights)
            {
                light.Draw(lightModel, view, projection);
            }

            //plane.Draw(view, Color.Gray);


            #region Terrain

            foreach (ModelMesh mesh in terrain.Meshes)
            {
                foreach (BasicEffect currentEffect in mesh.Effects)
                {
                    currentEffect.World = transforms[mesh.ParentBone.Index] *
                                Matrix.CreateTranslation(Vector3.Zero);
                    currentEffect.View = view;
                    currentEffect.Projection = projection;
                    currentEffect.EnableDefaultLighting();
                    currentEffect.PreferPerPixelLighting = true;
                }
                mesh.Draw();
            }

            #endregion

            // Draw car
            foreach (var mesh in car.Model.Meshes) // 5 meshes
            {
                foreach (Effect effect in mesh.Effects) // 5 effects for main, 1 for each wheel
                {
                    EffectParameterCollection parameters = effect.Parameters;
                    parameters["MaterialShininess"].SetValue(10.0f);
                    parameters["MaterialReflection"].SetValue(0.3f);
                    parameters["EnvironmentMap"].SetValue(cubeMap);

                    Matrix world = Matrix.Identity;
                    // Wheel transformation
                    if (mesh.Name.StartsWith("wheel"))
                    {
                        world *= Matrix.CreateRotationX(car.WheelRotationX);

                        if (mesh.Name.EndsWith("001") || mesh.Name.EndsWith("002"))
                            world *= Matrix.CreateRotationY(car.WheelRotationY);
                    }

                    // Local modelspace, due to bad .X-file/exporter
                    world *= car.Model.Bones[1 + car.Model.Meshes.IndexOf(mesh) * 2].Transform;

                    world *= car.RotationMatrix * car.TranslationMatrix;

                    parameters["World"].SetValue(world);

                    Matrix normalMatrix = Matrix.Invert(Matrix.Transpose(world));
                    parameters["NormalMatrix"].SetValue(normalMatrix);

                    parameters["View"].SetValue(view);
                    parameters["Projection"].SetValue(projection);
                    parameters["EyePosition"].SetValue(view.Translation);

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

                    parameters["DirectionalDirection"].SetValue(directionalLight.Direction);
                    parameters["DirectionalDiffuse"].SetValue(directionalLight.Diffuse);
                    parameters["DirectionalAmbient"].SetValue(directionalLight.Ambient);

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

            foreach (var world in treeTransforms)
            {
                tree.World = world;
                tree.Draw(view);
            }

            base.Draw(gameTime);
        }
    }
}
