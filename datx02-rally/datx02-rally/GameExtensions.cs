using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Particle3DSample;

namespace datx02_rally
{
    /// <summary>
    /// Static class to provide extensionmethods for game management. If you don't know what an extension method is, ask me. 
    /// /Marcus
    /// </summary>
    static class GameExtensions
    {
        #region GameServices

        public static T GetService<T>(this Game game)
        {
            return (T)game.Services.GetService(typeof(T));
        }

        #endregion

        #region Vectors and Matrices

        public static Matrix GetRotationMatrix(this Vector3 source, Vector3 target)
        {
            float dot = Vector3.Dot(source, target);
            if (!float.IsNaN(dot))
            {
                float angle = (float)Math.Acos(dot);
                if (!float.IsNaN(angle))
                {
                    Vector3 cross = Vector3.Cross(source, target);
                    if (cross.LengthSquared() == 0)
                        return Matrix.Identity;
                    cross.Normalize();
                    Matrix rotation = Matrix.CreateFromAxisAngle(cross, angle);
                    return rotation;
                }
            }
            return Matrix.Identity;
        }

        public static Vector3 GetXZProjection(this Vector3 source, bool normalized)
        {
            float length = normalized ? (float)Math.Sqrt(source.X * source.X + source.Z * source.Z) : 1;
            return new Vector3(source.X / length, 0, source.Z / length);
        }

        #endregion

        #region Bounding Volumes

        public static BoundingBox Translate(this BoundingBox box, Matrix translationMatrix)
        {
            return new BoundingBox(Vector3.Transform(box.Min, translationMatrix), Vector3.Transform(box.Max, translationMatrix));
        }

        /// <summary>
        /// Spawns particles on the surface of the box.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="yellowSystem"></param>
        public static void IlluminateBoundingBox(this BoundingBox b, ParticleSystem yellowSystem)
        {
            for (int i = 0; i < 20; i++)
            {
                int face = UniversalRandom.GetInstance().Next(6);
                float u = (float)UniversalRandom.GetInstance().NextDouble(),
                      v = (float)UniversalRandom.GetInstance().NextDouble();
                var diff = b.Max - b.Min;
                switch (face)
                {
                    case 0:
                        yellowSystem.AddParticle(b.Min + new Vector3(u * diff.X, v * diff.Y, 0), Vector3.Zero);
                        break;
                    case 1:
                        yellowSystem.AddParticle(b.Min + new Vector3(0, v * diff.Y, u * diff.Z), Vector3.Zero);
                        break;
                    case 2:
                        yellowSystem.AddParticle(b.Min + new Vector3(diff.X, v * diff.Y, u * diff.Z), Vector3.Zero);
                        break;
                    case 3:
                        yellowSystem.AddParticle(b.Min + new Vector3(u * diff.X, v * diff.Y, diff.Z), Vector3.Zero);
                        break;
                    case 4:
                        yellowSystem.AddParticle(b.Min + new Vector3(u * diff.X, 0, v * diff.Z), Vector3.Zero);
                        break;
                    case 5:
                        yellowSystem.AddParticle(b.Min + new Vector3(u * diff.X, diff.Y, v * diff.Z), Vector3.Zero);
                        break;
                }
            }
        }


        #endregion
    }
}
