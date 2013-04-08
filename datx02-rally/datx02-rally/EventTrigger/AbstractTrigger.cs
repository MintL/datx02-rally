using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using datx02_rally.GameLogic;
using datx02_rally.Entities;

namespace datx02_rally.EventTrigger
{
    public abstract class AbstractTrigger
    {
        public bool Enabled { get; protected set; }
        public CurveRasterization Curve { get; protected set; }

        protected int currentPoint = 0;

        public event EventHandler<EventArgs> Triggered;

        //public TimeSpan Duration { get; protected set; }
        //private TimeSpan activeTime;

        public AbstractTrigger()
        {
            Enabled = true;
        }

        public AbstractTrigger(CurveRasterization curve) : this()
        {
            Curve = curve;
        }

        public abstract void Update(IMovingObject movingObject) ;

        protected void Trigger()
        {
            Triggered(this, EventArgs.Empty);
        }
    }

    //public struct TriggerData
    //{
    //    public Vector3 Position;
    //    public TimeSpan Time;

    //    public TriggerData (Vector3 position, TimeSpan time)
    //    {
    //        Position = position;
    //        Time = time;
    //    }
    //}
}
