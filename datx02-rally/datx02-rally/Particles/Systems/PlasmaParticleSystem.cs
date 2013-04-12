using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally.Particles.Systems
{
    class PlasmaParticleSystem : ParticleSystem
    {
        public PlasmaParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = @"Particles/plasma";

            settings.MaxParticles = 17500;

            //settings.Duration = TimeSpan.FromSeconds(2.5f);
            settings.Duration = TimeSpan.FromSeconds(5f);

            settings.MinHorizontalVelocity = 5;
            settings.MaxHorizontalVelocity = 10;

            settings.MinVerticalVelocity = 5;
            settings.MaxVerticalVelocity = 15;

            settings.EndVelocity = 0.75f;

            // All zero
            //settings.MinHorizontalVelocity = settings.MaxHorizontalVelocity = settings.MinVerticalVelocity = settings.MaxVerticalVelocity = settings.EndVelocity = 0;


            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 50;
            settings.MaxStartSize = 100;

            settings.MinEndSize = 0;
            settings.MaxEndSize = 150;

            // All same
            settings.MinStartSize = settings.MaxStartSize = settings.MinEndSize = settings.MaxEndSize = 10;

            settings.MinColor = settings.MaxColor = Color.Cyan;

            settings.BlendState = BlendState.Additive;

            this.VisibleChanged += delegate
            {
                Console.WriteLine("Vis: " + this.Visible);
            };

            this.EnabledChanged += delegate
            {
                Console.WriteLine("Enabled: " + this.Enabled);
            };
        }
    }
}
