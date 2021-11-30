using MediatR;

namespace ConverterService.WebApi.Queries
{
    public record GetConvertedFileQuery(Guid SessionId, string FileName) : IRequest<GetConvertedFileResponse>;
}
