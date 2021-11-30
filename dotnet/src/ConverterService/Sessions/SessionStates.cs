namespace ConverterService.Sessions
{
    /// <summary>
    /// Contains a list of supported states a conversion session can have.
    /// </summary>
    public enum SessionStates
    {
        /// <summary>
        /// Default value indicating uninitialized state.
        /// </summary>
        Undefined,

        /// <summary>
        /// Uploading of all files has succeeded, conversion can start.
        /// </summary>
        UploadSucceeded,

        /// <summary>
        /// Conversion is in progress for one or more files within the session.
        /// </summary>
        Converting,

        /// <summary>
        /// Conversion has failed.
        /// </summary>
        ConversionFailed,

        /// <summary>
        /// Conversion has succeeded and its results are available for downloading.
        /// </summary>
        ConversionSucceeded,

        /// <summary>
        /// All files in conversion session have been downloaded and so their cleanup can start.
        /// </summary>
        ResultsDownloaded
    }
}
