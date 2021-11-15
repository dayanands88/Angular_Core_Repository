using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using API.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API.Middleware
{
    public class ExceptionMiddleWare
    {
        public readonly RequestDelegate _next;

        private readonly ILogger<ExceptionMiddleWare> _logger;
        private readonly IHostEnvironment _environment;
        public ExceptionMiddleWare(RequestDelegate next, ILogger<ExceptionMiddleWare> logger,
           IHostEnvironment environment)
        {
            _next = next;
            _environment = environment;
            _logger = logger;

        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType="application/json";
                context.Response.StatusCode =(int) HttpStatusCode.InternalServerError;
                var response = _environment.IsDevelopment()
                    ? new ApiException(context.Response.StatusCode,ex.Message,ex.StackTrace?.ToString())
                    : new ApiException(context.Response.StatusCode,"Internal server error");
                var options = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
                var json = JsonSerializer.Serialize(response,options);
                await context.Response.WriteAsync(json);
            }
        }
    
    }

    
}