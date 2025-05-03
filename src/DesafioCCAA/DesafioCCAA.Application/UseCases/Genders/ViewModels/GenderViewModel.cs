using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Shared.ViewModels;
using System.ComponentModel;

namespace DesafioCCAA.Application.UseCases.Genders.ViewModels;

public class GenderViewModel : BaseViewModel
{
    [DisplayName("Nome")]
    public string Name { get; set; }
    [DisplayName("Descrição")]
    public string Description { get; set; }

    public GenderViewModel(
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

    public static GenderViewModel FromEntity(Gender Gender)
    {
        return new GenderViewModel(
            Gender.Id,
            Gender.Name,
            Gender.Description,
            Gender.CreatedAt,
            Gender.UpdatedAt,
            Gender.DeletedAt,
            Gender.IsDeleted);
    }
}
