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
    class SmokeCloudParticleSystem : ParticleSystem
    {
        public SmokeCloudParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = @"Particles/smoke";

            settings.MaxParticles = 600;

            settings.Duration = TimeSpan.FromSeconds(8);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 15;

            settings.MinVerticalVelocity = 10;
            settings.MaxVerticalVelocity = 20;

            // Create a wind effect by tilting the gravity vector sideways.
            settings.Gravity = 50 * new Vector3(-0.8f, -1f, -0.8f);

            settings.EndVelocity = 0.75f;

            settings.MinRotateSpeed = -.5f;
            settings.MaxRotateSpeed = .5f;

            settings.MinStartSize = 400;
            settings.MaxStartSize = 600;

            settings.MinEndSize = 600;
            settings.MaxEndSize = 700;
        }
    }
}
