using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace datx02_rally
{
    class UniversalRandom
    {
        private static int currentSeed = (int)DateTime.Now.Ticks;
        private static Random instance = new Random(currentSeed);

        public static int CurrentSeed { get { return currentSeed; } }

        public static void ResetInstance()
        {
            ResetInstance(currentSeed);
        }

        public static void ResetInstance(int seed)
        {
            instance = new Random(currentSeed = seed);
        }

        public static Random GetInstance()
        {
            return instance;
        }
    }
}
