using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using datx02_rally.Graphics;
using datx02_rally.GameLogic;
using datx02_rally.Entities;

using datx02_rally.Particles.WeatherSystems;
using Microsoft.Xna.Framework.Content;
using datx02_rally.Particles.Systems;
using datx02_rally.MapGeneration;
using datx02_rally.EventTrigger;
using datx02_rally.Components;
using Microsoft.Xna.Framework.Input;
using datx02_rally.Particles;
using datx02_rally.GameplayModes;
using datx02_rally.Sound;

namespace datx02_rally.Menus
{
    class GamePlayView : GameStateView
    {
        #region Field

        bool init = false;
        public bool Initialized { get; set; }
        public GameplayMode mode;
        GameModeChoice gameModeChoice;

        OverlayComponent overlayComponent;
        PauseMenu pauseMenu;
        GameOverMenu gameOverMenu;
        Matrix projectionMatrix;

        #region PostProcess

        PostProcessingComponent postProcessingComponent;

        RenderTarget2D postProcessTexture;

        #endregion

        List<GameObject> GraphicalObjects = new List<GameObject>();
        List<GameObject> ShadowCasterObjects = new List<GameObject>();
        List<GameObject> FireflyCandidates = new List<GameObject>();

        #region Animals

        Model birdModel;
        datx02_rally.GameLogic.Curve birdCurve;

        #endregion

        #region Lights

        List<PointLight> pointLights = new List<PointLight>();
        List<SpotLight> spotLights = new List<SpotLight>();
        DirectionalLight directionalLight;

        #endregion

        #region Countdown
        Color[] countdownColors = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green };
        
        /// <summary>
        /// Time spent on the GO state
        /// </summary>
        TimeSpan countdownTimeGo = TimeSpan.Zero;
        #endregion

        #region Level terrain

        // Old value: 32
        int terrainSegmentSize = 25;
        int terrainSegmentsCount = 16;

        // XZ- & Y scaling.
        //Vector3 terrainScale = new Vector3(75, 10000, 75);
        Vector3 terrainScale = new Vector3(75, 3500, 75);

        float roadWidth = 7; // #Quads
        float roadFalloff = 30; // #Quads

        RaceTrack raceTrack;
        NavMesh navMesh;

        TerrainModel[,] terrainSegments;

        Effect terrainEffect;

        #endregion

        #region Car

        public Car Car { get; private set; }
        Effect carEffect;
        
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

        ParticleEmitter dustEmitter;
        ParticleSystem dustSystem;

        ParticleSystem smokeSystem;
        float smokeTime;

        ParticleSystem fireflySystem;
        List<ParticleEmitter> fireflyEmitter = new List<ParticleEmitter>();

        FireParticleSystem fireSystem;
        SmokePlumeParticleSystem fireSmokeSystem;

        #endregion

        #region SkyBox

        Model skyBoxModel;
        Effect skyBoxEffect;
        TextureCube skyMap;

        #endregion

        #region Weather

        ThunderParticleSystem thunderParticleSystem;
        ThunderBoltGenerator thunderBoltGenerator;

        RainParticleSystem rainSystem;

        #endregion

        #region DynamicEnvironment

        RenderTargetCube environmentCubeMap;

        #endregion

        #region ShadowMap

        Effect shadowMapEffect;
        Plane zeroPlane = new Plane(Vector3.Up, 0);
        bool shadowMapNotRendered = true;

        #endregion

        #region Audio
        LoopSoundManager loopSoundManager = new LoopSoundManager();
        #endregion

        PrelightingRenderer prelightingRenderer;

        BoundingFrustum viewFrustum;

        #endregion

        #region Initialization

        public GamePlayView(Game game, int? seed, GameModeChoice gamePlayChoice)
            : base(game, GameState.Gameplay)
        {
            game.GetService<ServerClient>().GamePlay = this;
            int usedSeed = seed.HasValue ? seed.Value : 0;
            UniversalRandom.ResetInstance(usedSeed);
            gameModeChoice = gamePlayChoice;

            content = new ContentManager(Game.Services, "Content");

            UpdateOrder = -1;
            DrawOrder = -1;
        }

        public override void ChangeResolution()
        {
        }

        public override void Initialize()
        {
            // Components
            var services = Game.Services;

            var hudComponent = new HUDComponent(gameInstance);
            Game.Components.Add(hudComponent);
            Game.AddService(typeof(HUDComponent), hudComponent);
            
            var carControlComponent = new CarControlComponent(gameInstance);
            Game.Components.Add(carControlComponent);
            Game.AddService(typeof(CarControlComponent), carControlComponent);

            var triggerManager = new TriggerManager(gameInstance);
            Game.Components.Add(triggerManager);
            Game.AddService(typeof(TriggerManager), triggerManager);

            // Particle systems

            plasmaSystem = new PlasmaParticleSystem(gameInstance, content);
            Game.Components.Add(plasmaSystem);
            particleSystems.Add(plasmaSystem);

            redSystem = new RedPlasmaParticleSystem(gameInstance, content);
            Game.Components.Add(redSystem);
            particleSystems.Add(redSystem);

            yellowSystem = new YellowPlasmaParticleSystem(gameInstance, content);
            Game.Components.Add(yellowSystem);
            particleSystems.Add(yellowSystem);

            greenSystem = new GreenParticleSystem(gameInstance, content);
            Game.Components.Add(greenSystem);
            particleSystems.Add(greenSystem);

            airParticles = new AirParticleSystem(gameInstance, content);
            Game.Components.Add(airParticles);
            particleSystems.Add(airParticles);

            thunderParticleSystem = new ThunderParticleSystem(gameInstance, content);
            Game.Components.Add(thunderParticleSystem);
            particleSystems.Add(thunderParticleSystem);

            rainSystem = new RainParticleSystem(gameInstance, content);
            Game.Components.Add(rainSystem);
            particleSystems.Add(rainSystem);

            smokeSystem = new SmokeCloudParticleSystem(gameInstance, content);
            Game.Components.Add(smokeSystem);
            particleSystems.Add(smokeSystem);

            fireflySystem = new FireflySystem(gameInstance, content);
            Game.Components.Add(fireflySystem);
            particleSystems.Add(fireflySystem);

            dustSystem = new DustParticleSystem(gameInstance, content);
            Game.Components.Add(dustSystem);
            particleSystems.Add(dustSystem);

            fireSmokeSystem = new SmokePlumeParticleSystem(gameInstance, content);
            Game.Components.Add(fireSmokeSystem);
            particleSystems.Add(fireSmokeSystem);

            fireSystem = new FireParticleSystem(gameInstance, content);
            Game.Components.Add(fireSystem);
            particleSystems.Add(fireSystem);

            pauseMenu = new PauseMenu(gameInstance);
            pauseMenu.ChangeResolution();
            pauseMenu.Enabled = false;
            Game.Components.Add(pauseMenu);

            gameOverMenu = new GameOverMenu(gameInstance);
            gameOverMenu.ChangeResolution();
            gameOverMenu.Enabled = false;
            Game.Components.Add(gameOverMenu);

            //dustSystem.Enabled = false;
            //rainSystem.Enabled = false;
            //smokeSystem.Enabled = false;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Overlay component, used to draw the pause menu and game over menu
            overlayComponent = new OverlayComponent(Game, spriteBatch);
            Game.Components.Add(overlayComponent);

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                GraphicsDevice.Viewport.AspectRatio, 0.1f, 50000);

