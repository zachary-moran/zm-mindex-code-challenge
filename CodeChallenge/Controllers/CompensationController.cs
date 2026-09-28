using System;
using CodeChallenge.Models;
using CodeChallenge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CodeChallenge.Controllers
{
    [ApiController]
    [Route("api/compensation")]
    public class CompensationController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ICompensationService _compensationService;

        public CompensationController(ILogger<CompensationController> logger, ICompensationService compensationService)
        {
            _logger = logger;
            _compensationService = compensationService;
        }

        [HttpPost]
        public IActionResult CreateCompensation([FromBody] Compensation compensation)
        {
            _logger.LogDebug($"Received compensation create request for employee '{compensation?.Employee?.EmployeeId}'");

            var created = _compensationService.Create(compensation);
            if (created == null)
                return BadRequest("A valid existing employee.employeeId is required.");

            return CreatedAtRoute("getCompensationByEmployeeId", new { id = created.Employee.EmployeeId }, created);
        }

        [HttpGet("{id}", Name = "getCompensationByEmployeeId")]
        public IActionResult GetByEmployeeId(String id)
        {
            _logger.LogDebug($"Received compensation get request for employee '{id}'");

            var compensation = _compensationService.GetByEmployeeId(id);
            if (compensation == null)
                return NotFound();

            return Ok(compensation);
        }

        [HttpGet("{id}/all")]
        public IActionResult GetAllByEmployeeId(String id)
        {
            _logger.LogDebug($"Received all-compensation get request for employee '{id}'");

            var compensations = _compensationService.GetAllByEmployeeId(id);
            if (compensations == null || compensations.Count == 0)
                return NotFound();

            return Ok(compensations);
        }
    }
}
