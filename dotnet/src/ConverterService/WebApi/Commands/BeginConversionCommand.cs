using MediatR;
using ConverterService.Configuration;

namespace ConverterService.WebApi.Commands
{

    public record BeginConversionCommand(HttpRequest Request, Operations Operation) : IRequest<BeginConversionResponse>;
}
