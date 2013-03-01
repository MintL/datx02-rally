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
using datx02_rally.Menus;

namespace datx02_rally
{
    public enum GameState { None, MainMenu, OptionsMenu, Gameplay, PausedGameplay, MultiplayerMenu, SingleplayerMenu, Exiting };
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public GameStateView currentView { get; private set; }
        public GameState currentState { get; private set; }
        private static Game1 Instance = null;
        public GraphicsDeviceManager Graphics { get; private set; }
        // shared sprite batch
        public SpriteBatch spriteBatch;

        #region Initialization

        public Game1()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Instance = this;

            Graphics.PreferredBackBufferWidth = GameSettings.Default.ResolutionWidth;
            Graphics.PreferredBackBufferHeight = GameSettings.Default.ResolutionHeight;

            //if (GameSettings.Default.FullScreen != Graphics.IsFullScreen)
            //    Graphics.ToggleFullScreen();

            IsMouseVisible = true;
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

            var serverComponent = new ServerClient(this);
            Components.Add(serverComponent);
            Services.AddService(typeof(ServerClient), serverComponent);

            var inputComponent = new InputComponent(this);
            //inputComponent.CurrentController = Controller.GamePad;
            Components.Add(inputComponent);
            Services.AddService(typeof(InputComponent), inputComponent);

            var consoleComponent = new HUDConsoleComponent(this);
            Components.Add(consoleComponent);
            Services.AddService(typeof(HUDConsoleComponent), consoleComponent);

            currentView = new MainMenu(this);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            base.LoadContent();
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
            GameState nextState = currentView.UpdateState(gameTime);
            if (currentState != nextState) 
            {
                switch (nextState)
                {
                    case GameState.None:
                        break;
                    case GameState.MainMenu:
                        currentView = new MainMenu(this);
                        break;
                    case GameState.OptionsMenu:
                        currentView = new OptionsMenu(this);
                        break;
                    case GameState.Gameplay:
                        currentView = new GamePlayView(this, null, GamePlayMode.Singleplayer);
                        //
                        break;
                    case GameState.PausedGameplay:
                        break;
                    case GameState.MultiplayerMenu:
                        this.GetService<ServerClient>().Connect(System.Net.IPAddress.Loopback);
                        currentView = new GamePlayView(this, 0, GamePlayMode.Multiplayer);//MultiplayerMenu(this);
                        break;
                    case GameState.SingleplayerMenu:
                        break;
                    case GameState.Exiting:
                        this.Exit();
                        break;
                    default:
                        break;
                }
                currentState = nextState;
            }
            base.Update(gameTime);
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
            currentView.Draw(gameTime);
            base.Draw(gameTime);
        }

        #endregion
    }
}
