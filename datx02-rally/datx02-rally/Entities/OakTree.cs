using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace datx02_rally.Entities
{
    public class OakTree : GameObject
    {
        Game1 game;
        private static Model oakTree;

        public OakTree(Game1 game)
            : base()
        {
            this.game = game;
            Model = oakTree;
        }

        public override void Update(GameTime gameTime)
        {
            BoundingSphere = new BoundingSphere();
            foreach (var mesh in Model.Meshes)
            {
                BoundingSphere = BoundingSphere.CreateMerged(mesh.BoundingSphere.Transform(mesh.ParentBone.Transform), BoundingSphere);
            }
            BoundingSphere = BoundingSphere.Transform(Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position));
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
            oakTree = content.Load<Model>(@"Foliage\Oak_tree");
            Effect alphaMapEffect = content.Load<Effect>(@"Effects\AlphaMap");

            // Initialize the material settings
            foreach (ModelMesh mesh in oakTree.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    BasicEffect basicEffect = (BasicEffect)part.Effect;
                    part.Effect = alphaMapEffect.Clone();
                    part.Effect.Parameters["ColorMap"].SetValue(basicEffect.Texture);
                }
                mesh.Effects[0].Parameters["NormalMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\BarkMossy-tiled-n"));

                mesh.Effects[1].Parameters["NormalMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\leaf-mapple-yellow-ni"));
                mesh.Effects[1].Parameters["AlphaMap"].SetValue(content.Load<Texture2D>(@"Foliage\Textures\leaf-mapple-yellow-a"));
            }
        }
    }
}
