using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace datx02_rally.Entities
{
    public class BirchTree : GameObject
    {
        Game1 game;
        private static Model birchTree;

        public BirchTree(Game1 game)
            : base()
        {
            this.game = game;
            Model = birchTree;
        }

        public override void Update(GameTime gameTime)
        {
            BoundingSphere = new BoundingSphere(Position, Scale * 18);
        }

        protected override void SetEffectParameters(Effect effect)
        {
            DirectionalLight directionalLight = game.GetService<DirectionalLight>();
            if (effect.Parameters["DirectionalDirection"] != null)
                effect.Parameters["DirectionalDirection"].SetValue(directionalLight.Direction);
            if (effect.Parameters["DirectionalDiffuse"] != null)
                effect.Parameters["DirectionalDiffuse"].SetValue(directionalLight.Diffuse);
            if (effect.Parameters["DirectionalAmbient"] != null)
                effect.Parameters["DirectionalAmbient"].SetValue(directionalLight.Ambient);
        }

        /// <summary>
        /// Loads the effect and model as well as sets the material parameters on the effect.
        /// Is called before creating an object of this type
        /// </summary>
        /// <param name="content"></param>
        public static void LoadMaterial(ContentManager content)
        {
            Effect alphaMapEffect = content.Load<Effect>(@"Effects\AlphaMap");
            birchTree = content.Load<Model>(@"Foliage\Birch_04");
            foreach (ModelMesh mesh in birchTree.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = alphaMapEffect.Clone();
                }
                mesh.Effects[0].Parameters["ColorMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\branches"));
                //mesh.Effects[0].Parameters["NormalMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\BarkMossy-tiled-n"));

                //mesh.Effects[1].Parameters["ColorMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\branches"));
                mesh.Effects[0].Parameters["AlphaMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\branches_Alpha"));
                mesh.Effects[1].Parameters["ColorMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\bark"));
            }
        }
    }
}
