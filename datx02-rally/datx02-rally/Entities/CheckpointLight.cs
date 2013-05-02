using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace datx02_rally.Entities
{
    public class CheckpointLight : PointLight
    {

        public CheckpointLight(Vector3 position)
            : base(position, Color.OrangeRed.ToVector3(), 400)
        {

        }

        public override void Update(GameTime gameTime)
        {
            
        }

        public static void LoadMaterial()
        {

        }

    }
}
