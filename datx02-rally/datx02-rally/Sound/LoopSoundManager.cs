using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace datx02_rally.Sound
{
    public class LoopSoundManager
    {
        AudioEngineManager AEM;

        List<Cue> playedSounds;

        //Store cues in a list
        //Can stop all sounds
        public LoopSoundManager()
        {
            playedSounds = new List<Cue>();

            AEM = AudioEngineManager.GetInstance();
        }

        public void AddNewSound(string cueName)
        {
            Cue cue = AEM.soundBank.GetCue(cueName);
            cue.Play();
            playedSounds.Add(cue);
        }

        public void StopAllSounds()
        {
            foreach (Cue cue in playedSounds)
            {
                cue.Stop(AudioStopOptions.AsAuthored);
            }
            playedSounds.Clear();
        }

    }
}
