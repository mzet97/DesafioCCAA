using DesafioCCAA.Application.UseCases.Publishers.Commands;
using DesafioCCAA.Application.UseCases.Publishers.Commands.Handlers;
using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Domain.Repositories.Interfaces.Books;
using DesafioCCAA.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace DesafioCCAA.Application.Test.UseCases.Publishers.Commands.Handlers;

public class UpdatePublisherCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IPublisherRepository> _mockPublisherRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<UpdatePublisherCommandHandler>> _mockLogger;
    private readonly UpdatePublisherCommandHandler _handler;

    public UpdatePublisherCommandHandlerTests()
    {
        _mockPublisherRepository = new Mock<IPublisherRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<UpdatePublisherCommandHandler>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.PublisherRepository).Returns(_mockPublisherRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new UpdatePublisherCommandHandler(
            _mockUnitOfWork.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    private Publisher CreateExistingPublisher(Guid? id = null)
    {
        var publisherId = id ?? Guid.NewGuid();
        var publisher = new Publisher(
            id: publisherId, name: "Editora Existente", description: "Descrição Original",
            createdAt: DateTime.UtcNow.AddDays(-20), updatedAt: DateTime.UtcNow.AddDays(-5),
            deletedAt: null, isDeleted: false
        );
        var eventsListField = publisher.GetType().BaseType?.BaseType?.GetField("_events", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventsListField != null) { (eventsListField.GetValue(publisher) as List<IDomainEvent>)?.Clear(); }
        return publisher;
    }

    private UpdatePublisherCommand CreateValidUpdateCommand(Guid id)
    {
        return new UpdatePublisherCommand
        {
            Id = id,
            Name = "Nome Atualizado",
            Description = "Descrição Atualizada"
        };
    }

    private void AddDummyUpdateEvent(Publisher entity)
    {
        var dummyEvent = new Mock<IDomainEvent>().Object;
        entity.GetType().GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance)?
              .Invoke(entity, new object[] { dummyEvent });
    }

    [Fact(DisplayName = "Handle Deve Atualizar Publisher e Retornar ID com Sucesso")]
    [Trait("Publisher", "Update - Handler")]
    public async Task Handle_Should_UpdatePublisherAndReturnId_When_CommandIsValid()
    {
        // Arrange
        var existingPublisher = CreateExistingPublisher();
        var command = CreateValidUpdateCommand(existingPublisher.Id);
        Publisher? capturedPublisher = null;

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync(existingPublisher);
        _mockPublisherRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Publisher>()))
            .Callback<Publisher>(p => capturedPublisher = p) 
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(existingPublisher.Id, result.Data);
        Assert.Equal("Publisher updated successfully", result.Message);
        Assert.NotNull(capturedPublisher);
        Assert.Equal(command.Name, capturedPublisher?.Name);
        Assert.Equal(command.Description, capturedPublisher?.Description);

        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(command.Id), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.UpdateAsync(It.Is<Publisher>(p => p.Id == command.Id)), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Never);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException se Publisher Não For Encontrado")]
    [Trait("Publisher", "Update - Handler")]
    public async Task Handle_Should_ThrowNotFoundException_When_PublisherNotFound()
    {
        // Arrange
        var command = CreateValidUpdateCommand(Guid.NewGuid());
        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(command.Id)).Returns(Task.FromResult<Publisher?>(null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(command.Id), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Not found Publisher")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockPublisherRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Publisher>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory(DisplayName = "Handle Deve Lançar ValidationException (Custom) se Dados Atualizados são Inválidos")]
    [Trait("Publisher", "Update - Handler - Validation")]
    [InlineData("", "Descrição Atualizada Válida")]
    [InlineData(null, "Descrição Atualizada Válida")]
    [InlineData("Nome Atualizado Muito Longo ... (mais de 150 caracteres)", "Descrição Atualizada Válida")]
    [InlineData("Nome Atualizado Válido", "Descrição Atualizada Muito Longa ... (mais de 4000 caracteres)")]
    public async Task Handle_Should_ThrowCustomValidationException_When_UpdatedDataIsInvalid(string invalidName, string invalidDescription)
    {
        // Arrange
        if (invalidName?.Length > 50 && invalidName.StartsWith("Nome Atualizado Muito Longo")) invalidName = new string('N', 151);
        if (invalidDescription?.Length > 50 && invalidDescription.StartsWith("Descrição Atualizada Muito Longa")) invalidDescription = new string('D', 4001);

        var existingPublisher = CreateExistingPublisher();
        var command = new UpdatePublisherCommand { Id = existingPublisher.Id, Name = invalidName, Description = invalidDescription };

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync(existingPublisher);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DesafioCCAA.Domain.Exceptions.ValidationException>(() => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Validate Publisher has error", exception.Message);

        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(command.Id), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Validate Publisher has error")), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockPublisherRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Publisher>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Repositorio UpdateAsync Falhar")]
    [Trait("Publisher", "Update - Handler - Failure")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_UpdateAsyncThrowsException()
    {
        // Arrange
        var existingPublisher = CreateExistingPublisher();
        var command = CreateValidUpdateCommand(existingPublisher.Id);
        var dbException = new Exception("Database error during update");

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync(existingPublisher);
        _mockPublisherRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Publisher>())).ThrowsAsync(dbException);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(existingPublisher.Id, result.Data);
        Assert.Equal("Error to update Publisher", result.Message);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(command.Id), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Publisher>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to update Publisher")), dbException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se UoW CommitAsync Falhar")]
    [Trait("Publisher", "Update - Handler - Failure")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_CommitAsyncThrowsException()
    {
        // Arrange
        var existingPublisher = CreateExistingPublisher();
        var command = CreateValidUpdateCommand(existingPublisher.Id);
        var commitException = new Exception("Commit transaction error");

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync(existingPublisher);
        _mockPublisherRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Publisher>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(commitException);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(existingPublisher.Id, result.Data);
        Assert.Equal("Error to update Publisher", result.Message);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(command.Id), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Publisher>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
          x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to update Publisher")), commitException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Mediator Publish Falhar")]
    [Trait("Publisher", "Update - Handler - Failure")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_PublishThrowsException()
    {
        // Arrange
        var existingPublisher = CreateExistingPublisher();
        var command = CreateValidUpdateCommand(existingPublisher.Id);
        var mediatorException = new Exception("Mediator publish error");

        AddDummyUpdateEvent(existingPublisher);

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync(existingPublisher);
        _mockPublisherRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Publisher>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(mediatorException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(existingPublisher.Id, result.Data);
        Assert.Equal("Error to update Publisher", result.Message);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(command.Id), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Publisher>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to update Publisher")), mediatorException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}