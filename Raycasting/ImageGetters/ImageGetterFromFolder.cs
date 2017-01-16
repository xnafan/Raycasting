using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting
{
    public class ImageGetterFromFolder : IImageGetter
    {

        public const string AppSettingsKey = "ImageFolderPath";
        public Texture2D[][] GetImages(GraphicsDevice graphicsDevice)
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
                    using (FileStream fileStream = new FileStream(item, FileMode.Open))
                    {
                        textures.Add(Texture2D.FromStream(graphicsDevice, fileStream));
                    }
                }
                if(textures.Count > 0)
                { tempTextures.Add(textures.ToArray()); }
                
            }
            return tempTextures.ToArray();
        }
    }
}
