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


namespace datx02_rally.Menus
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class OptionsMenu : GameStateView
    {
        List<MenuItem> menuItems = new List<MenuItem>();
        int selectedIndex = 0;
        SpriteFont font;
        Color itemColor;
        Color selectedColor;

        public OptionsMenu(Game game)
            : base(game)
        {
            font = game.Content.Load<SpriteFont>(@"Menu/MenuFont");
            menuItems.Add(new MenuItem("Option 1"));
            menuItems.Add(new MenuItem("Option 2"));
            menuItems.Add(new MenuItem("Option 3"));
            menuItems.Add(new MenuItem("Back", GameState.MainMenu));

            itemColor = Color.Blue;
            selectedColor = Color.RoyalBlue;
        }

        public override void Draw(GameTime gameTime)
        {
            game.spriteBatch.Begin();
            float centerX = game.GraphicsDevice.Viewport.Width / 2;
            float centerY = game.GraphicsDevice.Viewport.Height / 2;
            int fontSize = font.LineSpacing;

            for (int i = 0; i < menuItems.Count; i++)
            {
                Color color = i == selectedIndex ? selectedColor : itemColor;
                Vector2 textSize = font.MeasureString(menuItems[i].Text);
                game.spriteBatch.DrawString(font, menuItems[i].Text, new Vector2(centerX - (textSize.X / 2), centerY + (fontSize * i)), color);
            }
            game.spriteBatch.End();

            base.Draw(gameTime);
        }

        public override GameState UpdateState()
        {
            InputComponent input = game.GetService<InputComponent>();
            GameState nextGameState = GameState.None;
            if (input.GetKey(Keys.Down))
                selectedIndex = Math.Min(menuItems.Count - 1, selectedIndex + 1);
            else if (input.GetKey(Keys.Up))
                selectedIndex = Math.Max(0, selectedIndex - 1);
            else if (input.GetKey(Keys.Enter))
                nextGameState = menuItems[selectedIndex].NextState;
            return nextGameState != GameState.None ? nextGameState : GameState.OptionsMenu;
        }
    }
}
