using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Particle3DSample;
using Microsoft.Xna.Framework;

namespace datx02_rally.Particles.WeatherSystems
{
    class ThunderBoltGenerator : GameComponent
    {
        private ParticleSystem particleSystem;

        private Vector3 startPosition = new Vector3(0, 10000, 0),
            endPosition = new Vector3(0);

        private bool spawn;

        private float flash = 0;
        private Vector3 flashColor = new Vector3(4f, 3f, 5f); // new Vector3(.56f, .35f, .75f);

        private float rotationSeed = MathHelper.Pi / 1.3f;

        public ThunderBoltGenerator(Game1 game, ThunderParticleSystem thunderParticleSystem)
            : base(game)
        {
            particleSystem = thunderParticleSystem;
        }

        private class SubBolt
        {
            public Vector3 position, target;
            public float minDist = float.MaxValue;

            public SubBolt Clone()
            {
                return new SubBolt()
                {
                    position = this.position,
                    target = this.target,
                    minDist = float.MaxValue
                };
            }
        }

        public void Flash()
        {
            spawn = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (spawn)
            {
                spawn = false;
                flash = 1;
                Game.GetService<CameraComponent>().CurrentCamera.Shake();

                List<SubBolt> branches = new List<SubBolt>();

                branches.Add(new SubBolt()
                {
                    position = startPosition + new Vector3(
                        5000 * ((float)UniversalRandom.GetInstance().NextDouble() - .5f), 0,
                        5000 * ((float)UniversalRandom.GetInstance().NextDouble() - .5f)),
                    target = endPosition + new Vector3(
                        20000 * ((float)UniversalRandom.GetInstance().NextDouble() - .5f), 0,
                        20000 * ((float)UniversalRandom.GetInstance().NextDouble() - .5f))
                });

                while (branches.Count > 0)
                {
                    for (int i = 0; i < branches.Count; i++)
                    {
                        Vector3 dTarget = branches[i].target - branches[i].position;
                        float distance = dTarget.Length();

                        // Remove
                        if (distance < 150)
                        {
                            branches.RemoveAt(i--);
                            continue;
                        }

                        // Add
                        if (UniversalRandom.GetInstance().NextDouble() < .002 && i == 0)
                        {
                            var newBolt = branches[i].Clone();

                            newBolt.target = newBolt.position + Vector3.Transform(dTarget,
                                Matrix.CreateFromYawPitchRoll(
                                    rotationSeed * ((float)UniversalRandom.GetInstance().NextDouble() - .5f),
                                    rotationSeed * ((float)UniversalRandom.GetInstance().NextDouble() - .5f),
                                    rotationSeed * ((float)UniversalRandom.GetInstance().NextDouble() - .5f)));
                            newBolt.target = Vector3.Lerp(newBolt.target, branches[0].target, .4f);
                            branches.Add(newBolt);
                        }

                        branches[i].minDist = distance;
                        dTarget.Normalize();


                        // Offset dTarget
                        Vector3 offset = Vector3.Transform(2f * dTarget,
                                Matrix.CreateFromYawPitchRoll(
                                    rotationSeed * ((float)UniversalRandom.GetInstance().NextDouble() - .5f),
                                    rotationSeed * ((float)UniversalRandom.GetInstance().NextDouble() - .5f),
                                    rotationSeed * ((float)UniversalRandom.GetInstance().NextDouble() - .5f)));

                        for (int j = 0; j < 2; j++)
                        {
                            particleSystem.AddParticle(branches[i].position, Vector3.Zero);
                            branches[i].position += 10 * j * offset;
                        }


                    }
                }
            }
            if (flash > 0)
                flash = Math.Max(flash - .01f, 0);

            Game.GetService<DirectionalLight>().Ambient = flash * flashColor;

            base.Update(gameTime);
        }

    }
}
