using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            settings.TextureName = @"Particles/Rain2";

            settings.MaxParticles = 10000;

            settings.Duration = TimeSpan.FromSeconds(.8f);

            settings.Gravity = 50 * new Vector3(-0.8f, -1f, -0.8f);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;

            settings.MinVerticalVelocity = -65;
            settings.MaxVerticalVelocity = -160;


            //settings.EndVelocity = 0.75f;

            //settings.MinRotateSpeed = -1;
            //settings.MaxRotateSpeed = 1;

            //settings.MinStartSize = 50;
            //settings.MaxStartSize = 100;

            //settings.MinEndSize = 0;
            //settings.MaxEndSize = 150;

            // All same
            settings.MinStartSize = settings.MinEndSize = 3f;
            settings.MaxStartSize = settings.MaxEndSize = 5f;
            settings.MinColor = settings.MaxColor = new Color(.65f, .6f, 1f, .7f);

            settings.BlendState = BlendState.Additive;
        }
    }
}
