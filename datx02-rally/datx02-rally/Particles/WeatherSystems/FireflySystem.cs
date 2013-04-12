using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally.Particles.WeatherSystems
{
    public class FireflySystem : ParticleSystem
    {
        public FireflySystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = @"Particles/Firefly";

            settings.MaxParticles = 10000;

            settings.Duration = TimeSpan.FromSeconds(4);

            settings.MinHorizontalVelocity = -20;
            settings.MaxHorizontalVelocity = 50;

            settings.MinVerticalVelocity = -20;
            settings.MaxVerticalVelocity = 50;

            settings.MinStartSize = 10f;
            settings.MinEndSize = 15f;

            settings.MaxStartSize = 10f;
            settings.MaxEndSize = 15f;

            settings.MinColor = new Color(.91f, .83f, .27f);
            settings.MaxColor = new Color(.98f, .97f, .19f);

            settings.BlendState = BlendState.AlphaBlend;
        }
    }
}
