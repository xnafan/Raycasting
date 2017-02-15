using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting.ImageSources
{
    public class AnimatedImageSource : IImageSource
    {
        private int _currentTextureIndex;
        public float MsBetweenImages { get; set; } = 50;
        private float _msSpentInCurrentFrame;
        public Texture2D CurrentTexture { get { return Textures[_currentTextureIndex]; }  }
        public List<Texture2D> Textures { get; private set; }

        public AnimatedImageSource(List<Texture2D> textures)
        {
            Textures = textures;
        }

        public void Update(GameTime gameTime)
        {
            float msLeftToSpend = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            msLeftToSpend += _msSpentInCurrentFrame;

            while (msLeftToSpend > 0 && msLeftToSpend >= MsBetweenImages)
            {
                msLeftToSpend -= MsBetweenImages;
                MoveToNextImage();
            }
            _msSpentInCurrentFrame = msLeftToSpend;
        }

        private void MoveToNextImage()
        {
            _currentTextureIndex++;
            _currentTextureIndex %= Textures.Count;
            _msSpentInCurrentFrame = 0;
        }
        public static implicit operator Texture2D(AnimatedImageSource source)
        {
            return source.CurrentTexture;
        }

    }
}
