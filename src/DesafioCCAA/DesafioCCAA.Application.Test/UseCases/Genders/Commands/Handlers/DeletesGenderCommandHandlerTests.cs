using DesafioCCAA.Application.UseCases.Genders.Commands;
using DesafioCCAA.Application.UseCases.Genders.Commands.Handlers;
using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Domain.Repositories.Interfaces.Books;
using DesafioCCAA.Shared.Models;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace DesafioCCAA.Application.Test.UseCases.Genders.Commands.Handlers;

public class DeletesGenderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IGenderRepository> _mockGenderRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<DeletesGenderCommandHandler>> _mockLogger;
    private readonly DeletesGenderCommandHandler _handler;

    public DeletesGenderCommandHandlerTests()
    {
        _mockGenderRepository = new Mock<IGenderRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<DeletesGenderCommandHandler>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.GenderRepository).Returns(_mockGenderRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new DeletesGenderCommandHandler(
            _mockUnitOfWork.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    private Gender CreateValidGender(Guid? id = null)
    {
        var genderId = id ?? Guid.NewGuid();
        var gender = new Gender(
            id: genderId, name: "Gênero para Teste", description: "Descrição",
            createdAt: DateTime.UtcNow.AddDays(-10), updatedAt: null,
            deletedAt: null, isDeleted: false
        );

        try
        {
            var eventsListField = gender.GetType().BaseType?.BaseType?.GetField("_events", BindingFlags.NonPublic | BindingFlags.Instance);
            if (eventsListField != null) { (eventsListField.GetValue(gender) as List<IDomainEvent>)?.Clear(); }
        }
        catch { }
        return gender;
    }

    [Fact(DisplayName = "Handle Deve Deletar Gender com Sucesso para um ID Válido")]
    [Trait("Gender", "Delete - Handler")]
    public async Task Handle_Should_DeleteGender_When_IdIsValid()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var command = new DeletesGenderCommand(new List<Guid> { genderId });
        var existingGender = CreateValidGender(genderId);

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId)).ReturnsAsync(existingGender);
        _mockGenderRepository.Setup(repo => repo.RemoveByIdAsync(genderId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Gender(s) deleted successfully");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
        _mockGenderRepository.Verify(repo => repo.RemoveByIdAsync(genderId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Deletar Genders com Sucesso para Múltiplos IDs Válidos")]
    [Trait("Gender", "Delete - Handler")]
    public async Task Handle_Should_DeleteGenders_When_MultipleIdsAreValid()
    {
        // Arrange
        var genderId1 = Guid.NewGuid();
        var genderId2 = Guid.NewGuid();
        var command = new DeletesGenderCommand(new List<Guid> { genderId1, genderId2 });
        var existingGender1 = CreateValidGender(genderId1);
        var existingGender2 = CreateValidGender(genderId2);

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId1)).ReturnsAsync(existingGender1);
        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId2)).ReturnsAsync(existingGender2);
        _mockGenderRepository.Setup(repo => repo.RemoveByIdAsync(It.IsIn(genderId1, genderId2))).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Gender(s) deleted successfully");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId1), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId2), Times.Once);
        _mockGenderRepository.Verify(repo => repo.RemoveByIdAsync(genderId1), Times.Once);
        _mockGenderRepository.Verify(repo => repo.RemoveByIdAsync(genderId2), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }


    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Gender Não For Encontrado")]
    [Trait("Gender", "Delete - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_GenderNotFound()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var command = new DeletesGenderCommand(new List<Guid> { genderId });

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId)).Returns(Task.FromResult<Gender?>(null));
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error deleting Gender(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.RemoveByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Gender with id {genderId} not found") || v.ToString().Contains("Not found Gender")), It.IsAny<NotFoundException>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Repositorio RemoveByIdAsync Falhar")]
    [Trait("Gender", "Delete - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_RemoveByIdAsyncThrowsException()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var command = new DeletesGenderCommand(new List<Guid> { genderId });
        var existingGender = CreateValidGender(genderId);
        var dbException = new Exception("Database error during delete");

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId)).ReturnsAsync(existingGender);
        _mockGenderRepository.Setup(repo => repo.RemoveByIdAsync(genderId)).ThrowsAsync(dbException);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error deleting Gender(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
        _mockGenderRepository.Verify(repo => repo.RemoveByIdAsync(genderId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting Gender(s)")), dbException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se UoW CommitAsync Falhar")]
    [Trait("Gender", "Delete - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_CommitAsyncThrowsException()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var command = new DeletesGenderCommand(new List<Guid> { genderId });
        var existingGender = CreateValidGender(genderId);
        var commitException = new Exception("Commit transaction error");

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId)).ReturnsAsync(existingGender);
        _mockGenderRepository.Setup(repo => repo.RemoveByIdAsync(genderId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(commitException);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error deleting Gender(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
        _mockGenderRepository.Verify(repo => repo.RemoveByIdAsync(genderId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
          x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting Gender(s)")), commitException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Mediator Publish Falhar")]
    [Trait("Gender", "Delete - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_PublishThrowsException()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var command = new DeletesGenderCommand(new List<Guid> { genderId });
        var existingGender = CreateValidGender(genderId);
        var mediatorException = new Exception("Mediator publish error");

        var dummyEvent = new Mock<IDomainEvent>().Object;
        try
        {
            existingGender.GetType().GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance)?
                 .Invoke(existingGender, new object[] { dummyEvent });
        }
        catch { }


        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId)).ReturnsAsync(existingGender);
        _mockGenderRepository.Setup(repo => repo.RemoveByIdAsync(genderId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(mediatorException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error deleting Gender(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
        _mockGenderRepository.Verify(repo => repo.RemoveByIdAsync(genderId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting Gender(s)")), mediatorException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}