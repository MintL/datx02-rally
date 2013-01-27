using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally
{
    class Particle
    {
        private Texture2D particleBase;
        private Vector2 position;
        private Vector2 direction;
        private float scale;
        private Color color;
        private float life;

        public Particle(Texture2D particleBase, Vector2 position, Vector2 direction,
                            float scale, Color color, float life)
        {
            this.particleBase = particleBase;
            this.position = position;
            this.direction = direction;
            this.scale = scale;
            this.color = color;
            this.life = life;
        }

        public bool Update(float dt)
        {
            position += direction * dt;
            life -= dt;
            if (life > 0)
            {
                return true;
            }
            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(particleBase, position, null, color, 0, new Vector2(0,0), scale, SpriteEffects.None, 0);
        }
    }
}
