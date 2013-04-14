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
    public class MainMenu : OverlayView
    {
        public MainMenu(Game game) : base(game, GameState.MainMenu) 
        {
            MenuTitle = "Main Menu";
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            Vector2 size = GetScreenPosition(new Vector2(0.3f, 0.6f));
            Bounds = new Rectangle(0, 0, (int)size.X, (int)size.Y);

            List<Tuple<String, GameState>> itemInfo = new List<Tuple<string,GameState>>();
            itemInfo.Add(new Tuple<String, GameState>("Play", GameState.Gameplay));
            itemInfo.Add(new Tuple<String, GameState>("Choose car", GameState.CarChooser));
            itemInfo.Add(new Tuple<String, GameState>("Multiplayer", GameState.MultiplayerMenu));
            itemInfo.Add(new Tuple<String, GameState>("Options", GameState.OptionsMenu));
            itemInfo.Add(new Tuple<String, GameState>("Exit", GameState.Exiting));
            
            foreach (var info in itemInfo) 
            {
                MenuItem item = new StateActionMenuItem(info.Item1, info.Item2);
                item.Background = ButtonBackground;
                item.Font = MenuFont;
                item.FontColor = ItemColor;
                item.FontColorSelected = ItemColorSelected;
                item.SetWidth(Bounds.Width);
                AddMenuItem(item);
            }

            
        }
    }
}
