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
    public class DirectionalLight
    {
        public Vector3 Direction { get; set; }
        public Vector3 Ambient { get; set; }
        public Vector3 Diffuse { get; set; }

        public DirectionalLight(Vector3 direction, Vector3 ambient, Vector3 diffuse)
        {
            Direction = direction;
            Ambient = ambient;
            Diffuse = diffuse;
        }

    }
}
