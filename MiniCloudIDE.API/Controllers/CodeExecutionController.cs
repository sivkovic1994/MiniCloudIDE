using Microsoft.AspNetCore.Mvc;
using MiniCloudIDE.Application.DTOs;
using MiniCloudIDE.Application.Interfaces;
using System.Security.Claims;

namespace MiniCloudIDE.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CodeExecutionController : ControllerBase
    {
        private readonly IScriptHistoryService _historyService;
        private readonly ICodeExecutionService _codeExecutionService;

        public CodeExecutionController(IScriptHistoryService historyService, ICodeExecutionService codeExecutionService)
        {
            _historyService = historyService;
            _codeExecutionService = codeExecutionService;
        }

        [HttpPost]
        public async Task<IActionResult> Run([FromBody] CodeRequest request)
        {
            try
            {
                var result = await _codeExecutionService.ExecuteAsync(request.Language, request.Code);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveScript([FromBody] CodeRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            await _historyService.SaveScript(request.Language, request.Code, userId);

            var history = await _historyService.GetHistory(request.Language, userId);

            return Ok(history);
        }

        [HttpGet("history/{language}")]
        public async Task<IActionResult> GetHistory(string language)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var history = await _historyService.GetHistory(language, userId);
            return Ok(history);
        }

        [HttpGet("script/{id}")]
        public async Task<IActionResult> GetScriptById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var script = await _historyService.GetScriptById(id, userId);

            if (script == null)
                return NotFound(new { error = "Script not found" });

            return Ok(script);
        }
    }
}
