﻿using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting.ImageSources
{
    public static class ImageSourceFactory
    {
        public static IImageSource CreateSourceFromFile(string file)
        {
            if (Path.GetExtension(file) != ".gif")
            {
                using (FileStream fileStream = new FileStream(file, FileMode.Open))
                {

                    return new ImageSource(Texture2D.FromStream(Game1.CurrentGraphicsDevice, fileStream));
                }
            }
            else
            {
                return CreateImageSourceFromGif(file);
            }
        }

        public static IImageSource CreateSourceFromStream(Stream fileStream, string fileName)
        {
            if (Path.GetExtension(fileName) != ".gif")
            {
                using (FileStream fileStream = new FileStream(file, FileMode.Open))
                {

                    return new ImageSource(Texture2D.FromStream(Game1.CurrentGraphicsDevice, fileStream));
                }
            }
            else
            {
                return CreateImageSourceFromGif(file);
            }
        }


        public static IImageSource CreateImageSourceFromGif(string file)
    {
        using (Image gifImg = Image.FromFile(file))
        {
            FrameDimension dimension = new FrameDimension(gifImg.FrameDimensionsList[0]);
            int numberOfFrames = gifImg.GetFrameCount(dimension);
            if (numberOfFrames > 1)
            {
                    var animSource = new AnimatedImageSource(AnimatedGifToTextureList(gifImg));
                    animSource.MsBetweenImages = GetDelay(gifImg);
                    Renderer.AnimatedGifs.Add(animSource);
                    return animSource;
                }
            else
            {
                return new ImageSource(BitmapToTexture2D((Bitmap)gifImg));
            }
        }
    }

        private static float GetDelay(Image gifImg)
        {
            var propertyItem = gifImg.GetPropertyItem(20736);
          return (int)Math.Min(Math.Max((propertyItem.Value[0] + propertyItem.Value[1] * 256) * 10, 25), 250);
        }

        public static List<Texture2D> AnimatedGifToTextureList(Image gifImg)
    {
        var textures = new List<Texture2D>();

        FrameDimension dimension = new FrameDimension(gifImg.FrameDimensionsList[0]);
        int numberOfFrames = gifImg.GetFrameCount(dimension);
        for (int frameCounter = 0; frameCounter < numberOfFrames; frameCounter++)
        {
            gifImg.SelectActiveFrame(dimension, frameCounter);
            Texture2D texture = BitmapToTexture2D((Bitmap)gifImg);
            textures.Add(texture);
        }
        return textures;
    }


    static Texture2D BitmapToTexture2D(Bitmap image)
    {
        // Buffer size is size of color array multiplied by 4 because   
        // each pixel has four color bytes  
        int bufferSize = image.Height * image.Width * 4;

        // Create new memory stream and save image to stream so   
        // we don't have to save and read file  
        System.IO.MemoryStream memoryStream =
          new System.IO.MemoryStream(bufferSize);
        image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

        // Creates a texture from IO.Stream - our memory stream  
        Texture2D texture = Texture2D.FromStream(
          Game1.CurrentGraphicsDevice, memoryStream);

        return texture;
    }
}
}
