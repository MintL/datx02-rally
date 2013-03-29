#region File Description
//-----------------------------------------------------------------------------
// SmokePlumeParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Particle3DSample
{
    /// <summary>
    /// Custom particle system for creating a giant plume of long lasting smoke.
    /// </summary>
    class SmokePlumeParticleSystem : ParticleSystem
    {
        public SmokePlumeParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = @"Particles/smoke";

            settings.MaxParticles = 2400;

            settings.Duration = TimeSpan.FromSeconds(2);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 15;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 10;
            
            settings.Gravity = new Vector3(0, 50, 0);

            settings.EndVelocity = 0.75f;

            //settings.MinRotateSpeed = -1;
            //settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 4;
            settings.MaxStartSize = 7;

            settings.MinEndSize = 60;
            settings.MaxEndSize = 100;

            settings.MinColor = settings.MaxColor = new Color(80, 80, 80, 250);

            BlendState bs = new BlendState();

            bs.AlphaDestinationBlend = Blend.One; //BlendState.Additive.AlphaDestinationBlend;
            bs.AlphaSourceBlend = Blend.One;// BlendState.Additive.AlphaSourceBlend;

            bs.ColorDestinationBlend = Blend.InverseSourceAlpha;
            bs.ColorSourceBlend = Blend.SourceAlpha; // BlendState.Additive.ColorSourceBlend;

            settings.BlendState = bs;
        }
    }
}
