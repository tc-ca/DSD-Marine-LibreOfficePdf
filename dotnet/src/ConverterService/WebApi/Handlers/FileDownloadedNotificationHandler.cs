using ConverterService.Sessions;
using ConverterService.WebApi.Commands;
using MediatR;

namespace ConverterService.WebApi.Handlers
{
    public class FileDownloadedNotificationHandler : IRequestHandler<FileDownloadedNotification>
    {
        private readonly ISessionRepository _sessionRepository;

        public FileDownloadedNotificationHandler(ISessionRepository repository)
        {
            _sessionRepository = repository;
        }

        public async Task<Unit> Handle(FileDownloadedNotification request, CancellationToken cancellationToken)
        {
            ConversionSession session = _sessionRepository.GetSession(request.SessionId);

            if (session == null)
            {
                return await Task.FromResult(Unit.Value);
            }

            OutputFile? file = SessionHelper.FindByFileName(session, request.FileName);

            if (file == null)
            {
                return await Task.FromResult(Unit.Value);
            }

            file.IsDownloaded = true;
            bool hasNotDownloadedFiles = session.OutputFiles.Any(metadata => metadata.IsDownloaded == false);

            if (!hasNotDownloadedFiles)
            {
                session.State = SessionStates.ResultsDownloaded;
            }

            _sessionRepository.SetSession(session);
            return Unit.Value;
        }
    }
}
