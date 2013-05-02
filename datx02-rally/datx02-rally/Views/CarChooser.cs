using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using datx02_rally.Cameras;

namespace datx02_rally.Menus
{
    public class CarChooser : OverlayView
    {
        #region Field

        Car car;
        Effect carEffect;

        Color diffuseColor = Color.Red;
        DirectionalLight directionalLight;
        Matrix projection;

        Vector3 cameraPosition;
        float rotation = 0;

        List<Color> availableColors = new List<Color>() { Color.Red, Color.RoyalBlue, Color.Purple, Color.White };

        #endregion

        public CarChooser(Game game)
            : base(game, GameState.CarChooser)
        {
            MenuTitle = "Choose color";
        }

        Camera cam;

        public override void Initialize()
        {
            base.Initialize();
            cam = new CarChooserCamera(300, .5f);
            var camComp = Game.GetService<CameraComponent>();
            camComp .AddCamera(cam);
            camComp.CurrentCamera = cam;

        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Vector2 size = GetScreenPosition(new Vector2(0.5f, 0.6f));
            Bounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);

            List<Tuple<String, GameState>> itemInfo = new List<Tuple<string, GameState>>();
            itemInfo.Add(new Tuple<String, GameState>("Back", GameState.MainMenu));
            itemInfo.Add(new Tuple<String, GameState>("Start", GameState.Gameplay));

            foreach (var info in itemInfo)
            {
                MenuItem item = new StateActionMenuItem(info.Item1, info.Item2);
                item.Background = ButtonBackground;
                item.Font = MenuFont;
                item.FontColor = ItemColor;
                item.FontColorSelected = ItemColorSelected;
                item.SetWidth(ButtonBackground.Bounds.Width);
                AddMenuItem(item);
            }

            // 3D variables needed to display the car
            MakeCar();
            cameraPosition = new Vector3(0, 100f, 300f);

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.01f, 500f);

            if (availableColors.Exists(c => c == GameSettings.Default.CarColor)) {
                diffuseColor = GameSettings.Default.CarColor;
            }

            directionalLight = new DirectionalLight(
                new Vector3(1, -1, 1), // new Vector3(3f, -10f, -5.0f), // Direction
                new Vector3(.1f, .1f, .1f), // Ambient
                new Vector3(.7f, .7f, .7f)); // Diffuse
        }

        public override GameState UpdateState(GameTime gameTime)
        {
            //rotation += (float)gameTime.ElapsedGameTime.TotalSeconds * .5f;
            //car.Speed = ((float)Math.Sin(gameTime.TotalGameTime.TotalSeconds) + 2f) * 1.5f;
            //car.Update();

            InputComponent input = Game.GetService<InputComponent>();
            int i = availableColors.FindIndex(0, c => c == diffuseColor);
            if (input.GetKey(Keys.Left))
            {
                i--;
                if (i < 0) i = availableColors.Count - 1;
            }
            else if (input.GetKey(Keys.Right))
                i = (++i % availableColors.Count);

            diffuseColor = availableColors[i];
            GameSettings.Default.CarColor = diffuseColor;

            GameState state = base.UpdateState(gameTime);
            if (state != this.gameState)
            {
                GameSettings.Default.Save();
            }
            return state;
        }

        protected override void RenderContent(Vector2 renderOffset)
        {
            // Reset render settings
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // TODO: CARMOVE
            car.View = Game.GetService<CameraComponent>().View;
            
            car.Projection = projection;

            car.Position = Vector3.Zero;

            car.Draw(null);

            //// Draw the car
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
            //    world *= Matrix.CreateRotationY(rotation) * car.TranslationMatrix;

            //    foreach (Effect effect in mesh.Effects) // 5 effects for main, 1 for each wheel
            //    {
            //        EffectParameterCollection param = effect.Parameters;

            //        param["MaterialReflection"].SetValue(.1f);
            //        param["MaterialShininess"].SetValue(10);

            //        param["World"].SetValue(world);
            //        param["View"].SetValue(view);
            //        param["Projection"].SetValue(projection);

            //        param["NormalMatrix"].SetValue(Matrix.Invert(Matrix.Transpose(world)));
            //        param["EyePosition"].SetValue(cameraPosition);

            //        if (mesh.Name == "main")
            //        {
            //            param["MaterialDiffuse"].SetValue(diffuseColor.ToVector3());
            //            param["MaterialAmbient"].SetValue(diffuseColor.ToVector3());
            //        }

            //        param["DirectionalLightDirection"].SetValue(directionalLight.Direction);
            //        param["DirectionalLightDiffuse"].SetValue(directionalLight.Diffuse);
            //        param["DirectionalLightAmbient"].SetValue(directionalLight.Ambient);
            //    }

            //    mesh.Draw();
            //}

            spriteBatch.Begin();

            Vector2 leftPos = renderOffset + new Vector2(Bounds.Width / 10, Bounds.Height / 2 - ArrowLeft.Height * 10 / 2);
            Vector2 rightPos = renderOffset + new Vector2(Bounds.Width * 9 / 10, Bounds.Height / 2 - ArrowRight.Height * 10 / 2);
            spriteBatch.Draw(ArrowLeft, new Rectangle((int)leftPos.X, (int)leftPos.Y, ArrowLeft.Width, ArrowLeft.Height * 10), Color.White);
            spriteBatch.Draw(ArrowRight, new Rectangle((int)rightPos.X, (int)rightPos.Y, ArrowRight.Width, ArrowRight.Height * 10), Color.White);

            // Render the buttons down to the right
            for (int i = 0; i < MenuItems.Count; i++)
            {
                MenuItem menuItem = MenuItems[i];
                Vector2 position = renderOffset + new Vector2(Bounds.Width, Bounds.Height) - new Vector2(menuItem.Bounds.Width, menuItem.Bounds.Height) - GetScreenPosition(Vector2.UnitY * 0.03f);
                position.X -= (MenuItems.Count - i - 1) * menuItem.Bounds.Width;

                menuItem.Draw(spriteBatch, position, i == selectedIndex);
            }

            spriteBatch.End();

        }

        public void MakeCar()
        {
            // Load car effect (10p-light, env-map)
            carEffect = content.Load<Effect>(@"Effects/CarShading");

            car = Car.CreateCar(Game); // (content.Load<Model>(@"Models/Cars/porsche_new"), 13.4631138f);

            // TODO: Directional should be a service.
            car.DirectionalLight = directionalLight;
        }

    }
}
