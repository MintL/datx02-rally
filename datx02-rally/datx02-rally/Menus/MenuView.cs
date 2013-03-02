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
        public OverlayView CurrentOverlay;

        #region Plane
        VertexPositionTexture[] verts;
        VertexBuffer vertexBuffer;
        BasicEffect basicEffect;
        #endregion

        public MenuView(Game game, GameState gameState) : base(game, gameState)
        {
            Overlays.Add(new MainMenu(game));
            CurrentOverlay = Overlays.First<OverlayView>();

            Speed = 5f;
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
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.View = Matrix.CreateLookAt(Vector3.UnitZ * 25, Vector3.Zero, Vector3.Up);
            basicEffect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 100f);
        }

        private void ResetMenu()
        {
            CurrentOverlay.Position = new Vector2(Bounds.Width, 0);
        }

        public override void ChangeResolution()
        {
            Rectangle bounds = CurrentOverlay.Bounds;
            Vector2 offset = GetScreenPosition(new Vector2(0.5f, 0.5f));
            offset -= new Vector2(bounds.Width, bounds.Height) / 2;
            CurrentOverlay.Bounds = new Rectangle(0, 0, bounds.Width, bounds.Height);
            TargetPosition = Vector2.Zero;

            CurrentOverlay.ChangeResolution();
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            
            Texture2D overlayTexture = CurrentOverlay.Render();

            Vector2 position = new Vector2(0.5f, 0.15f);

            spriteBatch.Begin();
            spriteBatch.Draw(Background, Bounds, Color.White);
            spriteBatch.Draw(Logo, GetScreenPosition(position) - new Vector2(Logo.Bounds.Width, Logo.Bounds.Height) / 2, Color.White);
            spriteBatch.End();

            basicEffect.World = 
                Matrix.CreateTranslation(CurrentOverlay.Position.X * 0.01f, -CurrentOverlay.Position.Y * 0.01f, 3) *
                Matrix.CreateRotationY(MathHelper.ToRadians(CurrentOverlay.Rotation)) * 
                Matrix.CreateScale(CurrentOverlay.Bounds.Width * 0.01f, CurrentOverlay.Bounds.Height * 0.01f, 1f);
            
            basicEffect.Texture = overlayTexture;
            basicEffect.TextureEnabled = true;
            EffectPass pass = basicEffect.CurrentTechnique.Passes[0];
            pass.Apply();

            GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
                    (PrimitiveType.TriangleStrip, verts, 0, 2);

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
            if (state == GameState.OptionsMenu)
            {
                ResetMenu();
            }
            return GameState.MainMenu;
        }

    }
}
