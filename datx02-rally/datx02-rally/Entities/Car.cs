using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace datx02_rally
{
    public class Car : ITargetNode
    {
        public Model Model { get; set; }

        /// <summary>
        /// Set only for repositioning, not driving
        /// </summary>
        public float Rotation { get; set; }

        public Vector3 Normal { get; set; }
        private Matrix normalMatrix;
        
        /// <summary>
        /// Set only for repositioning, not driving
        /// </summary>
        public Vector3 Position { get; set; }

        private float wheelRadius;
        public float WheelRotationX { get; private set; }

        public Matrix TranslationMatrix { get { return Matrix.CreateTranslation(Position); } }
        public Matrix RotationMatrix { get { return Matrix.CreateRotationY(Rotation) * normalMatrix; } }

        /// <summary>
        /// This is set from outside to make the car go forward or backward.
        /// </summary>
        public float Speed { get; set; }
        /// <summary>
        /// This is set from outside to make the car turn.
        /// </summary>
        public float WheelRotationY { get; set; }

        // Constants
        public float MaxSpeed { get; protected set; }
        public float Acceleration { get; protected set; }
        public float MaxWheelTurn { get; protected set; }
        public float TurnSpeed { get; protected set; }
        
        // Physics

        public BoundingBox BBox { get; protected set; }

        // Distance between wheelaxis.
        private float L = 40.197f;

        // Constructor

        public Car(Model model, float wheelRadius)
        {
            this.Model = model;

            BBox = GetBoundingBox(model);

            this.Position = Vector3.Zero;

            // Constants
            Acceleration = .35f;
            MaxSpeed = 20;
            MaxWheelTurn = MathHelper.PiOver4 / 1.7f;
            TurnSpeed = .005f;

            Normal = Vector3.Up;

            this.wheelRadius = wheelRadius;
        }

        protected BoundingBox GetBoundingBox(Model model)
        {
            // Initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            // For each mesh of the model
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Vertex buffer parameters
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    // Get vertex data as float
                    float[] vertexData = new float[vertexBufferSize / sizeof(float)];
                    meshPart.VertexBuffer.GetData<float>(vertexData);

                    // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        Vector3 transformedPosition = new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]);

                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);
                    }
                }
            }

            // Create and return bounding box
            return new BoundingBox(min, max);
        }

        public Vector3 previousPos;
        public float previousRotation;

        public void Update()
        {
            previousPos = Position;
            previousRotation = Rotation;

            Vector3 forward = Vector3.Transform(Vector3.Forward,
                Matrix.CreateRotationY(Rotation));
            Vector3 axisOffset = L * forward;

            Vector3 front = Position + axisOffset;
            Vector3 back = Position - axisOffset;

            front += Speed * Vector3.Transform(Vector3.Forward,
                Matrix.CreateRotationY(Rotation + WheelRotationY));
            back += Speed * forward;

            Vector3 oldPos = Position;
            Position = (front + back) / 2;
            Rotation = (float)Math.Atan2(back.X - front.X, back.Z - front.Z);
            WheelRotationX += (Speed < 0 ? 1 : -1) * (Position - oldPos).Length() / wheelRadius;

            normalMatrix = Matrix.Lerp(normalMatrix, Vector3.Up.GetRotationMatrix(Normal), .2f);

        }
    }
}