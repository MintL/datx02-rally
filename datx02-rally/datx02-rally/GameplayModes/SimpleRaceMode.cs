using datx02_rally.EventTrigger;
using datx02_rally.Menus;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using datx02_rally.GameLogic;
using datx02_rally.Components;
using datx02_rally.GameplayModes;
using datx02_rally.Sound;
using System.Timers;
using datx02_rally.Entities;

namespace datx02_rally
{
    class SimpleRaceMode : GameplayMode
    {
        private int laps;
        private int checkpoints;
        private RaceTrack raceTrack;
        private Car car;
        protected List<Player> players = new List<Player>();
        public int PlayerPlace { get; set; }
        public TimeSpan TotalRaceTime { get; private set; }
        private bool countdown = false;
        protected CurveRasterization checkpointRasterization;
        protected CurveRasterization placementRasterization;
        private DateTime lastCheckpointPass = DateTime.MinValue;

        private List<TimeSpan> goalLineTimes = new List<TimeSpan>();

        public SimpleRaceMode(GameManager gameInstance, int laps, int noOfCheckpoints, RaceTrack raceTrack, Car localCar)
            : base(gameInstance)
        {
            this.laps = laps;
            this.checkpoints = noOfCheckpoints;
            this.raceTrack = raceTrack;
            this.car = localCar;
            this.placementRasterization = raceTrack.GetCurveRasterization(100);
            PlayerPlace = 1;
            GameStarted = false;
            TotalRaceTime = TimeSpan.Zero;

            var player = gameInstance.GetService<Player>();
            player.Lap = 0;
            players.Add(player);

            Initialize();
        }

        public override void Initialize()
        {
            checkpointRasterization = raceTrack.GetCurveRasterization(checkpoints);

            List<AbstractTrigger> checkpointTriggers = new List<AbstractTrigger>();
            for (int i = 0; i < checkpoints; i++)
            {
                PositionTrigger trigger = new PositionTrigger(checkpointRasterization, i, true, true);
                string outputDebug = "Passing checkpoint " + i;
                trigger.Triggered += (sender, e) =>
                {
                    Console.WriteLine(outputDebug);
                    
                    var now = DateTime.Now;
                    if (lastCheckpointPass != DateTime.MinValue)
                        gameInstance.GetService<HUDComponent>().ShowTextNotification(Color.BurlyWood, (now - lastCheckpointPass).ToString(@"m\:ss\:ff"));
                    lastCheckpointPass = now;

                    var current = states[CurrentState];
                    var aTrigger = sender as AbstractTrigger;
                    if (e.Object == car && current.Triggers.ContainsKey(aTrigger))
                        current.Triggers[aTrigger] = e;
                    AudioEngineManager.PlaySound("passfinishline");
                };

                string checkpointID = "checkpoint" + i;
                gameInstance.GetService<TriggerManager>().Triggers.Add(checkpointID, trigger);
                checkpointTriggers.Add(trigger);
                addedTriggers.Add(checkpointID);
            }

            // Keep track of times when passing goal line
            var goalTrigger = checkpointTriggers[0];
            goalTrigger.Triggered += (sender, e) =>
            {
                goalLineTimes.Add(e.Time.TotalGameTime);
                var player = gameInstance.GetService<Player>();
                string notification = (++player.Lap) > laps ? "Race finished!" : "Lap " + (player.Lap);
                gameInstance.GetService<HUDComponent>().ShowTextNotification(Color.Teal, notification);
            };
            
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
            goalLineTimes[0] = StartTime;
            TotalRaceTime = goalLineTimes[goalLineTimes.Count - 1] - goalLineTimes[0];
            players.Find(p => p.LOCAL_PLAYER).RaceTime = TotalRaceTime;

            var playerHeading = new EndGameStatistics.Heading();
            playerHeading.Title = null;
            playerHeading.Updateable = true;
            foreach (var player in players)
                playerHeading.Items[PlayerPlace + ". " + player.PlayerName] = TotalRaceTime.ToString(@"m\:ss\:ff");

            var lapsHeading = new EndGameStatistics.Heading();
            lapsHeading.Title = "Your times";
            for (int i = 1; i < goalLineTimes.Count; i++)
            {
                var lapTime = goalLineTimes[i] - goalLineTimes[i - 1];
                lapsHeading.Items["Lap " + i] = lapTime.ToString(@"m\:ss\:ff");
            }

            /*var statsHeading = new EndGameStatistics.Heading();
            statsHeading.Title = "Statistics";
            statsHeading.Items["You suck"] = null;*/

            var itemList = new List<EndGameStatistics.Heading> { playerHeading, lapsHeading/*, statsHeading*/ };

            bool won = PlayerPlace == 1;
            Statistics = new EndGameStatistics(itemList, won);
        }

