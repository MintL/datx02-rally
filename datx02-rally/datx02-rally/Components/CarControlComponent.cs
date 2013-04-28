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

        public CarControlComponent(Game1 game)
            : base(game)
        {
            this.input = game.GetService<InputComponent>();
            this.simulationStrategy = new DeadReckoningStrategy();
        }

        public void AddCar(Player player, Model model)
        {
            if (model == null)
                model = Game.Content.Load<Model>(@"Models/porsche");

            if (Game1.GetInstance().currentState == GameState.Gameplay)
                Cars[player] = (Game1.GetInstance().currentView as GamePlayView).MakeCar();
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
            simulationStrategy.UpdatePosition(player, car);
            car.Position = player.Position;
            car.Rotation = player.Rotation;
            car.Speed = player.Speed;
            car.Update();
        }

        private void UpdateLocalCar(Player player, Car car)
        {
            // Calculate real position
            if (Game.GetService<CameraComponent>().CurrentCamera is ThirdPersonCamera)
            {
                //Accelerate
                car.Speed = Math.Min(car.Speed + car.Acceleration *
                    (input.GetState(Input.Thrust) -
                    input.GetState(Input.Brake)), car.MaxSpeed);

                // Turn Wheel
                car.WheelRotationY += (input.GetState(Input.Steer) * car.TurnSpeed);
                car.WheelRotationY = MathHelper.Clamp(car.WheelRotationY, -car.MaxWheelTurn, car.MaxWheelTurn);
                if (Math.Abs(car.WheelRotationY) > .001f)
                    car.WheelRotationY *= .95f;
                else
                    car.WheelRotationY = 0;
            }

            //Friction if is not driving
            float friction = .97f;
            if (Math.Abs(input.GetState(Input.Thrust)) < .001f)
            {
                car.Speed *= friction;
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
