using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ImageTools
{
	/// <summary>
	/// Summary description for FileFinderTool.
	/// </summary>
	public static class FileFinderTool
	{

        private static readonly Random rnd = new Random();

		public enum FileFormat
		{
			Image,
			Compressed, 
			Document,
			Movie,
			Audio,
			Executable,
			Web,
			Data
		}

		private static string ImageExtensions = ".jpg.jpeg.jpe.bmp.gif.tif.tiff.iff.png.pcx.wmf.cur.ico.pic";
		private static string CompressedExtensions = ".zip.arj.rar.ace.arc";
		private static string DocumentExtensions = ".doc.txt.xsl.ppt.cdr.pdf.";
		private static string MovieExtensions = ".mov.avi.wmv.mpg.mpg.mpeg.asf";
		private static string AudioExtensions = ".wav.mp3.ogg.wma.au.ra.ram.rm.midi.mid";
		private static string WebExtensions = ".htm.html.asp.aspx.php.jsp.css.pl.js.";
		private static string ExecutableExtensions = ".exe.com.bat.pif";
		private static string DataExtensions = ".mdb.db.db2.db3.mdf.wdb.xml.xsl.xslt.xsd.dtd";

		private static Dictionary<FileFormat, string> _formatExtensions;

		static FileFinderTool()
		{
			_formatExtensions = new Dictionary<FileFormat, string>();

			_formatExtensions.Add( FileFormat.Image,		ImageExtensions);
			_formatExtensions.Add( FileFormat.Compressed,	CompressedExtensions);
			_formatExtensions.Add( FileFormat.Document,		DocumentExtensions);
			_formatExtensions.Add( FileFormat.Movie,		MovieExtensions);
			_formatExtensions.Add( FileFormat.Audio,		AudioExtensions);
			_formatExtensions.Add( FileFormat.Executable,	ExecutableExtensions);
			_formatExtensions.Add( FileFormat.Web,			WebExtensions);
			_formatExtensions.Add( FileFormat.Data,			DataExtensions);
		}

        //*************************'
        //TODO: make FindIMAGE methods into Find FILES in folders  ******************

        public static List<FileInfo> FindImageFilesInFolder(string absoluteFolderPath, string searchString)
        {
            List<FileInfo> images = FindImageFilesInFolder(absoluteFolderPath);
            foreach (FileInfo file in images)
            {
                if (file.Name.IndexOf(searchString, StringComparison.CurrentCultureIgnoreCase)== -1)
                {
                    images.Remove(file);
                }
            }
            return images;
        }
		public static List<FileInfo> FindImageFilesInFolder(string absoluteFolderPath)
		{
			return FileFinderTool.FindImageFilesInFolder(absoluteFolderPath, false);
		}

		public static List<FileInfo> FindImageFilesInFolder(string absoluteFolderPath, bool recursiveSearch)
		{
            List<FileInfo> images = new List<FileInfo>();
            FindImageFilesInFolder(absoluteFolderPath, recursiveSearch, images);
            return images;
		}

        public static void FindImageFilesInFolder(string absoluteFolderPath, bool recursiveSearch, List<FileInfo> images)
        {
            DirectoryInfo directoryToLookIn = new DirectoryInfo(absoluteFolderPath);
            
            foreach (FileInfo file in directoryToLookIn.GetFiles())
            {
                if (FileFinderTool.IsType(file, FileFinderTool.FileFormat.Image))
                    images.Add(file);
            }
            if (recursiveSearch)
            {
                foreach (DirectoryInfo directory in directoryToLookIn.GetDirectories())
                {
                    FileFinderTool.FindImageFilesInFolder(directory.FullName, true, images);
                }
            }

        }

    //    public static List<FileInfo> FindImageFilesInFolder(string absoluteFolderPath, bool recursiveSearch, int maxImages)
    //    {
            
    //        List<FileInfo> chosenImages = new List<FileInfo>();
    //        List<DirectoryInfo> dirs = GetAllFoldersInFolder(new DirectoryInfo(absoluteFolderPath));
    //        while (dirs.Count > 0 && chosenImages.Count < maxImages)
    //        {
    //            DirectoryInfo randomDir = dirs[rnd.Next(dirs.Count)];
    //            List<FileInfo> potentialImages = FindImageFilesInFolder(randomDir.FullName, true);
    //            if (potentialImages.Count == 0)
    //{
    //                continue;
		 
    //}
    //            FileInfo randomPic  = potentialImages<rnd
    //        }

    //        return chosenImages;
    //    }

		public static bool IsType(FileInfo file, FileFinderTool.FileFormat format)
		{
			string extensions = FileFinderTool._formatExtensions[format];
		
			if (extensions == null)
				throw new ArgumentException("The format '" + format.ToString() + "' is not supported!");

            bool found = extensions.IndexOf(file.Extension.ToLower()) != -1;
			return found;
		}

        public static List<DirectoryInfo> GetAllFoldersInFolder(DirectoryInfo rootFolder)
        {
            return GetAllFoldersInFolder(rootFolder, new List<DirectoryInfo>());
        }

        public static List<DirectoryInfo> GetAllFoldersInFolder(DirectoryInfo rootFolder, List<DirectoryInfo> folders)
        {
            
            foreach (DirectoryInfo dir in rootFolder.GetDirectories())
            {
                folders.Add(dir);
                GetAllFoldersInFolder(dir, folders);
            }
            return folders;
        }
	}
}
