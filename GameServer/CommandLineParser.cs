using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer
{
    /// <summary>
    /// Very basic parser of command line arguments.
    /// Considers two forms of arguments, specified as such:
    /// 
    /// - Arguments preceeded by a identifier flag, e.g. program.exe -a foo
    /// - Simple flags without values, e.g. program.exe -b
    /// 
    /// </summary>
    class CommandLineParser
    {
        private readonly string[] ArgumentList;
        /// <summary>
        /// Constructor creating a parser for the specified
        /// argument list. The argument list is assumed to be
        /// formatted as in the standard Main(args) method. 
        /// </summary>
        /// <param name="arguments"></param>
        public CommandLineParser(string[] arguments)
        {
            this.ArgumentList = arguments;
        }

        /// <summary>
        /// Returns the value with the preceeding identifier, e.g.
        /// for "program.exe -a foo", GetArgument("a") returns
        /// the value "foo". 
        /// 
        /// Multiword values must be enclosed in quotes. Values 
        /// may not start with a dash.
        /// </summary>
        /// <param name="identifier">the identifier of which to get the value.</param>
        /// <returns>the value if it exists and is valid, null otherwise.</returns>
        public string GetArgument(string identifier)
        {
            return null;
        }
    }
}
