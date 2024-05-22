using Asp.Versioning;
using FinanceManager.TransportLibrary.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Statistics.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public HealthModel GetHealth()
    {
        return new HealthModel
        {
            Status = "OK"
        };
    }
}