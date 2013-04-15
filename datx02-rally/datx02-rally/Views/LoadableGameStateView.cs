using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;

namespace datx02_rally.Menus
{
    public abstract class LoadableGameStateView : GameStateView
    {
        private Thread loadingThread;
        public bool IsLoaded
        {
            get
            {
                return loadingThread.ThreadState == ThreadState.Stopped;
            }
        }

        public LoadableGameStateView(Game game, GameState gameState)
            : base(game, gameState)
        {
        }

        protected sealed override void LoadContent()
        {
            loadingThread = new Thread(Load);
            loadingThread.Start();
            base.LoadContent();
        }

        public abstract void Load();
    }
}
