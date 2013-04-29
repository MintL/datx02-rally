﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.Network
{
    class DeadReckoningStrategy : ISimulationStrategy
    {
        public Dictionary<Player, byte> latestMessageSeq = new Dictionary<Player, byte>();
        private Car simulatedLocalCar = new Car(null, 13.4631138f);
        Vector3 threshold;

        public DeadReckoningStrategy(Vector3 threshold)
        {
            this.threshold = threshold;
        }

        public DeadReckoningStrategy() : this( new Vector3(0.01f, 0.01f, 0.01f) )
        {        }

        public bool UpdatePosition(Player player, Car car)
        {
            Player.PositionMessage latest = player.LastReceived;
            if (!latestMessageSeq.ContainsKey(player))
            {
                latestMessageSeq[player] = byte.MinValue;
            }

            Car simCar;
            if (player.LOCAL_PLAYER)
                simCar = simulatedLocalCar;
            else
                simCar = car;

            // If new position available, update, else simulate
            if (latest.Sequence != latestMessageSeq[player]/* || 
                (latestMessageSeq[player] > byte.MaxValue-10 && latest.Sequence < byte.MinValue+10)*/) // handle wrap-around, very ugly
            {
                //Console.WriteLine("New msg! Setting player pos");
                simCar.Position = latest.Position;
                simCar.Rotation = latest.Rotation;
                simCar.Speed = latest.Velocity;
                latestMessageSeq[player] = latest.Sequence;
                simCar.Update();

                return false;
            }
            else
            {
                simCar.Update();

                // Return whether car position differs from simulated position more than threshold
                Vector3 diff = (car.Position - simCar.Position);
                if (player.LOCAL_PLAYER && Math.Abs(diff.X) > threshold.X || 
                    Math.Abs(diff.Y) > threshold.Y || Math.Abs(diff.Z) > threshold.Z)
                    return true;
                return false;
            }
        }
    }
}
