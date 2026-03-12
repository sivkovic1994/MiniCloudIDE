using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using MiniCloudIDE.Application.DTOs;
using MiniCloudIDE.Application.Interfaces;
using System.Diagnostics;

namespace MiniCloudIDE.Application.Services
{
    public class CodeExecutionService : ICodeExecutionService
    {
        private readonly IPythonExecutionService _pythonExecutionService;

        public CodeExecutionService(IPythonExecutionService pythonExecutionService)
        {
            _pythonExecutionService = pythonExecutionService;
        }

        public async Task<ExecutionResult> ExecuteAsync(string language, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return new ExecutionResult { Output = "No code entered" };

            return language.ToLower() switch
            {
                "c#" => await RunCSharpAsync(code),
                "python" => await RunPythonAsync(code),
                _ => throw new ArgumentException($"Unsupported language: {language}")
            };
        }

        private async Task<ExecutionResult> RunCSharpAsync(string code)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var scriptOptions = ScriptOptions.Default
                    .WithImports("System")
                    .WithReferences(typeof(object).Assembly);
                var result = await CSharpScript.EvaluateAsync(code, scriptOptions);
                stopwatch.Stop();

                return new ExecutionResult
                {
                    Output = result?.ToString() ?? "null",
                    ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds
                };
            }
            catch (CompilationErrorException ex)
            {
                stopwatch.Stop();
                return new ExecutionResult
                {
                    Errors = string.Join("\n", ex.Diagnostics),
                    ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new ExecutionResult
                {
                    Errors = ex.Message,
                    ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds
                };
            }
        }

        private async Task<ExecutionResult> RunPythonAsync(string code)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var (output, errors) = await _pythonExecutionService.ExecuteAsync(code);
                stopwatch.Stop();

                return new ExecutionResult
                {
                    Output = output,
                    Errors = errors,
                    ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new ExecutionResult
                {
                    Errors = ex.Message,
                    ExecutionTimeMs = stopwatch.Elapsed.TotalMilliseconds
                };
            }
        }
    }
}
