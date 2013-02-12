using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using datx02_rally.GameLogic;

namespace datx02_rally.Entities
{
    class NavMeshVisualizer
    {
        private GraphicsDevice device;

        public VertexPositionNormalTexture[] vertices;
        private VertexBuffer vertexbuffer;

        private IndexBuffer indexBuffer;

        public BasicEffect Effect { get; set; }

        public BBoxZone[] boxes;

        public struct BBoxZone
        {
            public int start, end;
            public BoundingBox box;
        }

        public NavMeshVisualizer(GraphicsDevice device, datx02_rally.GameLogic.Curve curve, int resolution, float width)
        {
            this.device = device;
            width /= 2f;

            #region Vertices

            vertices = new VertexPositionNormalTexture[2 * resolution];
            for (int i = 0; i < vertices.Length; i += 2)
            {
                float t = i / (float)vertices.Length;
                var position = curve.GetPoint(t);
                var side = Vector3.Normalize(Vector3.Cross((curve.GetPoint(t + .0001f) - position), Vector3.Up));

                vertices[i] = new VertexPositionNormalTexture(position - width * side, Vector3.Up, Vector2.Zero);
                vertices[i + 1] = new VertexPositionNormalTexture(position + width * side, Vector3.Up, Vector2.Zero);
            }

            #endregion

            #region Indicies

            int[] indices = new int[6 * resolution];
            int index = 0;
            for (int i = 0; i < 3 * resolution; i++)
            {
                if (i % 3 == 0 && i > 0)
                    index--;
                if (index == 2 * resolution)
                    index = 0;
                indices[i] = index;
                index++;
            }

            index = 1;
            for (int i = 0; i < 3 * resolution; i++)
            {
                if (i % 3 == 0 && i > 0)
                    index--;
                if (index == 2 * resolution)
                    index -= 2 * resolution;
                indices[3 * resolution + i] = index;
                index++;
            }

            #endregion

            #region 10-triangle spatial structure

            boxes = new BBoxZone[resolution / 10];

            for (int j = 0; j < resolution / 10; j++) // 20
            {
                int indexOffset = j * 60;
                Vector3 min = new Vector3(float.MaxValue), max = new Vector3(float.MinValue);
                for (int i = 0; i < (3 * resolution / 10); i++)
                {
                    var position = vertices[indices[indexOffset + i]].Position;
                    min = Vector3.Min(min, position);
                    max = Vector3.Max(max, position);
                }
                boxes[j] = new BBoxZone() { box = new BoundingBox(min, max), start = indexOffset, end = indexOffset + 60 };
            }

            #endregion

            #region Buffers

            indexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.None);
            indexBuffer.SetData(indices);

            vertexbuffer = new VertexBuffer(device,
                    typeof(VertexPositionNormalTexture), vertices.Length,
                    BufferUsage.None);
            vertexbuffer.SetData(vertices);

            #endregion

            Effect = new BasicEffect(device);
        }

        public void Draw(Matrix view, Matrix projection)
        {
            Effect.EnableDefaultLighting();
            Effect.World = Matrix.Identity;
            Effect.View = view;
            Effect.Projection = projection;

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.Indices = indexBuffer;
                device.SetVertexBuffer(vertexbuffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 
                    0, 0, 
                    vertexbuffer.VertexCount, 0, 
                    vertexbuffer.VertexCount * 2);

            }

        }

    }
}
