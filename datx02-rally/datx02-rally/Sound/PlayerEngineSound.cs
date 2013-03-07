using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace datx02_rally.Sound
{

    public class PlayerEngineSound
    {
        private Cue currentSound;

        private float previousVelocity;

        private AudioEngineManager AEM;

        public PlayerEngineSound()
        {
            AEM = AudioEngineManager.GetInstance();
            previousVelocity = 0;

            currentSound = AEM.soundBank.GetCue("EngineIdle");
        }

        public void UpdateEngineSound(float currentVelocity) 
        {
            //Positive = accelerating, negative = deccelerating, 0 = maintained speed
            float acceleration = currentVelocity - previousVelocity;

            //double layered switch(?), based on first currentVelocity, and then current acceleration

            previousVelocity = currentVelocity;
        }


    }
}
