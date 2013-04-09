using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.Entities
{
    public interface IMovingObject
    {
        Vector3 Position { get; set; }
        Vector3 Heading { get; set; }

        float Speed { get; set; }
    }
}
