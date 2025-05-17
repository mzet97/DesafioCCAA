using SistemaLivro.Shared.Models;
using FluentValidation;
using System.Text.RegularExpressions;

namespace SistemaLivro.Domain.Domains.Books.Validations;

public class BookValidation : AbstractValidator<Book>
{
    public BookValidation()
    {
        Include(new EntityValidation<Book>());

        RuleFor(order => order.Title)
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MaximumLength(150).WithMessage("O título não pode exceder 150 caracteres.");

        RuleFor(order => order.Author)
            .NotEmpty().WithMessage("O autor é obrigatório.")
            .MaximumLength(150).WithMessage("O autor não pode exceder 150 caracteres.");

        RuleFor(order => order.Synopsis)
            .NotEmpty().WithMessage("A sinopse é obrigatória.")
            .MaximumLength(4000).WithMessage("A sinopse não pode exceder 4000 caracteres.");

        RuleFor(order => order.GenderId)
           .NotEmpty().WithMessage("O gênero é obrigatório.");

        RuleFor(order => order.PublisherId)
            .NotEmpty().WithMessage("A editora é obrigatória.");

        RuleFor(order => order.UserCreatedId)
            .NotEmpty().WithMessage("O usuário criador é obrigatório.");

        RuleFor(order => order.ISBN)
            .NotEmpty().WithMessage("O ISBN é obrigatório.")
            .Must(BeValidISBN).WithMessage("Formato de ISBN inválido. Deve ser um ISBN-10 ou ISBN-13 válido.");
    }

    private bool BeValidISBN(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            return false;

        isbn = Regex.Replace(isbn, "[-\\s]", "");

        return isbn.Length switch
        {
            10 => IsValidISBN10(isbn),
            13 => IsValidISBN13(isbn),
            _ => false
        };
    }

    private bool IsValidISBN10(string isbn)
    {
        for (int i = 0; i < 9; i++)
        {
            if (!char.IsDigit(isbn[i]))
                return false;
        }

        if (!char.IsDigit(isbn[9]) && isbn[9] != 'X' && isbn[9] != 'x')
            return false;

        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            sum += (isbn[i] - '0') * (10 - i);
        }

        if (isbn[9] == 'X' || isbn[9] == 'x')
        {
            sum += 10;
        }
        else
        {
            sum += isbn[9] - '0';
        }

        return sum % 11 == 0;
    }

    private bool IsValidISBN13(string isbn)
    {
        if (!isbn.StartsWith("978") && !isbn.StartsWith("979"))
            return false;

        foreach (char c in isbn)
        {
            if (!char.IsDigit(c))
                return false;
        }

        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = isbn[i] - '0';
            sum += i % 2 == 0 ? digit : digit * 3;
        }

        int checkDigit = (10 - sum % 10) % 10;
        return checkDigit == isbn[12] - '0';
    }
}
