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
        private string _imageSource;

        public ImageGetterFromFolder(string rootImageFolder = null)
        {
            _imageSource = rootImageFolder;
        }


        public override void GetImages(GraphicsDevice graphicsDevice, List<IImageSource[]> textureSetListToAddTo, ref bool stop)
        {
            List<string> imageFolders = new List<string>();

            string debugInfoPictureFileName = "";
            try
            {
                if (string.IsNullOrWhiteSpace(_imageSource))
                {
                    var _runningFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    _imageSource = ConfigurationManager.AppSettings[AppSettingsKey] ?? _runningFolder;
                }
                if (Directory.Exists(_imageSource))
                {
                    imageFolders = Directory.GetDirectories(_imageSource).ToList();
                    imageFolders.Add(_imageSource);
                }
                else
                {
                    List<IImageSource> textures = new List<ImageSources.IImageSource>();
                    AddFile(textures, _imageSource);
                    textureSetListToAddTo.Add(textures.ToArray());
                    return;
                }
                
                for (int i = 0; i < imageFolders.Count; i++)
                {
                    List<IImageSource> textures = new List<IImageSource>();
                    var files = Directory.GetFiles(imageFolders[i], "*.jpg").ToList();
                    files.AddRange(Directory.GetFiles(imageFolders[i], "*.png"));
                    files.AddRange(Directory.GetFiles(imageFolders[i], "*.gif"));

                    foreach (var item in files)
                    {
                        Console.WriteLine(item);
                        if (stop) return;
                        try
                        {
                            AddFile(textures, item);
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

        private void AddFile(List<IImageSource> textures, string item)
        {
            IImageSource source = ImageSourceFactory.CreateSourceFromFile(item);
            textures.Add(source);
            OnTextureLoaded(source.CurrentTexture);
        }
    }
}