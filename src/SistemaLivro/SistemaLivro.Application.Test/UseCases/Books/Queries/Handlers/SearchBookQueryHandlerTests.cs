using SistemaLivro.Application.UseCases.Books.Queries;
using SistemaLivro.Application.UseCases.Books.Queries.Handlers;
using SistemaLivro.Application.UseCases.Books.ViewModels;
using SistemaLivro.Domain.Domains.Books;
using SistemaLivro.Domain.Domains.Books.Fakes;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Domain.Repositories.Interfaces.Books;
using FluentAssertions;
using Moq;
using SistemaLivro.Shared.Responses;
using System.Linq.Expressions;

namespace SistemaLivro.Application.Test.UseCases.Books.Queries.Handlers;

public class SearchBookQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly SearchBookQueryHandler _handler;

    public SearchBookQueryHandlerTests()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.BookRepository).Returns(_mockBookRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new SearchBookQueryHandler(_mockUnitOfWork.Object);
    }

    private BaseResultList<Book> CreateMockSearchResult(
        int pageIndex, int pageSize, int totalRowCount, int itemsOnPageCount)
    {
        var fakeBooks = new List<Book>();
        for (int i = 0; i < itemsOnPageCount; i++)
        {
            var book = FakeBook.GetValid(1).First();
            book.Gender ??= FakeGender.GetValid(1).First();
            book.Publisher ??= FakePublisher.GetValid(1).First();
            fakeBooks.Add(book);
        }
        var pagedResult = PagedResult.Create(page: pageIndex, pageSize: pageSize, count: totalRowCount);
        return new BaseResultList<Book>(fakeBooks, pagedResult, true, "Success");
    }

    private BaseResultList<Book> CreateMockEmptySearchResult(int pageIndex, int pageSize)
    {
        var pagedResult = PagedResult.Create(page: pageIndex, pageSize: pageSize, count: 0);
        return new BaseResultList<Book>(new List<Book>(), pagedResult, true, "No results");
    }


    [Fact(DisplayName = "Handle Deve Retornar Lista Vazia Quando Repositório Não Retorna Dados (Book)")]
    [Trait("Book", "Search - Handler")]
    public async Task Handle_Should_ReturnEmptyList_When_RepositoryReturnsNoData()
    {
        // Arrange
        var query = new SearchBookQuery { PageIndex = 1, PageSize = 10 };
        var emptyRepoResult = CreateMockEmptySearchResult(query.PageIndex, query.PageSize);

        _mockBookRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Book, bool>>>(),
            It.IsAny<Func<IQueryable<Book>, IOrderedQueryable<Book>>>(),
            It.Is<string>(s => s == "Gender,Publisher"),
            query.PageSize, query.PageIndex))
            .ReturnsAsync(emptyRepoResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
        result.PagedResult.RowCount.Should().Be(0);
        result.PagedResult.PageCount.Should().Be(0);
        _mockBookRepository.Verify(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Book, bool>>>(),
            It.Is<Func<IQueryable<Book>, IOrderedQueryable<Book>>>(o => o == null || o.Method.Name.Contains("OrderBy")),
            "Gender,Publisher", query.PageSize, query.PageIndex),
            Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Lista Mapeada e Paginação Quando Repositório Retorna Dados (Book)")]
    [Trait("Book", "Search - Handler")]
    public async Task Handle_Should_ReturnMappedListAndPaging_When_RepositoryReturnsData()
    {
        // Arrange
        var query = new SearchBookQuery { PageIndex = 1, PageSize = 2 };
        int totalItems = 5;
        int itemsOnThisPage = 2;
        var repoResult = CreateMockSearchResult(query.PageIndex, query.PageSize, totalItems, itemsOnThisPage);
        var expectedViewModelCount = repoResult.Data.Count();

        _mockBookRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Book, bool>>>(),
            It.IsAny<Func<IQueryable<Book>, IOrderedQueryable<Book>>>(),
            It.Is<string>(s => s == "Gender,Publisher"),
             query.PageSize, query.PageIndex))
            .ReturnsAsync(repoResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(expectedViewModelCount);
        result.Data.Should().AllBeOfType<BookViewModel>();
        result.Data.First().Id.Should().Be(repoResult.Data.First().Id);

        result.Data.First().GenderName.Should().Be(repoResult.Data.First().Gender.Name);
        result.Data.First().PublisherName.Should().Be(repoResult.Data.First().Publisher.Name);

        result.PagedResult.Should().NotBeNull();
        result.PagedResult.CurrentPage.Should().Be(query.PageIndex);
        result.PagedResult.PageSize.Should().Be(query.PageSize);
        result.PagedResult.RowCount.Should().Be(totalItems);
        _mockBookRepository.Verify(repo => repo.SearchAsync(
           It.IsAny<Expression<Func<Book, bool>>>(),
           It.Is<Func<IQueryable<Book>, IOrderedQueryable<Book>>>(o => o == null || o.Method.Name.Contains("OrderBy")),
            "Gender,Publisher", query.PageSize, query.PageIndex),
           Times.Once);
    }

    [Theory(DisplayName = "Handle Deve Passar Paginação Correta para Repositório (Book)")]
    [Trait("Book", "Search - Handler - Paging")]
    [InlineData(1, 10)]
    [InlineData(3, 25)]
    public async Task Handle_Should_PassCorrectPagingToRepository(int pageIndex, int pageSize)
    {
        // Arrange
        var query = new SearchBookQuery { PageIndex = pageIndex, PageSize = pageSize };
        var emptyRepoResult = CreateMockEmptySearchResult(query.PageIndex, query.PageSize);

        _mockBookRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Book, bool>>>(),
            It.IsAny<Func<IQueryable<Book>, IOrderedQueryable<Book>>>(),
            It.IsAny<string>(),
            pageSize,
            pageIndex))
            .ReturnsAsync(emptyRepoResult)
            .Verifiable();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockBookRepository.Verify();
    }

    [Fact(DisplayName = "Handle Deve Aplicar Filtro de Título Quando Fornecido (Book)")]
    [Trait("Book", "Search - Handler - Filter")]
    public async Task Handle_Should_ApplyTitleFilter_When_Provided()
    {
        // Arrange
        var query = new SearchBookQuery { Title = "Domain" };
        var emptyRepoResult = CreateMockEmptySearchResult(query.PageIndex, query.PageSize);
        Expression<Func<Book, bool>>? capturedFilter = null;

        _mockBookRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Book, bool>>>(),
            It.IsAny<Func<IQueryable<Book>, IOrderedQueryable<Book>>>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Callback((Expression<Func<Book, bool>> filter, Func<IQueryable<Book>, IOrderedQueryable<Book>> orderBy, string include, int ps, int p) => { capturedFilter = filter; })
            .ReturnsAsync(emptyRepoResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockBookRepository.Verify(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Book, bool>>>(),
            It.Is<Func<IQueryable<Book>, IOrderedQueryable<Book>>>(o => o == null || o.Method.Name.Contains("OrderBy")),
             "Gender,Publisher", query.PageSize, query.PageIndex), Times.Once);
        capturedFilter.Should().NotBeNull();
        capturedFilter.ToString().Should().Contain(".Title.Contains");
    }

    [Fact(DisplayName = "Handle Deve Aplicar Filtro de Nome de Gênero Quando Fornecido (Book)")]
    [Trait("Book", "Search - Handler - Filter")]
    public async Task Handle_Should_ApplyGenderNameFilter_When_Provided()
    {
        // Arrange
        var query = new SearchBookQuery { GenderName = "Fic" };
        var emptyRepoResult = CreateMockEmptySearchResult(query.PageIndex, query.PageSize);
        Expression<Func<Book, bool>>? capturedFilter = null;

        _mockBookRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Book, bool>>>(),
            It.IsAny<Func<IQueryable<Book>, IOrderedQueryable<Book>>>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Callback((Expression<Func<Book, bool>> filter, Func<IQueryable<Book>, IOrderedQueryable<Book>> orderBy, string include, int ps, int p) => { capturedFilter = filter; })
            .ReturnsAsync(emptyRepoResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockBookRepository.Verify(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Book, bool>>>(),
            It.Is<Func<IQueryable<Book>, IOrderedQueryable<Book>>>(o => o == null || o.Method.Name.Contains("OrderBy")),
            "Gender,Publisher", query.PageSize, query.PageIndex), Times.Once);
        capturedFilter.Should().NotBeNull();
        capturedFilter.ToString().Should().Contain(".Gender.Name.Contains");
    }


    [Fact(DisplayName = "Handle Deve Aplicar Ordenação por Autor Quando Fornecida (Book)")]
    [Trait("Book", "Search - Handler - Order")]
    public async Task Handle_Should_ApplyOrdering_When_Provided()
    {
        // Arrange
        var query = new SearchBookQuery { Order = "Author asc" };
        var emptyRepoResult = CreateMockEmptySearchResult(query.PageIndex, query.PageSize);
        Func<IQueryable<Book>, IOrderedQueryable<Book>>? capturedOrderBy = null;

        _mockBookRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Book, bool>>>(),
            It.IsAny<Func<IQueryable<Book>, IOrderedQueryable<Book>>>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Callback((Expression<Func<Book, bool>> filter, Func<IQueryable<Book>, IOrderedQueryable<Book>> orderBy, string include, int ps, int p) => { capturedOrderBy = orderBy; })
            .ReturnsAsync(emptyRepoResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockBookRepository.Verify(repo => repo.SearchAsync(
           It.IsAny<Expression<Func<Book, bool>>>(),
           It.IsAny<Func<IQueryable<Book>, IOrderedQueryable<Book>>>(),
            "Gender,Publisher", query.PageSize, query.PageIndex), Times.Once);
        capturedOrderBy.Should().NotBeNull();
    }

    [Fact(DisplayName = "Handle Deve Propagar Exceção do Repositório SearchAsync (Book)")]
    [Trait("Book", "Search - Handler - Failure")]
    public async Task Handle_Should_PropagateException_When_RepositorySearchAsyncThrows()
    {
        // Arrange
        var query = new SearchBookQuery();
        var repositoryException = new InvalidOperationException("DB Error during search");

        _mockBookRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Book, bool>>>(),
            It.IsAny<Func<IQueryable<Book>, IOrderedQueryable<Book>>>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(repositoryException);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex == repositoryException);

        _mockBookRepository.Verify(repo => repo.SearchAsync(
           It.IsAny<Expression<Func<Book, bool>>>(),
            It.Is<Func<IQueryable<Book>, IOrderedQueryable<Book>>>(o => o == null || o.Method.Name.Contains("OrderBy")),
            "Gender,Publisher", query.PageSize, query.PageIndex), Times.Once);
    }
}