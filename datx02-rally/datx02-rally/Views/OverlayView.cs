using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using datx02_rally.Sound;

namespace datx02_rally.Menus
{
    public class OverlayView : GameStateView
    {
        public float Rotation { get; set; }

        public string MenuTitle { get; set; }
        public SpriteFont MenuFont { get; set; }
        public SpriteFont MenuHeaderFont { get; set; }
        public Texture2D Background { get; set; }
        public Texture2D RightBorder { get; set; }
        public Texture2D LeftBorder { get; set; }
        public Texture2D ButtonBackground { get; set; }
        public Texture2D ArrowLeft { get; set; }
        public Texture2D ArrowRight { get; set; }
        public Texture2D OptionSelected { get; set; }
        public Vector2 MenuItemOffset { get; set; }
        public Color ItemColor { get; set; }
        public Color ItemColorSelected { get; set; }
        public float Transparency { get; set; }
        public List<MenuItem> MenuItems { get; private set; }
        protected int selectedIndex = 0;

        public Rectangle RenderBounds { get; set; }
        protected RenderTarget2D renderTarget;

        public OverlayView(Game game, GameState gameState)
            : base(game, gameState)
        {
            
            Transparency = 1f; //no transparency
            MenuItemOffset = new Vector2(0.005f, 0.005f);

            var index = selectedIndex;
            while (selectedIndex < MenuItems.Count && !MenuItems[selectedIndex].Selectable)
                selectedIndex++;
            if (selectedIndex > MenuItems.Count - 1)
                selectedIndex = index;

            Rotation = 0;
            
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            ItemColor = Color.White;
            ItemColorSelected = Color.Red;
            MenuItems = new List<MenuItem>();

            MenuFont = Game.Content.Load<SpriteFont>(@"Menu/MenuFont");
            MenuHeaderFont = Game.Content.Load<SpriteFont>(@"Menu/MenuHeaderFont");
            Background = Game.Content.Load<Texture2D>(@"Menu/Menu-BG");
            RightBorder = Game.Content.Load<Texture2D>(@"Menu/Menu-Right-Border");
            LeftBorder = Game.Content.Load<Texture2D>(@"Menu/Menu-Left-Border");
            ButtonBackground = Game.Content.Load<Texture2D>(@"Menu/Menu-button");
            ArrowLeft = Game.Content.Load<Texture2D>(@"Menu/Menu-Left-Arrow");
            ArrowRight = Game.Content.Load<Texture2D>(@"Menu/Menu-Right-Arrow");
            OptionSelected = Game.Content.Load<Texture2D>(@"Menu/Menu-Option-Selected");

            Vector2 size = GetScreenPosition(new Vector2(0.8f, 0.8f));
            RenderBounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);

        }

        public override void ChangeResolution()
        {
            
            renderTarget = new RenderTarget2D(GraphicsDevice, RenderBounds.Width, RenderBounds.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
        }

        public void OffsetPosition(Vector2 offset)
        {
            Position += offset;
        }

        protected virtual void RenderContent(Vector2 renderOffset)
        {
            int noOfItemsBottom = 0,
                noOfItemsTop = 0,
                noOfItemsCenter = 0;

            spriteBatch.Begin();

            for (int i = 0; i < MenuItems.Count; i++)
            {
                MenuItem menuItem = MenuItems[i];
                int noInOrder;
                if (menuItem.MenuPositionY == ItemPositionY.TOP)
                    noInOrder = noOfItemsTop++;
                else if (menuItem.MenuPositionY == ItemPositionY.BOTTOM)
                    noInOrder = noOfItemsBottom++;
                else
                    noInOrder = noOfItemsCenter++;

                Vector2 position = renderOffset;
                Vector2 offset = GetScreenPosition(MenuItemOffset);

                position.Y += Bounds.Height / 2 - MenuItems.Count / 2 * (menuItem.Bounds.Height + offset.Y) +
                    noInOrder * (menuItem.Bounds.Height + offset.Y);
                menuItem.Draw(spriteBatch, position, i == selectedIndex);
            }

            spriteBatch.End();
        }

        public Texture2D Render()
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Transparent);

            Vector2 renderOffset = new Vector2((RenderBounds.Width - Bounds.Width) / 2,
                    (RenderBounds.Height - Bounds.Height) / 2);

