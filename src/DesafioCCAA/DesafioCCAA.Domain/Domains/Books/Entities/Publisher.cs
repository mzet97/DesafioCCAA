using DesafioCCAA.Domain.Domains.Books.Entities.Validations;
using DesafioCCAA.Domain.Domains.Books.Events.Publishers;
using DesafioCCAA.Shared.Models;

namespace DesafioCCAA.Domain.Domains.Books.Entities;

public class Publisher : Entity<Publisher>
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    public Publisher(
        Guid id,
        string name,
        string description,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt,
        bool isDeleted) : base(new PublisherValidation(),
                               id,
                               createdAt,
                               updatedAt,
                               deletedAt,
                               isDeleted)
    {
        Name = name;
        Description = description;
        Validate();
    }

    public static Publisher Create(
        string name,
        string description)
    {
        var entity =
            new Publisher(
                Guid.NewGuid(),
                name,
                description,
                DateTime.Now,
                null, null, false);

        entity.AddEvent(
            new PublisherCreated(entity));

        entity.Validate();

        return entity;
    }

    public void Update(
        string name,
        string description)
    {
        Name = name;
        Description = description;

        Validate();

        AddEvent(
            new PublisherUpdated(this));
    }

    public void Delete()
    {
        Validate();
        AddEvent(
            new PublisherDeleted(this));
    }

    public override void Disabled()
    {
        base.Disabled();
        Validate();
        AddEvent(
            new PublisherDisabled(this));
    }

    public override void Activate()
    {
        base.Activate();
        Validate();
        AddEvent(
            new PublisherAtived(this));
    }
}