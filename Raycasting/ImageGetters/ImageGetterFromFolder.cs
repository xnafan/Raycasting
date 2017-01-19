using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using Raycasting.ImageSources;

namespace Raycasting
{
    public class ImageGetterFromFolder : BaseImageGetter
    {

        public const string AppSettingsKey = "ImageFolderPath";
        private string _rootImageFolder;

        public ImageGetterFromFolder(string rootImageFolder = null)
        {
            _rootImageFolder = rootImageFolder;
        }


        public override void GetImages(GraphicsDevice graphicsDevice, List<IImageSource[]> textureSetListToAddTo, ref bool stop)
        {
            List<string> imageFolders = new List<string>();
            string debugInfoPictureFileName = "";
            try
            {
                if (string.IsNullOrWhiteSpace(_rootImageFolder))
                {
                    var _runningFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    _rootImageFolder = ConfigurationManager.AppSettings[AppSettingsKey] ?? _runningFolder;
                }
                if (Directory.Exists(_rootImageFolder))
                {
                    imageFolders = Directory.GetDirectories(_rootImageFolder).ToList();
                    imageFolders.Add(_rootImageFolder);
                }


                var tempTextures = new List<Texture2D[]>();
                for (int i = 0; i < imageFolders.Count; i++)
                {
                    List<IImageSource> textures = new List<IImageSource>();
                    var files = Directory.GetFiles(imageFolders[i], "*.jpg").ToList();
                    files.AddRange(Directory.GetFiles(imageFolders[i], "*.png"));
                    files.AddRange(Directory.GetFiles(imageFolders[i], "*.gif"));

                    foreach (var item in files)
                    {
                        if (stop) return;
                        try
                        {
                            IImageSource source = ImageSourceFactory.CreateSourceFromFile(item);
                            textures.Add(source);
                            OnTextureLoaded(source.CurrentTexture);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format("Error loading image '{0}'. Error is: {1}", debugInfoPictureFileName, ex.Message));
                        }
                    }

                    if (textures.Count > 0)
                    { textureSetListToAddTo.Add(textures.ToArray()); }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error loading image '{0}'. Error is: {1}", debugInfoPictureFileName, ex.ToString()), ex);
            }
        }
        public static List<Texture2D> AnimatedGifToTextureList(string imagePath)
        {
            var textures = new List<Texture2D>();
            Image gifImg = Image.FromFile(imagePath);
            FrameDimension dimension = new FrameDimension(gifImg.FrameDimensionsList[0]);
            int numberOfFrames = gifImg.GetFrameCount(dimension);
            for (int frameCounter = 0; frameCounter < numberOfFrames; frameCounter++)
            {
                gifImg.SelectActiveFrame(dimension, frameCounter);
                Texture2D texture = BitmapToTexture2D(Game1.CurrentGraphicsDevice, (Bitmap)gifImg);
                textures.Add(texture);
            }
            return textures;
        }

        static Texture2D BitmapToTexture2D(
   GraphicsDevice GraphicsDevice,
   System.Drawing.Bitmap image)
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
              GraphicsDevice, memoryStream);

            return texture;
        }
    }
}