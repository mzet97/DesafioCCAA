using SistemaLivro.Application.UseCases.Books.Commands;
using SistemaLivro.Application.UseCases.Books.Commands.Handlers;
using SistemaLivro.Domain.Domains.Books;
using SistemaLivro.Domain.Domains.Books.Fakes;
using SistemaLivro.Domain.Domains.Books.ValueObjects;
using SistemaLivro.Domain.Domains.Identities;
using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Domain.Repositories.Interfaces.Books;
using SistemaLivro.Domain.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaLivro.Shared.Models;
using System.Reflection;

namespace SistemaLivro.Application.Test.UseCases.Books.Commands.Handlers;

public class DownloadFileQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly Mock<IFileService> _mockFileService;
    private readonly Mock<ILogger<DownloadFileQueryHandler>> _mockLogger;
    private readonly DownloadFileQueryHandler _handler;

    public DownloadFileQueryHandlerTests()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockFileService = new Mock<IFileService>();
        _mockLogger = new Mock<ILogger<DownloadFileQueryHandler>>();

        _mockRepositoryFactory.Setup(rf => rf.BookRepository).Returns(_mockBookRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new DownloadFileQueryHandler(
            _mockUnitOfWork.Object,
            _mockFileService.Object,
            _mockLogger.Object);
    }

    private Book CreateValidBookWithCover(Guid? id = null, string? coverFileName = null)
    {
        var bookId = id ?? Guid.NewGuid();
        var book = FakeBook.GetValid(1).First();
        try { typeof(Entity<Book>).GetProperty("Id")?.SetValue(book, bookId); } catch { }

        var coverImage = new CoverImage(coverFileName ?? "cover_teste.png", "/path/to/cover.png");
        var updateUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "UpdateUserTest" };
        var updateUserId = updateUser.Id;

        book.UpdateImage(coverImage, updateUserId, updateUser);

        try
        {
            var eventsListField = book.GetType().BaseType?.BaseType?.GetField("_events", BindingFlags.NonPublic | BindingFlags.Instance);
            if (eventsListField != null) { (eventsListField.GetValue(book) as List<IDomainEvent>)?.Clear(); }
        }
        catch { }

        return book;
    }

    [Fact(DisplayName = "Handle Deve Retornar Stream Quando Book e Arquivo Existem")]
    [Trait("Book", "DownloadFile - Handler")]
    public async Task Handle_Should_ReturnStream_When_BookAndFileExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var query = new DownloadFileQuery(bookId);
        var coverFileName = "cover123.jpg";
        var existingBook = CreateValidBookWithCover(bookId, coverFileName);
        var expectedStream = new MemoryStream(new byte[] { 1, 2, 3 });

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(existingBook);
        _mockFileService.Setup(fs => fs.GetFileStreamAsync(coverFileName)).ReturnsAsync(expectedStream);

        // Act
        var resultStream = await _handler.Handle(query, CancellationToken.None);

        // Assert
        resultStream.Should().NotBeNull();
        resultStream.Should().BeSameAs(expectedStream);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockFileService.Verify(fs => fs.GetFileStreamAsync(coverFileName), Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException Quando Book Não Encontrado")]
    [Trait("Book", "DownloadFile - Handler")]
    public async Task Handle_Should_ThrowNotFoundException_When_BookNotFound()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var query = new DownloadFileQuery(bookId);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).Returns(Task.FromResult<Book?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        exception.Message.Should().Be("Livro não encontrado");
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockFileService.Verify(fs => fs.GetFileStreamAsync(It.IsAny<string>()), Times.Never);
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Book not found")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar Exception Quando Book Não Tem CoverImage")]
    [Trait("Book", "DownloadFile - Handler")]
    public async Task Handle_Should_ThrowException_When_BookHasNullCoverImage()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var query = new DownloadFileQuery(bookId);
        var bookWithoutCover = CreateValidBookWithCover(bookId);
        bookWithoutCover.GetType().GetProperty("CoverImage")?.SetValue(bookWithoutCover, null);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(bookWithoutCover);

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => _handler.Handle(query, CancellationToken.None));
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockFileService.Verify(fs => fs.GetFileStreamAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException Quando Arquivo Não Encontrado Pelo FileService")]
    [Trait("Book", "DownloadFile - Handler")]
    public async Task Handle_Should_ThrowNotFoundException_When_FileServiceReturnsNull()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var query = new DownloadFileQuery(bookId);
        var coverFileName = "cover_nao_existe.png";
        var existingBook = CreateValidBookWithCover(bookId, coverFileName);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(existingBook);
        _mockFileService.Setup(fs => fs.GetFileStreamAsync(coverFileName)).ReturnsAsync((Stream)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        exception.Message.Should().Be("Arquivo não encontrado");
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockFileService.Verify(fs => fs.GetFileStreamAsync(coverFileName), Times.Once);
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("File not found")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Propagar Exceção do FileService GetFileStreamAsync")]
    [Trait("Book", "DownloadFile - Handler")]
    public async Task Handle_Should_PropagateException_When_FileServiceThrows()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var query = new DownloadFileQuery(bookId);
        var coverFileName = "arquivo_com_erro.jpg";
        var existingBook = CreateValidBookWithCover(bookId, coverFileName);
        var serviceException = new IOException("Erro ao ler o arquivo");

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(existingBook);
        _mockFileService.Setup(fs => fs.GetFileStreamAsync(coverFileName)).ThrowsAsync(serviceException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<IOException>(() => _handler.Handle(query, CancellationToken.None));
        exception.Should().BeSameAs(serviceException);

        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockFileService.Verify(fs => fs.GetFileStreamAsync(coverFileName), Times.Once);
    }
}