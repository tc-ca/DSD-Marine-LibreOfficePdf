using Microsoft.AspNetCore.Mvc;

namespace ConverterService.WebApi.Files
{
    /// <summary>
    /// Reprsents an <see cref="ActionResult"/> that when executed will write
    /// file from a stream to response and invoke a callback once completed.
    /// </summary>
    public class FileStreamObservableResult : FileStreamResult
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FileStreamObservableResult"/>.
        /// </summary>
        /// <param name="fileStream">In instance of <see cref="Stream"/>.</param>
        /// <param name="contentType"><see cref="string"/> representing a content type.</param>
        /// <param name="completionCallback">A delegate to invoke once action execution completes.</param>
        public FileStreamObservableResult(
            Stream fileStream, 
            string contentType,
            Action completionCallback) 
            : base(fileStream, contentType)
        {
            ArgumentNullException.ThrowIfNull(completionCallback, nameof(completionCallback));
            CompletionCallback = completionCallback;
        }

        /// <summary>
        /// A callback to invoke once execution of the action completes.
        /// </summary>
        public Action CompletionCallback { get; }

        /// <inheritdoc/>
        public override async Task ExecuteResultAsync(ActionContext context)
        {
            await base.ExecuteResultAsync(context).ConfigureAwait(false);
            CompletionCallback();
        }
    }
}
