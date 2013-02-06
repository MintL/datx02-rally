using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace datx02_rally.MapGeneration
{
    class TerrainGenerator
    {

    //    int terrainWidth;
    //    int terrainLength;
    //    float[,] heightData;

    //    VertexBuffer terrainVertexBuffer;
    //    IndexBuffer terrainIndexBuffer;
    //    VertexDeclaration terrainVertexDeclaration;

    //    VertexBuffer waterVertexBuffer;
    //    VertexDeclaration waterVertexDeclaration;

    //    VertexBuffer treeVertexBuffer;
    //    VertexDeclaration treeVertexDeclaration;


    //    Effect effect;




    //    VertexPositionTexture[] fullScreenVertices;
    //    VertexDeclaration fullScreenVertexDeclaration;


    //    // -----------------------------------

    //    public TerrainGenerator()
    //    {

    //    }
        
    //    GraphicsDevice device;

    //    public void LoadContent(GraphicsDevice device, ContentManager content)
    //    {
    //        this.device = device;
    //        effect = content.Load<Effect>(@"Effects/CarShading");

    //        LoadVertices(content);
    //    }

    //    private void LoadVertices(ContentManager Content)
    //     {

    //        Texture2D heightMap = Content.Load<Texture2D> ("heightmapCopy"); 
    //        LoadHeightData(heightMap);
    //        VertexPositionNormalTexture[] terrainVertices = SetUpTerrainVertices();
    //        int[] terrainIndices = SetUpTerrainIndices();
    //        terrainVertices = CalculateNormals(terrainVertices, terrainIndices);
    //        CopyToTerrainBuffers(terrainVertices, terrainIndices);
    //        terrainVertexDeclaration = VertexPositionNormalTexture.VertexDeclaration;

    //        //SetUpWaterVertices();
    //        //waterVertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);


    //        //Texture2D treeMap = Content.Load<Texture2D> ("treeMap");
    //        //List<Vector3> treeList = GenerateTreePositions(treeMap, terrainVertices);            CreateBillboardVerticesFromList(treeList);

 
    //         fullScreenVertices = SetUpFullscreenVertices();
    //         fullScreenVertexDeclaration = VertexPositionTexture.VertexDeclaration;
    //     }


    //    private void LoadHeightData(Texture2D heightMap)
    //    {
    //        float minimumHeight = float.MaxValue;
    //        float maximumHeight = float.MinValue;

    //        terrainWidth = heightMap.Width;
    //        terrainLength = heightMap.Height;

    //        Color[] heightMapColors = new Color[terrainWidth * terrainLength];
    //        heightMap.GetData(heightMapColors);

    //        heightData = new float[terrainWidth, terrainLength];
    //        for (int x = 0; x < terrainWidth; x++)
    //            for (int y = 0; y < terrainLength; y++)
    //            {
    //                heightData[x, y] = heightMapColors[x + y * terrainWidth].R;
    //                if (heightData[x, y] < minimumHeight) minimumHeight = heightData[x, y];
    //                if (heightData[x, y] > maximumHeight) maximumHeight = heightData[x, y];
    //            }

    //        for (int x = 0; x < terrainWidth; x++)
    //            for (int y = 0; y < terrainLength; y++)
    //                heightData[x, y] = (heightData[x, y] - minimumHeight) / (maximumHeight - minimumHeight) * 30.0f;
    //    }

    //    private VertexPositionNormalTexture[] SetUpTerrainVertices()
    //    {
    //        VertexPositionNormalTexture[] terrainVertices = new VertexPositionNormalTexture[terrainWidth * terrainLength];

    //        for (int x = 0; x < terrainWidth; x++)
    //        {
    //            for (int y = 0; y < terrainLength; y++)
    //            {
    //                terrainVertices[x + y * terrainWidth].Position = new Vector3(x, heightData[x, y], -y);
    //                terrainVertices[x + y * terrainWidth].TextureCoordinate.X = (float)x / 30.0f;
    //                terrainVertices[x + y * terrainWidth].TextureCoordinate.Y = (float)y / 30.0f;
    //            }
    //        }

    //        return terrainVertices;
    //    }


    //    private int[] SetUpTerrainIndices()
    //    {
    //        int[] indices = new int[(terrainWidth - 1) * (terrainLength - 1) * 6];
    //        int counter = 0;
    //        for (int y = 0; y < terrainLength - 1; y++)
    //        {
    //            for (int x = 0; x < terrainWidth - 1; x++)
    //            {
    //                int lowerLeft = x + y * terrainWidth;
    //                int lowerRight = (x + 1) + y * terrainWidth;
    //                int topLeft = x + (y + 1) * terrainWidth;
    //                int topRight = (x + 1) + (y + 1) * terrainWidth;

    //                indices[counter++] = topLeft;
    //                indices[counter++] = lowerRight;
    //                indices[counter++] = lowerLeft;

    //                indices[counter++] = topLeft;
    //                indices[counter++] = topRight;
    //                indices[counter++] = lowerRight;
    //            }
    //        }

    //        return indices;
    //    }



    //    private VertexPositionNormalTexture[] CalculateNormals(VertexPositionNormalTexture[] vertices, int[] indices)
    //    {
    //        for (int i = 0; i < vertices.Length; i++)
    //            vertices[i].Normal = new Vector3(0, 0, 0);

    //        for (int i = 0; i < indices.Length / 3; i++)
    //        {
    //            int index1 = indices[i * 3];
    //            int index2 = indices[i * 3 + 1];
    //            int index3 = indices[i * 3 + 2];

    //            Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
    //            Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
    //            Vector3 normal = Vector3.Cross(side1, side2);

    //            vertices[index1].Normal += normal;
    //            vertices[index2].Normal += normal;
    //            vertices[index3].Normal += normal;
    //        }

    //        for (int i = 0; i < vertices.Length; i++)
    //            vertices[i].Normal.Normalize();

    //        return vertices;
    //    }

    //    private void CopyToTerrainBuffers(VertexPositionNormalTexture[] vertices, int[] indices)
    //    {
    //        terrainVertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture),
    //            vertices.Length, BufferUsage.None);
    //        terrainVertexBuffer.SetData(vertices);

    //        terrainIndexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
    //        terrainIndexBuffer.SetData(indices);
    //    }


    //    private Texture2D CreateStaticMap(int resolution)
    //    {
    //        Random rand = new Random();
    //        Color[] noisyColors = new Color[resolution * resolution];
    //        for (int x = 0; x < resolution; x++)
    //            for (int y = 0; y < resolution; y++)
    //                noisyColors[x + y * resolution] = new Color(new Vector3((float)rand.Next(1000) / 1000.0f, 0, 0));

    //        Texture2D noiseImage = new Texture2D(device, resolution, resolution, true, SurfaceFormat.Color);
    //        noiseImage.SetData(noisyColors);
    //        return noiseImage;
    //    }

    //    private VertexPositionTexture[] SetUpFullscreenVertices()
    //    {
    //        VertexPositionTexture[] vertices = new VertexPositionTexture[4];

    //        vertices[0] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 1));
    //        vertices[1] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 1));
    //        vertices[2] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 0));
    //        vertices[3] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 0));

    //        return vertices;
    //    }




    //    public void DrawTerrain(Matrix view, Matrix projection)
    //    {
    //        //effect.CurrentTechnique = effect.Techniques["MultiTextured"];
    //        //effect.Parameters["xTexture0"].SetValue(texture);
    //        //effect.Parameters["xTexture1"].SetValue(texture);
    //        //effect.Parameters["xTexture2"].SetValue(texture);
    //        //effect.Parameters["xTexture3"].SetValue(texture);

    //        Matrix worldMatrix = Matrix.Identity;
    //        effect.Parameters["World"].SetValue(Matrix.Identity);
    //        effect.Parameters["View"].SetValue(view);
    //        effect.Parameters["Projection"].SetValue(projection);

    //        //effect.Parameters["xEnableLighting"].SetValue(true);
    //        //effect.Parameters["xAmbient"].SetValue(0.4f);
    //        //effect.Parameters["xLightDirection"].SetValue(new Vector3(-0.5f, -1, -0.5f));

    //        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
    //        {
    //            pass.Apply();

    //            device.SetVertexBuffer(terrainVertexBuffer);
    //            device.Indices = terrainIndexBuffer;
    //            //device.Vert VertexDeclaration = terrainVertexDeclaration;

    //            device.DrawPrimitives(PrimitiveType.TriangleList, 0, terrainVertexBuffer.VertexCount);

    //            //int noVertices = terrainVertexBuffer. SizeInBytes / VertexMultitextured.SizeInBytes;
    //            //int noTriangles = terrainIndexBuffer.SizeInBytes / sizeof(int) / 3;
    //            //device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, noVertices, 0, noTriangles);

    //        }
    //    }



    //}

    //public struct VertexMultitextured
    //{
    //    public Vector3 Position;
    //    public Vector3 Normal;
    //    public Vector4 TextureCoordinate;
    //    public Vector4 TexWeights;

    //    public static int SizeInBytes = (3 + 3 + 4 + 4) * sizeof(float);
    //    public static VertexElement[] VertexElements = new VertexElement[]
    //      {
    //          new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
    //          new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
    //          new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
    //          new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1)
    //      };


    }

}
