using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace datx02_rally.GameplayModes
{
    public class EndGameStatistics
    {
        public List<EndGameStatistics.Heading> CategorizedItems { get; private set; }
        public bool Won { get; private set; }

        public EndGameStatistics(List<EndGameStatistics.Heading> items, bool won)
        {
            CategorizedItems = items;
            Won = won;
        }

        public void SetItemText(string itemTitle, string text) 
        {
            foreach (var itemCategory in CategorizedItems)
            {
                if (itemCategory.Items.ContainsKey(itemTitle))
                {
                    itemCategory.Items[itemTitle] = text;
                    return;
                }
            }
        }

        public class Heading
        {
            public string Title { get; set; }
            public Dictionary<string, string> Items { get; set; }

            public Heading()
            {
                Items = new Dictionary<string, string>();
            }
        }
    }
}
