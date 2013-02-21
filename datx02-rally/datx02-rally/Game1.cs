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
using datx02_rally.Entities;
using datx02_rally.Particles.Systems;
using datx02_rally.Particles.WeatherSystems;
using datx02_rally.Graphics;

namespace datx02_rally
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Field

        private static Game1 Instance = null;
        GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;

        Random random = new Random();

        Matrix projectionMatrix;

        #region PostProcess
        RenderTarget2D postProcessTexture;
        Effect postEffect;
        Bloom bloom;
        GaussianBlur gaussianBlur;

        #endregion

        #region Foliage

        Model oakTree;
        Vector3[] treePositions;
        Matrix[] treeTransforms;

        //Model mushroomGroup;

        #endregion

        #region Lights

        // Model to represent a location of a pointlight
        Model lightModel;

        List<PointLight> pointLights = new List<PointLight>();
        DirectionalLight directionalLight;

        #endregion

        #region Map

        int mapSize = 512;
        float triangleSize = 50;
        float heightScale = 150;  //33;

        float roadWidth = 6; // Grids
        float roadFalloff = 35; // Grids

        RaceTrack raceTrack;
        NavMeshVisualizer navMesh;

        TerrainModel terrain;

        #endregion

        #region Car

        public Car Car { get; private set; }
        Effect carEffect;
        CarShadingSettings carSettings = new CarShadingSettings()
        {
            MaterialReflection = .3f,
            MaterialShininess = 10
        };

        // Used for raycollision test.
        int lastTriangle;

        #endregion

        #region Particle-systems

        List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        ParticleSystem plasmaSystem;
        ParticleSystem redSystem;
        ParticleSystem yellowSystem;

        ParticleSystem greenSystem;
        ParticleSystem airParticles;

        ParticleEmitter[] dustEmitter;
        ParticleSystem[] dustParticles;

        #endregion

        #region SkyBox

        Model skyBoxModel;
        Effect skyBoxEffect;
        TextureCube cubeMap;

        #endregion

        #region Weather

        ThunderParticleSystem thunderParticleSystem;
        ThunderBoltGenerator thunderBoltGenerator;

        RainParticleSystem rainSystem;

        #endregion

        #region DynamicEnvironment

        RenderTargetCube refCubeMap;

        #endregion

        #endregion

        #region Initialization

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1366;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();

            //graphics.ToggleFullScreen();

            IsMouseVisible = true;
            Instance = this;
        }

        public static Game1 GetInstance()
        {
            return Instance;
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

            var consoleComponent = new HUDConsoleComponent(this);
            Components.Add(consoleComponent);
            Services.AddService(typeof(HUDConsoleComponent), consoleComponent);

            var serverComponent = new ServerClient(this);
            Components.Add(serverComponent);
            Services.AddService(typeof(ServerClient), serverComponent);

            Console.WriteLine("isConnected " + GamePad.GetState(PlayerIndex.One).IsConnected);

            // Particle systems

            plasmaSystem = new PlasmaParticleSystem(this, Content);
            Components.Add(plasmaSystem);
            particleSystems.Add(plasmaSystem);

            redSystem = new RedPlasmaParticleSystem(this, Content);
            Components.Add(redSystem);
            particleSystems.Add(redSystem);

            yellowSystem = new YellowPlasmaParticleSystem(this, Content);
            Components.Add(yellowSystem);
            particleSystems.Add(yellowSystem);

            greenSystem = new GreenParticleSystem(this, Content);
            Components.Add(greenSystem);
            particleSystems.Add(greenSystem);

            airParticles = new AirParticleSystem(this, Content);
            Components.Add(airParticles);
            particleSystems.Add(airParticles);

            dustParticles = new ParticleSystem[2];
            dustParticles[0] = new SmokePlumeParticleSystem(this, Content);
            dustParticles[1] = new SmokePlumeParticleSystem(this, Content);
            foreach (var dustSystem in dustParticles)
            {
                Components.Add(dustSystem);
                particleSystems.Add(dustSystem);
            }

            thunderParticleSystem = new ThunderParticleSystem(this, Content);
            Components.Add(thunderParticleSystem);
            particleSystems.Add(thunderParticleSystem);

            rainSystem = new RainParticleSystem(this, Content);
            Components.Add(rainSystem);
            particleSystems.Add(rainSystem);

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

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                GraphicsDevice.Viewport.AspectRatio, .1f, 500000f);

            #region Lights

            // Load model to represent our lightsources
            lightModel = Content.Load<Model>(@"Models/light");

            //directionalLight = new DirectionalLight(new Vector3(-0.6f, -1.0f, 1.0f), new Vector3(1.0f, 0.8f, 1.0f) * 0.4f, Color.White.ToVector3() * 0.2f);
            directionalLight = new DirectionalLight(new Vector3(-1.0f, -1.0f, 1.0f), new Vector3(1.0f, 0.8f, 1.0f) * 0.1f, Color.White.ToVector3() * 0.4f);
            Services.AddService(typeof(DirectionalLight), directionalLight);

            #endregion

            #region MapGeneration

            HeightMap heightmapGenerator = new HeightMap(mapSize);
            var heightmap = heightmapGenerator.Generate();

            var roadMap = new float[mapSize, mapSize];
            raceTrack = new RaceTrack(((mapSize / 2) * triangleSize));

            navMesh = new NavMeshVisualizer(GraphicsDevice, raceTrack.Curve, 1500, roadWidth * triangleSize, triangleSize, heightScale);

            Vector3 lastPosition = raceTrack.Curve.GetPoint(.01f);
            lastPosition /= triangleSize;
            lastPosition += new Vector3(.5f, 0, .5f) * mapSize;
            for (float t = 0; t < 1; t += .0002f)
            {
                var e = raceTrack.Curve.GetPoint(t);
                var height = e.Y;
                e /= triangleSize;
                e += new Vector3(mapSize) / 2f;

                for (float j = -roadFalloff; j <= roadFalloff; j++)
                {
                    var pos = e + j * Vector3.Normalize(Vector3.Cross(lastPosition - e, Vector3.Up));

                    // Indices
                    int x = (int)pos.X, z = (int)pos.Z;

                    if (Math.Abs(j) <= roadWidth)
                    {
                        heightmap[x, z] = height;
                        roadMap[x, z] = 1;
                    }
                    else
                    {
                        float amount = (Math.Abs(j) - roadWidth) / (roadFalloff - roadWidth);
                        heightmap[x, z] = MathHelper.Lerp(height,
                            heightmap[x, z], amount);
                        roadMap[x, z] = amount / 10f;
                    }
                }
                lastPosition = e;
            }

            heightmapGenerator.Smoothen();
            heightmapGenerator.Perturb(30f);

            // Creates a terrainmodel around Vector3.Zero
            terrain = new TerrainModel(GraphicsDevice, 0, mapSize, 0, mapSize, triangleSize, heightScale, heightmap, roadMap);

            Effect terrainEffect = Content.Load<Effect>(@"Effects\TerrainShading");
            terrainEffect.Parameters["TextureMap0"].SetValue(Content.Load<Texture2D>(@"Terrain\sand"));
            terrainEffect.Parameters["TextureMap1"].SetValue(Content.Load<Texture2D>(@"Terrain\grass"));
            terrainEffect.Parameters["TextureMap2"].SetValue(Content.Load<Texture2D>(@"Terrain\rock"));
            terrainEffect.Parameters["TextureMap3"].SetValue(Content.Load<Texture2D>(@"Terrain\snow"));
            terrain.Effect = terrainEffect.Clone();

            #endregion

            #region Car

            Car = MakeCar();
            Player localPlayer = this.GetService<ServerClient>().LocalPlayer;
            this.GetService<CarControlComponent>().Cars[localPlayer] = Car;

            #endregion

            dustEmitter = new ParticleEmitter[]{
                new ParticleEmitter(dustParticles[0], 150, Car.Position),
                new ParticleEmitter(dustParticles[1], 150, Car.Position)
            };

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

            //cubeMap = new TextureCube(GraphicsDevice, 1024, false, SurfaceFormat.Color);
            //string[] cubemapfaces = { 
            //    @"SkyBoxes/StormyDays/stormydays_ft", 
            //    @"SkyBoxes/StormyDays/stormydays_bk", 
            //    @"SkyBoxes/StormyDays/stormydays_up", 
            //    @"SkyBoxes/StormyDays/stormydays_dn", 
            //    @"SkyBoxes/StormyDays/stormydays_rt", 
            //    @"SkyBoxes/StormyDays/stormydays_lf" 
            //};

            //cubeMap = new TextureCube(GraphicsDevice, 1024, false, SurfaceFormat.Color);
            //string[] cubemapfaces = { 
            //    @"SkyBoxes/Miramar/miramar_ft", 
            //    @"SkyBoxes/Miramar/miramar_bk",
            //    @"SkyBoxes/Miramar/miramar_up", 
            //    @"SkyBoxes/Miramar/miramar_dn", 
            //    @"SkyBoxes/Miramar/miramar_rt",
            //    @"SkyBoxes/Miramar/miramar_lf"
            //};


            for (int i = 0; i < cubemapfaces.Length; i++)
                LoadCubemapFace(cubeMap, cubemapfaces[i], (CubeMapFace)i);

            skyBoxEffect.Parameters["SkyboxTexture"].SetValue(cubeMap);

            foreach (var mesh in skyBoxModel.Meshes)
                foreach (var part in mesh.MeshParts)
                    part.Effect = skyBoxEffect;

            #endregion

            #region Weather

            thunderBoltGenerator = new ThunderBoltGenerator(this, thunderParticleSystem);
            Components.Add(thunderBoltGenerator);

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

            int numTrees = 50;
            treePositions = new Vector3[numTrees];
            treeTransforms = new Matrix[numTrees];
            for (int i = 0; i < numTrees; i++)
            {
                //treePositions[i] = new Vector3(
                //    MathHelper.Lerp(300, 800, (float)random.NextDouble()),
                //    0,
                //    MathHelper.Lerp(-200, 400, (float)random.NextDouble())
                //);

                var t = navMesh.triangles[random.Next(navMesh.triangles.Length)];
                float v = (float)random.NextDouble();
                float u = ((float)random.NextDouble() - .5f);
                if (u < 0)
                    u -= .5f;
                else
                    u += 1.5f;

                var treePos = t.vertices[0] + u * t.ab + v * t.ac;

                float X = treePos.X / triangleSize + mapSize / 2f,
                    Z = treePos.Z / triangleSize + mapSize / 2f;

                float Xlerp = X % 1f,
                    Zlerp = Z % 1f;

                int x0 = (int)X,
                    z0 = (int)Z,
                    x1 = x0 + 1,
                    z1 = z0 + 1;

                float height;
                if (Xlerp + Zlerp > 1)
                {
                    height = MathHelper.Lerp(
                        MathHelper.Lerp(heightmap[x0, z1], heightmap[x1, z1], Xlerp),
                        MathHelper.Lerp(heightmap[x1, z0], heightmap[x1, z1], Zlerp),
                        .5f);
                }
                else
                {
                    height = MathHelper.Lerp(
                        MathHelper.Lerp(heightmap[x0, z0], heightmap[x1, z0], Xlerp),
                        MathHelper.Lerp(heightmap[x0, z0], heightmap[x0, z1], Zlerp),
                        .5f);
                }

                treePos.Y = height * heightScale * triangleSize;

                treePositions[i] = treePos;
                treeTransforms[i] = Matrix.CreateScale(1 + 8 * (float)random.NextDouble()) * Matrix.CreateRotationY(MathHelper.Lerp(0, MathHelper.Pi * 2, (float)random.NextDouble()));
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

            #region Point lights

            int numlights = 50;
            Vector3 pointLightOffset = new Vector3(0, 50, 0);
            for (int i = 0; i < numlights; i++)
            {
                float t = i / (float)numlights;
                Vector3 point = raceTrack.Curve.GetPoint(t);
                //Vector3 point2 = raceTrack.Curve.GetPoint(t + .01f * (t > .5f ? -1 : 1));
                //var heading = (point2 - point1);
                //var side = triangleSize * roadWidth * Vector3.Normalize(Vector3.Cross(Vector3.Up, heading));
                //point1.Y *= heightScale * triangleSize;
                
                point.Y *= heightScale * triangleSize;

                Vector3 color = new Vector3(
                    .6f + .4f * (float)random.NextDouble(),
                    .6f + .4f * (float)random.NextDouble(),
                    .6f + .4f * (float)random.NextDouble());
                pointLights.Add(new PointLight(point + pointLightOffset, color, 500.0f));

            }

            #endregion

            #region Cameras

            var input = this.GetService<InputComponent>();
            this.GetService<CameraComponent>().AddCamera(new ThirdPersonCamera(Car, 75 * Vector3.Up, input));
            this.GetService<CameraComponent>().AddCamera(new DebugCamera(new Vector3(0, 200, 100), input));

            #endregion

            #region DynamicEnvironment
            refCubeMap = new RenderTargetCube(this.GraphicsDevice, 256, true, SurfaceFormat.Color, DepthFormat.Depth16);
            carSettings.EnvironmentMap = refCubeMap;
            //skyBoxEffect.Parameters["SkyboxTexture"].SetValue(refCubeMap);
            #endregion

            #region PostProcess
            postProcessTexture = new RenderTarget2D(GraphicsDevice,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, GraphicsDevice.PresentationParameters.DepthStencilFormat);
            postEffect = Content.Load<Effect>(@"Effects\PostProcess");

            gaussianBlur = new GaussianBlur(this);
            bloom = new Bloom(this, gaussianBlur);

            #endregion
        }

        public Car MakeCar()
        {
            // Load car effect (10p-light, env-map)
            carEffect = Content.Load<Effect>(@"Effects/CarShading");

            Car car = new Car(Content.Load<Model>(@"Models/porsche"), 10.4725f);

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


            // Keep some old settings from imported modeleffect, then replace with carEffect
            foreach (ModelMesh mesh in car.Model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    BasicEffect oldEffect = part.Effect as BasicEffect;
                    if (oldEffect != null)
                    {
                        part.Effect = carEffect.Clone();
                        part.Effect.Parameters["MaterialDiffuse"].SetValue(oldEffect.DiffuseColor);
                        part.Effect.Parameters["MaterialAmbient"].SetValue(oldEffect.DiffuseColor * .5f);
                        part.Effect.Parameters["MaterialSpecular"].SetValue(oldEffect.DiffuseColor * .3f);
                    }
                }
            }
            car.Model.Meshes[0].Effects[1].Parameters["MaterialUnshaded"].SetValue(true);
            car.Model.Meshes[0].Effects[1].Parameters["MaterialAmbient"].SetValue(Color.Red.ToVector3() * 2.0f);
            car.Model.Meshes[0].Effects[2].Parameters["MaterialUnshaded"].SetValue(true);
            car.Model.Meshes[0].Effects[2].Parameters["MaterialAmbient"].SetValue(Color.Red.ToVector3() * 2.0f);

            // Place car at start.
            SetCarAtStart(car);
            car.Update();
            return car;
        }

        private void SetCarAtStart(Car car)
        {
            Vector3 carPosition = raceTrack.Curve.GetPoint(0);
            Vector3 carHeading = (raceTrack.Curve.GetPoint(.001f) - carPosition);
            car.Position = carPosition;
            car.Rotation = (float)Math.Atan2(carHeading.X, carHeading.Z) - (float)Math.Atan2(0, -1);
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

        #endregion

        #region Update

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

            if (input.GetPressed(Input.Console))
                this.GetService<HUDConsoleComponent>().Toggle();

            //Apply changes to car
            Car.Update();

            #region Ray

            bool onTrack = false;

            for (int i = 0; i < navMesh.triangles.Length; i++)
            {
                var triangle = navMesh.triangles[i];

                if (CollisionCheck(triangle))
                {
                    lastTriangle = i;
                    onTrack = true;
                    //break;
                }
            }

            if (!onTrack)
            {
                // Project car.Pos on lastTriangle.
                var t = navMesh.triangles[lastTriangle];

                float coord = Vector3.Dot(Car.Position - t.vertices[0], t.ac) / t.ac.LengthSquared();
                bool trans = false;

                if (coord < 0)
                {
                    trans = true;
                    lastTriangle += (lastTriangle % 2 == 0 ? -2 : 2);
                }
                else if (coord > 1)
                {
                    trans = true;
                    lastTriangle += (lastTriangle % 2 == 0 ? 2 : -2);
                }

                if (lastTriangle < 0)
                    lastTriangle += navMesh.triangles.Length;
                if (lastTriangle >= navMesh.triangles.Length)
                    lastTriangle -= navMesh.triangles.Length;

                if (!trans)
                {
                    Car.Position = coord * t.ac + t.vertices[0];
                }
                else if (!CollisionCheck(navMesh.triangles[lastTriangle]))
                {
                    t = navMesh.triangles[lastTriangle];
                    coord = MathHelper.Clamp(Vector3.Dot(Car.Position - t.vertices[0], t.ac) / t.ac.LengthSquared(), 0, 1);
                    Car.Position = coord * t.ac + t.vertices[0];
                    Car.Normal = t.normal;
                }
            }

            #endregion

            for (int x = -6; x < 6; x++)
            {
                for (int z = -6; z < 6; z++)
                {
                    rainSystem.AddParticle(Car.Position + new Vector3(
                        (float)random.NextDouble() * x * 200,
                        500 * (float)random.NextDouble(),
                        (float)random.NextDouble() * z * 200),
                        Vector3.Down);
                }
            }

            
            Vector3 carDirection = Vector3.Transform(Vector3.Forward,
                Matrix.CreateRotationY(Car.Rotation));
            Vector3 carPosition = Car.Position + carDirection * 1000;
            pointLights.Sort(
                delegate(PointLight x, PointLight y)
                {
                    return (int)(Vector3.DistanceSquared(x.Position, carPosition) - Vector3.DistanceSquared(y.Position, carPosition));
                }
            );

            base.Update(gameTime);
        }

        private bool CollisionCheck(NavMeshVisualizer.NavMeshTriangle triangle)
        {
            var downray = new Ray(Car.Position, Vector3.Down);
            var upray = new Ray(Car.Position, Vector3.Up);

            float? d1 = downray.Intersects(triangle.trianglePlane),
                d2 = upray.Intersects(triangle.trianglePlane);

            if (d1.HasValue || d2.HasValue)
            {
                float d = d1.HasValue ? d1.Value : d2.Value;
                Ray ray = d1.HasValue ? downray : upray;

                var point = ray.Position + d * ray.Direction;

                bool onTriangle = PointInTriangle(triangle.vertices[0],
                    triangle.vertices[1],
                    triangle.vertices[2],
                    point);

                if (onTriangle)
                {
                    Car.Position = point;
                    Car.Normal = triangle.normal;
                }

                return onTriangle;
            }
            return false;
        }

        /// <summary>
        /// Determine whether a point P is inside the triangle ABC. Note, this function
        /// assumes that P is coplanar with the triangle.
        /// </summary>
        /// <returns>True if the point is inside, false if it is not.</returns>
        public static bool PointInTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
        {
            // Prepare our barycentric variables
            Vector3 u = B - A;
            Vector3 v = C - A;
            Vector3 w = P - A;
            Vector3 vCrossW = Vector3.Cross(v, w);
            Vector3 vCrossU = Vector3.Cross(v, u);

            // Test sign of r
            if (Vector3.Dot(vCrossW, vCrossU) < 0)
                return false;

            Vector3 uCrossW = Vector3.Cross(u, w);
            Vector3 uCrossV = Vector3.Cross(u, v);

            // Test sign of t
            if (Vector3.Dot(uCrossW, uCrossV) < 0)
                return false;

            // At this piont, we know that r and t and both > 0
            float denom = uCrossV.Length();
            float r = vCrossW.Length() / denom;
            float t = uCrossW.Length() / denom;

            return (r <= 1 && t <= 1 && r + t <= 1);
        }

        #endregion

        #region Rendering

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Honeydew);
            skyBoxEffect.Parameters["ElapsedTime"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);

            Matrix view = this.GetService<CameraComponent>().View;

            if (!GameSettings.Default.PerformanceMode)
                RenderEnvironmentMap(gameTime);

            GraphicsDevice.SetRenderTarget(postProcessTexture);

            // Reset render settings
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            GraphicsDevice.Clear(Color.White);

            RenderScene(gameTime, view, projectionMatrix, false);
            base.Draw(gameTime);

            GraphicsDevice.SetRenderTarget(null);

                //System.IO.Stream stream = System.IO.File.OpenWrite(@"C:\Development\tex.jpg");
                //renderTarget.SaveAsJpeg(stream, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                //stream.Dispose();

            RenderPostProcess();

        }

        private void RenderPostProcess()
        {
            // Apply bloom effect
            Texture2D finalTexture;
            finalTexture = bloom.PerformBloom(postProcessTexture);

            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, postEffect);
            foreach (EffectPass pass in postEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                spriteBatch.Draw(finalTexture, Vector2.Zero, Color.White);

            }
            spriteBatch.End();

        }

        private void RenderEnvironmentMap(GameTime gameTime)
        {
            Matrix viewMatrix;
            for (int i = 0; i < 6; i++)
            {
                CubeMapFace cubeMapFace = (CubeMapFace)i;
                if (cubeMapFace == CubeMapFace.NegativeX)
                    viewMatrix = Matrix.CreateLookAt(Car.Position, Car.Position + Vector3.Left, Vector3.Up);
                else if (cubeMapFace == CubeMapFace.NegativeY)
                    continue;
                //viewMatrix = Matrix.CreateLookAt(Car.Position, Car.Position + Vector3.Down, Vector3.Forward);
                else if (cubeMapFace == CubeMapFace.PositiveZ)
                    viewMatrix = Matrix.CreateLookAt(Car.Position, Car.Position + Vector3.Backward, Vector3.Up);
                else if (cubeMapFace == CubeMapFace.PositiveX)
                    viewMatrix = Matrix.CreateLookAt(Car.Position, Car.Position + Vector3.Right, Vector3.Up);
                else if (cubeMapFace == CubeMapFace.PositiveY)
                    viewMatrix = Matrix.CreateLookAt(Car.Position, Car.Position + Vector3.Up, Vector3.Backward);
                else if (cubeMapFace == CubeMapFace.NegativeZ)
                    viewMatrix = Matrix.CreateLookAt(Car.Position, Car.Position + Vector3.Forward, Vector3.Up);
                else
                    viewMatrix = Matrix.Identity;

                GraphicsDevice.SetRenderTarget(refCubeMap, cubeMapFace);
                GraphicsDevice.Clear(Color.White);

                Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                    1.0f, 100f, 5000f);
                RenderScene(gameTime, viewMatrix, projection, true);
            }

            // Default target
            GraphicsDevice.SetRenderTarget(null);
        }

        private void RenderScene(GameTime gameTime, Matrix view, Matrix projection, bool environment)
        {
            BoundingFrustum viewFrustum = new BoundingFrustum(view * projection);
            
            Matrix[] transforms = new Matrix[Car.Model.Bones.Count];
            Car.Model.CopyAbsoluteBoneTransformsTo(transforms);

            GraphicsDevice.BlendState = BlendState.Opaque;

            #region SkyBox

            skyBoxEffect.Parameters["View"].SetValue(view);
            skyBoxEffect.Parameters["Projection"].SetValue(projection);
            skyBoxModel.Meshes[0].Draw();

            #endregion

            terrain.Draw(view, projection, this.GetService<CameraComponent>().Position, directionalLight, pointLights);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            // Set view to particlesystems
            foreach (ParticleSystem pSystem in particleSystems)
                pSystem.SetCamera(view, projection);

            for (int i = 0; i < 10; i++)
                pointLights[i].Draw(lightModel, view, projection);

            #region Foliage

            for (int i = 0; i < treePositions.Length; i++)
            {
                BoundingSphere sourceSphere = new BoundingSphere(treePositions[i], oakTree.Meshes[0].BoundingSphere.Radius);
                if (viewFrustum.Intersects(sourceSphere))
                    DrawModel(oakTree, view, projection, treePositions[i], treeTransforms[i]);
            }

            //DrawModel(mushroomGroup, new Vector3(100, 0, 100), 0.0f);

            #endregion

            if (!environment)
            {
                foreach (Car c in this.GetService<CarControlComponent>().Cars.Values) 
                    DrawCar(view, projection, c);
                DrawGhostCar(view, projection, gameTime);
            }
        }

        private void DrawModel(Model m, Matrix view, Matrix projection, Vector3 position, Matrix transform)
        {
            Matrix[] transforms = new Matrix[m.Bones.Count];
            m.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in m.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    Matrix world = transforms[mesh.ParentBone.Index] * transform *
                        Matrix.CreateTranslation(position);
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

        #region Car drawing

        private void DrawCar(Matrix view, Matrix projection, Car car)
        {
            carSettings.View = view;
            carSettings.Projection = projection;

            //carSettings.EyePosition = view.Translation;
            carSettings.EyePosition = this.GetService<CameraComponent>().Position;

            //Vector3 direction = car.Position - carSettings.EyePosition;
            //direction.Y = 0;
            //direction = Vector3.Normalize(direction);
            

            Vector3[] positions = new Vector3[pointLights.Count];
            Vector3[] diffuses = new Vector3[pointLights.Count];
            float[] ranges = new float[pointLights.Count];
            for (int i = 0; i < 2; i++)
            {
                positions[i] = pointLights[i].Position;
                diffuses[i] = pointLights[i].Diffuse;
                ranges[i] = pointLights[i].Range;
            }

            carSettings.LightPosition = positions;
            carSettings.LightDiffuse = diffuses;
            carSettings.LightRange = ranges;
            carSettings.NumLights = 2;

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

        float ghostWheelRotation = 0;
        Vector3 ghostPosition;

        private void DrawGhostCar(Matrix view, Matrix projection, GameTime gameTime)
        {
            float t = ((float)gameTime.TotalGameTime.TotalSeconds / 1020f) % 1f;

            foreach (var mesh in Car.Model.Meshes) // 5 meshes
            {
                Matrix world = Matrix.Identity;

                // Wheel transformation
                if ((int)mesh.Tag > 0)
                {
                    world *= Matrix.CreateRotationX(ghostWheelRotation);
                }

                // Local modelspace, due to bad .X-file/exporter
                world *= Car.Model.Bones[1 + Car.Model.Meshes.IndexOf(mesh) * 2].Transform;

                // World
                Vector3 position = raceTrack.Curve.GetPoint(t);
                Vector3 heading = raceTrack.Curve.GetPoint((t + .01f) % 1f) - position;
                heading.Normalize();

                Vector3 normal = Vector3.Up;

                position.Y *= heightScale * triangleSize;

                foreach (var triangle in navMesh.triangles)
                {
                    var downray = new Ray(position, Vector3.Down);
                    var upray = new Ray(position, Vector3.Up);

                    float? d1 = downray.Intersects(triangle.trianglePlane),
                        d2 = upray.Intersects(triangle.trianglePlane);

                    if (d1.HasValue || d2.HasValue)
                    {
                        float d = d1.HasValue ? d1.Value : d2.Value;
                        Ray ray = d1.HasValue ? downray : upray;

                        var point = ray.Position + d * ray.Direction;

                        bool onTriangle = PointInTriangle(triangle.vertices[0],
                            triangle.vertices[1],
                            triangle.vertices[2],
                            point);

                        if (onTriangle)
                        {
                            position = point;
                            normal = triangle.normal;
                            break;
                        }
                    }
                }

                ghostWheelRotation -= (position - ghostPosition).Length() / 10.4725f;
                ghostPosition = position;

                world *= Vector3.Forward.GetRotationMatrix(heading) * Vector3.Up.GetRotationMatrix(normal) *
                    Matrix.CreateTranslation(position);

                carSettings.World = world;
                carSettings.NormalMatrix = Matrix.Invert(Matrix.Transpose(world));

                foreach (Effect effect in mesh.Effects) // 5 effects for main, 1 for each wheel
                    effect.SetCarShadingParameters(carSettings);

                mesh.Draw();
            }

        }

        #endregion

        #endregion
    }
}
