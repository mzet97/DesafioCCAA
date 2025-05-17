using SistemaLivro.Application.UseCases.Books.Commands;
using SistemaLivro.Application.UseCases.Books.Commands.Handlers;
using SistemaLivro.Domain.Domains.Books;
using SistemaLivro.Domain.Domains.Books.Fakes;
using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Domain.Repositories.Interfaces.Books;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaLivro.Shared.Models;
using System.Reflection;

namespace SistemaLivro.Application.Test.UseCases.Books.Commands.Handlers;

public class DisablesBookCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<DisablesBookCommandHandler>> _mockLogger;
    private readonly DisablesBookCommandHandler _handler;

    public DisablesBookCommandHandlerTests()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<DisablesBookCommandHandler>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.BookRepository).Returns(_mockBookRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new DisablesBookCommandHandler(
            _mockUnitOfWork.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    private Book CreateActiveBook(Guid? id = null)
    {
        var bookId = id ?? Guid.NewGuid();
        var book = FakeBook.GetValid(1).First();
        typeof(Entity<Book>).GetProperty("Id").SetValue(book, bookId);
        typeof(Entity<Book>).GetProperty("IsDeleted").SetValue(book, false);
        typeof(Entity<Book>).GetProperty("DeletedAt").SetValue(book, null);

        try { var el = book.GetType().BaseType?.BaseType?.GetField("_events", BindingFlags.NonPublic | BindingFlags.Instance); if (el != null) { (el.GetValue(book) as List<IDomainEvent>)?.Clear(); } } catch { }
        return book;
    }

    private void AddDummyEvent(Book entity, IDomainEvent domainEvent)
    {
        try
        {
            entity.GetType().GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance)?
                .Invoke(entity, new object[] { domainEvent });
        }
        catch { }
    }

    [Fact(DisplayName = "Handle Deve Desativar Book com Sucesso para um ID Válido")]
    [Trait("Book", "Disable - Handler")]
    public async Task Handle_Should_DisableBook_When_IdIsValid()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new DisablesBookCommand(new List<Guid> { bookId });
        var activeBook = CreateActiveBook(bookId);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(activeBook);
        _mockBookRepository.Setup(repo => repo.DisableAsync(bookId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Book(s) disabled successfully");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        activeBook.IsDeleted.Should().BeTrue();
        activeBook.DeletedAt.Should().NotBeNull();
        _mockBookRepository.Verify(repo => repo.DisableAsync(bookId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Desativar Books com Sucesso para Múltiplos IDs Válidos")]
    [Trait("Book", "Disable - Handler")]
    public async Task Handle_Should_DisableBooks_When_MultipleIdsAreValid()
    {
        // Arrange
        var bookId1 = Guid.NewGuid();
        var bookId2 = Guid.NewGuid();
        var command = new DisablesBookCommand(new List<Guid> { bookId1, bookId2 });
        var activeBook1 = CreateActiveBook(bookId1);
        var activeBook2 = CreateActiveBook(bookId2);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId1)).ReturnsAsync(activeBook1);
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId2)).ReturnsAsync(activeBook2);
        _mockBookRepository.Setup(repo => repo.DisableAsync(It.IsIn(bookId1, bookId2))).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Book(s) disabled successfully");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId1), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId2), Times.Once);
        activeBook1.IsDeleted.Should().BeTrue();
        activeBook2.IsDeleted.Should().BeTrue();
        _mockBookRepository.Verify(repo => repo.DisableAsync(bookId1), Times.Once);
        _mockBookRepository.Verify(repo => repo.DisableAsync(bookId2), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Book Não For Encontrado")]
    [Trait("Book", "Disable - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_BookNotFound()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new DisablesBookCommand(new List<Guid> { bookId });

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).Returns(Task.FromResult<Book?>(null));
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error disabling Book(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.DisableAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Book with id {bookId} not found") || v.ToString().Contains("Not found Book")), It.IsAny<NotFoundException>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Repositorio DisableAsync Falhar")]
    [Trait("Book", "Disable - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_DisableAsyncThrowsException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new DisablesBookCommand(new List<Guid> { bookId });
        var activeBook = CreateActiveBook(bookId);
        var dbException = new Exception("Database error during disable");

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(activeBook);
        _mockBookRepository.Setup(repo => repo.DisableAsync(bookId)).ThrowsAsync(dbException);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error disabling Book(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockBookRepository.Verify(repo => repo.DisableAsync(bookId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error disabling Book(s)")), dbException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se UoW CommitAsync Falhar")]
    [Trait("Book", "Disable - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_CommitAsyncThrowsException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new DisablesBookCommand(new List<Guid> { bookId });
        var activeBook = CreateActiveBook(bookId);
        var commitException = new Exception("Commit transaction error");

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(activeBook);
        _mockBookRepository.Setup(repo => repo.DisableAsync(bookId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(commitException);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error disabling Book(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockBookRepository.Verify(repo => repo.DisableAsync(bookId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
          x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error disabling Book(s)")), commitException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }


    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Mediator Publish Falhar")]
    [Trait("Book", "Disable - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_PublishThrowsException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new DisablesBookCommand(new List<Guid> { bookId });
        var activeBook = CreateActiveBook(bookId);
        var mediatorException = new Exception("Mediator publish error");

        var dummyEvent = new Mock<IDomainEvent>().Object;
        AddDummyEvent(activeBook, dummyEvent);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(activeBook);
        _mockBookRepository.Setup(repo => repo.DisableAsync(bookId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(mediatorException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error disabling Book(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockBookRepository.Verify(repo => repo.DisableAsync(bookId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error disabling Book(s)")), mediatorException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}