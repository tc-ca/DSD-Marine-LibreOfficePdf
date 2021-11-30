using ConverterService.Configuration;
using ConverterService.WebApi.Queries;
using ConverterService.Sessions;
using MediatR;

namespace ConverterService.WebApi.Handlers
{
    public class GetConvertedFileHandler : IRequestHandler<GetConvertedFileQuery, GetConvertedFileResponse>
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IResultRepository _resultRepository;

        public GetConvertedFileHandler(ISessionRepository sessionRepository, IResultRepository resultRepository)
        {
            _sessionRepository = sessionRepository;
            _resultRepository = resultRepository;
        }

        public async Task<GetConvertedFileResponse> Handle(GetConvertedFileQuery request, CancellationToken cancellationToken)
        {
            ConversionSession session = _sessionRepository.GetSession(request.SessionId);
            
            if (session == null)
            {
                return await Task.FromResult(new GetConvertedFileResponse(null!, null!));
            }

            string contentType = GetContentType(request.FileName);
            Stream? stream = _resultRepository.GetConvertedFile(session, request.FileName);

            return await Task.FromResult(new GetConvertedFileResponse(stream, contentType));
        }

        private static string GetContentType(string filePath)
        {
            string extension = Path.GetExtension(filePath);

            if (string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return Constants.ContentTypePdf;
            }
            
            if (string.Equals(extension, ".fdf", StringComparison.OrdinalIgnoreCase))
            {
                return Constants.ContentTypeFdf;
            }
            
            throw new NotSupportedException($"Downloading of files with extension '{extension}' is not supported.");
        }
    }
}
