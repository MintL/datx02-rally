using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
using datx02_rally.GameLogic;

/*Implementation from the following link: 
 * 
 *  http://www.float4x4.net/index.php/2010/06/generating-realistic-and-playable-terrain-height-maps/
 */

namespace datx02_rally.MapGeneration
{
    class HeightMap
    {
        public float[,] Heights { get; set; }
        private PerlinGenerator Perlin { get; set; }
        public int Size { get; set; }

        public HeightMap(int size)
        {
            Size = size;
            Heights = new float[Size, Size];
            Perlin = new PerlinGenerator(0);
        }

        public void AddPerlinNoise(float f)
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    
                    Heights[i, j] += (Perlin.Noise(f * i / (float)Size, f * j / (float)Size, 0)) + 0.5f;
                    if (Heights[i, j] > 1 || Heights[i, j] < 0) 
                    {
                        if (Heights[i, j] > 1)
                        {
                            Heights[i, j] -= (Heights[i, j] - 1) / 2;
                        }
                        else 
                        {
                            Heights[i, j] += Math.Abs(Heights[i, j]) / 2;
                        }

                    }
                }
            }
        }

        public void Perturb(float f, float d)
        {
            int u, v;
            float[,] temp = new float[Size, Size];
            for (int i = 0; i < Size; ++i)
            {
                for (int j = 0; j < Size; ++j)
                {
                    u = i + (int)(Perlin.Noise(f * i / (float)Size, f * j / (float)Size, 0) * d);
                    v = j + (int)(Perlin.Noise(f * i / (float)Size, f * j / (float)Size, 1) * d);
                    if (u < 0) u = 0; if (u >= Size) u = Size - 1;
                    if (v < 0) v = 0; if (v >= Size) v = Size - 1;
                    temp[i, j] = Heights[u, v];
                }
            }
            Heights = temp;
        }

        public void Erode(float smoothness)
        {
            for (int i = 1; i < Size - 1; i++)
            {
                for (int j = 1; j < Size - 1; j++)
                {
                    float d_max = 0.0f;
                    int[] match = { 0, 0 };

                    for (int u = -1; u <= 1; u++)
                    {
                        for (int v = -1; v <= 1; v++)
                        {
                            if (Math.Abs(u) + Math.Abs(v) > 0)
                            {
                                float d_i = Heights[i, j] - Heights[i + u, j + v];
                                if (d_i > d_max)
                                {
                                    d_max = d_i;
                                    match[0] = u; match[1] = v;
                                }
                            }
                        }
                    }

                    if (0 < d_max && d_max <= (smoothness / (float)Size))
                    {
                        float d_h = 0.5f * d_max;
                        Heights[i, j] -= d_h;
                        Heights[i + match[0], j + match[1]] += d_h;
                    }
                }
            }
        }

        public void Smoothen()
        {
            for (int i = 1; i < Size - 1; ++i)
            {
                for (int j = 1; j < Size - 1; ++j)
                {
                    float total = 0.0f;
                    for (int u = -1; u <= 1; u++)
                    {
                        for (int v = -1; v <= 1; v++)
                        {
                            total += Heights[i + u, j + v];
                        }
                    }

                    Heights[i, j] = total / 9.0f;
                }
            }
        }

        public float[,] Generate()
        {   
            AddPerlinNoise(8.0f);

            Perturb(40.0f, 40.0f);
            

            for (int i = 0; i < 10; i++)
                Erode(20.0f); 
            
            Smoothen();

            return Heights;
        }

        public void Store(GraphicsDevice graphicsDevice) 
        { 
            //string imageDestination = "z:/GeneratedMaps/new.bmp";
            
            //float lowest = 1, highest = 0;

            //FileStream stream = new FileStream(imageDestination, FileMode.Create);


            //Texture2D mapImage = new Texture2D(graphicsDevice, Size, Size, false, SurfaceFormat.Color);

            //Color[] heightMapColors = new Color[Heights.Length];

            //for (int x = 0; x < Size; x++)
            //{
            //    for (int y = 0; y < Size; y++)
            //    {
            //        var next = Heights[x, y];
                    
            //        lowest = Math.Min(lowest, next);
            //        highest = Math.Max(highest, next);

            //        byte colorData = (byte)(next * 255);

            //        heightMapColors[x + y * Size].R = colorData;
            //        heightMapColors[x + y * Size].G = colorData;
            //        heightMapColors[x + y * Size].B = colorData;
            //        heightMapColors[x + y * Size].A = 255;
            //    }
            //}

            //mapImage.SetData<Color>(heightMapColors);

            //mapImage.SaveAsPng(stream, Size, Size);

            //Console.WriteLine("Lowest noise: " + lowest);
            //Console.WriteLine("Highest noise: " + highest);

            //stream.Close();
        }

        public void loadMap(RaceTrack track, int triangleSize)
        {
            Vector3 trackCoordinate = Vector3.Zero;
            Vector3 offsetVector = new Vector3(Size/2, 0, Size/2);

            for (float f = 0; f <= 1; f += 0.001f)
            {
                trackCoordinate = track.Curve.GetPoint(f);

                trackCoordinate = trackCoordinate / triangleSize;
                trackCoordinate += offsetVector;



                for (int x = -4; x < 4; x++)
                {
                    for (int z = -4; z < 4; z++)
                    {
                        Heights[(int)trackCoordinate.X + x, (int)trackCoordinate.Z + z] = 0.5f;
                    }
                }
            }
        }
    }
}
