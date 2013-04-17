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
            MenuTitle = "Options";
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Vector2 size = GetScreenPosition(new Vector2(0.6f, 0.75f));
            Bounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);

            OptionMenuItem<Tuple<int, int>> resolution = new OptionMenuItem<Tuple<int, int>>("Resolution", "res");
            foreach (var res in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.Select(m => new Tuple<int,int>(m.Width, m.Height)).Distinct().OrderBy(r => r.Item1))
                resolution.AddOption(res.Item1 + "x" + res.Item2, res);
            resolution.SetStartOption(new Tuple<int, int>(gameInstance.Graphics.PreferredBackBufferWidth, gameInstance.Graphics.PreferredBackBufferHeight));
            resolution.Bounds = Bounds;
            resolution.Font = MenuFont;
            resolution.ArrowLeft = ArrowLeft;
            resolution.ArrowRight = ArrowRight;
            resolution.Background = OptionSelected;
            resolution.FontColor = ItemColor;
            resolution.FontColorSelected = Color.Black;
            AddMenuItem(resolution);
            
            OptionMenuItem<int> displayMode = new OptionMenuItem<int>("Display Mode", "display");
            displayMode.AddOption("Fullscreen", 1);
            displayMode.AddOption("Windowed", 2);
            displayMode.SetStartOption(gameInstance.Graphics.IsFullScreen ? 1 : 2);
            displayMode.Bounds = Bounds;
            displayMode.Font = MenuFont;
            displayMode.ArrowLeft = ArrowLeft;
            displayMode.ArrowRight = ArrowRight;
            displayMode.Background = OptionSelected;
            displayMode.FontColor = ItemColor;
            displayMode.FontColorSelected = Color.Black;
            AddMenuItem(displayMode);

            TextInputMenuItem playerName = new TextInputMenuItem("Player Name", "name");
            playerName.Bounds = Bounds;
            playerName.Font = MenuFont;
            playerName.EnteredText = GameSettings.Default.PlayerName;
            playerName.Background = OptionSelected;
            playerName.FontColor = ItemColor;
            playerName.FontColorSelected = Color.Black;
            AddMenuItem(playerName);

            BoolOptionMenuItem shadows = new BoolOptionMenuItem("Shadows", "shadows");
            shadows.Bounds = Bounds;
            shadows.Font = MenuFont;
            shadows.SetStartOption(GameSettings.Default.Shadows);
            shadows.ArrowLeft = ArrowLeft;
            shadows.ArrowRight = ArrowRight;
            shadows.Background = OptionSelected;
            shadows.FontColor = ItemColor;
            shadows.FontColorSelected = Color.Black;
            AddMenuItem(shadows);

            BoolOptionMenuItem bloom = new BoolOptionMenuItem("Bloom", "bloom");
            bloom.Bounds = Bounds;
            bloom.Font = MenuFont;
            bloom.SetStartOption(GameSettings.Default.Bloom);
            bloom.ArrowLeft = ArrowLeft;
            bloom.ArrowRight = ArrowRight;
            bloom.Background = OptionSelected;
            bloom.FontColor = ItemColor;
            bloom.FontColorSelected = Color.Black;
            AddMenuItem(bloom);

            MenuItem item = new StateActionMenuItem("Cancel", GameState.MainMenu);
            item.Background = ButtonBackground;
            item.Font = MenuFont;
            item.FontColor = ItemColor;
            item.FontColorSelected = ItemColorSelected;
            item.SetWidth(Bounds.Width);
            AddMenuItem(item);

            item = new ActionMenuItem("Apply", ApplySettings);
            item.Background = ButtonBackground;
            item.Font = MenuFont;
            item.FontColor = ItemColor;
            item.FontColorSelected = ItemColorSelected;
            item.SetWidth(Bounds.Width);
            AddMenuItem(item);
            
        }

        private void ApplySettings()
        {
            OptionMenuItem<Tuple<int, int>> resolution = GetMenuItem("res") as OptionMenuItem<Tuple<int, int>>;
            OptionMenuItem<int> displayMode = GetMenuItem("display") as OptionMenuItem<int>;
            TextInputMenuItem playerName = GetMenuItem("name") as TextInputMenuItem;
            BoolOptionMenuItem shadows = GetMenuItem("shadows") as BoolOptionMenuItem;
            BoolOptionMenuItem bloom = GetMenuItem("bloom") as BoolOptionMenuItem;

            GameSettings.Default.ResolutionWidth = resolution.SelectedValue().Item1;
            GameSettings.Default.ResolutionHeight = resolution.SelectedValue().Item2;
            GameSettings.Default.FullScreen = displayMode.SelectedValue() == 1;
            GameSettings.Default.PlayerName = playerName.EnteredText;
            GameSettings.Default.Shadows = shadows.SelectedValue();
            GameSettings.Default.Bloom = bloom.SelectedValue();
            GameSettings.Default.Save();

            graphics.PreferredBackBufferWidth = GameSettings.Default.ResolutionWidth;
            graphics.PreferredBackBufferHeight = GameSettings.Default.ResolutionHeight;
            if (graphics.IsFullScreen != GameSettings.Default.FullScreen)
                graphics.ToggleFullScreen();
            graphics.ApplyChanges();
        }
    }
}
