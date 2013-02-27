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
    public class MainMenu : GameStateView
    {
        List<StateActionMenuItem> menuItems = new List<StateActionMenuItem>();
        SpriteBatch spriteBatch;
        int selectedIndex = 0;
        SpriteFont font;
        Color itemColor;
        Color selectedColor;

        public MainMenu(Game game) : base(game)
        {
            font = game.Content.Load<SpriteFont>(@"Menu/MenuFont");
            menuItems.Add(new StateActionMenuItem("Singleplayer", GameState.Gameplay));
            menuItems.Add(new StateActionMenuItem("Multiplayer", GameState.MultiplayerMenu));
            menuItems.Add(new StateActionMenuItem("Options", GameState.OptionsMenu));
            menuItems.Add(new StateActionMenuItem("Exit", GameState.Exiting));

            itemColor = Color.Blue;
            selectedColor = Color.Red;
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            float centerX = gameInstance.GraphicsDevice.Viewport.Width / 2;
            float centerY = gameInstance.GraphicsDevice.Viewport.Height / 2;
            int fontSize = font.LineSpacing;

            for (int i = 0; i < menuItems.Count; i++)
            {
                Color color = i == selectedIndex ? selectedColor : itemColor;
                Vector2 textSize = font.MeasureString(menuItems[i].Text);
                spriteBatch.DrawString(font, menuItems[i].Text, new Vector2(centerX - (textSize.X/2), centerY + (fontSize * i)), color);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public override GameState UpdateState(GameTime gameTime)
        {
            InputComponent input = gameInstance.GetService<InputComponent>();
            GameState nextGameState = GameState.None;
            if (input.GetKey(Keys.Down))
                selectedIndex = Math.Min(menuItems.Count - 1, selectedIndex + 1);
            else if (input.GetKey(Keys.Up))
                selectedIndex = Math.Max(0, selectedIndex - 1);
            else if (input.GetKey(Keys.Enter))
                nextGameState = menuItems[selectedIndex].NextState;
            return nextGameState != GameState.None ? nextGameState : GameState.MainMenu;
        }
    }
}
