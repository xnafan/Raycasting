using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace ImageTools
{

    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public static class ImageTool
    {

        //For looking up ImageCodecs based on ImageFormat
        private static Dictionary<ImageFormat, ImageCodecInfo> codecLookup = new Dictionary<ImageFormat, ImageCodecInfo>();

        //Class Constructor which creates the dictionary to look up ImageCodecs
        static ImageTool()
        {
            ImageFormat[] formats = new ImageFormat[] { ImageFormat.Bmp, ImageFormat.Emf, ImageFormat.Exif, ImageFormat.Gif, ImageFormat.Icon, ImageFormat.Jpeg, ImageFormat.Png, ImageFormat.Tiff, ImageFormat.Wmf };

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                foreach (ImageFormat format in formats)
                {
                    if (format.Guid == codec.FormatID)
                        codecLookup.Add(format, codec);
                }
            }
        }


        /// <summary>
        /// Get the Codec for a given ImageFormat
        /// </summary>
        /// <param name="format">The ImageFormat of the Image</param>
        /// <returns>The ImageCodec for that ImageFormat</returns>
        public static ImageCodecInfo GetCodec(ImageFormat format)
        {
            return codecLookup[format];
        }

        /// <summary>
        /// Get the codec for a given filetype through it's filenameextension
        /// </summary>
        /// <param name="imageFileExtension">The extension of the file, e.g. 'GIF', 'JPEG', 'JPG'</param>
        /// <returns>The ImageCodec for that filetype</returns>
        public static ImageCodecInfo GetCodec(string imageFileExtension)
        {
            return codecLookup[GetImageFormat(imageFileExtension)];
        }

        /// <summary>
        /// A way of storing whether an image is Landscape or Portrait mode
        /// </summary>
        private enum Orientation
        {
            /// <summary>
            /// The image is wider than it is tall
            /// </summary>
            Landscape,
            /// <summary>
            /// The image is taller than it is wide
            /// </summary>
            Portrait
        }

        /// <summary>
        /// A way of storing how to manipulate an image
        /// </summary>
        public enum ScaleMode
        {
            /// <summary>
            /// Does not resize the image
            /// </summary>
            None,

            /// <summary>
            /// Resizes the image to a percentage of the original
            /// </summary>
            Percent,

            /// <summary>
            /// Resizes the image, making sure neither width nor height exceeds the desired measurement. E.g. an 800x600 image with MaxSizeInPixels of 600 would be reduced to 600x450, a 600x800 image would similarly be resized to 450x600.
            /// </summary>
            MaxSizeInPixels,

            /// <summary>
            /// Resizes an image to a given width, regardless of the width. Height/width ratio is kept.
            /// </summary>
            MaxWidthInPixels,

            /// <summary>
            /// Resizes an image to a given height, regardless of the width. Height/width ratio is kept.
            /// </summary>
            MaxHeightInPixels,

            /// <summary>
            /// Crops the largest possible square from an image, so a square from an 800x600 image would be 600x600.
            /// The square is always cut out from the horizontal center and the vertical top of the image.
            /// </summary>
            SquareCropInPixels,

            /// <summary>
            /// Resizes the image inside a square frame, making sure the image fits the frame
            /// </summary>
            SquareFrame
        }


        /// <summary>
        /// Gets the new size of a set of dimensions to fit inside a square box of maxSize. 
        /// </summary>
        /// <param name="height">The initial height</param>
        /// <param name="width">The initial width</param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        internal static SizeF FitSize(float height, float width, int maxSize)
        {
            float ratio = (float)height / (float)width;

            //portrait format
            if (height > width)
            {
                height = maxSize;
                width = height / ratio;
            }
            else  //landscape format
            {
                width = maxSize;
                height = width * ratio;
            }
            return new SizeF(width, height);
        }

        //Base code from BobPowell.net
        //http://www.bobpowell.net/highqualitythumb.htm

        /// <summary>
        /// Scales an Image to another size
        /// </summary>
        /// <param name="original">The original Image to resize</param>
        /// <param name="newSizeInPixels">The size of the new Image to return</param>
        /// <param name="mode">How to interpret the "newSizeInPixels" parameter</param>
        /// <returns></returns>
        public static Image ScalePicture(Image originalImage, int newSizeInPixels, ScaleMode mode, int borderWidth, Color borderColor)
        {

            if (!Enum.IsDefined(typeof(ScaleMode), mode))
                throw new ArgumentException(string.Format("ScaleMode value of '{0}' is not defined", mode));

            //store original dimensions
            SizeF originalSize = new Size(originalImage.Width, originalImage.Height);
            SizeF newOuterSize = new SizeF();

            float ratio = originalSize.Height / originalSize.Width;
            int shortestSide = (int)Math.Min(originalSize.Height, originalSize.Width);

            Orientation imageOrientation = (ratio < 1 ? Orientation.Landscape : Orientation.Portrait);

            switch (mode)
            {
                case ScaleMode.Percent:

                    if (newSizeInPixels < 1)
                        throw new ArgumentException("Thumbnail size must be at least 1% of the original size");
                    newOuterSize.Width = (int)(originalImage.Width * 0.01f * newSizeInPixels);
                    newOuterSize.Height = (int)(originalImage.Height * 0.01f * newSizeInPixels);
                    //shortestSideOnNewImage = (int) shortestSide * 0.01f * newSizeInPixels ;
                    break;

                case ScaleMode.MaxWidthInPixels:
                    newOuterSize.Width = newSizeInPixels;
                    newOuterSize.Height = newOuterSize.Width * ratio;
                    break;

                case ScaleMode.MaxHeightInPixels:
                    newOuterSize.Height = newSizeInPixels;
                    newOuterSize.Width = newOuterSize.Height / ratio;
                    break;

                case ScaleMode.MaxSizeInPixels:
                    newOuterSize = FitSize(originalImage.Height, originalImage.Width, newSizeInPixels);
                    break;

                case ScaleMode.SquareCropInPixels:
                case ScaleMode.SquareFrame:
                    newOuterSize.Height = newOuterSize.Width = newSizeInPixels;
                    break;
            }


            Bitmap tn = new Bitmap((int)newOuterSize.Width, (int)newOuterSize.Height);

            Graphics g = Graphics.FromImage(tn);

            Rectangle destinationRectangle;
            Rectangle sourceRectangle;
            g.FillRectangle(Brushes.White, 0, 0, tn.Width, tn.Height);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;


            //SOURCE RECTANGLE
            if (mode == ScaleMode.SquareCropInPixels)
            {
                if (imageOrientation == Orientation.Portrait)
                {
                    sourceRectangle = new Rectangle(0, 0, shortestSide, shortestSide);
                }
                else
                {
                    sourceRectangle = new Rectangle((int)((originalImage.Width - shortestSide) / 2), 0, shortestSide, shortestSide);
                }
            }

            else
            {
                sourceRectangle = new Rectangle(0, 0, originalImage.Width, originalImage.Height);
            }

            if (mode == ScaleMode.SquareFrame)
            {
                SizeF innerImageSize = FitSize(originalImage.Height, originalImage.Width, newSizeInPixels - 2 * borderWidth);
                int topBorder = (int)(tn.Height - innerImageSize.Height) / 2;
                int leftBorder = (int)(tn.Width - innerImageSize.Width) / 2;
                destinationRectangle = new Rectangle(new Point(leftBorder, topBorder), Size.Round(innerImageSize));
                //new Rectangle(borderWidth, borderWidth, tn.Width - borderWidth * 2, tn.Height - borderWidth * 2);
            }
            else
            {


                //DESTINATION RECTANGLE
                if (borderWidth > 0)
                {
                    g.FillRectangle(new SolidBrush(borderColor), 0, 0, tn.Width, tn.Height);
                    //todo: check if borders are wider than half of shortest side of image, in that case image is blank or worse
                    //if(borderWidth > 
                    destinationRectangle = new Rectangle(borderWidth, borderWidth, tn.Width - borderWidth * 2, tn.Height - borderWidth * 2);
                }
                else
                {

                    destinationRectangle = new Rectangle(0, 0, tn.Width, tn.Height);

                }
            }

            g.DrawImage(originalImage, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
            g.DrawRectangle(Pens.Silver, destinationRectangle);
            if (borderWidth > 0)
            {
                //g.DrawRectangle(Pens.Black, destinationRectangle);
            }
            g.Dispose();

            return (Image)tn;
        }

        public static void ScalePictureFile(string absoluteSourcePath, string absoluteTargetPath, int newSize, ScaleMode mode, ImageFormat destinationFormat, int borderWidth, Color borderColor)
        {
            Image sourceImage = Image.FromFile(absoluteSourcePath);
            Image thumb = ScalePicture(sourceImage, newSize, mode, 0, borderColor);
            thumb.Save(absoluteTargetPath, destinationFormat);
            sourceImage.Dispose();
            thumb.Dispose();

        }


        public static Image ConvertImage(Image imageToConvert, ImageFormat destinationFormat)
        {
            return null;
        }

        public static void ScalePictureFile(string absoluteSourcePath, string absoluteTargetPath, int newSize, ScaleMode mode, ImageFormat destinationFormat)
        {
            ScalePictureFile(absoluteSourcePath, absoluteTargetPath, newSize, mode, destinationFormat, 0, Color.White);
        }

        public static void ScalePicturesInFolder(string sourceFolderAbsolutePath, string targetFolderAbsolutePath, int newSize, ScaleMode mode, ImageFormat destinationFormat)
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo(sourceFolderAbsolutePath);
            foreach (FileInfo file in FileFinderTool.FindImageFilesInFolder(sourceFolderAbsolutePath))
            {
                string destinationFile = Path.Combine(targetFolderAbsolutePath, file.Name);
                if (!Directory.Exists(targetFolderAbsolutePath))
                    Directory.CreateDirectory(targetFolderAbsolutePath);

                ScalePictureFile(file.FullName, Path.ChangeExtension(destinationFile, destinationFormat.ToString()), newSize, mode, destinationFormat);
            }
        }

        public static ImageFormat GetImageFormat(FileInfo file)
        {
            return ImageTool.GetImageFormat(file.ToString());
        }


        public static ImageFormat GetImageFormat(string imagefileOrExtension)
        {
            string extensionLowercase = null;
            if (imagefileOrExtension.IndexOf(".") == -1)
                extensionLowercase = imagefileOrExtension;
            else
                extensionLowercase = Path.GetExtension(imagefileOrExtension).ToLower().Replace(".", "");
            switch (extensionLowercase)
            {
                case "bmp": return ImageFormat.Bmp;
                case "exif": return ImageFormat.Exif;
                case "gif": return ImageFormat.Gif;
                case "jpg":
                case "jpe":
                case "jpeg": return ImageFormat.Jpeg;
                case "png": return ImageFormat.Png;
                case "tif":
                case "tiff": return ImageFormat.Tiff;
                case "wmf": return ImageFormat.Wmf;
            }
            return null;
        }

        public static string GetContentType(string imagefileOrExtension)
        {
            string extensionLowercase = null;
            if (imagefileOrExtension.IndexOf(".") == -1)
                extensionLowercase = imagefileOrExtension;
            else
                extensionLowercase = Path.GetExtension(imagefileOrExtension).ToLower().Replace(".", "");
            switch (extensionLowercase)
            {
                case "bmp": return "image/x-ms-bmp";
                case "jpg":
                case "jpe":
                case "jpeg": return "image/jpeg";
                case "png": return "image/x-png";
                case "tiff":
                case "tif": return "image/tiff";
                case "wmf": return "image/x-wmf";
                case "gif": return "image/gif";
                default: throw new ArgumentException("Type not supported");

                    //todo: already in::.??

                    //                private byte[] GetImageBytes(Image image)
                    //{
                    //    ImageCodecInfo codec = null;

                    //    foreach (ImageCodecInfo e in ImageCodecInfo.GetImageEncoders())
                    //    {
                    //        if (e.MimeType == "image/png")
                    //        {
                    //            codec = e;
                    //            break;

            }
        }

        //inspiration from http://community.bartdesmet.net/blogs/bart/archive/2006/09/19/4450.aspx
        public static byte[] GetImageBytes(Image image, ImageFormat format)
        {
            ImageCodecInfo codec = GetCodec(format);


            using (EncoderParameters ep = new EncoderParameters())
            {
                ep.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, codec, ep);
                    return ms.ToArray();
                }
            }
        }

        public static Bitmap Sharpen(Bitmap b) { Conv3x3(b, SharpeningMatrix); return b; }

        private static ConvMatrix SharpeningMatrix = new ConvMatrix() { TopMid = -2, MidLeft = -2, BottomMid = -2, MidRight = -2, Pixel = 11, Factor = 3 };

        private static bool Conv3x3(Bitmap b, ConvMatrix m)
        {
            // Avoid divide by zero errors
            if (0 == m.Factor)
                return false; Bitmap

            // GDI+ still lies to us - the return format is BGR, NOT RGB. 
            bSrc = (Bitmap)b.Clone();
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                ImageLockMode.ReadWrite,
                                PixelFormat.Format24bppRgb);
            BitmapData bmSrc = bSrc.LockBits(new Rectangle(0, 0, bSrc.Width, bSrc.Height),
                               ImageLockMode.ReadWrite,
                               PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            int stride2 = stride * 2;

            System.IntPtr Scan0 = bmData.Scan0;
            System.IntPtr SrcScan0 = bmSrc.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                byte* pSrc = (byte*)(void*)SrcScan0;
                int nOffset = stride - b.Width * 3;
                int nWidth = b.Width - 2;
                int nHeight = b.Height - 2;

                int nPixel;

                for (int y = 0; y < nHeight; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        nPixel = ((((pSrc[2] * m.TopLeft) +
                            (pSrc[5] * m.TopMid) +
                            (pSrc[8] * m.TopRight) +
                            (pSrc[2 + stride] * m.MidLeft) +
                            (pSrc[5 + stride] * m.Pixel) +
                            (pSrc[8 + stride] * m.MidRight) +
                            (pSrc[2 + stride2] * m.BottomLeft) +
                            (pSrc[5 + stride2] * m.BottomMid) +
                            (pSrc[8 + stride2] * m.BottomRight))
                            / m.Factor) + m.Offset);

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;
                        p[5 + stride] = (byte)nPixel;

                        nPixel = ((((pSrc[1] * m.TopLeft) +
                            (pSrc[4] * m.TopMid) +
                            (pSrc[7] * m.TopRight) +
                            (pSrc[1 + stride] * m.MidLeft) +
                            (pSrc[4 + stride] * m.Pixel) +
                            (pSrc[7 + stride] * m.MidRight) +
                            (pSrc[1 + stride2] * m.BottomLeft) +
                            (pSrc[4 + stride2] * m.BottomMid) +
                            (pSrc[7 + stride2] * m.BottomRight))
                            / m.Factor) + m.Offset);

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;
                        p[4 + stride] = (byte)nPixel;

                        nPixel = ((((pSrc[0] * m.TopLeft) +
                                       (pSrc[3] * m.TopMid) +
                                       (pSrc[6] * m.TopRight) +
                                       (pSrc[0 + stride] * m.MidLeft) +
                                       (pSrc[3 + stride] * m.Pixel) +
                                       (pSrc[6 + stride] * m.MidRight) +
                                       (pSrc[0 + stride2] * m.BottomLeft) +
                                       (pSrc[3 + stride2] * m.BottomMid) +
                                       (pSrc[6 + stride2] * m.BottomRight))
                            / m.Factor) + m.Offset);

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;
                        p[3 + stride] = (byte)nPixel;

                        p += 3;
                        pSrc += 3;
                    }

                    p += nOffset;
                    pSrc += nOffset;
                }
            }

            b.UnlockBits(bmData);
            bSrc.UnlockBits(bmSrc);
            return true;
        }

    }

    public class ConvMatrix
    {
        public int TopLeft = 0, TopMid = 0, TopRight = 0;
        public int MidLeft = 0, Pixel = 1, MidRight = 0;
        public int BottomLeft = 0, BottomMid = 0, BottomRight = 0;
        public int Factor = 1;
        public int Offset = 0;
        public void SetAll(int nVal)
        {
            TopLeft = TopMid = TopRight = MidLeft = Pixel = MidRight =
                      BottomLeft = BottomMid = BottomRight = nVal;
        }
    }
}
