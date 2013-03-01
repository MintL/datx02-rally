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

        public List<OverlayView> Overlays = new List<OverlayView>();
        public OverlayView CurrentOverlay;

        public MenuView(Game game, GameState gameState) : base(game, gameState)
        {
            Overlays.Add(new MainMenu(game));
            CurrentOverlay = Overlays.First<OverlayView>();

            
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Background = Game.Content.Load<Texture2D>(@"Menu\Background");
            Logo = Game.Content.Load<Texture2D>(@"Menu\Menu-Title");

            
        }

        public override void ChangeResolution()
        {
            Rectangle bounds = CurrentOverlay.Bounds;
            Vector2 offset = GetScreenPosition(new Vector2(0.5f, 0.5f));
            offset -= new Vector2(bounds.Width, bounds.Height) / 2;
            CurrentOverlay.Bounds = new Rectangle((int)offset.X, (int)offset.Y, bounds.Width, bounds.Height);
        }

        public override void Draw(GameTime gameTime)
        {
            Vector2 position = new Vector2(0.5f, 0.15f);

            spriteBatch.Begin();
            spriteBatch.Draw(Background, Bounds, Color.White);
            spriteBatch.Draw(Logo, GetScreenPosition(position) - new Vector2(Logo.Bounds.Width, Logo.Bounds.Height) / 2, Color.White);
            spriteBatch.End();

            
            CurrentOverlay.Draw(gameTime);
            
            
            base.Draw(gameTime);
        }

        public override GameState UpdateState(GameTime gameTime)
        {
            CurrentOverlay.OffsetPosition(GetScreenPosition(new Vector2(0.001f, 0f)));//GetScreenPosition(new Vector2(0.5f, 0)  * gameTime.ElapsedGameTime.Seconds));

            return CurrentOverlay.UpdateState(gameTime);
        }

    }
}
