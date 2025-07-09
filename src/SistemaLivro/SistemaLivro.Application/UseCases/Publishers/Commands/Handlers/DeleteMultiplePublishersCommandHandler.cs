using MediatR;
using SistemaLivro.Application.UseCases.Publishers.Commands;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Publishers.Commands.Handlers;

public class DeleteMultiplePublishersCommandHandler(IUnitOfWork unitOfWork) :
    IRequestHandler<DeleteMultiplePublishersCommand, BaseResult>
{
    public async Task<BaseResult> Handle(DeleteMultiplePublishersCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var repository = unitOfWork.RepositoryFactory.PublisherRepository;

            // Busca os publishers que existem e não estão deletados
            var publishers = await repository.FindAsync(p => request.Ids.Contains(p.Id) && !p.IsDeleted);
            var existingPublishers = publishers.ToList();

            if (!existingPublishers.Any())
            {
                return new BaseResult(false, "Nenhuma editora encontrada para exclusão.");
            }

            // Marca os publishers como deletados
            foreach (var publisher in existingPublishers)
            {
                publisher.Delete();
                await repository.UpdateAsync(publisher);
            }

            await unitOfWork.CommitAsync();

            var deletedCount = existingPublishers.Count;
            var message = deletedCount == 1
                ? "Editora excluída com sucesso."
                : $"{deletedCount} editoras excluídas com sucesso.";

            return new BaseResult(true, message);
        }
        catch (Exception ex)
        {
            return new BaseResult(false, $"Erro ao excluir editoras: {ex.Message}");
        }
    }
}
