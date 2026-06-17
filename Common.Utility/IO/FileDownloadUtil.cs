namespace Common.Utility.IO
{
    using System;
    using System.IO;
    using System.Net;

    /// <summary>
    /// This utility class provides methods for easy file download programatically
    /// </summary>
    public static class FileDownloadUtil
    {

        /// <summary>
        /// Downloads file to local machine and returns full file path of downloaded file.
        /// </summary>
        /// <param name="downloadUrl">url to download the file</param>
        /// <param name="downloadDir">Path where downloaded file should be saved</param>
        /// <param name="fileName">If specified, then downloaded file will use this name and get a valid name from this field and use it, If NULL then use the name of the file being uploaded only.</param>
        /// <param name="saveOption">
        /// RenameIfExist : rename the file if it already exist by post fixing numbers and save it anyway.
        /// OverWriteIfExist : overwrite the existing file by uploading file.
        /// ErrorIfExist : throw error if file with same name already exist.
        /// </param>
        /// <param name="maxFileNameLength">optional parameter to specify max length of the file name to be saved with.</param>
        /// <returns>Full file path of downloaded file</returns>
        public static string Download(string downloadUrl, string downloadDir, string fileName = null, FileSaveOption saveOption = FileSaveOption.ErrorIfExist, int? maxFileNameLength = null)
        {

            string fileNameWithoutExt;

            if (saveOption == FileSaveOption.UseGUIDAsName)
                fileNameWithoutExt = Guid.NewGuid().ToString();
            else if (string.IsNullOrEmpty(fileName)) // downloaded file name
                fileNameWithoutExt = FileUtil.GetValidName(Path.GetFileNameWithoutExtension(downloadUrl), maxFileNameLength);
            else // use passed file name
                fileNameWithoutExt = FileUtil.GetValidName(Path.GetFileNameWithoutExtension(fileName), maxFileNameLength);

            string fileExt = Path.GetExtension(downloadUrl);
            string filePath = Path.Combine(downloadDir, fileNameWithoutExt + fileExt);

            if (!Directory.Exists(downloadDir))
                Directory.CreateDirectory(downloadDir);

            if (File.Exists(filePath))
            {
                switch (saveOption)
                {
                    case FileSaveOption.RenameIfExist:
                        {
                            int i = 1;
                            while (File.Exists(filePath))
                            {
                                filePath = Path.Combine(downloadDir, fileNameWithoutExt + "_" + i.ToString() + fileExt);
                                i++;
                            }

                            break;
                        }

                    case FileSaveOption.OverWriteIfExist:
                        {
                            System.IO.File.Delete(filePath);
                            break;
                        }

                    case FileSaveOption.ErrorIfExist:
                        {
                            throw new Exception("File " + filePath + " already exist.");
                        }

                    case FileSaveOption.UseGUIDAsName:
                        {
                            do
                            {
                                fileNameWithoutExt = Guid.NewGuid().ToString();
                                filePath = Path.Combine(downloadDir, fileNameWithoutExt + fileExt);
                            }
                            while (System.IO.File.Exists(filePath));
                            break;
                        }
                }
            }

            WebClient webClient = new WebClient();
            webClient.DownloadFile(downloadUrl, filePath);
            return Path.GetFileName(filePath);

        }
    }
}
