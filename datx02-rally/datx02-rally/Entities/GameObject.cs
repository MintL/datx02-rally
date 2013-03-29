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

        public BoundingSphere BoundingSphere { get; set; }

        public Model Model { get; set; }

        protected Matrix world;
        protected Matrix[] baseTransforms;

        

        public GameObject()
        {
            Scale = 1;
            Rotation = Vector3.Zero;
        }

        public virtual void Update(GameTime gameTime)
        {
        }

        /// <summary>
        /// Called for every effect in the model each Draw()
        /// </summary>
        /// <param name="effect"></param>
        protected abstract void SetEffectParameters(Effect effect);

        public virtual void Draw(Matrix view, Matrix projection)
        {
            baseTransforms = new Matrix[this.Model.Bones.Count];
            this.Model.CopyAbsoluteBoneTransformsTo(baseTransforms);

            // Do nothing if the object is outside the view frustum
            BoundingFrustum viewFrustum = new BoundingFrustum(view * projection);
            if (viewFrustum.Intersects(BoundingSphere))
            {
                world = Matrix.CreateScale(Scale) * Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) * Matrix.CreateTranslation(Position);

                foreach (ModelMesh mesh in Model.Meshes)
                {
                    foreach (Effect currentEffect in mesh.Effects)
                    {
                        if (currentEffect is BasicEffect)
                        {
                            BasicEffect effect = currentEffect as BasicEffect;
                            effect.World = baseTransforms[mesh.ParentBone.Index] * world;
                            effect.View = view;
                            effect.Projection = projection;
                        }
                        else
                        {
                            EffectParameterCollection parameters = currentEffect.Parameters;
                            if (parameters["World"] != null)
                                parameters["World"].SetValue(baseTransforms[mesh.ParentBone.Index] * world);
                            if (parameters["View"] != null)
                                parameters["View"].SetValue(view);
                            if (parameters["Projection"] != null)
                                parameters["Projection"].SetValue(projection);
                        }
                        SetEffectParameters(currentEffect);
                    }
                    mesh.Draw();
                }
            }
        }
    }
}
