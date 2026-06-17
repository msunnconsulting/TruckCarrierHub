namespace PartnerCarrier.Infrastructure.Database
{
    using System;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// a classed used to log SQL queries
    /// </summary>
    public class SQLLogger
    {
        private static string FilePath; // path to the file where SQL is logged
        static object lockLogMethod = new object();

        /// <summary>
        /// constructor for the logger class
        /// </summary>
        /// <param name="filePath">path to the file where SQL is logged</param>
        /// <param name="clearFile">path to the file where SQL is logged</param>
        public static void Init(string filePath, bool clearFile = true)
        {
            if (filePath != null)
                FilePath = filePath;
            else
            {
                //string reqPath = string.Empty;
                //if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null)
                //    reqPath = System.Web.HttpContext.Current.Request.Url.AbsolutePath.Replace('/', '-');

                string dirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
                dirPath = dirPath.Replace(@"file:\", string.Empty).Trim();
                dirPath = Directory.GetParent(dirPath).FullName;
                FilePath = dirPath + "\\EFSQLLog\\SQLLog-" + DateTime.Today.ToString("dd-MM-yyyy") + ".txt";
            }

            FilePath = FilePath.Replace(@"file:\", string.Empty).Trim();

            string fileDirectory = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(fileDirectory))
                Directory.CreateDirectory(fileDirectory);

            if (clearFile && File.Exists(FilePath))
            {
                FileInfo f = new FileInfo(FilePath);
                if (f.LastWriteTime < DateTime.Now.AddSeconds(-10))
                    //Log("");
                    File.Delete(FilePath);
            }
        }
        /// <summary>
        /// Logs the message for recently executed SQL Query
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            lock (lockLogMethod)
            {


                //tryagain:
                //try
                //{


                if (!message.Contains("__MigrationHistory") && !message.Contains("INFORMATION_SCHEMA")
                    && !message.Contains("-- Executing at ") && !message.Contains("-- Completed ")
                    && message != "\r\n")
                {
                    //var url = (System.Web.HttpContext.Current != null 
                    //    && System.Web.HttpContext.Current.Request != null
                    //    && !message.StartsWith("--")) 

                    //    ? System.Web.HttpContext.Current.Request.Url.ToString() 
                    //    : string.Empty;                    

                    //message =  url + Environment.NewLine + message;
                }
                using (StreamWriter sw = File.AppendText(FilePath))
                {
                    sw.WriteLine(message);
                    sw.Close();
                    sw.Dispose();
                }


                //}
                //catch (IOException ex)
                //{
                //    if (ex.Message.Contains("process cannot access"))
                //    {
                //        System.Threading.Thread.Sleep(1000);
                //        goto tryagain;

                //    }
                //    else if (ex.Message.Contains("Access to the path denied"))
                //    {
                //        System.Threading.Thread.Sleep(2000);
                //        goto tryagain;
                //    }
                //    else
                //        throw ex;
                //}
                //finally
                //{

                //}                                    
            }
        }
    }
}
