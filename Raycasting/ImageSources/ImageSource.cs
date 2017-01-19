using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Raycasting.ImageSources
{
    class ImageSource : IImageSource
    {
        public Texture2D CurrentTexture {get; set;}

        public ImageSource(Texture2D texture)
        {
            CurrentTexture = texture;
        }

        public static implicit operator Texture2D(ImageSource source)
        {
            return source.CurrentTexture;
        }

        public static implicit operator ImageSource(Texture2D texture)
        {
            return new ImageSource(texture);
        }

    }
}
