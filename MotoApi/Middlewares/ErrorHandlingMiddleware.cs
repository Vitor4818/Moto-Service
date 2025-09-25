using System.Net;
using Microsoft.AspNetCore.Mvc;
using MotoBusiness.Exceptions;

namespace MotoApi.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            ProblemDetails problemDetails;

            switch (exception)
            {
                case NotFoundException:
                    problemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "Recurso não encontrado",
                        Detail = exception.Message,
                        Instance = context.Request.Path
                    };
                    break;

                case ValidationException ve:
                    problemDetails = new ValidationProblemDetails(ve.Errors)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Erro de validação",
                        Detail = ve.Message,
                        Instance = context.Request.Path
                    };
                    break;

                case BusinessException:
                    problemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status409Conflict,
                        Title = "Erro de negócio",
                        Detail = exception.Message,
                        Instance = context.Request.Path
                    };
                    break;

                default:
                    problemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Erro interno no servidor",
                        Detail = exception.Message,
                        Instance = context.Request.Path
                    };
                    break;
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = problemDetails.Status.Value;

            return context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
