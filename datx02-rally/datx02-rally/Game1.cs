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
        Model light;
        Vector3 modelPosition = Vector3.Zero;
        float modelRotation = 0.0f;
        float lightRotation;
        float lightDistance = 300.0f;

        ThirdPersonCamera camera;
        Vector2 screenCenter;

        Matrix projection;

        List<PointLight> pointLights;

        Effect effect;

        #region SkySphere

        Model skySphereModel;
        Effect skySphereEffect;

        #endregion

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
            light = Content.Load<Model>(@"Models/light");

            // Light specific parameters
            pointLights.Add(new PointLight(Vector3.Zero, Color.Black.ToVector3() * 0.2f, Color.White.ToVector3() * 1.0f, 500.0f));
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

            #region SkySphere

            skySphereModel = Content.Load<Model>(@"Models/skysphere");
            skySphereEffect = Content.Load<Effect>(@"Effects/SkySphere");
            
            TextureCube cubeMap = new TextureCube(GraphicsDevice, 2048, false, SurfaceFormat.Color);

            string[] cubemapfaces = { @"SkyBoxes/PurpleSky/skybox_right1", 
@"SkyBoxes/PurpleSky/skybox_left2", 
@"SkyBoxes/PurpleSky/skybox_top3", 
@"SkyBoxes/PurpleSky/skybox_bottom4", 
@"SkyBoxes/PurpleSky/skybox_front5", 
@"SkyBoxes/PurpleSky/skybox_back6" 
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

            Console.WriteLine(1 - Math.Pow(lightDistance / 500.0f, 2));

            lightRotation += (float)gameTime.ElapsedGameTime.Milliseconds * MathHelper.ToRadians(0.05f);

            PointLight pointLight = pointLights.First<PointLight>();
            pointLight.Position = Vector3.Transform(new Vector3(0.0f, 0.0f, 0.0f),
                Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, lightDistance)) * Matrix.CreateRotationY(lightRotation));

            camera.Update(Keyboard.GetState(), Mouse.GetState(), screenCenter);

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

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    EffectParameterCollection parameters = currentEffect.Parameters;
                    parameters["MaterialShininess"].SetValue(10.0f);

                    Matrix worldMatrix = transforms[mesh.ParentBone.Index] *
                                Matrix.CreateRotationY(modelRotation) *
                                Matrix.CreateTranslation(modelPosition);

                    Matrix normalMatrix = Matrix.Invert(Matrix.Transpose(view * worldMatrix));
                    
                    parameters["World"].SetValue(worldMatrix);
                    parameters["View"].SetValue(view);
                    parameters["Projection"].SetValue(projection);
                    parameters["NormalMatrix"].SetValue(normalMatrix);

                    // light parameters
                    PointLight pointLight = pointLights.First<PointLight>();
                    parameters["LightPosition"].SetValue(pointLight.Position);
                    parameters["LightAmbient"].SetValue(pointLight.Ambient);
                    parameters["LightDiffuse"].SetValue(pointLight.Diffuse);
                    parameters["LightRange"].SetValue(pointLight.Range);
                }
                mesh.Draw();
            }

            foreach (ModelMesh mesh in light.Meshes)
            {
                foreach (BasicEffect currentEffect in mesh.Effects)
                {
                    PointLight pointLight = pointLights.First<PointLight>();
                    currentEffect.World = transforms[mesh.ParentBone.Index] *
                                Matrix.CreateRotationY(modelRotation) *
                                Matrix.CreateTranslation(pointLight.Position);
                    currentEffect.View = view;
                    currentEffect.Projection = projection;
                    
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


            base.Draw(gameTime);
        }
    }
}
