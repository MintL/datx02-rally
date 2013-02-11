using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace datx02_rally
{
    class Car : ITargetNode
    {
        public Model Model { get; set; }

        /// <summary>
        /// Set only for repositioning, not driving
        /// </summary>
        public float Rotation { get; set; }
        
        /// <summary>
        /// Set only for repositioning, not driving
        /// </summary>
        public Vector3 Position { get; set; }

        private float wheelRadius;
        public float WheelRotationX { get; private set; }

        public Matrix TranslationMatrix { get { return Matrix.CreateTranslation(Position); } }
        public Matrix RotationMatrix { get { return Matrix.CreateRotationY(Rotation); } }

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

        // Distance between wheelaxis.
        private float L = 40.197f;

        // Constructor

        public Car(Model model, float wheelRadius)
        {
            this.Model = model;
            this.Position = Vector3.Zero;

            // Constants
            Acceleration = .35f;
            MaxSpeed = 40;
            MaxWheelTurn = MathHelper.PiOver4 / 1.7f;
            TurnSpeed = .005f;

            this.wheelRadius = wheelRadius;
        }

        public void Update()
        {
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

            
        }
    }
}