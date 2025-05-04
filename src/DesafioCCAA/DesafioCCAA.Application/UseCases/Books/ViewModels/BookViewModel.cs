using DesafioCCAA.Application.UseCases.Genders.ViewModels;
using DesafioCCAA.Application.UseCases.Publishers.ViewModels;
using DesafioCCAA.Application.UseCases.Users.ViewModels;
using DesafioCCAA.Domain.Domains.Books;
using DesafioCCAA.Shared.ViewModels;

namespace DesafioCCAA.Application.UseCases.Books.ViewModels;

public class BookViewModel : BaseViewModel
{
    public string Title { get; set; }
    public string Author { get; set; }
    public string Synopsis { get; set; }
    public string ISBN { get; set; }

    public CoverImageViewModel CoverImage { get;  set; }

    public Guid GenderId { get;  set; }
    public string GenderName { get => Gender.Name; }
    public GenderViewModel Gender { get; set; }

    public Guid PublisherId { get; set; }
    public string PublisherName { get => Publisher.Name; }
    public PublisherViewModel Publisher { get; set; }

    public Guid UserCreatedId { get; set; }
    public ApplicationUserViewModel UserCreated { get; set; }

    public Guid? UserUpdatedId { get; set; }
    public ApplicationUserViewModel? UserUpdated { get; set; }

    public BookViewModel(
        Guid id,
        string title,
        string author,
        string synopsis,
        string iSBN,
        CoverImageViewModel coverImage,
        Guid genderId,
        GenderViewModel gender,
        Guid publisherId,
        PublisherViewModel publisher,
        Guid userCreatedId,
        ApplicationUserViewModel userCreated,
        Guid? userUpdatedId,
        ApplicationUserViewModel? userUpdated,
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
        Title = title;
        Author = author;
        Synopsis = synopsis;
        ISBN = iSBN;
        CoverImage = coverImage;
        GenderId = genderId;
        Gender = gender;
        PublisherId = publisherId;
        Publisher = publisher;
        UserCreatedId = userCreatedId;
        UserCreated = userCreated;
        UserUpdatedId = userUpdatedId;
        UserUpdated = userUpdated;
    }

    public static BookViewModel FromEntity(Book entity)
    {
        return new BookViewModel(
            entity.Id,
            entity.Title,
            entity.Author,
            entity.Synopsis,
            entity.ISBN,
            entity.CoverImage is not null ? CoverImageViewModel.FromEntity(entity.CoverImage) : null,
            entity.GenderId,
            entity.Gender is not null ? GenderViewModel.FromEntity(entity.Gender) : null,
            entity.PublisherId,
             entity.Publisher is not null ? PublisherViewModel.FromEntity(entity.Publisher) : null,
            entity.UserCreatedId,
            entity.UserCreated is not null ? ApplicationUserViewModel.FromEntity(entity.UserCreated) : null,
            entity.UserUpdatedId,
            entity.UserUpdated is not null ? ApplicationUserViewModel.FromEntity(entity.UserUpdated) : null,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.DeletedAt,
            entity.IsDeleted);
    }
}
