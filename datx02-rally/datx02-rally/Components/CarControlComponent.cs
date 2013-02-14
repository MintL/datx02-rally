using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace datx02_rally
{
    class CarControlComponent : GameComponent
    {
        private InputComponent input;
        public Car Car { get; set; }

        public CarControlComponent(Game1 game)
            : base(game)
        {
            this.input = game.GetService<InputComponent>();
        }

        public override void Update(GameTime gameTime)
        {
            if (Game.GetService<CameraComponent>().CurrentCamera is ThirdPersonCamera)
            {
                //Accelerate
                Car.Speed = Math.Min(Car.Speed + Car.Acceleration *
                    (input.GetState(Input.Thrust) -
                    input.GetState(Input.Brake)), Car.MaxSpeed);

                // Turn Wheel
                Car.WheelRotationY += (input.GetState(Input.Steer) * Car.TurnSpeed);
                Car.WheelRotationY = MathHelper.Clamp(Car.WheelRotationY, -Car.MaxWheelTurn, Car.MaxWheelTurn);
                if (Math.Abs(Car.WheelRotationY) > MathHelper.Pi / 720)
                    Car.WheelRotationY *= .95f;
                else
                    Car.WheelRotationY = 0;
            }

            //Friction if is not driving
            float friction = .97f; // 0.995f;
            if (Math.Abs(input.GetState(Input.Thrust)) < 0.001f)
            {
                Car.Speed *= friction;
            }

            base.Update(gameTime);
        }
    }
}
