using FinanceManager.TransportLibrary.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.File.Controllers;

[ApiController]
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