using SistemaLivro.Domain.Services.Interfaces;
using Microsoft.Extensions.Options;
using SistemaLivro.Shared.Settings;

namespace SistemaLivro.Application.Services;

public class FileService : IFileService
{
    private readonly string _uploadPath;
    private static readonly HashSet<string> _permittedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif"
        };

    public FileService(IOptions<AppSettings> options)
    {
        _uploadPath = options.Value.UploadPath
            ?? throw new ArgumentException("UploadPath não configurado em AppSettings");
        Directory.CreateDirectory(_uploadPath);
    }

    public async Task<(string, string)> SaveImageAsync(Stream fileStream, string fileName)
    {
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext) || !_permittedExtensions.Contains(ext))
            throw new InvalidDataException("Tipo de arquivo não permitido.");

        var uniqueName = Guid.NewGuid().ToString() + ext;
        var fullPath = Path.Combine(_uploadPath, uniqueName);

        await using var targetStream = File.Create(fullPath);
        await fileStream.CopyToAsync(targetStream);

        return (fullPath, uniqueName);
    }

    public Task<Stream> GetFileStreamAsync(string fileName)
    {
        var path = Path.Combine(_uploadPath, fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException("Arquivo não encontrado.", fileName);

        Stream stream = File.OpenRead(path);
        return Task.FromResult(stream);
    }

    public async Task<string> GetFileBase64Async(string fileName)
    {
        var path = Path.Combine(_uploadPath, fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException("Arquivo não encontrado.", fileName);

        var bytes = await File.ReadAllBytesAsync(path);
        return Convert.ToBase64String(bytes);
    }
}
