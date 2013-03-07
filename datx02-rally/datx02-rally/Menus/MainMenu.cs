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
    public class MainMenu : MenuView
    {
        public MainMenu(Game game) : base(game, GameState.MainMenu) { }

        protected override void LoadContent()
        {
            AddMenuItem(new StateActionMenuItem("Singleplayer", GameState.Gameplay));
            AddMenuItem(new StateActionMenuItem("Multiplayer", GameState.MultiplayerMenu));
            AddMenuItem(new StateActionMenuItem("Options", GameState.OptionsMenu));
            AddMenuItem(new StateActionMenuItem("Exit", GameState.Exiting));

            base.LoadContent();
        }
    }
}
