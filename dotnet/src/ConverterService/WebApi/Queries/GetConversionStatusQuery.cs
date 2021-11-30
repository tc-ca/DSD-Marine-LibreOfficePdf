using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConverterService.WebApi.Queries
{
    public record GetConversionStatusQuery(Guid SessionId) : IRequest<ConversionStatus>;
}
