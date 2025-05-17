using SistemaLivro.Application.UseCases.Publishers.Commands;
using SistemaLivro.Application.UseCases.Publishers.Commands.Handlers;
using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Domain.Repositories.Interfaces.Books;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaLivro.Shared.Models;
using System.Reflection;

namespace SistemaLivro.Application.Test.UseCases.Publishers.Commands.Handlers;

public class DeletesPublisherCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IPublisherRepository> _mockPublisherRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<DeletesPublisherCommandHandler>> _mockLogger;
    private readonly DeletesPublisherCommandHandler _handler;

    public DeletesPublisherCommandHandlerTests()
    {
        _mockPublisherRepository = new Mock<IPublisherRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<DeletesPublisherCommandHandler>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.PublisherRepository).Returns(_mockPublisherRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new DeletesPublisherCommandHandler(
            _mockUnitOfWork.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    private Publisher CreateValidPublisher(Guid? id = null)
    {
        var publisherId = id ?? Guid.NewGuid();
        var publisher = new Publisher(
            id: publisherId, name: "Publisher Teste", description: "Descrição",
            createdAt: DateTime.UtcNow.AddDays(-10), updatedAt: null,
            deletedAt: null, isDeleted: false
        );

        return publisher;
    }

    [Fact(DisplayName = "Handle Deve Deletar Publisher com Sucesso para um ID Válido")]
    [Trait("Publisher", "Delete - Handler")]
    public async Task Handle_Should_DeletePublisher_When_IdIsValid()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new DeletesPublisherCommand(new List<Guid> { publisherId });
        var existingPublisher = CreateValidPublisher(publisherId);

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId)).ReturnsAsync(existingPublisher);
        _mockPublisherRepository.Setup(repo => repo.RemoveByIdAsync(publisherId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Publisher(s) deleted successfully", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.RemoveByIdAsync(publisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Deletar Publishers com Sucesso para Múltiplos IDs Válidos")]
    [Trait("Publisher", "Delete - Handler")]
    public async Task Handle_Should_DeletePublishers_When_MultipleIdsAreValid()
    {
        // Arrange
        var publisherId1 = Guid.NewGuid();
        var publisherId2 = Guid.NewGuid();
        var command = new DeletesPublisherCommand(new List<Guid> { publisherId1, publisherId2 });
        var existingPublisher1 = CreateValidPublisher(publisherId1);
        var existingPublisher2 = CreateValidPublisher(publisherId2);

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId1)).ReturnsAsync(existingPublisher1);
        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId2)).ReturnsAsync(existingPublisher2);
        _mockPublisherRepository.Setup(repo => repo.RemoveByIdAsync(It.IsIn(publisherId1, publisherId2))).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Publisher(s) deleted successfully", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId1), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId2), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.RemoveByIdAsync(publisherId1), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.RemoveByIdAsync(publisherId2), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Publisher Não For Encontrado")]
    [Trait("Publisher", "Delete - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_PublisherNotFound()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new DeletesPublisherCommand(new List<Guid> { publisherId });

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId)).Returns(Task.FromResult<Publisher?>(null));
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error deleting publisher(s)", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.RemoveByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Publisher with id {publisherId} not found") || v.ToString().Contains("Not found Publisher")), It.IsAny<NotFoundException>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Repositorio RemoveByIdAsync Falhar")]
    [Trait("Publisher", "Delete - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_RemoveByIdAsyncThrowsException()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new DeletesPublisherCommand(new List<Guid> { publisherId });
        var existingPublisher = CreateValidPublisher(publisherId);
        var dbException = new Exception("Database error during delete");

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId)).ReturnsAsync(existingPublisher);
        _mockPublisherRepository.Setup(repo => repo.RemoveByIdAsync(publisherId)).ThrowsAsync(dbException);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error deleting publisher(s)", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.RemoveByIdAsync(publisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting publisher(s)")), dbException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se UoW CommitAsync Falhar")]
    [Trait("Publisher", "Delete - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_CommitAsyncThrowsException()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new DeletesPublisherCommand(new List<Guid> { publisherId });
        var existingPublisher = CreateValidPublisher(publisherId);
        var commitException = new Exception("Commit transaction error");

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId)).ReturnsAsync(existingPublisher);
        _mockPublisherRepository.Setup(repo => repo.RemoveByIdAsync(publisherId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(commitException);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error deleting publisher(s)", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.RemoveByIdAsync(publisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
          x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting publisher(s)")), commitException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Mediator Publish Falhar")]
    [Trait("Publisher", "Delete - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_PublishThrowsException()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new DeletesPublisherCommand(new List<Guid> { publisherId });
        var existingPublisher = CreateValidPublisher(publisherId);
        var mediatorException = new Exception("Mediator publish error");

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId)).ReturnsAsync(existingPublisher);
        _mockPublisherRepository.Setup(repo => repo.RemoveByIdAsync(publisherId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(mediatorException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error deleting publisher(s)", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.RemoveByIdAsync(publisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting publisher(s)")), mediatorException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}