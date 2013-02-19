using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally
{
    class CarControlComponent : GameComponent
    {
        private InputComponent input;
        public Dictionary<Player, Car> Cars = new Dictionary<Player, Car>();
        //public Car Car { get; set; }

        public CarControlComponent(Game1 game)
            : base(game)
        {
            this.input = game.GetService<InputComponent>();
        }

        public void AddCar(Player player, Model model, float wheelRadius)
        {
            if (model == null) 
            {
                model = Game.Content.Load<Model>(@"Models/porsche");
                wheelRadius = 10.4725f;
            }
            Cars[player] = Game1.GetInstance().MakeCar();
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var playerCarPair in Cars)
            {
                if (playerCarPair.Key.LOCAL_PLAYER)
                    UpdateLocalCar(playerCarPair.Value);
                else
                    UpdateNetworkCar(playerCarPair.Key, playerCarPair.Value);
            }
            

            base.Update(gameTime);
        }

        private void UpdateNetworkCar(Player player, Car car)
        {
            car.Position = player.Position;
        }

        private void UpdateLocalCar(Car car)
        {
            if (Game.GetService<CameraComponent>().CurrentCamera is ThirdPersonCamera)
            {
                //Accelerate
                car.Speed = Math.Min(car.Speed + car.Acceleration *
                    (input.GetState(Input.Thrust) -
                    input.GetState(Input.Brake)), car.MaxSpeed);

                // Turn Wheel
                car.WheelRotationY += (input.GetState(Input.Steer) * car.TurnSpeed);
                car.WheelRotationY = MathHelper.Clamp(car.WheelRotationY, -car.MaxWheelTurn, car.MaxWheelTurn);
                if (Math.Abs(car.WheelRotationY) > MathHelper.Pi / 720)
                    car.WheelRotationY *= .95f;
                else
                    car.WheelRotationY = 0;
            }

            //Friction if is not driving
            float friction = .97f; // 0.995f;
            if (Math.Abs(input.GetState(Input.Thrust)) < 0.001f)
            {
                car.Speed *= friction;
            }
        }
    }
}
