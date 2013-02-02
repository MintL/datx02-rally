using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace datx02_rally
{
    public enum Input { Thrust, Brake, Steer, ChangeController, ChangeCamera, CameraX, CameraY, Exit };
    public enum Controller { Keyboard, GamePad };

    class InputComponent : GameComponent
    {
        private KeyboardState keyboard;
        private KeyboardState previousKeyboard;
        private GamePadState gamePad;
        private GamePadState previousGamePad;

        public Controller CurrentController { get; set; }

        public InputComponent(Game1 game)
            : base(game)
        {
        }

        public float GetState(Input input)
        {
            if (CurrentController == Controller.Keyboard)
            {
                switch (input)
                {
                    case Input.Thrust:
                        return keyboard.IsKeyDown(Keys.W) ? 1.0f : 0.0f;
                    case Input.Brake:
                        return keyboard.IsKeyDown(Keys.S) ? 1.0f : 0.0f;
                    case Input.Steer:
                        return (keyboard.IsKeyDown(Keys.A) ? 1.0f : 0.0f) -
                            (keyboard.IsKeyDown(Keys.D) ? 1.0f : 0.0f);
                    case Input.CameraX:
                        return (keyboard.IsKeyDown(Keys.Right) ? 1.0f : 0.0f) -
                            (keyboard.IsKeyDown(Keys.Left) ? 1.0f : 0.0f);
                    case Input.CameraY:
                        return (keyboard.IsKeyDown(Keys.Up) ? 1.0f : 0.0f) -
                            (keyboard.IsKeyDown(Keys.Down) ? 1.0f : 0.0f);
                }
            }
            else if (CurrentController == Controller.GamePad)
            {
                switch (input)
                {
                    case Input.Thrust:
                        return gamePad.Triggers.Right;
                    case Input.Brake:
                        return gamePad.Triggers.Left;
                    case Input.Steer:
                        return -gamePad.ThumbSticks.Left.X;
                    case Input.CameraX:
                        return -gamePad.ThumbSticks.Right.X;
                    case Input.CameraY:
                        return -gamePad.ThumbSticks.Right.Y;
                }
            }
            return 0.0f;
        }

        public bool GetPressed(Input input)
        {
            if (CurrentController == Controller.Keyboard)
            {
                switch (input)
                {
                    case Input.ChangeController:
                        return previousKeyboard.IsKeyUp(Keys.F1) && keyboard.IsKeyDown(Keys.F1);
                    case Input.ChangeCamera:
                        return previousKeyboard.IsKeyUp(Keys.C) && keyboard.IsKeyDown(Keys.C);
                    case Input.Exit:
                        return keyboard.IsKeyDown(Keys.Escape);
                }
            }
            else if (CurrentController == Controller.GamePad)
            {
                switch (input)
                {
                    case Input.ChangeController:
                        return previousGamePad.IsButtonUp(Buttons.Start) && gamePad.IsButtonDown(Buttons.Start);
                    case Input.ChangeCamera:
                        return previousGamePad.IsButtonUp(Buttons.RightStick) && gamePad.IsButtonDown(Buttons.RightStick);
                    case Input.Exit:
                        return gamePad.IsButtonDown(Buttons.Back);
                }
            }
            return false;
        }

        public void UpdatePreviousState()
        {
            if (CurrentController == Controller.Keyboard)
                previousKeyboard = Keyboard.GetState();
            else if (CurrentController == Controller.GamePad)
                previousGamePad = GamePad.GetState(PlayerIndex.One);
        }

        public override void Update(GameTime gameTime)
        {
            if (CurrentController == Controller.Keyboard)
                keyboard = Keyboard.GetState();
            else if (CurrentController == Controller.GamePad)
                gamePad = GamePad.GetState(PlayerIndex.One);
        }
    }
}
