// ValidationMiddleware.cs
using System.Text.Json;

namespace TodoApi.Middleware
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/api/todo") &&
                (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put))
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: System.Text.Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);

                var bodyStr = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (!string.IsNullOrWhiteSpace(bodyStr))
                {
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(bodyStr);
                        var root = jsonDoc.RootElement;

                        if (!root.TryGetProperty("title", out var titleProp) ||
                            string.IsNullOrWhiteSpace(titleProp.GetString()))
                        {
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            await context.Response.WriteAsync("Title is required.");
                            return;
                        }

                        if (!root.TryGetProperty("description", out var descProp) ||
                            string.IsNullOrWhiteSpace(descProp.GetString()))
                        {
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                            await context.Response.WriteAsync("Description is required.");
                            return;
                        }
                    }
                    catch (JsonException)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync("Invalid JSON.");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
