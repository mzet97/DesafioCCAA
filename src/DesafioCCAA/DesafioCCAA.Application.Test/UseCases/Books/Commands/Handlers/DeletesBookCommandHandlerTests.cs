using DesafioCCAA.Application.UseCases.Books.Commands;
using DesafioCCAA.Application.UseCases.Books.Commands.Handlers;
using DesafioCCAA.Domain.Domains.Books;
using DesafioCCAA.Domain.Domains.Books.Fakes;
using DesafioCCAA.Domain.Domains.Identities;
using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Domain.Repositories.Interfaces.Books;
using DesafioCCAA.Domain.Services.Interfaces;
using DesafioCCAA.Shared.Models;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace DesafioCCAA.Application.Test.UseCases.Books.Commands.Handlers;

public class DeletesBookCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly Mock<IApplicationUserService> _mockUserService;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<DeletesBookCommandHandler>> _mockLogger;
    private readonly DeletesBookCommandHandler _handler;

    public DeletesBookCommandHandlerTests()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<DeletesBookCommandHandler>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserService = new Mock<IApplicationUserService>();

        _mockRepositoryFactory.Setup(rf => rf.BookRepository).Returns(_mockBookRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new DeletesBookCommandHandler(
            _mockUserService.Object,
            _mockUnitOfWork.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    private Book CreateValidBook(Guid? id = null)
    {
        var bookId = id ?? Guid.NewGuid();
        var book = FakeBook.GetValid(1).First();
        typeof(Entity<Book>).GetProperty("Id")?.SetValue(book, bookId);
        try
        {
            var eventsListField = book.GetType().BaseType?.BaseType?.GetField("_events", BindingFlags.NonPublic | BindingFlags.Instance);
            if (eventsListField != null) { (eventsListField.GetValue(book) as List<IDomainEvent>)?.Clear(); }
        }
        catch { }
        return book;
    }
    private ApplicationUser CreateValidUser(Guid? id = null) => new ApplicationUser { Id = id ?? Guid.NewGuid(), UserName = "Delete User" };

    private void AddDummyEvent(Book entity, IDomainEvent domainEvent)
    {
        try
        {
            entity.GetType().GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance)?
                .Invoke(entity, new object[] { domainEvent });
        }
        catch { }
    }

    [Fact(DisplayName = "Handle Deve Deletar Book com Sucesso para um ID Válido")]
    [Trait("Book", "Delete - Handler")]
    public async Task Handle_Should_DeleteBook_When_IdAndUserAreValid()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeletesBookCommand(new List<Guid> { bookId }, userId);
        var existingBook = CreateValidBook(bookId);
        var existingUser = CreateValidUser(userId);

        _mockUserService.Setup(s => s.FindByIdAsync(userId)).ReturnsAsync(existingUser);
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(existingBook);
        _mockBookRepository.Setup(repo => repo.RemoveAsync(existingBook));
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Book(s) deleted successfully");
        _mockUserService.Verify(s => s.FindByIdAsync(userId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockBookRepository.Verify(repo => repo.RemoveAsync(existingBook), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Deletar Books com Sucesso para Múltiplos IDs Válidos")]
    [Trait("Book", "Delete - Handler")]
    public async Task Handle_Should_DeleteBooks_When_MultipleIdsAndUserAreValid()
    {
        // Arrange
        var bookId1 = Guid.NewGuid();
        var bookId2 = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeletesBookCommand(new List<Guid> { bookId1, bookId2 }, userId);
        var existingBook1 = CreateValidBook(bookId1);
        var existingBook2 = CreateValidBook(bookId2);
        var existingUser = CreateValidUser(userId);

        _mockUserService.Setup(s => s.FindByIdAsync(userId)).ReturnsAsync(existingUser);
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId1)).ReturnsAsync(existingBook1);
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId2)).ReturnsAsync(existingBook2);
        _mockBookRepository.Setup(repo => repo.RemoveAsync(It.IsIn(existingBook1, existingBook2)));
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Book(s) deleted successfully");
        _mockUserService.Verify(s => s.FindByIdAsync(userId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId1), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId2), Times.Once);
        _mockBookRepository.Verify(repo => repo.RemoveAsync(existingBook1), Times.Once);
        _mockBookRepository.Verify(repo => repo.RemoveAsync(existingBook2), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException se Usuário Não For Encontrado")]
    [Trait("Book", "Delete - Handler - Failure")]
    public async Task Handle_Should_ThrowNotFoundException_When_UserNotFound()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeletesBookCommand(new List<Guid> { bookId }, userId);

        _mockUserService.Setup(s => s.FindByIdAsync(userId)).Returns(Task.FromResult<ApplicationUser?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Usuário não encontrado");
        _mockUserService.Verify(s => s.FindByIdAsync(userId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
          x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User not found")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Book Não For Encontrado")]
    [Trait("Book", "Delete - Handler - Failure")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_BookNotFound()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeletesBookCommand(new List<Guid> { bookId }, userId);
        var existingUser = CreateValidUser(userId);

        _mockUserService.Setup(s => s.FindByIdAsync(userId)).ReturnsAsync(existingUser);
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).Returns(Task.FromResult<Book?>(null));
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error deleting Book(s)");
        _mockUserService.Verify(s => s.FindByIdAsync(userId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.RemoveAsync(It.IsAny<Book>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Book with id {bookId} not found") || v.ToString().Contains("Not found Book")), It.IsAny<NotFoundException>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Repositorio RemoveAsync Falhar")]
    [Trait("Book", "Delete - Handler - Failure")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_RemoveAsyncThrowsException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeletesBookCommand(new List<Guid> { bookId }, userId);
        var existingBook = CreateValidBook(bookId);
        var existingUser = CreateValidUser(userId);
        var dbException = new Exception("Database error during delete");

        _mockUserService.Setup(s => s.FindByIdAsync(userId)).ReturnsAsync(existingUser);
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(existingBook);
        _mockBookRepository.Setup(repo => repo.RemoveAsync(existingBook)).Throws(dbException);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error deleting Book(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockBookRepository.Verify(repo => repo.RemoveAsync(existingBook), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting Book(s)")), dbException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se UoW CommitAsync Falhar")]
    [Trait("Book", "Delete - Handler - Failure")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_CommitAsyncThrowsException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeletesBookCommand(new List<Guid> { bookId }, userId);
        var existingBook = CreateValidBook(bookId);
        var existingUser = CreateValidUser(userId);
        var commitException = new Exception("Commit transaction error");

        _mockUserService.Setup(s => s.FindByIdAsync(userId)).ReturnsAsync(existingUser);
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(existingBook);
        _mockBookRepository.Setup(repo => repo.RemoveAsync(existingBook)); // Setup void
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(commitException);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error deleting Book(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockBookRepository.Verify(repo => repo.RemoveAsync(existingBook), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
          x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting Book(s)")), commitException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Mediator Publish Falhar")]
    [Trait("Book", "Delete - Handler - Failure")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_PublishThrowsException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeletesBookCommand(new List<Guid> { bookId }, userId);
        var existingBook = CreateValidBook(bookId);
        var existingUser = CreateValidUser(userId);
        var mediatorException = new Exception("Mediator publish error");

        var dummyEvent = new Mock<IDomainEvent>().Object;
        AddDummyEvent(existingBook, dummyEvent);

        _mockUserService.Setup(s => s.FindByIdAsync(userId)).ReturnsAsync(existingUser);
        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(existingBook);
        _mockBookRepository.Setup(repo => repo.RemoveAsync(existingBook));
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(mediatorException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Error deleting Book(s)");
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockBookRepository.Verify(repo => repo.RemoveAsync(existingBook), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting Book(s)")), mediatorException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}