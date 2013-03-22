using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace datx02_rally.Entities
{
    public class OakTree : GameObject
    {
        Game1 game;

        public OakTree(Game1 game, ContentManager content)
            : base(@"Foliage\Oak_tree", content)
        {
            this.game = game;
        }

        public override void Update(GameTime gameTime)
        {
            BoundingSphere = new BoundingSphere(Position, Scale * 18);
        }

        protected override void SetEffectParameters(Effect effect)
        {
            DirectionalLight directionalLight = game.GetService<DirectionalLight>();
            if (effect.Parameters["DirectionalDirection"] != null) 
                effect.Parameters["DirectionalDirection"].SetValue(directionalLight.Direction);
            if (effect.Parameters["DirectionalDiffuse"] != null)
                effect.Parameters["DirectionalDiffuse"].SetValue(directionalLight.Diffuse);
            if (effect.Parameters["DirectionalAmbient"] != null) 
                effect.Parameters["DirectionalAmbient"].SetValue(directionalLight.Ambient);
        }
    }
}
