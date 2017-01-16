using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting
{
    public class ImageGetterFromZipFiles : IImageGetter
    {
        public const string AppSettingsKey = "ImageFolderPath";
        public Texture2D[][] GetImages(GraphicsDevice device)
        {
            var returnValues = new List<List<string>>();
            var runningFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var imageFolder = ConfigurationManager.AppSettings[AppSettingsKey] ?? runningFolder;
            var zipFiles = Directory.EnumerateFiles(imageFolder, "*.zip").ToList();
            var tempTextures = new List<Texture2D[]>();

            for (int i = 0; i < zipFiles.Count(); i++)
            {
                List<Texture2D> textures = new List<Texture2D>();
                using (ZipArchive archive = ZipFile.OpenRead(zipFiles[i]))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if(entry.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                        {
                            using (Stream fileStream = entry.Open())
                            {
                                using (var ms = new MemoryStream())
                                {
                                    fileStream.CopyTo(ms);
                                    ms.Position = 0; // rewind
                                                     // do something with ms
                                    textures.Add(Texture2D.FromStream(device, ms));
                                }
                            }
                        }
                    }
                }
                if (textures.Count > 0)
                { tempTextures.Add(textures.ToArray()); }
            }
            return tempTextures.ToArray();
        }
    }
}