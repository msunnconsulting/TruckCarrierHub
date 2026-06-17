namespace Common.Utility.Security
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// This is a static RSAEncryption class.
    /// </summary>
    public static class RSAEncryption
    {
        ////private static RSACryptoServiceProvider rsa;

        /// <summary>
        /// This is a constant field for SEPARATOR1
        /// </summary>
        public const string SEPERATOR1 = "##%%##";

        /// <summary>
        /// This is a constant field for SEPARATOR2
        /// </summary>
        public const string SEPERATOR2 = "##$$##";

        /// <summary>
        /// This is a constant field for SEPARATOR3
        /// </summary>
        public const string SEPERATOR3 = "##@@##";

        /// <summary>
        /// This is a constant field for PROVIDERSAFULL
        /// </summary>
        private const int PROVIDERRSAFULL = 1;

        /// <summary>
        /// This is a constant field for CONTAINERNAME
        /// </summary>
        private const string CONTAINERNAME = "RSAKeyContainer";

        /// <summary>
        /// This is a constant field for PRIVATEKEYXMLNAME
        /// </summary>
        private const string PRIVATEKEYXMLNAME = "RSAPrivateKey.xml";

        /// <summary>
        /// This is a constant field for PUBLICKEYXMLNAME
        /// </summary>
        private const string PUBLICKEYXMLNAME = "RSAPublicKey.xml";

        /// <summary>
        /// used to generate new private and public keys
        /// </summary>
        /// <param name="folderPathOfKeyFiles">path where private key and public key will be generated.</param>
        public static void GenerateNewKeys(string folderPathOfKeyFiles)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(GetParameters()))
            {
                ////provide public and private RSA params
                StreamWriter writer = new StreamWriter(Path.Combine(folderPathOfKeyFiles, PRIVATEKEYXMLNAME));
                string publicPrivateKeyXML = rsa.ToXmlString(true);
                writer.Write(publicPrivateKeyXML);
                writer.Close();

                ////provide public only RSA params
                writer = new StreamWriter(Path.Combine(folderPathOfKeyFiles, PUBLICKEYXMLNAME));
                string publicOnlyKeyXML = rsa.ToXmlString(false);
                writer.Write(publicOnlyKeyXML);
                writer.Close();
                rsa.Clear();
            }
        }

        /// <summary>
        /// Encrypts the data using the public key.
        /// </summary>
        /// <param name="dataToEncrypt">data to be encrypted</param>
        /// <param name="publicKeyFilePath">path of public key file</param>
        /// <returns>returns Encrypted Data</returns>
        public static string EncryptData(string dataToEncrypt, string publicKeyFilePath)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(GetParameters()))
            {
                StreamReader reader = new StreamReader(publicKeyFilePath);
                string publicOnlyKeyXML = reader.ReadToEnd();
                reader.Close();
                rsa.FromXmlString(publicOnlyKeyXML);

                string strEncryptedData = string.Empty;
                ////int maxLength = 117;
                string[] arrayChunk = Regex.Split(dataToEncrypt, "(.{117})", RegexOptions.Singleline);

                int intLoopCount = 0;

                foreach (string chunk in arrayChunk)
                {
                    if (intLoopCount != 0)
                    {
                        strEncryptedData += RSAEncryption.SEPERATOR3;
                    }

                    ////read plaintext, encrypt it to ciphertext
                    byte[] plainbytes = System.Text.Encoding.UTF8.GetBytes(chunk);
                    byte[] cipherbytes = rsa.Encrypt(plainbytes, false);

                    strEncryptedData += Convert.ToBase64String(cipherbytes);
                    intLoopCount += 1;
                }

                rsa.Clear();
                return strEncryptedData;
            }
        }

        /// <summary>
        /// decrypts the data using private key.
        /// </summary>
        /// <param name="dataToDecrypt">data to be decrypted</param>
        /// <param name="privateKeyFilePath">path of private key file</param>
        /// <returns>returns string of decrypted data</returns>
        public static string DecryptData(string dataToDecrypt, string privateKeyFilePath)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(GetParameters()))
            {
                StreamReader reader = new StreamReader(privateKeyFilePath);
                string publicPrivateKeyXML = reader.ReadToEnd();
                reader.Close();
                rsa.FromXmlString(publicPrivateKeyXML);

                string strDecryptedData = string.Empty;
                string[] arrEncrypted = dataToDecrypt.Split(new string[] { RSAEncryption.SEPERATOR3 }, StringSplitOptions.None);
                foreach (string encryptedDataChunk in arrEncrypted)
                {
                    byte[] encryptedDataChunkBytes = Convert.FromBase64String(encryptedDataChunk);

                    ////read ciphertext, decrypt it to plaintext
                    byte[] plain = rsa.Decrypt(encryptedDataChunkBytes, false);
                    strDecryptedData += System.Text.Encoding.UTF8.GetString(plain);
                }

                rsa.Clear();
                return strDecryptedData;
            }
        }

        /// <summary>
        /// This is a method for getting Parameters
        /// </summary>
        /// <returns>returns Parameters</returns>
        private static CspParameters GetParameters()
        {
            CspParameters cspParams = new CspParameters(PROVIDERRSAFULL);
            cspParams.KeyContainerName = CONTAINERNAME;
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            cspParams.ProviderName = "Microsoft Strong Cryptographic Provider";
            return cspParams;
        }
    }

    /// <summary>
    /// This is a TripleDESEncryption class.
    /// </summary>
    public class TripleDESEncryption
    {
        ////REFERENCE SITE: http://codemaverick.blogspot.com/2007/01/tripledes-encryption-in-net.html

        /// <summary>
        /// This is a static readonly field for IVector
        /// </summary>
        private static readonly byte[] IVector = new byte[8] { 27, 9, 45, 27, 0, 72, 171, 54 };

        /// <summary>
        /// This is a static field for key
        /// </summary>
        private static string key = "C5F1433A-AC99-4650-ABE8-34B6D65447A0";

        /// <summary>
        /// Encrypt the supplied data using TripleDES Algorithm.
        /// </summary>
        /// <param name="dataToEncrypt">Enter Data To Encrypt</param>
        /// <returns>returns Encrypted Data</returns>
        public static string EncryptData(string dataToEncrypt)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(dataToEncrypt);
            TripleDESCryptoServiceProvider tripleDes = new TripleDESCryptoServiceProvider();
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            tripleDes.Key = md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(key));
            tripleDes.IV = IVector;
            ICryptoTransform itransform = tripleDes.CreateEncryptor();
            return Convert.ToBase64String(itransform.TransformFinalBlock(buffer, 0, buffer.Length));
        }

        /// <summary>
        /// Decrypt TripleDES encrypted string.
        /// </summary>
        /// <param name="dataToDecrypt">Enter Data To Decrypt</param>
        /// <returns>returns Decrypted Data</returns>
        public static string DecryptData(string dataToDecrypt)
        {
            byte[] buffer = Convert.FromBase64String(dataToDecrypt);
            TripleDESCryptoServiceProvider tripleDes = new TripleDESCryptoServiceProvider();
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            tripleDes.Key = md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(key));
            tripleDes.IV = IVector;
            ICryptoTransform itransform = tripleDes.CreateDecryptor();
            return Encoding.ASCII.GetString(itransform.TransformFinalBlock(buffer, 0, buffer.Length));
        }
    }
}