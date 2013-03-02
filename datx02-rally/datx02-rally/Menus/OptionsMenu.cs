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
    public class OptionsMenu : OverlayView
    {

        public OptionsMenu(Game game)
            : base(game, GameState.OptionsMenu)
        {
            Vector2 size = GetScreenPosition(new Vector2(0.6f, 0.6f));
            Bounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);
        }

        protected override void LoadContent()
        {
            /*OptionMenuItem<DisplayMode> resolution = new OptionMenuItem<DisplayMode>("Resolution", "Resolution");
            foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                resolution.AddOption(mode.Width + "x" + mode.Height, mode);
            resolution.SetStartOption(Game1.GetInstance().GraphicsDevice.Viewport.Width + "x" + GraphicsDevice.Viewport.Height);

            BoolOptionMenuItem fullscreen = new BoolOptionMenuItem("Fullscreen").SetStartOption(graphics.IsFullScreen) as BoolOptionMenuItem;
            BoolOptionMenuItem performanceMode = new BoolOptionMenuItem("Performance mode", "PerfMode").SetStartOption(GameSettings.Default.PerformanceMode) as BoolOptionMenuItem;
            ActionMenuItem applyButton = new ActionMenuItem("Apply", new ActionMenuItem.Action(ApplySettings));
            StateActionMenuItem backButton = new StateActionMenuItem("Back", GameState.MainMenu);

            AddMenuItem(resolution);
            AddMenuItem(fullscreen);
            AddMenuItem(performanceMode);
            AddMenuItem(applyButton);
            AddMenuItem(backButton);*/

            base.LoadContent();

            List<Tuple<String, GameState>> itemInfo = new List<Tuple<string, GameState>>();
            itemInfo.Add(new Tuple<String, GameState>("MainMenu", GameState.MainMenu));
            itemInfo.Add(new Tuple<String, GameState>("OptionsMenu", GameState.OptionsMenu));
            
            foreach (var info in itemInfo)
            {
                MenuItem item = new StateActionMenuItem(info.Item1, info.Item2);
                item.Background = ButtonBackground;
                item.Font = MenuFont;
                AddMenuItem(item);
            }

            
        }

        private void ApplySettings()
        {
            OptionMenuItem<DisplayMode> resolution = GetMenuItem("Resolution") as OptionMenuItem<DisplayMode>;
            BoolOptionMenuItem fullscreen = GetMenuItem("Fullscreen") as BoolOptionMenuItem;
            BoolOptionMenuItem performanceMode = GetMenuItem("PerfMode") as BoolOptionMenuItem;

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
    }
}
