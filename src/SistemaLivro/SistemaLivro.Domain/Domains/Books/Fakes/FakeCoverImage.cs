using Bogus;
using SistemaLivro.Domain.Domains.Books.ValueObjects;

namespace SistemaLivro.Domain.Domains.Books.Fakes;

public static class FakeCoverImage
{
    private const string Locale = "pt_BR";

    public static IEnumerable<CoverImage> GetValid(int qtd)
    {
        var coverImageGenerator = new Faker<CoverImage>(Locale)
            .CustomInstantiator(f =>
            {
                var fileName = $"{f.Lorem.Slug()}{f.PickRandom(CoverImage.AllowedExtensions)}";

                var path = f.System.DirectoryPath();
                if (string.IsNullOrEmpty(path)) path = "/valid/path";

                var coverImage = new CoverImage(fileName, path);
                return coverImage;
            });

        return coverImageGenerator.Generate(qtd);
    }

    public static IEnumerable<CoverImage> GetInvalid(int qtd)
    {
        var invalidCoverImages = new List<CoverImage>(qtd);
        var baseFaker = new Faker(Locale);

        for (int i = 0; i < qtd; i++)
        {
            string fileName = $"{baseFaker.Lorem.Slug()}{baseFaker.PickRandom(CoverImage.AllowedExtensions)}";
            string path = baseFaker.System.DirectoryPath();
            if (string.IsNullOrEmpty(path)) path = "/valid/path/for/invalidation";

            var invalidationType = i % 4;
            switch (invalidationType)
            {
                case 0: fileName = ""; break;
                case 1: fileName = null; break;
                case 2:
                    fileName = $"{baseFaker.Lorem.Slug()}.txt";
                    break;
                case 3: path = ""; break;
            }

            var coverImage = new CoverImage(fileName, path);
            invalidCoverImages.Add(coverImage);
        }
        return invalidCoverImages;
    }

    public static IEnumerable<CoverImage> GetRandom(int numValid = 5, int numInvalid = 5)
    {
        var validItems = GetValid(numValid);
        var invalidItems = GetInvalid(numInvalid);

        return validItems.Concat(invalidItems)
                         .OrderBy(x => Guid.NewGuid());
    }
}