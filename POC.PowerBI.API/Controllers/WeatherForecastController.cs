using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.PowerBI.Api.Models;
using POC.PowerBI.API.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace POC.PowerBI.API.Controllers
{
    [ApiController]
    [Route("sample")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly PowerBiService _powerBiService;

        public WeatherForecastController(PowerBiService powerBiService)
        {
            _powerBiService = powerBiService;
        }

        [HttpGet("/gettoken")]
        public async Task<IActionResult> GetToken()
        {
            var result = await _powerBiService.GetAccessToken();
            
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReports()
        {
            var result = await _powerBiService.GetAllReports();
            return Ok(result);
        }

        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetReport(Guid reportId)
        
        {
            var result = await _powerBiService.GetReport(reportId);
            return Ok(result);
        }

        [HttpGet("GetAllReportsUsingAPI")]
        public async Task<IActionResult> GetAllReportsUsingAPI()
        {
            var result = await _powerBiService.GetAllReportsUsingAPI();
            return Ok(result);
        }

        [HttpPost("UploadDataset")]
        public async Task<IActionResult> UploadDataset([FromBody]CreateDatasetRequest datasetSchema)
        {
            var result = await _powerBiService.UploadDataset(datasetSchema);
            return Ok(result);
        }

        [HttpPost("{datasetId}/table/{tableName}/AddRows")]
        public async Task<IActionResult> AddRows(Guid datasetId, string tableName, [FromBody] PostRowsRequest rows)
        {
            var result = await _powerBiService.AddRows(datasetId, tableName, rows);
            return Ok(result);
        }

        [HttpPost("ImportPBIX/{datasetDisplayName}")]
        public async Task<IActionResult> ImportPBIX(string datasetDisplayName, [FromForm]IFormFile file)
        {
            //var filePath = Path.GetTempFileName();
            //var stream = System.IO.File.Create(filePath);

            //if (file.Length > 0)
            //{
            //    await file.CopyToAsync(stream);
            //}
            var result = await _powerBiService.ImportPBIX(file, datasetDisplayName);
            return Ok(result);
        }
    }
}
