using DesafioCCAA.Domain.Domains.Books.Entities.Validations;
using DesafioCCAA.Domain.Domains.Books.Events.Genders;
using DesafioCCAA.Shared.Models;

namespace DesafioCCAA.Domain.Domains.Books.Entities;

public class Gender : Entity<Gender>
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    public Gender(
        Guid id,
        string name,
        string description,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt,
        bool isDeleted) : base(new GenderValidation(),
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

    public static Gender Create(
        string name,
        string description)
    {
        var entity =
            new Gender(
                Guid.NewGuid(),
                name,
                description,
                DateTime.Now,
                null, null, false);

        entity.AddEvent(
            new GenderCreated(entity));

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
            new GenderUpdated(this));
    }

    public void Delete()
    {
        Validate();
        AddEvent(
            new GenderDeleted(this));
    }

    public override void Disabled()
    {
        base.Disabled();
        Validate();
        AddEvent(
            new GenderDisabled(this));
    }

    public override void Activate()
    {
        base.Activate();
        Validate();
        AddEvent(
            new GenderAtived(this));
    }
}