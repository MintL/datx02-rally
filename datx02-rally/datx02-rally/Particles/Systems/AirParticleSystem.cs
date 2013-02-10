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
    class AirParticleSystem : Particle3DSample.ParticleSystem
    {
        public AirParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = @"Particles/plasma";

            settings.MaxParticles = 1500;

            settings.Duration = TimeSpan.FromSeconds(10);

            settings.MinHorizontalVelocity = 6;
            settings.MaxHorizontalVelocity = 8;

            settings.MinVerticalVelocity = 6;
            settings.MaxVerticalVelocity = 8;

            settings.EndVelocity = 0.0f;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 40;
            settings.MaxStartSize = 60;

            settings.MinEndSize = 20;
            settings.MaxEndSize = 30;

            settings.MinColor = new Color(200, 0, 200, 200);
            settings.MaxColor = new Color(128, 0, 128, 128);

            BlendState bs = new BlendState();

            bs.AlphaDestinationBlend = Blend.One; //BlendState.Additive.AlphaDestinationBlend;
            bs.AlphaSourceBlend = Blend.One;// BlendState.Additive.AlphaSourceBlend;

            bs.ColorDestinationBlend = Blend.InverseSourceAlpha;
            bs.ColorSourceBlend = Blend.SourceAlpha; // BlendState.Additive.ColorSourceBlend;

            settings.BlendState = bs;

        }
    }
}
