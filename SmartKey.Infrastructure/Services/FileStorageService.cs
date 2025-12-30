using Microsoft.Extensions.Logging;
using SmartKey.Application.Common.Interfaces.Services;
using Supabase;

namespace SmartKey.Infrastructure.Services
{
    public sealed class SupabaseStorageOptions
    {
        public string Url { get; set; } = default!;
        public string ServiceRoleKey { get; set; } = default!;
        public string Bucket { get; set; } = "uploads";
    }

    public class FileStorageService : IFileStorageService
    {
        private readonly ILogger<FileStorageService> _logger;
        private readonly Client _supabase;
        private readonly SupabaseStorageOptions _options;

        public FileStorageService(
            ILogger<FileStorageService> logger,
            Client supabase,
            SupabaseStorageOptions options)
        {
            _logger = logger;
            _supabase = supabase;
            _options = options;
        }

        public async Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string? contentType = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var ms = new MemoryStream();
                await fileStream.CopyToAsync(ms, cancellationToken);
                var fileBytes = ms.ToArray();

                // Giữ folder, chỉ random phần tên file
                fileName = (fileName ?? string.Empty).Replace('\\', '/').TrimStart('/');
                var dir = Path.GetDirectoryName(fileName)?.Replace('\\', '/');
                var baseName = Path.GetFileName(fileName);
                var uniqueBase = $"{Guid.NewGuid():N}_{baseName}";
                var objectPath = string.IsNullOrWhiteSpace(dir)
                    ? uniqueBase
                    : $"{dir}/{uniqueBase}";

                Supabase.Storage.FileOptions? fileOptions = null;
                if (!string.IsNullOrWhiteSpace(contentType))
                {
                    fileOptions = new Supabase.Storage.FileOptions
                    {
                        ContentType = contentType,
                        Upsert = false
                    };
                }

                await _supabase.Storage
                    .From(_options.Bucket)
                    .Upload(fileBytes, objectPath, fileOptions); // Upload(path)

                var publicUrl = _supabase.Storage
                    .From(_options.Bucket)
                    .GetPublicUrl(objectPath); // GetPublicUrl(path)

                return publicUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload file to Supabase Storage failed");
                throw;
            }
        }

        public async Task DeleteAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var objectPath = ExtractObjectPath(filePath);

                await _supabase.Storage
                    .From(_options.Bucket)
                    .Remove(new List<string> { objectPath }); // Remove(List<string>)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete file from Supabase Storage failed");
                throw;
            }
        }

        private string ExtractObjectPath(string filePathOrUrl)
        {
            if (Uri.TryCreate(filePathOrUrl, UriKind.Absolute, out var uri))
            {
                var absolutePath = uri.AbsolutePath;
                var marker = $"/storage/v1/object/public/{_options.Bucket}/";
                var idx = absolutePath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    var path = absolutePath[(idx + marker.Length)..];
                    return Uri.UnescapeDataString(path).TrimStart('/');
                }

                return Uri.UnescapeDataString(Path.GetFileName(absolutePath)).TrimStart('/');
            }

            return filePathOrUrl.Replace('\\', '/').TrimStart('/');
        }
    }
}
