namespace Common.Utility.ImageProcessing
{
    using IO;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;

    /// <summary>
    /// this class is use to image generate,remove images etc (all the image related task)
    /// </summary>
    public static class ImagerResizer
    {
        /// <summary>
        /// this is a constant field for default thumb suffix 
        /// </summary>
        private const string DefaultThumbSuffix = "_thumb";

        /// <summary>
        /// Generates image thumb from passed image path. Original Image will remain as it is. Newly generated thumb will have _thumb as suffix to the original image name.
        /// </summary>
        /// <param name="imagePath">Path of image, for which we want to generate thumb.</param>
        /// <param name="maxWidth">Max width of the newly generated thumb.</param>
        /// <param name="maxHeight">Max height of the newly generate thumb.</param>
        /// <returns>returns newly generated image name. Example : MyPic.jpg</returns>
        public static string GenerateThumb(string imagePath, int? maxWidth, int? maxHeight)
        {
            return GenerateThumb(imagePath, maxWidth, maxHeight, DefaultThumbSuffix);
        }

        /// <summary>
        /// Generates image thumb from passed image path. Original Image will remain as it is.
        /// </summary>
        /// <param name="imagePath">Path of image, for which we want to generate thumb.</param>
        /// <param name="maxWidth">Max width of the newly generated thumb.</param>
        /// <param name="maxHeight">Max height of the newly generate thumb.</param>
        /// <param name="thumbSuffix">thumb suffix for the newly thumb image name</param>
        /// <returns>returns newly generated image name. Example : MyPic.jpg</returns>
        public static string GenerateThumb(string imagePath, int? maxWidth, int? maxHeight, string thumbSuffix)
        {
            //string thumbImageName = GetResizeImageName(imagePath, thumbSuffix);
            //string thumbImageName = GetResizeImageName(imagePath, thumbSuffix);
            string fileExt = Path.GetExtension(imagePath);
            string thumbImagePath = Path.GetDirectoryName(imagePath) + "\\" + thumbSuffix + fileExt;
            File.Copy(imagePath, thumbImagePath);

            Resize(thumbImagePath, maxWidth, maxHeight);

            return thumbSuffix;
        }

