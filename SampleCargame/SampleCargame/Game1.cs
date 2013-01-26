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

namespace SampleCargame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        // A car
        Car car;

        // ... in a city.
        Model city;

        // Camera
        Vector3 camPos, camLookAt;
        Vector2 camRot = Vector2.Zero;
        Matrix view, projection;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Full HD
            graphics.PreferredBackBufferHeight = 1080;
            graphics.PreferredBackBufferWidth = 1960;

            //graphics.ToggleFullScreen();

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

            camRot = new Vector2(0, (float)MathHelper.PiOver4/2);
            camPos = Vector3.Zero;
            camLookAt = Vector3.Zero;

            view = Matrix.CreateLookAt(
                camPos,
                camLookAt,
                Vector3.Up);

            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                graphics.GraphicsDevice.Viewport.AspectRatio,
                0.1f,
                10000.0f);

            // (model, distance between axis taken from modelling package for steering physics.
            car = new Car(Content.Load<Model>(@"Models/porsche"), 10.4725f);
            
            city = Content.Load<Model>(@"Models/city");
            // Our city is only one mesh, with one basiceffect, set projection and enable default light shading.
            BasicEffect cityeffect = city.Meshes[0].Effects[0] as BasicEffect;
            cityeffect.EnableDefaultLighting();
            cityeffect.Projection = projection;
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

            #region Camera control

            camRot += new Vector2(0.05f * (
                (Keyboard.GetState().IsKeyDown(Keys.Right) ? 1 : 0) -
                (Keyboard.GetState().IsKeyDown(Keys.Left) ? 1 : 0)),
                0.05f * (
                (Keyboard.GetState().IsKeyDown(Keys.Up) ? 1 : 0) -
                (Keyboard.GetState().IsKeyDown(Keys.Down) ? 1 : 0)));

            //GamePad
            camRot += 0.05f * GamePad.GetState(PlayerIndex.One).ThumbSticks.Right;
            //Clamp
            camRot.Y = MathHelper.Clamp(camRot.Y, 0.05f, MathHelper.PiOver2 - 0.05f);

            while (Math.Abs(camRot.X - car.Rotation) > MathHelper.Pi)
            {
                if (camRot.X > car.Rotation)
                    camRot.X -= MathHelper.TwoPi;
                else
                    camRot.X += MathHelper.TwoPi;
            }
            camRot.X = MathHelper.Clamp(camRot.X, car.Rotation - MathHelper.PiOver4, car.Rotation + MathHelper.PiOver4);
            //Fade back camera.
            if (Math.Abs(camRot.X - car.Rotation) > MathHelper.Pi / 360)
                camRot.X += (car.Rotation - camRot.X) / 10;

            #endregion

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
            if (Math.Abs(car.WheelRotationY) > MathHelper.Pi / 360)
                car.WheelRotationY *= .9f;

            //Apply changes to car
            car.Update();

            //Friktion if is not driving
            float friction = .97f; // 0.995f;
            if (!Keyboard.GetState().IsKeyDown(Keys.W) ||
                !GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.RightTrigger) && GamePad.GetState(PlayerIndex.One).IsConnected)
                car.Speed *= friction;

            #endregion

            //Apply changes to camera
            camLookAt = car.Position + 5 * Vector3.Up;
            float zoom = 250;
            camPos = car.Position + Vector3.Transform(Vector3.UnitZ * zoom, Matrix.CreateRotationX(-camRot.Y) *
                Matrix.CreateRotationY(camRot.X));
            // Update viewmatrix
            view = Matrix.CreateLookAt(camPos,
                camLookAt, Vector3.Up);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightBlue);

            //Draw city
            var cityMesh = city.Meshes[0];
            (cityMesh.Effects[0] as BasicEffect).View = view;
            cityMesh.Draw();
            
            // Draw car
            foreach (var mesh in car.Model.Meshes) // 5 meshes
            {
                foreach (BasicEffect effect in mesh.Effects) // 5 effects for main, 1 for each wheel
                {
                    effect.EnableDefaultLighting();
                    effect.World = Matrix.Identity;
                    // If this mesh is a wheel, apply rotation
                    if (mesh.Name.StartsWith("wheel"))
                    {
                        effect.World *= Matrix.CreateRotationX(car.WheelRotationX);

                        if (mesh.Name.EndsWith("001") || mesh.Name.EndsWith("002"))
                            effect.World *= Matrix.CreateRotationY(car.WheelRotationY);
                    }
                    // Local morldspace, due to bad .X-file/exporter
                    effect.World *= car.Model.Bones[1 + car.Model.Meshes.IndexOf(mesh) * 2].Transform;
                    effect.World *= Matrix.CreateRotationY(car.Rotation) *
                        Matrix.CreateTranslation(car.Position);
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }

            base.Draw(gameTime);
        }

    }
}
