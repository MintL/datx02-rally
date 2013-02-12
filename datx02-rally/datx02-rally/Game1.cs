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
using datx02_rally.ModelPresenters;
using datx02_rally.GameLogic;
using datx02_rally.MapGeneration;

namespace datx02_rally
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        #region Foliage
        Model oakTree;
        //Model mushroomGroup;
        Vector3[] treePositions;
        float[] treeRotations;
        #endregion

        // Model to represent a location of a pointlight
        Model lightModel;

        // 

        static int mapSize = 512;
        static float triangleSize = 100;
        static float heightScale = 33;

        RaceTrack raceTrack;

        Matrix projection;

        List<PointLight> pointLights = new List<PointLight>();
        DirectionalLight directionalLight;

        // Car

        Car car;
        Effect carEffect;
        CarShadingSettings carSettings = new CarShadingSettings()
        {
            MaterialReflection = .3f,
            MaterialShininess = 10
        };


        // P-systems

        List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        ParticleSystem plasmaSystem;
        ParticleSystem greenSystem;
        ParticleSystem airParticles;
        int nextSpawn;

        Random random = new Random();

        #region SkyBox

        Model skyBoxModel;
        Effect skyBoxEffect;
        TextureCube cubeMap;

        #endregion


        TerrainModel terrain;

        #region DynamicEnvironment
        RenderTargetCube refCubeMap;
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
            // Components

            var inputComponent = new InputComponent(this);
            //inputComponent.CurrentController = Controller.GamePad;
            Components.Add(inputComponent);
            Services.AddService(typeof(InputComponent), inputComponent);

            // Components

            var cameraComponent = new CameraComponent(this);
            Components.Add(cameraComponent);
            Services.AddService(typeof(CameraComponent), cameraComponent);

            var carControlComponent = new CarControlComponent(this);
            Components.Add(carControlComponent);
            Services.AddService(typeof(CarControlComponent), carControlComponent);

            Console.WriteLine("isConnected " + GamePad.GetState(PlayerIndex.One).IsConnected);

            // Particle systems

            plasmaSystem = new PlasmaParticleSystem(this, Content);
            Components.Add(plasmaSystem);
            particleSystems.Add(plasmaSystem);

            greenSystem = new GreenParticleSystem(this, Content);
            Components.Add(greenSystem);
            particleSystems.Add(greenSystem);

            airParticles = new AirParticleSystem(this, Content);
            Components.Add(airParticles);
            particleSystems.Add(airParticles);

            //smoke = new SmokePlumeParticleSystem(this, Content);
            //smoke.DrawOrder = 500;
            //Components.Add(smoke);

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

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                GraphicsDevice.Viewport.AspectRatio, .1f, 500000f);

            #region Lights

            // Load model to represent our lightsources
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

                //pointLights.Add(new PointLight(new Vector3(0.0f, 100.0f, z), color * 0.8f, 400.0f));
            }
            directionalLight = new DirectionalLight(new Vector3(-0.6f, -1.0f, 1.0f), new Vector3(1.0f, 0.8f, 1.0f) * 0.4f, Color.White.ToVector3() * 0.2f);

            #endregion

            // Load car effect (10p-light, env-map)
            carEffect = Content.Load<Effect>(@"Effects/CarShading");

            // TODO: Uses first Technique?
            //carEffect.CurrentTechnique = carEffect.Techniques["CarShading"];

            car = new Car(Content.Load<Model>(@"Models/porsche"), 10.4725f);

            foreach (var mesh in car.Model.Meshes)
            {
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

            carSettings.Projection = projection;

            // Keep some old settings from imported modeleffect, then replace with carEffect
            foreach (ModelMesh mesh in car.Model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    BasicEffect oldEffect = part.Effect as BasicEffect;
                    part.Effect = carEffect.Clone();
                    part.Effect.Parameters["MaterialDiffuse"].SetValue(oldEffect.DiffuseColor);
                    part.Effect.Parameters["MaterialAmbient"].SetValue(oldEffect.DiffuseColor * .5f);
                    part.Effect.Parameters["MaterialSpecular"].SetValue(oldEffect.DiffuseColor * .3f);

                }
            }

            this.GetService<CarControlComponent>().Car = car;

            #region MapGeneration


            HeightMap heightmapGenerator = new HeightMap(mapSize);
            var heightmap = heightmapGenerator.Generate();

            raceTrack = new RaceTrack(((mapSize / 2) * triangleSize));

            float roadWidth = 2;
            float lerpDist = 5;

            Vector3 lastPosition = raceTrack.Curve.GetPoint(.01f);
            lastPosition /= triangleSize;
            lastPosition += new Vector3(.5f, 0, .5f) * mapSize;
            for (float i = 0; i < 1; i += .0003f)
            {
                var e = raceTrack.Curve.GetPoint(i);
                e /= triangleSize;
                e += new Vector3(.5f, 0, .5f) * mapSize;

                for (float j = -lerpDist; j <= lerpDist; j++)
                {
                    var pos = e + j * Vector3.Normalize(Vector3.Cross(lastPosition - e, Vector3.Up));

                    // Indices
                    int x = (int)pos.X, z = (int)pos.Z;

                    if (Math.Abs(j) <= roadWidth)
                        heightmap[x, z] = .429f;
                    else
                        heightmap[x, z] = MathHelper.Lerp(.43f, heightmap[x, z], (Math.Abs(j) - roadWidth) / (lerpDist - roadWidth));
                }
                lastPosition = e;
            }


            // Creates a terrainmodel around Vector3.Zero
            terrain = new TerrainModel(GraphicsDevice, 0, mapSize, 0, mapSize, triangleSize, heightScale, heightmap);

            Effect terrainEffect = Content.Load<Effect>(@"Effects\TerrainShading");
            terrainEffect.Parameters["TextureMap0"].SetValue(Content.Load<Texture2D> (@"Terrain\sand"));
            terrainEffect.Parameters["TextureMap1"].SetValue(Content.Load<Texture2D> (@"Terrain\grass"));
            terrainEffect.Parameters["TextureMap2"].SetValue(Content.Load<Texture2D> (@"Terrain\rock"));
            terrainEffect.Parameters["TextureMap3"].SetValue(Content.Load<Texture2D> (@"Terrain\snow"));
            terrain.Effect = terrainEffect.Clone();
            terrain.Projection = projection;

            #endregion


            #region SkySphere

            skyBoxModel = Content.Load<Model>(@"Models/skybox");
            skyBoxEffect = Content.Load<Effect>(@"Effects/SkyBox");

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

            skyBoxEffect.Parameters["SkyboxTexture"].SetValue(cubeMap);

            foreach (var mesh in skyBoxModel.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = skyBoxEffect;
                }
            }

            #endregion

            #region Foliage
            oakTree = Content.Load<Model>(@"Foliage\Oak_tree");
            Effect alphaMapEffect = Content.Load<Effect>(@"Effects\AlphaMap");

            // Initialize the material settings
            foreach (ModelMesh mesh in oakTree.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    BasicEffect basicEffect = (BasicEffect)part.Effect;
                    part.Effect = alphaMapEffect.Clone();
                    part.Effect.Parameters["ColorMap"].SetValue(basicEffect.Texture);
                }
                mesh.Effects[0].Parameters["NormalMap"].SetValue(Content.Load<Texture2D>(@"Foliage\Textures\BarkMossy-tiled-n"));

                mesh.Effects[1].Parameters["NormalMap"].SetValue(Content.Load<Texture2D>(@"Foliage\Textures\leaf-mapple-yellow-ni"));
                mesh.Effects[1].Parameters["AlphaMap"].SetValue(Content.Load<Texture2D>(@"Foliage\Textures\leaf-mapple-yellow-a"));
            }

            treePositions = new Vector3[10];
            treeRotations = new float[10];
            for (int i = 0; i < 10; i++)
            {
                treePositions[i] = new Vector3(
                    MathHelper.Lerp(300, 800, (float)random.NextDouble()),
                    0,
                    MathHelper.Lerp(-200, 400, (float)random.NextDouble())
                );
                treeRotations[i] = MathHelper.Lerp(0, MathHelper.Pi * 2, (float)random.NextDouble());
            }

            // {
            //     mushroomGroup = Content.Load<Model>(@"Foliage\MushroomGroup");
            //     ModelMesh mesh = mushroomGroup.Meshes.First<ModelMesh>();
            //     foreach (ModelMeshPart part in mesh.MeshParts)
            //     {
            //         part.Effect = alphaMapEffect.Clone();
            //         part.Effect.Parameters["ColorMap"].SetValue(Content.Load<Texture2D>(@"Foliage\Textures\mushrooms-c"));
            //         part.Effect.Parameters["NormalMap"].SetValue(Content.Load<Texture2D>(@"Foliage\Textures\mushrooms-n"));
            //     }
            // }


            #endregion

            
            // Place car at start.

            Vector3 carPosition = raceTrack.Curve.GetPoint(0);
            Vector3 carHeading = (raceTrack.Curve.GetPoint(.001f) - carPosition);
            car.Position = carPosition;
            car.Rotation = (float)Math.Atan2(carHeading.X, carHeading.Z) - (float)Math.Atan2(0, -1);

            //Creates pointlights along the track
            for (int j = 0; j < 200; j++)
            {
                float i = j / 200.0f;
                Vector3 point1 = raceTrack.Curve.GetPoint(i);
                Vector3 point2 = raceTrack.Curve.GetPoint(i + .01f * (i > .5f ? -1 : 1));
                var heading = (point2 - point1);

                var side = 150 * Vector3.Normalize(Vector3.Cross(Vector3.Up, heading));

                pointLights.Add(new PointLight(point1 + side + Vector3.Up * 50, Color.Cyan.ToVector3() * 0.3f, 800.0f));
                pointLights.Add(new PointLight(point1 - side + Vector3.Up * 50, Color.Cyan.ToVector3() * 0.3f, 800.0f));
            }


            

            #region Cameras
            var input = this.GetService<InputComponent>();
            this.GetService<CameraComponent>().AddCamera(new DebugCamera(new Vector3(0, 200, 100), input));
            this.GetService<CameraComponent>().AddCamera(new ThirdPersonCamera(car, Vector3.Up * 50, input));
            #endregion

            #region DynamicEnvironment
            refCubeMap = new RenderTargetCube(this.GraphicsDevice, 256, true, SurfaceFormat.Color, DepthFormat.Depth16);
            carSettings.EnvironmentMap = refCubeMap;
            //skyBoxEffect.Parameters["SkyboxTexture"].SetValue(refCubeMap);
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

            // Spawn particles
            //Vector3 radius = 100 * Vector3.UnitX;
            //for (int z = 0; z < 10000; z += 1000)
            //{
            //    float next = (float)random.NextDouble();
            //    plasmaSystem.AddParticle(Vector3.Transform(radius + Vector3.UnitZ * next * -10000,
            //        Matrix.CreateRotationZ(MathHelper.TwoPi * 20 * next)), Vector3.Zero);
            //}

            //if (input.GetKey(Keys.Y))
                 // raceTrack.Curve = new GameLogic.Curve(25000);

            for (int j = 0; j < 20; j++)
            {
                var i = (float)random.NextDouble();
                Vector3 point1 = raceTrack.Curve.GetPoint(i);
                Vector3 point2 = raceTrack.Curve.GetPoint(i + .01f * (i > .5f ? -1 : 1));
                var heading = (point2 - point1);

                var side = 150 * Vector3.Normalize(Vector3.Cross(Vector3.Up, heading));

                plasmaSystem.AddParticle(point1 + side, Vector3.Zero);
                plasmaSystem.AddParticle(point1 - side, Vector3.Zero);
            }


            for (int i = 0; i < 1; i++)
            {
                greenSystem.AddParticle(new Vector3(105, 10, 100), Vector3.Up);
            }

            for (int i = 0; i < 1; i++)
            {
                nextSpawn -= gameTime.ElapsedGameTime.Milliseconds;
                Camera camera = this.GetService<CameraComponent>().CurrentCamera;
                if (camera is ThirdPersonCamera && nextSpawn <= 0)
                {
                    Vector3 direction = car.Position - camera.Position;
                    direction.Y = 0;
                    direction = Vector3.Normalize(direction);
                    Vector3 crossDirection = Vector3.Normalize(Vector3.Cross(direction, Vector3.Up));

                    Vector3 position = camera.Position;
                    position += direction * MathHelper.Lerp(400.0f, 1000.0f, (float)random.NextDouble());
                    position += crossDirection * MathHelper.Lerp(-400.0f, 400.0f, (float)random.NextDouble());
                    position += Vector3.Up * MathHelper.Lerp(-400.0f, 400.0f, (float)random.NextDouble());
                    airParticles.AddParticle(position, Vector3.Up);

                    nextSpawn += 100;
                }
            }

            //Apply changes to car
            car.Update();

            base.Update(gameTime);
        }

        private void RenderEnvironmentMap()
        {
            Matrix viewMatrix;
            for (int i = 0; i < 6; i++)
            {
                CubeMapFace cubeMapFace = (CubeMapFace)i;
                if (cubeMapFace == CubeMapFace.NegativeX)
                    viewMatrix = Matrix.CreateLookAt(car.Position, car.Position + Vector3.Left, Vector3.Up);
                else if (cubeMapFace == CubeMapFace.NegativeY)
                    viewMatrix = Matrix.CreateLookAt(car.Position, car.Position + Vector3.Down, Vector3.Forward);
                else if (cubeMapFace == CubeMapFace.PositiveZ)
                    viewMatrix = Matrix.CreateLookAt(car.Position, car.Position + Vector3.Backward, Vector3.Up);
                else if (cubeMapFace == CubeMapFace.PositiveX)
                    viewMatrix = Matrix.CreateLookAt(car.Position, car.Position + Vector3.Right, Vector3.Up);
                else if (cubeMapFace == CubeMapFace.PositiveY)
                    viewMatrix = Matrix.CreateLookAt(car.Position, car.Position + Vector3.Up, Vector3.Backward);
                else if (cubeMapFace == CubeMapFace.NegativeZ)
                    viewMatrix = Matrix.CreateLookAt(car.Position, car.Position + Vector3.Forward, Vector3.Up);
                else
                    viewMatrix = Matrix.Identity;

                GraphicsDevice.SetRenderTarget(refCubeMap, cubeMapFace);
                GraphicsDevice.Clear(Color.White);
                RenderScene(viewMatrix, true);
            }

            GraphicsDevice.SetRenderTarget(null);
        }

        private void RenderScene(Matrix view, bool environment)
        {
            Matrix[] transforms = new Matrix[car.Model.Bones.Count];
            car.Model.CopyAbsoluteBoneTransformsTo(transforms);

            GraphicsDevice.BlendState = BlendState.Opaque;

            #region SkyBox

            skyBoxEffect.Parameters["View"].SetValue(view);
            skyBoxEffect.Parameters["Projection"].SetValue(projection);
            skyBoxModel.Meshes[0].Draw();

            #endregion

            terrain.Draw(view, this.GetService<CameraComponent>().Position, directionalLight, pointLights);

            //testTerrain1.Draw(view);
            //testTerrain2.Draw(view);
            //testTerrain3.Draw(view);
            //testTerrain4.Draw(view);

            //terrainGenerator.DrawTerrain(view, projection);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            // Set view to particlesystems
            foreach (ParticleSystem pSystem in this.Components.Where(c => c is ParticleSystem))
                pSystem.SetCamera(view, projection);

            //smoke.SetCamera(view, projection);
            //plasmaSystem.SetCamera(view, projection);

            for (int i = 0; i < 10; i++)
            {
                pointLights[i].Draw(lightModel, view, projection);
            }

            //foreach (PointLight light in pointLights)
            //{
            //    light.Draw(lightModel, view, projection);
            //}

            #region Foliage
            for (int i = 0; i < 10; i++)
            {
                DrawModel(oakTree, view, treePositions[i], treeRotations[i]);
            }

            //DrawModel(mushroomGroup, new Vector3(100, 0, 100), 0.0f);
            #endregion


            if (!environment)
            {
                DrawCar(view, projection);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Honeydew);
            skyBoxEffect.Parameters["ElapsedTime"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);

            //RenderEnvironmentMap();

            Matrix view = this.GetService<CameraComponent>().View;
            RenderScene(view, false);

            base.Draw(gameTime);
        }

        private void DrawModel(Model m, Matrix view, Vector3 position, float rotation)
        {
            Matrix[] transforms = new Matrix[m.Bones.Count];
            m.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in m.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    Matrix world = transforms[mesh.ParentBone.Index] *
                        Matrix.CreateTranslation(position) *
                        Matrix.CreateRotationY(rotation);
                    Matrix normalMatrix = Matrix.Invert(Matrix.Transpose(world));

                    //effect.Parameters["NormalMatrix"].SetValue(normalMatrix);
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);

                    effect.Parameters["DirectionalDirection"].SetValue(directionalLight.Direction);
                    effect.Parameters["DirectionalDiffuse"].SetValue(directionalLight.Diffuse);
                    effect.Parameters["DirectionalAmbient"].SetValue(directionalLight.Ambient);
                }
                mesh.Draw();
            }
        }

        private void DrawCar(Matrix view, Matrix projection)
        {
            carSettings.View = view;
            carSettings.Projection = projection;

            //carSettings.EyePosition = view.Translation;
            carSettings.EyePosition = this.GetService<CameraComponent>().Position;

            Vector3 direction = car.Position - carSettings.EyePosition;
            direction.Y = 0;
            direction = Vector3.Normalize(direction);
            Vector3 carPosition = car.Position;
            pointLights.Sort(
                delegate(PointLight x, PointLight y)
                {
                    if (Vector3.DistanceSquared(x.Position, carPosition) < Vector3.DistanceSquared(y.Position, carPosition))
                    {
                        return -1;
                    }
                    else {
                        return 1;
                    }
                }
            );

            Vector3[] positions = new Vector3[pointLights.Count];
            Vector3[] diffuses = new Vector3[pointLights.Count];
            float[] ranges = new float[pointLights.Count];
            for (int i = 0; i < 10; i++)
            {
                positions[i] = pointLights[i].Position;
                diffuses[i] = pointLights[i].Diffuse;
                ranges[i] = pointLights[i].Range;
            }

            carSettings.LightPosition = positions;
            carSettings.LightDiffuse = diffuses;
            carSettings.LightRange = ranges;
            carSettings.NumLights = 10;

            carSettings.DirectionalLightDirection = directionalLight.Direction;
            carSettings.DirectionalLightDiffuse = directionalLight.Diffuse;
            carSettings.DirectionalLightAmbient = directionalLight.Ambient;

            foreach (var mesh in car.Model.Meshes) // 5 meshes
            {
                Matrix world = Matrix.Identity;
                // Wheel transformation
                if ((int)mesh.Tag > 0)
                {
                    world *= Matrix.CreateRotationX(car.WheelRotationX);
                    if ((int)mesh.Tag > 1)
                        world *= Matrix.CreateRotationY(car.WheelRotationY);
                }

                // Local modelspace, due to bad .X-file/exporter
                world *= car.Model.Bones[1 + car.Model.Meshes.IndexOf(mesh) * 2].Transform;
                // World
                world *= car.RotationMatrix * car.TranslationMatrix;

                carSettings.World = world;
                carSettings.NormalMatrix = Matrix.Invert(Matrix.Transpose(world));

                foreach (Effect effect in mesh.Effects) // 5 effects for main, 1 for each wheel
                    effect.SetCarShadingParameters(carSettings);

                mesh.Draw();
            }

        }

    }
}
