using ImageTools;
using Microsoft.Xna.Framework.Graphics;
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
            var extension = Path.GetExtension(file).ToLower();
            if(IsValidVideoFile(extension))
            {
                return new VideoImageSource(file);

            }
            else if (extension == ".gif")
            {
                return CreateImageSourceFromGif(file);
            }
            else if(IsValidImageFile(file))
            {
                using (FileStream fileStream = new FileStream(file, FileMode.Open))
                {
                    using (Bitmap img = (Bitmap)Bitmap.FromStream(fileStream))
                    {
                            
                        //return new ImageSource(BitmapToTexture2D((Bitmap)ImageTool.ScalePicture(img, 1024, ImageTool.ScaleMode.SquareFrame, 0, Color.Black)));
                        return new ImageSource(BitmapToTexture2D((Bitmap)img));
                    }
                }
            }
            else
            {
                throw new ArgumentException("Not a valid file for getting images");
            }
            
        }
        public static IImageSource CreateSourceFromStream(Stream stream, string filename)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                ms.Position = 0; // rewind
                if (Path.GetExtension(filename) != ".gif")
                {
                    using (Bitmap img = (Bitmap)Bitmap.FromStream(ms))
                    {

                        //return new ImageSource(BitmapToTexture2D((Bitmap)ImageTool.ScalePicture(img, 1024, ImageTool.ScaleMode.SquareFrame, 0, Color.Black)));
                        return new ImageSource(BitmapToTexture2D((Bitmap)img));
                    }
                }
                else
                {
                    return CreateImageSourceFromGifStream(ms);
                }
            }
        }


        public static IImageSource CreateImageSourceFromGifStream(Stream stream)
        {
            using (Image gifImg = Image.FromStream(stream))
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

        public static IImageSource CreateImageSourceFromGif(string file)
        {
            using (Stream stream = File.OpenRead(file))
            {
                return CreateImageSourceFromGifStream(stream);
            }
        }

        private static float GetDelay(Image gifImg)
        {
            var propertyItem = gifImg.GetPropertyItem(20736);
            return (int)Math.Min(Math.Max((propertyItem.Value[0] + propertyItem.Value[1] * 256) * 10, 25), 5000);
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


        public static Texture2D BitmapToTexture2D(Bitmap image)
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

        public static bool IsValidVideoFile(string file)
        {
            var extension = Path.GetExtension(file);
            return ".avi.mkv.mp4.wmv.webm".Contains(extension.ToLower());
        }

        public static bool IsValidImageFile(string file)
        {
            var extension = Path.GetExtension(file);
            return ".jpg.png.bmp.gif".Contains(extension.ToLower());
        }


        public static bool IsValidImageSourceFile(string file)
        {
            return IsValidVideoFile(file) || IsValidImageFile(file);
        }
    }
}
