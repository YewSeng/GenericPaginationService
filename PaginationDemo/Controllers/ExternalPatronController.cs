using Microsoft.AspNetCore.Mvc;
using PaginationDemo.Models;
using PaginationDemo.Service;
using PaginationDemo.Utilities;
using PaginationDemo.Utilities.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using PaginationDemo.Response;


namespace PaginationDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Consumes("application/json")]
    public class ExternalPatronController : ControllerBase
    {
        private readonly ExternalPatronService _externalPatronService;
        public ExternalPatronController(ExternalPatronService externalPatronService)
        {
            _externalPatronService = externalPatronService;
        }

        [HttpPost("filterPatrons")]
        public ActionResult<PagedResponse<ExternalPatron>> FilterHighRiskPatrons(
              [FromBody] Dictionary<string, object>? searchCriteria = null,
              [FromQuery] int page = 1,
              [FromQuery] int size = 10)
        {
            if (searchCriteria == null || !searchCriteria.Any())
            {
                return GetAllExternalPatrons(page, size);
            }

            try
            {
                var filteredData = _externalPatronService.FilterExternalPatrons(searchCriteria, page, size);
                return Ok(filteredData);
            }
            catch (JsonException e)
            {
                Console.WriteLine($"[JsonException]: {e}");
                return BadRequest("Invalid JSON format in the request body.");
            }
        }

        [HttpPost("getAll")]
        public ActionResult<PagedResponse<ExternalPatron>> GetAllExternalPatrons(int page = 1, int size = 10)
        {
            var allPatrons = _externalPatronService.GetAllExternalPatrons(page, size);
            return Ok(allPatrons);
        }
    }
}