            spriteBatch.Begin();
            spriteBatch.Draw(Background,
                new Rectangle((int)renderOffset.X, (int)renderOffset.Y, Bounds.Width, Bounds.Height),
                Color.White);
            spriteBatch.Draw(LeftBorder,
                new Rectangle((int)renderOffset.X - LeftBorder.Width, (int)renderOffset.Y, LeftBorder.Width, Bounds.Height),
                Color.White);
            spriteBatch.Draw(RightBorder,
                new Rectangle((int)renderOffset.X + Bounds.Width, (int)renderOffset.Y, LeftBorder.Width, Bounds.Height),
                Color.White);
            spriteBatch.DrawString(MenuHeaderFont, MenuTitle,
                new Vector2((int)(renderOffset.X + Bounds.Width * 0.03), (int)(renderOffset.Y + Bounds.Height * 0.14) - MenuHeaderFont.MeasureString(MenuTitle).Y),
                Color.White);
            //
            if (mousePos.HasValue) 
            {
                Texture2D rect = new Texture2D(GraphicsDevice, 5, 5);

                Color[] data = new Color[5 * 5];
                for (int i = 0; i < data.Length; ++i) data[i] = Color.Blue;
                rect.SetData(data);

                Vector2 coor = new Vector2(mousePos.Value.X, mousePos.Value.Y);
                spriteBatch.Draw(rect, coor, Color.White);
            }
            //
            spriteBatch.End();

            RenderContent(renderOffset);

            GraphicsDevice.SetRenderTarget(null);

            return (Texture2D)renderTarget;
        }

        MouseState? mousePos = null;
        public override GameState UpdateState(GameTime gameTime)
        {
            InputComponent input = gameInstance.GetService<InputComponent>();
            var prevIndex = selectedIndex;
            GameState nextGameState = GameState.None;

            mousePos = input.GetMousePosition();
            if (mousePos.HasValue)
            {
                int posX = (int)(mousePos.Value.X - 0.5f);
                int posY = (int)(mousePos.Value.Y - 0.15f);
                mousePos = new MouseState(posX, posY, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
                for (int i = 0; i < MenuItems.Count; i++)
                {
                    var item = MenuItems[i];
                    if (item.Selectable && item.LastDrawRectangle.Contains(posX, posY))
                    {
                        selectedIndex = i;
                        Console.WriteLine(item.LastDrawRectangle + ", item " + item.Text + " contains mouse");
                    }
                    Console.WriteLine(DateTime.Now + ": " + mousePos);
                }
            }

            if (input.GetKey(Keys.Down))
            {
                var index = selectedIndex;
                do
                {
                    selectedIndex++;
                } while (selectedIndex < MenuItems.Count && !MenuItems[selectedIndex].Selectable);

                if (selectedIndex > MenuItems.Count - 1)
                    selectedIndex = index;
            }
            else if (input.GetKey(Keys.Up))
            {
                var index = selectedIndex;
                do
                {
                    selectedIndex--;
                } while (selectedIndex >= 0 && !MenuItems[selectedIndex].Selectable);

                if (selectedIndex < 0)
                    selectedIndex = index;
            }
            if (selectedIndex != prevIndex)
                AudioEngineManager.PlaySound("menutick");
            else if (input.GetKey(Keys.Right) && MenuItems[selectedIndex] is OptionMenuItem)
                (MenuItems[selectedIndex] as OptionMenuItem).NextOption();
            else if (input.GetKey(Keys.Left) && MenuItems[selectedIndex] is OptionMenuItem)
                (MenuItems[selectedIndex] as OptionMenuItem).PreviousOption();
            else if (input.GetKey(Keys.Enter) && MenuItems[selectedIndex] is StateActionMenuItem)
                nextGameState = (MenuItems[selectedIndex] as StateActionMenuItem).NextState;
            else if (input.GetKey(Keys.Enter) && MenuItems[selectedIndex] is ActionMenuItem)
                (MenuItems[selectedIndex] as ActionMenuItem).PerformAction();
            return nextGameState != GameState.None ? nextGameState : this.gameState;
        }

        public void AddMenuItem(MenuItem menuItem)
        {
            MenuItems.Add(menuItem);
        }

        /// <summary>
        /// Replaces the menu item with the given identifier with provided menu item.
        /// If the menu item is not found, nothing will happen.
        /// </summary>
        /// <param name="identifier"></param>
        public void SetMenuItem(string identifier, MenuItem menuItem)
        {
            int index = MenuItems.FindIndex(item => item.Identifier == identifier);
            if (index > -1)
            {
                MenuItems[index] = menuItem;
            }
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
            return MenuItems.Find(item => item.Identifier == identifier);
        }

    }
}
