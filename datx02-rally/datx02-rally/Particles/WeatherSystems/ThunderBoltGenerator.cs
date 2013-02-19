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
        private static Random random = new Random();
        private ParticleSystem particleSystem;

        private Vector3 startPosition = new Vector3(0, 10000, 0),
            endPosition = new Vector3(0);

        private float flash = 0;
        private Vector3 flashColor = new Vector3(.56f, .35f, .75f);

        public ThunderBoltGenerator(Game1 game, ThunderParticleSystem thunderParticleSystem)
            : base(game)
        {
            particleSystem = thunderParticleSystem;
        }

        private class SubBolt
        {
            public Vector3 position, target;

            public SubBolt Clone()
            {
                return new SubBolt()
                {
                    position = this.position,
                    target = this.target
                };
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (random.NextDouble() < .005) //.013)
            {
                flash = 1;
                Game.GetService<CameraComponent>().CurrentCamera.Shake();

                List<SubBolt> branches = new List<SubBolt>();

                branches.Add(new SubBolt()
                {
                    position = startPosition + new Vector3(
                        30000 * ((float)random.NextDouble() - .5f), 0,
                        30000 * ((float)random.NextDouble() - .5f)),
                    target = endPosition + new Vector3(
                        30000 * ((float)random.NextDouble() - .5f), 0,
                        30000 * ((float)random.NextDouble() - .5f))
                });

                while (branches.Count > 0)
                {
                    for (int i = 0; i < branches.Count; i++)
                    {
                        Vector3 offset = Vector3.Transform(.75f * Vector3.Normalize(branches[i].target - branches[i].position),
                                Matrix.CreateFromYawPitchRoll(
                                    MathHelper.Pi * ((float)random.NextDouble() - .5f),
                                    MathHelper.Pi * ((float)random.NextDouble() - .5f),
                                    MathHelper.Pi * ((float)random.NextDouble() - .5f)));
                        for (int j = 0; j < 10; j++)
                        {
                            branches[i].position += j * offset;
                            particleSystem.AddParticle(branches[i].position, Vector3.Zero);
                        }

                        if (random.NextDouble() < .001)
                        {
                            var newBolt = branches.Last().Clone();
                            newBolt.target = offset;
                            newBolt.target.Y = 0;
                            branches.Add(newBolt);
                        }

                        if (branches[i].position.Y < 0)
                            branches.RemoveAt(i--);
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
