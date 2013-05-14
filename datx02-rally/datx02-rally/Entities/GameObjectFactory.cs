using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace datx02_rally.Entities
{
    class GameObjectFactory
    {
        private static ContentManager content;
        private static Dictionary<string, string> assets = new Dictionary<string,string>();


        public static void Initialize(ContentManager content)
        {
            GameObjectFactory.content = content;
        }

        public static void AddAsset(string key, string assetName)
        {
            assets.Add(key, assetName);
        }

        private static void CheckContentManager()
        {
            if (content == null)
                throw new MethodAccessException("Call Initialize first");
        }

        public static GameObject Create(string key)
        {
            CheckContentManager();
            return new GameObject(content.Load<Model>(assets[key]), 1, Vector3.Zero);
        }

        public static GameObject CreateRandom()
        {
            CheckContentManager();
            return new GameObject(content.Load<Model>(assets.Values.ElementAt(UniversalRandom.GetInstance().Next(assets.Values.Count))), 1, Vector3.Zero);
        }
    }
}
