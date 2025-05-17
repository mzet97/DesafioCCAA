using SistemaLivro.Domain.Domains.Books;
using SistemaLivro.Domain.Domains.Books.ValueObjects;
using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using SistemaLivro.Application.UseCases.Books.Commands;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Books.Commands.Handlers;

public class CreateBookCommandHandler(
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<CreateBookCommandHandler> logger
    ) : IRequestHandler<CreateBookCommand, BaseResult<Guid>>
{

    public async Task<BaseResult<Guid>> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        var entity = Book.Create(
            request.Title,
            request.Author,
            request.Synopsis,
            request.ISBN,
            new CoverImage("", ""),
            request.GenderId,
            request.PublisherId,
            request.UserCreatedId
        );

        if (!entity.IsValid())
        {
            logger.LogError("Validate Book has error");
            throw new ValidationException("Livor tem erros na validação", entity.Errors);
        }

        var gender = await unitOfWork
            .RepositoryFactory
            .GenderRepository
            .GetByIdAsync(request.GenderId);

        if(gender is null)
        {
            logger.LogError("Gender not found");
            throw new NotFoundException("Gênero não encontrado");
        }

        var publisher = await unitOfWork
            .RepositoryFactory
            .PublisherRepository
            .GetByIdAsync(request.PublisherId);

        if (publisher is null)
        {
            logger.LogError("Publisher not found");
            throw new NotFoundException("Editora não encontrado");
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            var BookRepository = unitOfWork.RepositoryFactory.BookRepository;
            await BookRepository.AddAsync(entity);
            await unitOfWork.CommitAsync();

            foreach (var @event in entity.Events)
            {
                await mediator.Publish(@event);
            }

            return new BaseResult<Guid>(entity.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error to create Book");
            await unitOfWork.RollbackAsync();
            throw new Exception("Erro ao criar livro", ex);
        }
    }
}
