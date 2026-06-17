namespace Common.Utility
{
    using System;
    using System.IO;

    public static class Base64Util
    {
        public static void Base64ToImage(string imagePath, string base64)
        {
            var tokenToVerify = "base64,";
            if (base64.Contains(tokenToVerify))
            {
                base64 = base64.Substring(base64.IndexOf(tokenToVerify) + tokenToVerify.Length);
            }
            var bytes = Convert.FromBase64String(base64);
            using (var imageFile = new FileStream(imagePath, FileMode.Create))
            {
                imageFile.Write(bytes, 0, bytes.Length);
                imageFile.Flush();
            }
        }

        public static bool HasBase64ImageData(string base64)
        {
            return base64.Contains(";base64,");
        }
    }
}
