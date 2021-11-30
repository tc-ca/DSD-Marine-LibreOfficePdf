using FluentValidation.Results;

namespace ConverterService.WebApi.Exceptions
{
    public class RequestValidationException : Exception
    {
        public RequestValidationException()
            : base("Request validation failed")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public RequestValidationException(IEnumerable<ValidationFailure> failures)
            : this()
        {
            Errors = failures
                .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
                .ToDictionary(g => g.Key, g => g.ToArray());
        }

        public Dictionary<string, string[]> Errors { get; }
    }
}
