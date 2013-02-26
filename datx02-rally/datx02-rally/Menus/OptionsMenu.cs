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
        SpriteBatch spriteBatch;
        int selectedIndex = 0;
        SpriteFont font;
        Color itemColor;
        Color selectedColor;
        MenuItem applyButton;
        OptionMenuItem resolution;
        BoolOptionMenuItem performanceMode;

        public OptionsMenu(Game game)
            : base(game)
        {
            font = game.Content.Load<SpriteFont>(@"Menu/MenuFont");

            itemColor = Color.Blue;
            selectedColor = Color.RoyalBlue;
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // populate menu

            List<string> resolutionList = new List<string>();
            foreach (var mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                resolutionList.Add(mode.Width + "x" + mode.Height);
            }

            string currentResolution = Game1.GetInstance().GraphicsDevice.Viewport.Width + "x" + GraphicsDevice.Viewport.Height;

            menuItems.Add((resolution = new OptionMenuItem("Resolution", resolutionList, currentResolution)));
            menuItems.Add((performanceMode = new BoolOptionMenuItem("Performance mode", GameSettings.Default.PerformanceMode)));
            menuItems.Add((applyButton = new ActionMenuItem("Apply", null)));
            menuItems.Add(new ActionMenuItem("Back", GameState.MainMenu));
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            float centerX = gameInstance.GraphicsDevice.Viewport.Width / 2;
            float centerY = gameInstance.GraphicsDevice.Viewport.Height / 2;
            int fontSize = font.LineSpacing;

            for (int i = 0; i < menuItems.Count; i++)
            {
                MenuItem item = menuItems[i];
                Color color = i == selectedIndex ? selectedColor : itemColor;
                Vector2 textSize = font.MeasureString(menuItems[i].Text);
                if (item is OptionMenuItem)
                {
                    OptionMenuItem optItem = item as OptionMenuItem;
                    string text = optItem.Text + ":       " + optItem.SelectedOption();
                    textSize = font.MeasureString(text);
                    spriteBatch.DrawString(font, text, new Vector2(centerX - (textSize.X / 2), centerY + (fontSize * i)), color);
                }
                else if (item is ActionMenuItem)
                {
                    spriteBatch.DrawString(font, item.Text, new Vector2(centerX - (textSize.X / 2), centerY + (fontSize * i)), color);
                }
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
            else if (input.GetKey(Keys.Right) && menuItems[selectedIndex] is OptionMenuItem)
                (menuItems[selectedIndex] as OptionMenuItem).Next();
            else if (input.GetKey(Keys.Left) && menuItems[selectedIndex] is OptionMenuItem)
                (menuItems[selectedIndex] as OptionMenuItem).Previous();
            else if (input.GetKey(Keys.Enter) && menuItems[selectedIndex] is ActionMenuItem)
            {
                if (menuItems[selectedIndex] == applyButton)
                    ApplySettings();
                else
                    nextGameState = (menuItems[selectedIndex] as ActionMenuItem).NextState;
            }
            return nextGameState != GameState.None ? nextGameState : GameState.OptionsMenu;
        }

        private void ApplySettings()
        {
            string[] res = resolution.SelectedOption().Split('x');
            GameSettings.Default.ResolutionX = int.Parse(res[0]);
            GameSettings.Default.ResolutionY = int.Parse(res[1]);
            GameSettings.Default.PerformanceMode = performanceMode.IsEnabled();
            GameSettings.Default.Save();

        }
    }
}
