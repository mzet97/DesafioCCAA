using SistemaLivro.Application.UseCases.Publishers.Commands.Handlers;
using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Domain.Repositories.Interfaces.Books;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Shared.Responses;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SistemaLivro.Application.UseCases.Publishers.Commands;
using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Shared.Models;

namespace SistemaLivro.Application.Test.UseCases.Publishers.Commands.Handlers;

public class DisablesPublisherCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IPublisherRepository> _mockPublisherRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<DisablesPublisherCommandHandler>> _mockLogger;
    private readonly DisablesPublisherCommandHandler _handler;

    public DisablesPublisherCommandHandlerTests()
    {
        _mockPublisherRepository = new Mock<IPublisherRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<DisablesPublisherCommandHandler>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.PublisherRepository).Returns(_mockPublisherRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new DisablesPublisherCommandHandler(
            _mockUnitOfWork.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    private Publisher CreateActivePublisher(Guid? id = null)
    {
        var publisherId = id ?? Guid.NewGuid();
        var publisher = new Publisher(
            id: publisherId, name: "Publisher Teste Ativo", description: "Descrição Ativa",
            createdAt: DateTime.UtcNow.AddDays(-10), updatedAt: null,
            deletedAt: null, isDeleted: false
        );
        var eventsListField = publisher.GetType().BaseType?.BaseType?.GetField("_events", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventsListField != null) { (eventsListField.GetValue(publisher) as List<IDomainEvent>)?.Clear(); }
        return publisher;
    }

    [Fact(DisplayName = "Handle Deve Desativar Publisher com Sucesso para um ID Válido")]
    [Trait("Publisher", "Disable - Handler")]
    public async Task Handle_Should_DisablePublisher_When_IdIsValid()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new DisablesPublisherCommand(new List<Guid> { publisherId });
        var activePublisher = CreateActivePublisher(publisherId);

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId)).ReturnsAsync(activePublisher);
        _mockPublisherRepository.Setup(repo => repo.DisableAsync(publisherId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Publisher(s) disabled successfully", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId), Times.Once);
        Assert.True(activePublisher.IsDeleted);
        Assert.NotNull(activePublisher.DeletedAt);
        _mockPublisherRepository.Verify(repo => repo.DisableAsync(publisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Desativar Publishers com Sucesso para Múltiplos IDs Válidos")]
    [Trait("Publisher", "Disable - Handler")]
    public async Task Handle_Should_DisablePublishers_When_MultipleIdsAreValid()
    {
        // Arrange
        var publisherId1 = Guid.NewGuid();
        var publisherId2 = Guid.NewGuid();
        var command = new DisablesPublisherCommand(new List<Guid> { publisherId1, publisherId2 });
        var activePublisher1 = CreateActivePublisher(publisherId1);
        var activePublisher2 = CreateActivePublisher(publisherId2);

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId1)).ReturnsAsync(activePublisher1);
        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId2)).ReturnsAsync(activePublisher2);
        _mockPublisherRepository.Setup(repo => repo.DisableAsync(It.IsIn(publisherId1, publisherId2))).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Publisher(s) disabled successfully", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId1), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId2), Times.Once);
        Assert.True(activePublisher1.IsDeleted);
        Assert.True(activePublisher2.IsDeleted);
        _mockPublisherRepository.Verify(repo => repo.DisableAsync(publisherId1), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.DisableAsync(publisherId2), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Publisher Não For Encontrado")]
    [Trait("Publisher", "Disable - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_PublisherNotFound()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new DisablesPublisherCommand(new List<Guid> { publisherId });

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId)).Returns(Task.FromResult<Publisher?>(null));
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error disabling publisher(s)", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.DisableAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Publisher with id {publisherId} not found") || v.ToString().Contains("Not found Publisher")), It.IsAny<NotFoundException>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Repositorio DisableAsync Falhar")]
    [Trait("Publisher", "Disable - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_DisableAsyncThrowsException()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new DisablesPublisherCommand(new List<Guid> { publisherId });
        var activePublisher = CreateActivePublisher(publisherId);
        var dbException = new Exception("Database error during disable");

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId)).ReturnsAsync(activePublisher);
        _mockPublisherRepository.Setup(repo => repo.DisableAsync(publisherId)).ThrowsAsync(dbException);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error disabling publisher(s)", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.DisableAsync(publisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error disabling publisher(s)")), dbException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se UoW CommitAsync Falhar")]
    [Trait("Publisher", "Disable - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_CommitAsyncThrowsException()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new DisablesPublisherCommand(new List<Guid> { publisherId });
        var activePublisher = CreateActivePublisher(publisherId);
        var commitException = new Exception("Commit transaction error");

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId)).ReturnsAsync(activePublisher);
        _mockPublisherRepository.Setup(repo => repo.DisableAsync(publisherId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(commitException);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error disabling publisher(s)", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.DisableAsync(publisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
          x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error disabling publisher(s)")), commitException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Mediator Publish Falhar")]
    [Trait("Publisher", "Disable - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_PublishThrowsException()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new DisablesPublisherCommand(new List<Guid> { publisherId });
        var activePublisher = CreateActivePublisher(publisherId);
        var mediatorException = new Exception("Mediator publish error");

        var dummyEvent = new Mock<IDomainEvent>().Object;
        activePublisher.GetType().GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance)?
              .Invoke(activePublisher, new object[] { dummyEvent });

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId)).ReturnsAsync(activePublisher);
        _mockPublisherRepository.Setup(repo => repo.DisableAsync(publisherId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(mediatorException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Error disabling publisher(s)", result.Message);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.DisableAsync(publisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error disabling publisher(s)")), mediatorException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}