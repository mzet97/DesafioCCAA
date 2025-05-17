using SistemaLivro.Application.UseCases.Genders.Commands;
using SistemaLivro.Application.UseCases.Genders.Commands.Handlers;
using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Domain.Repositories.Interfaces.Books;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaLivro.Shared.Models;
using System.Reflection;

namespace SistemaLivro.Application.Test.UseCases.Genders.Commands.Handlers;

public class AtivesGenderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IGenderRepository> _mockGenderRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<AtivesGenderCommandHandler>> _mockLogger;
    private readonly AtivesGenderCommandHandler _handler;

    public AtivesGenderCommandHandlerTests()
    {
        _mockGenderRepository = new Mock<IGenderRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<AtivesGenderCommandHandler>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.GenderRepository).Returns(_mockGenderRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new AtivesGenderCommandHandler(
            _mockUnitOfWork.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    private Gender CreateDisabledGender(Guid? id = null)
    {
        var genderId = id ?? Guid.NewGuid();
        var gender = new Gender(
            id: genderId, name: "Gênero Desativado", description: "Descrição",
            createdAt: DateTime.UtcNow.AddDays(-10), updatedAt: DateTime.UtcNow.AddDays(-5),
            deletedAt: DateTime.UtcNow.AddDays(-1), isDeleted: true
        );

        try
        {
            var eventsListField = gender.GetType().BaseType?.BaseType?.GetField("_events", BindingFlags.NonPublic | BindingFlags.Instance);
            if (eventsListField != null) { (eventsListField.GetValue(gender) as List<IDomainEvent>)?.Clear(); }
        }
        catch { }
        return gender;
    }


    [Fact(DisplayName = "Handle Deve Ativar Gender com Sucesso para um ID Válido")]
    [Trait("Gender", "Activate - Handler")]
    public async Task Handle_Should_ActivateGender_When_IdIsValid()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var command = new AtivesGenderCommand(new List<Guid> { genderId });
        var disabledGender = CreateDisabledGender(genderId);

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId)).ReturnsAsync(disabledGender);
        _mockGenderRepository.Setup(repo => repo.ActiveAsync(genderId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Gender(s) actived successfully");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
        disabledGender.IsDeleted.Should().BeFalse();
        disabledGender.DeletedAt.Should().BeNull();
        _mockGenderRepository.Verify(repo => repo.ActiveAsync(genderId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Ativar Genders com Sucesso para Múltiplos IDs Válidos")]
    [Trait("Gender", "Activate - Handler")]
    public async Task Handle_Should_ActivateGenders_When_MultipleIdsAreValid()
    {
        // Arrange
        var genderId1 = Guid.NewGuid();
        var genderId2 = Guid.NewGuid();
        var command = new AtivesGenderCommand(new List<Guid> { genderId1, genderId2 });
        var disabledGender1 = CreateDisabledGender(genderId1);
        var disabledGender2 = CreateDisabledGender(genderId2);

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId1)).ReturnsAsync(disabledGender1);
        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId2)).ReturnsAsync(disabledGender2);
        _mockGenderRepository.Setup(repo => repo.ActiveAsync(It.IsIn(genderId1, genderId2))).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Gender(s) actived successfully");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId1), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId2), Times.Once);
        disabledGender1.IsDeleted.Should().BeFalse();
        disabledGender2.IsDeleted.Should().BeFalse();
        _mockGenderRepository.Verify(repo => repo.ActiveAsync(genderId1), Times.Once);
        _mockGenderRepository.Verify(repo => repo.ActiveAsync(genderId2), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }


    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Gender Não For Encontrado")]
    [Trait("Gender", "Activate - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_GenderNotFound()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var command = new AtivesGenderCommand(new List<Guid> { genderId });

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId)).Returns(Task.FromResult<Gender?>(null));
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error activating Gender(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.ActiveAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Gender with id {genderId} not found") || v.ToString().Contains("Not found Gender")), It.IsAny<NotFoundException>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Repositorio ActiveAsync Falhar")]
    [Trait("Gender", "Activate - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_ActiveAsyncThrowsException()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var command = new AtivesGenderCommand(new List<Guid> { genderId });
        var disabledGender = CreateDisabledGender(genderId);
        var dbException = new Exception("Database error during activation");

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId)).ReturnsAsync(disabledGender);
        _mockGenderRepository.Setup(repo => repo.ActiveAsync(genderId)).ThrowsAsync(dbException);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error activating Gender(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
        _mockGenderRepository.Verify(repo => repo.ActiveAsync(genderId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error activating Gender(s)")), dbException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se UoW CommitAsync Falhar")]
    [Trait("Gender", "Activate - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_CommitAsyncThrowsException()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var command = new AtivesGenderCommand(new List<Guid> { genderId });
        var disabledGender = CreateDisabledGender(genderId);
        var commitException = new Exception("Commit transaction error");

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId)).ReturnsAsync(disabledGender);
        _mockGenderRepository.Setup(repo => repo.ActiveAsync(genderId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(commitException);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error activating Gender(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
        _mockGenderRepository.Verify(repo => repo.ActiveAsync(genderId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
          x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error activating Gender(s)")), commitException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Mediator Publish Falhar")]
    [Trait("Gender", "Activate - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_PublishThrowsException()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var command = new AtivesGenderCommand(new List<Guid> { genderId });
        var disabledGender = CreateDisabledGender(genderId);
        var mediatorException = new Exception("Mediator publish error");

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId)).ReturnsAsync(disabledGender);
        _mockGenderRepository.Setup(repo => repo.ActiveAsync(genderId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(mediatorException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error activating Gender(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
        _mockGenderRepository.Verify(repo => repo.ActiveAsync(genderId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error activating Gender(s)")), mediatorException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}