using Microsoft.AspNetCore.Mvc;
using System;

namespace EnterpriseRag.Api.Controllers
{
    [ApiController]
    [Route("api/v1/health")]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var response = new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow
            };

            return Ok(response);
        }
    }
}
