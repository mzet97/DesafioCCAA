using Bogus;
using SistemaLivro.Domain.Domains.Books.Entities;

namespace SistemaLivro.Domain.Domains.Books.Fakes;

public static class FakePublisher
{
    private const string Locale = "pt_BR";

    public static IEnumerable<Publisher> GetValid(int qtd)
    {
        var publisherGenerator = new Faker<Publisher>(Locale)
            .CustomInstantiator(f =>
            {
                var createdAt = f.Date.Past(2, DateTime.Now.AddMinutes(-5));
                var id = f.Random.Guid();
                var isDeleted = false;
                DateTime? deletedAt = null;
                DateTime? updatedAt = f.Random.Bool(0.5f) ? f.Date.Between(createdAt.AddSeconds(1), DateTime.Now) : null;

                var name = f.Company.CompanyName().ClampLength(1, 150);
                var description = f.Lorem.Sentence(f.Random.Int(5, 50)).ClampLength(0, 4000);

                var publisher = new Publisher(
                    id: id,
                    name: name,
                    description: description,
                    createdAt: createdAt,
                    updatedAt: updatedAt,
                    deletedAt: deletedAt,
                    isDeleted: isDeleted
                );
                return publisher;
            });

        return publisherGenerator.Generate(qtd);
    }

    public static IEnumerable<Publisher> GetInvalid(int qtd)
    {
        var invalidPublishers = new List<Publisher>(qtd);
        var baseFaker = new Faker(Locale);

        for (int i = 0; i < qtd; i++)
        {
            var id = baseFaker.Random.Guid();
            var createdAt = baseFaker.Date.Past(2, DateTime.Now.AddMinutes(-5));
            DateTime? updatedAt = null;
            DateTime? deletedAt = null;
            bool isDeleted = false;

            string name = baseFaker.Company.CompanyName().ClampLength(1, 150);
            string description = baseFaker.Lorem.Sentence(10).ClampLength(0, 4000);

            var invalidationType = i % 3;
            switch (invalidationType)
            {
                case 0: name = ""; break;
                case 1: name = baseFaker.Lorem.Letter(151); break;
                case 2: description = baseFaker.Lorem.Letter(4001); break;
            }

            isDeleted = baseFaker.Random.Bool(0.1f);
            if (isDeleted)
            {
                deletedAt = baseFaker.Date.Between(createdAt.AddSeconds(1), DateTime.Now);
            }
            else
            {
                deletedAt = null;
            }
            if (baseFaker.Random.Bool(0.5f))
            {
                var maxUpdateDate = deletedAt ?? DateTime.Now;
                if (maxUpdateDate > createdAt.AddSeconds(1))
                {
                    updatedAt = baseFaker.Date.Between(createdAt.AddSeconds(1), maxUpdateDate);
                }
            }

            var publisher = new Publisher(
                id: id, name: name, description: description, createdAt: createdAt,
                updatedAt: updatedAt, deletedAt: deletedAt, isDeleted: isDeleted
            );
            invalidPublishers.Add(publisher);
        }
        return invalidPublishers;
    }

    public static IEnumerable<Publisher> GetRandom(int numValid = 5, int numInvalid = 5)
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
        return str.ClampLength(0, max);
    }
}
