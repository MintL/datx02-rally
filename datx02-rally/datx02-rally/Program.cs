using System;

namespace datx02_rally
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (GameManager game = new GameManager())
            {
                game.Run();
            }
        }
    }
#endif
}

