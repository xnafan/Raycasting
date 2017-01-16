using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Raycasting
{
    public class ImageGetterFromFolder : IImageGetter
    {

        public const string AppSettingsKey = "ImageFolderPath";
        public void GetImages(GraphicsDevice graphicsDevice, List<Texture2D[]> textureSetListToAddTo, ref bool stop)
        {
            try
            {
                var runningFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var rootImageFolder = ConfigurationManager.AppSettings[AppSettingsKey] ?? runningFolder;
                var imageFolders = Directory.GetDirectories(rootImageFolder).ToList();
                imageFolders.Add(rootImageFolder);
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
                            textures.Add(Texture2D.FromStream(graphicsDevice, fileStream));
                        }
                    }
                    if (textures.Count > 0)
                    { textureSetListToAddTo.Add(textures.ToArray()); }
                }
            }
            catch 
            {
            }
        }
    }
}