using SistemaLivro.Application.UseCases.Books.Commands;
using SistemaLivro.Application.UseCases.Books.Commands.Handlers;
using SistemaLivro.Domain.Domains.Books;
using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Domain.Domains.Books.Fakes;
using SistemaLivro.Domain.Domains.Identities;
using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Domain.Repositories.Interfaces.Books;
using SistemaLivro.Domain.Services.Interfaces;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaLivro.Shared.Models;
using System.Reflection;

namespace SistemaLivro.Application.Test.UseCases.Books.Commands.Handlers;

public class UpdateBookCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly Mock<IGenderRepository> _mockGenderRepository;
    private readonly Mock<IPublisherRepository> _mockPublisherRepository;
    private readonly Mock<IApplicationUserService> _mockUserService;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<UpdateBookCommandHandler>> _mockLogger;
    private readonly UpdateBookCommandHandler _handler;

    public UpdateBookCommandHandlerTests()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _mockGenderRepository = new Mock<IGenderRepository>();
        _mockPublisherRepository = new Mock<IPublisherRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<UpdateBookCommandHandler>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserService = new Mock<IApplicationUserService>();

        _mockRepositoryFactory.Setup(rf => rf.BookRepository).Returns(_mockBookRepository.Object);
        _mockRepositoryFactory.Setup(rf => rf.GenderRepository).Returns(_mockGenderRepository.Object);
        _mockRepositoryFactory.Setup(rf => rf.PublisherRepository).Returns(_mockPublisherRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new UpdateBookCommandHandler(
            _mockUserService.Object,
            _mockUnitOfWork.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    private Book CreateExistingBook(Guid? id = null) { return FakeBook.GetValid(1).Select(b => { if (id.HasValue) typeof(Entity<Book>).GetProperty("Id")?.SetValue(b, id.Value); return b; }).First(); }
    private Gender CreateValidGender(Guid id) => new Gender(id, "G Teste", "D Teste", DateTime.UtcNow, null, null, false);
    private Publisher CreateValidPublisher(Guid id) => new Publisher(id, "P Teste", "D Teste", DateTime.UtcNow, null, null, false);
    private ApplicationUser CreateValidUser(Guid id) => new ApplicationUser { Id = id, UserName = "Update User" };
    private UpdateBookCommand CreateValidUpdateCommand(Guid bookId, Guid userId, Guid? genderId = null, Guid? publisherId = null) => new UpdateBookCommand
    {
        Id = bookId,
        Title = "Título Atualizado",
        Author = "Autor Atualizado",
        Synopsis = "Sinopse Atualizada",
        ISBN = "978-0-306-40615-7",
        GenderId = genderId ?? Guid.NewGuid(),
        PublisherId = publisherId ?? Guid.NewGuid(),
        UserUpdatedId = userId
    };
    private void AddDummyEvent(Book entity, IDomainEvent domainEvent) { try { entity.GetType().GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(entity, new object[] { domainEvent }); } catch { } }


    [Fact(DisplayName = "Handle Deve Atualizar Book e Retornar ID com Sucesso")]
    [Trait("Book", "Update - Handler")]
    public async Task Handle_Should_UpdateBookAndReturnId_When_CommandIsValid_And_RelationsExist()
    {
        // Arrange
        var existingBook = CreateExistingBook();
        var user = CreateValidUser(Guid.NewGuid());
        var gender = CreateValidGender(Guid.NewGuid());
        var publisher = CreateValidPublisher(Guid.NewGuid());
        var command = CreateValidUpdateCommand(existingBook.Id, user.Id, gender.Id, publisher.Id);
        Book? capturedBook = null;

        _mockUserService.Setup(s => s.FindByIdAsync(command.UserUpdatedId)).ReturnsAsync(user);
        _mockBookRepository.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(existingBook);
        _mockGenderRepository.Setup(r => r.GetByIdAsync(command.GenderId)).ReturnsAsync(gender);
        _mockPublisherRepository.Setup(r => r.GetByIdAsync(command.PublisherId)).ReturnsAsync(publisher);
        _mockBookRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Book>())).Callback<Book>(b => capturedBook = b).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().Be(existingBook.Id);
        result.Message.Should().Be("Livro atualizado com sucesso");
        capturedBook.Should().NotBeNull();
        capturedBook?.Title.Should().Be(command.Title);
        capturedBook?.Author.Should().Be(command.Author);
        capturedBook?.GenderId.Should().Be(command.GenderId);
        capturedBook?.PublisherId.Should().Be(command.PublisherId);
        capturedBook?.UserUpdatedId.Should().Be(command.UserUpdatedId);

        _mockUserService.Verify(s => s.FindByIdAsync(command.UserUpdatedId), Times.Once);
        _mockBookRepository.Verify(r => r.GetByIdAsync(command.Id), Times.Once);
        _mockGenderRepository.Verify(r => r.GetByIdAsync(command.GenderId), Times.Once);
        _mockPublisherRepository.Verify(r => r.GetByIdAsync(command.PublisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.UpdateAsync(It.Is<Book>(b => b.Id == command.Id)), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException se Usuário Não For Encontrado")]
    [Trait("Book", "Update - Handler - Failure")]
    public async Task Handle_Should_ThrowNotFoundException_When_UserNotFound()
    {
        // Arrange
        var command = CreateValidUpdateCommand(Guid.NewGuid(), Guid.NewGuid());
        _mockUserService.Setup(s => s.FindByIdAsync(command.UserUpdatedId)).Returns(Task.FromResult<ApplicationUser?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Usuário não encontrado");
        _mockUserService.Verify(s => s.FindByIdAsync(command.UserUpdatedId), Times.Once);
        _mockBookRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User not found")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException se Livro Não For Encontrado")]
    [Trait("Book", "Update - Handler - Failure")]
    public async Task Handle_Should_ThrowNotFoundException_When_BookNotFound()
    {
        // Arrange
        var user = CreateValidUser(Guid.NewGuid());
        var command = CreateValidUpdateCommand(Guid.NewGuid(), user.Id);
        _mockUserService.Setup(s => s.FindByIdAsync(command.UserUpdatedId)).ReturnsAsync(user);
        _mockBookRepository.Setup(r => r.GetByIdAsync(command.Id)).Returns(Task.FromResult<Book?>(null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        _mockBookRepository.Verify(r => r.GetByIdAsync(command.Id), Times.Once);
        _mockGenderRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Not found Book")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException se Gênero Não For Encontrado")]
    [Trait("Book", "Update - Handler - Failure")]
    public async Task Handle_Should_ThrowNotFoundException_When_GenderNotFound()
    {
        // Arrange
        var existingBook = CreateExistingBook();
        var user = CreateValidUser(Guid.NewGuid());
        var command = CreateValidUpdateCommand(existingBook.Id, user.Id);

        _mockUserService.Setup(s => s.FindByIdAsync(command.UserUpdatedId)).ReturnsAsync(user);
        _mockBookRepository.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(existingBook);
        _mockGenderRepository.Setup(r => r.GetByIdAsync(command.GenderId)).Returns(Task.FromResult<Gender?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Gênero não encontrado");
        _mockGenderRepository.Verify(r => r.GetByIdAsync(command.GenderId), Times.Once);
        _mockPublisherRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Gender not found")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException se Editora Não For Encontrada")]
    [Trait("Book", "Update - Handler - Failure")]
    public async Task Handle_Should_ThrowNotFoundException_When_PublisherNotFound()
    {
        // Arrange
        var existingBook = CreateExistingBook();
        var user = CreateValidUser(Guid.NewGuid());
        var gender = CreateValidGender(Guid.NewGuid());
        var command = CreateValidUpdateCommand(existingBook.Id, user.Id, gender.Id);

        _mockUserService.Setup(s => s.FindByIdAsync(command.UserUpdatedId)).ReturnsAsync(user);
        _mockBookRepository.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(existingBook);
        _mockGenderRepository.Setup(r => r.GetByIdAsync(command.GenderId)).ReturnsAsync(gender);
        _mockPublisherRepository.Setup(r => r.GetByIdAsync(command.PublisherId)).Returns(Task.FromResult<Publisher?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Editora não encontrado");
        _mockPublisherRepository.Verify(r => r.GetByIdAsync(command.PublisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Publisher not found")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Theory(DisplayName = "Handle Deve Lançar ValidationException (Custom) se Dados Atualizados são Inválidos (Book)")]
    [Trait("Book", "Update - Handler - Validation")]
    [InlineData("", "Autor Válido", "Sinopse", "978-3-16-148410-0")]
    [InlineData("Título Válido", "", "Sinopse", "978-3-16-148410-0")]
    [InlineData("Título Válido", "Autor", "", "978-3-16-148410-0")]
    [InlineData("Título Válido", "Autor", "Sinopse", "123")]
    [InlineData("Este é um título de livro propositalmente muito longo para exceder o limite de cento e cinquenta caracteres definido na validação e assim garantir que o teste de MaxLength falhe corretamente.", "Autor Válido", "Sinopse Válida", "978-3-16-148410-0")]
    public async Task Handle_Should_ThrowCustomValidationException_When_UpdatedDataIsInvalid(string invalidTitle, string invalidAuthor, string invalidSynopsis, string invalidIsbn)
    {
        // Arrange
        var existingBook = CreateExistingBook();
        var user = CreateValidUser(Guid.NewGuid());
        var gender = CreateValidGender(Guid.NewGuid());
        var publisher = CreateValidPublisher(Guid.NewGuid());
        var command = new UpdateBookCommand
        {
            Id = existingBook.Id,
            Title = invalidTitle,
            Author = invalidAuthor,
            Synopsis = invalidSynopsis,
            ISBN = invalidIsbn,
            GenderId = gender.Id,
            PublisherId = publisher.Id,
            UserUpdatedId = user.Id
        };

        _mockUserService.Setup(s => s.FindByIdAsync(command.UserUpdatedId)).ReturnsAsync(user);
        _mockBookRepository.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(existingBook);
        _mockGenderRepository.Setup(r => r.GetByIdAsync(command.GenderId)).ReturnsAsync(gender);
        _mockPublisherRepository.Setup(r => r.GetByIdAsync(command.PublisherId)).ReturnsAsync(publisher);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<SistemaLivro.Domain.Exceptions.ValidationException>(() => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Validate Book has error");

        _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Validate Book has error")), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockBookRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Book>()), Times.Never);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Repositorio UpdateAsync Falhar")]
    [Trait("Book", "Update - Handler - Failure")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_UpdateAsyncThrowsException()
    {
        // Arrange
        var existingBook = CreateExistingBook();
        var user = CreateValidUser(Guid.NewGuid());
        var gender = CreateValidGender(Guid.NewGuid());
        var publisher = CreateValidPublisher(Guid.NewGuid());
        var command = CreateValidUpdateCommand(existingBook.Id, user.Id, gender.Id, publisher.Id);
        var dbException = new Exception("Database error during update");

        _mockUserService.Setup(s => s.FindByIdAsync(command.UserUpdatedId)).ReturnsAsync(user);
        _mockBookRepository.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(existingBook);
        _mockGenderRepository.Setup(r => r.GetByIdAsync(command.GenderId)).ReturnsAsync(gender);
        _mockPublisherRepository.Setup(r => r.GetByIdAsync(command.PublisherId)).ReturnsAsync(publisher);
        _mockBookRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Book>())).ThrowsAsync(dbException);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().Be(existingBook.Id);
        result.Message.Should().Be("Erro ao atualizar o livro");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Book>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to update Book")), dbException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se UoW CommitAsync Falhar")]
    [Trait("Book", "Update - Handler - Failure")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_CommitAsyncThrowsException()
    {
        // Arrange
        var existingBook = CreateExistingBook();
        var user = CreateValidUser(Guid.NewGuid());
        var gender = CreateValidGender(Guid.NewGuid());
        var publisher = CreateValidPublisher(Guid.NewGuid());
        var command = CreateValidUpdateCommand(existingBook.Id, user.Id, gender.Id, publisher.Id);
        var commitException = new Exception("Commit transaction error");

        _mockUserService.Setup(s => s.FindByIdAsync(command.UserUpdatedId)).ReturnsAsync(user);
        _mockBookRepository.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(existingBook);
        _mockGenderRepository.Setup(r => r.GetByIdAsync(command.GenderId)).ReturnsAsync(gender);
        _mockPublisherRepository.Setup(r => r.GetByIdAsync(command.PublisherId)).ReturnsAsync(publisher);
        _mockBookRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(commitException);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().Be(existingBook.Id);
        result.Message.Should().Be("Erro ao atualizar o livro");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Book>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
          x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to update Book")), commitException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Mediator Publish Falhar")]
    [Trait("Book", "Update - Handler - Failure")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_PublishThrowsException()
    {
        // Arrange
        var existingBook = CreateExistingBook();
        var user = CreateValidUser(Guid.NewGuid());
        var gender = CreateValidGender(Guid.NewGuid());
        var publisher = CreateValidPublisher(Guid.NewGuid());
        var command = CreateValidUpdateCommand(existingBook.Id, user.Id, gender.Id, publisher.Id);
        var mediatorException = new Exception("Mediator publish error");
        var dummyEvent = new Mock<IDomainEvent>().Object;

        AddDummyEvent(existingBook, dummyEvent);

        _mockUserService.Setup(s => s.FindByIdAsync(command.UserUpdatedId)).ReturnsAsync(user);
        _mockBookRepository.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(existingBook);
        _mockGenderRepository.Setup(r => r.GetByIdAsync(command.GenderId)).ReturnsAsync(gender);
        _mockPublisherRepository.Setup(r => r.GetByIdAsync(command.PublisherId)).ReturnsAsync(publisher);
        _mockBookRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(mediatorException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().Be(existingBook.Id);
        result.Message.Should().Be("Erro ao atualizar o livro");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Book>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to update Book")), mediatorException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}