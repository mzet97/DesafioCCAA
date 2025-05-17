using SistemaLivro.Application.UseCases.Books.Queries;
using SistemaLivro.Application.UseCases.Books.Queries.Handlers;
using SistemaLivro.Application.UseCases.Books.ViewModels;
using SistemaLivro.Domain.Domains.Books;
using SistemaLivro.Domain.Domains.Books.ValueObjects;
using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Domain.Repositories.Interfaces.Books;
using FluentAssertions;
using Moq;
using SistemaLivro.Shared.Models;
using System.Reflection;

namespace SistemaLivro.Application.Test.UseCases.Books.Queries.Handlers;

public class GetByIdBookQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly GetByIdBookQueryHandler _handler;

    public GetByIdBookQueryHandlerTests()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.BookRepository).Returns(_mockBookRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new GetByIdBookQueryHandler(_mockUnitOfWork.Object);
    }

    private Book CreateValidBook(Guid? id = null)
    {
        var bookId = id ?? Guid.NewGuid();
        var coverImage = new CoverImage("test.png", "/test/path.png");

        var book = new Book(
            id: bookId,
            title: "Título Teste",
            author: "Autor Teste",
            synopsis: "Sinopse Teste",
            isbn: "978-3-16-148410-0",
            coverImage: coverImage,
            genderId: Guid.NewGuid(),
            publisherId: Guid.NewGuid(),
            userCreatedId: Guid.NewGuid(),
            createdAt: DateTime.UtcNow.AddDays(-5),
            updatedAt: null,
            deletedAt: null,
            isDeleted: false
        );

        try
        {
            var eventsListField = book.GetType().BaseType?.BaseType?.GetField("_events", BindingFlags.NonPublic | BindingFlags.Instance);
            if (eventsListField != null) { (eventsListField.GetValue(book) as List<IDomainEvent>)?.Clear(); }
        }
        catch { }
        return book;
    }

    [Fact(DisplayName = "Handle Deve Retornar BookViewModel com Sucesso Quando Encontrado (Simples)")]
    [Trait("Book", "GetById - Handler")]
    public async Task Handle_Should_ReturnBookViewModel_When_BookExists()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var query = new GetByIdBookQuery(bookId);
        var existingBook = CreateValidBook(bookId);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                                .ReturnsAsync(existingBook);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeOfType<BookViewModel>();

        result.Data.Id.Should().Be(existingBook.Id);
        result.Data.Title.Should().Be(existingBook.Title);
        result.Data.Author.Should().Be(existingBook.Author);
        result.Data.ISBN.Should().Be(existingBook.ISBN);
        result.Data.Synopsis.Should().Be(existingBook.Synopsis);
        result.Data.GenderId.Should().Be(existingBook.GenderId);
        result.Data.PublisherId.Should().Be(existingBook.PublisherId);
        result.Data.UserCreatedId.Should().Be(existingBook.UserCreatedId);
        result.Data.CoverImage?.FileName.Should().Be(existingBook.CoverImage?.FileName);

        result.Data.Gender.Should().BeNull();
        result.Data.Publisher.Should().BeNull();
        result.Data.UserCreated.Should().BeNull();


        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException Quando Book Não Encontrado")]
    [Trait("Book", "GetById - Handler")]
    public async Task Handle_Should_ThrowNotFoundException_When_BookNotFound()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var query = new GetByIdBookQuery(bookId);

        _mockBookRepository.Setup(repo => repo.GetByIdAsync(bookId))
                                .Returns(Task.FromResult<Book?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));

        exception.Message.Should().Be("Not found");
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(bookId), Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar ArgumentNullException Quando Request é Nulo")]
    [Trait("Book", "GetById - Handler")]
    public async Task Handle_Should_ThrowArgumentNullException_When_RequestIsNull()
    {
        // Arrange
        GetByIdBookQuery? query = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(query!, CancellationToken.None));

        exception.ParamName.Should().Be("request");
        _mockBookRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }
}