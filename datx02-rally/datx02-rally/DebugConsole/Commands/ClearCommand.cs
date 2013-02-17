using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Microsoft.Xna.Framework;

namespace datx02_rally.DebugConsole.Commands
{
    class ClearCommand : ICommand
    {
        public string Name
        {
            get { return "clear"; }
        }

        public string[] Description
        {
            get { return new string[] {"Clear the output history"}; }
        }

        private HUDConsoleComponent console;

        public ClearCommand(HUDConsoleComponent console)
        {
            this.console = console;
        }

        public string[] Execute(string[] arguments)
        {
            console.ClearOutput();
            return new string[0];
        }

    }
}
