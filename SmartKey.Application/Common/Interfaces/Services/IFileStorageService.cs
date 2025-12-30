namespace SmartKey.Application.Common.Interfaces.Services
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string? contentType = null,
            CancellationToken cancellationToken = default
        );

        Task DeleteAsync(
            string filePath,
            CancellationToken cancellationToken = default
        );
    }
}
