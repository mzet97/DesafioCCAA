using DesafioCCAA.Application.UseCases.Books.Commands;
using DesafioCCAA.Application.UseCases.Books.Commands.Handlers;
using DesafioCCAA.Domain.Domains.Books;
using DesafioCCAA.Domain.Domains.Books.Fakes;
using DesafioCCAA.Domain.Domains.Books.ValueObjects;
using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Domain.Repositories.Interfaces.Books;
using DesafioCCAA.Domain.Services.Interfaces;
using DesafioCCAA.Shared.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace DesafioCCAA.Application.Test.UseCases.Books.Commands.Handlers;

public class GetFileBase64QueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly Mock<IFileService> _mockFileService;
    private readonly Mock<ILogger<GetFileBase64QueryHandler>> _mockLogger;
    private readonly GetFileBase64QueryHandler _handler;

    public GetFileBase64QueryHandlerTests()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockFileService = new Mock<IFileService>();
        _mockLogger = new Mock<ILogger<GetFileBase64QueryHandler>>();

        _mockRepositoryFactory.Setup(rf => rf.BookRepository).Returns(_mockBookRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new GetFileBase64QueryHandler(
            _mockUnitOfWork.Object,
            _mockFileService.Object,
            _mockLogger.Object);
    }

    private Book CreateValidBookWithCover(Guid? id = null, string? coverFileName = null)
    {
        var bookId = id ?? Guid.NewGuid();
        var book = FakeBook.GetValid(1).First();
        try { typeof(Entity<Book>).GetProperty("Id")?.SetValue(book, bookId); } catch { }
        book.GetType().GetProperty("CoverImage")?.SetValue(book, new CoverImage(coverFileName ?? "cover_teste.png", "/path/to/cover.png"));
        try { var el = book.GetType().BaseType?.BaseType?.GetField("_events", BindingFlags.NonPublic | BindingFlags.Instance); if (el != null) { (el.GetValue(book) as List<IDomainEvent>)?.Clear(); } } catch { }
        return book;
    }

    [Fact(DisplayName = "Handle Deve Retornar Base64 com Sucesso Quando Book e Arquivo Existem")]
    [Trait("Book", "GetFileBase64 - Handler")]
    public async Task Handle_Should_ReturnBase64_When_BookAndFileExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var query = new GetFileBase64Query(bookId);
        var coverFileName = "cover123.jpg";
        var existingBook = CreateValidBookWithCover(bookId, coverFileName);
        var expectedBase64 = "AAECAwQFBgcICQ==";

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(existingBook);
        _mockFileService.Setup(fs => fs.GetFileBase64Async(coverFileName)).ReturnsAsync(expectedBase64);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().Be(expectedBase64);
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockFileService.Verify(fs => fs.GetFileBase64Async(coverFileName), Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException Quando Book Não Encontrado")]
    [Trait("Book", "GetFileBase64 - Handler")]
    public async Task Handle_Should_ThrowNotFoundException_When_BookNotFound()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var query = new GetFileBase64Query(bookId);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).Returns(Task.FromResult<Book?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        exception.Message.Should().Be("Livro não encontrado");
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockFileService.Verify(fs => fs.GetFileBase64Async(It.IsAny<string>()), Times.Never);
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Book not found")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar Exception Quando Book Não Tem CoverImage")]
    [Trait("Book", "GetFileBase64 - Handler")]
    public async Task Handle_Should_ThrowException_When_BookHasNullCoverImage()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var query = new GetFileBase64Query(bookId);
        var bookWithoutCover = CreateValidBookWithCover(bookId);
        bookWithoutCover.GetType().GetProperty("CoverImage")?.SetValue(bookWithoutCover, null);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(bookWithoutCover);

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => _handler.Handle(query, CancellationToken.None));

        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockFileService.Verify(fs => fs.GetFileBase64Async(It.IsAny<string>()), Times.Never);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException Quando Arquivo Não Encontrado Pelo FileService (Retorna Null)")]
    [Trait("Book", "GetFileBase64 - Handler")]
    public async Task Handle_Should_ThrowNotFoundException_When_FileServiceReturnsNull()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var query = new GetFileBase64Query(bookId);
        var coverFileName = "cover_nao_existe.png";
        var existingBook = CreateValidBookWithCover(bookId, coverFileName);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(existingBook);
        _mockFileService.Setup(fs => fs.GetFileBase64Async(coverFileName)).ReturnsAsync((string)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        exception.Message.Should().Be("Arquivo não encontrado");
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockFileService.Verify(fs => fs.GetFileBase64Async(coverFileName), Times.Once);
        _mockLogger.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("File not found")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Propagar Exceção do FileService GetFileBase64Async")]
    [Trait("Book", "GetFileBase64 - Handler")]
    public async Task Handle_Should_PropagateException_When_FileServiceThrows()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var query = new GetFileBase64Query(bookId);
        var coverFileName = "arquivo_com_erro.jpg";
        var existingBook = CreateValidBookWithCover(bookId, coverFileName);
        var serviceException = new FileNotFoundException("Erro simulado do serviço.", coverFileName);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId)).ReturnsAsync(existingBook);
        _mockFileService.Setup(fs => fs.GetFileBase64Async(coverFileName)).ThrowsAsync(serviceException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        exception.Should().BeSameAs(serviceException);

        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
        _mockFileService.Verify(fs => fs.GetFileBase64Async(coverFileName), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Never);
    }


    [Fact(DisplayName = "Handle Deve Lançar NullReferenceException Quando Request é Nulo")]
    [Trait("Book", "GetFileBase64 - Handler")]
    public async Task Handle_Should_ThrowNullReferenceException_When_RequestIsNull()
    {
        // Arrange
        GetFileBase64Query? query = null;

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => _handler.Handle(query!, CancellationToken.None));

        _mockBookRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockFileService.Verify(fs => fs.GetFileBase64Async(It.IsAny<string>()), Times.Never);
    }
}