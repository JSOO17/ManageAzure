using ManageAzure.Config;
using ManageAzure.Interfaces;
using ManageAzure.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManageAzure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly IAzureDevOpsRepository _azureDevOpsRepository;

        private readonly ILogger<TicketController> _logger;

        public TicketController(IAzureDevOpsRepository azureDevOpsRepository, ILogger<TicketController> logger)
        {
            _azureDevOpsRepository = azureDevOpsRepository;
            _logger = logger;
        }

        [HttpPost]
        [ValidateApiKey]
        public async Task<ActionResult<TicketResponse>> Post([FromBody] Ticket ticket)
        {
            try
            {
                return await _azureDevOpsRepository.CreateIssue(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Error Server");

                return StatusCode(500, "something went wrong");
            }

        }
    }
}
