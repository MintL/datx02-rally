using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace datx02_rally
{
    class CameraComponent : GameComponent
    {
        private List<Camera> cameras = new List<Camera>();
        private int currentCamera;

        public Camera CurrentCamera { get { return cameras[currentCamera]; } }

        public Matrix View { get { return cameras[currentCamera].View; } }

        public CameraComponent(Game1 game) : base(game)
        {
        }

        public void AddCamera(Camera camera)
        {
            cameras.Add(camera);
        }

        public override void Update(GameTime gameTime)
        {
            var input = Game.GetService<InputComponent>();
            if (input.GetPressed(Input.ChangeCamera))
                currentCamera++;
            if (currentCamera == cameras.Count)
                currentCamera = 0;
            cameras[currentCamera].Update(gameTime);
            base.Update(gameTime);
        }
    }
}
