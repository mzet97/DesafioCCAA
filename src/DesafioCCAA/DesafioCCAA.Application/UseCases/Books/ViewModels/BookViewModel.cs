using DesafioCCAA.Application.UseCases.Genders.ViewModels;
using DesafioCCAA.Application.UseCases.Publishers.ViewModels;
using DesafioCCAA.Shared.ViewModels;

namespace DesafioCCAA.Application.UseCases.Books.ViewModels;

public class BookViewModel : BaseViewModel
{

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

    public string Title { get; set; }
    public string Author { get; set; }
    public string Synopsis { get; set; }
    public string ISBN { get; set; }

    public CoverImageViewModel CoverImage { get;  set; }

    public Guid GenderId { get;  set; }
    public GenderViewModel Gender { get; set; }

    public Guid PublisherId { get; set; }
    public PublisherViewModel Publisher { get; set; }

    public Guid UserCreatedId { get; set; }
    public ApplicationUserViewModel UserCreated { get; set; }

    public Guid? UserUpdatedId { get; set; }
    public ApplicationUserViewModel? UserUpdated { get; set; }

}
