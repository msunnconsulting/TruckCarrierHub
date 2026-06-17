namespace Common.Utility.CaptchaUtil
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;

    /// <summary>
    /// Class that gives information about generated Captcha Image
    /// </summary>
    public class CaptchaImage
    {
        /// <summary>
        /// Text of the Captcha generated
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// Images in Base64Encoded format
        /// </summary>
        public readonly string Image;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="image"></param>
        public CaptchaImage(string text, string image)
        {
            this.Text = text;
            this.Image = image;
        }
    }

    /// <summary>
    /// Utility class that helps generating captcha
    /// </summary>
    public static class CaptchaUtil
    {

        /// <summary>
        /// Generates random captcha and returns information of the same
        /// </summary>
        /// <param name="width">width of image</param>
        /// <param name="height">height of image</param>
        /// <param name="fontFamily">font family for the captcha text</param>
        /// <param name="fontColor">font color in hex format for the captcha text</param>
        /// <returns></returns>
        public static CaptchaImage GetCaptchaImage(int width, int height, FontFamily fontFamily, string fontColor)
        {
            string s1 = Util.GetRandomAlphaNumericString(4);
            string s2 = Util.GetRandomAlphaNumericString(3);
            return GetCaptchaImage(width, height, fontFamily, fontColor, s1 + " " + s2);
        }

        /// <summary>
        /// Generates captcha for supplied text and returns information of the same
        /// </summary>
        /// <param name="width">width of image</param>
        /// <param name="height">height of image</param>
        /// <param name="fontFamily">font family for the captcha text</param>
        /// <param name="fontColor">font color in hex format for the captcha text</param>
        /// <param name="text">text for captcha</param>
        /// <returns></returns>
        public static CaptchaImage GetCaptchaImage(int width, int height, FontFamily fontFamily, string fontColor, string text)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width");

            if (height <= 0)
                throw new ArgumentOutOfRangeException("height");

            if (fontFamily == null)
                throw new ArgumentNullException("fontFamily");

            if (string.IsNullOrEmpty(fontColor))
                throw new ArgumentNullException("fontColor");

            // try color conversion, to ensure the process can continue
            ColorTranslator.FromHtml(fontColor);

            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            Bitmap bitmap = null;
            Graphics g = null;
            HatchBrush hatchBrush = null;
            GraphicsPath path = null;
            Matrix matrix = null;
            Font font = null;

            try
            {
                // Create a new 32-bit bitmap image.
                bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                // Create a graphics object for drawing.
                g = Graphics.FromImage(bitmap);
                g.PageUnit = GraphicsUnit.Pixel;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(0, 0, width, height);

                // Fill in the background.
                hatchBrush = new HatchBrush(HatchStyle.Shingle, Color.Gray, Color.White);
                g.FillRectangle(hatchBrush, rect);

                // Set up the text font.
                SizeF size;
                float fontSize = rect.Height + 1;
                // Adjust the font size until the text fits within the image.
                do
                {
                    fontSize--;
                    font = new Font(fontFamily.Name, fontSize, GraphicsUnit.Pixel);
                    size = g.MeasureString(text, font);
                } while (size.Width > rect.Width);

                // Set up the text format.
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                // Create a path using the text and warp it randomly.
                path = new GraphicsPath();

                path.AddString(text, font.FontFamily, (int)font.Style, font.Size, rect, format);
                float v = 4F;
                Random random = new Random();
                PointF[] points =
            {
                new PointF(random.Next(rect.Width) / v, random.Next(rect.Height) / v),
                new PointF(rect.Width - random.Next(rect.Width) / v, random.Next(rect.Height) / v),
                new PointF(random.Next(rect.Width) / v, rect.Height - random.Next(rect.Height) / v),
                new PointF(rect.Width - random.Next(rect.Width) / v, rect.Height - random.Next(rect.Height) / v)
            };
                matrix = new Matrix();
                matrix.Translate(0F, 0F);
                path.Warp(points, rect, matrix, WarpMode.Perspective, 0F);

                // Draw the text.
                hatchBrush = new HatchBrush(HatchStyle.Shingle, ColorTranslator.FromHtml(fontColor), ColorTranslator.FromHtml(fontColor));
                g.FillPath(hatchBrush, path);

                //// Add some random noise.
                //HatchBrush hatchBrush1 = new HatchBrush(HatchStyle.Shingle, Color.White, Color.White);            
                //int m = Math.Max(rect.Width, rect.Height);
                //for (int i = 0; i < (int)(rect.Width * rect.Height / 30F); i++)
                //{
                //    int x = this.random.Next(rect.Width);
                //    int y = this.random.Next(rect.Height);
                //    int w = this.random.Next(m / 50);
                //    int h = this.random.Next(m / 50);
                //    g.FillEllipse(hatchBrush1, x, y, w, h);
                //}

                // Convert the image to byte[]
                MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Bmp);
                byte[] imageBytes = stream.ToArray();

                CaptchaImage image = new CaptchaImage(text, Convert.ToBase64String(imageBytes));

                return image;

            }
            finally
            {
                if (bitmap != null)
                    bitmap.Dispose();

                if (g != null)
                    g.Dispose();

                if (hatchBrush != null)
                    hatchBrush.Dispose();

                if (path != null)
                    path.Dispose();

                if (matrix != null)
                    matrix.Dispose();

                if (font != null)
                    font.Dispose();
            }
        }
    }

}