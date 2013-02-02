using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Particle3DSample;

namespace datx02_rally
{
    class PlasmaParticleSystem : Particle3DSample.ParticleSystem
    {
        public PlasmaParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = @"Particles/plasma";

            settings.MaxParticles = 6000;

            settings.Duration = TimeSpan.FromSeconds(2);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 15;

            settings.MinVerticalVelocity = 10;
            settings.MaxVerticalVelocity = 20;

            settings.EndVelocity = 0.75f;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 20;
            settings.MaxStartSize = 20;


            settings.MinEndSize = 20;
            settings.MaxEndSize = 20;

            settings.MinColor = settings.MaxColor = Color.White;
            //settings.MinColor = Color.LightBlue;
        }
    }
}
