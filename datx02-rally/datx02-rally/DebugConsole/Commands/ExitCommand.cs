using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Microsoft.Xna.Framework;

namespace datx02_rally.DebugConsole.Commands
{
    class ExitCommand : ICommand
    {
        public string Name
        {
            get { return "exit"; }
        }

        public string[] Description
        {
            get { return new string[] {"Exit the game"}; }
        }

        private Game game;

        public ExitCommand(Game game)
        {
            this.game = game;
        }

        public string[] Execute(string[] arguments)
        {
            game.Exit();
            return new string[0];
        }

    }
}
