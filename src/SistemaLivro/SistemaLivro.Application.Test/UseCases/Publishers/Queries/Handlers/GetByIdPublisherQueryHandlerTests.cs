using SistemaLivro.Application.UseCases.Publishers.Queries;
using SistemaLivro.Application.UseCases.Publishers.Queries.Handlers;
using SistemaLivro.Application.UseCases.Publishers.ViewModels;
using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Domain.Exceptions;
using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Domain.Repositories.Interfaces.Books;
using Moq;
using SistemaLivro.Shared.Models;

namespace SistemaLivro.Application.Test.UseCases.Publishers.Queries.Handlers;

public class GetByIdPublisherQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IPublisherRepository> _mockPublisherRepository;
    private readonly GetByIdPublisherQueryHandler _handler;

    public GetByIdPublisherQueryHandlerTests()
    {
        _mockPublisherRepository = new Mock<IPublisherRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.PublisherRepository).Returns(_mockPublisherRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new GetByIdPublisherQueryHandler(_mockUnitOfWork.Object);
    }

    private Publisher CreateValidPublisher(Guid? id = null)
    {
        var publisherId = id ?? Guid.NewGuid();
        var publisher = new Publisher(
           id: publisherId, name: "Editora Encontrada", description: "Descrição da Editora",
           createdAt: DateTime.UtcNow.AddDays(-5), updatedAt: null,
           deletedAt: null, isDeleted: false
       );
        try
        {
            var eventsListField = publisher.GetType().BaseType?.BaseType?.GetField("_events", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (eventsListField != null) { (eventsListField.GetValue(publisher) as List<IDomainEvent>)?.Clear(); }
        }
        catch { }
        return publisher;
    }

    [Fact(DisplayName = "Handle Deve Retornar PublisherViewModel com Sucesso Quando Encontrado")]
    [Trait("Publisher", "GetById - Handler")]
    public async Task Handle_Should_ReturnPublisherViewModel_When_PublisherExists()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var query = new GetByIdPublisherQuery(publisherId);
        var existingPublisher = CreateValidPublisher(publisherId);

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId))
                                .ReturnsAsync(existingPublisher);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.IsType<PublisherViewModel>(result.Data);
        Assert.Equal(existingPublisher.Id, result.Data.Id);
        Assert.Equal(existingPublisher.Name, result.Data.Name);
        Assert.Equal(existingPublisher.Description, result.Data.Description);
        Assert.Equal(existingPublisher.IsDeleted, result.Data.IsDeleted);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId), Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException Quando Publisher Não Encontrado")]
    [Trait("Publisher", "GetById - Handler")]
    public async Task Handle_Should_ThrowNotFoundException_When_PublisherNotFound()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var query = new GetByIdPublisherQuery(publisherId);

        _mockPublisherRepository.Setup(repo => repo.GetByIdAsync(publisherId))
                                .Returns(Task.FromResult<Publisher?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));

        Assert.Equal("Not found", exception.Message);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(publisherId), Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar ArgumentNullException Quando Request é Nulo")]
    [Trait("Publisher", "GetById - Handler")]
    public async Task Handle_Should_ThrowArgumentNullException_When_RequestIsNull()
    {
        // Arrange
        GetByIdPublisherQuery? query = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(query!, CancellationToken.None));

        Assert.Equal("request", exception.ParamName);
        _mockPublisherRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }
}
