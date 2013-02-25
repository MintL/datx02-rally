using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace datx02_rally.Menus
{
    class MenuItem
    {
        public string Text { get; private set; }
        public GameState NextState { get; private set; }

        public MenuItem(string text) : this(text, null) { }

        public MenuItem(string text, GameState? nextState)
        {
            this.Text = text;
            this.NextState = nextState.HasValue ? nextState.Value : GameState.None;
        }

        

    }
}
