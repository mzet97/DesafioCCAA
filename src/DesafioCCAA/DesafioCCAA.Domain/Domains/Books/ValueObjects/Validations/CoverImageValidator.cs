using FluentValidation;

namespace DesafioCCAA.Domain.Domains.Books.ValueObjects.Validations;

public class CoverImageValidator : AbstractValidator<CoverImage>
{
    public CoverImageValidator()
    {
        RuleFor(ci => ci.FileName)
            .NotEmpty().WithMessage("Nome do arquivo é obrigatório.")
            .Must(fn => CoverImage.AllowedExtensions.Contains(System.IO.Path.GetExtension(fn).ToLowerInvariant()))
            .WithMessage($"Extensão inválida. Permitido: {string.Join(", ", CoverImage.AllowedExtensions)}.");

        RuleFor(ci => ci.Path)
            .NotEmpty().WithMessage("Caminho da imagem é obrigatório.");
    }
}