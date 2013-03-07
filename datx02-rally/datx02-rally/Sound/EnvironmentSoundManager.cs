using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace datx02_rally.Sound
{
    public class CueEmitter
    {
        protected AudioEmitter audioEmitter;
        protected Cue cue;

        public CueEmitter(Cue sound, Vector3 position)
        {
            cue = sound;

            audioEmitter = new AudioEmitter();
            audioEmitter.Position = position;
            audioEmitter.Velocity = Vector3.Zero;
        }

        public AudioEmitter GetEmitter()
        {
            return audioEmitter;
        }

        public Cue GetCue()
        {
            return cue;
        }
    }

    public class EnvironmentSoundManager
    {
        AudioListener player;

        AudioEngineManager AEM;

        List<CueEmitter> environmentSounds;


        //Only one listener, several emitters
        //Store emitters in a list?
        //Store listener as a single variable in the class
        //Update receives emitters current position and velocity
        //

        public EnvironmentSoundManager(Vector3 position, Vector3 velocity) 
        {
            player = new AudioListener();
            player.Position = position;
            player.Velocity = velocity;

            environmentSounds = new List<CueEmitter>();

            AEM = AudioEngineManager.GetInstance();
        }

        public void AddNewSound(string cueName, Vector3 position)
        {
            Cue cue = AEM.soundBank.GetCue(cueName);

            environmentSounds.Add(new CueEmitter(cue, position));
        }

        public void Update(Vector3 position, Vector3 velocity)
        {
            player.Position = position;
            player.Velocity = velocity;

            foreach (CueEmitter sound in environmentSounds) 
            {
                sound.GetCue().Apply3D(player, sound.GetEmitter());

                //add dopplereffect and possibly other things
            }
        }

    }
}
