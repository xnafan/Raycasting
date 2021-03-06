﻿using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Raycasting.ImageSources;

namespace Raycasting
{
    public interface ITextureGetter
    {
        
        void GetImages(GraphicsDevice graphicsDevice, List<IImageSource[]> textureSetListToAddTo, ref bool stop);
      
        event TextureEventHandler TextureLoadedEvent;

    }
    public delegate void TextureEventHandler(object sender, TextureEventArgs e);
    public class TextureEventArgs
    {
        
        public TextureEventArgs(Texture2D texture) { Texture = texture; }
        public Texture2D Texture{ get; private set; } // readonly
    }
    public abstract class BaseImageGetter : ITextureGetter
    {
        public event TextureEventHandler TextureLoadedEvent;

        public abstract void GetImages(GraphicsDevice graphicsDevice, List<IImageSource[]> textureSetListToAddTo, ref bool stop);

        protected virtual void OnTextureLoaded(Texture2D texture)
        {
            if (TextureLoadedEvent != null)
                TextureLoadedEvent(this, new TextureEventArgs(texture));
        }
    }

}