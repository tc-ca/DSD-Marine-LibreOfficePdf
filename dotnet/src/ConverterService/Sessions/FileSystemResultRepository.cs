using ConverterService.Configuration;
using System.Net;

namespace ConverterService.Sessions
{
    public class FileSystemResultRepository : IResultRepository
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly LinkGenerator _linkGenerator;
        private readonly ILogger<FileSystemResultRepository> _logger;

        public FileSystemResultRepository(
            IHttpContextAccessor contextAccessor, 
            LinkGenerator linkGenerator, 
            ILogger<FileSystemResultRepository> logger)
        {
            ArgumentNullException.ThrowIfNull(contextAccessor, nameof(contextAccessor));
            ArgumentNullException.ThrowIfNull(linkGenerator, nameof(linkGenerator));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            _contextAccessor = contextAccessor;
            _linkGenerator = linkGenerator; 
            _logger = logger;
        }

        public Task<bool> CollectConvertedFiles(ConversionSession session)
        {
            ArgumentNullException.ThrowIfNull(session, nameof(session));

            if(session.State != SessionStates.ConversionSucceeded)
            {
                throw new InvalidOperationException(
                    $"Converted files cannot be collected when session state is {session.State}");
            }

            // The files are already on the local container file system.
            return Task.FromResult(true);
        }

        public IEnumerable<string> GetDownloadUrls(ConversionSession session)
        {
            IEnumerable<string> urls = session.OutputFiles.Select(metadata =>
            {
                if (string.IsNullOrEmpty(metadata.ConvertedPath))
                {
                    throw new ApplicationException(
                        $"Missing converted file paths for completed conversion session {session.Id}");
                }

                string fileName = Path.GetFileName(metadata.ConvertedPath);
                string encodedFileName = WebUtility.UrlEncode(fileName);
                string url = _linkGenerator.GetUriByName(
                    Constants.DownloadFileRouteName,
                    new { sessionId = session.Id, fileName = encodedFileName },
                    _contextAccessor.HttpContext!.Request.Scheme,
                    _contextAccessor.HttpContext!.Request.Host!)!;

                return url;
            });

            return urls;
        }

        public Stream? GetConvertedFile(ConversionSession session, string fileName)
        {
            ArgumentNullException.ThrowIfNull(session, nameof (session)); 
            ArgumentNullException.ThrowIfNull(fileName, nameof (fileName));

            OutputFile? file = SessionHelper.FindByFileName(session, fileName);

            if (file == null 
                || string.IsNullOrEmpty(file.ConvertedPath) 
                || !File.Exists(file.ConvertedPath))
            {
                _logger.LogWarning(
                    "Could not find output file named {FileName} for session {SessionId}", 
                    fileName, 
                    session.Id);
                return null;
            }

            return File.OpenRead(file.ConvertedPath);
        }
    }
}
