﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.EventTrigger
{
    class PlayerWrapperTrigger
    {
        private AbstractTrigger trigger;
        private Car player;
        public bool Active { 
            private set;
            get { return trigger.Active; }
        }

        public PlayerWrapperTrigger(AbstractTrigger trigger, Car player)
        {
            this.trigger = trigger;
            this.player = player;
        }

        public void Update(GameTime gameTime)
        {
            trigger.Update(gameTime, player.Position);
        }
    }
}
