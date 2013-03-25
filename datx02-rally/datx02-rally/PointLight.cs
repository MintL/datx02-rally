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
        public float Range { get; set; }

        private Vector3 colorDir = Vector3.One;
        private Vector3 dir;

        private static Model pointLight;

        public PointLight(Vector3 lightPosition, Vector3 diffuse, float range)
            : base()
        {
            Position = lightPosition;
            Diffuse = diffuse;
            Range = range;

            Model = pointLight;

            dir = new Vector3(-1 + 2 * (float)UniversalRandom.GetInstance().NextDouble(),
                        -1 + 2 * (float)UniversalRandom.GetInstance().NextDouble(),
                        -1 + 2 * (float)UniversalRandom.GetInstance().NextDouble());
            dir.Normalize();
        }

        public PointLight(Vector3 lightPosition)
            : this(lightPosition, Color.White.ToVector3(), 400.0f)
        {   
        }

        public override void Update(GameTime gameTime)
        {
            Vector3 color = Diffuse;
            color += colorDir * 0.01f;

            if (color.X < 0.4f || color.X > 0.99f) colorDir.X *= -1f;
            if (color.Y < 0.4f || color.Y > 0.99f) colorDir.Y *= -1f;
            if (color.Z < 0.4f || color.Z > 0.99f) colorDir.Z *= -1f;

            Diffuse = color;

            // position
            dir += colorDir * 0.05f;
            dir.Normalize();
            //Position += new Vector3(dir.X * 10f, 0, dir.Y * 10f);

            BoundingSphere = new BoundingSphere(Position, 1);

        }

        protected override void SetEffectParameters(Effect effect)
        {
            ((BasicEffect)effect).DiffuseColor = Diffuse;
        }

        public static void LoadMaterial(ContentManager content)
        {
            pointLight = content.Load<Model>(@"Models/light");
        }

    }
}
