using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally
{
    public class TerrainModel
    {
        private GraphicsDevice device;

        private VertexPositionNormalTexture[] vertices;
        private VertexBuffer vertexbuffer;

        private IndexBuffer indexBuffer;

        public Effect Effect { get; set; }

        public Matrix Projection { get; set; }
        
        public TerrainModel(Vector2 start, Vector2 end, float uvScale, GraphicsDevice device, Texture2D texture, Matrix projection, Matrix world)
        {
        }

        public TerrainModel(GraphicsDevice device, int width, int height, int triangleSize) : this(device, 0, width,0, height, triangleSize, 0, null) {
        }

        public TerrainModel (GraphicsDevice device, int offsetX, int width, int offsetZ, int depth, float triangleSize, int heightScale, float[,] heightMap)
        {
            this.device = device;

            int vertexCount = width * depth,
                lastWidthIndex = width - 1,
                lastHeightIndex = depth - 1;

            float halfWidth = width / 2f,
                halfDepth = depth / 2f;

            vertices = new VertexPositionNormalTexture[vertexCount];

            for (int z = 0; z < depth; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    vertices[z * width + x] = new VertexPositionNormalTexture(
                        new Vector3(
                            ((x - offsetX) - halfWidth) * triangleSize, // X
                            (heightMap != null ? heightScale * triangleSize * heightMap[x, z] : 0) - 1425, // Y
                            ((z - offsetZ) - halfDepth) * triangleSize), // Z
                        Vector3.Zero, // Normal
                        new Vector2(x % 2, z % 2));

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
                if(i % 2 == 1)
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

            vertexbuffer = new VertexBuffer(device,
                    typeof(VertexPositionNormalTexture), vertices.Length,
                    BufferUsage.None);
            vertexbuffer.SetData(vertices);
        }

        
        public void Draw(Matrix view, Vector3 cameraPosition, DirectionalLight directionalLight)
        {

            Effect.Parameters["EyePosition"].SetValue(cameraPosition);
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["World"].SetValue(Matrix.Identity);
            Effect.Parameters["Projection"].SetValue(Projection);

            Effect.Parameters["DirectionalDirection"].SetValue(directionalLight.Direction);
            Effect.Parameters["DirectionalDiffuse"].SetValue(directionalLight.Diffuse);
            Effect.Parameters["DirectionalAmbient"].SetValue(directionalLight.Ambient);

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.Indices = indexBuffer;
                device.SetVertexBuffer(vertexbuffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexbuffer.VertexCount, 0, vertexbuffer.VertexCount*2);
            }
        }
    }
}
