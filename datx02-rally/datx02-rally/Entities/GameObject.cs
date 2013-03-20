using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace datx02_rally.Entities
{
    public abstract class GameObject
    {
        public Vector3 Position { get; set; }
        public float Scale { get; set; }
        
        /// <summary>
        /// Yaw, pitch, roll
        /// </summary>
        public Vector3 Rotation { get; set; }

        protected Matrix world;
        protected Model model;
        protected ContentManager content;
        protected Matrix[] baseTransforms;

        public GameObject(string modelName, ContentManager content)
        {
            this.content = content;
            this.model = content.Load<Model>(modelName);

            baseTransforms = new Matrix[this.model.Bones.Count];
            this.model.CopyAbsoluteBoneTransformsTo(baseTransforms);

            
        }

        public void Update(GameTime gameTime)
        {
            // Matrix.CreateScale(Scale) * Matrix.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z) * 
            world = Matrix.CreateScale(Scale) * Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) * Matrix.CreateTranslation(Position);
        }

        /// <summary>
        /// Called for every effect in the model each Draw()
        /// </summary>
        /// <param name="effect"></param>
        public abstract void SetEffectParameters(Effect effect);

        public void Draw(Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    EffectParameterCollection parameters = currentEffect.Parameters;

                    if (parameters["World"] != null)
                        parameters["World"].SetValue(baseTransforms[mesh.ParentBone.Index] * world);
                    if (parameters["View"] != null)
                        parameters["View"].SetValue(view);
                    if (parameters["Projection"] != null)
                        parameters["Projection"].SetValue(projection);

                    SetEffectParameters(currentEffect);
                }
                mesh.Draw();
            }
        }
    }
}
