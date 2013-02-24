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

        TerrainModel terrain;
        List<PointLight> pointLights;
        Model lightModel;

        Effect depthNormalEffect;
        Effect lightingEffect;

        GraphicsDevice device;
        ContentManager content;

        Matrix lightProjection;

        int viewWidth;
        int viewHeight;

        public PrelightingRenderer(GraphicsDevice device, ContentManager content, Model lightModel)
        {
            this.device = device;
            this.content = content;
            this.lightModel = lightModel;
            
            lightProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
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

        public void Render(Matrix view, DirectionalLight directionalLight, TerrainModel terrain, List<PointLight> pointLights)
        {
            this.terrain = terrain;
            this.pointLights = pointLights;
            
            RenderDepthNormal(view);
            RenderLight(view * lightProjection);
            
            terrain.Effect.Parameters["LightTexture"].SetValue(LightTarget);
            terrain.Effect.Parameters["PrelightProjection"].SetValue(lightProjection);
            terrain.Effect.Parameters["viewportWidth"].SetValue(device.Viewport.Width);
            terrain.Effect.Parameters["viewportHeight"].SetValue(device.Viewport.Height);
        }

        public void RenderDepthNormal(Matrix view)
        {
            device.RasterizerState = RasterizerState.CullNone;
            device.DepthStencilState = new DepthStencilState { DepthBufferEnable = true, DepthBufferFunction = CompareFunction.LessEqual };
            device.SetRenderTargets(NormalTarget, DepthTarget);
            device.Clear(new Color(0, 255, 255));

            Effect current = terrain.Effect;
            terrain.Effect = depthNormalEffect;
            terrain.Draw(view, lightProjection);
            terrain.Effect = current;

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


            BasicEffect current = (BasicEffect)lightModel.Meshes[0].MeshParts[0].Effect;
            foreach (PointLight light in pointLights)
            {
                lightingEffect.Parameters["LightColor"].SetValue(light.Diffuse * 2);
                lightingEffect.Parameters["LightPosition"].SetValue(light.Position);
                lightingEffect.Parameters["LightAttenuation"].SetValue(light.Range);

                lightModel.Meshes[0].MeshParts[0].Effect = lightingEffect;
                Matrix wvp = (Matrix.CreateScale(light.Range / 10) * Matrix.CreateTranslation(light.Position)) * viewProjection;
                lightingEffect.Parameters["WorldViewProjection"].SetValue(wvp);

                device.RasterizerState = RasterizerState.CullCounterClockwise;
                lightModel.Meshes[0].Draw();
            }

            lightModel.Meshes[0].MeshParts[0].Effect = current;

            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;

            device.SetRenderTarget(null);
        }

    }
}
