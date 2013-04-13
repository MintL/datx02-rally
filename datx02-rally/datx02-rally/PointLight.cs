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
            color += colorVariation * 0.01f;

            if (color.X < 0.4f || color.X > 0.99f) colorVariation.X *= -1f;
            if (color.Y < 0.4f || color.Y > 0.99f) colorVariation.Y *= -1f;
            if (color.Z < 0.4f || color.Z > 0.99f) colorVariation.Z *= -1f;

            Diffuse = color;

            // position
            velocity += colorVariation * 0.05f;
            velocity.Normalize();
        }

        protected override void SetEffectParameters(Effect effect)
        {
            ((BasicEffect)effect).DiffuseColor = Diffuse;
        }
    }
}
