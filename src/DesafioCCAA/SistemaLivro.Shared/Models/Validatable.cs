using FluentValidation;
using FluentValidation.Results;

namespace SistemaLivro.Shared.Models;

public abstract class Validatable<T>
{
    private readonly IValidator<T> _validator;
    protected bool _isValid;
    protected IEnumerable<string> _errors = Enumerable.Empty<string>();

    protected Validatable(IValidator<T> validator)
    {
        _validator = validator;
    }

    public virtual void Validate()
    {
        ValidationResult result = _validator.Validate((T)(object)this)!;
        _isValid = result.IsValid;
        _errors = result.Errors.Select(e => e.ErrorMessage);
    }

    public bool IsValid() => _isValid;
    public IEnumerable<string> Errors => _errors;
}