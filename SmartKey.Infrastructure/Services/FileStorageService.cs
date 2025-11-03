using Microsoft.Extensions.Logging;
using SmartKey.Application.Common.Interfaces.Services;

namespace SmartKey.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(ILogger<FileStorageService> logger)
        {
            _logger = logger;
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName)
        {
            // Giả lập việc tải tệp lên
            await Task.Delay(50);
            return $"https://dummy.local/files/{fileName}";
        }
    }
}
