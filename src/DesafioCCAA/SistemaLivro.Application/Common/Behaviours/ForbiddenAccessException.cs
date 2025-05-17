namespace SistemaLivro.Application.Common.Behaviours;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string? message) : base(message)
    {
    }
}
