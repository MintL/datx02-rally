using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace datx02_rally
{
    public enum Input { Thrust, Brake, Steer, ChangeController, ChangeCamera, CameraX, CameraY, Exit, Console };
    public enum Controller { Keyboard, GamePad };

    class InputComponent : GameComponent
    {
        private KeyboardState keyboard;
        private KeyboardState previousKeyboard;
        private GamePadState gamePad;
        private GamePadState previousGamePad;
        private Boolean enabled = true;

        public Controller CurrentController { get; set; }

        public InputComponent(Game1 game)
            : base(game)
        {
        }

        public float GetState(Input input)
        {
            if (CurrentController == Controller.Keyboard && enabled)
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
            if (CurrentController == Controller.Keyboard && enabled)
            {
                switch (input)
                {
                    case Input.ChangeController:
                        return GetKey(Keys.F1);
                    case Input.ChangeCamera:
                        return GetKey(Keys.C);
                    case Input.Exit:
                        return keyboard.IsKeyDown(Keys.Escape) && previousKeyboard.IsKeyUp(Keys.Escape);
                    case Input.Console:
                        if (previousKeyboard.IsKeyUp(Keys.F2) && keyboard.IsKeyDown(Keys.F2))
                            enabled = !enabled;
                        return previousKeyboard.IsKeyUp(Keys.F2) && keyboard.IsKeyDown(Keys.F2);
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
            else if (input == Input.Console) 
            {
                if (previousKeyboard.IsKeyUp(Keys.F2) && keyboard.IsKeyDown(Keys.F2))
                    enabled = !enabled;
                return previousKeyboard.IsKeyUp(Keys.F2) && keyboard.IsKeyDown(Keys.F2);
            }
            return false;
        }

        /// <summary>
        /// Returns the text a key represents in lower case, or empty if not applicable.
        /// I.e. Keys.A would return "a", Keys.OemPeriod returns "."
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetKeyText(Keys key)
        {
            String sKey = key.ToString();
            if (key == Keys.Space)
            {
                return " ";
            }
            else if (sKey.Length == 1 && Char.IsLetter(sKey[0]))
            {
                return Char.ToLower(sKey[0]).ToString();
            }
            else if (key == Keys.OemPeriod)
            {
                return ".";
            }
            else if (key >= Keys.D0 && key <= Keys.D9)
            {
                return sKey[1].ToString();
            }
            return "";
        }

        public bool GetKey(Keys key)
        {
            return previousKeyboard.IsKeyUp(key) && keyboard.IsKeyDown(key);
        }

        private  void UpdatePreviousState()
        {
            if (CurrentController == Controller.Keyboard)
                previousKeyboard = keyboard;
            else if (CurrentController == Controller.GamePad)
                previousGamePad = gamePad;
        }

        /// <summary>
        /// Updates before other components to have up-to-date input.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            UpdatePreviousState();
            if (CurrentController == Controller.Keyboard)
                keyboard = Keyboard.GetState();
            else if (CurrentController == Controller.GamePad)
                gamePad = GamePad.GetState(PlayerIndex.One);
        }
    }
}