        /// <summary>
        /// Resize the passed image keeping its aspect ratio. It would decide we should resize based on width / height depending on the aspect ratio, 
        /// and either take max width or max height to perform re-size operation.
        /// </summary>
        /// <param name="imagePath">Physical path of image file to be re-sized.</param>
        /// <param name="maxWidth">What should be the max width while re-sizing.</param>
        /// <param name="maxHeight">What should be the max height while re-sizing.</param>
        public static void Resize(string imagePath, int? maxWidth, int? maxHeight)
        {
            System.Drawing.Image objThumbNail = null, objImg = null;
            Graphics objGraphic = null;

            try
            {
                int nw = 0, nh = 0;
                float ratioW, ratioH;

                if (!maxWidth.HasValue && !maxHeight.HasValue)
                    return;

                if (!maxWidth.HasValue)
                    maxWidth = int.MaxValue;

                if (!maxHeight.HasValue)
                    maxHeight = int.MaxValue;

                objImg = System.Drawing.Image.FromFile(imagePath);

                //// If we don't have a max width or max height, OR the image is smaller than both
                //// we do not want to resize it, so we simply output the original image and exit
                if (maxWidth >= objImg.Width && maxHeight >= objImg.Height)
                {
                    return;
                }

                ratioW = (float)maxWidth.Value / (float)objImg.Width;
                ratioH = (float)maxHeight.Value / (float)objImg.Height;

                if (ratioW * objImg.Height < maxHeight)
                {
                    //// Resize the image based on width
                    nh = Convert.ToInt16(Math.Ceiling(ratioW * (float)objImg.Height));
                    nw = maxWidth.Value;
                }
                else
                {
                    nw = Convert.ToInt16(Math.Ceiling(ratioH * (float)objImg.Width));
                    nh = maxHeight.Value;
                }

                ////oThumbNail = new Bitmap(NW, NH, oImg.PixelFormat);
                objThumbNail = new Bitmap(nw, nh);
                objGraphic = Graphics.FromImage(objThumbNail);
                objGraphic.CompositingQuality = CompositingQuality.HighQuality;
                objGraphic.SmoothingMode = SmoothingMode.HighQuality;
                objGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                objGraphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                Rectangle objRectangle = new Rectangle(0, 0, nw, nh);
                objGraphic.DrawImage(objImg, objRectangle);

                string fileExt = Path.GetExtension(imagePath).ToLower();
                ////ImageFormat oFormat;
                ////switch (FileExt)
                ////{
                ////    case ".jpg":
                ////    case ".jpeg":
                ////        {
                ////            oFormat = ImageFormat.Jpeg;
                ////            break;
                ////        }
                ////    case ".gif":
                ////        {
                ////            oFormat = ImageFormat.Gif;
                ////            break;
                ////        }
                ////    case ".bmp":
                ////        {
                ////            oFormat = ImageFormat.Bmp;
                ////            break;
                ////        }
                ////    case ".png":
                ////        {
                ////            oFormat = ImageFormat.Png;
                ////            break;
                ////        }
                ////    default:
                ////        {
                ////            throw new Exception("File Format " + FileExt + " Not Supported.");
                ////        }
                ////}

                string fileDir = Path.GetDirectoryName(imagePath) + "\\";

                string newFileName;

                newFileName = Path.GetRandomFileName();
                while (File.Exists(fileDir + newFileName))
                {
                    newFileName = Path.GetRandomFileName();
                }

                objThumbNail.Save(fileDir + newFileName);

                objImg.Dispose();
                objThumbNail.Dispose();
                objGraphic.Dispose();

                //// renaming to actual image name
                FileUtil.Rename(fileDir + newFileName, imagePath, true);
                ////File.Delete(imagePath);
                ////File.Move(fileDir + newFileName, imagePath);
            }
            catch (Exception ex)
            {
                if (objImg != null)
                    objImg.Dispose();

                if (objThumbNail != null)
                    objThumbNail.Dispose();

                if (objGraphic != null)
                    objGraphic.Dispose();

                throw ex;
            }
            finally
            {
                if (objImg != null)
                    objImg.Dispose();

                if (objThumbNail != null)
                    objThumbNail.Dispose();

                if (objGraphic != null)
                    objGraphic.Dispose();
            }
        }

        /// <summary>
        /// Converts file name to thumb image name by suffixing the default thumb suffix.
        /// </summary>
        /// <param name="fileName">File name for which thumb file is generated. Ex: test.jpg to test_thumb.jpg</param>
        /// <returns>Image name which is resized</returns>
        public static string GetResizeImageName(string fileName)
        {
            return GetResizeImageName(fileName, DefaultThumbSuffix);
        }

        /// <summary>
        /// Converts file name to thumb image name by suffixing the suffix supplied.
        /// </summary>
        /// <param name="fileName">File name for which thumb file is generated. Ex: test.jpg to test_thumb.jpg</param>
        /// <param name="suffix">suffix for this thumb file name generation.</param>
        /// <returns>Image name which is resized</returns>
        public static string GetResizeImageName(string fileName, string suffix)
        {
            return Path.GetFileNameWithoutExtension(fileName) + suffix + Path.GetExtension(fileName);
        }

        /// <summary>
        ///  return image info from image path
        /// </summary>
        /// <param name="filePath">Enter File Path for which thumb file is generated</param>
        /// <returns>image info</returns>
        public static ImageInfo GetImageInfo(string filePath)
        {
            System.Drawing.Image img = null;
            Graphics objGraphic = null;
            ImageInfo imgInfo = null;
            try
            {
                img = System.Drawing.Image.FromFile(filePath);
                objGraphic = Graphics.FromImage(img);

                imgInfo = new ImageInfo()
                {
                    FileName = Path.GetFileNameWithoutExtension(filePath),
                    Extension = Path.GetExtension(filePath),
                    Height = img.Height,
                    Width = img.Width,
                    PPI = img.HorizontalResolution,
                    Format = img.RawFormat
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (img != null)
                    img.Dispose();

                if (objGraphic != null)
                    objGraphic.Dispose();
            }

            return imgInfo;
        }
    }
}
