using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Microsoft.Xna.Framework;

namespace datx02_rally.DebugConsole.Commands
{
    class HelpCommand : ICommand
    {
        public string Name
        {
            get { return "help"; }
        }

        public string[] Description
        {
            get { return new string[] {"Display help about the specified command"}; }
        }

        private HUDConsoleComponent console;

        public HelpCommand(HUDConsoleComponent console)
        {
            this.console = console;
        }

        public string[] Execute(string[] arguments)
        {
            List<string> output = new List<string>();
            if (arguments.Length > 1)
            {
                ICommand command = console.Commands.Find(x => x.Name.ToLower().Equals(arguments[1]));
                if (command != null)
                {
                    //output.Add(command.Name + ":");
                    output.AddRange(command.Description);
                }
                else
                {
                    output.Add("Couldn't find command " + arguments[1]);
                }
            }
            else
            {
                output.Add("Usage: help <command>");
            }
            return output.ToArray();
        }

    }
}
