using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using datx02_rally.Menus;
using datx02_rally.Network;

namespace datx02_rally
{
    class CarControlComponent : GameComponent
    {
        private InputComponent input;
        private ISimulationStrategy simulationStrategy;
        public Dictionary<Player, Car> Cars = new Dictionary<Player, Car>();

        public CarControlComponent(GameManager game)
            : base(game)
        {
            this.input = game.GetService<InputComponent>();
            this.simulationStrategy = new DeadReckoningStrategy(Game);
        }

        public void AddCar(Player player, Model model, GamePlayView gameplay)
        {
            if (model == null)
                model = Game.Content.Load<Model>(@"Models/porsche");

            var car = gameplay.MakeCar();
            Cars[player] = car;
        }

        public void RemoveCar(Player player)
        {
            Cars.Remove(player);
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var playerCarPair in Cars)
            {                    
                if (playerCarPair.Key.LOCAL_PLAYER)
                    UpdateLocalCar(playerCarPair.Key, playerCarPair.Value);
                else
                    UpdateNetworkCar(playerCarPair.Key, playerCarPair.Value);
            }
            

            base.Update(gameTime);
        }

        /// <summary>
        /// Update a remote car with dead reckoning.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="car"></param>
        private void UpdateNetworkCar(Player player, Car car)
        {
            car.MaterialDiffuse = player.CarColor;
            simulationStrategy.UpdatePosition(player, car);
        }

        private void UpdateLocalCar(Player player, Car car)
        {
            // Calculate real position
            if (Game.GetService<CameraComponent>().CurrentCamera is ThirdPersonCamera)
            {
                //Accelerate
                car.Speed = Math.Min(car.Speed + (car.Acceleration - car.Speed * 0.00305f) *
                    input.GetState(Input.Thrust) -
                    input.GetState(Input.Brake) * car.Deacceleration, car.MaxSpeed);

                // Turn Wheel
                car.WheelRotationY += (input.GetState(Input.Steer) * (car.TurnSpeed - Math.Abs(car.Speed) * 0.00005f));
                car.WheelRotationY = MathHelper.Clamp(car.WheelRotationY, -car.MaxWheelTurn, car.MaxWheelTurn);
                if (Math.Abs(car.WheelRotationY) > .001f)
                    car.WheelRotationY *= .95f;
                else
                    car.WheelRotationY = 0;
            }

            //Friction if is not driving
            float friction = .995f;
            if (Math.Abs(input.GetState(Input.Thrust)) < .001f)
            {
                car.Speed *= (friction - Math.Abs(car.Speed-car.MaxSpeed) * 0.0005f);
            }

            // If in a network game, run parallell simulation of car
            var serverClient = Game.GetService<ServerClient>();
            if (Game.GetService<ServerClient>().connected)
            {
                bool isOverThreshold = simulationStrategy.UpdatePosition(player, car);

                // If car has deviated too much from simulated path, distribute position to server
                if (isOverThreshold)
                {
                    //Console.WriteLine(DateTime.Now + ": Local car over threshold, sending update!");
                    player.SetPosition(car.Position.X, car.Position.Y, car.Position.Z, car.Rotation, car.Speed, (byte)(player.LastReceived.Sequence + 1), DateTime.UtcNow);
                    serverClient.SendPlayerPosition();
                }
            }
            
        }
    }
}
