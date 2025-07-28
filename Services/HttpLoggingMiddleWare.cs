using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyStockAPIs.Services;

namespace MyStockAPIs.Services
{
    public class HttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpLoggingMiddleware> _logger;
        private readonly string _logFilePath;
        private readonly bool _useBeautifulFormat;
        private readonly bool _logToSeparateFiles;
        private readonly string _myTimeZone;

        public HttpLoggingMiddleware(RequestDelegate next, ILogger<HttpLoggingMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _logFilePath = configuration.GetValue<string>("CustomHttpLogging:FileName") ?? "logs/http-requests.log";
            _useBeautifulFormat = configuration.GetValue<bool>("CustomHttpLogging:HttpLogging:UseBeautifulFormat", true);
            _logToSeparateFiles = configuration.GetValue<bool>("CustomHttpLogging:HttpLogging:LogToSeparateFiles", false);
            //_myTimeZone = configuration.GetValue<string>("CustomHttpLogging:HttpLogging:TimeZone");
            _myTimeZone = configuration.GetSection("CustomHttpLogging:HttpLogging:TimeZone").Value
              ?? "E. Africa Standard Time";

            // Ensure directory exists// if not the app helps create
            var directory = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Log request
            var requestLog = await LogRequest(context.Request);

            // Capture response
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during request processing");
                throw;
            }

            // Log response
            stopwatch.Stop();
            var responseLog = await LogResponse(context.Response, stopwatch.ElapsedMilliseconds);

            // Write to file
            await WriteLogToFile(requestLog, responseLog, context.TraceIdentifier);

            // Copy response back
            await responseBody.CopyToAsync(originalBodyStream);
        }

        private async Task<RequestLog> LogRequest(HttpRequest request)
        {
            request.EnableBuffering();

            var body = string.Empty;
            if (request.ContentLength > 0 && request.ContentLength < 10000) // Limit body size
            {
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }
            var context = request.HttpContext;

            return new RequestLog
            {
                Method = request.Method,
                Path = request.Path,
                QueryString = request.QueryString.ToString(),
                Headers = request.Headers
                    .Where(h => !h.Key.ToLower().Contains("authorization")) // Exclude sensitive headers
                    .ToDictionary(h => h.Key, h => h.Value.ToString()),
                Body = body,
                Timestamp = DateTime.UtcNow,
                UserAgent = request.Headers["User-Agent"].FirstOrDefault(),
                RemoteIP = context.Connection.RemoteIpAddress?.ToString()
            };
        }

        private async Task<ResponseLog> LogResponse(HttpResponse response, long elapsedMs)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var body = string.Empty;

            if (response.Body.Length < 10000) // Limit response body size
            {
                body = await new StreamReader(response.Body).ReadToEndAsync();
            }

            response.Body.Seek(0, SeekOrigin.Begin);

            return new ResponseLog
            {
                StatusCode = response.StatusCode,
                Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                Body = body,
                ElapsedMilliseconds = elapsedMs,
                ContentType = response.ContentType
            };
        }

        private async Task WriteLogToFile(RequestLog request, ResponseLog response, string traceId)
        {
            string logContent;

            if (_useBeautifulFormat)
            {
                logContent = FormatBeautifulLog(request, response, traceId);
            }
            else
            {
                // Original JSON format
                var logEntry = new
                {
                    TraceId = traceId,
                    request.Timestamp,
                    Request = request,
                    Response = response
                };

                logContent = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }) + "," + Environment.NewLine;
            }

            try
            {
                if (_logToSeparateFiles)
                {
                    var fileName = $"http-request-{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss-fff}-{traceId}.log";
                    var filePath = Path.Combine(Path.GetDirectoryName(_logFilePath), fileName);
                    await File.WriteAllTextAsync(filePath, logContent);
                }
                else
                {
                    await File.AppendAllTextAsync(_logFilePath, logContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write HTTP log to file");
            }
        }

        private string FormatBeautifulLog(RequestLog request, ResponseLog response, string traceId)
        {
            var sb = new StringBuilder();
            var separator = new string('=', 80);
            var subSeparator = new string('-', 40);
            // Convert UTC to East Africa Time (EAT)
            //var eatTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time");
            //var eatTime = TimeZoneInfo.ConvertTimeFromUtc(request.Timestamp, eatTimeZone);
            ////var timeZoneId= _myTimeZone;
            ////var eatTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var timeZoneId = _myTimeZone;
            var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var localTimestamp = TimeZoneInfo.ConvertTimeFromUtc(request.Timestamp, targetTimeZone);

            // Header with timestamp and trace ID
            sb.AppendLine(separator);
            sb.AppendLine($"🌐 HTTP REQUEST/RESPONSE LOG WITH LOCAL TIME");
            //sb.AppendLine($"📅 Timestamp: {request.Timestamp:yyyy-MM-dd HH:mm:ss.fff} UTC");
            sb.AppendLine($"📅 Timestamp: {localTimestamp:yyyy-MM-dd HH:mm:ss.fff} ({targetTimeZone.DisplayName})");
            sb.AppendLine($"🔍 Trace ID: {traceId}");
            sb.AppendLine($"⏱️  Duration: {response.ElapsedMilliseconds}ms");
            sb.AppendLine(separator);

            // REQUEST SECTION
            sb.AppendLine();
            sb.AppendLine("📤 REQUEST");
            sb.AppendLine(subSeparator);
            sb.AppendLine($"Method: {request.Method}");
            sb.AppendLine($"Path: {request.Path}");

            if (!string.IsNullOrEmpty(request.QueryString))
            {
                sb.AppendLine($"Query String: {request.QueryString}");
            }

            if (!string.IsNullOrEmpty(request.RemoteIP))
            {
                sb.AppendLine($"Remote IP: {request.RemoteIP}");
            }

            if (!string.IsNullOrEmpty(request.UserAgent))
            {
                sb.AppendLine($"User Agent: {request.UserAgent}");
            }

            // Request Headers
            if (request.Headers?.Any() == true)
            {
                sb.AppendLine();
                sb.AppendLine("📋 Request Headers:");
                foreach (var header in request.Headers.OrderBy(h => h.Key))
                {
                    sb.AppendLine($"  {header.Key}: {header.Value}");
                }
            }

            // Request Body
            if (!string.IsNullOrEmpty(request.Body))
            {
                sb.AppendLine();
                sb.AppendLine("📝 Request Body:");
                sb.AppendLine(FormatJsonIfPossible(request.Body));
            }

            // RESPONSE SECTION
            sb.AppendLine();
            sb.AppendLine("📥 RESPONSE");
            sb.AppendLine(subSeparator);
            sb.AppendLine($"Status Code: {response.StatusCode} {GetStatusCodeDescription(response.StatusCode)}");

            if (!string.IsNullOrEmpty(response.ContentType))
            {
                sb.AppendLine($"Content Type: {response.ContentType}");
            }

            // Response Headers
            if (response.Headers?.Any() == true)
            {
                sb.AppendLine();
                sb.AppendLine("📋 Response Headers:");
                foreach (var header in response.Headers.OrderBy(h => h.Key))
                {
                    sb.AppendLine($"  {header.Key}: {header.Value}");
                }
            }

            // Response Body
            if (!string.IsNullOrEmpty(response.Body))
            {
                sb.AppendLine();
                sb.AppendLine("📄 Response Body:");
                sb.AppendLine(FormatJsonIfPossible(response.Body));
            }

            // Footer
            sb.AppendLine();
            sb.AppendLine(separator);
            sb.AppendLine();

            return sb.ToString();
        }

        private string FormatJsonIfPossible(string content)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(content);
                return JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch
            {
                // Not JSON, return as-is with indentation
                return content;
            }
        }

        private string GetStatusCodeDescription(int statusCode)
        {
            return statusCode switch
            {
                200 => "OK",
                201 => "Created",
                204 => "No Content",
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                405 => "Method Not Allowed",
                500 => "Internal Server Error",
                502 => "Bad Gateway",
                503 => "Service Unavailable",
                _ => ""
            };
        }
    }

    public class RequestLog
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserAgent { get; set; }
        public string RemoteIP { get; set; }
    }

    public class ResponseLog
    {
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public string ContentType { get; set; }
    }
}



