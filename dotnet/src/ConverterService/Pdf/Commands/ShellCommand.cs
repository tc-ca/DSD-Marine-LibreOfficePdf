using ConverterService.Configuration;
using System.Diagnostics;

namespace ConverterService.Pdf.Commands
{
    /// <summary>
    /// Base type for executing shell commands.
    /// </summary>
    /// <typeparam name="TInput">Type of input data required to build command with parameters.</typeparam>
    public abstract class ShellCommand<TInput> : IDisposable
    {
        private readonly ILogger<ShellCommand<TInput>> _logger;
        private Process _process;
        private bool _disposed = false;

        /// <summary>
        /// Creates a new instance of <see cref="ShellCommand{T}"/>.
        /// </summary>
        /// <param name="options">Configuration options used during conversion. Options include linux shell command template.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public ShellCommand(ConversionOptions options, IServiceProvider serviceProvider)
        {
            ConversionOptions = options;
            ServiceProvider = serviceProvider;
            _logger = ServiceProvider.GetRequiredService<ILogger<ShellCommand<TInput>>>();
            _process = new Process();
        }

        /// <summary>
        /// Releases resources used by this instance.
        /// </summary>
        public void Dispose() 
        { 
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Configuration options used during conversion.
        /// </summary>
        protected ConversionOptions ConversionOptions { get; init; }

        /// <summary>
        /// Reference to <see cref="IServiceProvider"/> instance.
        /// </summary>
        protected IServiceProvider ServiceProvider { get; init; }

        /// <summary>
        /// Executes shell command.
        /// </summary>
        /// <param name="input">An object of type <see cref="TInput"/> 
        /// containing input data for building the command.</param>
        /// <param name="sessionId">Unique identifier of conversion session.</param>
        /// <returns>An instance of <see cref="CommandResult"/>.</returns>
        public async Task<CommandResult> Execute(TInput input, Guid sessionId)
        {
            CommandInfo commandInfo = PrepareCommand(input, sessionId);
            bool succeeded = await RunCommand(commandInfo);
            CommandResult result = PrepareResult(succeeded, input, sessionId);
            return result;
        }

        /// <summary>
        /// When implemented in derived types, converts the 
        /// input into an instance of <see cref="CommandInfo"/>.
        /// </summary>
        /// <param name="input">An instance of generic type <see cref="TInput"/> containing input data.</param>
        /// <param name="sessionId">Unique identifier of conversion session.</param>
        /// <returns>An instance of <see cref="CommandInfo"/>.</returns>
        protected abstract CommandInfo PrepareCommand(TInput input, Guid sessionId);

        /// <summary>
        /// When iplemented in derived types, prepares an instance of <see cref="CommandResult"/> to return.
        /// </summary>
        /// <remarks>Typically implementations would check if converted file exists at expected path.</remarks>
        /// <param name="succeeded">Boolean indicating wheher or not the shell command execution has succeeded.</param>
        /// <param name="input">Original input parameters.</param>
        /// <param name="sessionId">Unique session identifier.</param>
        /// <returns>An instance of <see cref="CommandResult"/>.</returns>
        protected abstract CommandResult PrepareResult(bool succeeded, TInput input, Guid sessionId);
        
        /// <summary>
        /// Implementation of disposable pattern.
        /// </summary>
        /// <param name="disposing">Boolean indicating whether 
        /// the method is invoked by an object's consumer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _process.Dispose();
                    _process = null!;
                }

                _disposed = true;
            }
        }
 
        /// <summary>
        /// Starts an operating system shell process using supplied command information.
        /// </summary>
        /// <param name="command">Command information, such as its path and arguments.</param>
        /// <returns>An instance of <see cref="CommandResult"/>.</returns>
        protected virtual async Task<bool> RunCommand(CommandInfo command)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command.Name,
                Arguments = command.Arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            _process.StartInfo = startInfo;
            _logger.LogInformation("Starting process: {CommandName} {Arguments}", command.Name, command.Arguments);

            try
            {
                if(!_process.Start())
                {
                    _logger.LogError("Failed to start the process");
                    return await Task.FromResult(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error has occurred while starting the process");
                return await Task.FromResult(false);
            }
            
            await _process.WaitForExitAsync();
            string output = await _process.StandardOutput.ReadToEndAsync();
            string error = await _process.StandardError.ReadToEndAsync();
            
            if(string.IsNullOrEmpty(error))
            {
                _logger.LogInformation("Process (id={ProcessId}) has completed. stdout: {Output}", _process.Id, output);
                return await Task.FromResult(true);
            }
            else
            {
                _logger.LogError(
                    "Process (id={ProcessId}) has completed with errors. stdout: {Output}. stderr: {Error}", 
                    _process.Id, 
                    output, 
                    error);
                return await Task.FromResult(false);
            }
        }

        /// <summary>
        /// Takes a shell command template and a list of well-formed arguments and generates a <see cref="CommandInfo"/> instance.
        /// </summary>
        /// <param name="template">Template as set in appsettings.json.</param>
        /// <param name="arguments">An array of arguments to match a template.</param>
        /// <returns>An instance of <see cref="CommandInfo"/> type.</returns>
        protected virtual CommandInfo CreateCommandInfo(string template, params object[] arguments)
        {
            int breakIdx = template.IndexOf(' ');
            string command = template[..breakIdx];
            string argumentsTemplate = template[(breakIdx + 1)..];
            string argumentsString = string.Format(argumentsTemplate, arguments);
            
            CommandInfo commandInfo = new()
            { 
                Name = command,
                Arguments = argumentsString
            };

            return commandInfo;
        }
    }
}
