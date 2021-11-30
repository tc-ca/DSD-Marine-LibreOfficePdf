using ConverterService.WebApi.Queries;
using ConverterService.Sessions;
using MediatR;

namespace ConverterService.WebApi.Handlers
{
    public class GetConversionStatusHandler : IRequestHandler<GetConversionStatusQuery, ConversionStatus>
    {
        private readonly ISessionRepository _sessionRepository;
        private readonly IResultRepository _resultRepository;

        public GetConversionStatusHandler(
            ISessionRepository sessionRepository,
            IResultRepository resultRepository)
        {
            _sessionRepository = sessionRepository;
            _resultRepository = resultRepository;
        }

        public async Task<ConversionStatus> Handle(GetConversionStatusQuery request, CancellationToken cancellationToken)
        {
            ConversionSession session = _sessionRepository.GetSession(request.SessionId); 
            ConversionStatus status;
            
            if (session == null)
            {
                status = new ConversionStatus(
                    Guid.Empty, 
                    SessionStates.Undefined,
                    null,
                    false); 
            }
            else if(session.State == SessionStates.ConversionSucceeded)
            {
                IEnumerable<string> urls = _resultRepository.GetDownloadUrls(session);
                status = new ConversionStatus(session.Id, session.State, urls, true);
            }
            else
            {
                status = new ConversionStatus(session.Id, session.State, null, true);
            }

            return await Task.FromResult(status);
        }
    }
}