////Instructions on using this file:
//(Custom Middleware) is completely reusable across any .NET Core/.NET system. Here's what you need to do:
//Steps to Reuse the Middleware:
//1.Create the middleware file - You can put it anywhere, but common locations are:
//o Middleware/HttpLoggingMiddleware.cs
//o	Services/HttpLoggingMiddleware.cs
//o
//2.	Add it to your pipeline - Just one line in Startup.cs or Program.cs:
//3.app.UseMiddleware<HttpLoggingMiddleware>(); // add just after //main builder// which is var app = builder.Build();
/////app.UseMiddleware<HttpLoggingMiddleware>(); // Add this line
//// ... rest of pipeline
//4.Add the follwing to appsettings.json if you want to customize it
//////"CustomHttpLogging": {
//////    "FileName":"HttpLogsFile/http-RequestsAndResponses.json", // this will create a folder named HttpLogs in the same directory as the application": null,
//////    "HttpLogging": {
//////        "UseBeautifulFormat": true,
//////      "LogToSeparateFiles": false,
//////      "TimeZone": "E. Africa Standard Time"
//////    }
//////},
//

//Startup.cs(.NET Core 3.1 /.NET 5):
//public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//{
//    // ... other middleware
//    app.UseMiddleware<HttpLoggingMiddleware>(); // Add this line
//    // ... rest of pipeline
//}

// TIPPPPP:
//🔁 Optional: If you Want to log only application-specific requests (skip Swagger, static files)?
//Move it after UseStaticFiles() and UseSwaggerUI() like this:

//app.UseHttpsRedirection();
//app.UseStaticFiles();
//app.UseSwagger();
//app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OfficeAPI v1"));

//// Now log only meaningful application traffic
//app.UseMiddleware<HttpLoggingMiddleware>();
// TIPPPPPPPPP
//Key Benefits of This Approach:
//•	Zero dependencies - Uses only built-in .NET libraries
//•	Copy & paste ready - Just copy the middleware file to any project
//•	Configurable - Reads from appsettings.json automatically
//•	No service registration required - Middleware is resolved automatically
//•	Works with any .NET Core version - Compatible with 3.1, 5, 6, 7, 8+

