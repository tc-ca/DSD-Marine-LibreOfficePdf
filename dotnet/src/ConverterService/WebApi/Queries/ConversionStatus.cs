using ConverterService.Sessions;

namespace ConverterService.WebApi.Queries
{
    public record ConversionStatus(Guid SessionId, SessionStates State, IEnumerable<string>? DownloadUrls, bool Found);

}
