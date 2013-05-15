using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally
{
    public struct MultitexturedVertex : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Binormal;
        public Vector3 Tangent;
        public Vector2 TextureCoordinate;
        public Vector4 TexWeights;

        public static int SizeInBytes = (3 + 3 + 3 + 3 + 2 + 4) * sizeof(float);
        public static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
             new VertexElement( 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
             new VertexElement( sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0 ),
             new VertexElement( sizeof(float) * 6, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0),
             new VertexElement( sizeof(float) * 9, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
             new VertexElement( sizeof(float) * 12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
             new VertexElement( sizeof(float) * 14, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1 )
        );

        public MultitexturedVertex(Vector3 position, Vector3 normal, Vector3 binormal, Vector3 tangent, Vector2 texCoordinate, Vector4 texWeights)
        {
            Position = position;
            Normal = normal;
            Binormal = binormal;
            Tangent = tangent;
            TextureCoordinate = texCoordinate;
            TexWeights = texWeights;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }

    public class TerrainModel
    {
        private GraphicsDevice device;

        private MultitexturedVertex[] vertices;
        
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;


        public Effect Effect { get; set; }

        public BoundingBox BoundingBox { get; set; }
        public BoundingBox MinBox { get; set; }

        //public Vector3[] Corner { get; set; }

        public Matrix ShadowMapView, ShadowMapProjection;
        public RenderTarget2D ShadowMap { get; set; }

        public TerrainModel(GraphicsDevice device, 
            int terrainSize, int terrainSegments, float terrainStart,
            int xOffset, int zOffset, Vector3 terrainScale, 
            float[,] heightMap, float[,] roadMap,
            Effect effect, DirectionalLight directionalLight)
        {
            this.device = device;

            int terrainSizeMinusOne = terrainSize;
            terrainSize++;

            int vertexCount = terrainSize * terrainSize;
            vertices = new MultitexturedVertex[vertexCount];

            for (int z = 0; z < terrainSize; z++)
            {
                for (int x = 0; x < terrainSize; x++)
                {
                    var textureWeights = new Vector4(
                        roadMap[xOffset + x, zOffset + z],
                        MathHelper.Clamp(1 - Math.Abs(heightMap[xOffset + x, zOffset + z] - 0.2f) / 0.4f, 0, 1),
                        MathHelper.Clamp(1 - Math.Abs(heightMap[xOffset + x, zOffset + z] - 0.7f) / 0.2f, 0, 1),
                        MathHelper.Clamp(1 - Math.Abs(heightMap[xOffset + x, zOffset + z] - 1) / 0.3f, 0, 1)
                    );

                    textureWeights.Normalize();

                    vertices[z * terrainSize + x] = new MultitexturedVertex(
                        terrainScale * new Vector3(
                            (terrainStart + xOffset + x), // X
                            heightMap[xOffset + x, zOffset + z], // Y
                            (terrainStart + zOffset + z)), // Z
                        Vector3.Zero, // Normal
                        Vector3.Zero, // Binormal
                        Vector3.Zero, // Tangent
                        new Vector2(x / 15f, z / 15f),
                        textureWeights);

                }
            }

            BoundingBox = BoundingBox.CreateFromPoints(vertices.Select(vert => vert.Position));

            //MinBox = BoundingBox.CreateFromPoints(new Vector3[] { BoundingBox.Min.GetXZProjection(false), BoundingBox.Max.GetXZProjection(false) });

            #region Indicies & Vertex normals setup

            int flexIndice = 1;
            int rowIndex = 2;
            int[] indices = new int[terrainSizeMinusOne * terrainSizeMinusOne * 6];
            indices[0] = 0;
            indices[1] = terrainSize;
            indices[2] = flexIndice;

            for (int i = 5; i <= indices.Length; i += 3)
            {
                indices[i - 2] = indices[i - 4];
                indices[i - 1] = indices[i - 3];

                if (i % 2 == 0)
                {
                    flexIndice -= terrainSizeMinusOne;
                    indices[i] = flexIndice;
                }
                else
                {
                    flexIndice += terrainSize;
                    indices[i] = flexIndice;
                }
                if (i + 3 >= indices.Length)
                    break;
                else if (rowIndex * terrainSize - 1 == indices[i])
                {
                    indices[i + 1] = flexIndice - terrainSize + 1;
                    indices[i + 2] = flexIndice + 1;
                    indices[i + 3] = flexIndice - terrainSize + 2;
                    flexIndice = indices[i + 3];
                    rowIndex++;
                    i += 3;
                }
            }

            for (int i = 0; i < indices.Length / 3; i++)
            {
                Vector3 firstvec = vertices[indices[i * 3]].Position;
                Vector3 secondvec = vertices[indices[i * 3 + 1]].Position;
                Vector3 thirdvec = vertices[indices[i * 3 + 2]].Position;
                Vector3 firstsub = secondvec - firstvec;
                Vector3 secondsub = thirdvec - firstvec;

                Vector3 normal;
                if(i % 2 == 0)
                {
                    normal = Vector3.Cross(firstsub, secondsub);
                }else{
                    normal = Vector3.Cross(secondsub, firstsub);
                }
                
                normal.Normalize();
                vertices[indices[i * 3]].Normal += normal;
                vertices[indices[i * 3 + 1]].Normal += normal;
                vertices[indices[i * 3 + 2]].Normal += normal;

                #region Binormal & Tangent
                // Calculate binormal and tangent to get normal map to work
                // Retrieved from http://xboxforums.create.msdn.com/forums/p/30443/172880.aspx
                Vector2 w1 = vertices[indices[i * 3]].TextureCoordinate;
                Vector2 w2 = vertices[indices[i * 3  + 1]].TextureCoordinate;
                Vector2 w3 = vertices[indices[i * 3 + 2]].TextureCoordinate;

                float s1 = w2.X - w1.X,
                    s2 = w3.X - w1.X,
                    t1 = w2.Y - w1.Y,
                    t2 = w3.Y - w1.Y;

                float r = 1.0f / (s1 * t2 - s2 * t1);
                Vector3 sdir = new Vector3((t2 * firstsub.X - t1 * secondsub.X) * r, (t2 * firstsub.Y - t1 * secondsub.Y) * r, (t2 * firstsub.Z - t1 * secondsub.Z) * r);
                Vector3 tdir = new Vector3((s1 * secondsub.X - s2 * firstsub.X) * r, (s1 * secondsub.Y - s2 * firstsub.Y) * r, (s1 * secondsub.Z - s2 * firstsub.Z) * r);

                // Gram-Schmidt orthogonalize
                Vector3 tangent = sdir - normal * Vector3.Dot(normal, sdir);
                tangent.Normalize();

                // Calculate handedness (here maybe you need to switch >= with <= depend on the geometry winding order)
                float tangentdir = (Vector3.Dot(Vector3.Cross(normal, sdir), tdir) <= 0.0f) ? 1.0f : -1.0f;
                Vector3 binormal = Vector3.Cross(normal, tangent) * tangentdir;
                
                // Set the values to the vertices
                vertices[indices[i * 3]].Tangent = tangent;
                vertices[indices[i * 3 + 1]].Tangent = tangent;
                vertices[indices[i * 3 + 2]].Tangent = tangent;

                vertices[indices[i * 3]].Binormal = binormal;
                vertices[indices[i * 3 + 1]].Binormal = binormal;
                vertices[indices[i * 3 + 2]].Binormal = binormal;
                #endregion
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Normal.Normalize();
            }

            #endregion

            indexBuffer = new IndexBuffer(device, typeof(int), terrainSizeMinusOne * terrainSizeMinusOne * 6, BufferUsage.None);
            indexBuffer.SetData(indices);

            vertexBuffer = new VertexBuffer(device,
                    typeof(MultitexturedVertex), vertices.Length,
                    BufferUsage.None);
            vertexBuffer.SetData(vertices);

            #region Effect

            this.Effect = effect;
            this.ShadowMap = new RenderTarget2D(device, 1024, 1024, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);

            #endregion


        }

        public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition, DirectionalLight directionalLight)
        {
            Effect.Parameters["EyePosition"].SetValue(cameraPosition);
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["World"].SetValue(Matrix.Identity);
            Effect.Parameters["Projection"].SetValue(projection);

            Effect.Parameters["ShadowMapView"].SetValue(ShadowMapView);
            Effect.Parameters["ShadowMapProjection"].SetValue(ShadowMapProjection);
            Effect.Parameters["ShadowMap"].SetValue(ShadowMap);

            Effect.Parameters["NormalMatrix"].SetValue(Matrix.Invert(Matrix.Transpose(Matrix.Identity)));

            Effect.Parameters["DirectionalDirection"].SetValue(directionalLight.Direction);
            Effect.Parameters["DirectionalDiffuse"].SetValue(directionalLight.Diffuse);
            Effect.Parameters["DirectionalAmbient"].SetValue(directionalLight.Ambient);
            

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.Indices = indexBuffer;
                device.SetVertexBuffer(vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
            }
        }

        public void Draw(Matrix view, Matrix projection)
        {
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["World"].SetValue(Matrix.Identity);
            Effect.Parameters["Projection"].SetValue(projection);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.Indices = indexBuffer;
                device.SetVertexBuffer(vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
            }
        }

        internal void Draw(BasicEffect btest)
        {
            foreach (EffectPass pass in btest.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.Indices = indexBuffer;
                device.SetVertexBuffer(vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
            }
        }
    }
}
