using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using datx02_rally.Entities;

namespace datx02_rally
{
    /// <summary>
    /// ???
    /// </summary>
    public class PrelightingRenderer
    {
        #region Field

        //public RenderTarget2D DepthTarget { get; protected set; }
        //public RenderTarget2D NormalTarget { get; protected set; }
        public RenderTarget2D NormalDepthTarget { get; protected set; }

        public RenderTarget2D LightTarget { get; protected set; }

        TerrainModel[,] terrainSegments;
        int terrainSegmentsCount;
        Car car;

//        List<PointLight> pointLights;
        //List<SpotLight> spotLights;

        Effect depthNormalEffect;
        Effect lightingEffect;

        GraphicsDevice device;
        ContentManager content;

        public Matrix LightProjection { get; set; }

        int viewWidth;
        int viewHeight;

        private List<PointLight> pointLights = new List<PointLight>();
        private List<GameObject> otherGameObjects = new List<GameObject>();
        public List<GameObject> GameObjects
        {
            get
            {
                var objects = new List<GameObject>();
                objects.AddRange(pointLights);
                objects.AddRange(otherGameObjects);
                return objects;
            }
            set
            {
                pointLights.Clear();
                otherGameObjects.Clear();
                foreach (var gameObject in value)
                {
                    if (gameObject is PointLight)
                        pointLights.Add(gameObject as PointLight);
                    else
                        otherGameObjects.Add(gameObject);
                }
            }
        }

        #endregion

        /// <summary>
        /// Constructs a PrelightRenderer. TODO: Explain more...
        /// </summary>
        /// <param name="device"></param>
        /// <param name="content"></param>
        public PrelightingRenderer(GraphicsDevice device, ContentManager content)
        {
            this.device = device;
            this.content = content;

            LightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                device.Viewport.AspectRatio, 100f, 15000f);

            viewWidth = device.Viewport.Width;
            viewHeight = device.Viewport.Height;

            //DepthTarget = new RenderTarget2D(device, viewWidth, viewHeight, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            //NormalTarget = new RenderTarget2D(device, viewWidth, viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            NormalDepthTarget = new RenderTarget2D(device, viewWidth, viewHeight, false, SurfaceFormat.HalfVector4, DepthFormat.Depth24);

            LightTarget = new RenderTarget2D(device, viewWidth, viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);

            depthNormalEffect = content.Load<Effect>(@"Effects\Prelight\DepthNormal");
            lightingEffect = content.Load<Effect>(@"Effects\Prelight\Light");

            lightingEffect.Parameters["viewportWidth"].SetValue(viewWidth);
            lightingEffect.Parameters["viewportHeight"].SetValue(viewHeight);
        }

        public void Render(Matrix view, TerrainModel[,] terrainSegments, int terrainSegmentsCount, Car car)
        {
            this.terrainSegments = terrainSegments;
            this.terrainSegmentsCount = terrainSegmentsCount;
            //this.spotLights = spotLights;

            this.car = car;
            
            RenderDepthNormal(view);
            RenderLight(view * LightProjection);

            for (int z = 0; z < terrainSegmentsCount; z++)
            {
                for (int x = 0; x < terrainSegmentsCount; x++)
                {
                    if (x == 0 && z == 0)
                        continue;

                    var terrain = terrainSegments[x, z];
                    terrain.Effect.Parameters["LightTexture"].SetValue(LightTarget);
                    terrain.Effect.Parameters["PrelightProjection"].SetValue(LightProjection);
                    terrain.Effect.Parameters["viewportWidth"].SetValue(device.Viewport.Width);
                    terrain.Effect.Parameters["viewportHeight"].SetValue(device.Viewport.Height);

                }
            }

            foreach (GameObject obj in otherGameObjects)
            {
                foreach (ModelMesh mesh in obj.Model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect.Parameters["LightTexture"].SetValue(LightTarget);
                        part.Effect.Parameters["PrelightProjection"].SetValue(LightProjection);
                        part.Effect.Parameters["viewportWidth"].SetValue(device.Viewport.Width);
                        part.Effect.Parameters["viewportHeight"].SetValue(device.Viewport.Height);
                    }
                }
                
            }

