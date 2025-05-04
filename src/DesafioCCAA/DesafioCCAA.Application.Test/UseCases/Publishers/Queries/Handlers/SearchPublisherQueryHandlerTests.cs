using DesafioCCAA.Application.UseCases.Publishers.Queries;
using DesafioCCAA.Application.UseCases.Publishers.Queries.Handlers;
using DesafioCCAA.Application.UseCases.Publishers.ViewModels;
using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Domain.Domains.Books.Fakes;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Domain.Repositories.Interfaces.Books;
using DesafioCCAA.Shared.Responses;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;

namespace DesafioCCAA.Application.Test.UseCases.Publishers.Queries.Handlers;

public class SearchPublisherQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IPublisherRepository> _mockPublisherRepository;
    private readonly SearchPublisherQueryHandler _handler;

    public SearchPublisherQueryHandlerTests()
    {
        _mockPublisherRepository = new Mock<IPublisherRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.PublisherRepository).Returns(_mockPublisherRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new SearchPublisherQueryHandler(_mockUnitOfWork.Object);
    }

    private BaseResultList<Publisher> CreateMockSearchResult(
        int pageIndex, int pageSize, int totalRowCount, int itemsOnPageCount)
    {
        var fakePublishers = FakePublisher.GetValid(itemsOnPageCount).ToList();
        var pagedResult = PagedResult.Create(page: pageIndex, pageSize: pageSize, count: totalRowCount);
        return new BaseResultList<Publisher>(fakePublishers, pagedResult, true, "Success");
    }

    private BaseResultList<Publisher> CreateMockEmptySearchResult(int pageIndex, int pageSize)
    {
        var pagedResult = PagedResult.Create(page: pageIndex, pageSize: pageSize, count: 0);
        return new BaseResultList<Publisher>(new List<Publisher>(), pagedResult, true, "No results");
    }

    [Fact(DisplayName = "Handle Deve Retornar Lista Vazia Quando Repositório Não Retorna Dados")]
    [Trait("Publisher", "Search - Handler")]
    public async Task Handle_Should_ReturnEmptyList_When_RepositoryReturnsNoData()
    {
        // Arrange
        var query = new SearchPublisherQuery { PageIndex = 1, PageSize = 10 };
        var emptyRepoResult = CreateMockEmptySearchResult(query.PageIndex, query.PageSize);

        _mockPublisherRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Publisher, bool>>>(),
            It.IsAny<Func<IQueryable<Publisher>, IOrderedQueryable<Publisher>>>(),
            It.IsAny<string>(), query.PageSize, query.PageIndex))
            .ReturnsAsync(emptyRepoResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
        result.PagedResult.RowCount.Should().Be(0);
        result.PagedResult.PageCount.Should().Be(0);
        result.PagedResult.CurrentPage.Should().Be(query.PageIndex);
        result.PagedResult.PageSize.Should().Be(query.PageSize);
        _mockPublisherRepository.Verify(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Publisher, bool>>>(),
             It.Is<Func<IQueryable<Publisher>, IOrderedQueryable<Publisher>>>(o => o == null || o.Method.Name.Contains("OrderBy")),
             "", query.PageSize, query.PageIndex),
            Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Lista Mapeada e Paginação Quando Repositório Retorna Dados")]
    [Trait("Publisher", "Search - Handler")]
    public async Task Handle_Should_ReturnMappedListAndPaging_When_RepositoryReturnsData()
    {
        // Arrange
        var query = new SearchPublisherQuery { PageIndex = 2, PageSize = 5 };
        int totalItems = 23;
        int itemsOnThisPage = 5;
        var repoResult = CreateMockSearchResult(query.PageIndex, query.PageSize, totalItems, itemsOnThisPage);
        var expectedViewModelCount = repoResult.Data.Count();

        _mockPublisherRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Publisher, bool>>>(),
            It.IsAny<Func<IQueryable<Publisher>, IOrderedQueryable<Publisher>>>(),
            It.IsAny<string>(), query.PageSize, query.PageIndex))
            .ReturnsAsync(repoResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(expectedViewModelCount);
        result.Data.Should().AllBeOfType<PublisherViewModel>();
        result.Data.First().Id.Should().Be(repoResult.Data.First().Id);

        result.PagedResult.Should().NotBeNull();
        result.PagedResult.CurrentPage.Should().Be(query.PageIndex);
        result.PagedResult.PageSize.Should().Be(query.PageSize);
        result.PagedResult.RowCount.Should().Be(totalItems);
        result.PagedResult.PageCount.Should().Be(repoResult.PagedResult.PageCount);
        _mockPublisherRepository.Verify(repo => repo.SearchAsync(
           It.IsAny<Expression<Func<Publisher, bool>>>(),
           It.Is<Func<IQueryable<Publisher>, IOrderedQueryable<Publisher>>>(o => o == null || o.Method.Name.Contains("OrderBy")),
            "", query.PageSize, query.PageIndex),
           Times.Once);
    }

    [Theory(DisplayName = "Handle Deve Passar Paginação Correta para Repositório")]
    [Trait("Publisher", "Search - Handler - Paging")]
    [InlineData(1, 10)]
    [InlineData(3, 25)]
    [InlineData(1, 100)]
    public async Task Handle_Should_PassCorrectPagingToRepository(int pageIndex, int pageSize)
    {
        // Arrange
        var query = new SearchPublisherQuery { PageIndex = pageIndex, PageSize = pageSize };
        var emptyRepoResult = CreateMockEmptySearchResult(query.PageIndex, query.PageSize);

        _mockPublisherRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Publisher, bool>>>(),
            It.IsAny<Func<IQueryable<Publisher>, IOrderedQueryable<Publisher>>>(),
            It.IsAny<string>(),
            pageSize,
            pageIndex))
            .ReturnsAsync(emptyRepoResult)
            .Verifiable();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockPublisherRepository.Verify();
    }

    [Fact(DisplayName = "Handle Deve Aplicar Filtro de Nome Quando Fornecido")]
    [Trait("Publisher", "Search - Handler - Filter")]
    public async Task Handle_Should_ApplyNameFilter_When_Provided()
    {
        // Arrange
        var query = new SearchPublisherQuery { Name = "Test" };
        var emptyRepoResult = CreateMockEmptySearchResult(query.PageIndex, query.PageSize);
        Expression<Func<Publisher, bool>>? capturedFilter = null;

        _mockPublisherRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Publisher, bool>>>(),
            It.IsAny<Func<IQueryable<Publisher>, IOrderedQueryable<Publisher>>>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Callback((Expression<Func<Publisher, bool>> filter,
                       Func<IQueryable<Publisher>, IOrderedQueryable<Publisher>> orderBy,
                       string include, int ps, int p) =>
            {
                capturedFilter = filter;
            })
            .ReturnsAsync(emptyRepoResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockPublisherRepository.Verify(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Publisher, bool>>>(),
            It.Is<Func<IQueryable<Publisher>, IOrderedQueryable<Publisher>>>(o => o == null || o.Method.Name.Contains("OrderBy")),
            "", query.PageSize, query.PageIndex), Times.Once);
        capturedFilter.Should().NotBeNull();
        capturedFilter.ToString().Should().Contain(".Name.Contains");
        capturedFilter.ToString().Should().NotContain(".Description.Contains");
    }

    [Fact(DisplayName = "Handle Deve Aplicar Ordenação Quando Fornecida")]
    [Trait("Publisher", "Search - Handler - Order")]
    public async Task Handle_Should_ApplyOrdering_When_Provided()
    {
        // Arrange
        var query = new SearchPublisherQuery { Order = "Name desc" };
        var emptyRepoResult = CreateMockEmptySearchResult(query.PageIndex, query.PageSize);
        Func<IQueryable<Publisher>, IOrderedQueryable<Publisher>>? capturedOrderBy = null;

        _mockPublisherRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Publisher, bool>>>(),
            It.IsAny<Func<IQueryable<Publisher>, IOrderedQueryable<Publisher>>>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
             .Callback((Expression<Func<Publisher, bool>> filter,
                       Func<IQueryable<Publisher>, IOrderedQueryable<Publisher>> orderBy,
                       string include, int ps, int p) =>
             {
                 capturedOrderBy = orderBy;
             })
            .ReturnsAsync(emptyRepoResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockPublisherRepository.Verify(repo => repo.SearchAsync(
           It.IsAny<Expression<Func<Publisher, bool>>>(),
           It.IsAny<Func<IQueryable<Publisher>, IOrderedQueryable<Publisher>>>(),
           "", query.PageSize, query.PageIndex), Times.Once);
        capturedOrderBy.Should().NotBeNull();
    }

    [Fact(DisplayName = "Handle Deve Propagar Exceção do Repositório SearchAsync")]
    [Trait("Publisher", "Search - Handler - Failure")]
    public async Task Handle_Should_PropagateException_When_RepositorySearchAsyncThrows()
    {
        // Arrange
        var query = new SearchPublisherQuery();
        var repositoryException = new InvalidOperationException("Error during search");

        _mockPublisherRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Publisher, bool>>>(),
            It.IsAny<Func<IQueryable<Publisher>, IOrderedQueryable<Publisher>>>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(repositoryException);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex == repositoryException);

        _mockPublisherRepository.Verify(repo => repo.SearchAsync(
           It.IsAny<Expression<Func<Publisher, bool>>>(),
            It.Is<Func<IQueryable<Publisher>, IOrderedQueryable<Publisher>>>(o => o == null || o.Method.Name.Contains("OrderBy")),
            "", query.PageSize, query.PageIndex), Times.Once);
    }
}