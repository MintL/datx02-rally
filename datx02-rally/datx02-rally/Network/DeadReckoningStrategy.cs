using System;
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

        public DeadReckoningStrategy() : this( new Vector3(1f, 1f, 1f) )
        {        }

        public bool UpdatePosition(Player player, Car car)
        {
            Player.PositionMessage latest = player.LastReceived;
            if (!latestMessageSeq.ContainsKey(player))
            {
                latestMessageSeq[player] = byte.MinValue;
            }

            // If new position available, update, else simulate
            if (latest.Sequence > latestMessageSeq[player] || 
                (latestMessageSeq[player] > byte.MaxValue-10 && latest.Sequence < byte.MinValue+10)) // handle wrap-around, very ugly
            {
                //Console.WriteLine("New msg! Setting player pos");
                player.Position = latest.Position;
                player.Rotation = latest.Rotation;
                player.Speed = latest.Velocity;
                latestMessageSeq[player] = latest.Sequence;

                // For local car, update simulated car as well
                if (player.LOCAL_PLAYER)
                {
                    simulatedLocalCar.Position = player.Position;
                    simulatedLocalCar.Rotation = player.Rotation;
                    simulatedLocalCar.Speed = player.Speed;
                    simulatedLocalCar.Update();
                }

                return false;
            }
            else
            {
                var simulatedPosition = player.Position;
                if (player.LOCAL_PLAYER)
                {
                    simulatedLocalCar.Update();
                    simulatedPosition = simulatedLocalCar.Position;
                }

                // Return whether car position differs from simulated position more than threshold
                Vector3 diff = (car.Position - simulatedPosition);
                if (Math.Abs(diff.X) > threshold.X || Math.Abs(diff.Y) > threshold.Y || Math.Abs(diff.Z) > threshold.Z)
                    return true;
                return false;
            }
        }
    }
}
