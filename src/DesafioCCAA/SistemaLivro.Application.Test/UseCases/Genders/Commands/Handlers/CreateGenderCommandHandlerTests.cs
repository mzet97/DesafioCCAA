using SistemaLivro.Application.UseCases.Genders.Commands;
using SistemaLivro.Application.UseCases.Genders.Commands.Handlers;
using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Domain.Repositories.Interfaces.Books;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaLivro.Shared.Models;
using System.Reflection;

namespace SistemaLivro.Application.Test.UseCases.Genders.Commands.Handlers;

public class CreateGenderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IGenderRepository> _mockGenderRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<CreateGenderCommandHandler>> _mockLogger;
    private readonly CreateGenderCommandHandler _handler;

    public CreateGenderCommandHandlerTests()
    {
        _mockGenderRepository = new Mock<IGenderRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<CreateGenderCommandHandler>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.GenderRepository).Returns(_mockGenderRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new CreateGenderCommandHandler(
            _mockUnitOfWork.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    private CreateGenderCommand CreateValidCommand()
    {
        return new CreateGenderCommand
        {
            Name = "Novo Gênero Válido",
            Description = "Descrição do novo gênero."
        };
    }

    [Fact(DisplayName = "Handle Deve Criar Gender e Retornar ID com Sucesso")]
    [Trait("Gender", "Create - Handler")]
    public async Task Handle_Should_CreateGenderAndReturnId_When_CommandIsValid()
    {
        // Arrange
        var command = CreateValidCommand();
        Gender? capturedGender = null;

        _mockGenderRepository.Setup(repo => repo.AddAsync(It.IsAny<Gender>()))
            .Callback<Gender>(g => capturedGender = g)
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        capturedGender.Should().NotBeNull();
        capturedGender?.Name.Should().Be(command.Name);
        capturedGender?.Description.Should().Be(command.Description);

        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.AddAsync(It.IsAny<Gender>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Never);
    }

    [Theory(DisplayName = "Handle Deve Lançar ValidationException (Custom) se Dados Inválidos para Entidade Gender")]
    [Trait("Gender", "Create - Handler - Validation")]
    [InlineData("Nome Gênero Muito Longo Para Ser Válido ... (mais de 150 caracteres)", "Descrição Válida")]
    [InlineData("Nome Válido", "Descrição Gênero Muito Longa ... (mais de 4000 caracteres)")]
    public async Task Handle_Should_ThrowCustomValidationException_When_EntityValidationFails(string name, string description)
    {
        // Arrange
        if (name.Length > 50 && name.StartsWith("Nome Gênero Muito Longo")) name = new string('N', 151);
        if (description.Length > 50 && description.StartsWith("Descrição Gênero Muito Longa")) description = new string('D', 4001);
        var command = new CreateGenderCommand { Name = name, Description = description };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<SistemaLivro.Domain.Exceptions.ValidationException>(() => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Validate Gender has error");

        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Validate Gender has error")), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockGenderRepository.Verify(repo => repo.AddAsync(It.IsAny<Gender>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Handle Deve Lançar Exceção e Rollback se Repositorio AddAsync Falhar (Gender)")]
    [Trait("Gender", "Create - Handler - Failure")]
    public async Task Handle_Should_ThrowExceptionAndRollback_When_AddAsyncThrowsException()
    {
        // Arrange
        var command = CreateValidCommand();
        var dbException = new Exception("Database save error");

        _mockGenderRepository.Setup(repo => repo.AddAsync(It.IsAny<Gender>())).ThrowsAsync(dbException);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Error to create Gender");
        exception.InnerException.Should().BeSameAs(dbException);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.AddAsync(It.IsAny<Gender>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to create Gender")), dbException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar Exceção e Rollback se UoW CommitAsync Falhar (Gender)")]
    [Trait("Gender", "Create - Handler - Failure")]
    public async Task Handle_Should_ThrowExceptionAndRollback_When_CommitAsyncThrowsException()
    {
        // Arrange
        var command = CreateValidCommand();
        var commitException = new Exception("Commit transaction error");

        _mockGenderRepository.Setup(repo => repo.AddAsync(It.IsAny<Gender>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(commitException);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Error to create Gender");
        exception.InnerException.Should().BeSameAs(commitException);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.AddAsync(It.IsAny<Gender>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
          x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to create Gender")), commitException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar Exceção e Rollback se Mediator Publish Falhar (Gender)")]
    [Trait("Gender", "Create - Handler - Failure")]
    public async Task Handle_Should_ThrowExceptionAndRollback_When_PublishThrowsException()
    {
        // Arrange
        var command = CreateValidCommand();
        var mediatorException = new Exception("Mediator publish error");
        Gender? capturedGender = null;

        _mockGenderRepository.Setup(repo => repo.AddAsync(It.IsAny<Gender>()))
            .Callback<Gender>(g => {
                capturedGender = g;
                var dummyEvent = new Mock<IDomainEvent>().Object;
                g.GetType().GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance)?
                          .Invoke(g, new object[] { dummyEvent });
            })
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(mediatorException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Error to create Gender");
        exception.InnerException.Should().BeSameAs(mediatorException);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.AddAsync(It.IsAny<Gender>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to create Gender")), mediatorException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}