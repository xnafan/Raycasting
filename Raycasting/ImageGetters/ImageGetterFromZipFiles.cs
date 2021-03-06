﻿using Microsoft.Xna.Framework.Graphics;
using Raycasting.ImageSources;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Raycasting
{
    public class ImageGetterFromZipFiles : BaseImageGetter
    {
        string _itemToOpen;

        public ImageGetterFromZipFiles(string itemToOpen = null)
        {
            _itemToOpen = itemToOpen;
        }

        public const string AppSettingsKey = "ImageFolderPath";
        public override void GetImages(GraphicsDevice graphicsDevice, List<IImageSource[]> textureSetListToAddTo, ref bool stop)
        {
            List<string> zipFilesToOpen = new List<string>();
            string debugInfoZipFileName = "";
            string debugInfoPictureFileName = "";

            try
            {
                string imageFolder = null;
                if (!string.IsNullOrWhiteSpace(_itemToOpen))
                {
                    if (Path.GetExtension(_itemToOpen) == ".zip")
                    {
                        zipFilesToOpen.Add(_itemToOpen);
                    }
                    else if (Directory.Exists(_itemToOpen))
                    {
                        imageFolder = _itemToOpen;
                        zipFilesToOpen = Directory.EnumerateFiles(imageFolder, "*.zip").ToList();
                    }
                }
                else
                {
                    var runningFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    imageFolder = ConfigurationManager.AppSettings[AppSettingsKey] ?? runningFolder;
                    if (Directory.Exists(imageFolder))
                    { zipFilesToOpen = Directory.EnumerateFiles(imageFolder, "*.zip").ToList(); }
                }
                var tempTextures = new List<Texture2D[]>();

                for (int i = 0; i < zipFilesToOpen.Count(); i++)
                {
                    List<IImageSource> textures = new List<IImageSource>();
                    debugInfoZipFileName = zipFilesToOpen[i];
                    using (ZipArchive archive = ZipFile.OpenRead(zipFilesToOpen[i]))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            if (stop) return;
                            if (ImageSourceFactory.IsValidImageFile(Path.GetExtension(entry.Name)))
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
                                    Console.WriteLine(string.Format("Error loading image '{0}' from file '{1}'. Error is: {2}", debugInfoPictureFileName, debugInfoZipFileName, ex.Message));
                                }
                            }
                        }
                    }
                    if (textures.Count > 0)
                    { textureSetListToAddTo.Add(textures.ToArray()); }
                }
            }
            catch (Exception ex)
            {

                throw new Exception(string.Format("Error loading image '{0}' from file '{1}'. Error is: {2}", debugInfoPictureFileName, debugInfoZipFileName, ex.ToString()), ex);
            }
        }


    }
}