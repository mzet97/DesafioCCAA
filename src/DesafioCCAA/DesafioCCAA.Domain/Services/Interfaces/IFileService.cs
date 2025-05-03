namespace DesafioCCAA.Domain.Services.Interfaces;

public interface IFileService
{
    Task<(string, string)> SaveImageAsync(Stream fileStream, string fileName);
    Task<Stream> GetFileStreamAsync(string fileName);
    Task<string> GetFileBase64Async(string fileName);
}