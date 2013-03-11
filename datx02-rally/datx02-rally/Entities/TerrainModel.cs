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
        public Vector2 TextureCoordinate;
        public Vector4 TexWeights;

        public static int SizeInBytes = (3 + 3 + 2 + 4) * sizeof(float);
        public static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
             new VertexElement( 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
             new VertexElement( sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0 ),
             new VertexElement( sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
             new VertexElement( sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1 )
        );

        public MultitexturedVertex(Vector3 position, Vector3 normal, Vector2 texCoordinate, Vector4 texWeights)
        {
            Position = position;
            Normal = normal;
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

        public Vector3 StartPoint { get; set; }
        public Vector3 EndPoint { get; set; }

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
                        (1 - roadMap[xOffset + x, zOffset + z]) * MathHelper.Clamp(1 - Math.Abs(heightMap[xOffset + x, zOffset + z] - 0.2f) / 0.4f, 0, 1),
                        (1 - roadMap[xOffset + x, zOffset + z]) * MathHelper.Clamp(1 - Math.Abs(heightMap[xOffset + x, zOffset + z] - 0.7f) / 0.2f, 0, 1),
                        (1 - roadMap[xOffset + x, zOffset + z]) * MathHelper.Clamp(1 - Math.Abs(heightMap[xOffset + x, zOffset + z] - 1) / 0.3f, 0, 1)
                    );

                    textureWeights.Normalize();

                    vertices[z * terrainSize + x] = new MultitexturedVertex(
                        terrainScale * new Vector3(
                            (terrainStart + xOffset + x), // X
                            heightMap[xOffset + x, zOffset + z], // Y
                            (terrainStart + zOffset + z)), // Z
                        Vector3.Zero, // Normal
                        new Vector2(x / 21f, z / 21f),
                        textureWeights);

                }
            }

            BoundingBox = BoundingBox.CreateFromPoints(vertices.Select(vert => vert.Position));

            //var zeroPlane = new Plane(Vector3.Up, 0);
            //StartPoint = vertices.First().Position + directionalLight.Direction * new Ray(vertices.First().Position, directionalLight.Direction).Intersects(zeroPlane).Value;
            //EndPoint = vertices.Last().Position + directionalLight.Direction * new Ray(vertices.Last().Position, directionalLight.Direction).Intersects(zeroPlane).Value;

            StartPoint = vertices.First().Position;
            EndPoint = vertices.Last().Position;

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
            this.ShadowMap = new RenderTarget2D(device, 2048, 2048, false, SurfaceFormat.Color, DepthFormat.Depth24);

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
    }
}
