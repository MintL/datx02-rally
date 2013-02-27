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
        private enum OptionID { Resolution, Fullscreen, PerformanceMode, Apply, Back }
        SpriteBatch spriteBatch;
        int selectedIndex = 0;
        SpriteFont font;
        Color itemColor;
        Color selectedColor;

        // options
        List<Tuple<OptionID, IMenuItem>> menuItems = new List<Tuple<OptionID, IMenuItem>>();

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

            OptionMenuItem<DisplayMode> resolution = new OptionMenuItem<DisplayMode>("Resolution");
            foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                resolution.AddOption(mode.Width + "x" + mode.Height, mode);
            resolution.SetStartOption(Game1.GetInstance().GraphicsDevice.Viewport.Width + "x" + GraphicsDevice.Viewport.Height);

            BoolOptionMenuItem fullscreen = new BoolOptionMenuItem("Fullscreen").SetStartOption(graphics.IsFullScreen) as BoolOptionMenuItem;
            BoolOptionMenuItem performanceMode = new BoolOptionMenuItem("Performance mode").SetStartOption(GameSettings.Default.PerformanceMode) as BoolOptionMenuItem;
            ActionMenuItem applyButton = new ActionMenuItem("Apply", new ActionMenuItem.Action(ApplySettings));
            StateActionMenuItem backButton = new StateActionMenuItem("Back", GameState.MainMenu);

            menuItems.Add(new Tuple<OptionID, IMenuItem>(OptionID.Resolution, resolution));
            menuItems.Add(new Tuple<OptionID, IMenuItem>(OptionID.Fullscreen, fullscreen));
            menuItems.Add(new Tuple<OptionID, IMenuItem>(OptionID.PerformanceMode, performanceMode));
            menuItems.Add(new Tuple<OptionID, IMenuItem>(OptionID.Apply, applyButton));
            menuItems.Add(new Tuple<OptionID, IMenuItem>(OptionID.Back, backButton));

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            float centerX = gameInstance.GraphicsDevice.Viewport.Width / 2;
            float centerY = gameInstance.GraphicsDevice.Viewport.Height / 2;

            spriteBatch.Begin();

            for (int i = 0; i < menuItems.Count; i++)
            {
                IMenuItem item = menuItems[i].Item2;
                Vector2 textSize = font.MeasureString(item.Text);
                Color color = i == selectedIndex ? selectedColor : itemColor;

                spriteBatch.DrawString(font, item.Text, new Vector2(centerX - (textSize.X / 2), centerY + (font.LineSpacing * i)), color);
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
            else if (input.GetKey(Keys.Right) && SelectedMenuItem() is IOptionMenuItem)
                (SelectedMenuItem() as IOptionMenuItem).NextOption();
            else if (input.GetKey(Keys.Left) && SelectedMenuItem() is IOptionMenuItem)
                (SelectedMenuItem() as IOptionMenuItem).PreviousOption();
            else if (input.GetKey(Keys.Enter) && SelectedMenuItem() is StateActionMenuItem)
                nextGameState = (SelectedMenuItem() as StateActionMenuItem).NextState;
            else if (input.GetKey(Keys.Enter) && SelectedMenuItem() is ActionMenuItem)
                (SelectedMenuItem() as ActionMenuItem).PerformAction();
            return nextGameState != GameState.None ? nextGameState : GameState.OptionsMenu;
        }

        private void ApplySettings()
        {
            OptionMenuItem<DisplayMode> resolution = menuItems.Find(t => t.Item1 == OptionID.Resolution).Item2 as OptionMenuItem<DisplayMode>;
            BoolOptionMenuItem fullscreen = menuItems.Find(t => t.Item1 == OptionID.Fullscreen).Item2 as BoolOptionMenuItem;
            BoolOptionMenuItem performanceMode = menuItems.Find(t => t.Item1 == OptionID.PerformanceMode).Item2 as BoolOptionMenuItem;

            GameSettings.Default.ResolutionWidth = resolution.SelectedValue().Width;
            GameSettings.Default.ResolutionHeight = resolution.SelectedValue().Height;
            GameSettings.Default.PerformanceMode = performanceMode.SelectedValue();
            GameSettings.Default.FullScreen = fullscreen.SelectedValue();
            GameSettings.Default.Save();

            graphics.PreferredBackBufferWidth = GameSettings.Default.ResolutionWidth;
            graphics.PreferredBackBufferHeight = GameSettings.Default.ResolutionHeight;
            if (graphics.IsFullScreen != GameSettings.Default.FullScreen)
                graphics.ToggleFullScreen();
            graphics.ApplyChanges();
        }

        private IMenuItem SelectedMenuItem()
        {
            return menuItems[selectedIndex].Item2;
        }
    }
}
