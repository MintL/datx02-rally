using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using datx02_rally.GameLogic;
using Microsoft.Xna.Framework;
using datx02_rally.Menus;
using datx02_rally.EventTrigger;
using datx02_rally.Entities;

namespace datx02_rally.GameplayModes
{
    class MultiplayerRaceMode : SimpleRaceMode
    {
        public MultiplayerRaceMode(Game1 gameInstance, int laps, int noOfCheckpoints, RaceTrack raceTrack, Car localCar)
            : base(gameInstance, laps, noOfCheckpoints, raceTrack, localCar)
        {
            this.Mode = Mode.Multiplayer;
            players.AddRange(gameInstance.GetService<ServerClient>().Players.Values);
            AddLapTriggers();
        }

        public void AddLapTriggers()
        {
            // add lap counter triggers at finish line
            foreach (var player in players.FindAll(p => !p.LOCAL_PLAYER))
            {
                var lPlayer = player;
                var car = gameInstance.GetService<CarControlComponent>().Cars[lPlayer];
                PositionTrigger trigger = new PositionTrigger(checkpointRasterization, 0, true, true);
                trigger.Triggered += (sender, e) =>
                {
                    lPlayer.Lap++;
                };
                Tuple<AbstractTrigger, List<IMovingObject>> objTrigger =
                    new Tuple<AbstractTrigger, List<IMovingObject>>(trigger, new List<IMovingObject> { car });
                string id = "lapT" + lPlayer.PlayerName;
                gameInstance.GetService<TriggerManager>().ObjectTriggers.Add(id, objTrigger);
                addedObjTriggers.Add(id);
            }
        }

        public override void PrepareStatistics()
        {
            base.PrepareStatistics();
        }

        public override void Update(GameTime gameTime, GamePlayView gamePlay)
        {
            if (TotalRaceTime != TimeSpan.Zero && allStatesFinished)
                gameInstance.GetService<ServerClient>().SendRaceTime(TotalRaceTime);
            if (Statistics != null) {
                var playerHeading = Statistics.CategorizedItems.First(h => h.Updateable);
                playerHeading.Items.Clear();
                int place = 1;
                foreach (var player in players.OrderBy(p => p.RaceTime))
                    playerHeading.Items[place++ + ". " + player.PlayerName] = player.RaceTime == TimeSpan.MaxValue ? "" : player.RaceTime.ToString(@"m\:ss\:ff");
            }
            base.Update(gameTime, gamePlay);
        }
    }
}
