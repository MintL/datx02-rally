using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.Network
{
    interface ISimulationStrategy
    {
        bool UpdatePosition(Player player, Car car);
    }
}
