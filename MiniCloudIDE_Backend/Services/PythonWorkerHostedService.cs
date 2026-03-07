using System.Diagnostics;

namespace MiniCloudIDE_Backend.Services
{
    public class PythonWorkerHostedService : IHostedService, IDisposable
    {
        private Process? _pythonProcess;
        private readonly ILogger<PythonWorkerHostedService> _logger;

        public bool IsRunning => _pythonProcess != null && !_pythonProcess.HasExited;
        public int? ProcessId => _pythonProcess?.Id;

        public PythonWorkerHostedService(ILogger<PythonWorkerHostedService> logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Python worker...");

            try
            {
                string workerPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "PythonWorker", "worker.py");

                if (!File.Exists(workerPath))
                {
                    _logger.LogError("Python worker not found at: {WorkerPath}", workerPath);
                    return;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{workerPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false,
                    WorkingDirectory = Path.GetDirectoryName(workerPath)
                };

                _pythonProcess = Process.Start(psi);

                if (_pythonProcess == null)
                {
                    _logger.LogError("Failed to start Python worker process");
                    return;
                }

                _pythonProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        _logger.LogInformation("[Python Worker] {Output}", e.Data);
                };

                _pythonProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        _logger.LogError("[Python Worker] {Error}", e.Data);
                };

                _pythonProcess.BeginOutputReadLine();
                _pythonProcess.BeginErrorReadLine();

                await Task.Delay(1000, cancellationToken);

                _logger.LogInformation("Python worker started successfully (PID: {ProcessId})", _pythonProcess.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Python worker");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Python worker...");

            try
            {
                if (_pythonProcess != null && !_pythonProcess.HasExited)
                {
                    _pythonProcess.Kill(entireProcessTree: true);
                    _pythonProcess.WaitForExit(5000);
                    _logger.LogInformation("Python worker stopped");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping Python worker");
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _pythonProcess?.Dispose();
        }
    }
}
