using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace datx02_rally.Entities
{
    enum GameObjectType
    {
        Foliage,
        Particle
    }

    public class GameObject
    {
        // TODO: UpdateBoundingVolume has to be called if either the position or scale 
        //       is updated but that means it's updated twice in the beginning
        private Vector3 position;
        public Vector3 Position
        {
            get { return position; }
            set { position = value; UpdateBoundingVolume(); }
        }

        private float scale;
        public float Scale 
        {
            get { return scale; }
            set { scale = value; UpdateBoundingVolume(); }
        }

        private Model model;
        public Model Model 
        {
            get { return model; }
            set { model = value; UpdateBoundingVolume(); }
        }

        /// <summary>
        /// Yaw, pitch, roll TODO: Why not matrix/quarternion?
        /// </summary>
        public Vector3 Rotation { get; set; }

        public BoundingSphere BoundingSphere { get; set; }

        

        protected Matrix world;
        protected Matrix[] baseTransforms;

        #region Initialization

        public GameObject() :
            this(1, Vector3.Zero)
        {
        }

        public GameObject(float scale, Vector3 rotation) :
            this(null, 1, Vector3.Zero)
        {
        }

        public GameObject(Model model, float scale, Vector3 rotation)
        {
            Model = model;
            Scale = scale;
            Rotation = rotation;
        }

        public virtual void UpdateBoundingVolume()
        {
            if (Model != null)
            {
                BoundingSphere = new BoundingSphere();
                foreach (var mesh in Model.Meshes)
                {
                    BoundingSphere = BoundingSphere.CreateMerged(mesh.BoundingSphere.Transform(mesh.ParentBone.Transform), BoundingSphere);
                }
                BoundingSphere = BoundingSphere.Transform(Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Position));
            }
        }

        #endregion

        public virtual void Update(GameTime gameTime)
        {
        }

        /// <summary>
        /// Called for every effect in the model each Draw()
        /// </summary>
        /// <param name="effect"></param>
        protected virtual void SetEffectParameters(Effect effect) { }

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

        public void DrawShadowCaster(GraphicsDevice device, Effect shadowMapEffect, Matrix view, Matrix projection)
        {
            baseTransforms = new Matrix[this.Model.Bones.Count];
            this.Model.CopyAbsoluteBoneTransformsTo(baseTransforms);

            world = Matrix.CreateScale(Scale) * Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) * Matrix.CreateTranslation(Position);

            foreach (var mesh in Model.Meshes)
            {
                var orderedParts = mesh.MeshParts.OrderBy(part => part.Effect.Parameters["AlphaMap"] == null ? -1 : 1); 

                //for (int p = mesh.MeshParts.Count - 1; p >= 0; p--) // Need reversed draw order!
                foreach (var part in orderedParts)
                {
                    //var part = mesh.MeshParts[p];

                    //shadowMapEffect.Parameters["AlphaEnabled"].SetValue(false); //p != 0);
                    var hasAlpha = part.Effect.Parameters["AlphaMap"] != null;
                    if (hasAlpha)
                        shadowMapEffect.Parameters["AlphaMap"].SetValue(part.Effect.Parameters["AlphaMap"].GetValueTexture2D());
                    shadowMapEffect.Parameters["AlphaEnabled"].SetValue(hasAlpha);

                    EffectParameterCollection parameters = shadowMapEffect.Parameters;
                    if (parameters["World"] != null)
                        parameters["World"].SetValue(baseTransforms[mesh.ParentBone.Index] * world);
                    if (parameters["View"] != null)
                        parameters["View"].SetValue(view);
                    if (parameters["Projection"] != null)
                        parameters["Projection"].SetValue(projection);

                    foreach (EffectPass pass in shadowMapEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        device.Indices = part.IndexBuffer;
                        device.SetVertexBuffer(part.VertexBuffer);
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                            part.VertexOffset,
                            0,
                            part.NumVertices,
                            part.StartIndex,
                            part.PrimitiveCount);
                    }
                }

            }
        }
    }
}
