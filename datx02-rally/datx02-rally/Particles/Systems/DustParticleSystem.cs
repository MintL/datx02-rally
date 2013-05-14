using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally.Particles.Systems
{
    class DustParticleSystem : ParticleSystem
    {
        public DustParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = @"Particles/plasma";

            settings.MaxParticles = 4000;

            settings.Duration = TimeSpan.FromSeconds(.5f);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 15;

            settings.MinVerticalVelocity = 180;
            settings.MaxVerticalVelocity = 380;

            settings.Gravity = new Vector3(0, -10, 0);

            settings.EndVelocity = 0.75f;

            //settings.MinRotateSpeed = -1;
            //settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 15;
            settings.MaxStartSize = 30;

            settings.MinEndSize = 30;
            settings.MaxEndSize = 50;

            settings.MinColor = settings.MaxColor = new Color(91, 79, 62, 150);
            //settings.MinColor = settings.MaxColor = Color.Gray;

            settings.BlendState = BlendState.Additive;
        }
    }
}
