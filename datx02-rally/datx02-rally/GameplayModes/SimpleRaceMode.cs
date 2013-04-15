using datx02_rally.EventTrigger;
using datx02_rally.Menus;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using datx02_rally.GameLogic;
using datx02_rally.Components;

namespace datx02_rally
{
    class SimpleRaceMode : GameplayMode
    {
        private int laps;
        private int checkpoints;
        private RaceTrack raceTrack;
        private Car car;

        public SimpleRaceMode(Game1 gameInstance, int laps, int noOfCheckpoints, RaceTrack raceTrack, Car localCar)
            : base(gameInstance)
        {
            this.laps = laps;
            this.checkpoints = noOfCheckpoints;
            this.raceTrack = raceTrack;
            this.car = localCar;
            Initialize();
        }

        public override void Initialize()
        {
            List<AbstractTrigger> checkpointTriggers = new List<AbstractTrigger>();
            for (int i = 0; i < checkpoints; i++)
            {
                PositionTrigger trigger = new PositionTrigger(raceTrack.CurveRasterization, 
                    (int)(((float)i / checkpoints) * raceTrack.CurveRasterization.Points.Count), true, true);
                string outputDebug = "Passing checkpoint " + i;
                trigger.Triggered += (sender, e) =>
                {
                    Console.WriteLine(outputDebug);
                    gameInstance.GetService<HUDComponent>().ShowTextNotification(Color.Red, outputDebug);
                    var current = states[CurrentState];
                    var aTrigger = sender as AbstractTrigger;
                    if (e.Object == car && current.Triggers.ContainsKey(aTrigger))
                        current.Triggers[aTrigger] = e;
                };
                
                string checkpointID = "checkpoint" + i;
                gameInstance.GetService<TriggerManager>().Triggers.Add(checkpointID, trigger);
                checkpointTriggers.Add(trigger);
                addedTriggers.Add(checkpointID);
            }

            //// Starting state, waiting for countdown/start signal from server
            //const int countdown = 3;
            //for (int i = 0; i < countdown; i++)
            //{
			     
            //}

            for (int i = 0; i < laps; i++)
            {
                List<AbstractTrigger> lapTriggers = new List<AbstractTrigger>();
                lapTriggers.AddRange(checkpointTriggers);

                states.Add(new GameModeState(lapTriggers));
            }

            // Add state for passing the finish line
            states.Add(new GameModeState(checkpointTriggers.GetRange(index: 0, count: 1)));
        }
    }
}
