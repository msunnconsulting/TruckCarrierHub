namespace Common.Utility.ImageProcessing
{
    using System.Drawing.Imaging;

    /// <summary>
    /// this class represent image information
    /// </summary>
    public sealed class ImageInfo
    {
        /// <summary>
        /// Gets or sets file name without extension
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets extension of the image file
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets Height of the image in pixels
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets Width of the image in pixels
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets pixel per image for the image
        /// </summary>
        public float PPI { get; set; }

        /// <summary>
        /// Gets or sets raw format of the image e.g. jpeg, gif etc
        /// </summary>
        public ImageFormat Format { get; set; }
    }
}
