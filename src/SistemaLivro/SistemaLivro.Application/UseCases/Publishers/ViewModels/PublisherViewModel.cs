using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Shared.ViewModels;

namespace SistemaLivro.Application.UseCases.Publishers.ViewModels;

public class PublisherViewModel : BaseViewModel
{
    public string Name { get; set; }
    public string Description { get; set; }

    public PublisherViewModel(
        Guid id,
        string name,
        string description,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt,
        bool isDeleted) : base(
            id,
            createdAt,
            updatedAt,
            deletedAt,
            isDeleted)
    {
        Name = name;
        Description = description;
    }

    public static PublisherViewModel FromEntity(Publisher publisher)
    {
        return new PublisherViewModel(
            publisher.Id,
            publisher.Name,
            publisher.Description,
            publisher.CreatedAt,
            publisher.UpdatedAt,
            publisher.DeletedAt,
            publisher.IsDeleted);
    }
}
