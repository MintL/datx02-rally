#region File Description
//-----------------------------------------------------------------------------
// FireParticleSystem.cs
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

namespace datx02_rally.Particles.Systems
{
    /// <summary>
    /// Custom particle system for creating a flame effect.
    /// </summary>
    class FireParticleSystem : ParticleSystem
    {
        public FireParticleSystem(Game game, ContentManager content)
            : base(game, content)
        {
            DrawOrder = 7;
        }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = @"Particles/smoke_2";

            settings.MaxParticles = 10000;

            settings.Duration = TimeSpan.FromSeconds(2);

            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 15;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 10;

            // Set gravity upside down, so the flames will 'fall' upward.
            settings.Gravity = new Vector3(0, 30, 0);



            settings.MinColor = new Color(250, 200, 0, 100);
            settings.MaxColor = new Color(240, 20, 60, 100);//new Color(255, 165, 0);

            //, new Color(250, 140, 140),
            //, 

            settings.MinStartSize = 10;
            settings.MaxStartSize = 20;

            settings.MinEndSize = 20;
            settings.MaxEndSize = 50;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
