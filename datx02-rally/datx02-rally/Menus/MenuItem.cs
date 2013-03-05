using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally.Menus
{
    public enum ItemPositionX { LEFT, CENTER, RIGHT }
    public enum ItemPositionY { TOP, CENTER, BOTTOM }

    public abstract class MenuItem
    {
        public string Identifier { get; set; }
        public string Text { get; set; }
        public ItemPositionX MenuPositionX { get; set; }
        public ItemPositionY MenuPositionY { get; set; }
        public SpriteFont Font { get; set; }
        public Texture2D Background { get; set; }
        public Color FontColor { get; set; }

        public abstract void Draw(SpriteBatch spriteBatch, Vector2 position);

        private Rectangle bounds;
        public Rectangle Bounds 
        {
            get { 
                return new Rectangle(0, 0, bounds.Width, 
                    (Background != null) ? Background.Bounds.Height : (int)Font.MeasureString(Text).Y); 
            }
            set { bounds = value; } 
        }
        public void SetWidth(int width)
        {
            var b = Bounds;
            b.Width = width;
            Bounds = b;
        }
    }

    public abstract class OptionMenuItem : MenuItem
    {
        public abstract bool IsLastOption();

        public abstract bool IsFirstOption();

        public abstract void NextOption();

        public abstract void PreviousOption();

        public abstract string SelectedOption();

    }

    public class StateActionMenuItem : MenuItem
    {
        public GameState NextState { get; set; }

        public StateActionMenuItem(string text) : this(text, null, null) { }

        public StateActionMenuItem(string text, GameState? nextState) : this(text, nextState, null) { }

        public StateActionMenuItem(string text, GameState? nextState, string identifier)
        {
            this.Text = text;
            this.Identifier = identifier != null ? identifier : text;
            this.NextState = nextState.HasValue ? nextState.Value : GameState.None;

            MenuPositionX = ItemPositionX.CENTER;
            MenuPositionY = ItemPositionY.CENTER;

            FontColor = Color.White;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            position.X += Bounds.Width / 2 - Background.Bounds.Width / 2;
            spriteBatch.Draw(Background, position, Color.White);

            Vector2 textOffset = Font.MeasureString(Text);
            textOffset /= 2;
            textOffset.X = Background.Bounds.Width / 2 - textOffset.X;
            textOffset.Y = Background.Bounds.Height / 2 - textOffset.Y + 3;
            spriteBatch.DrawString(Font, Text, position + textOffset, FontColor);
        }
    }

    public class ActionMenuItem : MenuItem
    {
        public delegate void Action();
        public Action PerformAction;

        public ActionMenuItem(string text) : this(text, null, null) { }

        public ActionMenuItem(string text, Action action) : this(text, action, null) { }

        public ActionMenuItem(string text, Action action, string identifier)
        {
            this.Text = text;
            this.Identifier = identifier != null ? identifier : text;
            this.PerformAction = action;

            MenuPositionX = ItemPositionX.CENTER;
            MenuPositionY = ItemPositionY.CENTER;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.DrawString(Font, Text, position, FontColor);
        }
    }

    public class BoolOptionMenuItem : OptionMenuItem<bool>
    {
        public BoolOptionMenuItem(string text) : this(text, null) { }

        public BoolOptionMenuItem(string text, string identifier)
            : base(text, identifier)
        {
            this.AddOption("Off", false).AddOption("On", true);
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.DrawString(Font, Text, position, FontColor);
        }

    }

    public class OptionMenuItem<T> : OptionMenuItem
    {
        public List<Tuple<string,T>> options = new List<Tuple<string, T>>();
        public int selectedOptionIndex = 0;

        public OptionMenuItem(string text) : this(text, null) { }

        public OptionMenuItem(string text, string identifier)
        {
            this.Text = text;
            this.Identifier = identifier != null ? identifier : text;

            MenuPositionX = ItemPositionX.CENTER;
            MenuPositionY = ItemPositionY.CENTER;
        }

        public OptionMenuItem<T> AddOption(string text, T value) 
        {
            options.Add(new Tuple<string, T>(text, value));
            return this;
        }

        public OptionMenuItem<T> SetStartOption(string text)
        {
            int matchingIndex = -1;
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].Item1 == text)
                {
                    matchingIndex = i;
                    break;
                }
            }
            if (matchingIndex > -1)
                selectedOptionIndex = matchingIndex;
            return this;
        }

        public OptionMenuItem<T> SetStartOption(T value)
        {
            int matchingIndex = -1;
            for (int i = 0; i < options.Count; i++)
            {
                if (Equal(options[i].Item2, value))
                {
                    matchingIndex = i;
                    break;
                }
            }
            if (matchingIndex > -1) 
                selectedOptionIndex = matchingIndex;
            return this;
        }

        public override bool IsLastOption() 
        {
            return selectedOptionIndex == options.Count - 1;
        }

        public override bool IsFirstOption()
        {
            return selectedOptionIndex == 0;
        }

        public override void NextOption() 
        {
            selectedOptionIndex = Math.Min(options.Count - 1, selectedOptionIndex + 1);
        }

        public override void PreviousOption()
        {
            selectedOptionIndex = Math.Max(0, selectedOptionIndex - 1);
        }

        public override string SelectedOption()
        {
            return options[selectedOptionIndex].Item1;
        }

        public T SelectedValue()
        {
            return options[selectedOptionIndex].Item2;
        }

        // Hack to get equal to work for both reference and value types
        private bool Equal(T o1, T o2)
        {
            if (o1 is ValueType)
                return o1.Equals(o2);
            else
                return (Object)o1 == (Object)o2;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            // Title string
            Vector2 textOffset = Font.MeasureString(Text);
            textOffset.X = Bounds.Width / 6;
            textOffset /= 2;
            spriteBatch.DrawString(Font, Text, position + textOffset, FontColor);

            // Option string
            textOffset = Font.MeasureString(Text);
            textOffset.X = Bounds.Width - Bounds.Width / 6;
            textOffset /= 2;
            spriteBatch.DrawString(Font, SelectedOption(), position + textOffset, FontColor);
        }

    }
}