            // TODO: CARMOVE 
            // Car
            car.PreparePrelighting(LightTarget, LightProjection, device.Viewport.Width, device.Viewport.Height);
            
        }

        private void RenderDepthNormal(Matrix view)
        {
            device.RasterizerState = RasterizerState.CullNone;
            device.DepthStencilState = new DepthStencilState { DepthBufferEnable = true, DepthBufferFunction = CompareFunction.LessEqual };
            device.BlendState = BlendState.Opaque;
            device.SetRenderTarget(NormalDepthTarget);
            device.Clear(new Color(0, 255, 255));

            // Terrain
            for (int z = 0; z < terrainSegmentsCount; z++)
            {
                for (int x = 0; x < terrainSegmentsCount; x++)
                {
                    var terrain = terrainSegments[x, z];
                    Effect current = terrain.Effect;
                    terrain.Effect = depthNormalEffect;
                    terrain.Draw(view, LightProjection);
                    //terrain.Draw(view, Vector3.Zero, null, null);
                    terrain.Effect = current;
                }
            }
            // Car and objects
            foreach (GameObject obj in otherGameObjects)
            {
                Model model = obj.Model;
                Dictionary<ModelMeshPart, Effect> oldEffects = new Dictionary<ModelMeshPart, Effect>();
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        oldEffects.Add(part, part.Effect);
                        part.Effect = depthNormalEffect;
                    }
                }
                obj.Draw(view, LightProjection);

                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = oldEffects[part];
                    }
                }

                // TODO: CARMOVE
                //// Car rendering
                //foreach (var mesh in car.Model.Meshes) // 5 meshes
                //{
                //    Matrix world = Matrix.Identity;

                //    // Wheel transformation
                //    if ((int)mesh.Tag > 0)
                //    {
                //        world *= Matrix.CreateRotationX(car.WheelRotationX);
                //        if ((int)mesh.Tag > 1)
                //            world *= Matrix.CreateRotationY(car.WheelRotationY);
                //    }

                //    // Local modelspace
                //    world *= mesh.ParentBone.Transform;

                //    // World
                //    world *= car.RotationMatrix * car.TranslationMatrix;

                //    foreach (ModelMeshPart part in mesh.MeshParts) // 5 effects for main, 1 for each wheel
                //    {
                //        oldEffects.Add(part, part.Effect);
                //        part.Effect = depthNormalEffect;

                //        EffectParameterCollection param = part.Effect.Parameters;

                //        param["World"].SetValue(world);
                //        param["View"].SetValue(view);
                //        param["Projection"].SetValue(LightProjection);
                //    }
                //    mesh.Draw();
                //}


                // Reset all effects

                //foreach (ModelMesh mesh in car.Model.Meshes)
                //{
                //    foreach (ModelMeshPart part in mesh.MeshParts)
                //    {
                //        part.Effect = oldEffects[part];
                //    }
                //}

                car.DrawDepthNormal(depthNormalEffect, LightProjection);
            }


            device.SetRenderTargets(null);
        }

        private void RenderLight(Matrix viewProjection)
        {
            lightingEffect.Parameters["NormalDepthTexture"].SetValue(NormalDepthTarget);
            //lightingEffect.Parameters["NormalTexture"].SetValue(NormalTarget);
            Matrix invViewProjection = Matrix.Invert(viewProjection);
            lightingEffect.Parameters["InvViewProjection"].SetValue(invViewProjection);

            device.SetRenderTarget(LightTarget);
            device.Clear(Color.Black);
            device.BlendState = BlendState.Additive;
            device.DepthStencilState = DepthStencilState.None;


            BasicEffect current = (BasicEffect)pointLights[0].Model.Meshes[0].MeshParts[0].Effect;
            foreach (PointLight light in pointLights)
            {
                lightingEffect.Parameters["LightColor"].SetValue(light.Diffuse * 2);
                lightingEffect.Parameters["LightPosition"].SetValue(light.Position);
                lightingEffect.Parameters["LightAttenuation"].SetValue(light.Radius);

                light.Model.Meshes[0].MeshParts[0].Effect = lightingEffect;
                Matrix wvp = (Matrix.CreateScale(light.Radius / 10) * Matrix.CreateTranslation(light.Position)) * viewProjection;
                lightingEffect.Parameters["WorldViewProjection"].SetValue(wvp);

                device.RasterizerState = RasterizerState.CullCounterClockwise;
                light.Model.Meshes[0].Draw();
                light.Model.Meshes[0].MeshParts[0].Effect = current;
            }

            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;

            device.SetRenderTarget(null);
        }

    }
}
