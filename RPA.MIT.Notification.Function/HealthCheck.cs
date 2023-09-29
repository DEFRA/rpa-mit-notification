using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace RPA.MIT.Notification;

public class HealthCheck
{
    private readonly ILogger _logger;

    public HealthCheck(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<HealthCheck>();
    }

    [Function("healthy")]
    public HttpResponseData RunHealthy(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Healthy check.");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        return response;
    }

    [Function("healthz")]
    public HttpResponseData RunHealthz(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Healthz check");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        return response;
    }
}
