using SistemaLivro.Shared.Models;
using FluentValidation;

namespace SistemaLivro.Domain.Domains.Books.Entities.Validations;

public class GenderValidation : AbstractValidator<Gender>
{
    public GenderValidation()
    {
        Include(new EntityValidation<Gender>());

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(150).WithMessage("O nome não pode exceder 150 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("A descrição não pode exceder 4000 caracteres.");
    }
}
