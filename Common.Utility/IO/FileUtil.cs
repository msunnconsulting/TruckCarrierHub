namespace Common.Utility.IO
{
    using System.IO;

    /// <summary>
    /// This is a static class for File Util
    /// </summary>
    public static class FileUtil
    {
        /// <summary>
        /// This is a method for check the file is successfully deleted or not
        /// </summary>
        /// <param name="filePath">Enter file path to delete</param>
        /// <returns>indicates that specific file is deleted or not</returns>
        public static bool TryDelete(string filePath)
        {
            bool isSuccess = true;
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch
                {
                    isSuccess = false;
                }
            }

            return isSuccess;
        }

        /// <summary>
        /// Replaces all invalid characters in file name with _ and return new valid file name.
        /// </summary>
        /// <param name="fileName">current file name</param>        
        /// <returns>returns Valid name</returns>
        public static string GetValidName(string fileName)
        {
            return GetValidName(fileName, null);
        }

        /// <summary>
        /// Replaces all invalid characters in file name with _ and return new valid file name. Also valid max length as well.
        /// </summary>
        /// <param name="fileName">current file name</param>
        /// <param name="maxLength">maximum length allowed for file name</param>
        /// <returns>returns valid name</returns>
        public static string GetValidName(string fileName, int? maxLength = null)
        {
            string fileExtension = Path.GetExtension(fileName);

            fileName = Path.GetFileNameWithoutExtension(fileName);

            char[] arrInvalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in arrInvalidChars)
            {
                if (fileName.Contains(c.ToString()))
                    fileName = fileName.Replace(c, '_');
            }

            arrInvalidChars = new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|', '&', '#' };

            foreach (char c in arrInvalidChars)
            {
                if (fileName.Contains(c.ToString()))
                    fileName = fileName.Replace(c, '_');
            }

            if (maxLength.HasValue)
            {
                maxLength = maxLength - fileExtension.Length;

                if (fileName.Length > maxLength)
                    fileName = fileName.Substring(0, maxLength.Value);
            }

            return fileName + fileExtension;
        }

        /// <summary>
        /// Move file from source to destination. Also, check if destination directory exist or not. If not, it would created and move.
        /// </summary>
        /// <param name="sourceFilePath">Enter Source File Path</param>
        /// <param name="destinationFilePath">Enter Destination Path</param>
        public static void Move(string sourceFilePath, string destinationFilePath)
        {
            string destinationDirectory = Path.GetDirectoryName(destinationFilePath);
            if (!System.IO.Directory.Exists(destinationDirectory))
                System.IO.Directory.CreateDirectory(destinationDirectory);
            System.IO.File.Move(sourceFilePath, destinationFilePath);
        }

        /// <summary>
        /// Rename a file with another name.
        /// </summary>
        /// <param name="fileName">fileName with full path.</param>
        /// <param name="newFileName">new file name with full path.</param>
        ///  <param name="overwriteIfExist">overwrite if file exist.</param>
        /// <returns>True: indicates file re-named successfully. False, indicates no success in rename.</returns>
        public static bool Rename(string fileName, string newFileName, bool overwriteIfExist)
        {
            bool isSuccess = false;
            if (fileName != newFileName)
            {
                if (!System.IO.File.Exists(newFileName))
                {
                    System.IO.File.Copy(fileName, newFileName);
                    System.IO.File.Delete(fileName);
                    isSuccess = true;
                }
                else if (overwriteIfExist)
                {
                    //// delete existing file.
                    System.IO.File.Delete(newFileName);

                    System.IO.File.Copy(fileName, newFileName);
                    System.IO.File.Delete(fileName);
                    isSuccess = true;
                }
            }

            return isSuccess;
        }
    }
}
