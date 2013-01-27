using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally
{
    public class Emitter
    {
        private Vector2 relPosition; // Relative position to parent particle system
        private Texture2D particleTexture;
        private int budget; // Max number of active particles
        private float nextSpawnIn; // Next particle creation in seconds
        private float secondsPassed; // Seconds passed since last spawn
        private LinkedList<Particle> activeParticles;
        private Random random;

        private Vector2 spawnDirection;
        private Vector2 directionNoiseAngle;
        private Vector2 startScale;
        //private Vector2 endScale;
        private Color startColor1;
        private Color startColor2;
        //private Color endColor1;
        //private Color endColor2;
        private Vector2 startSpeed;
        private Vector2 startLife;


        public Emitter(Vector2 relPosition, Texture2D particleTexture, int budget, float nextSpawnIn, Random random,
                        Vector2 spawnDirection, Vector2 directionNoiseAngle, Vector2 startScale, Color startColor1, Color startColor2,
                        Vector2 startSpeed, Vector2 startLife)
        {
            this.relPosition = relPosition;
            this.particleTexture = particleTexture;
            this.budget = budget;
            this.nextSpawnIn = nextSpawnIn;
            this.random = random;
            this.activeParticles = new LinkedList<Particle>();
            this.secondsPassed = 0;

            this.spawnDirection = spawnDirection;
            this.directionNoiseAngle = directionNoiseAngle;
            this.startScale = startScale;
            this.startColor1 = startColor1;
            this.startColor2 = startColor2;
            this.startSpeed = startSpeed;
            this.startLife = startLife;
        }

        public void Update(float dt)
        {
            secondsPassed += dt;
            while (secondsPassed > nextSpawnIn)
            {
                if (activeParticles.Count < budget)
                {
                    // Spawn a particle
                    Vector2 direction = Vector2.Transform(spawnDirection, 
                        Matrix.CreateRotationZ(MathHelper.Lerp(directionNoiseAngle.X, directionNoiseAngle.Y, (float)random.NextDouble())));
                    direction.Normalize();
                    direction *= MathHelper.Lerp(startSpeed.X, startSpeed.Y, (float)random.NextDouble());
                    float scale = MathHelper.Lerp(startScale.X, startScale.Y, (float)random.NextDouble());
                    Color color = new Color(
                        (int)MathHelper.Lerp(startColor1.R, startColor2.R, (float)random.NextDouble()),
                        (int)MathHelper.Lerp(startColor1.G, startColor2.G, (float)random.NextDouble()),
                        (int)MathHelper.Lerp(startColor1.B, startColor2.B, (float)random.NextDouble()),
                        (int)MathHelper.Lerp(startColor1.A, startColor2.A, (float)random.NextDouble())
                    );
                    
                    //Color color = Color.Lerp(startColor1, startColor2, (float)random.NextDouble());
                    float life = MathHelper.Lerp(startLife.X, startLife.Y, (float)random.NextDouble());

                    activeParticles.AddLast(new Particle(
                        particleTexture,
                        relPosition,
                        direction,
                        scale,
                        color,
                        life
                    ));
                }
                secondsPassed -= nextSpawnIn;
            }

            LinkedListNode<Particle> node = activeParticles.First;
            while (node != null)
            {
                bool isAlive = node.Value.Update(dt);
                node = node.Next;
                if (!isAlive)
                {
                    if (node == null)
                    {
                        activeParticles.RemoveLast();
                    }
                    else
                    {
                        activeParticles.Remove(node.Previous);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Particle particle in activeParticles)
            {
                particle.Draw(spriteBatch);
            }
        }
    }
}
