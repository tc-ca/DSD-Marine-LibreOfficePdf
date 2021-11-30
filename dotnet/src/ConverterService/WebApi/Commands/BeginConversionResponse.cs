namespace ConverterService.WebApi.Commands
{
    public record BeginConversionResponse
    {
        public bool IsAccepted { get; set; }
        public Guid SessionId { get; set; } = default!;
    }
}
