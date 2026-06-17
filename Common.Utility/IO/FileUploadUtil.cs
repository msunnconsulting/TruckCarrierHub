namespace Common.Utility.IO
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.UI.WebControls;

    /// <summary>
    /// This is a static File Upload Util class.
    /// </summary>
    public static class FileUploadUtil
    {
        /// <summary>
        /// Used to check if uploaded file is image or not.
        /// </summary>
        /// <param name="fupImage">file upload control used to upload the image</param>
        /// <returns>indicates that image is valid or not</returns>
        public static bool IsValidImage(HttpPostedFile fupImage)
        {
            string ext = Path.GetExtension(fupImage.FileName);
            return (fupImage.ContentType.IndexOf("image") >= 0) && (ext == ".gif" || ext == ".jpg" || ext == ".png" || ext == ".jpeg");
        }

        /// <summary>
        /// Uploads file to directory.
        /// </summary>
        /// <param name="fup">file upload control having posted file.</param>
        /// <param name="uploadDir">physical path of the directory where to upload this file</param>
        /// <param name="saveOption">
        /// RenameIfExist : rename the file if it already exist by post fixing numbers and save it anyway.
        /// OverWriteIfExist : overwrite the existing file by uploading file.
        /// ErrorIfExist : throw error if file with same name already exist.
        /// </param>
        /// <returns>returns final file name without full path</returns>
        public static string Upload(HttpPostedFile fup, string uploadDir, FileSaveOption saveOption)
        {
            return Upload(fup, uploadDir, null, saveOption, null);
        }

        /// <summary>
        /// Uploads file to upload directory. Also, allow specifying new file name to be used while uploading.
        /// </summary>
        /// <param name="fup">file upload control having posted file.</param>
        /// <param name="uploadDir">physical path of the directory where to upload this file</param>
        /// <param name="fileName">If specified, then uploaded file will use this name and get a valid name from this field and use it, If NULL then use the name of the file being uploaded only.</param>
        /// <param name="saveOption">
        /// RenameIfExist : rename the file if it already exist by post fixing numbers and save it anyway.
        /// OverWriteIfExist : overwrite the existing file by uploading file.
        /// ErrorIfExist : throw error if file with same name already exist.
        /// </param>
        /// <param name="maxFileNameLength">optional parameter to specify max length of the file name to be saved with.</param>
        /// <returns>returns final file name without full path</returns>
        public static string Upload(HttpPostedFile fup, string uploadDir, string fileName, FileSaveOption saveOption, int? maxFileNameLength = null)
        {
            string fileNameWithoutExt;

            if (saveOption == FileSaveOption.UseGUIDAsName)
                fileNameWithoutExt = Guid.NewGuid().ToString();
            else if (string.IsNullOrEmpty(fileName)) // user uploaded file name
                fileNameWithoutExt = FileUtil.GetValidName(Path.GetFileNameWithoutExtension(fup.FileName), maxFileNameLength);
            else // use passed file name
                fileNameWithoutExt = FileUtil.GetValidName(Path.GetFileNameWithoutExtension(fileName), maxFileNameLength);

            string fileExt = Path.GetExtension(fup.FileName);
            string filePath = uploadDir + fileNameWithoutExt + fileExt;

            if (!System.IO.Directory.Exists(uploadDir))
                System.IO.Directory.CreateDirectory(uploadDir);

            if (System.IO.File.Exists(filePath))
            {
                switch (saveOption)
                {
                    case FileSaveOption.RenameIfExist:
                        {
                            int i = 1;
                            while (System.IO.File.Exists(filePath))
                            {
                                filePath = uploadDir + fileNameWithoutExt + "_" + i.ToString() + fileExt;
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
                            throw new Exception("File you are trying to upload already exist.");
                        }

                    case FileSaveOption.UseGUIDAsName:
                        {
                            do
                            {
                                fileNameWithoutExt = Guid.NewGuid().ToString();
                                filePath = uploadDir + fileNameWithoutExt + fileExt;
                            }
                            while (System.IO.File.Exists(filePath));
                            break;
                        }
                }
            }

            fup.SaveAs(filePath);

            return Path.GetFileName(filePath);
        }

        /// <summary>
        /// Gets the information about the file being uploaded from uploaded file control.
        /// </summary>
        /// <param name="fup">file upload control uploading the file.</param>
        /// <param name="fileContent">parameter which holds the byte content of file</param>
        /// <param name="fileName">parameter which holds the file name.</param>
        /// <param name="mimeType">parameter which holds the mime type of file.</param>
        /// <returns>True: Success, False: Failure.</returns>
        public static bool GetUploadInfo(FileUpload fup, ref byte[] fileContent, ref string fileName, ref string mimeType)
        {
            if (fup.HasFile)
            {
                fileContent = new byte[fup.PostedFile.ContentLength];
                fup.PostedFile.InputStream.Read(fileContent, 0, fup.PostedFile.ContentLength);

                fileName = fup.FileName;

                mimeType = fup.PostedFile.ContentType;
            }

            return fup.HasFile;
        }
    }
}
