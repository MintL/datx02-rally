using datx02_rally.EventTrigger;
using datx02_rally.Menus;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace datx02_rally
{
    class TimeTrialMode : GameplayMode
    {
        private int laps;
        private List<Vector3[]> checkpoints;
        Vector3[] goalLine;

        public TimeTrialMode(Game1 gameInstance, int laps, List<Vector3[]> checkpoints, Vector3[] goalLine)
            : base(gameInstance)
        {
            this.laps = laps;
            this.checkpoints = checkpoints;
            this.goalLine = goalLine;
        }

        public override void Initialize()
        {
            List<PlayerWrapperTrigger> checkpointTriggers = new List<PlayerWrapperTrigger>();
            PlayerWrapperTrigger goalTrigger = new PlayerWrapperTrigger(
                    new RectangleTrigger(goalLine[0], goalLine[1], goalLine[2], goalLine[3], new TimeSpan(0, 0, 5)),
                    (gameInstance.currentView as GamePlayView).Car);

            foreach (var checkpoint in checkpoints)
            {
                checkpointTriggers.Add(new PlayerWrapperTrigger(
                    new RectangleTrigger(checkpoint[0], checkpoint[1], checkpoint[2], checkpoint[3], new TimeSpan(0, 0, 5)),
                    (gameInstance.currentView as GamePlayView).Car));
            }

            // Starting state, waiting for countdown/start signal from server
            //const int countdown = 3;
            //for (int i = 0; i < countdown; i++)
            //{

            //}

            // pass start line state
            states.Add(new GameModeState(new List<PlayerWrapperTrigger> { goalTrigger }));

            for (int i = 0; i < laps; i++)
            {
                List<PlayerWrapperTrigger> lapTriggers = new List<PlayerWrapperTrigger>();
                lapTriggers.AddRange(checkpointTriggers);
                lapTriggers.Add(goalTrigger);

                states.Add(new GameModeState(lapTriggers));
            }
        }
    }
}
