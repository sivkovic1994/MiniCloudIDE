using Microsoft.AspNetCore.Mvc;

namespace MiniCloudIDE_Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CodeExecutionController : ControllerBase
    {
        [HttpPost]
        public IActionResult Run([FromBody] CodeRequest request)
        {
            return Ok(new { output = $"You sent: {request.Code}" });
        }
    }

    public class CodeRequest
    {
        public string Language { get; set; } = "C#";
        public string Code { get; set; } = "";
    }
}