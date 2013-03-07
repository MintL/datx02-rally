using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace datx02_rally.Sound
{
    public class AudioEngineManager
    {
        public AudioEngine audioEngine;
        public SoundBank soundBank;
        public WaveBank waveBank;

        private static AudioEngineManager instance;


        public static AudioEngineManager GetInstance()
        {
            if (instance == null) instance = new AudioEngineManager();
            return instance;
        }

        private AudioEngineManager() 
        {
            //create and use appropriate XACT objects
            audioEngine = new AudioEngine("");
            soundBank = new SoundBank(audioEngine, "");
            waveBank = new WaveBank(audioEngine, "");
        }


        /// <summary>
        /// Plays a single sound once
        /// </summary>
        /// <param name="cueName">The sound to be played</param>
        public static void PlaySound(string cueName)
        {
            AudioEngineManager.GetInstance().soundBank.PlayCue(cueName);
        }


        /// <summary>
        /// Call once per update cycle to update what sounds to play
        /// </summary>
        public void Update() 
        {
            audioEngine.Update();
        }


    }
}