            directionalLight = new DirectionalLight(
                new Vector3(-1.25f, -2f, 5.0f), // Direction
                new Vector3(.1f, .1f, .1f),//new Vector3(.15f, .14f, .29f), // Ambient
                new Vector3(.46f, .33f, .75f)); // Diffuse

            Game.AddService(typeof(DirectionalLight), directionalLight);

            #region Level terrain generation

            int heightMapSize = terrainSegmentsCount * terrainSegmentSize + 1;
            float halfHeightMapSize = heightMapSize / 2f;
            HeightMap heightmapGenerator = new HeightMap(heightMapSize);
            var heightMap = heightmapGenerator.Generate();

            var roadMap = new float[heightMapSize, heightMapSize];
            raceTrack = new RaceTrack(heightMapSize, terrainScale);

            navMesh = new NavMesh(GraphicsDevice, raceTrack.Curve, 1500, roadWidth, terrainScale);

            Vector3 lastPosition = raceTrack.Curve.GetPoint(.01f) / terrainScale;

            for (float t = 0; t < 1; t += .0002f)
            {
                var e = raceTrack.Curve.GetPoint(t) / terrainScale;

                for (float j = -roadFalloff; j <= roadFalloff; j++)
                {
                    var pos = e + j * Vector3.Normalize(Vector3.Cross(lastPosition - e, Vector3.Up));

                    // Indices
                    int x = (int)(pos.X + halfHeightMapSize),
                        z = (int)(pos.Z + halfHeightMapSize);

                    float height = e.Y;

                    if (Math.Abs(j) <= roadWidth)
                    {
                        heightMap[x, z] = height;
                        roadMap[x, z] = 1;
                    }
                    else
                    {
                        float amount = (Math.Abs(j) - roadWidth) / (roadFalloff - roadWidth);
                        heightMap[x, z] = MathHelper.Lerp(height,
                            heightMap[x, z], amount);
                        roadMap[x, z] = amount / 10f;
                    }
                }
                lastPosition = e;
            }

            heightmapGenerator.Smoothen();
            heightmapGenerator.Perturb(30f);

            for (int i = 0; i < 5; i++)
            {
                heightmapGenerator.Smoothen();
            }

            terrainEffect = content.Load<Effect>(@"Effects\TerrainShading");

            

            //terrainEffect.Parameters["TextureMap0"].SetValue(content.Load<Texture2D>(@"Terrain\sand"));
            #region TEXTURE RENDERING

            //var unprocessedGrassTexture = content.Load<Texture2D>(@"Terrain\grass");
            //var grassTexture = new RenderTarget2D(GraphicsDevice, unprocessedGrassTexture.Width, unprocessedGrassTexture.Height);
            
            //GraphicsDevice.SetRenderTarget(grassTexture);
            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            //spriteBatch.Draw(unprocessedGrassTexture, new Rectangle(0, 0, unprocessedGrassTexture.Width, unprocessedGrassTexture.Height), Color.White);
            //spriteBatch.Draw(content.Load<Texture2D>(@"Particles\fire"), new Rectangle(0, 0, unprocessedGrassTexture.Width, unprocessedGrassTexture.Height), Color.White);
            //spriteBatch.End();
            //GraphicsDevice.SetRenderTarget(null);

            //terrainEffect.Parameters["TextureMap1"].SetValue(grassTexture);

            #endregion
            terrainEffect.Parameters["TextureMap0"].SetValue(content.Load<Texture2D>(@"Terrain\road"));
            terrainEffect.Parameters["TextureMap1"].SetValue(content.Load<Texture2D>(@"Terrain\grass"));
            terrainEffect.Parameters["TextureMap2"].SetValue(content.Load<Texture2D>(@"Terrain\rock"));
            terrainEffect.Parameters["TextureMap3"].SetValue(content.Load<Texture2D>(@"Terrain\snow"));
            terrainEffect.Parameters["RoadNormalMap"].SetValue(content.Load<Texture2D>(@"Terrain\road_n"));
            terrainEffect.Parameters["Projection"].SetValue(projectionMatrix);

            // Creates a terrainmodel around Vector3.Zero
            terrainSegments = new TerrainModel[terrainSegmentsCount, terrainSegmentsCount];

            float terrainStart = -.5f * heightMapSize;

            for (int z = 0; z < terrainSegmentsCount; z++)
            {
                for (int x = 0; x < terrainSegmentsCount; x++)
                {
                    terrainSegments[x, z] = new TerrainModel(GraphicsDevice,
                        terrainSegmentSize, terrainSegmentsCount, terrainStart,
                        x * terrainSegmentSize, z * terrainSegmentSize,
                        terrainScale, heightMap, roadMap, terrainEffect, directionalLight);
                }
            }

            #endregion

            #region Car

            Car = MakeCar();
            Player localPlayer = gameInstance.GetService<ServerClient>().LocalPlayer;
            gameInstance.GetService<CarControlComponent>().Cars[localPlayer] = Car;
            gameInstance.AddService(typeof(Player), localPlayer);

            #endregion

            #region Lights

            // Load model to represent our lightsources
            var pointLightModel = content.Load<Model>(@"Models\light");

            //spotLightModel = content.Load<Model>(@"Models\Cone");

