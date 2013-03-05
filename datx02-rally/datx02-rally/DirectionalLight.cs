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
        
        private Vector3 defaultAmbient, additionalAmbient;
        public Vector3 Ambient { get { return defaultAmbient + additionalAmbient; } set { additionalAmbient = value; } }

        private Vector3 defaultDiffuse, additionalDiffuse;
        public Vector3 Diffuse { get { return defaultDiffuse + additionalDiffuse; } set { additionalDiffuse = value; } }

        public DirectionalLight(Vector3 direction, Vector3 ambient, Vector3 diffuse)
        {
            Direction = Vector3.Normalize(direction);
            defaultAmbient = ambient;
            defaultDiffuse = diffuse;
        }

    }
}
