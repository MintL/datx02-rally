using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SampleCargame
{
    class Car
    {
        public Model Model { get; private set; }
        public float Rotation { get; private set; }
        public Vector3 Position { get; private set; }
        private float wheelRadius;
        public float WheelRotationX { get; private set; }

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
            Acceleration = .05f;
            MaxSpeed = 10;
            MaxWheelTurn = MathHelper.PiOver4 / 1.7f;
            TurnSpeed = .05f;

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
            WheelRotationX -= (Position - oldPos).Length() / wheelRadius;
        }
    }
}