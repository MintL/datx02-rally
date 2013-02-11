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

        public TerrainModel(GraphicsDevice device, int width, int height, int triangleSize) : this(device, 0, width,0, height, triangleSize, null) {
        }

        public TerrainModel (GraphicsDevice device, int offsetX, int width, int offsetY, int height, float triangleSize, float[,] heightMap)
        {
            this.device = device;

            int vertexCount = width * height,
                lastWidthIndex = width - 1,
                lastHeightIndex = height - 1;

            float halfWidth = width / 2f,
                halfHeight = height / 2f;

            vertices = new VertexPositionNormalTexture[vertexCount];

            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    vertices[z * width + x] = new VertexPositionNormalTexture(
                        new Vector3(
                            ((x - offsetX) - halfWidth) * triangleSize, // X
                            (heightMap != null ? 13 * triangleSize * heightMap[x, z] : 0) - 1000, // Y
                            ((z - offsetY) - halfHeight) * triangleSize), // Z
                        Vector3.Zero, // Normal
                        new Vector2(x % 2, z % 2));

                }
            }

            // normals v1.

            //for (int z = 0; z < height; z++)
            //{
            //    for (int x = 0; x < width; x++)
            //    {
            //        var current = vertices[z * width + x];
            //        Vector3? prevX = null, nextX = null, prevZ = null, nextZ = null;

            //        if (x > 0)
            //            prevX = vertices[z * width + x - 1].Position - current.Position;

            //        if (x < lastWidthIndex)
            //            nextX = vertices[z * width + x + 1].Position - current.Position;

            //        if (z > 0)
            //            prevZ = vertices[(z - 1) * width + x].Position - current.Position;

            //        if (z < lastHeightIndex)
            //            nextZ = vertices[(z + 1) * width + x].Position - current.Position;

            //        Vector3 newNormal = Vector3.Zero;

            //        if (prevX.HasValue)
            //            newNormal += Vector3.Cross(Vector3.UnitZ, prevX.Value);
                                                                     
            //        if (nextX.HasValue)                              
            //            newNormal += Vector3.Cross(Vector3.UnitZ, nextX.Value);
                                                                     
            //        if (prevZ.HasValue)                              
            //            newNormal += Vector3.Cross(-Vector3.UnitX, prevZ.Value);
                                                                     
            //        if (nextZ.HasValue)                              
            //            newNormal += Vector3.Cross(Vector3.UnitX, nextZ.Value);

            //        vertices[z * width + x].Normal = newNormal;
            //    }
            //}


            int flexIndice = 1;
            int rowIndex = 2;
            int[] indices = new int[(width-1) * (height-1) * 6];
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


            //int[] indices = new int[(width-1) * (height-1) * 6];
            //indices[0] = 0;
            //indices[1] = width;

            //int offset = 0;

            //for (int i = 2; i < indices.Length + offset; i++)
            //{
            //    if (i % 3 < 2)
            //        indices[i - offset] = indices[i - 2 - offset];
            //    else
            //        indices[i - offset] = 1 + i / 2 + (i % 2 == 0 ? 0 : width);

            //    if (indices[i - offset] == 2 * width - 1)
            //    {
            //        i += 6;
            //        offset += 6;
            //    }
            //}

            //for (int i = 0; i < indices.Length; i++)
            //{
            //    System.Console.WriteLine(indices[i]);
            //}



            indexBuffer = new IndexBuffer(device, typeof(int), (width-1) * (height-1) * 6, BufferUsage.None);
            indexBuffer.SetData(indices);

            vertexbuffer = new VertexBuffer(device,
                    typeof(VertexPositionNormalTexture), vertices.Length,
                    BufferUsage.None);
            vertexbuffer.SetData(vertices);
        }

        
        public void Draw(Matrix view)
        {
            //var effect = Effect as BasicEffect;
            //effect.View = view;
            //effect.DiffuseColor = Color.LightBlue.ToVector3();

            Effect.Parameters["EyePosition"].SetValue(view.Translation);
            Effect.Parameters["WorldViewProj"].SetValue(view * Projection);

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
