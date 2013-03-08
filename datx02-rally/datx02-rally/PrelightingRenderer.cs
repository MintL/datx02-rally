using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace datx02_rally
{
    public class PrelightingRenderer
    {
        public RenderTarget2D DepthTarget { get; protected set; }
        public RenderTarget2D NormalTarget { get; protected set; }
        public RenderTarget2D LightTarget { get; protected set; }

        TerrainModel[,] terrainSegments;
        int terrainSegmentsCount;
        List<PointLight> pointLights;
        List<SpotLight> spotLights;
        Model pointLightModel;
        Model spotLightModel;

        Effect depthNormalEffect;
        Effect lightingEffect;

        GraphicsDevice device;
        ContentManager content;

        public Matrix LightProjection { get; set; }

        int viewWidth;
        int viewHeight;

        public PrelightingRenderer(GraphicsDevice device, ContentManager content, Model pointLightModel, Model spotLightModel)
        {
            this.device = device;
            this.content = content;
            this.pointLightModel = pointLightModel;
            this.spotLightModel = spotLightModel;

            LightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                device.Viewport.AspectRatio, 100f, 15000f);

            viewWidth = device.Viewport.Width;
            viewHeight = device.Viewport.Height;

            DepthTarget = new RenderTarget2D(device, viewWidth, viewHeight, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            NormalTarget = new RenderTarget2D(device, viewWidth, viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            LightTarget = new RenderTarget2D(device, viewWidth, viewHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);

            depthNormalEffect = content.Load<Effect>(@"Effects\Prelight\DepthNormal");
            lightingEffect = content.Load<Effect>(@"Effects\Prelight\Light");

            lightingEffect.Parameters["viewportWidth"].SetValue(viewWidth);
            lightingEffect.Parameters["viewportHeight"].SetValue(viewHeight);
        }

        public void Render(Matrix view, DirectionalLight directionalLight, TerrainModel[,] terrainSegments, int terrainSegmentsCount, List<PointLight> pointLights, List<SpotLight> spotLights)
        {
            this.terrainSegments = terrainSegments;
            this.terrainSegmentsCount = terrainSegmentsCount;
            this.pointLights = pointLights;
            this.spotLights = spotLights;
            
            RenderDepthNormal(view);
            RenderLight(view * LightProjection);

            for (int z = 0; z < terrainSegmentsCount; z++)
                for (int x = 0; x < terrainSegmentsCount; x++)
                {
                    var terrain = terrainSegments[x, z];
                    terrain.Effect.Parameters["LightTexture"].SetValue(LightTarget);
                    terrain.Effect.Parameters["PrelightProjection"].SetValue(LightProjection);
                    terrain.Effect.Parameters["viewportWidth"].SetValue(device.Viewport.Width);
                    terrain.Effect.Parameters["viewportHeight"].SetValue(device.Viewport.Height);

                }
            
        }

        public void RenderDepthNormal(Matrix view)
        {
            device.RasterizerState = RasterizerState.CullNone;
            device.DepthStencilState = new DepthStencilState { DepthBufferEnable = true, DepthBufferFunction = CompareFunction.LessEqual };
            device.BlendState = BlendState.Opaque;
            device.SetRenderTargets(NormalTarget, DepthTarget);
            device.Clear(new Color(0, 255, 255));

            for (int z = 0; z < terrainSegmentsCount; z++)
                for (int x = 0; x < terrainSegmentsCount; x++)
                {
                    var terrain = terrainSegments[x, z];
                    Effect current = terrain.Effect;
                    terrain.Effect = depthNormalEffect;
                    terrain.Draw(view, LightProjection);
                    //terrain.Draw(view, Vector3.Zero, null, null);
                    terrain.Effect = current;
                }

            device.SetRenderTargets(null);
        }

        public void RenderLight(Matrix viewProjection)
        {
            lightingEffect.Parameters["DepthTexture"].SetValue(DepthTarget);
            lightingEffect.Parameters["NormalTexture"].SetValue(NormalTarget);
            Matrix invViewProjection = Matrix.Invert(viewProjection);
            lightingEffect.Parameters["InvViewProjection"].SetValue(invViewProjection);

            device.SetRenderTarget(LightTarget);
            device.Clear(Color.Black);
            device.BlendState = BlendState.Additive;
            device.DepthStencilState = DepthStencilState.None;


            BasicEffect current = (BasicEffect)pointLightModel.Meshes[0].MeshParts[0].Effect;
            foreach (PointLight light in pointLights)
            {
                lightingEffect.Parameters["LightColor"].SetValue(light.Diffuse * 2);
                lightingEffect.Parameters["LightPosition"].SetValue(light.Position);
                lightingEffect.Parameters["LightAttenuation"].SetValue(light.Range);

                pointLightModel.Meshes[0].MeshParts[0].Effect = lightingEffect;
                Matrix wvp = (Matrix.CreateScale(light.Range / 10) * Matrix.CreateTranslation(light.Position)) * viewProjection;
                lightingEffect.Parameters["WorldViewProjection"].SetValue(wvp);

                device.RasterizerState = RasterizerState.CullCounterClockwise;
                pointLightModel.Meshes[0].Draw();
            }

            pointLightModel.Meshes[0].MeshParts[0].Effect = current;

            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;

            device.SetRenderTarget(null);
        }

    }
}
