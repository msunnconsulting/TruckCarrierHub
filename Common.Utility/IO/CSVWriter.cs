namespace Common.Utility.IO
{
    using System.IO;
    using System.Text;

    /// <summary>
    /// This is sealed class for CSVWriter
    /// </summary>
    public sealed class CSVWriter
    {
        /// <summary>
        /// This is a constant field for QUOTE
        /// </summary>
        private const string QUOTE = "\"";

        /// <summary>
        /// This is a constant field for ESCAPED QUOTE
        /// </summary>
        private const string ESCAPEDQUOTE = "\"\"";

        /// <summary>
        /// This is a field for those characters that must be quoted
        /// </summary>
        private char[] characterThatMustBeQuoted = { ',', '"', '\n' };

        /// <summary>
        /// Initializes a new instance of the CSVWriter class.
        /// </summary>
        /// <param name="filePath">Enter file path</param>
        /// <param name="separator">Separator ","</param>
        public CSVWriter(string filePath, string separator = ",")
        {
            this.Separator = separator;
            this.FilePath = filePath;
        }

        /// <summary>
        /// Gets or sets a value of the separator
        /// </summary>
        private string Separator { get; set; }

        /// <summary>
        /// Gets or sets a value of the File Path
        /// </summary>
        private string FilePath { get; set; }

        /// <summary>
        /// This method is for write data into a file
        /// </summary>
        /// <param name="record">Enter records for write</param>
        public void Write(params string[] record)
        {
            StringBuilder sbData = new StringBuilder();

            for (int i = 0; i < record.Length; i++)
            {
                sbData.Append(this.Escape(record[i]));
                if (i != record.Length - 1)
                    sbData.Append(this.Separator);
            }

            StreamWriter sw = File.AppendText(this.FilePath);
            sw.WriteLine(sbData.ToString());
            sw.Close();
        }

        /// <summary>
        /// This method is for escape specific string
        /// </summary>
        /// <param name="s">Enter String that you want to escape</param>
        /// <returns>returns string that you enter with replacement</returns>
        private string Escape(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            if (s.Contains(QUOTE))
                s = s.Replace(QUOTE, ESCAPEDQUOTE);

            if (s.IndexOfAny(this.characterThatMustBeQuoted) > -1)
                s = QUOTE + s + QUOTE;

            return s;
        }
    }
}
