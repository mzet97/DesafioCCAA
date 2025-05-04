using DesafioCCAA.Application.UseCases.Genders.Queries;
using DesafioCCAA.Application.UseCases.Genders.Queries.Handlers;
using DesafioCCAA.Application.UseCases.Genders.ViewModels;
using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Domain.Repositories.Interfaces.Books;
using FluentAssertions;
using Moq;

namespace DesafioCCAA.Application.Test.UseCases.Genders.Queries.Handlers;

public class GetByIdGenderQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IGenderRepository> _mockGenderRepository;
    private readonly GetByIdGenderQueryHandler _handler;

    public GetByIdGenderQueryHandlerTests()
    {
        _mockGenderRepository = new Mock<IGenderRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.GenderRepository).Returns(_mockGenderRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new GetByIdGenderQueryHandler(_mockUnitOfWork.Object);
    }

    private Gender CreateValidGender(Guid? id = null)
    {
        var genderId = id ?? Guid.NewGuid();

        var gender = new Gender(
           id: genderId, name: "Gênero Teste", description: "Descrição",
           createdAt: DateTime.UtcNow.AddDays(-5), updatedAt: null,
           deletedAt: null, isDeleted: false
       );

        try
        {
            var eventsListField = gender.GetType().BaseType?.BaseType?.GetField("_events", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (eventsListField != null) { (eventsListField.GetValue(gender) as System.Collections.Generic.List<DesafioCCAA.Shared.Models.IDomainEvent>)?.Clear(); }
        }
        catch { }
        return gender;
    }

    [Fact(DisplayName = "Handle Deve Retornar GenderViewModel com Sucesso Quando Encontrado")]
    [Trait("Gender", "GetById - Handler")]
    public async Task Handle_Should_ReturnGenderViewModel_When_GenderExists()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var query = new GetByIdGenderQuery(genderId);
        var existingGender = CreateValidGender(genderId);

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId))
                                .ReturnsAsync(existingGender);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeOfType<GenderViewModel>();
        result.Data.Id.Should().Be(existingGender.Id);
        result.Data.Name.Should().Be(existingGender.Name);
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException Quando Gender Não Encontrado")]
    [Trait("Gender", "GetById - Handler")]
    public async Task Handle_Should_ThrowNotFoundException_When_GenderNotFound()
    {
        // Arrange
        var genderId = Guid.NewGuid();
        var query = new GetByIdGenderQuery(genderId);

        _mockGenderRepository.Setup(repo => repo.GetByIdAsync(genderId))
                                .Returns(Task.FromResult<Gender?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));

        exception.Message.Should().Be("Not found");
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(genderId), Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar ArgumentNullException Quando Request é Nulo")]
    [Trait("Gender", "GetById - Handler")]
    public async Task Handle_Should_ThrowArgumentNullException_When_RequestIsNull()
    {
        // Arrange
        GetByIdGenderQuery? query = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(query!, CancellationToken.None));

        exception.ParamName.Should().Be("request");
        _mockGenderRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }
}