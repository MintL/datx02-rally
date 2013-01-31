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
    class PointLight
    {
        public Vector3 Position { get; set; }
        public Vector3 Ambient { get; set; }
        public Vector3 Diffuse { get; set; }
        public float Range { get; set; }

        public PointLight(Vector3 lightPosition, Vector3 ambient, Vector3 diffuse, float range)
        {
            Position = lightPosition;
            Ambient = ambient;
            Diffuse = diffuse;
            Range = range;
        }
    }
}
