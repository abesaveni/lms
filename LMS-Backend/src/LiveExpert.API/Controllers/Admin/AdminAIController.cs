using LiveExpert.API.Services;
using LiveExpert.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LiveExpert.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/ai")]
    [Authorize(Roles = "Admin")]
    public class AdminAIController : ControllerBase
    {
        private readonly LMSAIService _lmsAIService;
        private readonly ILogger<AdminAIController> _logger;

        public AdminAIController(LMSAIService lmsAIService, ILogger<AdminAIController> logger)
        {
            _lmsAIService = lmsAIService;
            _logger = logger;
        }

        [HttpPost("churn-prediction")]
        public async Task<IActionResult> ChurnPrediction([FromBody] ChurnPredictionRequest request)
        {
            try
            {
                var result = await _lmsAIService.ChurnPrediction(request.StudentUsageData);
                return Ok(new { prediction = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ChurnPrediction endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("revenue-analytics")]
        public async Task<IActionResult> RevenueAnalytics([FromBody] RevenueAnalyticsRequest request)
        {
            try
            {
                var result = await _lmsAIService.RevenueAnalytics(request.FinancialData, request.Period);
                return Ok(new { analytics = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RevenueAnalytics endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("support-triage")]
        public async Task<IActionResult> SupportTriage([FromBody] SupportTriageRequest request)
        {
            try
            {
                var result = await _lmsAIService.SupportTriage(request.TicketContent);
                return Ok(new { triage = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SupportTriage endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("fraud-detection")]
        public async Task<IActionResult> FraudDetection([FromBody] FraudDetectionRequest request)
        {
            try
            {
                var result = await _lmsAIService.FraudDetection(request.TransactionData, request.UserIp);
                return Ok(new { assessment = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FraudDetection endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("dispute-resolution")]
        public async Task<IActionResult> DisputeResolution([FromBody] DisputeResolutionRequest request)
        {
            try
            {
                var result = await _lmsAIService.DisputeResolution(request.DisputeDetails, request.SessionTranscript);
                return Ok(new { resolution = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DisputeResolution endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
