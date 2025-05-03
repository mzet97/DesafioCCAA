using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Domain.Domains.Books.Events.Books;
using DesafioCCAA.Domain.Domains.Books.Validations;
using DesafioCCAA.Domain.Domains.Books.ValueObjects;
using DesafioCCAA.Domain.Domains.Identities;
using DesafioCCAA.Shared.Models;

namespace DesafioCCAA.Domain.Domains.Books;

public class Book : AggregateRoot<Book>
{
    public string Title { get; private set; }
    public string Author { get; private set; }
    public string Synopsis { get; private set; }
    public string ISBN { get; private set; }

    public CoverImage CoverImage { get; private set; }

    public Guid GenderId { get; private set; }
    public Gender Gender { get; set; } = null!;

    public Guid PublisherId { get; private set; }
    public Publisher Publisher { get; set; } = null!;

    public Guid UserCreatedId { get; private set; }
    public ApplicationUser UserCreated { get; set; } = null!;

    public Guid? UserUpdatedId { get; private set; }
    public ApplicationUser? UserUpdated { get; set; } = null!;

    public Book() : base(new BookValidation())
    {

    }

    public Book(
        Guid id,
        string title,
        string author,
        string synopsis,
        string isbn,
        CoverImage coverImage,
        Guid genderId,
        Guid publisherId,
        Guid userCreatedId,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt,
        bool isDeleted) : base(new BookValidation(), id, createdAt, updatedAt, deletedAt, isDeleted)
    {
        Title = title;
        Author = author;
        Synopsis = synopsis;
        ISBN = isbn;
        CoverImage = coverImage;
        GenderId = genderId;
        PublisherId = publisherId;
        UserCreatedId = userCreatedId;
        UserUpdatedId = userCreatedId;

        Validate();
    }

    public static Book Create(
        string title,
        string author,
        string synopsis,
        string isbn,
        CoverImage coverImage,
        Guid genderId,
        Guid publisherId,
        Guid userCreatedId)
    {
        var entity =
            new Book(
                Guid.NewGuid(),
                title,
                author,
                synopsis,
                isbn,
                coverImage,
                genderId,
                publisherId,
                userCreatedId,
                DateTime.Now,
                null, null, false);

        entity.Validate();

        entity.AddEvent(
            new BookCreated(entity));

        return entity;
    }

    public void Update(
        string title,
        string author,
        string synopsis,
        string isbn,
        CoverImage coverImage,
        Guid genderId,
        Gender gender,
        Guid publisherId,
        Publisher publisher,
        Guid userUpdatedId,
        ApplicationUser userUpdated
        )
    {
        Title = title;
        Author = author;
        Synopsis = synopsis;
        ISBN = isbn;
        CoverImage = coverImage;
        GenderId = genderId;
        Gender = gender;
        PublisherId = publisherId;
        Publisher = publisher;
        UserUpdatedId = userUpdatedId;
        UserUpdated = userUpdated;

        Validate();

        AddEvent(
            new BookUpdated(this));
    }

    public void UpdateImage(
        CoverImage coverImage,
        Guid userUpdatedId,
        ApplicationUser userUpdated)
    {
        CoverImage = coverImage;
        UserUpdatedId = userUpdatedId;
        UserUpdated = userUpdated;
        Validate();
        AddEvent(
            new BookUpdated(this));
    }


    public void Delete(
        Guid userDeletedId,
        ApplicationUser userDeleted)
    {
        UserUpdatedId = userDeletedId;
        UserUpdated = userDeleted;
        Validate();
        AddEvent(
            new BookDeleted(this));
    }

    public override void Disabled()
    {
        base.Disabled();
        Validate();
        AddEvent(
            new BookDisabled(this));
    }

    public override void Activate()
    {
        base.Activate();
        Validate();
        AddEvent(
            new BookAtived(this));
    }
}