using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.EventTrigger
{
    public abstract class AbstractTrigger
    {
        public bool Active { get; protected set; }
        public TimeSpan Duration { get; protected set; }
        
        private TimeSpan activeTime;

        public AbstractTrigger(TimeSpan duration)
        {
            Active = false;
            Duration = duration;
        }

        public abstract void Update(GameTime gameTime, Vector3 position);

        public void Update(GameTime gameTime)
        {
            activeTime += gameTime.ElapsedGameTime;
            if (Active && activeTime > Duration)
                Active = false;
        }

        public void Trigger(TriggerData data)
        {
            Active = true;
        }

    }

    public struct TriggerData
    {
        public Vector3 Position;
        public TimeSpan Time;

        public TriggerData (Vector3 position, TimeSpan time)
        {
            Position = position;
            Time = time;
        }
    }
}
