using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace datx02_rally.Menus
{
    public abstract class MenuView : GameStateView
    {
        public SpriteFont MenuFont { get; set; }
        public Color ItemColor { get; set; }
        public Color ItemColorSelected { get; set; }
        public readonly GameState gameState;
        private List<IMenuItem> menuItems = new List<IMenuItem>();
        protected SpriteBatch spriteBatch;
        private int selectedIndex = 0;

        public MenuView(Game game, GameState gameState) : base(game)
        {
            ItemColor = Color.Black;
            ItemColorSelected = Color.Red;
            this.gameState = gameState;

            MenuFont = game.Content.Load<SpriteFont>(@"Menu/MenuFont");
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
        }

        public override void Draw(GameTime gameTime)
        {
            int noOfItemsBottom = 0,
                noOfItemsTop = 0,
                noOfItemsCenter = 0;

            spriteBatch.Begin();

            for (int i = 0; i < menuItems.Count; i++)
            {
                IMenuItem menuItem = menuItems[i];
                int noInOrder;
                if (menuItem.MenuPositionY == ItemPositionY.TOP)
                    noInOrder = noOfItemsTop++;
                else if (menuItem.MenuPositionY == ItemPositionY.BOTTOM)
                    noInOrder = noOfItemsBottom++;
                else
                    noInOrder = noOfItemsCenter++;

                Color color = i == selectedIndex ? ItemColorSelected : ItemColor;

                spriteBatch.DrawString(MenuFont, menuItem.Text, CalculateMenuItemPosition(menuItem, noInOrder), color);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public override GameState UpdateState(Microsoft.Xna.Framework.GameTime gameTime)
        {
            InputComponent input = gameInstance.GetService<InputComponent>();
            GameState nextGameState = GameState.None;
            if (input.GetKey(Keys.Down))
                selectedIndex = Math.Min(menuItems.Count - 1, selectedIndex + 1);
            else if (input.GetKey(Keys.Up))
                selectedIndex = Math.Max(0, selectedIndex - 1);
            else if (input.GetKey(Keys.Right) && menuItems[selectedIndex] is IOptionMenuItem)
                (menuItems[selectedIndex] as IOptionMenuItem).NextOption();
            else if (input.GetKey(Keys.Left) && menuItems[selectedIndex] is IOptionMenuItem)
                (menuItems[selectedIndex] as IOptionMenuItem).PreviousOption();
            else if (input.GetKey(Keys.Enter) && menuItems[selectedIndex] is StateActionMenuItem)
                nextGameState = (menuItems[selectedIndex] as StateActionMenuItem).NextState;
            else if (input.GetKey(Keys.Enter) && menuItems[selectedIndex] is ActionMenuItem)
                (menuItems[selectedIndex] as ActionMenuItem).PerformAction();
            return nextGameState != GameState.None ? nextGameState : this.gameState;
        }

        public void AddMenuItem(IMenuItem menuItem)
        {
            menuItems.Add(menuItem);
        }

        // TODO: Only works if all menuitems have equal height
        private Vector2 CalculateMenuItemPosition(IMenuItem menuItem, int numberInOrder)
        {
            Vector2 textSize = MenuFont.MeasureString(menuItem.Text);

            float posX = 0;
            switch (menuItem.MenuPositionX)
            {
                case ItemPositionX.LEFT:
                    posX = 0;
                    break;
                case ItemPositionX.CENTER:
                    posX = (gameInstance.GraphicsDevice.Viewport.Width / 2) - (textSize.X / 2);
                    break;
                case ItemPositionX.RIGHT:
                    posX = (gameInstance.GraphicsDevice.Viewport.Width) - textSize.X;
                    break;
            }

            float posY = 0;
            switch (menuItem.MenuPositionY)
            {
                case ItemPositionY.TOP:
                    posY = 0 + (numberInOrder * textSize.Y);
                    break;
                case ItemPositionY.CENTER:
                    posY = (gameInstance.GraphicsDevice.Viewport.Height / 2) + (numberInOrder * textSize.Y);
                    break;
                case ItemPositionY.BOTTOM:
                    posY = (gameInstance.GraphicsDevice.Viewport.Height) - (numberInOrder * textSize.Y);
                    break;
            }

            return new Vector2(posX, posY);
        }

        public IMenuItem GetMenuItem(string identifier)
        {
            return menuItems.Find(item => item.Identifier == identifier);
        }
    }
}
