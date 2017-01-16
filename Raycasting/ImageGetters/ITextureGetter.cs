using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Raycasting
{
    public interface IImageGetter
    {
        void GetImages(GraphicsDevice graphicsDevice, List<Texture2D[]> textureSetListToAddTo, ref bool stop);
    }
}