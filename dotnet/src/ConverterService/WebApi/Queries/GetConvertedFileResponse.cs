namespace ConverterService.WebApi.Queries
{
    public record GetConvertedFileResponse(Stream? FileStream, string ContentType);
}
