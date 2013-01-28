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

        Model model;
        Vector3 modelPosition = Vector3.Zero;
        float modelRotation = 0.0f;
        float lightRotation;
        Vector3 cameraPosition = new Vector3(0.0f, 100.0f, 300.0f);
        Vector3 cameraTarget = new Vector3(0.0f, 50.0f, 0.0f);
        float aspectRatio;

        Vector3 lightPosition;

        Effect effect;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();

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

            
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }
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

            

            // Model specific parameters
            //effect.Parameters["MaterialAmbient"].SetValue(Color.White.ToVector3() * 0.5f);
            //effect.Parameters["MaterialDiffuse"].SetValue(Color.DarkGreen.ToVector3() * 0.9f);
            //effect.Parameters["MaterialSpecular"].SetValue(Color.White.ToVector3() * 0.2f);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    currentEffect.Parameters["MaterialShininess"].SetValue(10.0f);

                    Matrix worldMatrix = transforms[mesh.ParentBone.Index] *
                                Matrix.CreateRotationX(modelRotation) *
                                Matrix.CreateTranslation(modelPosition);
                    Matrix viewMatrix = Matrix.CreateLookAt(cameraPosition,
                        cameraTarget, Vector3.Up);
                    Matrix projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
                    Matrix normalMatrix = Matrix.Invert(Matrix.Transpose(viewMatrix * worldMatrix));
                    
                    currentEffect.Parameters["World"].SetValue(worldMatrix);
                    currentEffect.Parameters["View"].SetValue(viewMatrix);
                    currentEffect.Parameters["Projection"].SetValue(projectionMatrix);
                    currentEffect.Parameters["NormalMatrix"].SetValue(normalMatrix);
                    currentEffect.Parameters["LightPosition"].SetValue(lightPosition);
                }
                mesh.Draw();
            }
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    currentEffect.Parameters["MaterialShininess"].SetValue(10.0f);

                    currentEffect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] *
                                Matrix.CreateRotationY(modelRotation) *
                                Matrix.CreateTranslation(lightPosition));
                    currentEffect.Parameters["View"].SetValue(Matrix.CreateLookAt(cameraPosition,
                        cameraTarget, Vector3.Up));
                    currentEffect.Parameters["Projection"].SetValue(Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f));
                    currentEffect.Parameters["LightPosition"].SetValue(lightPosition);
                }
                mesh.Draw();
            }

            base.Draw(gameTime);
        }
    }
}
