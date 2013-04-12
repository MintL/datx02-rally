using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using datx02_rally.Particles.Systems;
using datx02_rally.Particles;

namespace datx02_rally.Entities
{
    public class FireObject : PointLight
    {
        FireParticleSystem fireSystem;
        ParticleEmitter fireEmitter;
        SmokePlumeParticleSystem fireSmokeSystem;
        ParticleEmitter fireSmokeEmitter;

        Vector3 smokeOffset;

        private TimeSpan colorTime = TimeSpan.Zero;
        private int colorIndex = 0;
        private static Color[] colors = new Color[] { new Color(255, 100, 50), new Color(255, 50, 25) };

        public FireObject(Game1 gameInstance, ContentManager content, Vector3 firePosition, Vector3 smokeOffset)
            : base(firePosition, Color.Red.ToVector3(), 400)
        {
            fireSystem = new FireParticleSystem(gameInstance, content);
            fireSystem.Initialize();

            fireSmokeSystem = new SmokePlumeParticleSystem(gameInstance, content);
            fireSmokeSystem.Initialize();

            fireEmitter = new ParticleEmitter(fireSystem, 100, firePosition);
            fireEmitter.Origin = firePosition;

            fireSmokeEmitter = new ParticleEmitter(fireSmokeSystem, 100, firePosition + smokeOffset);
            fireSmokeEmitter.Origin = firePosition + smokeOffset;
        }
        
        public override void Update(GameTime gameTime)
        {
            colorTime += gameTime.ElapsedGameTime;
            if (colorTime.TotalSeconds > 0.15f)
            {
                colorIndex = (++colorIndex) % colors.Length;
                Diffuse = colors[colorIndex].ToVector3();
                colorTime = TimeSpan.Zero;
            }

            //Position += Vector3.Right * (float)gameTime.ElapsedGameTime.TotalSeconds * 100f;

            BoundingSphere = new BoundingSphere(Position, 1);

            fireEmitter.Origin = Position;
            fireEmitter.Update(gameTime, fireEmitter.Origin);
            fireSystem.Update(gameTime);

            fireSmokeEmitter.Origin = Position + smokeOffset;
            fireSmokeEmitter.Update(gameTime, fireSmokeEmitter.Origin);
            fireSmokeSystem.Update(gameTime);
        }

        public override void Draw(Matrix view, Matrix projection)
        {
            fireSystem.SetCamera(view, projection);
            fireSmokeSystem.SetCamera(view, projection);

            fireSmokeSystem.Draw(null);
            fireSystem.Draw(null);
        }

        public static void LoadMaterial()
        {

        }

    }
}
