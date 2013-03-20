using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace datx02_rally.Menus
{
    public class OverlayView : GameStateView
    {
        public float Rotation { get; set; }

        public SpriteFont MenuFont { get; set; }
        public Texture2D Background { get; set; }
        public Texture2D ButtonBackground { get; set; }
        public Texture2D ArrowLeft { get; set; }
        public Texture2D ArrowRight { get; set; }
        public Texture2D OptionSelected { get; set; }
        public Vector2 MenuItemOffset { get; set; }
        public Color ItemColor { get; set; }
        public Color ItemColorSelected { get; set; }
        public float Transparency { get; set; }
        private List<MenuItem> menuItems = new List<MenuItem>();
        private int selectedIndex = 0;

        public Rectangle RenderBounds { get; set; }
        private RenderTarget2D renderTarget;

        public OverlayView(Game game, GameState gameState)
            : base(game, gameState)
        {
            
            Transparency = 1f; //no transparency
            MenuItemOffset = new Vector2(0.005f, 0.005f);
            
            

            Rotation = 0;
            
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            ItemColor = Color.White;
            ItemColorSelected = Color.Red;

            MenuFont = Game.Content.Load<SpriteFont>(@"Menu/MenuFont");
            Background = Game.Content.Load<Texture2D>(@"Menu/Menu-BG");
            ButtonBackground = Game.Content.Load<Texture2D>(@"Menu/Menu-button");
            ArrowLeft = Game.Content.Load<Texture2D>(@"Menu/Menu-Left-Arrow");
            ArrowRight = Game.Content.Load<Texture2D>(@"Menu/Menu-Right-Arrow");
            OptionSelected = Game.Content.Load<Texture2D>(@"Menu/Menu-Option-Selected");

            Vector2 size = GetScreenPosition(new Vector2(0.6f, 0.6f));
            RenderBounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);

        }

        public override void ChangeResolution()
        {
            
            renderTarget = new RenderTarget2D(GraphicsDevice, RenderBounds.Width, RenderBounds.Height);
        }

        public void OffsetPosition(Vector2 offset)
        {
            Position += offset;

        }

        public Texture2D Render()
        {
            int noOfItemsBottom = 0,
                noOfItemsTop = 0,
                noOfItemsCenter = 0;


            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Transparent);

            Vector2 renderOffset = new Vector2((RenderBounds.Width - Bounds.Width) / 2,
                    (RenderBounds.Height - Bounds.Height) / 2);

            spriteBatch.Begin();
            spriteBatch.Draw(Background,
                new Rectangle((int)renderOffset.X, (int)renderOffset.Y, Bounds.Width, Bounds.Height),
                Color.White);

            for (int i = 0; i < menuItems.Count; i++)
            {
                MenuItem menuItem = menuItems[i];
                int noInOrder;
                if (menuItem.MenuPositionY == ItemPositionY.TOP)
                    noInOrder = noOfItemsTop++;
                else if (menuItem.MenuPositionY == ItemPositionY.BOTTOM)
                    noInOrder = noOfItemsBottom++;
                else
                    noInOrder = noOfItemsCenter++;

                Vector2 position = renderOffset;
                Vector2 offset = GetScreenPosition(MenuItemOffset);

                position.Y += Bounds.Height / 2 - menuItems.Count / 2 * (menuItem.Bounds.Height + offset.Y) +
                    noInOrder * (menuItem.Bounds.Height + offset.Y);
                menuItem.Draw(spriteBatch, position, i == selectedIndex); 
            }

            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            return (Texture2D)renderTarget;
        }

        public override GameState UpdateState(Microsoft.Xna.Framework.GameTime gameTime)
        {
            InputComponent input = gameInstance.GetService<InputComponent>();
            GameState nextGameState = GameState.None;
            if (input.GetKey(Keys.Down))
                selectedIndex = Math.Min(menuItems.Count - 1, selectedIndex + 1);
            else if (input.GetKey(Keys.Up))
                selectedIndex = Math.Max(0, selectedIndex - 1);
            else if (input.GetKey(Keys.Right) && menuItems[selectedIndex] is OptionMenuItem)
                (menuItems[selectedIndex] as OptionMenuItem).NextOption();
            else if (input.GetKey(Keys.Left) && menuItems[selectedIndex] is OptionMenuItem)
                (menuItems[selectedIndex] as OptionMenuItem).PreviousOption();
            else if (input.GetKey(Keys.Enter) && menuItems[selectedIndex] is StateActionMenuItem)
                nextGameState = (menuItems[selectedIndex] as StateActionMenuItem).NextState;
            else if (input.GetKey(Keys.Enter) && menuItems[selectedIndex] is ActionMenuItem)
                (menuItems[selectedIndex] as ActionMenuItem).PerformAction();
            return nextGameState != GameState.None ? nextGameState : this.gameState;
        }

        public void AddMenuItem(MenuItem menuItem)
        {
            menuItems.Add(menuItem);
        }

        // TODO: Only works if all menuitems have equal height
        private Vector2 CalculateMenuItemPosition(MenuItem menuItem, int numberInOrder)
        {
            Vector2 textSize = MenuFont.MeasureString(menuItem.Text);
            
            float posX = 0;
            switch (menuItem.MenuPositionX)
            {
                case ItemPositionX.LEFT:
                    posX = 0;
                    break;
                case ItemPositionX.CENTER:
                    posX = (Bounds.Width / 2);
                    break;
                case ItemPositionX.RIGHT:
                    posX = (Bounds.Width) - textSize.X;
                    break;
            }

            float posY = 0;
            switch (menuItem.MenuPositionY)
            {
                case ItemPositionY.TOP:
                    posY = 0 + (numberInOrder * textSize.Y);
                    break;
                case ItemPositionY.CENTER:
                    posY = (Bounds.Height / 2) + (numberInOrder * textSize.Y);
                    break;
                case ItemPositionY.BOTTOM:
                    posY = (Bounds.Height) - (numberInOrder * textSize.Y);
                    break;
            }

            return new Vector2(posX, posY);
        }

        public MenuItem GetMenuItem(string identifier)
        {
            return menuItems.Find(item => item.Identifier == identifier);
        }
    }
}
