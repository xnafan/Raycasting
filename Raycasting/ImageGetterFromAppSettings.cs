using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting
{
    public class ImageGetterFromAppSettings : ITextureGetter
    {
        public Texture2D[][] GetTextures(GraphicsDevice graphicsDevice)
        {
            var imageFolders = ConfigurationManager.AppSettings["imagePath"] ?? @"D:\Dropbox\Programming\XNA\Raycasting\Raycasting\Images\Clean";
            var imageFolderNames = imageFolders.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var tempTextures = new Texture2D[imageFolderNames.Length][];
            for (int i = 0; i < imageFolderNames.Length; i++)
            {
                List<Texture2D> textures = new List<Texture2D>();
                var files = Directory.GetFiles(imageFolderNames[i]);
                foreach (var item in files)
                {
                    using (FileStream fileStream = new FileStream(item, FileMode.Open))
                    {
                        textures.Add(Texture2D.FromStream(graphicsDevice, fileStream));
                    }
                }
                tempTextures[i] = textures.ToArray();
            }
            return tempTextures;
        }
    }
}
