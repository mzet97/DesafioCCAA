using DesafioCCAA.Application.UseCases.Books.Commands;
using DesafioCCAA.Application.UseCases.Books.Commands.Handlers;
using DesafioCCAA.Domain.Domains.Books;
using DesafioCCAA.Domain.Domains.Books.Fakes;
using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Domain.Repositories.Interfaces.Books;
using DesafioCCAA.Shared.Models;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace DesafioCCAA.Application.Test.UseCases.Books.Commands.Handlers;

public class AtivesBookCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<AtivesBookCommandHandler>> _mockLogger;
    private readonly AtivesBookCommandHandler _handler;

    public AtivesBookCommandHandlerTests()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<AtivesBookCommandHandler>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.BookRepository).Returns(_mockBookRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new AtivesBookCommandHandler(
            _mockUnitOfWork.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }


    private Book CreateDisabledBook(Guid? id = null)
    {
        var bookId = id ?? Guid.NewGuid();
        var book = FakeBook.GetValid(1).First();

        typeof(Entity<Book>).GetProperty("Id").SetValue(book, bookId);
        typeof(Entity<Book>).GetProperty("IsDeleted").SetValue(book, true);
        typeof(Entity<Book>).GetProperty("DeletedAt").SetValue(book, DateTime.UtcNow.AddDays(-1));


        try
        {
            var eventsListField = book.GetType().BaseType?.BaseType?.GetField("_events", BindingFlags.NonPublic | BindingFlags.Instance);
            if (eventsListField != null) { (eventsListField.GetValue(book) as List<IDomainEvent>)?.Clear(); }
        }
        catch { }
        return book;
    }

    private void AddDummyEvent(Entity<Book> entity, IDomainEvent domainEvent)
    {
        try
        {
            entity.GetType().GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance)?
                .Invoke(entity, new object[] { domainEvent });
        }
        catch { }
    }


    [Fact(DisplayName = "Handle Deve Ativar Book com Sucesso para um ID Válido")]
    [Trait("Book", "Activate - Handler")]
    public async Task Handle_Should_ActivateBook_When_IdIsValid()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new AtivesBookCommand(new List<Guid> { bookId });
        var disabledBook = CreateDisabledBook(bookId);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(disabledBook);
        _mockBookRepository.Setup(repo => repo.ActiveAsync(bookId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Book(s) actived successfully");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        disabledBook.IsDeleted.Should().BeFalse();
        disabledBook.DeletedAt.Should().BeNull();
        _mockBookRepository.Verify(repo => repo.ActiveAsync(bookId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Ativar Books com Sucesso para Múltiplos IDs Válidos")]
    [Trait("Book", "Activate - Handler")]
    public async Task Handle_Should_ActivateBooks_When_MultipleIdsAreValid()
    {
        // Arrange
        var bookId1 = Guid.NewGuid();
        var bookId2 = Guid.NewGuid();
        var command = new AtivesBookCommand(new List<Guid> { bookId1, bookId2 });
        var disabledBook1 = CreateDisabledBook(bookId1);
        var disabledBook2 = CreateDisabledBook(bookId2);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId1)).ReturnsAsync(disabledBook1);
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId2)).ReturnsAsync(disabledBook2);
        _mockBookRepository.Setup(repo => repo.ActiveAsync(It.IsIn(bookId1, bookId2))).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Book(s) actived successfully");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId1), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId2), Times.Once);
        disabledBook1.IsDeleted.Should().BeFalse();
        disabledBook2.IsDeleted.Should().BeFalse();
        _mockBookRepository.Verify(repo => repo.ActiveAsync(bookId1), Times.Once);
        _mockBookRepository.Verify(repo => repo.ActiveAsync(bookId2), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }


    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Book Não For Encontrado")]
    [Trait("Book", "Activate - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_BookNotFound()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new AtivesBookCommand(new List<Guid> { bookId });

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).Returns(Task.FromResult<Book?>(null));
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error activating Book(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.ActiveAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Book with id {bookId} not found") || v.ToString().Contains("Not found Book")), It.IsAny<NotFoundException>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Repositorio ActiveAsync Falhar")]
    [Trait("Book", "Activate - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_ActiveAsyncThrowsException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new AtivesBookCommand(new List<Guid> { bookId });
        var disabledBook = CreateDisabledBook(bookId);
        var dbException = new Exception("Database error during activation");

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(disabledBook);
        _mockBookRepository.Setup(repo => repo.ActiveAsync(bookId)).ThrowsAsync(dbException);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error activating Book(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockBookRepository.Verify(repo => repo.ActiveAsync(bookId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error activating Book(s)")), dbException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se UoW CommitAsync Falhar")]
    [Trait("Book", "Activate - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_CommitAsyncThrowsException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new AtivesBookCommand(new List<Guid> { bookId });
        var disabledBook = CreateDisabledBook(bookId);
        var commitException = new Exception("Commit transaction error");

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(disabledBook);
        _mockBookRepository.Setup(repo => repo.ActiveAsync(bookId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(commitException);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error activating Book(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockBookRepository.Verify(repo => repo.ActiveAsync(bookId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
          x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error activating Book(s)")), commitException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Mediator Publish Falhar")]
    [Trait("Book", "Activate - Handler")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_PublishThrowsException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new AtivesBookCommand(new List<Guid> { bookId });
        var disabledBook = CreateDisabledBook(bookId);
        var mediatorException = new Exception("Mediator publish error");

        var dummyEvent = new Mock<IDomainEvent>().Object;
        AddDummyEvent(disabledBook, dummyEvent);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(disabledBook);
        _mockBookRepository.Setup(repo => repo.ActiveAsync(bookId)).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(mediatorException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error activating Book(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockBookRepository.Verify(repo => repo.ActiveAsync(bookId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error activating Book(s)")), mediatorException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}