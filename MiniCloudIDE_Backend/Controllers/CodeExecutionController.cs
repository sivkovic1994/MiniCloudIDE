using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using MiniCloudIDE_Backend.Services;

namespace MiniCloudIDE_Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CodeExecutionController : ControllerBase
    {
        private readonly IScriptHistoryService _historyService;
        private readonly IPythonExecutionService _pythonExecutionService;

        public CodeExecutionController(IScriptHistoryService historyService, IPythonExecutionService pythonExecutionService)
        {
            _historyService = historyService;
            _pythonExecutionService = pythonExecutionService;
        }

        #region Public Methods

        [HttpPost]
        public async Task<IActionResult> Run([FromBody] CodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                return Ok(new { output = "No code entered" });

            switch (request.Language.ToLower())
            {
                case "c#":
                    return await RunCSharp(request.Code);
                case "python":
                    return await RunPython(request.Code);
                default:
                    return BadRequest(new { error = "Unsupported language" });
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveScript([FromBody] CodeRequest request)
        {
            await _historyService.SaveScript(request.Language, request.Code);

            var history = await _historyService.GetHistory(request.Language);

            return Ok(history);
        }

        [HttpGet("history/{language}")]
        public async Task<IActionResult> GetHistory(string language)
        {
            var history = await _historyService.GetHistory(language);
            return Ok(history);
        }

        #endregion

        #region Helpers

        private async Task<IActionResult> RunCSharp(string code)
        {
            try
            {
                // Configure Roslyn scripting environment with basic imports and references.
                var scriptOptions = ScriptOptions.Default
                    .WithImports("System")
                    .WithReferences(typeof(object).Assembly);

                // Run C# code in-memory with Roslyn (no external process).
                var result = await CSharpScript.EvaluateAsync(code, scriptOptions);

                return Ok(new { output = result?.ToString() ?? "null" });
            }
            catch (CompilationErrorException ex)
            {
                return Ok(new { output = string.Join("\n", ex.Diagnostics) });
            }
        }

        private async Task<IActionResult> RunPython(string code)
        {
            try
            {
                // Execute Python code via PythonWorkerHostedService (TCP socket communication)
                var (output, errors) = await _pythonExecutionService.ExecuteAsync(code);

                return Ok(new { output, errors });
            }
            catch (Exception ex)
            {
                return Ok(new { output = "", errors = ex.Message });
            }
        }

        #endregion
    }

    public class CodeRequest
    {
        public string Language { get; set; } = "C#";
        public string Code { get; set; } = "";
    }
}
