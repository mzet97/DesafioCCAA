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

public class UpdateGenderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IGenderRepository> _mockGenderRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<UpdateGenderCommandHandler>> _mockLogger;
    private readonly UpdateGenderCommandHandler _handler;

    public UpdateGenderCommandHandlerTests()
    {
        _mockGenderRepository = new Mock<IGenderRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<UpdateGenderCommandHandler>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.GenderRepository).Returns(_mockGenderRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new UpdateGenderCommandHandler(
            _mockUnitOfWork.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    private Gender CreateExistingGender(Guid? id = null)
    {
        var genderId = id ?? Guid.NewGuid();
        var gender = new Gender(
            id: genderId, name: "Gênero Original", description: "Descrição Original",
            createdAt: DateTime.UtcNow.AddDays(-20), updatedAt: DateTime.UtcNow.AddDays(-5),
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

    private UpdateGenderCommand CreateValidUpdateCommand(Guid id)
    {
        return new UpdateGenderCommand
        {
            Id = id,
            Name = "Nome Gênero Atualizado",
            Description = "Descrição Gênero Atualizada"
        };
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

    [Fact(DisplayName = "Handle Deve Atualizar Gender e Retornar ID com Sucesso")]
    [Trait("Gender", "Update - Handler")]
    public async Task Handle_Should_UpdateGenderAndReturnId_When_CommandIsValid()
    {
        // Arrange
        var existingGender = CreateExistingGender();
        var command = CreateValidUpdateCommand(existingGender.Id);
        Gender? capturedGender = null;

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync(existingGender);
        _mockGenderRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Gender>()))
            .Callback<Gender>(g => capturedGender = g)
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().Be(existingGender.Id);
        result.Message.Should().Be("Gender updated successfully");
        capturedGender.Should().NotBeNull();
        capturedGender?.Name.Should().Be(command.Name);
        capturedGender?.Description.Should().Be(command.Description);

        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(command.Id), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.UpdateAsync(It.Is<Gender>(g => g.Id == command.Id)), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Never);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException se Gender Não For Encontrado")]
    [Trait("Gender", "Update - Handler")]
    public async Task Handle_Should_ThrowNotFoundException_When_GenderNotFound()
    {
        // Arrange
        var command = CreateValidUpdateCommand(Guid.NewGuid());
        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(command.Id)).Returns(Task.FromResult<Gender?>(null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(command.Id), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Not found Gender")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockGenderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Gender>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory(DisplayName = "Handle Deve Lançar ValidationException (Custom) se Dados Atualizados são Inválidos (Gender)")]
    [Trait("Gender", "Update - Handler - Validation")]
    [InlineData("", "Descrição Atualizada Válida")]
    [InlineData(null, "Descrição Atualizada Válida")] 
    [InlineData("Nome Gênero Atualizado Muito Longo ... (mais de 150 caracteres)", "Descrição Atualizada Válida")]
    [InlineData("Nome Atualizado Válido", "Descrição Gênero Atualizada Muito Longa ... (mais de 4000 caracteres)")]
    public async Task Handle_Should_ThrowCustomValidationException_When_UpdatedDataIsInvalid(string invalidName, string invalidDescription)
    {
        // Arrange
        if (invalidName?.Length > 50 && invalidName.StartsWith("Nome Gênero Atualizado Muito Longo")) invalidName = new string('N', 151);
        if (invalidDescription?.Length > 50 && invalidDescription.StartsWith("Descrição Gênero Atualizada Muito Longa")) invalidDescription = new string('D', 4001);

        var existingGender = CreateExistingGender();
        var command = new UpdateGenderCommand { Id = existingGender.Id, Name = invalidName, Description = invalidDescription };

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync(existingGender);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DesafioCCAA.Domain.Exceptions.ValidationException>(() => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Validate Gender has error");

        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(command.Id), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Validate Gender has error")), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockGenderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Gender>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Repositorio UpdateAsync Falhar")]
    [Trait("Gender", "Update - Handler - Failure")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_UpdateAsyncThrowsException()
    {
        // Arrange
        var existingGender = CreateExistingGender();
        var command = CreateValidUpdateCommand(existingGender.Id);
        var dbException = new Exception("Database error during update");

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync(existingGender);
        _mockGenderRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Gender>())).ThrowsAsync(dbException);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().Be(existingGender.Id);
        result.Message.Should().Be("Error to update Gender");
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(command.Id), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Gender>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to update Gender")), dbException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se UoW CommitAsync Falhar")]
    [Trait("Gender", "Update - Handler - Failure")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_CommitAsyncThrowsException()
    {
        // Arrange
        var existingGender = CreateExistingGender();
        var command = CreateValidUpdateCommand(existingGender.Id);
        var commitException = new Exception("Commit transaction error");

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync(existingGender);
        _mockGenderRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Gender>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(commitException);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().Be(existingGender.Id);
        result.Message.Should().Be("Error to update Gender");
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(command.Id), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Gender>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
          x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to update Gender")), commitException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Mediator Publish Falhar")]
    [Trait("Gender", "Update - Handler - Failure")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_PublishThrowsException()
    {
        // Arrange
        var existingGender = CreateExistingGender();
        var command = CreateValidUpdateCommand(existingGender.Id);
        var mediatorException = new Exception("Mediator publish error");

        AddDummyEvent(existingGender, new Mock<IDomainEvent>().Object);

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(command.Id)).ReturnsAsync(existingGender);
        _mockGenderRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Gender>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(mediatorException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().Be(existingGender.Id);
        result.Message.Should().Be("Error to update Gender");
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(command.Id), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockGenderRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Gender>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to update Gender")), mediatorException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}