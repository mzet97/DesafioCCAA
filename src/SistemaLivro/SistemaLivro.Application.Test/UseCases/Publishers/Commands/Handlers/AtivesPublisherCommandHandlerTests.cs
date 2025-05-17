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

public class AtivesPublisherCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IPublisherRepository> _mockPublisherRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<AtivesPublisherCommandHandler>> _mockLogger;
    private readonly AtivesPublisherCommandHandler _handler;

    public AtivesPublisherCommandHandlerTests()
    {
        _mockPublisherRepository = new Mock<IPublisherRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<AtivesPublisherCommandHandler>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.PublisherRepository).Returns(_mockPublisherRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new AtivesPublisherCommandHandler(
            _mockUnitOfWork.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    private Publisher CreateDisabledPublisher(Guid? id = null)
    {
        var publisherId = id ?? Guid.NewGuid();
        var publisher = new Publisher(
            id: publisherId, name: "Publisher Teste Desativado", description: "Descrição para teste",
            createdAt: DateTime.UtcNow.AddDays(-10), updatedAt: DateTime.UtcNow.AddDays(-5),
            deletedAt: DateTime.UtcNow.AddDays(-1), isDeleted: true
        );

        var eventsListField = publisher.GetType().BaseType?.BaseType?.GetField("_events", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventsListField != null)
        {
            var eventsList = eventsListField.GetValue(publisher) as List<IDomainEvent>;
            eventsList?.Clear();

            var dummyEvent = new Mock<IDomainEvent>().Object;
            publisher.GetType().GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance)?
                  .Invoke(publisher, new object[] { dummyEvent });
        }

        return publisher;
    }


    [Fact(DisplayName = "Handle Deve Ativar Publisher com Sucesso para um ID Válido")]
    [Trait("Publisher", "Activate - Handler")]
    public async Task Handle_Should_ActivatePublisher_When_IdIsValid()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new AtivesPublisherCommand(new List<Guid> { publisherId });
        var disabledPublisher = CreateDisabledPublisher(publisherId);

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId)).ReturnsAsync(disabledPublisher);
        _mockPublisherRepository.Setup(repo => repo.ActiveAsync(publisherId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Publisher(s) actived successfully", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId), Times.Once);
        Assert.False(disabledPublisher.IsDeleted);
        Assert.Null(disabledPublisher.DeletedAt);
        _mockPublisherRepository.Verify(repo => repo.ActiveAsync(publisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Ativar Publishers com Sucesso para Múltiplos IDs Válidos")]
    [Trait("Publisher", "Activate - Handler")]
    public async Task Handle_Should_ActivatePublishers_When_MultipleIdsAreValid()
    {
        // Arrange
        var publisherId1 = Guid.NewGuid();
        var publisherId2 = Guid.NewGuid();
        var command = new AtivesPublisherCommand(new List<Guid> { publisherId1, publisherId2 });
        var disabledPublisher1 = CreateDisabledPublisher(publisherId1);
        var disabledPublisher2 = CreateDisabledPublisher(publisherId2);

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId1)).ReturnsAsync(disabledPublisher1);
        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId2)).ReturnsAsync(disabledPublisher2);
        _mockPublisherRepository.Setup(repo => repo.ActiveAsync(It.IsIn(publisherId1, publisherId2))).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Publisher(s) actived successfully", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId1), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId2), Times.Once);
        Assert.False(disabledPublisher1.IsDeleted);
        Assert.False(disabledPublisher2.IsDeleted);
        _mockPublisherRepository.Verify(repo => repo.ActiveAsync(publisherId1), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.ActiveAsync(publisherId2), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }


    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Publisher Não For Encontrado")]
    [Trait("Publisher", "Activate - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_PublisherNotFound()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new AtivesPublisherCommand(new List<Guid> { publisherId });

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId))
                                .Returns(Task.FromResult<Publisher?>(null));
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error activating publisher(s)", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockPublisherRepository.Verify(repo => repo.ActiveAsync(It.IsAny<Guid>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Publisher with id {publisherId} not found") || v.ToString().Contains("Not found Publisher")), It.IsAny<NotFoundException>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Repositorio ActiveAsync Falhar")]
    [Trait("Publisher", "Activate - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_ActiveAsyncThrowsException()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new AtivesPublisherCommand(new List<Guid> { publisherId });
        var disabledPublisher = CreateDisabledPublisher(publisherId);
        var dbException = new InvalidOperationException("Database error during activation");

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId)).ReturnsAsync(disabledPublisher);
        _mockPublisherRepository.Setup(repo => repo.ActiveAsync(publisherId)).ThrowsAsync(dbException);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error activating publisher(s)", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.ActiveAsync(publisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error activating publisher(s)")), dbException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Mediator Publish Falhar")]
    [Trait("Publisher", "Activate - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_MediatorPublishThrowsException()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new AtivesPublisherCommand(new List<Guid> { publisherId });
        var disabledPublisher = CreateDisabledPublisher(publisherId);
        var mediatorException = new InvalidOperationException("Mediator error");

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId)).ReturnsAsync(disabledPublisher);
        _mockPublisherRepository.Setup(repo => repo.ActiveAsync(publisherId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(mediatorException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error activating publisher(s)", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.ActiveAsync(publisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error activating publisher(s)")), mediatorException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}