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

namespace datx02_rally
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        KeyboardState lastKeyboardState;

        Texture2D particleBase;
        Texture2D smokeTexture;
        Emitter fire;
        Emitter bluePulse;
        Emitter blueSparkle;
        Emitter redSparkle;
        Emitter smoke;
        int followMouse; // what emitter follows mouse

        float nextLightning;

        Random random;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();

            IsMouseVisible = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            random = new Random();
            followMouse = 0;
            nextLightning = MathHelper.Lerp(3.0f, 10.0f, (float)random.NextDouble());

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

            particleBase = Content.Load<Texture2D>(@"Textures/ParticleBase1");
            smokeTexture = Content.Load<Texture2D>(@"Textures/Smoke");
            fire = new Emitter(new Vector2(500, 400), particleBase, 1000, 0.01f, random,
                new Vector2(0, -1), new Vector2(0.01f * MathHelper.Pi, 0.01f * -MathHelper.Pi),
                new Vector2(0.3f, 1.3f), new Vector2(0.3f, 0.5f),
                new Color(250, 200, 0), new Color(250, 140, 140),
                new Color(255, 165, 0, 0), new Color(220, 20, 60, 0),
                new Vector2(400, 500), new Vector2(100, 120), new Vector2(0.5f, 0.75f), Vector2.Zero);

            bluePulse = new Emitter(new Vector2(500, 400), particleBase, 1000, 0.01f, random,
                new Vector2(0, -1), new Vector2(1.0f * MathHelper.Pi, 1.0f * -MathHelper.Pi),
                new Vector2(0.6f, 1.3f), new Vector2(0.8f, 1.2f),
                new Color(99, 255, 255), new Color(148, 255, 255),
                new Color(27, 69, 129, 0), new Color(113, 225, 225, 0),
                new Vector2(400, 500), new Vector2(100, 120), new Vector2(0.4f, 0.5f), Vector2.Zero);

            blueSparkle = new Emitter(new Vector2(900, 200), particleBase, 1000, 0.01f, random,
                new Vector2(-1, 1), new Vector2(0.05f * MathHelper.Pi, 0.05f * -MathHelper.Pi),
                new Vector2(0.1f, 0.15f), new Vector2(0.08f, 0.1f),
                new Color(99, 255, 255), new Color(148, 255, 255),
                new Color(99, 255, 255, 100), new Color(148, 255, 255, 100),
                new Vector2(700, 800), new Vector2(700, 800), new Vector2(0.3f, 0.5f), Vector2.Zero);

            redSparkle = new Emitter(new Vector2(700, 200), particleBase, 1000, 0.01f, random,
                new Vector2(1, 1), new Vector2(0.05f * MathHelper.Pi, 0.05f * -MathHelper.Pi),
                new Vector2(0.1f, 0.15f), new Vector2(0.08f, 0.1f),
                new Color(250, 200, 0), new Color(250, 140, 140),
                new Color(255, 165, 0, 0), new Color(220, 20, 60, 0),
                new Vector2(700, 800), new Vector2(700, 800), new Vector2(0.3f, 0.5f), Vector2.Zero);

            smoke = new Emitter(new Vector2(100, 600), smokeTexture, 1000, 0.05f, random,
                new Vector2(0, -1), new Vector2(0.1f * MathHelper.Pi, 0.1f * -MathHelper.Pi),
                new Vector2(0.8f, 1.3f), new Vector2(0.3f, 0.5f),
                new Color(250, 255, 255), new Color(250, 250, 250),
                new Color(255, 255, 255, 0), new Color(250, 250, 250, 100),
                new Vector2(50, 100), new Vector2(40, 80), new Vector2(2.0f, 2.5f), new Vector2(-5f, 5f));
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
            KeyboardState keyboardState = Keyboard.GetState();
            
            
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            if (keyboardState.IsKeyDown(Keys.Space) && lastKeyboardState.IsKeyUp(Keys.Space))
            {
                followMouse = -1;
            }
            if (keyboardState.IsKeyDown(Keys.D1))
                followMouse = 0;
            else if (keyboardState.IsKeyDown(Keys.D2))
                followMouse = 1;
            else if (keyboardState.IsKeyDown(Keys.D3))
                followMouse = 2;
            else if (keyboardState.IsKeyDown(Keys.D4))
                followMouse = 3;

            lastKeyboardState = keyboardState;

            if (followMouse == 0)
            {
                fire.Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            }
            else if (followMouse == 1)
            {
                bluePulse.Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            }
            else if (followMouse == 2)
            {
                blueSparkle.Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            }
            else if (followMouse == 3)
            {
                redSparkle.Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            }

            bluePulse.Update(gameTime.ElapsedGameTime.Milliseconds / 1000f);
            fire.Update(gameTime.ElapsedGameTime.Milliseconds / 1000f);
            blueSparkle.Update(gameTime.ElapsedGameTime.Milliseconds / 1000f);
            redSparkle.Update(gameTime.ElapsedGameTime.Milliseconds / 1000f);
            smoke.Update(gameTime.ElapsedGameTime.Milliseconds / 1000f);

            nextLightning -= gameTime.ElapsedGameTime.Milliseconds / 1000f;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            /*if (nextLightning > 0 && nextLightning <= 0.2)
            {
                GraphicsDevice.Clear(Color.White);
                
            }
            else if (nextLightning <= 0)
            {
                nextLightning = MathHelper.Lerp(3.0f, 10.0f, (float)random.NextDouble());
            }
            else
            {*/
                GraphicsDevice.Clear(Color.Black);
            //}

            spriteBatch.Begin();
            //smoke.Draw(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Additive);
            bluePulse.Draw(spriteBatch);
            fire.Draw(spriteBatch);
            blueSparkle.Draw(spriteBatch);
            redSparkle.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
