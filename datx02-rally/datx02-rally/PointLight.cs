using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using datx02_rally.Entities;

namespace datx02_rally
{
    public class PointLight : GameObject
    {
        public Vector3 Diffuse { get; set; }
        public float Radius { get; set; }

        public float ColorTimeOffset { get; set; }

        private Vector3 colorVariation = Vector3.One;
        private Vector3 velocity;

        public PointLight(Vector3 position, Vector3 diffuse, float radius)
            : base()
        {
            Position = position;
            Diffuse = diffuse;
            Radius = radius;

            // [-1..1]
            velocity = new Vector3(-1 + 2 * (float)UniversalRandom.GetInstance().NextDouble(),
                        -1 + 2 * (float)UniversalRandom.GetInstance().NextDouble(),
                        -1 + 2 * (float)UniversalRandom.GetInstance().NextDouble());
            velocity.Normalize();

        }

        public PointLight(Vector3 lightPosition)
            : this(lightPosition, Color.White.ToVector3(), 400.0f)
        {   
        }

        public override void Update(GameTime gameTime)
        {
            Vector3 color = Diffuse;
                    
            //color += colorVariation * 0.01f;

            //if (color.X < 0.4f || color.X > 0.99f) colorVariation.X *= -1f;
            //if (color.Y < 0.4f || color.Y > 0.99f) colorVariation.Y *= -1f;
            //if (color.Z < 0.4f || color.Z > 0.99f) colorVariation.Z *= -1f;
            float elapsedGameTime = (float)gameTime.TotalGameTime.TotalSeconds;

            //Sinus curve based on elapsed game time, first multiplication determines the speed the change travels at
            //Offset determines the direction of the change and creates the pattern
            //Addition+division determines intensity of the effect and puts it in the 0-1 range

            //Main intensity determiner
            color.X = ((float)Math.Sin(elapsedGameTime * 3.5f - ColorTimeOffset) + 1.5f ) / 3;
            color.Y = ((float)Math.Sin(elapsedGameTime * 3.5f - ColorTimeOffset) + 1.5f ) / 3;
            color.Z = ((float)Math.Sin(elapsedGameTime * 3.5f - ColorTimeOffset) + 1.5f ) / 3;

            //Red and blue effect, creates the pink-purple-blue trail
            color.X += ((float)Math.Sin(elapsedGameTime * 2.5f + ColorTimeOffset/2) + 1.5f) / 10;
            color.Z += ((float)Math.Sin(elapsedGameTime * 2.5f + ColorTimeOffset/2) + 1.5f) / 10;

            //Removes green and red colour in the opposite direction
            color.X -= ((float)Math.Sin(elapsedGameTime * 2f + ColorTimeOffset / 3) + 1.5f) / 10;
            color.Y -= ((float)Math.Sin(elapsedGameTime * 2f + ColorTimeOffset / 3) + 1.5f) / 10;

            //Adds additional green in the trail direction
            color.Y += ((float)Math.Sin(elapsedGameTime * 1f - ColorTimeOffset / 4) + 1.5f) / 20;
            
            //Removes blue in the opposite direction
            //color.Z -= ((float)Math.Sin(elapsedGameTime * 6f + ColorTimeOffset) + 1.5f) / 15;

            Diffuse = color;
        }

        protected override void SetEffectParameters(Effect effect)
        {
            ((BasicEffect)effect).DiffuseColor = Diffuse;
        }
    }
}
