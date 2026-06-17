namespace Common.Utility.IO
{
    /// <summary>
    /// create custom data type to save the file
    /// </summary>
    public enum FileSaveOption
    {
        /// <summary>
        /// use to rename the file
        /// </summary>
        RenameIfExist = 1,

        /// <summary>
        /// use to overwrite file
        /// </summary>
        OverWriteIfExist = 2,

        /// <summary>
        /// show error if file exist
        /// </summary>
        ErrorIfExist = 3,

        /// <summary>
        /// use GUID  for file name
        /// </summary>
        UseGUIDAsName = 4
    }
}