            Vector3 pointLightOffset = new Vector3(0, 250, 0);

            var cr = new CurveRasterization(raceTrack.Curve, 75);

            float colorOffset = 0f;

            foreach (var point in cr.Points)
            {
                Random r = UniversalRandom.GetInstance();

                Vector3 color = new Vector3(0f,0f,0f);
                PointLight pl = new PointLight(point.Position + pointLightOffset, color, 450)
                {
                    Model = pointLightModel, 
                    ColorTimeOffset = colorOffset
                };

                pointLights.Add(pl);
                GraphicalObjects.Add(pl);

                colorOffset += 100 / cr.Points.Count;
            }

            #endregion

            dustEmitter = new ParticleEmitter(dustSystem, 150, Car.Position);

            #region SkySphere

            skyBoxModel = content.Load<Model>(@"Models/skybox");
            skyBoxEffect = content.Load<Effect>(@"Effects/SkyBox");

            skyMap = new TextureCube(GraphicsDevice, 2048, false, SurfaceFormat.Color);
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
                LoadCubemapFace(skyMap, cubemapfaces[i], (CubeMapFace)i);

            skyBoxEffect.Parameters["SkyboxTexture"].SetValue(skyMap);

            foreach (var mesh in skyBoxModel.Meshes)
                foreach (var part in mesh.MeshParts)
                    part.Effect = skyBoxEffect;

            #endregion

            #region Weather

            thunderBoltGenerator = new ThunderBoltGenerator(gameInstance, thunderParticleSystem);
            gameInstance.Components.Add(thunderBoltGenerator);

            #endregion

            #region GameObjects

            OakTree.LoadMaterial(content);
            BirchTree.LoadMaterial(content);
            Stone.LoadMaterial(content);

            int numObjects = 75;

            for (int i = 0; i < numObjects; i++)
            {

                var t = navMesh.triangles[UniversalRandom.GetInstance().Next(navMesh.triangles.Length)];
                float v = (float)UniversalRandom.GetInstance().NextDouble();

                //float u = (float) (UniversalRandom.GetInstance().NextDouble() - 1/2f);
                //if (u < 0)
                //    u -= .5f;
                //else
                //    u += 1.5f;

                float u = 0;
                if (UniversalRandom.GetInstance().NextDouble() <= .5)
                    u = .3f * (float)(-UniversalRandom.GetInstance().NextDouble());
                else
                    u = (float)(1 + .3f * UniversalRandom.GetInstance().NextDouble());


                var pos = (t.vertices[0] + u * t.ab + v * t.ac) / terrainScale;
                //var treePos = new Vector3(-halfHeightMapSize + (float)UniversalRandom.GetInstance().NextDouble() * (heightMapSize-50), 0,
                //    -halfHeightMapSize + (float)UniversalRandom.GetInstance().NextDouble() * (heightMapSize-50));


                float X = pos.X + heightMapSize / 2f,
                    Z = pos.Z +heightMapSize / 2f;

                float Xlerp = X % 1f,
                    Zlerp = Z % 1f;

                int x0 = (int)X,
                    z0 = (int)Z,
                    x1 = x0 + 1,
                    z1 = z0 + 1;

                float height;
                float k;
                if (Xlerp + Zlerp > 1)
                {
                    float h1 = MathHelper.Lerp(heightMap[x0, z1], heightMap[x1, z1], Xlerp);
                    float h2 = MathHelper.Lerp(heightMap[x1, z0], heightMap[x1, z1], Zlerp);
                    k = h2 / h1;
                    height = MathHelper.Lerp(h1, h2, .5f);
                }
                else
                {
                    float h1 = MathHelper.Lerp(heightMap[x0, z0], heightMap[x1, z0], Xlerp),
                        h2 = MathHelper.Lerp(heightMap[x0, z0], heightMap[x0, z1], Zlerp);
                    k = h2 / h1;
                    height = MathHelper.Lerp(h1, h2, .5f);
                }
                pos.Y = height - 0.002f;

                if (k > 1.02 ) continue;

                GameObject obj;
                switch(UniversalRandom.GetInstance().Next(0, 3)) 
                {
                case 0:
                    obj = new OakTree(gameInstance);
                    obj.Scale = 3 + 3 * (float)UniversalRandom.GetInstance().NextDouble();
                    FireflyCandidates.Add(obj);
                    break;
                case 1:
                    obj = new BirchTree(gameInstance);
                    obj.Scale = 3 + 3 * (float)UniversalRandom.GetInstance().NextDouble();
                    FireflyCandidates.Add(obj);
                    break;
                default:
                    obj = new Stone(gameInstance);
                    obj.Scale = 0.5f + (float)UniversalRandom.GetInstance().NextDouble();
                    break;
                }

                obj.Position = terrainScale * pos;
                obj.Rotation = new Vector3(0, MathHelper.Lerp(0, MathHelper.Pi * 2, (float)UniversalRandom.GetInstance().NextDouble()), 0);

                GraphicalObjects.Add(obj);
                ShadowCasterObjects.Add(obj);
            }

            for (int i = 0; i < FireflyCandidates.Count; i+=5)
            {
                ParticleEmitter emitter = new ParticleEmitter(fireflySystem, 80, FireflyCandidates[i].Position);
                emitter.Origin = FireflyCandidates[i].Position + Vector3.Up * 500;
                fireflyEmitter.Add(emitter);
            }

            #endregion

            //foreach (GameObject obj in GraphicalObjects)
            //{
            //    pointLights.Add(new PointLight(obj.Position + Vector3.Up * 500, new Vector3(0.7f, 0.7f, 0.7f), 450)
            //    {
            //        Model = pointLightModel
            //    });
            //}
            //GraphicalObjects.AddRange(pointLights);

            //List<FireObject> list = new List<FireObject>();
            //foreach (PointLight p in pointLights)
            //{
            //    FireObject obj = new FireObject(gameInstance, content, p.Position, p.Position + Vector3.Up * 10);
            //    list.Add(obj);
                
            //}
            //pointLights.AddRange(list);
            //GraphicalObjects.AddRange(list);


            #region Animals

            birdModel = gameInstance.Content.Load<Model>(@"Models\bird");
            birdCurve = new BirdCurve();

            #endregion

            #region Cameras

            var input = gameInstance.GetService<InputComponent>();

