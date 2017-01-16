using Microsoft.Xna.Framework.Graphics;

namespace Raycasting
{
    public interface IImageGetter
    {
        Texture2D[][] GetImages(GraphicsDevice graphicsDevice);
    }
}