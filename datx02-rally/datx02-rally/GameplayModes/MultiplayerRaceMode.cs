using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using datx02_rally.GameLogic;

namespace datx02_rally.GameplayModes
{
    class MultiplayerRaceMode : SimpleRaceMode
    {
        public MultiplayerRaceMode(Game1 gameInstance, int laps, int noOfCheckpoints, RaceTrack raceTrack) 
            : base(gameInstance, laps, noOfCheckpoints, raceTrack)
        {
            this.Mode = Mode.Multiplayer;
        }
    }
}
