using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace RPA.MIT.Notification;

public static class HealthCheck
{
    [FunctionName("healthy")]
    public static async Task<IActionResult> Healthy(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "healthy")] HttpRequest req,
        ILogger log)

    {
        log.LogInformation("Healthy check.");
        return new OkObjectResult("Healthy");
    }

    [FunctionName("healthz")]
    public static async Task<IActionResult> Healthz(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "healthz")] HttpRequest req,
        ILogger log)

    {
        log.LogInformation("Healthy check.");
        return new OkObjectResult("healthz");
    }
}
