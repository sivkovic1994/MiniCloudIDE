using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace MiniCloudIDE_Backend.Services
{
    public class PythonExecutionService : IPythonExecutionService, IDisposable
    {
        private readonly string _host = "127.0.0.1";
        private readonly int _port = 5555;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ILogger<PythonExecutionService> _logger;
        private readonly int _timeoutMs = 10000; // 10 seconds

        public PythonExecutionService(ILogger<PythonExecutionService> logger)
        {
            _logger = logger;
        }

        #region Public Methods

        public async Task<(string output, string errors)> ExecuteAsync(string code, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                using var client = new TcpClient();
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(_timeoutMs);

                await client.ConnectAsync(_host, _port, cts.Token);

                using var stream = client.GetStream();

                // Send code length + code
                var codeBytes = Encoding.UTF8.GetBytes(code);
                var lengthBytes = BitConverter.GetBytes(codeBytes.Length);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(lengthBytes);

                await stream.WriteAsync(lengthBytes, cts.Token);
                await stream.WriteAsync(codeBytes, cts.Token);

                // Read response length (4 bytes)
                var responseLengthBytes = await ReadExactAsync(stream, 4, cts.Token);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(responseLengthBytes);
                int responseLength = BitConverter.ToInt32(responseLengthBytes);

                // Read exact response
                var responseBytes = await ReadExactAsync(stream, responseLength, cts.Token);

                var responseJson = Encoding.UTF8.GetString(responseBytes);

                var result = JsonSerializer.Deserialize<PythonResult>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var finalOutput = result?.Output ?? "";
                var finalErrors = result?.Errors ?? "";

                _logger.LogInformation("=== Returning output: '{Output}', errors: '{Errors}' ===", finalOutput, finalErrors);

                return (finalOutput, finalErrors);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Python execution timed out");
                return ("", "Execution timed out");
            }
            catch (SocketException ex)
            {
                _logger.LogError(ex, "Socket error - ensure Python worker is running on {Host}:{Port}", _host, _port);
                return ("", $"Connection error: Python worker not available. Please start the Python worker.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute Python code");
                return ("", $"Error: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
        }

        #endregion

        #region Helpers

        private static async Task<byte[]> ReadExactAsync(NetworkStream stream, int count, CancellationToken cancellationToken)
        {
            var buffer = new byte[count];
            int totalRead = 0;

            while (totalRead < count)
            {
                int read = await stream.ReadAsync(buffer.AsMemory(totalRead, count - totalRead), cancellationToken);

                if (read == 0)
                    throw new IOException("Connection closed before all data received");

                totalRead += read;
            }

            return buffer;
        }

        private class PythonResult
        {
            public string Output { get; set; } = "";
            public string Errors { get; set; } = "";
        }

        #endregion
    }
}
