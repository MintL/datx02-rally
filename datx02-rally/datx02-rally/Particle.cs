using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally
{
    class Particle
    {
        private Texture2D particleBase;
        private Vector2 position;
        private Vector2 startDirection;
        private Vector2 endDirection;
        private float startScale;
        private float endScale;
        private Color startColor;
        private Color endColor;
        private float lifeLeft;
        private float startLife;
        private float rotation;
        private float rotationSpeed;
        private float lifePhase; // 0 means newly created, 1 means dead

        public Particle(Texture2D particleBase, Vector2 position, Vector2 startDirection, Vector2 endDirection,
                            float startScale, float endScale, Color startColor, Color endColor, float life, float rotationSpeed)
        {
            this.particleBase = particleBase;
            this.position = position;
            this.startDirection = startDirection;
            this.endDirection = endDirection;
            this.startScale = startScale;
            this.endScale = endScale;
            this.startColor = startColor;
            this.endColor = endColor;
            this.startLife = life;
            this.lifeLeft = life;
            this.rotation = 0;
            this.rotationSpeed = rotationSpeed;
        }

        public bool Update(float dt)
        {
            lifeLeft -= dt;
            if (lifeLeft <= 0)
            {
                return false;
            }
            lifePhase = 1 - lifeLeft / startLife;

            position += new Vector2(MathHelper.Lerp(startDirection.X, endDirection.X, lifePhase),
                                    MathHelper.Lerp(startDirection.Y, endDirection.Y, lifePhase)) * dt;
            rotation += rotationSpeed * dt;
            return true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float currScale = MathHelper.Lerp(startScale, endScale, lifePhase);
            Color currColor = new Color(
                (int)MathHelper.Lerp(startColor.R, endColor.R, lifePhase),
                (int)MathHelper.Lerp(startColor.G, endColor.G, lifePhase),
                (int)MathHelper.Lerp(startColor.B, endColor.B, lifePhase),
                (int)MathHelper.Lerp(startColor.A, endColor.A, lifePhase)
            );
            spriteBatch.Draw(particleBase, position, null, currColor, rotation, new Vector2(64,64), currScale, SpriteEffects.None, 0);
        }
    }
}
