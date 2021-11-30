using MediatR;

namespace ConverterService.WebApi.Commands
{
    public record FileDownloadedNotification(Guid SessionId, string FileName) : IRequest;
}
