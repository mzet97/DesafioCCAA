using DesafioCCAA.Application.UseCases.Publishers.Commands;
using DesafioCCAA.Application.UseCases.Publishers.Commands.Handlers;
using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Domain.Domains.Books.ValueObjects;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Domain.Repositories.Interfaces.Books;
using DesafioCCAA.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace DesafioCCAA.Application.Test.UseCases.Publishers.Commands.Handlers;

public class CreatePublisherCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IPublisherRepository> _mockPublisherRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<CreatePublisherCommandHandler>> _mockLogger;
    private readonly CreatePublisherCommandHandler _handler;

    public CreatePublisherCommandHandlerTests()
    {
        _mockPublisherRepository = new Mock<IPublisherRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<CreatePublisherCommandHandler>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.PublisherRepository).Returns(_mockPublisherRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new CreatePublisherCommandHandler(
            _mockUnitOfWork.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    private CreatePublisherCommand CreateValidCommand()
    {
        return new CreatePublisherCommand
        {
            Name = "Nova Editora Válida",
            Description = "Descrição da nova editora."
        };
    }

    private CoverImage CreateValidCoverImage()
    {
        return new CoverImage("valid_image.png", "/path/to/valid_image.png");
    }

    [Fact(DisplayName = "Handle Deve Criar Publisher e Retornar ID com Sucesso")]
    [Trait("Publisher", "Create - Handler")]
    public async Task Handle_Should_CreatePublisherAndReturnId_When_CommandIsValid()
    {
        // Arrange
        var command = CreateValidCommand();
        Publisher? capturedPublisher = null;

        _mockPublisherRepository.Setup(repo => repo.AddAsync(It.IsAny<Publisher>()))
            .Callback<Publisher>(p => capturedPublisher = p)
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotEqual(Guid.Empty, result.Data);
        Assert.NotNull(capturedPublisher);
        Assert.Equal(command.Name, capturedPublisher?.Name);
        Assert.Equal(command.Description, capturedPublisher?.Description);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.AddAsync(It.IsAny<Publisher>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Never);
    }

    [Theory(DisplayName = "Handle Deve Lançar ValidationException (Custom) se Validação da Entidade Falhar (MaxLength)")]
    [Trait("Publisher", "Create - Handler - Validation")]
    [InlineData("Nome Muito Longo Para Ser Válido ... (mais de 150 caracteres)", "Descrição Válida")]
    [InlineData("Nome Válido", "Descrição Muito Longa ... (mais de 4000 caracteres)")]
    public async Task Handle_Should_ThrowCustomValidationException_When_EntityValidationFails_MaxLength(string name, string description)
    {
        // Arrange
        if (name.Length > 50 && name.StartsWith("Nome Muito Longo")) name = new string('N', 151);
        if (description.Length > 50 && description.StartsWith("Descrição Muito Longa")) description = new string('D', 4001);
        var command = new CreatePublisherCommand { Name = name, Description = description };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DesafioCCAA.Domain.Exceptions.ValidationException>(() => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("Validate Publisher has error", exception.Message);

        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Validate Publisher has error")), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockPublisherRepository.Verify(repo => repo.AddAsync(It.IsAny<Publisher>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Handle Deve Lançar Exceção e Rollback se Repositorio AddAsync Falhar")]
    [Trait("Publisher", "Create - Handler - Failure")]
    public async Task Handle_Should_ThrowExceptionAndRollback_When_AddAsyncThrowsException()
    {
        // Arrange
        var command = CreateValidCommand();
        var dbException = new Exception("Database save error");

        _mockPublisherRepository.Setup(repo => repo.AddAsync(It.IsAny<Publisher>())).ThrowsAsync(dbException);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("Error to create Publisher", exception.Message);
        Assert.Same(dbException, exception.InnerException);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.AddAsync(It.IsAny<Publisher>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to create Publisher")), dbException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar Exceção e Rollback se UoW CommitAsync Falhar")]
    [Trait("Publisher", "Create - Handler - Failure")]
    public async Task Handle_Should_ThrowExceptionAndRollback_When_CommitAsyncThrowsException()
    {
        // Arrange
        var command = CreateValidCommand();
        var commitException = new Exception("Commit transaction error");

        _mockPublisherRepository.Setup(repo => repo.AddAsync(It.IsAny<Publisher>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(commitException);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("Error to create Publisher", exception.Message);
        Assert.Same(commitException, exception.InnerException);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.AddAsync(It.IsAny<Publisher>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
          x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to create Publisher")), commitException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar Exceção e Rollback se Mediator Publish Falhar")]
    [Trait("Publisher", "Create - Handler - Failure")]
    public async Task Handle_Should_ThrowExceptionAndRollback_When_PublishThrowsException()
    {
        // Arrange
        var command = CreateValidCommand();
        var mediatorException = new Exception("Mediator publish error");
        Publisher? capturedPublisher = null;

        _mockPublisherRepository.Setup(repo => repo.AddAsync(It.IsAny<Publisher>()))
            .Callback<Publisher>(p => {
                capturedPublisher = p;
                var dummyEvent = new Mock<IDomainEvent>().Object;
                p.GetType().GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance)?
                          .Invoke(p, new object[] { dummyEvent });
            })
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(mediatorException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("Error to create Publisher", exception.Message);
        Assert.Same(mediatorException, exception.InnerException);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisherRepository.Verify(repo => repo.AddAsync(It.IsAny<Publisher>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to create Publisher")), mediatorException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}