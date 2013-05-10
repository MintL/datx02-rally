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
        GameManager game;
        private static List<Model> birchTreesVariants = new List<Model>();

        public BirchTree(GameManager game)
            : base()
        {
            this.game = game;
            Model = birchTreesVariants[UniversalRandom.GetInstance().Next(birchTreesVariants.Count)];
        }

        public override void Update(GameTime gameTime)
        {
            
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
            birchTreesVariants.Clear();
            birchTreesVariants.Add(content.Load<Model>(@"Foliage\Birch_01"));
            birchTreesVariants.Add(content.Load<Model>(@"Foliage\Birch_02"));
            birchTreesVariants.Add(content.Load<Model>(@"Foliage\Birch_03"));
            birchTreesVariants.Add(content.Load<Model>(@"Foliage\Birch_04"));

            foreach (Model birchTree in birchTreesVariants)
            {
                foreach (ModelMesh mesh in birchTree.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = alphaMapEffect.Clone();
                    }
                }
            }

            // Different models need their textures in different order, possibly ordered by the 3d modeling application
            Model model = birchTreesVariants[0];
            model.Meshes[0].Effects[0].Parameters["ColorMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\branches"));
            model.Meshes[0].Effects[0].Parameters["AlphaMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\branches_Alpha"));
            model.Meshes[0].Effects[1].Parameters["ColorMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\bark"));

            model = birchTreesVariants[1];
            model.Meshes[0].Effects[1].Parameters["ColorMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\branches"));
            model.Meshes[0].Effects[1].Parameters["AlphaMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\branches_Alpha"));
            model.Meshes[0].Effects[0].Parameters["ColorMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\bark"));

            model = birchTreesVariants[2];
            model.Meshes[0].Effects[1].Parameters["ColorMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\branches"));
            model.Meshes[0].Effects[1].Parameters["AlphaMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\branches_Alpha"));
            model.Meshes[0].Effects[0].Parameters["ColorMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\bark"));

            model = birchTreesVariants[3];
            model.Meshes[0].Effects[0].Parameters["ColorMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\branches"));
            model.Meshes[0].Effects[0].Parameters["AlphaMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\branches_Alpha"));
            model.Meshes[0].Effects[1].Parameters["ColorMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\bark"));
        }
    }
}
