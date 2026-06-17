namespace Common.Utility.IO
{
    using Comparers;
    using Extensions;
    using Microsoft.VisualBasic.FileIO;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// This is a class that allows reading delimited files
    /// </summary>
    public sealed class CSVReader
    {
        # region Constructor

        /// <summary>
        /// Initializes a new instance of the CSVReader class.
        /// </summary>
        /// <param name="filePath">File path of file to be read as csv</param>
        /// <param name="isFirstLineContainsHeaders">indicates if first line of file containes header or not</param>
        /// <param name="delimiter">seperator used in the file to be read. by default its ","</param>
        /// <param name="hasFieldsEnclosedInQuotes">Indicates if field values are enclosed in quote or not</param>
        /// <param name="trimWhiteSpace">indicates if whitespace should be trimmed while reading the field values</param>
        public CSVReader(string filePath, bool isFirstLineContainsHeaders = true, string delimiter = ",", bool hasFieldsEnclosedInQuotes = false, bool trimWhiteSpace = false)
        {
            this.FilePath = filePath;
            this.IsFirstLineContainsHeaders = isFirstLineContainsHeaders;
            this.Delimiter = delimiter;
            this.HasFieldsEnclosedInQuotes = hasFieldsEnclosedInQuotes;
            this.TrimWhiteSpace = trimWhiteSpace;
            this.HasBeenRead = false;
            this.Records = new List<List<string>>();
            this.Headers = new List<string>();
        }

        # endregion

        # region Properties

        /// <summary>
        /// Gets a value of the Headers
        /// </summary>
        public List<string> Headers { get; private set; }

        /// <summary>
        /// Gets a value of the Records
        /// </summary>
        public List<List<string>> Records { get; private set; }

        /// <summary>
        /// Gets or sets a value of the File Path
        /// </summary>
        private string FilePath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the first line contains header or not
        /// </summary>
        private bool IsFirstLineContainsHeaders { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field has enclosed in quotes or not
        /// </summary>
        private bool HasFieldsEnclosedInQuotes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether white space will trim or not
        /// </summary>
        private bool TrimWhiteSpace { get; set; }

        /// <summary>
        /// Gets or sets a value of the Delimiter
        /// </summary>
        private string Delimiter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the File has been read or not
        /// </summary>
        private bool HasBeenRead { get; set; }

        #endregion

        # region Indexers

        /// <summary>
        /// Define an indexer
        /// </summary>
        /// <param name="recordIndex">Enter index of Record</param>
        /// <returns>returns records for given record index</returns>
        public List<string> this[int recordIndex]
        {
            get
            {
                if (!this.HasBeenRead)
                    this.Read();

                if (recordIndex > (this.Records.Count - 1))
                    throw new IndexOutOfRangeException(string.Format("There is no record at index {0}.", recordIndex));

                return this.Records[recordIndex];
            }
        }

        /// <summary>
        /// Define an indexer
        /// </summary>
        /// <param name="recordIndex">Enter index of record</param>
        /// <param name="fieldIndex">Enter index of Field</param>
        /// <returns>returns records from specific record index and field index</returns>
        public string this[int recordIndex, int fieldIndex]
        {
            get
            {
                if (!this.HasBeenRead)
                    this.Read();

                if (recordIndex > (this.Records.Count - 1))
                    throw new IndexOutOfRangeException(string.Format("There is no record at index {0}.", recordIndex));

                List<string> record = this.Records[recordIndex];
                if (fieldIndex > (this.Headers.Count - 1))
                    throw new IndexOutOfRangeException(string.Format("There is no field at index {0} in record {1}.", fieldIndex, recordIndex));

                return record[fieldIndex];
            }
        }

        /// <summary>
        /// Define an indexer
        /// </summary>
        /// <param name="recordIndex">Enter index of Record</param>
        /// <param name="fieldName">Enter index of Field</param>
        /// <returns>returns record from specific record index and field index when field index is equal to number of headers</returns>
        public string this[int recordIndex, string fieldName]
        {
            get
            {
                if (!this.HasBeenRead)
                    this.Read();

                if (recordIndex > (this.Records.Count - 1))
                    throw new IndexOutOfRangeException(string.Format("There is no record at index {0}.", recordIndex));

                List<string> record = this.Records[recordIndex];

                int fieldIndex = -1;

                for (int i = 0; i < this.Headers.Count; i++)
                {
                    if (string.Compare(this.Headers[i], fieldName, true) != 0)
                        continue;

                    fieldIndex = i;
                    break;
                }

                if (fieldIndex == -1)
                    throw new ArgumentException(string.Format("There is no field header with the name '{0}'", fieldName));

                if (fieldIndex > (this.Headers.Count - 1))
                    throw new IndexOutOfRangeException(string.Format("There is no field at index {0} in record {1}.", fieldIndex, recordIndex));

                return record[fieldIndex];
            }
        }

        /// <summary>
        /// Define an indexer
        /// </summary>
        /// <param name="fieldIndex">Enter index of Field</param>
        /// <param name="ignore">indicates that given index of file is ignored or not</param>
        /// <returns>returns records with specific field index or without field index</returns>
        public List<string> this[int fieldIndex, bool ignore]
        {
            get
            {
                if (!this.HasBeenRead)
                    this.Read();

                if (fieldIndex > (this.Records[0].Count - 1))
                    throw new IndexOutOfRangeException(string.Format("There is no field at index {0}.", fieldIndex));

                List<string> result = new List<string>();

                for (int i = 0; i <= this.Records.Count - 1; i++)
                    result.Add(this.Records[i][fieldIndex]);

                return result;
            }
        }

        /// <summary>
        /// Define an indexer
        /// </summary>
        /// <param name="fieldName">Enter Field Name</param>
        /// <returns>returns index of specific field name</returns>
        public List<string> this[string fieldName]
        {
            get
            {
                if (!this.HasBeenRead)
                    this.Read();

                if (!this.Headers.Contains(fieldName))
                    throw new ArgumentException(string.Format("There is no field header with the name '{0}'", fieldName));

                return this[this.Headers.IndexOf(fieldName), true];
            }
        }

        #endregion

        # region Public Methods

        /// <summary>
        /// This method is for getting specific record from record index
        /// </summary>
        /// <typeparam name="T">Enter Entity from which you want to get record</typeparam>
        /// <param name="recordIndex">Enter index of record</param>
        /// <returns>returns Entity object</returns>
        public T GetRecord<T>(int recordIndex) where T : new()
        {
            if (!this.HasBeenRead)
                this.Read();

            if (recordIndex > (this.Records.Count - 1))
                throw new IndexOutOfRangeException(string.Format("There is no record at index {0}.", recordIndex));

            T obj = new T();

            BindingFlags publicAttributes = BindingFlags.Public | BindingFlags.Instance;
            foreach (PropertyInfo property in
                     obj.GetType().GetProperties(publicAttributes))
            {
                if (this.Headers.Contains(property.Name, new IgnoreCaseContainsComparer()))
                    obj.SetPropertyValue(property.Name, this[recordIndex, property.Name]);
            }

            return obj;
        }

        /// <summary>
        /// This method is for getting all records
        /// </summary>
        /// <typeparam name="T">Enter Entity from which you want to get all records</typeparam>
        /// <returns>returns list of records</returns>
        public List<T> GetAllRecords<T>() where T : new()
        {
            if (!this.HasBeenRead)
                this.Read();

            List<T> records = new List<T>();
            T obj;
            BindingFlags publicAttributes = BindingFlags.Public | BindingFlags.Instance;
            for (int i = 0; i <= this.Records.Count - 1; i++)
            {
                obj = new T();
                foreach (PropertyInfo property in
                     obj.GetType().GetProperties(publicAttributes))
                {
                    if (this.Headers.Contains(property.Name, new IgnoreCaseContainsComparer()))
                        obj.SetPropertyValue(property.Name, this[i, property.Name]);
                }

                records.Add(obj);
            }

            return records;
        }

        #endregion

        # region Private Methods

        /// <summary>
        /// This method is for read File
        /// </summary>
        private void Read()
        {
            //// don't read file again it is already been read
            if (this.HasBeenRead)
                return;

            using (FileStream reader = File.OpenRead(this.FilePath))
            {
                using (TextFieldParser parser = new TextFieldParser(reader, Encoding.UTF8))
                {
                    bool isHeaderAdded = false;
                    parser.TrimWhiteSpace = this.TrimWhiteSpace;
                    parser.Delimiters = new[] { this.Delimiter };
                    parser.HasFieldsEnclosedInQuotes = this.HasFieldsEnclosedInQuotes;
                    while (!parser.EndOfData)
                    {
                        if (this.IsFirstLineContainsHeaders && !isHeaderAdded)
                        {
                            this.Headers = parser.ReadFields().ToList<string>();
                            isHeaderAdded = true;
                        }
                        else
                        {
                            this.Records.Add(parser.ReadFields().ToList<string>());
                        }
                    }
                }
            }

            this.HasBeenRead = true;
        }

        #endregion
    }
}
