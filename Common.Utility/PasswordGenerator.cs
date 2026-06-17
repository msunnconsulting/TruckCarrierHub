namespace Common.Utility
{
    using System;
    using System.Security.Cryptography;

    public class PasswordGenerator
    {
        /// <summary>
        /// hash the plain text password with provided salt and return hashed password
        /// </summary>
        /// <param name="salt">salt to used to generate hashed pwd</param>
        /// <param name="password">plain text pwd</param>
        /// <returns></returns>
        public static string GetHashedPassword(string salt, string password)
        {
            Rfc2898DeriveBytes hasher = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000);

            byte[] hash = hasher.GetBytes(64);

            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Gets a new salt, to be used when genearating a new password
        /// </summary>
        /// <returns></returns>
        public static string GetSalt()
        {
            int max_length = 32;

            byte[] salt = new byte[max_length];

            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            random.GetNonZeroBytes(salt);

            return Convert.ToBase64String(salt);
        }
    }
}
