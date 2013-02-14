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

        public MultitexturedVertex[] vertices;
        private VertexBuffer vertexBuffer;

        private IndexBuffer indexBuffer;

        public Effect Effect { get; set; }

        public Matrix Projection { get; set; }

        public float HeightOffset { get; set; }
        


        
        public TerrainModel(Vector2 start, Vector2 end, float uvScale, GraphicsDevice device, Texture2D texture, Matrix projection, Matrix world)
        {
        }

        public TerrainModel(GraphicsDevice device, int width, int height, int triangleSize) : this(device, 0, width,0, height, triangleSize, 0, null) 
        {
        }

        public TerrainModel (GraphicsDevice device, int offsetX, int width, int offsetZ, int depth, float triangleSize, float heightScale, float[,] heightMap)
        {
            this.device = device;

            int vertexCount = width * depth,
                lastWidthIndex = width - 1,
                lastHeightIndex = depth - 1;

            float halfWidth = width / 2f,
                halfDepth = depth / 2f;

            HeightOffset = -1425;

            vertices = new MultitexturedVertex[vertexCount];

            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector4 textureWeights = new Vector4(
                        MathHelper.Clamp(1 - Math.Abs(heightMap[x, z] - 0) / 0.3f, 0, 1),
                        MathHelper.Clamp(1 - Math.Abs(heightMap[x, z] - 0.4f) / 0.2f, 0, 1),
                        MathHelper.Clamp(1 - Math.Abs(heightMap[x, z] - 0.7f) / 0.2f, 0, 1),
                        MathHelper.Clamp(1 - Math.Abs(heightMap[x, z] - 1) / 0.3f, 0, 1)
                    );
                    textureWeights.Normalize();

                    vertices[z * width + x] = new MultitexturedVertex(
                        new Vector3(
                            ((x - offsetX) - halfWidth) * triangleSize, // X
                            (heightMap != null ? heightScale * triangleSize * heightMap[x, z] : 0) + HeightOffset, // Y
                            ((z - offsetZ) - halfDepth) * triangleSize), // Z
                        Vector3.Zero, // Normal
                        new Vector2(x / 10f, z / 10f),
                        textureWeights);

                }
            }


            int flexIndice = 1;
            int rowIndex = 2;
            int[] indices = new int[(width-1) * (depth-1) * 6];
            indices[0] = 0;
            indices[1] = width;
            indices[2] = flexIndice;

            for (int i = 5; i <= indices.Length; i += 3)
            {
                indices[i - 2] = indices[i - 4];
                indices[i - 1] = indices[i - 3];

                if (i % 2 == 0)
                {
                    flexIndice -= (width-1);
                    indices[i] = flexIndice;
                }
                else
                {
                    flexIndice += width;
                    indices[i] = flexIndice;
                }
                if (i + 3 >= indices.Length)
                    break;
                else if (rowIndex * width -1 == indices[i])
                {
                    indices[i + 1] = flexIndice - width + 1;
                    indices[i + 2] = flexIndice + 1;
                    indices[i + 3] = flexIndice - width + 2;
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


            indexBuffer = new IndexBuffer(device, typeof(int), (width-1) * (depth-1) * 6, BufferUsage.None);
            indexBuffer.SetData(indices);

            vertexBuffer = new VertexBuffer(device,
                    typeof(MultitexturedVertex), vertices.Length,
                    BufferUsage.None);
            vertexBuffer.SetData(vertices);
        }

        
        public void Draw(Matrix view, Vector3 cameraPosition, DirectionalLight directionalLight, List<PointLight> pointLights)
        {

            Effect.Parameters["EyePosition"].SetValue(cameraPosition);
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["World"].SetValue(Matrix.Identity);
            Effect.Parameters["Projection"].SetValue(Projection);

            Effect.Parameters["NormalMatrix"].SetValue(Matrix.Invert(Matrix.Transpose(Matrix.Identity)));

            Effect.Parameters["DirectionalDirection"].SetValue(directionalLight.Direction);
            Effect.Parameters["DirectionalDiffuse"].SetValue(directionalLight.Diffuse);
            Effect.Parameters["DirectionalAmbient"].SetValue(directionalLight.Ambient);

            /*Vector3[] positions = new Vector3[pointLights.Count];
            Vector3[] diffuses = new Vector3[pointLights.Count];
            float[] ranges = new float[pointLights.Count];
            for (int i = 0; i < 10; i++)
            {
                positions[i] = pointLights[i].Position;
                diffuses[i] = pointLights[i].Diffuse;
                ranges[i] = pointLights[i].Range;
            }

            Effect.Parameters["LightPosition"].SetValue(positions);
            Effect.Parameters["LightDiffuse"].SetValue(diffuses);
            Effect.Parameters["LightRange"].SetValue(ranges);
            Effect.Parameters["NumLights"].SetValue(10);
            */
            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.Indices = indexBuffer;
                device.SetVertexBuffer(vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, vertexBuffer.VertexCount*2);
            }
        }
    }
}