            gameInstance.GetService<CameraComponent>().AddCamera(new DebugCamera(new Vector3(-11800, 3000, -8200), input));
            Camera c;
            gameInstance.GetService<CameraComponent>().AddCamera(c = new ThirdPersonCamera(Car, input));
            gameInstance.GetService<CameraComponent>().CurrentCamera = c;

            
            #endregion

            #region DynamicEnvironment

            // TODO: CARMOVE
            environmentCubeMap = new RenderTargetCube(this.GraphicsDevice, 256, true, SurfaceFormat.Color, DepthFormat.Depth16);
            Car.EnvironmentMap = skyMap;

            #endregion

            #region PostProcess

            postProcessingComponent = new PostProcessingComponent(Game);
            Game.Components.Add(postProcessingComponent);

            postProcessTexture = new RenderTarget2D(GraphicsDevice,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height,
                true, SurfaceFormat.Color, DepthFormat.Depth24);

            #endregion

            // Adding a prelighingrenderer as a service
            prelightingRenderer = new PrelightingRenderer(GraphicsDevice, content);
            Game.AddService(typeof(PrelightingRenderer), prelightingRenderer);


            #region ShadowMapEffect

            shadowMapEffect = content.Load<Effect>(@"Effects\ShadowMap");

            #endregion

            #region Gameplay Trigger Manager (with sample)

            /// <summary>
            /// Gets the triggermanager
            /// Add new PositionTrigger
            /// Hook up to listener => when hit, use the thunderBoltGenerator and spawn a flash
            /// Adds it to triggers.
            /// </summary>

            //var triggerManager = gameInstance.GetService<TriggerManager>();

            //int noOfCheckpoints = 10;
            //for (int i = 0; i < noOfCheckpoints; i++)
            //{
            //    var trigger = new PositionTrigger(raceTrack.CurveRasterization, (int)(((float)i / noOfCheckpoints) * raceTrack.CurveRasterization.Points.Count), true, true);
            //    string cp = "Checkpoint " + i;
            //    trigger.Triggered += (sender, e) =>
            //    {
            //        Console.WriteLine(cp);
            //    };
            //    triggerManager.Triggers.Add("checkpoint"+i, trigger);
            //}
            
            #endregion

            #region Game Mode
            if (gameInstance.GetService<ServerClient>().connected)
            {
                foreach (var player in gameInstance.GetService<ServerClient>().Players.Values)
                {
                    gameInstance.GetService<CarControlComponent>().AddCar(player, null, this);
                }
                var carList = gameInstance.GetService<CarControlComponent>().Cars.OrderBy(pc => pc.Key.ID).Select(pc => pc.Value).ToList();
                SetCarsAtStart(carList);
            }

            int cp = 6;
            if (gameModeChoice == GameModeChoice.SimpleRace)
                this.mode = new SimpleRaceMode(gameInstance, 2, cp, raceTrack, Car);
            else if (gameModeChoice == GameModeChoice.Multiplayer)
                this.mode = new MultiplayerRaceMode(gameInstance, 2, cp, raceTrack, Car);
            else
                throw new Exception("Stop choosing weird game modes");

            gameInstance.AddService(typeof(GameplayMode), mode);

            #endregion

            
            #region Checkpoint lights
            for (int i=0; i<cp; i++) {
                var point = raceTrack.GetCurveRasterization(cp).Points[i];
            
                var pl = new CheckpointLight(point.Position + 500 * Vector3.Up)
                {
                    Model = pointLightModel
                };
                pointLights.Add(pl);
                GraphicalObjects.Add(pl);

                #region Fire
                int halfnumberoffire = 5;

                for (int o = -halfnumberoffire + 1; o < halfnumberoffire; o++)
                {
                    Vector3 side = Vector3.Cross(Vector3.Normalize(raceTrack.Curve.GetPoint((i) / (float)cp + .001f) - point.Position), Vector3.Up);

                    var fire = new FireObject(content, fireSystem, fireSmokeSystem, point.Position + side * 100 * o - 
                        Vector3.Up * 400 + 
                        Vector3.Up * 650 * (float)Math.Cos(o/(float)halfnumberoffire), Vector3.Up * 10);
                    pointLights.Add(fire);
                    GraphicalObjects.Add(fire);
                }
                #endregion
            }
            #endregion

            #region BackgroundSound
            loopSoundManager.AddNewSound("forestambient");
            #endregion

