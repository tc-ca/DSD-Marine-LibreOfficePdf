using ConverterService.Configuration;

namespace ConverterService.Sessions;

/// <summary>
/// Contains helper methods for working with files and folders.
/// </summary>
public static class FileSystemHelper
{
    /// <summary>
    /// Constructs a path to the destination directory and ensures that it exists.
    /// </summary>
    /// <param name="baseFolderName">Name of one of application folders residing 
    /// under <see cref="Constants.FileSystemRootFolderName"/> as defined in 
    /// application <see cref="Constants"/>.</param>
    /// <param name="sessionId">Unique identifier of a conversion session.</param>
    /// <returns>Absolute path to the destination directory.</returns>
    public static string EnsureDirectoryPath(string baseFolderName, Guid sessionId)
    {
        string commonFolderPath = Environment.GetFolderPath(
            Environment.SpecialFolder.CommonApplicationData,
            Environment.SpecialFolderOption.Create);

        string rootFolderPath = Path.Combine(commonFolderPath, Constants.FileSystemRootFolderName);

        if (!Directory.Exists(rootFolderPath))
        {
            Directory.CreateDirectory(rootFolderPath);
        }

        string baseFolderPath = Path.Combine(rootFolderPath, baseFolderName);

        if (!Directory.Exists(baseFolderPath))
        {
            Directory.CreateDirectory(baseFolderPath);
        }

        string sessionPath = Path.Combine(baseFolderPath, sessionId.ToString());

        if (!Directory.Exists(sessionPath))
        {
            Directory.CreateDirectory(sessionPath);
        }

        return sessionPath;
    }
}
