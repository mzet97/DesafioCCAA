using SistemaLivro.Shared.Models;
using FluentValidation;
using SistemaLivro.Domain.Domains.Books.Entities;

namespace SistemaLivro.Domain.Domains.Books.Entities.Validations;

public class PublisherValidation : AbstractValidator<Publisher>
{
    public PublisherValidation()
    {
        Include(new EntityValidation<Publisher>());

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(150).WithMessage("O nome não pode exceder 150 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("A descrição não pode exceder 4000 caracteres.");
    }
}
