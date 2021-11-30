using ConverterService.Sessions;
using ConverterService.WebApi.Commands;
using ConverterService.WebApi.Files;
using MediatR;

namespace ConverterService.WebApi.Handlers
{
    public class BeginConversionHandler : IRequestHandler<BeginConversionCommand, BeginConversionResponse>
    {
        private readonly IFileUploader _uploader;
        private readonly ISessionRepository _sessionRepository;
        private readonly ILogger<BeginConversionHandler> _logger;

        public BeginConversionHandler(
            IFileUploader uploader, 
            ISessionRepository sessionRepository, 
            ILogger<BeginConversionHandler> logger)
        {
            _uploader = uploader;
            _sessionRepository = sessionRepository;
            _logger = logger;
        }

        public async Task<BeginConversionResponse> Handle(BeginConversionCommand command, CancellationToken cancellationToken)
        {
            BeginConversionResponse response = new();

            try
            {
                Guid sessionId = Guid.NewGuid();
                List<string> uploadedPaths = await _uploader.SaveToDisk(command.Request, sessionId);
                
                List<InputFile> documents = uploadedPaths.Select(path => new InputFile() { UploadedPath = path }).ToList();
                
                _sessionRepository.AddSession(
                    new ConversionSession() 
                    { 
                        Id = sessionId, 
                        InputFiles = documents, 
                        State = SessionStates.UploadSucceeded,
                        Operation = command.Operation
                    });

                response.IsAccepted = true;
                response.SessionId = sessionId;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to begin conversion");
                response.IsAccepted = false;
            }

            return response;
        }
    }
}
