using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace datx02_rally.Particles.Systems
{
    class RedPlasmaParticleSystem : PlasmaParticleSystem
    {
        public RedPlasmaParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }

        protected override void InitializeSettings(Particle3DSample.ParticleSettings settings)
        {
            base.InitializeSettings(settings);

            settings.MaxEndSize = settings.MinEndSize = settings.MaxStartSize = settings.MinStartSize = 5;
            settings.MinColor = settings.MaxColor = Color.Red;
        }

    }
}
