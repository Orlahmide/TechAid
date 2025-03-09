using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TechAid.MiddleWare
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate request;
        private readonly ILogger<ExceptionMiddleware> _logger;
        public ExceptionMiddleware(RequestDelegate request, ILogger<ExceptionMiddleware> logger)
        {
            this.request = request;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await request.Invoke(httpContext);
            }
            
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                _logger.LogError("API Exception - {message} at \n {stacktrace}", ex.Message, ex.StackTrace);

                var response = string.IsNullOrEmpty(ex.Message) ? "An Error Occurred" : ex.Message;

                await httpContext.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
