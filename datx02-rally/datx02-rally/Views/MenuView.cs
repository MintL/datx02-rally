using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace datx02_rally.Menus
{
    public class MenuView : GameStateView
    {
        public Texture2D Background { get; set; }
        public Texture2D Logo { get; set; }

        public float Speed { get; set; }
        public Vector2 TargetPosition { get; set; }
        public float TargetRotation { get; set; }

        public List<OverlayView> Overlays = new List<OverlayView>();
        public Dictionary<OverlayView, Texture2D> OverlayTextures = new Dictionary<OverlayView, Texture2D>();
        public OverlayView CurrentOverlay;

        private GameState oldState;

        #region Plane
        VertexPositionTexture[] verts;
        VertexBuffer vertexBuffer;
        BasicEffect basicEffect;
        #endregion

        public MenuView(Game game, GameState gameState) : base(game, gameState)
        {
            Overlays.Add(new MainMenu(game));
            CurrentOverlay = Overlays.First<OverlayView>();
            oldState = GameState.MainMenu;

            ChangeResolution();

            Speed = 10f;
            TargetRotation = -65;
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Background = Game.Content.Load<Texture2D>(@"Menu\Background");
            Logo = Game.Content.Load<Texture2D>(@"Menu\Menu-Title");

            // Initialize vertices
            verts = new VertexPositionTexture[4];
            verts[0] = new VertexPositionTexture(
                new Vector3(-1, 1, 0), new Vector2(0, 0));
            verts[1] = new VertexPositionTexture(
                new Vector3(1, 1, 0), new Vector2(1, 0));
            verts[2] = new VertexPositionTexture(
                new Vector3(-1, -1, 0), new Vector2(0, 1));
            verts[3] = new VertexPositionTexture(
                new Vector3(1, -1, 0), new Vector2(1, 1));

            // Set vertex data in VertexBuffer
            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), verts.Length, BufferUsage.None);
            vertexBuffer.SetData(verts);
            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), verts.Length, BufferUsage.None);
            vertexBuffer.SetData(verts);
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.View = Matrix.CreateLookAt(Vector3.UnitZ * 25, Vector3.Zero, Vector3.Up);
            basicEffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f);
        }

        private void ResetMenu()
        {
            CurrentOverlay.Position = new Vector2(Bounds.Width/4, 0);
        }

        public override void ChangeResolution()
        {
            TargetPosition = Vector2.Zero;

            CurrentOverlay.ChangeResolution();

            //UpdateRenders();
        }

        public void UpdateRenders()
        {
            OverlayTextures.Clear();
            foreach (OverlayView overlay in Overlays)
            {
                OverlayTextures.Add(overlay, overlay.Render());
            }
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            Vector2 position = new Vector2(0.5f, 0.15f);

            spriteBatch.Begin();
            spriteBatch.Draw(Background, Bounds, Color.White);
            spriteBatch.Draw(Logo, GetScreenPosition(position) - new Vector2(Logo.Bounds.Width, Logo.Bounds.Height) / 2, Color.White);
            spriteBatch.End();

            foreach (var overlayTexture in OverlayTextures)
            {
                OverlayView overlay = overlayTexture.Key;
                basicEffect.World =
                    Matrix.CreateTranslation(overlay.Position.X * 0.01f, -overlay.Position.Y * 0.01f, 1.8f) *
                    Matrix.CreateRotationY(MathHelper.ToRadians(overlay.Rotation)) *
                    Matrix.CreateScale(overlay.RenderBounds.Width * 0.01f, overlay.RenderBounds.Height * 0.01f, 1f);

                basicEffect.Texture = overlayTexture.Value;
                basicEffect.TextureEnabled = true;
                EffectPass pass = basicEffect.CurrentTechnique.Passes[0];
                pass.Apply();

                GraphicsDevice.SetVertexBuffer(vertexBuffer);
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }

            base.Draw(gameTime);
        }

        // TEMP
        int rotationOffset = 0;

        public override GameState UpdateState(GameTime gameTime)
        {
            Vector2 direction = TargetPosition - CurrentOverlay.Position;
            if (direction.Length() > 5f)
            {
                //direction.Normalize();
                Vector2 d = direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                CurrentOverlay.OffsetPosition(d);
            }

            for (int i=0; i<Overlays.Count; i++)
            {
                OverlayView overlay = Overlays[i];
                float rotationDir = TargetRotation * (i+rotationOffset) - overlay.Rotation;
                if (Math.Abs(rotationDir) > 0.01f)
                {
                    overlay.Rotation += rotationDir * 10 * (float)gameTime.ElapsedGameTime.TotalSeconds; 
                }
            }

            InputComponent input = gameInstance.GetService<InputComponent>();
            if (input.GetKey(Keys.PageUp)) rotationOffset += 1;
            if (input.GetKey(Keys.PageDown)) rotationOffset -= 1;

            GameState state = CurrentOverlay.UpdateState(gameTime);
            
            
            if (state != oldState)
            {
                // If the new game state is the previous one
                if (Overlays.Count > 1 && Overlays[1].gameState == state)
                {
                    Overlays.RemoveAt(0);
                    CurrentOverlay = Overlays.First<OverlayView>();
                    ChangeResolution();
                }
                else
                {
                    if (state == GameState.MainMenu)
                    {
                        Overlays.Insert(0, new MainMenu(Game));
                        CurrentOverlay = Overlays.First<OverlayView>();
                        ChangeResolution();
                        ResetMenu();
                    }
                    else if (state == GameState.MultiplayerMenu)
                    {
                        Overlays.Insert(0, new MultiplayerMenu(Game));
                        CurrentOverlay = Overlays.First<OverlayView>();
                        ChangeResolution();
                        ResetMenu();
                    }
                    else if (state == GameState.OptionsMenu)
                    {
                        Overlays.Insert(0, new OptionsMenu(Game));
                        CurrentOverlay = Overlays.First<OverlayView>();
                        ChangeResolution();
                        ResetMenu();
                    }
                    else
                    {
                        if (state == GameState.Gameplay)
                        {
                            Game.Components.Add(new GamePlayView(Game, null, null));
                            return GameState.Gameplay;
                        }
                        return state;
                    }
                }
            }
            oldState = state;


            UpdateRenders();
                    

            return GameState.MainMenu;
        }

    }
}
