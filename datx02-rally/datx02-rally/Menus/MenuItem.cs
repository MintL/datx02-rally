using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace datx02_rally.Menus
{
    class MenuItem
    {
        public string Text {get; set; }
    }

    class ActionMenuItem : MenuItem
    {
        public GameState NextState { get; private set; }

        public ActionMenuItem(string text) : this(text, null) { }

        public ActionMenuItem(string text, GameState? nextState)
        {
            this.Text = text;
            this.NextState = nextState.HasValue ? nextState.Value : GameState.None;
        }
    }

    class BoolOptionMenuItem : OptionMenuItem
    {
        public BoolOptionMenuItem(string text, bool defaultValue)
            : base(text, new List<string>() { "On", "Off" }, defaultValue ? "On" : "Off")
        {
        }

        public bool IsEnabled()
        {
            return options[selectedOptionIndex] == "On";
        }
    }

    class OptionMenuItem : MenuItem
    {
        public List<string> options;
        public int selectedOptionIndex;

        public OptionMenuItem(string text, List<string> optionList) : this(text, optionList, null) { }

        public OptionMenuItem(string text, List<string> optionList, string defaultValue)
        {
            Text = text;
            options = optionList;

            if (defaultValue != null) 
            {
                int matchingIndex = -1;
                for (int i = 0; i < optionList.Count; i++)
                {
                    if (optionList[i] == defaultValue) 
                    {
                        matchingIndex = i;
                        break;
                    }
                }
                selectedOptionIndex = matchingIndex;
            } 
            else 
            {
                selectedOptionIndex = 0;
            }
        }

        public bool IsLastOption() 
        {
            return selectedOptionIndex == options.Count - 1;
        }

        public bool IsFirstOption()
        {
            return selectedOptionIndex == 0;
        }

        public void Next() 
        {
            selectedOptionIndex = Math.Min(options.Count - 1, selectedOptionIndex + 1);
        }

        public void Previous()
        {
            selectedOptionIndex = Math.Max(0, selectedOptionIndex - 1);
        }

        public string SelectedOption()
        {
            return options[selectedOptionIndex];
        }

    }
}
