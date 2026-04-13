using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using LiveExpert.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LiveExpert.API.Controllers
{
    [ApiController]
    [Route("api/whatsapp")]
    public class WhatsAppController : ControllerBase
    {
        private readonly IWhatsAppService _whatsAppService;
        private readonly IConfiguration _configuration;

        public WhatsAppController(IWhatsAppService whatsAppService, IConfiguration configuration)
        {
            _whatsAppService = whatsAppService;
            _configuration = configuration;
        }

        /// <summary>
        /// Send Hello World test template to a specified number (admin only)
        /// </summary>
        [HttpPost("send-test")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendTest([FromBody] SendTestRequest? request)
        {
            // Use number from request body, or fall back to config test number
            var testNumber = request?.PhoneNumber
                ?? _configuration["WhatsApp:TestPhoneNumber"]
                ?? "919390369835";

            var success = await _whatsAppService.SendTemplateMessageAsync(
                testNumber,
                "hello_world",
                new List<string>()
            );

            if (success)
                return Ok(new { message = $"WhatsApp test message sent to {testNumber}" });

            return BadRequest(new { message = "Failed to send WhatsApp test message" });
        }

        /// <summary>
        /// Send dynamic template message
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage(
            [FromBody] SendWhatsAppRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.TemplateName))
                return BadRequest(new { message = "Invalid request data" });

            var success = await _whatsAppService.SendTemplateMessageAsync(
                request.To,
                request.TemplateName,
                request.Parameters ?? new List<string>()
            );

            if (success)
                return Ok(new { message = "WhatsApp message sent successfully" });

            return BadRequest(new { message = "Failed to send WhatsApp message" });
        }

        /// <summary>
        /// Send plain text message
        /// </summary>
        [HttpPost("send-text")]
        public async Task<IActionResult> SendTextMessage(
            [FromBody] SendWhatsAppTextMessageRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.Message))
                return BadRequest(new { message = "Invalid request data" });

            var success = await _whatsAppService.SendMessageAsync(
                request.To,
                request.Message
            );

            if (success)
                return Ok(new { message = "WhatsApp message sent successfully" });

            return BadRequest(new { message = "Failed to send WhatsApp message" });
        }
    }

    public class SendTestRequest
    {
        public string? PhoneNumber { get; set; }
    }

    public class SendWhatsAppRequest
    {
        public string To { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public List<string>? Parameters { get; set; }
    }

    public class SendWhatsAppTextMessageRequest
    {
        public string To { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}