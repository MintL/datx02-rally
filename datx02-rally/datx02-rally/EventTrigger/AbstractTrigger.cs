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

        protected int currentPoint = -1;

        public event EventHandler<TriggeredEventArgs> Triggered;

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

        protected void Trigger(int index)
        {
            if (Triggered != null)
                Triggered(this, new TriggeredEventArgs(index));
        }
    }

    public class TriggeredEventArgs : EventArgs
    {
        public int Index { get; private set; }

        public TriggeredEventArgs(int index)
        {
            Index = index;
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
