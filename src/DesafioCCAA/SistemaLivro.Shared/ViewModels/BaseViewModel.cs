using System.ComponentModel;

namespace SistemaLivro.Shared.ViewModels;

public abstract class BaseViewModel
{
    public Guid Id { get; set; }

    [DisplayName("Criado em")]
    public DateTime CreatedAt { get; set; }

    [DisplayName("Atualizado em")]
    public DateTime? UpdatedAt { get; set; }

    [DisplayName("Excluído em")]
    public DateTime? DeletedAt { get; set; }

    [DisplayName("Deletado")]
    public bool IsDeleted { get; set; }

    protected BaseViewModel(Guid id,
                            DateTime createdAt,
                            DateTime? updatedAt,
                            DateTime? deletedAt,
                            bool isDeleted)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
        IsDeleted = isDeleted;
    }

}

