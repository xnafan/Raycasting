using Microsoft.Xna.Framework.Graphics;
using Raycasting.ImageSources;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace Raycasting
{
    public class ImageGetterFromOnlineZipFiles : BaseImageGetter
    {
        string _urlToOpen;

        public ImageGetterFromOnlineZipFiles(string urlToOpen = null)
        {
            _urlToOpen = urlToOpen;
        }

        public override void GetImages(GraphicsDevice graphicsDevice, List<IImageSource[]> textureSetListToAddTo, ref bool stop)
        {
            string debugInfoPictureFileName = "";

            var tempTextures = new List<Texture2D[]>();
            List<IImageSource> textures = new List<IImageSource>();

            WebRequest request = System.Net.HttpWebRequest.Create(_urlToOpen);
            var response = (HttpWebResponse)request.GetResponse();
            using (ZipArchive archive = new ZipArchive(response.GetResponseStream()))
            {
                foreach (var entry in archive.Entries)
                {
                    if (stop) return;
                    if (entry.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                        || entry.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                        || entry.Name.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            debugInfoPictureFileName = entry.FullName;
                            using (Stream fileStream = entry.Open())
                            {
                                IImageSource source = ImageSourceFactory.CreateSourceFromStream(fileStream, entry.FullName);
                                textures.Add(source);
                                OnTextureLoaded(source.CurrentTexture);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format("Error loading image '{0}' from zipfile at URL '{1}'. Error is: {2}", debugInfoPictureFileName, _urlToOpen, ex.Message));
                        }
                    }
                }
            }
            if (textures.Count > 0)
            { textureSetListToAddTo.Add(textures.ToArray()); }
        }
    }
}