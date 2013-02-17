using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


namespace datx02_rally.DebugConsole
{
    interface ICommand
    {
        /// <summary>
        /// The name of the command. 
        /// What the user types in to use the command.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The description that is displayed when using command "help".
        /// </summary>
        string[] Description { get; }

        /// <summary>
        /// This is what the command does. The returned value is outputted to the console.
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        string[] Execute(string[] arguments);
    }
}
