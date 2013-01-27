using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SampleCargame
{
    public class PlaneModel
    {
        private GraphicsDevice device;

        private VertexPositionNormalTexture[] vertices;
        private VertexBuffer vertexbuffer;

        private BasicEffect effect;
        
        public Texture2D Texture { get { return effect.Texture; } set { effect.Texture = value; } }
        public Matrix Projection { get { return effect.Projection; } set { effect.Projection = value; } }
        public Matrix World { get { return effect.World; } set { effect.World = value; } }
        

        public PlaneModel(Vector2 start, Vector2 end, float uvScale, GraphicsDevice device, Texture2D texture, Matrix projection, Matrix world)
        {
            this.device = device;

            effect = new BasicEffect(device);
            effect.EnableDefaultLighting();
            if (texture != null)
            {
                effect.TextureEnabled = true;
                effect.Texture = texture;
            }
            effect.Projection = projection;
            effect.World = world;

            vertices = new VertexPositionNormalTexture[6];

            vertices[0] = new VertexPositionNormalTexture(
                new Vector3(start.X, 0, start.Y), Vector3.Up, new Vector2(0, 0));
            vertices[1] = new VertexPositionNormalTexture(
                new Vector3(end.X, 0, start.Y), Vector3.Up, new Vector2(uvScale, 0));
            vertices[2] = new VertexPositionNormalTexture(
                new Vector3(start.X, 0, end.Y), Vector3.Up, new Vector2(0, uvScale));

            vertices[3] = new VertexPositionNormalTexture(
                new Vector3(start.X, 0, end.Y), Vector3.Up, new Vector2(0, uvScale));
            vertices[4] = new VertexPositionNormalTexture(
                new Vector3(end.X, 0, start.Y), Vector3.Up, new Vector2(uvScale, 0));
            vertices[5] = new VertexPositionNormalTexture(
                new Vector3(end.X, 0, end.Y), Vector3.Up, new Vector2(uvScale, uvScale));

            vertexbuffer = new VertexBuffer(device,
                    typeof(VertexPositionNormalTexture), vertices.Length,
                    BufferUsage.None);
            vertexbuffer.SetData(vertices);
        }

        public void Draw(Matrix view)
        {
            this.Draw(view, null);
        }
        
        public void Draw(Matrix view, Color? colorTint)
        {
            effect.View = view;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                if (colorTint.HasValue)
                    effect.DiffuseColor = colorTint.Value.ToVector3();
                device.SetVertexBuffer(vertexbuffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }
    }
}
