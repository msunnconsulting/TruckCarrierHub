namespace Common.Utility.Security
{
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// This is static MD5Util class.
    /// </summary>
    public static class MD5Util
    {
        /// <summary>
        /// This method generate hash code.
        /// </summary>
        /// <param name="input">Enter input for generate hash code</param>
        /// <returns>returns hash code</returns>
        public static string GenerateHash(string input)
        {
            //// step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            //// step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));
            return sb.ToString();
        }
    }
}