        public override void Update(GameTime gameTime, GamePlayView gamePlay)
        {
            if (StartTime == TimeSpan.Zero && GameStarted)
                StartTime = gameTime.TotalGameTime;
            if (Mode == Mode.Singleplayer && !countdown && gamePlay.Initialized)
            {
                countdown = true;
                StartCountdown();
            }
            /*Console.Write("players: ");
            foreach (var player in players)
            {
                Console.Write(player.PlayerName + " (" + player.Lap + "),");
            }
            Console.WriteLine();*/
            PlayerPlace = CalculatePlayerPosition();
            gameInstance.GetService<HUDComponent>().SetPlayerPosition(PlayerPlace);
            base.Update(gameTime, gamePlay);
        }

        private void StartCountdown()
        {
            string[] countdownStr = { "3", "2", "1", "Go!" };
            var hudComponent = gameInstance.GetService<HUDComponent>();
            for (int i = 0; i <= 3; i++)
            {
                int iCountdown = i;
                string countdown = countdownStr[iCountdown];
                Timer timer = new Timer(i * 1000 + 2000);
                timer.AutoReset = false;
                timer.Elapsed += (s, e) =>
                {
                    hudComponent.ShowTextNotification(Color.AliceBlue, countdown, TimeSpan.FromSeconds(0.8));
                    if (iCountdown == 3)
                        this.GameStarted = true;
                    this.CountDownState = iCountdown;
                };
                timer.Start();
            }
        }

        private int CalculatePlayerPosition()
        {
            var localPlayer = players.Find(p => p.LOCAL_PLAYER);
            var localCar = gameInstance.GetService<CarControlComponent>().Cars[localPlayer];
            var lDist = CalculateClosestPoint(localCar);
            int lClosestPositionIndex = lDist.Item1;
            float lDistanceToClosestPosition = lDist.Item2;

            List<Player> playersBefore = new List<Player>();

            var remotePlayers = players.FindAll(p => !p.LOCAL_PLAYER);
            foreach (var player in remotePlayers)
            {
                if (player.Lap > localPlayer.Lap)
                    playersBefore.Add(player);
                else if (player.Lap == localPlayer.Lap)
                {
                    var car = gameInstance.GetService<CarControlComponent>().Cars[player];
                    var dist = CalculateClosestPoint(car);
                    int closestPositionIndex = dist.Item1;
                    float distanceToClosestPosition = dist.Item2;
                    if (closestPositionIndex > lClosestPositionIndex ||
                        (closestPositionIndex == lClosestPositionIndex && distanceToClosestPosition > lDistanceToClosestPosition))
                        playersBefore.Add(player);
                }

            }
            return playersBefore.Count+1;
        }

        Tuple<int, float> cachedClosestPoint = new Tuple<int,float>(0, 0);
        private Tuple<int, float> CalculateClosestPoint(Car car)
        {
            var orderedPoints = placementRasterization.Points.OrderBy(point => Vector3.DistanceSquared(car.Position, point.Position));
            var closestPoint = orderedPoints.ElementAt(0);
            int closestIndex = placementRasterization.Points.IndexOf(closestPoint);
            var nextClosestPoint = orderedPoints.ElementAt(1);
            int nextClosestIndex = placementRasterization.Points.IndexOf(nextClosestPoint);

            int newClosestIndex = closestIndex;
            float newClosestDistance = Vector3.DistanceSquared(car.Position, closestPoint.Position);
            if (Math.Abs(closestIndex - nextClosestIndex) > 1)
            {
                // cases too god damn annoying, don't have time to code, just return same as before
                //return cachedClosestPoint;
                return new Tuple<int, float>(int.MaxValue, 0f);
                /*var thirdClosest = placementRasterization.Points.IndexOf(orderedPoints.ElementAt(3));
                Console.WriteLine("WOW, ABOUT TO LAP, closest: " + closestIndex + "(" + newClosestDistance + "), nextclosest: " + nextClosestIndex + "(" + Vector3.DistanceSquared(car.Position, nextClosestPoint.Position) + "), third:" + thirdClosest);
                if (Math.Abs(thirdClosest - closestIndex) > 2)//Vector3.DistanceSquared(closestPoint.Position, thirdClosest.Position) > Vector3.DistanceSquared(nextClosestPoint.Position, thirdClosest.Position))
                {
                    newClosestIndex = nextClosestIndex;
                    newClosestDistance = Vector3.DistanceSquared(car.Position, nextClosestPoint.Position);
                }*/
            }
            cachedClosestPoint = new Tuple<int, float>(newClosestIndex, newClosestDistance);
            return cachedClosestPoint;

        }

        private bool IsInFrontOf(Vector3 position, CurveRasterization.CurvePoint point)
        {
            var relativePosition = Vector3.Normalize(position - point.Position);

            return Vector3.Dot(relativePosition, point.Heading) > 0;
        }

    }
}
