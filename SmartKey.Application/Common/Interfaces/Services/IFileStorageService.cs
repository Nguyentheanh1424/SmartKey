namespace SmartKey.Application.Common.Interfaces.Services
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(Stream fileStream, string fileName);
    }
}
