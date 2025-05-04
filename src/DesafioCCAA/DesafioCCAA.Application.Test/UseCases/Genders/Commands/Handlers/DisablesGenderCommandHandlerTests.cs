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

public class DisablesGenderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IGenderRepository> _mockGenderRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<DisablesGenderCommandHandler>> _mockLogger;
    private readonly DisablesGenderCommandHandler _handler;

    public DisablesGenderCommandHandlerTests()
    {
        _mockGenderRepository = new Mock<IGenderRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<DisablesGenderCommandHandler>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.GenderRepository).Returns(_mockGenderRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new DisablesGenderCommandHandler(
            _mockUnitOfWork.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    private Gender CreateActiveGender(Guid? id = null)
    {
        var genderId = id ?? Guid.NewGuid();
        var gender = new Gender(
            id: genderId, name: "Gênero Ativo", description: "Descrição",
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

    private void AddDummyEvent(Entity<Gender> entity, IDomainEvent domainEvent)
    {
        try
        {
            entity.GetType().GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance)?
                .Invoke(entity, new object[] { domainEvent });
        }
        catch { }
    }

    [Fact(DisplayName = "Handle Deve Desativar Gender com Sucesso para um ID Válido")]
    [Trait("Gender", "Disable - Handler")]
    public async Task Handle_Should_DisableGender_When_IdIsValid()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var command = new DisablesGenderCommand(new List<Guid> { genderId });
        var activeGender = CreateActiveGender(genderId);

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId)).ReturnsAsync(activeGender);
        _mockGenderRepository.Setup(repo => repo.DisableAsync(genderId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Gender(s) disabled successfully");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
        activeGender.IsDeleted.Should().BeTrue();
        activeGender.DeletedAt.Should().NotBeNull();
        _mockGenderRepository.Verify(repo => repo.DisableAsync(genderId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Desativar Genders com Sucesso para Múltiplos IDs Válidos")]
    [Trait("Gender", "Disable - Handler")]
    public async Task Handle_Should_DisableGenders_When_MultipleIdsAreValid()
    {
        // Arrange
        var genderId1 = Guid.NewGuid();
        var genderId2 = Guid.NewGuid();
        var command = new DisablesGenderCommand(new List<Guid> { genderId1, genderId2 });
        var activeGender1 = CreateActiveGender(genderId1);
        var activeGender2 = CreateActiveGender(genderId2);

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId1)).ReturnsAsync(activeGender1);
        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId2)).ReturnsAsync(activeGender2);
        _mockGenderRepository.Setup(repo => repo.DisableAsync(It.IsIn(genderId1, genderId2))).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Gender(s) disabled successfully");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId1), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId2), Times.Once);
        activeGender1.IsDeleted.Should().BeTrue();
        activeGender2.IsDeleted.Should().BeTrue();
        _mockGenderRepository.Verify(repo => repo.DisableAsync(genderId1), Times.Once);
        _mockGenderRepository.Verify(repo => repo.DisableAsync(genderId2), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Gender Não For Encontrado")]
    [Trait("Gender", "Disable - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_GenderNotFound()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var command = new DisablesGenderCommand(new List<Guid> { genderId });

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId)).Returns(Task.FromResult<Gender?>(null));
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error disabling Gender(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.DisableAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Gender with id {genderId} not found") || v.ToString().Contains("Not found Gender")), It.IsAny<NotFoundException>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Repositorio DisableAsync Falhar")]
    [Trait("Gender", "Disable - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_DisableAsyncThrowsException()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var command = new DisablesGenderCommand(new List<Guid> { genderId });
        var activeGender = CreateActiveGender(genderId);
        var dbException = new Exception("Database error during disable");

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId)).ReturnsAsync(activeGender);
        _mockGenderRepository.Setup(repo => repo.DisableAsync(genderId)).ThrowsAsync(dbException);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error disabling Gender(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
        _mockGenderRepository.Verify(repo => repo.DisableAsync(genderId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error disabling Gender(s)")), dbException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se UoW CommitAsync Falhar")]
    [Trait("Gender", "Disable - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_CommitAsyncThrowsException()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var command = new DisablesGenderCommand(new List<Guid> { genderId });
        var activeGender = CreateActiveGender(genderId);
        var commitException = new Exception("Commit transaction error");

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId)).ReturnsAsync(activeGender);
        _mockGenderRepository.Setup(repo => repo.DisableAsync(genderId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(commitException);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error disabling Gender(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
        _mockGenderRepository.Verify(repo => repo.DisableAsync(genderId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
          x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error disabling Gender(s)")), commitException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Mediator Publish Falhar")]
    [Trait("Gender", "Disable - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_PublishThrowsException()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var command = new DisablesGenderCommand(new List<Guid> { genderId });
        var activeGender = CreateActiveGender(genderId);
        var mediatorException = new Exception("Mediator publish error");

        var dummyEvent = new Mock<IDomainEvent>().Object;
        try
        {
            activeGender.GetType().GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance)?
                 .Invoke(activeGender, new object[] { dummyEvent });
        }
        catch { }

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId)).ReturnsAsync(activeGender);
        _mockGenderRepository.Setup(repo => repo.DisableAsync(genderId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(mediatorException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error disabling Gender(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
        _mockGenderRepository.Verify(repo => repo.DisableAsync(genderId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error disabling Gender(s)")), mediatorException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}