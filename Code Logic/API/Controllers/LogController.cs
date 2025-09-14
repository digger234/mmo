using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MMOTool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private static List<LogEntry> _logs = new List<LogEntry>();
        private static int _logId = 1;

        [HttpGet("entries")]
        public IActionResult GetLogEntries()
        {
            try
            {
                return Ok(_logs.OrderByDescending(x => x.Timestamp).Take(50).ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("add")]
        public IActionResult AddLogEntry([FromBody] AddLogRequest request)
        {
            try
            {
                var logEntry = new LogEntry
                {
                    Id = _logId++,
                    Message = request.Message,
                    Type = request.Type,
                    Timestamp = DateTime.Now,
                    Icon = GetIconForType(request.Type)
                };

                _logs.Add(logEntry);

                if (_logs.Count > 100)
                {
                    _logs.RemoveAt(0);
                }

                return Ok(new { message = "Log entry added successfully", logEntry });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("clear")]
        public IActionResult ClearLogs()
        {
            try
            {
                _logs.Clear();
                return Ok(new { message = "Logs cleared successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private string GetIconForType(string type)
        {
            return type?.ToLower() switch
            {
                "account" => "👤",
                "email" => "📧",
                "job" => "💰",
                "proxy" => "🌐",
                "error" => "❌",
                "success" => "✅",
                "info" => "ℹ️",
                "warning" => "⚠️",
                _ => "📝"
            };
        }
    }

    public class LogEntry
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Icon { get; set; } = string.Empty;
    }

    public class AddLogRequest
    {
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
