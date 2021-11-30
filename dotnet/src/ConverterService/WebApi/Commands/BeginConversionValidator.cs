using ConverterService.WebApi.Files;
using FluentValidation;

namespace ConverterService.WebApi.Commands
{
    public class BeginConversionValidator : AbstractValidator<BeginConversionCommand>
    {
        public BeginConversionValidator()
            : base()
        {
            RuleFor(c => c.Request)
                .Must(r => MultipartRequestHelper.IsMultipartContentType(r.ContentType!))
                .WithMessage("The request is not of a multipart content type");
        }
    }
}
