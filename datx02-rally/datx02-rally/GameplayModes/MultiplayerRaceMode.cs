using datx02_rally.EventTrigger;
using datx02_rally.Menus;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace datx02_rally
{
    class MultiplayerRaceMode : GameplayMode
    {
        private int laps;
        private List<Vector3> checkpoints;

        public MultiplayerRaceMode(Game1 gameInstance, int laps, List<Vector3> checkpoints)
            : base(gameInstance)
        {
            this.laps = laps;
            this.checkpoints = checkpoints;
        }

        public override void Initialize()
        {
            List<Player
            foreach (var item in collection)
            {
                
            }
            for (int i = 0; i < laps; i++)
            {
                foreach (var item in checkpoints)
                {
                    
                }
                //gameInstance.GetService<CarControlComponent>().Cars.C
                //PlayerWrapperTrigger[] triggers = new PlayerWrapperTrigger[];

            }
        }
    }
}
