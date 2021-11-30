using Microsoft.AspNetCore.Mvc;

namespace ConverterService.WebApi.Controllers
{
    [ApiController]
    [Route("/")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DefaultController : ControllerBase
    {
        private readonly bool _isDevelopmentEnvironment;

        /// <summary>
        /// Creates a new instance of the controller.
        /// </summary>
        /// <param name="env">An instance of <see cref="IHostEnvironment"/>.</param>
        public DefaultController(IHostEnvironment env)
        {
            _isDevelopmentEnvironment = env.IsDevelopment();
        }

        /// <summary>
        /// Returns "OK" string in production environment and redirects 
        /// to swagger endpoint in non-production environments.
        /// </summary>
        /// <returns>A 302 in non-production environments, a 200 in production environment.</returns>
        public async Task<IActionResult> Index()
        {
            if (_isDevelopmentEnvironment)
                return await Task.FromResult(new RedirectResult("~/swagger"));
            else
                return await Task.FromResult(Ok("OK"));
        }
    }
}
