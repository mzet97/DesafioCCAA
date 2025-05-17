using SistemaLivro.Domain.Domains.Books.ValueObjects.Validations;
using SistemaLivro.Shared.Models;

namespace SistemaLivro.Domain.Domains.Books.ValueObjects;

public sealed class CoverImage : ValueObject<CoverImage>
{
    public static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

    public string FileName { get; }
    public string Path { get; }

    public CoverImage(string fileName, string path)
        : base(new CoverImageValidator())
    {
        FileName = fileName;
        Path = path;

        Validate();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FileName;
        yield return Path;
    }
}
