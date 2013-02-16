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

namespace datx02_rally
{
    public class PointLight
    {
        public Vector3 Position { get; set; }
        public Vector3 Diffuse { get; set; }
        public float Range { get; set; }

        public PointLight(Vector3 lightPosition, Vector3 diffuse, float range)
        {
            Position = lightPosition;
            Diffuse = diffuse;
            Range = range;
        }

        public PointLight(Vector3 lightPosition)
            : this(lightPosition, Color.White.ToVector3(), 400.0f)
        {   
        }

        public void Draw(Model model, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect currentEffect in mesh.Effects)
                {
                    currentEffect.DiffuseColor = Diffuse;
                    currentEffect.World = Matrix.CreateTranslation(Position);
                    currentEffect.View = view;
                    currentEffect.Projection = projection;
                }
                mesh.Draw();
            }
        }
    }
}
