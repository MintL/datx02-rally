using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally
{
    /// <summary>
    /// Static class to provide extensionmethods for game management. If you don't know what an extension method is, ask me. 
    /// /Marcus
    /// </summary>
    static class GameExtensions
    {
        public static T GetService<T>(this Game game)
        {
            return (T)game.Services.GetService(typeof(T));
        }
    }
}
