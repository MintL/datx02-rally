﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;
using datx02_rally.EventTrigger;
using datx02_rally.Menus;
using System;
using System.Timers;

namespace datx02_rally
{
    public enum Mode { Singleplayer, Multiplayer }
    abstract class GameplayMode
    {
        public Mode Mode { protected set; get; }
        protected Game1 gameInstance;
        protected List<GameModeState> states;
        protected List<string> addedTriggers;
        protected int CurrentState { get; private set; }
        private bool allStatesFinished = false;
        public bool GameOver { private set; get; }

        public GameplayMode(Game1 gameInstance)
        {
            this.gameInstance = gameInstance;
            this.Mode = Mode.Singleplayer;
            states = new List<GameModeState>();
            addedTriggers = new List<string>();
            GameOver = false;
            CurrentState = 0;
        }

        /// <summary>
        /// For initializing the states list
        /// </summary>
        public abstract void Initialize();

        public void Update(GameTime gameTime)
        {
            if (allStatesFinished)
                return;
            GameModeState current = states[CurrentState];
            if (current.IsStateFinished())
            {
                CurrentState++;
                Console.WriteLine("Passed from state " + (CurrentState - 1) + " to state " + CurrentState);
            }
            if (CurrentState > states.Count - 1)
            {
                Console.WriteLine("Game over!");
                GameOverProcedure();
            }
        }

        private void GameOverProcedure()
        {
            allStatesFinished = true;

            // remove triggers from trigger manager
            var triggerManager = gameInstance.GetService<TriggerManager>();
            var inputManager = gameInstance.GetService<InputComponent>();
            inputManager.InputEnabled = false;
            foreach (var trigger in addedTriggers)
            {
                if (triggerManager.Triggers.ContainsKey(trigger))
                    triggerManager.Triggers.Remove(trigger);
            }

            // disable keyboard for 3 seconds, then exit gameplay
            Timer timer = new Timer(3 * 1000);
            timer.Elapsed += (s, e) =>
            {
                inputManager.InputEnabled = true;
                GameOver = true;
            };
            timer.Start();
        }

    }


    public class GameModeState 
    {
        public Dictionary<AbstractTrigger, TriggeredEventArgs> Triggers = new Dictionary<AbstractTrigger, TriggeredEventArgs>();
        private bool finished = false;

        public GameModeState(List<AbstractTrigger> triggers) 
        {
            foreach (var trigger in triggers)
		        Triggers.Add(trigger, null);
        }

        //public void Update(GameTime gameTime) 
        //{
        //    foreach (var triggerPair in Triggers)
        //    {
        //        triggerPair.Key.Update(gameTime);
        //        if (triggerPair.Value == null && triggerPair.Key.Active)
        //            Triggers[triggerPair.Key] = new TriggerStatistics(gameTime);
        //    }
        //}

        public bool IsStateFinished()
        {
            if (finished)
                return true;

            foreach (var triggerStat in Triggers.Values)
            {
                if (triggerStat == null) 
                    return false;
            }
            return true;
        }
    }

    //public class TriggerStatistics
    //{
    //    private GameTime triggerTime;

    //    public TriggerStatistics(GameTime triggerTime)
    //    {
    //        this.triggerTime = triggerTime;
    //    }
    //}
}
