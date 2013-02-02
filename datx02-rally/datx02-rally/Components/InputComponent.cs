using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace datx02_rally
{
    public enum Input { Thrust, Brake, Steer, ChangeController, Exit };
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
                    case Input.Exit:
                        return keyboard.IsKeyDown(Keys.Escape);
                }
            }
            else if (CurrentController == Controller.GamePad)
            {
                switch (input)
                {
                    case Input.ChangeController:
                        return previousGamePad.IsButtonUp(Buttons.Y) && gamePad.IsButtonDown(Buttons.Y);
                    case Input.Exit:
                        return gamePad.IsButtonDown(Buttons.Start);
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
