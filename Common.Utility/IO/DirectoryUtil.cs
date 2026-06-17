namespace Common.Utility.IO
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// This is a static class for Directory Util
    /// </summary>
    public static class DirectoryUtil
    {
        /// <summary>
        /// Check if directory exist, if exist delete it, else not. Deletes the directory recursively.
        /// </summary>
        /// <param name="dirPath">Enter directory Path</param>
        /// <param name="recursive">indicates that directory is recursive or not by default it's true</param>
        /// /// <returns>true or false</returns>
        public static bool TryDelete(string dirPath, bool recursive = false)
        {
            bool isSuccess = true;
            if (System.IO.Directory.Exists(dirPath))
            {
                try
                {
                    System.IO.Directory.Delete(dirPath, recursive);
                }
                catch
                {
                    isSuccess = false;
                }
            }

            return isSuccess;
        }

        /// <summary>
        /// Faster Copy Directory from one location to another location using tool of Windows.
        /// </summary>
        /// <param name="sourceDir">Valid Source directory path. Must not end with \</param>
        /// <param name="destinationDir">Valid Destination directory path. Must not end with \</param>
        /// <param name="move">True will delete the source after copy.</param>
        /// <returns>Complete log of copy operation.</returns>
        public static string RoboCopy(string sourceDir, string destinationDir, bool move)
        {
            if (!Directory.Exists(sourceDir))
                throw new Exception("Source Doesn't Exist.");

            if (sourceDir.EndsWith("\\"))
                sourceDir = sourceDir.TrimEnd('\\');

            if (destinationDir.EndsWith("\\"))
                destinationDir = destinationDir.TrimEnd('\\');

            if (!Directory.Exists(destinationDir))
                Directory.CreateDirectory(destinationDir);

            string xcopyPath = Environment.GetEnvironmentVariable("WINDIR") + @"\System32\robocopy.exe";
            ProcessStartInfo info = new ProcessStartInfo(xcopyPath);
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            ////info.Arguments = string.Format("\"{0}\" \"{1}\" /C /E /F /I /G /H /K /B /J", source, destination);
            if (move)
                info.Arguments = string.Format("\"{0}\" \"{1}\" /E /ZB /COPYALL /MOVE /R:1 ", sourceDir, destinationDir);
            else
                info.Arguments = string.Format("\"{0}\" \"{1}\" /E /ZB /COPYALL /R:1 ", sourceDir, destinationDir);

            Process process = Process.Start(info);
            string result = process.StandardOutput.ReadToEnd();
            if (process.ExitCode != 1)
            {
                Exception ex = new Exception(result);
                throw ex;
            }

            return result;
        }
    }
}
