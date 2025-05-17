using FluentValidation.Results;

namespace SistemaLivro.Domain.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, Exception inner) : base(message, inner)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    public ValidationException(string message, IEnumerable<string> erros)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
        if (erros != null)
            Errors.Add("Errors", erros.ToArray());
    }

    public IDictionary<string, string[]> Errors { get; }
}