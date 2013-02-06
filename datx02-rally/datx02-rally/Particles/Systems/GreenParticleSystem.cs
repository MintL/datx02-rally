using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Particle3DSample;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally
{
    class GreenParticleSystem : Particle3DSample.ParticleSystem
    {
        public GreenParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = @"Particles/plasma";

            settings.MaxParticles = 2000;

            settings.Duration = TimeSpan.FromSeconds(2);

            settings.MinHorizontalVelocity = 5;
            settings.MaxHorizontalVelocity = 10;

            settings.MinVerticalVelocity = 2;
            settings.MaxVerticalVelocity = 2;

            settings.EndVelocity = 0.75f;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 25;
            settings.MaxStartSize = 26;

            settings.MinEndSize = 20;
            settings.MaxEndSize = 25;

            settings.MinColor = settings.MaxColor = Color.Green;

            settings.BlendState = BlendState.Additive;
        }
    }
}
