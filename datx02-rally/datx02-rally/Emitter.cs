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
        private Vector2 endScale;
        private Color startColor1;
        private Color startColor2;
        private Color endColor1;
        private Color endColor2;
        private Vector2 startSpeed;
        private Vector2 endSpeed;
        private Vector2 startLife;


        public Emitter(Vector2 relPosition, Texture2D particleTexture, int budget, float nextSpawnIn, Random random,
                        Vector2 spawnDirection, Vector2 directionNoiseAngle, Vector2 startScale, Vector2 endScale, 
                        Color startColor1, Color startColor2, Color endColor1, Color endColor2,
                        Vector2 startSpeed, Vector2 endSpeed, Vector2 startLife)
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
            this.endScale = endScale;
            this.startColor1 = startColor1;
            this.startColor2 = startColor2;
            this.endColor1 = endColor1;
            this.endColor2 = endColor2;
            this.startSpeed = startSpeed;
            this.endSpeed = endSpeed;
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
                    Vector2 startDirection = Vector2.Transform(spawnDirection, 
                        Matrix.CreateRotationZ(MathHelper.Lerp(directionNoiseAngle.X, directionNoiseAngle.Y, (float)random.NextDouble())));
                    startDirection.Normalize();
                    Vector2 endDirection = startDirection * MathHelper.Lerp(endSpeed.X, endSpeed.Y, (float)random.NextDouble());
                    startDirection *= MathHelper.Lerp(startSpeed.X, startSpeed.Y, (float)random.NextDouble());
                    Color startColor = new Color(
                        (int)MathHelper.Lerp(startColor1.R, startColor2.R, (float)random.NextDouble()),
                        (int)MathHelper.Lerp(startColor1.G, startColor2.G, (float)random.NextDouble()),
                        (int)MathHelper.Lerp(startColor1.B, startColor2.B, (float)random.NextDouble()),
                        (int)MathHelper.Lerp(startColor1.A, startColor2.A, (float)random.NextDouble())
                    );
                    Color endColor = new Color(
                        (int)MathHelper.Lerp(endColor1.R, endColor2.R, (float)random.NextDouble()),
                        (int)MathHelper.Lerp(endColor1.G, endColor2.G, (float)random.NextDouble()),
                        (int)MathHelper.Lerp(endColor1.B, endColor2.B, (float)random.NextDouble()),
                        (int)MathHelper.Lerp(endColor1.A, endColor2.A, (float)random.NextDouble())
                    );
                    
                    //Color color = Color.Lerp(startColor1, startColor2, (float)random.NextDouble());
                    float life = MathHelper.Lerp(startLife.X, startLife.Y, (float)random.NextDouble());

                    activeParticles.AddLast(
                        new Particle(
                            particleTexture,
                            relPosition,
                            startDirection,
                            endDirection,
                            MathHelper.Lerp(startScale.X, startScale.Y, (float)random.NextDouble()),
                            MathHelper.Lerp(endScale.X, endScale.Y, (float)random.NextDouble()),
                            startColor,
                            endColor,
                            life
                        )
                    );
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
