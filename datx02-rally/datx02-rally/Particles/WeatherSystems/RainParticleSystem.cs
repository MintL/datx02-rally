using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Particle3DSample;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally.Particles.WeatherSystems
{
    class RainParticleSystem : ParticleSystem
    {
        public RainParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = @"Particles/rainDrop";

            settings.MaxParticles = 10000;

            settings.Duration = TimeSpan.FromSeconds(.6f);

            settings.Gravity = 500 * Vector3.Down;

            settings.MinHorizontalVelocity = 50;
            settings.MaxHorizontalVelocity = 70;

            settings.MinVerticalVelocity = -200;
            settings.MaxVerticalVelocity = -200;


            //settings.EndVelocity = 0.75f;

            //settings.MinRotateSpeed = -1;
            //settings.MaxRotateSpeed = 1;

            //settings.MinStartSize = 50;
            //settings.MaxStartSize = 100;

            //settings.MinEndSize = 0;
            //settings.MaxEndSize = 150;

            // All same
            settings.MinStartSize = settings.MaxStartSize = settings.MinEndSize = settings.MaxEndSize = 3.5f;

            //settings.MinColor = settings.MaxColor = new Color(.15f, .1f, .6f);

            settings.BlendState = BlendState.AlphaBlend;
        }
    }
}
