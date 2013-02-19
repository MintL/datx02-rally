﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Particle3DSample;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally.Particles.WeatherSystems
{
    class ThunderParticleSystem : ParticleSystem
    {
        public ThunderParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = @"Particles/plasma";

            settings.MaxParticles = 25000;

            settings.Duration = TimeSpan.FromSeconds(3.25f);

            settings.MinHorizontalVelocity = 5;
            settings.MaxHorizontalVelocity = 10;

            settings.MinVerticalVelocity = 5;
            settings.MaxVerticalVelocity = 15;

            settings.EndVelocity = 0.75f;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            //settings.MinStartSize = 50;
            //settings.MaxStartSize = 100;

            //settings.MinEndSize = 0;
            //settings.MaxEndSize = 150;

            // All same
            settings.MinStartSize = settings.MaxStartSize = settings.MinEndSize = settings.MaxEndSize = 50;

            settings.MinColor = settings.MaxColor = new Color(.15f, .1f, .6f);

            settings.BlendState = BlendState.Additive;
        }
    }
}
