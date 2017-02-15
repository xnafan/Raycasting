using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Raycasting.ImageSources;

namespace Raycasting.ImageGetters
{
    public class VideoImageGetter : ITextureGetter
    {
        public VideoImageSource VideoSource { get; set; }
        public VideoImageGetter(string pathToVideoFile)
        {
            VideoSource = new ImageSources.VideoImageSource(pathToVideoFile);
        }

        public event TextureEventHandler TextureLoadedEvent;

        public void GetImages(GraphicsDevice graphicsDevice, List<IImageSource[]> textureSetListToAddTo, ref bool stop)
        {
            textureSetListToAddTo.Add(new List<IImageSource>(){ VideoSource}.ToArray());
        }
    }
}
