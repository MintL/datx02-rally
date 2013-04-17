using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace datx02_rally.Menus
{
    public class CarChooser : OverlayView
    {
        Car car;
        Effect carEffect;
        Color diffuseColor = Color.Red;
        DirectionalLight directionalLight;
        Matrix view, projection;

        Vector3 cameraPosition;
        float rotation = 0;

        List<Color> availableColors = new List<Color>() { Color.Red, Color.RoyalBlue, Color.Purple, Color.White };

        public CarChooser(Game game)
            : base(game, GameState.CarChooser)
        {
            MenuTitle = "Choose color";
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Vector2 size = GetScreenPosition(new Vector2(0.7f, 0.7f));
            Bounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);

            List<Tuple<String, GameState>> itemInfo = new List<Tuple<string, GameState>>();
            itemInfo.Add(new Tuple<String, GameState>("Back", GameState.MainMenu));

            foreach (var info in itemInfo)
            {
                MenuItem item = new StateActionMenuItem(info.Item1, info.Item2);
                item.Background = ButtonBackground;
                item.Font = MenuFont;
                item.FontColor = ItemColor;
                item.FontColorSelected = ItemColorSelected;
                item.SetWidth(Bounds.Width);
                AddMenuItem(item);
            }

            // 3D variables needed to display the car
            MakeCar();
            cameraPosition = new Vector3(0, 100f, 250f);
            view = Matrix.CreateLookAt(cameraPosition, new Vector3(0, 20f, 0f), Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.01f, 500f);

            if (availableColors.Exists(c => c == GameSettings.Default.CarColor)) {
                diffuseColor = GameSettings.Default.CarColor;
            }

            directionalLight = new DirectionalLight(
                new Vector3(1.25f, -2f, -5.0f), // Direction
                new Vector3(.1f, .1f, .1f), // Ambient
                new Vector3(0.7f, 0.7f, 0.7f)); // Diffuse
        }

        public override GameState UpdateState(GameTime gameTime)
        {
            rotation += (float)gameTime.ElapsedGameTime.TotalSeconds * .5f;

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
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            
            // Draw the car
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

                // Local modelspace
                world *= mesh.ParentBone.Transform;

                // World
                world *= Matrix.CreateRotationY(rotation) * car.TranslationMatrix;

                foreach (Effect effect in mesh.Effects) // 5 effects for main, 1 for each wheel
                {
                    EffectParameterCollection param = effect.Parameters;

                    param["MaterialReflection"].SetValue(.1f);
                    param["MaterialShininess"].SetValue(10);

                    param["World"].SetValue(world);
                    param["View"].SetValue(view);
                    param["Projection"].SetValue(projection);

                    param["NormalMatrix"].SetValue(Matrix.Invert(Matrix.Transpose(world)));
                    param["EyePosition"].SetValue(cameraPosition);

                    if (mesh.Name == "main")
                    {
                        param["MaterialDiffuse"].SetValue(diffuseColor.ToVector3());
                        param["MaterialAmbient"].SetValue(diffuseColor.ToVector3());
                    }

                    param["DirectionalLightDirection"].SetValue(directionalLight.Direction);
                    param["DirectionalLightDiffuse"].SetValue(directionalLight.Diffuse);
                    param["DirectionalLightAmbient"].SetValue(directionalLight.Ambient);
                }

                mesh.Draw();
            }

            base.RenderContent(renderOffset);

        }

        public void MakeCar()
        {
            // Load car effect (10p-light, env-map)
            carEffect = content.Load<Effect>(@"Effects/CarShading");

            car = new Car(content.Load<Model>(@"Models/Cars/porsche_new"), 13.4631138f);

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

                        part.Effect.Parameters["DiffuseMap"].SetValue(oldEffect.Texture);
                        //part.Effect.Parameters["DiffuseMap"].SetValue(content.Load<Texture2D>(@"Terrain\grass"));

                        part.Effect.Parameters["MaterialDiffuse"].SetValue(oldEffect.DiffuseColor);
                        part.Effect.Parameters["MaterialAmbient"].SetValue(oldEffect.DiffuseColor * .5f);
                        part.Effect.Parameters["MaterialSpecular"].SetValue(oldEffect.DiffuseColor * .3f);
                    }
                }
            }
        }

    }
}
