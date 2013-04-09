using Microsoft.Xna.Framework;
using System.Collections.Generic;
using datx02_rally.EventTrigger;
using datx02_rally.Menus;

namespace datx02_rally
{
    abstract class GameplayMode
    {
        protected Game1 gameInstance;
        protected List<GameModeState> states;
        private int currentState;
        public bool GameOver { private set; get; }

        public GameplayMode(Game1 gameInstance)
        {
            this.gameInstance = gameInstance;
            GameOver = false;
            currentState = 0;
            Initialize();
        }

        /// <summary>
        /// For initializing the states list
        /// </summary>
        public abstract void Initialize();

        public void Update(GameTime gameTime)
        {
            GameModeState current = states[currentState];
            current.Update(gameTime);
            if (current.IsStateFinished())
                currentState++;
            if (currentState > states.Count - 1)
                GameOver = true;
        }

    }


    public class GameModeState 
    {
        public Dictionary<PlayerWrapperTrigger, TriggerStatistics> Triggers = new Dictionary<PlayerWrapperTrigger, TriggerStatistics>();

        public GameModeState(List<PlayerWrapperTrigger> triggers) 
        {
            foreach (var trigger in triggers)
		        Triggers.Add(trigger, null);
        }

        public void Update(GameTime gameTime) 
        {
            foreach (var triggerPair in Triggers)
            {
                triggerPair.Key.Update(gameTime);
                if (triggerPair.Value == null && triggerPair.Key.Active)
                    Triggers[triggerPair.Key] = new TriggerStatistics(gameTime);
            }
        }

        public bool IsStateFinished()
        {
            foreach (var triggerStat in Triggers.Values)
            {
                if (triggerStat == null) 
                    return false;
            }
            return true;
        }
    }

    public class TriggerStatistics
    {
        private GameTime triggerTime;

        public TriggerStatistics(GameTime triggerTime)
        {
            this.triggerTime = triggerTime;
        }
    }
}
