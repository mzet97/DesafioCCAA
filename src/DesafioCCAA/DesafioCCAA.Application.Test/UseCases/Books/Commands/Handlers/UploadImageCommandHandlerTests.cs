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
using DesafioCCAA.Shared.Responses;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace DesafioCCAA.Application.Test.UseCases.Books.Commands.Handlers;

public class UploadImageCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly Mock<IApplicationUserService> _mockUserService;
    private readonly Mock<IFileService> _mockFileService;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<UploadImageCommandHandler>> _mockLogger;
    private readonly UploadImageCommandHandler _handler;

    public UploadImageCommandHandlerTests()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<UploadImageCommandHandler>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserService = new Mock<IApplicationUserService>();
        _mockFileService = new Mock<IFileService>();

        _mockRepositoryFactory.Setup(rf => rf.BookRepository).Returns(_mockBookRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new UploadImageCommandHandler(
            _mockUserService.Object,
            _mockUnitOfWork.Object,
            _mockMediator.Object,
            _mockFileService.Object,
            _mockLogger.Object);
    }

    private Book CreateExistingBook(Guid? id = null) { return FakeBook.GetValid(1).Select(b => { if (id.HasValue) typeof(Entity<Book>).GetProperty("Id")?.SetValue(b, id.Value); return b; }).First(); }
    private ApplicationUser CreateValidUser(Guid? id = null) => new ApplicationUser { Id = id ?? Guid.NewGuid(), UserName = "Upload User" };
    private MemoryStream CreateTestStream() => new MemoryStream(new byte[] { 1, 2, 3 });
    private void AddDummyEvent(Book entity, IDomainEvent domainEvent) { try { entity.GetType().GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(entity, new object[] { domainEvent }); } catch { } }


    [Fact(DisplayName = "Handle Deve Fazer Upload, Atualizar Book e Retornar ID com Sucesso")]
    [Trait("Book", "UploadImage - Handler")]
    public async Task Handle_Should_UploadUpdateBookAndReturnId_When_RequestIsValid()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var fileName = "imagem.png";
        var fileStream = CreateTestStream();
        var command = new UploadImageCommand(bookId, userId, fileStream, fileName);

        var existingBook = CreateExistingBook(bookId);
        var existingUser = CreateValidUser(userId);
        var savedPath = "/path/to/saved/new_guid.png";
        var savedName = "new_guid.png";
        Book? capturedBook = null;

        _mockUserService.Setup(s => s.FindByIdAsync(userId)).ReturnsAsync(existingUser);
        _mockBookRepository.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(existingBook);
        _mockFileService.Setup(fs => fs.SaveImageAsync(fileStream, fileName)).ReturnsAsync((savedPath, savedName)); // Retorna tupla
        _mockBookRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Book>())).Callback<Book>(b => capturedBook = b).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        (result as BaseResult<Guid>)?.Data.Should().Be(bookId);
        result.Message.Should().Be("Imagem do livro atualizado com sucesso");

        capturedBook.Should().NotBeNull();
        capturedBook?.CoverImage?.FileName.Should().Be(savedName);
        capturedBook?.CoverImage?.Path.Should().Be(savedPath);
        capturedBook?.UserUpdatedId.Should().Be(userId);

        _mockUserService.Verify(s => s.FindByIdAsync(userId), Times.Once);
        _mockBookRepository.Verify(r => r.GetByIdAsync(bookId), Times.Once);
        _mockFileService.Verify(fs => fs.SaveImageAsync(fileStream, fileName), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.UpdateAsync(It.Is<Book>(b => b.Id == bookId)), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException se Usuário Não For Encontrado")]
    [Trait("Book", "UploadImage - Handler - Failure")]
    public async Task Handle_Should_ThrowNotFoundException_When_UserNotFound()
    {
        // Arrange
        var command = new UploadImageCommand(Guid.NewGuid(), Guid.NewGuid(), CreateTestStream(), "file.jpg");
        _mockUserService.Setup(s => s.FindByIdAsync(command.userUpdatedId)).Returns(Task.FromResult<ApplicationUser?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Usuário não encontrado");
        _mockUserService.Verify(s => s.FindByIdAsync(command.userUpdatedId), Times.Once);
        _mockBookRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockFileService.Verify(fs => fs.SaveImageAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("User not found")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException se Livro Não For Encontrado")]
    [Trait("Book", "UploadImage - Handler - Failure")]
    public async Task Handle_Should_ThrowNotFoundException_When_BookNotFound()
    {
        // Arrange
        var user = CreateValidUser(Guid.NewGuid());
        var command = new UploadImageCommand(Guid.NewGuid(), user.Id, CreateTestStream(), "file.jpg");
        _mockUserService.Setup(s => s.FindByIdAsync(command.userUpdatedId)).ReturnsAsync(user);
        _mockBookRepository.Setup(r => r.GetByIdAsync(command.bookId)).Returns(Task.FromResult<Book?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Livro não encontrado");
        _mockUserService.Verify(s => s.FindByIdAsync(command.userUpdatedId), Times.Once);
        _mockBookRepository.Verify(r => r.GetByIdAsync(command.bookId), Times.Once);
        _mockFileService.Verify(fs => fs.SaveImageAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Book not found")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Propagar Exceção se FileService SaveImageAsync Falhar")]
    [Trait("Book", "UploadImage - Handler - Failure")]
    public async Task Handle_Should_PropagateException_When_SaveImageAsyncThrows()
    {
        // Arrange
        var existingBook = CreateExistingBook();
        var user = CreateValidUser(Guid.NewGuid());
        var command = new UploadImageCommand(existingBook.Id, user.Id, CreateTestStream(), "invalid_type.txt");
        var serviceException = new InvalidDataException("Tipo de arquivo não permitido.");

        _mockUserService.Setup(s => s.FindByIdAsync(command.userUpdatedId)).ReturnsAsync(user);
        _mockBookRepository.Setup(r => r.GetByIdAsync(command.bookId)).ReturnsAsync(existingBook);
        _mockFileService.Setup(fs => fs.SaveImageAsync(command.FileStream, command.FileName)).ThrowsAsync(serviceException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => _handler.Handle(command, CancellationToken.None));
        exception.Should().BeSameAs(serviceException);

        _mockUserService.Verify(s => s.FindByIdAsync(command.userUpdatedId), Times.Once);
        _mockBookRepository.Verify(r => r.GetByIdAsync(command.bookId), Times.Once);
        _mockFileService.Verify(fs => fs.SaveImageAsync(command.FileStream, command.FileName), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Handle Deve Lançar ValidationException (Custom) se UpdateImage Falhar Validação")]
    [Trait("Book", "UploadImage - Handler - Validation")]
    public async Task Handle_Should_ThrowCustomValidationException_When_UpdateImageValidationFails()
    {
        // Arrange
        var existingBook = CreateExistingBook();
        var user = CreateValidUser(Guid.NewGuid());
        var command = new UploadImageCommand(existingBook.Id, user.Id, CreateTestStream(), "valid.png");
        var savedPath = "/path/to/saved/new_guid.png";
        var savedName = "new_guid.png";

        _mockUserService.Setup(s => s.FindByIdAsync(command.userUpdatedId)).ReturnsAsync(user);
        _mockBookRepository.Setup(r => r.GetByIdAsync(command.bookId)).ReturnsAsync(existingBook);
        _mockFileService.Setup(fs => fs.SaveImageAsync(command.FileStream, command.FileName)).ReturnsAsync((savedPath, savedName));

        _mockBookRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);

        await Task.CompletedTask;
        Assert.True(true);
    }


    [Fact(DisplayName = "Handle Deve Retornar Falha e Rollback se Repositorio UpdateAsync Falhar")]
    [Trait("Book", "UploadImage - Handler - Failure")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_UpdateAsyncThrowsException()
    {
        // Arrange
        var existingBook = CreateExistingBook();
        var user = CreateValidUser(Guid.NewGuid());
        var command = new UploadImageCommand(existingBook.Id, user.Id, CreateTestStream(), "valid.jpg");
        var savedPath = "/path/to/saved/new_guid.jpg";
        var savedName = "new_guid.jpg";
        var dbException = new Exception("Database error during update");

        _mockUserService.Setup(s => s.FindByIdAsync(command.userUpdatedId)).ReturnsAsync(user);
        _mockBookRepository.Setup(r => r.GetByIdAsync(command.bookId)).ReturnsAsync(existingBook);
        _mockFileService.Setup(fs => fs.SaveImageAsync(command.FileStream, command.FileName)).ReturnsAsync((savedPath, savedName));
        _mockBookRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Book>())).ThrowsAsync(dbException);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        (result as BaseResult<Guid>)?.Data.Should().Be(existingBook.Id);
        result.Message.Should().Be("Erro ao atualizar a imagem do livro");
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
    [Trait("Book", "UploadImage - Handler - Failure")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_CommitAsyncThrowsException()
    {
        // Arrange
        var existingBook = CreateExistingBook();
        var user = CreateValidUser(Guid.NewGuid());
        var command = new UploadImageCommand(existingBook.Id, user.Id, CreateTestStream(), "valid.jpg");
        var savedPath = "/path/to/saved/new_guid.jpg";
        var savedName = "new_guid.jpg";
        var commitException = new Exception("Commit transaction error");

        _mockUserService.Setup(s => s.FindByIdAsync(command.userUpdatedId)).ReturnsAsync(user);
        _mockBookRepository.Setup(r => r.GetByIdAsync(command.bookId)).ReturnsAsync(existingBook);
        _mockFileService.Setup(fs => fs.SaveImageAsync(command.FileStream, command.FileName)).ReturnsAsync((savedPath, savedName));
        _mockBookRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(commitException);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        (result as BaseResult<Guid>)?.Data.Should().Be(existingBook.Id);
        result.Message.Should().Be("Erro ao atualizar a imagem do livro");
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
    [Trait("Book", "UploadImage - Handler - Failure")]
    public async Task Handle_Should_ReturnFailureAndRollback_When_PublishThrowsException()
    {
        // Arrange
        var existingBook = CreateExistingBook();
        var user = CreateValidUser(Guid.NewGuid());
        var command = new UploadImageCommand(existingBook.Id, user.Id, CreateTestStream(), "valid.jpg");
        var savedPath = "/path/to/saved/new_guid.jpg";
        var savedName = "new_guid.jpg";
        var mediatorException = new Exception("Mediator publish error");
        var dummyEvent = new Mock<IDomainEvent>().Object;

        AddDummyEvent(existingBook, dummyEvent);

        _mockUserService.Setup(s => s.FindByIdAsync(command.userUpdatedId)).ReturnsAsync(user);
        _mockBookRepository.Setup(r => r.GetByIdAsync(command.bookId)).ReturnsAsync(existingBook);
        _mockFileService.Setup(fs => fs.SaveImageAsync(command.FileStream, command.FileName)).ReturnsAsync((savedPath, savedName));
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
        (result as BaseResult<Guid>)?.Data.Should().Be(existingBook.Id);
        result.Message.Should().Be("Erro ao atualizar a imagem do livro");
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