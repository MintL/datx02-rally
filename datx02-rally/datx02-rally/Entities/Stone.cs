using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace datx02_rally.Entities
{
    public class Stone : GameObject
    {
        Game1 game;
        private static List<Model> stoneVariants = new List<Model>();

        public Stone(Game1 game)
            : base()
        {
            this.game = game;
            Model = stoneVariants[UniversalRandom.GetInstance().Next(stoneVariants.Count)];
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
            stoneVariants.Add(content.Load<Model>(@"Foliage\Stone_01"));
            stoneVariants.Add(content.Load<Model>(@"Foliage\Stone_02"));

            foreach (Model birchTree in stoneVariants)
            {
                foreach (ModelMesh mesh in birchTree.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = alphaMapEffect.Clone();
                    }
                    mesh.Effects[0].Parameters["ColorMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\stone_c"));
                    mesh.Effects[0].Parameters["NormalMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\stone_n"));
                }

            }

        }
    }
}
