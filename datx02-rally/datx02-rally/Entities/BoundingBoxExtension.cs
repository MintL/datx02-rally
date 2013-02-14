using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace datx02_rally.Entities
{
    static class BoundingBoxExtension
    {
        public static BoundingBox Translate(this BoundingBox box, Matrix translationMatrix)
        {
            return new BoundingBox(Vector3.Transform(box.Min, translationMatrix), Vector3.Transform(box.Max, translationMatrix));
        }

    }
}
