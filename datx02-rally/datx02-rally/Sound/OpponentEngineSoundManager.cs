using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace datx02_rally.Sound
{
    public class MovingCueEmitter : CueEmitter
    {

        public MovingCueEmitter(Cue sound, Vector3 position) : base(sound, position)
        {
        }

        public void SetEmitterPosition(Vector3 position)
        {
            audioEmitter.Position = position;
        }

        public void SetEmitterVelocity(Vector3 velocity)
        {
            audioEmitter.Velocity = velocity;
        }

        public void SetCue(Cue sound)
        {
            cue = sound;
        }
    }
    
    
    class OpponentEngineSoundManager
    {
        AudioListener player;

        AudioEngineManager AEM;

        Dictionary<string, MovingCueEmitter> opponentEngines;


        //Only one listener, several emitters
        //Store emitters in a list?
        //Store listener as a single variable in the class
        //Update receives emitters current position and velocity
        //

        public OpponentEngineSoundManager(Vector3 position, Vector3 velocity) 
        {
            player = new AudioListener();
            player.Position = position;
            player.Velocity = velocity;

            opponentEngines = new Dictionary<string, MovingCueEmitter>();

            AEM = AudioEngineManager.GetInstance();
        }

        public void AddNewSound(string playerId, string cueName, Vector3 position)
        {
            Cue cue = AEM.soundBank.GetCue(cueName);

            opponentEngines.Add(playerId, new MovingCueEmitter(cue, position));
        }

        public void Update(Vector3 position, Vector3 velocity)
        {
            player.Position = position;
            player.Velocity = velocity;

            foreach (CueEmitter sound in opponentEngines.Values) 
            {
                sound.GetCue().Apply3D(player, sound.GetEmitter());

                //add dopplereffect and possibly other things
            }
        }

        public void UpdatePlayer(string playerId, Vector3 position, Vector3 velocity)
        {
            if (opponentEngines.ContainsKey(playerId))
            {
                opponentEngines[playerId].SetEmitterPosition(position);
                opponentEngines[playerId].SetEmitterVelocity(velocity);

                //update the cue used based on current velocity
            }
        }

        public void RemovePlayer(string playerId)
        {

            if (opponentEngines.ContainsKey(playerId))
            {
                opponentEngines.Remove(playerId);
            }
        }

    }
}
