using ConverterService.Configuration;
using ConverterService.WebApi.Queries;
using ConverterService.Sessions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ConverterService.WebApi.Commands;
using ConverterService.WebApi.Files;

namespace ConverterService.WebApi.Controllers
{
    /// <summary>
    /// A facade for invoking PDF conversion operations. 
    /// </summary>
    [ApiController]
    [Route("/pdf")]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class PdfController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PdfController> _logger;

        /// <summary>
        /// Create a new instance of <see cref="PdfController"/>.
        /// </summary>
        /// <param name="mediator">An instance of <see cref="IMediator"/>.</param>
        /// <param name="logger"><see cref="ILogger{T}"/> implementation.</param>
        public PdfController(IMediator mediator, ILogger<PdfController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Uploads multiple documents and begins merging them into a single PDF file.
        /// </summary>
        /// <returns>A unique identifier of the conversion session.</returns>
        [HttpPost]
        [Route("merge")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> BeginMerging()
        {
            BeginConversionResponse result = await
                _mediator.Send(new BeginConversionCommand(Request, Operations.MergeDocuments));

            if (!result.IsAccepted)
            {
                return BadRequest();
            }

            return AcceptedAtRoute(
                nameof(GetStatus),
                new { sessionId = result.SessionId });
        }

        /// <summary>
        /// Uploads multiple documents and begins converting each one to PDF.
        /// </summary>
        /// <returns>A unique identifier of the conversion session.</returns>
        [HttpPost]
        [Route("convert")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> BeginPdfConversion()
        {
            BeginConversionResponse result = await 
                _mediator.Send(new BeginConversionCommand(Request, Operations.ConvertToPdf));

            if (!result.IsAccepted)
            {
                return BadRequest();
            }

            return AcceptedAtRoute(
                nameof(GetStatus), 
                new { sessionId = result.SessionId});
        }

        /// <summary>
        /// Uploads a PDF form along with FDF document containing field values and begins filling out the form.
        /// </summary>
        /// <returns>A unique identifier of the conversion session.</returns>
        [HttpPost]
        [Route("fill")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> BeginFillingForm()
        {
            BeginConversionResponse result = await
                _mediator.Send(new BeginConversionCommand(Request, Operations.FillOutPdfForm));

            if (!result.IsAccepted)
            {
                return BadRequest();
            }

            return AcceptedAtRoute(
                nameof(GetStatus),
                new { sessionId = result.SessionId });
        }

        /// <summary>
        /// Uploads a fillable PDF form and begins generation of an FDF template based on its fillable fields.
        /// </summary>
        /// <returns>A unique identifier of the conversion session.</returns>
        [HttpPost]
        [Route("generatefdf")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> BeginFdfGeneration()
        {
            BeginConversionResponse result = await
                _mediator.Send(new BeginConversionCommand(Request, Operations.GenerateFdfDocument));

            if (!result.IsAccepted)
            {
                return BadRequest();
            }

            return AcceptedAtRoute(
                nameof(GetStatus),
                new { sessionId = result.SessionId });
        }

        /// <summary>
        /// Gets a status of a conversion operation.
        /// </summary>
        /// <param name="sessionId">Unique identifier of a conversion session.</param>
        /// <returns>A list of URLs to download results from if conversion has completed, 
        /// or a 202 response with location header pointing at the same route 
        /// if conversion is still in progress.</returns>
        [HttpGet]
        [Route("{sessionId}/status", Name = nameof(GetStatus))]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStatus(Guid sessionId) 
        {
            ConversionStatus result = await _mediator.Send(new GetConversionStatusQuery(sessionId));

            if(!result.Found)
            {
                return NotFound(sessionId);
            }

            if(result.State == SessionStates.ConversionFailed)
            {
                _logger.LogError("Conversion has failed for session {SessionId}", sessionId);
                return BadRequest(sessionId);
            }

            if(result.State == SessionStates.ConversionSucceeded)
            {
                _logger.LogInformation("Conversion has succeeded for session {SessionId}", sessionId);
                return Ok(result.DownloadUrls);
            }

            _logger.LogInformation(
                "Conversion is in progress for session {SessionId}. Status: {State}", 
                sessionId, 
                result.State);
            return AcceptedAtRoute(
                nameof(GetStatus),
                new { sessionId = result.SessionId });
        }

        /// <summary>
        /// Gets the contents of a PDF or FDF document produced by a conversion operation.
        /// </summary>
        /// <param name="sessionId">Unique identifier of a conversion session.</param>
        /// <param name="fileName">Name of file to download.</param>
        /// <returns>A <see cref="FileResult"/> allowind to stream file content to the client.</returns>
        [HttpGet]
        [Route("{sessionId}/{fileName}", Name = Constants.DownloadFileRouteName)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<FileResult> GetConvertedFile(Guid sessionId, string fileName)
        {
            GetConvertedFileResponse response = await _mediator.Send(new GetConvertedFileQuery(sessionId, fileName));
            
            if(response.FileStream == null)
            {
                NotFound($"File {fileName} not found for session {sessionId}");
            }

            async void callback()
            {
                await _mediator.Send(new FileDownloadedNotification(sessionId, fileName));
            }

            var result = new FileStreamObservableResult(response.FileStream!, response.ContentType, callback);
            return result;
        }
    }
}
