using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System;

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

        public override void GetImages(GraphicsDevice graphicsDevice, List<Texture2D[]> textureSetListToAddTo, ref bool stop)
        {
            List<string> imageFolders = new List<string>();
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
                    List<Texture2D> textures = new List<Texture2D>();
                    var files = Directory.GetFiles(imageFolders[i], "*.jpg");
                    foreach (var item in files)
                    {
                        if (stop) return;
                        using (FileStream fileStream = new FileStream(item, FileMode.Open))
                        {
                            Texture2D texture = Texture2D.FromStream(graphicsDevice, fileStream);
                            textures.Add(texture);
                            OnTextureLoaded(texture);
                        }
                    }
                    if (textures.Count > 0)
                    { textureSetListToAddTo.Add(textures.ToArray()); }
                }
            }
            catch { throw;}
        }
    }
}