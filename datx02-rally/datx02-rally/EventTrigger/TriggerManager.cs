using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.EventTrigger
{
    public class TriggerManager : GameComponent
    {
        public Dictionary<string, AbstractTrigger> Triggers = new Dictionary<string, AbstractTrigger>();
        
        public TriggerManager(Game game)
            : base(game)
        {
        }

        public override void Update(GameTime gameTime)
        {

        }

        //public void Update(GameTime gameTime, Vector3 position)
        //{
        //    foreach (AbstractTrigger trigger in Triggers.Values)
        //    {
        //        trigger.Update(gameTime, position);
        //    }
        //}

        //public void CreatePositionTrigger(string name, Vector3 position, float distance, TimeSpan duration)
        //{
        //    Triggers.Add(name, new PositionTrigger(position, distance, duration));
        //}

        //public void CreateRectangleTrigger(string name, Vector3 cornerOne, Vector3 cornerTwo, Vector3 cornerThree, Vector3 cornerFour, TimeSpan duration)
        //{
        //    Triggers.Add(name, new RectangleTrigger(cornerOne, cornerTwo, cornerThree, cornerFour, duration));
        //}

        //public bool IsActive(string name)
        //{
        //    if (Triggers.ContainsKey(name))
        //    {
        //        return Triggers[name].Active;
        //    }
        //    return false;
        //}
    }
}
