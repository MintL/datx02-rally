using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using datx02_rally.GameLogic;

namespace datx02_rally.Entities
{
    class NavMeshTriangle
    {
        public Vector3[] vertices;
        public Vector3 baryCenter;
        public Vector3 normal;

        public Vector3 ab, ac;

        public Plane trianglePlane;

        public NavMeshTriangle(params Vector3[] verts)
        {
            if (verts.Length != 3)
                throw new ArgumentException("Not 3 vertices!");

            vertices = verts;
            normal = Vector3.Normalize(Vector3.Cross(ab = (vertices[1] - vertices[0]), ac = (vertices[2] - vertices[0])));
            float oneThird = 1 / 3f;
            baryCenter = vertices[0] + oneThird * ab + oneThird * ac;

            trianglePlane = new Plane(verts[0], verts[1], verts[2]);
        }
    }

    struct NavMeshTriangleVertex : IVertexType
    {
        public Vector3 Position;

        public static VertexDeclaration VertexDeclaration =
            new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0));

        public NavMeshTriangleVertex(Vector3 position)
        {
            Position = position;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }

    class NavMesh
    {
        private GraphicsDevice device;

        // public NavMeshTriangleVertex[] vertices;
        public VertexPositionColor[] vertices;

        private VertexBuffer vertexbuffer;

        private IndexBuffer indexBuffer;

        public BasicEffect Effect { get; set; }

        public NavMeshTriangle[] triangles;

        public NavMesh(GraphicsDevice device, datx02_rally.GameLogic.Curve curve, int resolution, float width, Vector3 terrainScale)
        {
            this.device = device;

            #region Vertices

            //vertices = new NavMeshTriangleVertex[2 * resolution];
            vertices = new VertexPositionColor[2 * resolution];
            {
                int i = 0;
                var curveRasterization = new CurveRasterization(curve, resolution).Points;
                foreach (var point in curveRasterization)
                {
                    vertices[i++] = new VertexPositionColor(10 * Vector3.Up + point.Position - (terrainScale * width * point.Side), Color.White);
                    vertices[i++] = new VertexPositionColor(10 * Vector3.Up + point.Position + (terrainScale * width * point.Side), Color.White);
                }
            }

            #endregion

            #region Indicies

            int[] indices = new int[6 * resolution];

            int[] offsets = { 0, 1, 2, 2, 1, 0 };
            for (int i = 0; i < indices.Length; i++)
            {
                int v = i / 3 + offsets[i % 6];
                if (v >= vertices.Length)
                    v -= vertices.Length;
                indices[i] = v;
            }

            #endregion

            #region Triangles

            triangles = new NavMeshTriangle[2 * resolution];

            for (int i = 0; i < indices.Length; i += 3)
            {
                triangles[i / 3] = new NavMeshTriangle(vertices[indices[i]].Position,
                    vertices[indices[i + 1]].Position,
                    vertices[indices[i + 2]].Position);
            }


            // Smoothen out normals
            for (int i = 1; i < triangles.Length; i += 2)
            {
                int j = i - 1, c = i - 2, d = j - 2;
                if (c < 0)
                {
                    c += triangles.Length;
                    d += triangles.Length;
                }

                Vector3 normal = Vector3.Lerp(triangles[i].normal, triangles[j].normal, .5f);
                Vector3 others = Vector3.Lerp(triangles[c].normal, triangles[d].normal, .5f);
                normal = Vector3.Lerp(normal, others, .6f);
                triangles[i].normal = triangles[j].normal = normal;
            }

            #endregion

            #region Buffers

            indexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.None);
            indexBuffer.SetData(indices);
            indexBuffer.SetData(indices, 0, 6);

            vertexbuffer = new VertexBuffer(device,
                    typeof(VertexPositionColor), vertices.Length,
                    BufferUsage.None);
            vertexbuffer.SetData(vertices);
            vertexbuffer.SetData(vertices, 0, 6);

            #endregion

            Effect = new BasicEffect(device);
        }

        public void Draw(Matrix view, Matrix projection)
        {
            Effect.VertexColorEnabled = true;

            Effect.World = Matrix.Identity;
            Effect.View = view;
            Effect.Projection = projection;

            Effect.Alpha = .2f;

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
