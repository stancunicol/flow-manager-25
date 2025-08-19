using FlowManager.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlowManager.Infrastructure.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch(EntryNotFoundException ex)
            {
                await HandleExpectionAsync(context, ex, HttpStatusCode.NotFound);
            }
            catch(UniqueConstraintViolationException ex)
            {
                await HandleExpectionAsync(context, ex, HttpStatusCode.Conflict);
            }
            catch(EmailNotSentException ex)
            {
                await HandleExpectionAsync(context, ex, HttpStatusCode.UnprocessableEntity);
            }
            catch (Exception ex)
            {
                await HandleExpectionAsync(context, ex, HttpStatusCode.InternalServerError);
            }
        }

        private static Task HandleExpectionAsync(HttpContext context, Exception expection, HttpStatusCode code)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = expection.Message,
                Result = new object(),
                Timestamp = DateTime.UtcNow
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(response, options);

            return context.Response.WriteAsync(json);
        }
    }
}
