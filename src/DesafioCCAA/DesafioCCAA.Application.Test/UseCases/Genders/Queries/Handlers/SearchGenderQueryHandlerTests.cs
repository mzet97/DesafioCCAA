using DesafioCCAA.Application.UseCases.Genders.Queries;
using DesafioCCAA.Application.UseCases.Genders.Queries.Handlers;
using DesafioCCAA.Application.UseCases.Genders.ViewModels;
using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Domain.Domains.Books.Fakes;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Domain.Repositories.Interfaces.Books;
using DesafioCCAA.Shared.Responses;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;

namespace DesafioCCAA.Application.Test.UseCases.Genders.Queries.Handlers;

public class SearchGenderQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IGenderRepository> _mockGenderRepository;
    private readonly SearchGenderQueryHandler _handler;

    public SearchGenderQueryHandlerTests()
    {
        _mockGenderRepository = new Mock<IGenderRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.GenderRepository).Returns(_mockGenderRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new SearchGenderQueryHandler(_mockUnitOfWork.Object);
    }

    private BaseResultList<Gender> CreateMockSearchResult(
        int pageIndex, int pageSize, int totalRowCount, int itemsOnPageCount)
    {
        var fakeGenders = FakeGender.GetValid(itemsOnPageCount).ToList();
        var pagedResult = PagedResult.Create(page: pageIndex, pageSize: pageSize, count: totalRowCount);
        return new BaseResultList<Gender>(fakeGenders, pagedResult, true, "Success");
    }

    private BaseResultList<Gender> CreateMockEmptySearchResult(int pageIndex, int pageSize)
    {
        var pagedResult = PagedResult.Create(page: pageIndex, pageSize: pageSize, count: 0);
        return new BaseResultList<Gender>(new List<Gender>(), pagedResult, true, "No results");
    }


    [Fact(DisplayName = "Handle Deve Retornar Lista Vazia Quando Repositório Não Retorna Dados (Gender)")]
    [Trait("Gender", "Search - Handler")]
    public async Task Handle_Should_ReturnEmptyList_When_RepositoryReturnsNoData()
    {
        // Arrange
        var query = new SearchGenderQuery { PageIndex = 1, PageSize = 10 };
        var emptyRepoResult = CreateMockEmptySearchResult(query.PageIndex, query.PageSize);

        _mockGenderRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Gender, bool>>>(),
            It.IsAny<Func<IQueryable<Gender>, IOrderedQueryable<Gender>>>(),
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
        _mockGenderRepository.Verify(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Gender, bool>>>(),
             It.Is<Func<IQueryable<Gender>, IOrderedQueryable<Gender>>>(o => o == null || o.Method.Name.Contains("OrderBy")),
             "", query.PageSize, query.PageIndex),
            Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Retornar Lista Mapeada e Paginação Quando Repositório Retorna Dados (Gender)")]
    [Trait("Gender", "Search - Handler")]
    public async Task Handle_Should_ReturnMappedListAndPaging_When_RepositoryReturnsData()
    {
        // Arrange
        var query = new SearchGenderQuery { PageIndex = 2, PageSize = 5 };
        int totalItems = 12;
        int itemsOnThisPage = 5;
        var repoResult = CreateMockSearchResult(query.PageIndex, query.PageSize, totalItems, itemsOnThisPage);
        var expectedViewModelCount = repoResult.Data.Count();

        _mockGenderRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Gender, bool>>>(),
            It.IsAny<Func<IQueryable<Gender>, IOrderedQueryable<Gender>>>(),
            It.IsAny<string>(), query.PageSize, query.PageIndex))
            .ReturnsAsync(repoResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(expectedViewModelCount);
        result.Data.Should().AllBeOfType<GenderViewModel>();
        result.Data.First().Id.Should().Be(repoResult.Data.First().Id);
        result.Data.First().Name.Should().Be(repoResult.Data.First().Name);

        result.PagedResult.Should().NotBeNull();
        result.PagedResult.CurrentPage.Should().Be(query.PageIndex);
        result.PagedResult.PageSize.Should().Be(query.PageSize);
        result.PagedResult.RowCount.Should().Be(totalItems);
        result.PagedResult.PageCount.Should().Be(repoResult.PagedResult.PageCount);
        _mockGenderRepository.Verify(repo => repo.SearchAsync(
           It.IsAny<Expression<Func<Gender, bool>>>(),
           It.Is<Func<IQueryable<Gender>, IOrderedQueryable<Gender>>>(o => o == null || o.Method.Name.Contains("OrderBy")),
            "", query.PageSize, query.PageIndex),
           Times.Once);
    }

    [Theory(DisplayName = "Handle Deve Passar Paginação Correta para Repositório (Gender)")]
    [Trait("Gender", "Search - Handler - Paging")]
    [InlineData(1, 15)]
    [InlineData(2, 50)]
    public async Task Handle_Should_PassCorrectPagingToRepository(int pageIndex, int pageSize)
    {
        // Arrange
        var query = new SearchGenderQuery { PageIndex = pageIndex, PageSize = pageSize };
        var emptyRepoResult = CreateMockEmptySearchResult(query.PageIndex, query.PageSize);

        _mockGenderRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Gender, bool>>>(),
            It.IsAny<Func<IQueryable<Gender>, IOrderedQueryable<Gender>>>(),
            It.IsAny<string>(),
            pageSize,
            pageIndex))
            .ReturnsAsync(emptyRepoResult)
            .Verifiable();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockGenderRepository.Verify();
    }

    [Fact(DisplayName = "Handle Deve Aplicar Filtro de Nome Quando Fornecido (Gender)")]
    [Trait("Gender", "Search - Handler - Filter")]
    public async Task Handle_Should_ApplyNameFilter_When_Provided()
    {
        // Arrange
        var query = new SearchGenderQuery { Name = "Ficção" };
        var emptyRepoResult = CreateMockEmptySearchResult(query.PageIndex, query.PageSize);
        Expression<Func<Gender, bool>>? capturedFilter = null;

        _mockGenderRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Gender, bool>>>(),
            It.IsAny<Func<IQueryable<Gender>, IOrderedQueryable<Gender>>>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Callback((Expression<Func<Gender, bool>> filter,
                       Func<IQueryable<Gender>, IOrderedQueryable<Gender>> orderBy,
                       string include, int ps, int p) => {
                           capturedFilter = filter;
                       })
            .ReturnsAsync(emptyRepoResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockGenderRepository.Verify(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Gender, bool>>>(),
            It.Is<Func<IQueryable<Gender>, IOrderedQueryable<Gender>>>(o => o == null || o.Method.Name.Contains("OrderBy")),
            "", query.PageSize, query.PageIndex), Times.Once);
        capturedFilter.Should().NotBeNull();
        capturedFilter.ToString().Should().Contain(".Name.Contains");
        capturedFilter.ToString().Should().NotContain(".Description.Contains");
    }

    [Fact(DisplayName = "Handle Deve Aplicar Ordenação Quando Fornecida (Gender)")]
    [Trait("Gender", "Search - Handler - Order")]
    public async Task Handle_Should_ApplyOrdering_When_Provided()
    {
        // Arrange
        var query = new SearchGenderQuery { Order = "CreatedAt desc" };
        var emptyRepoResult = CreateMockEmptySearchResult(query.PageIndex, query.PageSize);
        Func<IQueryable<Gender>, IOrderedQueryable<Gender>>? capturedOrderBy = null;

        _mockGenderRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Gender, bool>>>(),
            It.IsAny<Func<IQueryable<Gender>, IOrderedQueryable<Gender>>>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Callback((Expression<Func<Gender, bool>> filter,
                       Func<IQueryable<Gender>, IOrderedQueryable<Gender>> orderBy,
                       string include, int ps, int p) => {
                           capturedOrderBy = orderBy;
                       })
            .ReturnsAsync(emptyRepoResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockGenderRepository.Verify(repo => repo.SearchAsync(
           It.IsAny<Expression<Func<Gender, bool>>>(),
           It.IsAny<Func<IQueryable<Gender>, IOrderedQueryable<Gender>>>(),
           "", query.PageSize, query.PageIndex), Times.Once);
        capturedOrderBy.Should().NotBeNull();
    }

    [Fact(DisplayName = "Handle Deve Propagar Exceção do Repositório SearchAsync (Gender)")]
    [Trait("Gender", "Search - Handler - Failure")]
    public async Task Handle_Should_PropagateException_When_RepositorySearchAsyncThrows()
    {
        // Arrange
        var query = new SearchGenderQuery();
        var repositoryException = new InvalidOperationException("DB Error during search");

        _mockGenderRepository.Setup(repo => repo.SearchAsync(
            It.IsAny<Expression<Func<Gender, bool>>>(),
            It.IsAny<Func<IQueryable<Gender>, IOrderedQueryable<Gender>>>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(repositoryException);

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .Where(ex => ex == repositoryException);

        _mockGenderRepository.Verify(repo => repo.SearchAsync(
           It.IsAny<Expression<Func<Gender, bool>>>(),
            It.Is<Func<IQueryable<Gender>, IOrderedQueryable<Gender>>>(o => o == null || o.Method.Name.Contains("OrderBy")),
            "", query.PageSize, query.PageIndex), Times.Once);
    }

}