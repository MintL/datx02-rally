using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace datx02_rally.Particles.Systems
{
    class YellowPlasmaParticleSystem : RedPlasmaParticleSystem
    {
        public YellowPlasmaParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            base.InitializeSettings(settings);

            settings.MinColor = settings.MaxColor = Color.Yellow;
        }
    }
}
