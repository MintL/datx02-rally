using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace datx02_rally.DebugConsole.Commands
{
    class SetCommand : ICommand
    {
        public string Name
        {
            get { return "set"; }
        }
        private Game1 game;

        public string[] Description
        {
            get { return new string[] {"Set game variables"}; }
        }

        public SetCommand(Game1 game)
        {
            this.game = game;
        }

        public string[] Execute(string[] arguments)
        {
            List<string> output = new List<string>();
            if (arguments.Length > 1)
            {
                switch (arguments[1].ToLower())
                {
                    case "performancemode":
                        if (arguments.Length > 2)
                            GameSettings.Default.PerformanceMode = arguments[2] == "1";
                        break;
                    default:
                        output.Add("Unknown set command: "+String.Join(" ",arguments, 1, arguments.Length-1));
                        break;
                }
            }
            return output.ToArray();
        }
    }
}
