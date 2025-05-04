using Bogus;

namespace DesafioCCAA.Domain.Domains.Books.Fakes;

public static class FakeBook
{
    private const string Locale = "pt_BR";

    private static readonly string[] ValidIsbnPool = new string[]
    {
        "978-3-16-148410-0", // ISBN-13
        "978-0-306-40615-7", // ISBN-13
        "978-1-56619-909-4", // ISBN-13
        "0-306-40615-2",     // ISBN-10
        "0-596-52068-9",     // ISBN-10
        "1-56619-909-3"      // ISBN-10
    };

    public static IEnumerable<Book> GetValid(int qtd)
    {
        var validCoverImage = FakeCoverImage.GetValid(1).First();

        var bookGenerator = new Faker<Book>(Locale)
            .CustomInstantiator(f =>
            {
                var createdAt = f.Date.Past(1, DateTime.Now.AddMinutes(-5));
                var id = f.Random.Guid();
                var isDeleted = false;
                DateTime? deletedAt = null;
                DateTime? updatedAt = f.Random.Bool(0.5f) ? f.Date.Between(createdAt.AddSeconds(1), DateTime.Now) : null;

                var title = f.Commerce.ProductName().ClampLength(1, 150);
                var author = f.Name.FullName().ClampLength(1, 150);
                var synopsis = f.Lorem.Paragraph(3).ClampLength(1, 4000);

                var isbn = f.PickRandom(ValidIsbnPool);
                var genderId = f.Random.Guid();
                var publisherId = f.Random.Guid();
                var userCreatedId = f.Random.Guid();

                var book = new Book(
                    id: id, title: title, author: author, synopsis: synopsis, isbn: isbn,
                    coverImage: validCoverImage, genderId: genderId, publisherId: publisherId,
                    userCreatedId: userCreatedId, createdAt: createdAt, updatedAt: updatedAt,
                    deletedAt: deletedAt, isDeleted: isDeleted
                );
                return book;
            });

        return bookGenerator.Generate(qtd);
    }

    public static IEnumerable<Book> GetInvalid(int qtd)
    {
        var invalidBooks = new List<Book>(qtd);
        var baseFaker = new Faker(Locale);
        var validCoverImage = FakeCoverImage.GetValid(1).First();

        for (int i = 0; i < qtd; i++)
        {
            var id = baseFaker.Random.Guid();
            var createdAt = baseFaker.Date.Past(1, DateTime.Now.AddMinutes(-5));
            DateTime? updatedAt = null;
            DateTime? deletedAt = null;
            bool isDeleted = false;
            string title = baseFaker.Commerce.ProductName().ClampLength(1, 150);
            string author = baseFaker.Name.FullName().ClampLength(1, 150);
            string synopsis = baseFaker.Lorem.Paragraph(3).ClampLength(1, 4000);

            string isbn = baseFaker.PickRandom(ValidIsbnPool);
            Guid genderId = baseFaker.Random.Guid();
            Guid publisherId = baseFaker.Random.Guid();
            Guid userCreatedId = baseFaker.Random.Guid();

            var invalidationType = i % 7;
            switch (invalidationType)
            {
                case 0: title = ""; break;
                case 1: author = ""; break;
                case 2: synopsis = ""; break;
                case 3: isbn = "12345ABCDE"; break;
                case 4: genderId = Guid.Empty; break;
                case 5: publisherId = Guid.Empty; break;
                case 6: userCreatedId = Guid.Empty; break;
            }

            isDeleted = baseFaker.Random.Bool(0.1f);
            if (isDeleted) { deletedAt = baseFaker.Date.Between(createdAt.AddSeconds(1), DateTime.Now); }
            if (baseFaker.Random.Bool(0.5f))
            {
                var maxUpdateDate = deletedAt ?? DateTime.Now;
                if (maxUpdateDate > createdAt.AddSeconds(1))
                {
                    updatedAt = baseFaker.Date.Between(createdAt.AddSeconds(1), maxUpdateDate);
                }
            }

            var book = new Book(
                id: id, title: title, author: author, synopsis: synopsis, isbn: isbn,
                coverImage: validCoverImage, genderId: genderId, publisherId: publisherId,
                userCreatedId: userCreatedId, createdAt: createdAt, updatedAt: updatedAt,
                deletedAt: deletedAt, isDeleted: isDeleted
            );
            invalidBooks.Add(book);
        }
        return invalidBooks;
    }

    public static IEnumerable<Book> GetRandom(int numValid = 5, int numInvalid = 5)
    {
        var validItems = GetValid(numValid);
        var invalidItems = GetInvalid(numInvalid);

        return validItems.Concat(invalidItems)
                         .OrderBy(x => Guid.NewGuid());
    }

    private static string ClampLength(this string str, int min, int max)
    {
        if (string.IsNullOrEmpty(str))
        {
            return min > 0 ? new Bogus.DataSets.Lorem(Locale).Letter(min) : "";
        }
        if (str.Length > max)
        {
            return str.Substring(0, max);
        }
        if (min > 0 && str.Length < min)
        {
            return str + new Bogus.DataSets.Lorem(Locale).Letter(min - str.Length);
        }
        return str;
    }
    private static string ClampLength(this string str, int max)
    {
        return ClampLength(str, 0, max);
    }
}