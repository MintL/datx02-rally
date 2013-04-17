using datx02_rally.EventTrigger;
using datx02_rally.Menus;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using datx02_rally.GameLogic;
using datx02_rally.Components;
using datx02_rally.GameplayModes;

namespace datx02_rally
{
    class SimpleRaceMode : GameplayMode
    {
        private int laps;
        private int checkpoints;
        private RaceTrack raceTrack;
        private Car car;
        public int PlayerPosition { get; set; }

        private List<TimeSpan> goalLineTimes = new List<TimeSpan>();

        public SimpleRaceMode(Game1 gameInstance, int laps, int noOfCheckpoints, RaceTrack raceTrack, Car localCar)
            : base(gameInstance)
        {
            this.laps = laps;
            this.checkpoints = noOfCheckpoints;
            this.raceTrack = raceTrack;
            this.car = localCar;
            PlayerPosition = 1;
            Initialize();
        }

        public override void Initialize()
        {
            var trackRasterization = raceTrack.GetCurveRasterization(checkpoints);

            List<AbstractTrigger> checkpointTriggers = new List<AbstractTrigger>();
            for (int i = 0; i < checkpoints; i++)
            {
                PositionTrigger trigger = new PositionTrigger(trackRasterization, i, true, true);
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

            // Keep track of times when passing goal line
            var goalLine = checkpointTriggers[0];
            goalLine.Triggered += (sender, e) =>
            {
                goalLineTimes.Add(e.Time.TotalGameTime);
            };

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

        public override void PrepareStatistics()
        {
            TimeSpan totalTime = goalLineTimes[goalLineTimes.Count - 1] - goalLineTimes[0];

            var playerHeading = new EndGameStatistics.Heading();
            playerHeading.Title = null;
            playerHeading.Items[PlayerPosition + ". " + gameInstance.GetService<Player>().PlayerName] = totalTime.ToString(@"m\:ss\:ff");

            var lapsHeading = new EndGameStatistics.Heading();
            lapsHeading.Title = "Your times";
            for (int i = 1; i < goalLineTimes.Count; i++)
            {
                var lapTime = goalLineTimes[i] - goalLineTimes[i - 1];
                lapsHeading.Items["Lap " + i] = lapTime.ToString(@"m\:ss\:ff");
            }

            var statsHeading = new EndGameStatistics.Heading();
            statsHeading.Title = "Statistics";
            statsHeading.Items["You suck"] = null;

            var itemList = new List<EndGameStatistics.Heading> { playerHeading, lapsHeading, statsHeading };

            bool won = PlayerPosition == 1;
            Statistics = new EndGameStatistics(itemList, won);
        }


    }
}