            init = true;
        }

        public Car MakeCar()
        {
            // Load car effect (10p-light, env-map)
            carEffect = content.Load<Effect>(@"Effects/CarShading");

            Car car = Car.CreateCar(Game); // (content.Load<Model>(@"Models/Cars/porsche"), 13.4631138f);

            // TODO: Directional should be a service.
            car.DirectionalLight = directionalLight;

            // Place car at start.
            SetCarAtStart(car);
            car.Update();
            return car;
        }

        private void SetCarAtStart(Car car)
        {
            Vector3 carPosition = raceTrack.Curve.GetPoint(0.99f);
            Vector3 carHeading = (raceTrack.Curve.GetPoint(.001f) - carPosition);
            car.Position = carPosition;
            car.Rotation = (float)Math.Atan2(carHeading.X, carHeading.Z) - (float)Math.Atan2(0, -1);
        }

        private void SetCarsAtStart(List<Car> cars)
        {
            Vector3 carPosition = raceTrack.Curve.GetPoint(0.99f);
            Vector3 carHeading = (raceTrack.Curve.GetPoint(.001f) - carPosition);
            Vector3 carDistance = Vector3.Cross(Vector3.Normalize(carHeading), Vector3.Up) * 150;
            int middleIndex = (int)(cars.Count / 2); 

            for (int i = 0; i < cars.Count; i++)
            {
                //carPosition += ;
                var car = cars[i];
                car.Position = carPosition + (carDistance * (i - middleIndex));
                car.Rotation = (float)Math.Atan2(carHeading.X, carHeading.Z) - (float)Math.Atan2(0, -1);
            }
            
        }

        /// <summary>
        /// Loads a texture from Content and asign it to the cubemaps face.
        /// </summary>
        /// <param name="cubeMap"></param>
        /// <param name="filepath"></param>
        /// <param name="face"></param>
        private void LoadCubemapFace(TextureCube cubeMap, string filepath, CubeMapFace face)
        {
            Texture2D texture = content.Load<Texture2D>(filepath);
            byte[] data = new byte[texture.Width * texture.Height * 4];
            texture.GetData<byte>(data);
            cubeMap.SetData<byte>(face, data);
        }

        #endregion

        #region Update

        public override GameState UpdateState(GameTime gameTime)
        {
            GameState state = GameState.Gameplay;
            if (gameOverMenu.Enabled)
                state = gameOverMenu.NextState;
			else if (pauseMenu.Enabled) 
				state = pauseMenu.NextState;
				
			if (state == GameState.Gameplay)
				pauseMenu.Enabled = false;
			else if (state == GameState.MainMenu)
				this.ExitGame();

            if (state != GameState.PausedGameplay || state != GameState.GameOver)
                return state;
            else
                return GameState.Gameplay;
        }

        public override void Update(GameTime gameTime)
        {
            this.mode.Update(gameTime, this);

            if (this.mode.GameOver)
                gameOverMenu.Enabled = true;

            InputComponent input = gameInstance.GetService<InputComponent>();
            if (input.GetPressed(Input.Exit))
                pauseMenu.Enabled = !pauseMenu.Enabled;

            var hudComponent = Game.GetService<HUDComponent>();
            var cameraComponent = Game.GetService<CameraComponent>();
            var carControlComponent = Game.GetService<CarControlComponent>();

            carControlComponent.Enabled = mode.GameStarted;

            if (pauseMenu.Enabled || gameOverMenu.Enabled)
            {
                if (cameraComponent.Enabled)
                    cameraComponent.Enabled = false;
                hudComponent.Visible = false;

                //GameState state = pauseMenu.UpdateState(gameTime);

                //if (state == GameState.Gameplay)
                //    Paused = false;
                //else if (state != GameState.PausedGameplay)
                //{
                //    // Exit
                //    Game.Exit();
                //    return;
                //}
            }
            else
            {
                if (!cameraComponent.Enabled)
                    cameraComponent.Enabled = true;
                hudComponent.Visible = true;

                /*if (input.GetPressed(Input.ChangeController))
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
                }*/

                if (input.GetPressed(Input.Console))
                    gameInstance.GetService<HUDConsoleComponent>().Toggle();

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
                        Car.Speed *= 0.8f;
                    }
                }

                #endregion

                #region View frustum
                // Update the view frustum
                Matrix view = gameInstance.GetService<CameraComponent>().View;
                viewFrustum = new BoundingFrustum(view * projectionMatrix);
                #endregion
 
                foreach (GameObject obj in GraphicalObjects)
                {
                    if (obj.BoundingSphere.Intersects(viewFrustum))
                        obj.Update(gameTime);
                }


            }

            #region Audio
            // Spawn random audios
            if (gameTime.TotalGameTime.Milliseconds % 7000 < 10 && UniversalRandom.GetInstance().NextDouble() > 0.5)
            {
                AudioEngineManager.PlaySound("randomambient");
            }
            #endregion

            #region Particle emission
            // Particles should continue to spawn regardless of the pause state
            
            // Rain system
            for (int x = -4; x < 4; x++)
            {
                for (int z = -4; z < 4; z++)
                {
                    rainSystem.AddParticle(Car.Position + new Vector3(
                        (float)UniversalRandom.GetInstance().NextDouble() * x * 150,
                        500 * (float)UniversalRandom.GetInstance().NextDouble(),
                        (float)UniversalRandom.GetInstance().NextDouble() * z * 150),
                        new Vector3(-1, -1, -1));//Vector3.Down);
                }
            }

            
            // Smoke emission on part of track
            

            smokeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (smokeTime > 0.35) {
                CurveRasterization cr = new CurveRasterization(raceTrack.Curve, 150);
                for(int i = 25 ; i < 55; i += 2){
                    smokeSystem.AddParticle(cr.Points[i].Position +
                        new Vector3(
                            (-1f + 2 * (float)UniversalRandom.GetInstance().NextDouble()) * 1100,
                            450 * (-1f + 2 * (float)UniversalRandom.GetInstance().NextDouble()),
                            (float)UniversalRandom.GetInstance().NextDouble() * 1100),
                            Vector3.Down); 

                }
                smokeTime = 0;
            }

            
            //if (smokeTime > 0.2)
            //{
            //    smokeSystem.AddParticle(Car.Position + Car.Heading * 500 +
            //        new Vector3(
            //            (-1f + 2 * (float)UniversalRandom.GetInstance().NextDouble()) * 500,
            //            500 * (-1f + 2 * (float)UniversalRandom.GetInstance().NextDouble()),
            //            (float)UniversalRandom.GetInstance().NextDouble() * 500),
            //            Vector3.Up);
            //    smokeTime = 0;
            //}

            foreach (ParticleEmitter emitter in fireflyEmitter)
            {
                emitter.Update(gameTime, emitter.Origin +
                    new Vector3(
                        (-1f + 2 * (float)UniversalRandom.GetInstance().NextDouble()) * 200,
                        (-1f + 2 * (float)UniversalRandom.GetInstance().NextDouble()) * 200,
                        (-1f + 2 * (float)UniversalRandom.GetInstance().NextDouble()) * 200));
            }

            if (Car.Speed > 10)
            {
                dustEmitter.Update(gameTime, dustEmitter.Origin +
                    new Vector3(
                            (-1f + 2 * (float)UniversalRandom.GetInstance().NextDouble()) * 40,
                            0,
                            (-1f + 2 * (float)UniversalRandom.GetInstance().NextDouble()) * 40));
                dustEmitter.Origin = Car.Position;
            }
            #endregion

            #region Countdown Lights

            // Update all lights to represent the countdown state. Red - Orange - Yellow - Green
            if (countdownTimeGo.TotalSeconds < 1)
            {
                foreach (PointLight light in pointLights)
                {
                    light.Diffuse = countdownColors[mode.CountDownState].ToVector3();

                }
                if (mode.CountDownState == 3)
                {
                    countdownTimeGo += gameTime.ElapsedGameTime;
                }
            }

            #endregion

            //Vector3 originalDiffuse = new Vector3(.46f, .33f, .75f);
            Vector3 newDiffuse = new Vector3(.65f, .4f, .7f);
            Vector3 originalDiffuse = new Vector3(.7f, .6f, .8f);
            
            //float amount = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2) + 1) * 0.5f * 0.4f;
            float amount = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2));
            directionalLight.DefaultDiffuse = new Vector3(
                MathHelper.Lerp(originalDiffuse.X, newDiffuse.X, amount),
                MathHelper.Lerp(originalDiffuse.Y, newDiffuse.Y, amount),
                MathHelper.Lerp(originalDiffuse.Z, newDiffuse.Z, amount));

            //directionalLight.Direction = Vector3.Transform(
            //    directionalLight.Direction,
            //    Matrix.CreateRotationY(
            //    (float)gameTime.ElapsedGameTime.TotalSeconds));

            //Vector3 pointLightOffset = new Vector3(0, 250, 0), rotationAxis = new Vector3(0,-100,0);
            //int index = 0;
            //foreach (var point in raceTrack.CurveRasterization.Points)
            //{
            //    pointLights[index++].Position = terrainScale * point.Position + pointLightOffset +
            //        Vector3.Transform(rotationAxis, Matrix.CreateFromAxisAngle(point.Heading, 7 * (float)gameTime.TotalGameTime.TotalSeconds));
            //}

            //yellowSystem.AddParticle(new Vector3(-200, 1500, 2000), Vector3.Up);
            //redSystem.AddParticle(new Vector3(1500, 1500, 2000), Vector3.Up);
            //plasmaSystem.AddParticle(new Vector3(-200, 1500, 4000), Vector3.Up);
            //greenSystem.AddParticle(new Vector3(1500, 1500, 4000), Vector3.Up);

            //airParticles.AddParticle(Car.Position + Car.Forward * (float)UniversalRandom.GetInstance().NextDouble() * 5000 +
            //    new Vector3(
            //        (-1f + 2 * (float)UniversalRandom.GetInstance().NextDouble()) * 500,
            //        500 * (-1f + 2 * (float)UniversalRandom.GetInstance().NextDouble()),
            //        (float)UniversalRandom.GetInstance().NextDouble() * 500),
            //        Vector3.Up);
   
            base.Update(gameTime);
        }

        private void ExitGame()
        {
            // Unload components
            var components = gameInstance.Components;
            var services = gameInstance.Services;

            content.Unload();

            foreach (Type service in Game.GetAddedServices())
            {
                services.RemoveService(service);
            }
            
            components.Clear();
            foreach (var component in gameInstance.BaseComponents)
            {
                components.Add(component);
                gameInstance.SetService(component.GetType(), component);
            }

            loopSoundManager.StopAllSounds();
            
            (components.First(c => c is CameraComponent) as GameComponent).Enabled = true;            

        }

        private bool CollisionCheck(NavMeshTriangle triangle)
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
        /// Renders this game!
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            OverlayView overlay = null;
            Texture2D overlayTexture = null;
            if (gameOverMenu.Enabled)
                overlay = gameOverMenu;
            else if (pauseMenu.Enabled)
                overlay = pauseMenu;
            if (overlay != null)
                 overlayTexture = overlay.Render();

            //
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            gameInstance.GraphicsDevice.Clear(Color.CornflowerBlue);
            
            skyBoxEffect.Parameters["ElapsedTime"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);

            Matrix view = gameInstance.GetService<CameraComponent>().View;

            #region ShadowMap

            if (shadowMapNotRendered)
            {
                shadowMapNotRendered = false;

                for (int z = 0; z < terrainSegmentsCount; z++)
                {
                    for (int x = 0; x < terrainSegmentsCount; x++)
                    {
                        var terrain = terrainSegments[x, z];

                        //if (!viewFrustum.Intersects(terrain.BoundingBox))
                        //    continue;

                        GraphicsDevice.SetRenderTarget(terrain.ShadowMap);
                        GraphicsDevice.Clear(Color.Black);

                        var shadowBox = BoundingBox.CreateFromPoints(
                            terrain.BoundingBox.GetCorners().Select(corner =>
                                corner + new Ray(corner,
                                    directionalLight.Direction).Intersects(zeroPlane).Value *
                                    directionalLight.Direction));
                        
                        var startpoint = shadowBox.Min;
                        var endpoint = shadowBox.Max;

                        float projectionWidth = endpoint.Z - startpoint.Z,
                              projectionHeight = endpoint.X - startpoint.X,
                              projectionNear = .50f,
                              projectionFar = 50000;
                        var lookAtOffset = projectionNear + (projectionFar - projectionNear) / 2f;

                        var shadowmMapLookAtTarget = Vector3.Lerp(startpoint, endpoint, .5f);


                        terrain.ShadowMapView = Matrix.CreateLookAt(
                            shadowmMapLookAtTarget - lookAtOffset * directionalLight.Direction,
                            shadowmMapLookAtTarget, Vector3.Up);

                        var xzlight = directionalLight.Direction.GetXZProjection(true);
                        float dot = Vector3.Dot(directionalLight.Direction, xzlight);
                        float sw = 1 / (float)Math.Sin(Math.Acos(dot));
                        var projectionTranform = Matrix.CreateScale(new Vector3(1, sw, 1));

                        projectionTranform *= Matrix.CreateRotationZ((float)(-Math.Atan2(xzlight.Z, xzlight.X)));

                        //projectionTranform = Matrix.Identity;

                        terrain.ShadowMapProjection = projectionTranform * Matrix.CreateOrthographic(
                            projectionWidth, projectionHeight, projectionNear, projectionFar);

                        RenderShadowCasters(terrain.BoundingBox, terrain.ShadowMapView, terrain.ShadowMapProjection);
                    }
                }

                GraphicsDevice.SetRenderTarget(null); 
            }

            #endregion

            #region Prelight Rendering

            // Render to its rendertargets independently
            prelightingRenderer.Render(view, terrainSegments, terrainSegmentsCount, pointLights, Car, GraphicalObjects);

            #endregion

            //if (!GameSettings.Default.PerformanceMode)
            //RenderEnvironmentMap(gameTime);

            #region START RENDER SCENE!!! (to post processing target)

            GraphicsDevice.SetRenderTarget(postProcessTexture);

            // Reset render settings
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            GraphicsDevice.Clear(Color.White);

            RenderScene(gameTime, view, projectionMatrix, false);

            #endregion

            postProcessingComponent.RenderedImage = postProcessTexture;

            overlayComponent.Overlay = overlay;
            overlayComponent.OverlayTexture = overlayTexture;
            

            base.Draw(gameTime);
            if (init)
            {
                Initialized = true;
                init = false;
            }
        }

        
        
        private void RenderShadowCasters(BoundingBox boundingBox, Matrix shadowMapView, Matrix shadowMapProjection)
        {
            //Matrix[] transforms = new Matrix[oakTree.Bones.Count];
            //oakTree.CopyAbsoluteBoneTransformsTo(transforms);

            // Only one mesh
            //var mesh = oakTree.Meshes[0];

            // Backup
            //Effect[] oldEffects = new Effect[2];

            // Set shadowmapeffect
            //for (int i = 0; i < mesh.MeshParts.Count; i++)
            //{
            //    oldEffects[i] = mesh.MeshParts[i].Effect;
            //    mesh.MeshParts[i].Effect = shadowMapEffect;
            //}

            

            //shadowMapEffect.Parameters["View"].SetValue(shadowMapView);
            //shadowMapEffect.Parameters["Projection"].SetValue(shadowMapProjection);
            shadowMapEffect.Parameters["AlphaEnabled"].SetValue(false);

            GraphicsDevice.BlendState = BlendState.Opaque;


            for (int z = 0; z < terrainSegmentsCount; z++)
                for (int x = 0; x < terrainSegmentsCount; x++)
                {
                    var terrain = terrainSegments[x, z];
                    terrain.Effect = shadowMapEffect;
                    terrain.Draw(shadowMapView, shadowMapProjection);
                    terrain.Effect = terrainEffect;
                }

            foreach (var gameObject in ShadowCasterObjects)
            {
                //if (boundingBox.Intersects(gameObject.BoundingSphere))
                //{


                    //foreach (var meshpart in mesh.MeshParts)
                    //{
                    //    //Effect shadowMapEffectClone = shadowMapEffect.Clone();
                    //    if (meshpart.Effect.Parameters["AlphaMap"].GetValueTexture2D() != null)
                    //    {
                    //        shadowMapEffect.Parameters["AlphaMap"].SetValue(meshpart.Effect.Parameters["AlphaMap"].GetValueTexture2D());
                    //        shadowMapEffect.Parameters["AlphaEnabled"].SetValue(true);
                    //    }
                    //    else
                    //        shadowMapEffect.Parameters["AlphaEnabled"].SetValue(false);

                    //    oldEffects.Add(meshpart, meshpart.Effect);
                    //    meshpart.Effect = shadowMapEffect;
                    //}
                

                
                    gameObject.DrawShadowCaster(GraphicsDevice, shadowMapEffect, shadowMapView, shadowMapProjection);
                
            }
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
                    viewMatrix = Matrix.CreateLookAt(Car.Position, Car.Position + Vector3.Forward, Vector3.Up);
                else if (cubeMapFace == CubeMapFace.PositiveX)
                    viewMatrix = Matrix.CreateLookAt(Car.Position, Car.Position + Vector3.Right, Vector3.Up);
                else if (cubeMapFace == CubeMapFace.PositiveY)
                    viewMatrix = Matrix.CreateLookAt(Car.Position, Car.Position + Vector3.Up, Vector3.Backward);
                else if (cubeMapFace == CubeMapFace.NegativeZ)
                    viewMatrix = Matrix.CreateLookAt(Car.Position, Car.Position + Vector3.Backward, Vector3.Up);
                else
                    viewMatrix = Matrix.Identity;

                GraphicsDevice.SetRenderTarget(environmentCubeMap, cubeMapFace);
                GraphicsDevice.Clear(Color.White);

                //Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                    //1.0f, 100f, 5000f);
                RenderScene(gameTime, viewMatrix, projectionMatrix, true);
            }

            // Default target
            GraphicsDevice.SetRenderTarget(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="view"></param>
        /// <param name="projection"></param>
        /// <param name="environment">If this renderpass is render to our dynamic environmentmap</param>
        private void RenderScene(GameTime gameTime, Matrix view, Matrix projection, bool environment)
        {
            BoundingFrustum viewFrustum = new BoundingFrustum(Game.GetService<CameraComponent>().CurrentCamera.View * projection);

            // TODO: CARMOVE
            //Matrix[] transforms = new Matrix[Car.Model.Bones.Count];
            //Car.Model.CopyAbsoluteBoneTransformsTo(transforms);

            GraphicsDevice.BlendState = BlendState.Opaque;

            #region SkyBox

            skyBoxEffect.Parameters["View"].SetValue(view);
            skyBoxEffect.Parameters["Projection"].SetValue(projection);
            
            //skyBoxModel.Meshes[0].Draw();
            if (Keyboard.GetState().IsKeyUp(Keys.O))
            {
                skyBoxModel.Meshes[0].Draw();
            }


            #endregion

            for (int z = 0; z < terrainSegmentsCount; z++)
                for (int x = 0; x < terrainSegmentsCount; x++)
                {
                    var terrain = terrainSegments[x, z];
                    if (viewFrustum.Intersects(terrain.BoundingBox))
                    {
                        if (environment) {
                            Vector3 boxStart = Car.Position;
                            boxStart.Y = -5000;
                            Vector3 boxEnd = boxStart;
                            boxEnd.Y = 5000;
                            boxEnd.X += 50;
                            boxEnd.Z += 50;
                            if (terrain.BoundingBox.Intersects(new BoundingBox(boxStart, boxEnd)))
                                continue;
                        }

                        terrain.Draw(view, projection, gameInstance.GetService<CameraComponent>().Position,
                            directionalLight);
                    }
                }

            //navMesh.Draw(view, projection);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            
            // Set view to particlesystems
            foreach (ParticleSystem pSystem in particleSystems)
                pSystem.SetCamera(view, projection);

            #region RenderObjects

            foreach (GameObject obj in GraphicalObjects)
            {
                obj.Draw(view, projection);
            }

            #endregion

            #region Animals

            {
                var t = ((float)gameTime.TotalGameTime.TotalSeconds / 3f) % 1;
                var position = birdCurve.GetPoint(t);
                var heading = Vector3.Normalize(birdCurve.GetPoint(t + (t > .5 ? -.01f : .01f)) - position);
                if (t > .5)
                    heading *= -1;

                birdModel.Draw(Matrix.CreateScale(1) *
                    Vector3.Forward.GetRotationMatrix(heading) *
                    Matrix.CreateTranslation(position),
                    view, projection);
            }

            #endregion

            //int i = 0;
            //foreach (var GraphicalObject in GraphicalObjects)
            //{
            //    ParticleSystem s = null;
            //    switch (i++ % 4)
            //    {
            //        case 0:
            //            s = yellowSystem;
            //            break;
            //        case 1:
            //            s = redSystem;
            //            break;
            //        case 2:
            //            s = greenSystem;
            //            break;
            //        case 3:
            //            s = plasmaSystem;
            //            break;
            //    }
            //    BoundingBox.CreateFromSphere(GraphicalObject.BoundingSphere).IlluminateBoundingBox(s);
            //}

            if (!environment)
            {
                foreach (Car car in gameInstance.GetService<CarControlComponent>().Cars.Values)
                    DrawCar(view, projection, car);
                DrawGhostCar(view, projection, gameTime);
            }

        }

        private void DrawModel(Model model, Matrix view, Matrix projection, Matrix transform)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    Matrix world = transforms[mesh.ParentBone.Index] * transform;
                    Matrix normalMatrix = Matrix.Invert(Matrix.Transpose(world));

                    //effect.Parameters["NormalMatrix"].SetValue(normalMatrix);
                    effect.Parameters["World"].SetValue(world);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);

                    //effect.Parameters["DirectionalDirection"].SetValue(directionalLight.Direction);
                    //effect.Parameters["DirectionalDiffuse"].SetValue(directionalLight.Diffuse);
                    //effect.Parameters["DirectionalAmbient"].SetValue(directionalLight.Ambient);
                }
                mesh.Draw();
            }
        }

        #region Car drawing

        private void DrawCar(Matrix view, Matrix projection, Car car)
        {
            // TODO: CARMOVE

            car.View = view;
            car.Projection = projection;
            car.Draw(null);

            //foreach (var mesh in car.Model.Meshes) // 5 meshes
            //{
            //    Matrix world = Matrix.Identity;

            //    // Wheel transformation
            //    if ((int)mesh.Tag > 0)
            //    {
            //        world *= Matrix.CreateRotationX(car.WheelRotationX);
            //        if ((int)mesh.Tag > 1)
            //            world *= Matrix.CreateRotationY(car.WheelRotationY);
            //    }

            //    // Local modelspace
            //    world *= mesh.ParentBone.Transform;

            //    // World
            //    world *= car.RotationMatrix * car.TranslationMatrix;

            //    foreach (Effect effect in mesh.Effects) // 5 effects for main, 1 for each wheel
            //    {
            //        EffectParameterCollection param = effect.Parameters;

            //        if (mesh.Name.Equals("main"))
            //        {
            //            param["MaterialReflection"].SetValue(.9f);
            //            param["MaterialShininess"].SetValue(10); 
            //        }

            //        param["World"].SetValue(world);
            //        param["View"].SetValue(view);
            //        param["Projection"].SetValue(projection);

            //        if (mesh.Name.Equals("main"))
            //        {

            //            param["NormalMatrix"].SetValue(Matrix.Invert(Matrix.Transpose(world)));

            //            param["EyePosition"].SetValue(Game.GetService<CameraComponent>().Position);

            //            if (mesh.Name == "main")
            //            {
            //                param["MaterialDiffuse"].SetValue(GameSettings.Default.CarColor.ToVector3());
            //                param["MaterialAmbient"].SetValue(GameSettings.Default.CarColor.ToVector3());
            //            }

            //            param["DirectionalLightDirection"].SetValue(directionalLight.Direction);
            //            param["DirectionalLightDiffuse"].SetValue(directionalLight.Diffuse);
            //            param["DirectionalLightAmbient"].SetValue(directionalLight.Ambient);
            //        }
            //    }

            //    mesh.Draw();
            //}

        }

        float ghostWheelRotation = 0;
        Vector3 ghostPosition;

        private void DrawGhostCar(Matrix view, Matrix projection, GameTime gameTime)
        {
            //return;
            //float t = ((float)gameTime.TotalGameTime.TotalSeconds / 256f) % 1f;

            //foreach (var mesh in Car.Model.Meshes) // 5 meshes
            //{
            //    Matrix world = Matrix.Identity;

            //    // Wheel transformation
            //    if ((int)mesh.Tag > 0)
            //    {
            //        world *= Matrix.CreateRotationX(ghostWheelRotation);
            //    }

            //    // Local modelspace
            //    world *= mesh.ParentBone.Transform;

            //    // World
            //    Vector3 position = raceTrack.Curve.GetPoint(t);
            //    Vector3 heading = raceTrack.Curve.GetPoint((t + .01f) % 1f) - position;
            //    heading.Normalize();

            //    Vector3 normal = Vector3.Up;

            //    foreach (var triangle in navMesh.triangles)
            //    {
            //        var downray = new Ray(position, Vector3.Down);
            //        var upray = new Ray(position, Vector3.Up);

            //        float? d1 = downray.Intersects(triangle.trianglePlane),
            //            d2 = upray.Intersects(triangle.trianglePlane);

            //        if (d1.HasValue || d2.HasValue)
            //        {
            //            float d = d1.HasValue ? d1.Value : d2.Value;
            //            Ray ray = d1.HasValue ? downray : upray;

            //            var point = ray.Position + d * ray.Direction;

            //            bool onTriangle = PointInTriangle(triangle.vertices[0],
            //                triangle.vertices[1],
            //                triangle.vertices[2],
            //                point);

            //            if (onTriangle)
            //            {
            //                position = point;
            //                normal = triangle.normal;
            //                break;
            //            }
            //        }
            //    }

            //    ghostWheelRotation -= (position - ghostPosition).Length() / 10.4725f;
            //    ghostPosition = position;

            //    world *= Vector3.Forward.GetRotationMatrix(heading) * Vector3.Up.GetRotationMatrix(normal) *
            //        Matrix.CreateTranslation(position);

            //    carSettings.World = world;
            //    carSettings.NormalMatrix = Matrix.Invert(Matrix.Transpose(world));

            //    foreach (Effect effect in mesh.Effects) // 5 effects for main, 1 for each wheel
            //        effect.SetCarShadingParameters(carSettings);

            //    mesh.Draw();
            //}

        }

        #endregion

        #endregion


    }
}
