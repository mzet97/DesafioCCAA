using SistemaLivro.Domain.Domains.Books.ValueObjects;

namespace SistemaLivro.Application.UseCases.Books.ViewModels;

public class CoverImageViewModel
{

    public string FileName { get; }
    public string Path { get; }

    public CoverImageViewModel(string fileName, string path)
    {
        FileName = fileName;
        Path = path;
    }

    public static CoverImageViewModel FromEntity(CoverImage entity)
    {
        return new CoverImageViewModel(
            entity.FileName,
            entity.Path);
    }
}
