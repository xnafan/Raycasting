using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting
{
    public interface ITextureGetter
    {
        Texture2D[][] GetTextures(GraphicsDevice graphicsDevice);
    }
}
