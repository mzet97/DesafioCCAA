using DesafioCCAA.Application.UseCases.Books.Commands;
using DesafioCCAA.Application.UseCases.Books.Commands.Handlers;
using DesafioCCAA.Domain.Domains.Books;
using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Domain.Repositories.Interfaces.Books;
using DesafioCCAA.Shared.Models;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace DesafioCCAA.Application.Test.UseCases.Books.Commands.Handlers;

public class CreateBookCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IBookRepository> _mockBookRepository;
    private readonly Mock<IGenderRepository> _mockGenderRepository;
    private readonly Mock<IPublisherRepository> _mockPublisherRepository;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<CreateBookCommandHandler>> _mockLogger;
    private readonly CreateBookCommandHandler _handler;


    public CreateBookCommandHandlerTests()
    {
        _mockBookRepository = new Mock<IBookRepository>();
        _mockGenderRepository = new Mock<IGenderRepository>();
        _mockPublisherRepository = new Mock<IPublisherRepository>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<CreateBookCommandHandler>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockRepositoryFactory.Setup(rf => rf.BookRepository).Returns(_mockBookRepository.Object);
        _mockRepositoryFactory.Setup(rf => rf.GenderRepository).Returns(_mockGenderRepository.Object);
        _mockRepositoryFactory.Setup(rf => rf.PublisherRepository).Returns(_mockPublisherRepository.Object);
        _mockUnitOfWork.Setup(uow => uow.RepositoryFactory).Returns(_mockRepositoryFactory.Object);

        _handler = new CreateBookCommandHandler(
            _mockUnitOfWork.Object,
            _mockMediator.Object,
            _mockLogger.Object);
    }

    private CreateBookCommand CreateValidCommand()
    {
        return new CreateBookCommand
        {
            Title = "Livro Teste Válido",
            Author = "Autor Teste",
            Synopsis = "Sinopse de Teste Válida",
            ISBN = "978-3-16-148410-0",
            GenderId = Guid.NewGuid(),
            PublisherId = Guid.NewGuid(),
            UserCreatedId = Guid.NewGuid()
        };
    }
    private Gender CreateValidGender(Guid id) => new Gender(id, "G Teste", "D Teste", DateTime.UtcNow, null, null, false);
    private Publisher CreateValidPublisher(Guid id) => new Publisher(id, "P Teste", "D Teste", DateTime.UtcNow, null, null, false);
    private void AddDummyEvent(Book entity, IDomainEvent domainEvent) { try { entity.GetType().GetMethod("AddEvent", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(entity, new object[] { domainEvent }); } catch { } }

    [Fact(DisplayName = "Handle Deve Criar Book e Retornar ID com Sucesso")]
    [Trait("Book", "Create - Handler")]
    public async Task Handle_Should_CreateBookAndReturnId_When_CommandIsValid_And_RelationsExist()
    {
        // Arrange
        var command = CreateValidCommand();
        var validGender = CreateValidGender(command.GenderId);
        var validPublisher = CreateValidPublisher(command.PublisherId);
        Book? capturedBook = null;

        _mockGenderRepository.Setup(r => r.GetByIdAsync(command.GenderId)).ReturnsAsync(validGender);
        _mockPublisherRepository.Setup(r => r.GetByIdAsync(command.PublisherId)).ReturnsAsync(validPublisher);
        _mockBookRepository.Setup(repo => repo.AddAsync(It.IsAny<Book>())).Callback<Book>(b => capturedBook = b).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        capturedBook.Should().NotBeNull();
        capturedBook?.Title.Should().Be(command.Title);

        _mockGenderRepository.Verify(r => r.GetByIdAsync(command.GenderId), Times.Once);
        _mockPublisherRepository.Verify(r => r.GetByIdAsync(command.PublisherId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.AddAsync(It.IsAny<Book>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _mockLogger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => true), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Never);
    }

    [Theory(DisplayName = "Handle Deve Lançar ValidationException (Custom) se Validação da Entidade Book Falhar")]
    [Trait("Book", "Create - Handler - Validation")]
    [InlineData("", "Autor Válido", "Sinopse", "978-3-16-148410-0")]
    [InlineData("Título Válido", "", "Sinopse", "978-3-16-148410-0")]
    [InlineData("Título Válido", "Autor", "", "978-3-16-148410-0")]
    [InlineData("Título Válido", "Autor", "Sinopse", "123")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "Autor", "Sinopse", "978-3-16-148410-0")]
    public async Task Handle_Should_ThrowCustomValidationException_When_EntityValidationFails(string title, string author, string synopsis, string isbn)
    {
        // Arrange
        var command = new CreateBookCommand
        {
            Title = title,
            Author = author,
            Synopsis = synopsis,
            ISBN = isbn,
            GenderId = Guid.NewGuid(),
            PublisherId = Guid.NewGuid(),
            UserCreatedId = Guid.NewGuid()
        };
        _mockGenderRepository.Setup(r => r.GetByIdAsync(command.GenderId)).ReturnsAsync(CreateValidGender(command.GenderId));
        _mockPublisherRepository.Setup(r => r.GetByIdAsync(command.PublisherId)).ReturnsAsync(CreateValidPublisher(command.PublisherId));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DesafioCCAA.Domain.Exceptions.ValidationException>(() => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Livor tem erros na validação");

        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Validate Book has error")), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockBookRepository.Verify(repo => repo.AddAsync(It.IsAny<Book>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException se Gender Não For Encontrado")]
    [Trait("Book", "Create - Handler - RelationNotFound")]
    public async Task Handle_Should_ThrowNotFoundException_When_GenderNotFound()
    {
        // Arrange
        var command = CreateValidCommand();
        _mockGenderRepository.Setup(r => r.GetByIdAsync(command.GenderId)).Returns(Task.FromResult<Gender?>(null));
        _mockPublisherRepository.Setup(r => r.GetByIdAsync(command.PublisherId)).ReturnsAsync(CreateValidPublisher(command.PublisherId));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Gênero não encontrado");
        _mockGenderRepository.Verify(r => r.GetByIdAsync(command.GenderId), Times.Once);
        _mockPublisherRepository.Verify(r => r.GetByIdAsync(command.PublisherId), Times.Never);
        _mockLogger.Verify(
          x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Gender not found")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar NotFoundException se Publisher Não For Encontrado")]
    [Trait("Book", "Create - Handler - RelationNotFound")]
    public async Task Handle_Should_ThrowNotFoundException_When_PublisherNotFound()
    {
        // Arrange
        var command = CreateValidCommand();
        _mockGenderRepository.Setup(r => r.GetByIdAsync(command.GenderId)).ReturnsAsync(CreateValidGender(command.GenderId));
        _mockPublisherRepository.Setup(r => r.GetByIdAsync(command.PublisherId)).Returns(Task.FromResult<Publisher?>(null));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Editora não encontrado");
        _mockGenderRepository.Verify(r => r.GetByIdAsync(command.GenderId), Times.Once);
        _mockPublisherRepository.Verify(r => r.GetByIdAsync(command.PublisherId), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Publisher not found")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar Exceção e Rollback se Repositorio AddAsync Falhar (Book)")]
    [Trait("Book", "Create - Handler - Failure")]
    public async Task Handle_Should_ThrowExceptionAndRollback_When_AddAsyncThrowsException()
    {
        // Arrange
        var command = CreateValidCommand();
        var validGender = CreateValidGender(command.GenderId);
        var validPublisher = CreateValidPublisher(command.PublisherId);
        var dbException = new Exception("Database save error");

        _mockGenderRepository.Setup(r => r.GetByIdAsync(command.GenderId)).ReturnsAsync(validGender);
        _mockPublisherRepository.Setup(r => r.GetByIdAsync(command.PublisherId)).ReturnsAsync(validPublisher);
        _mockBookRepository.Setup(repo => repo.AddAsync(It.IsAny<Book>())).ThrowsAsync(dbException);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Erro ao criar livro");
        exception.InnerException.Should().BeSameAs(dbException);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.AddAsync(It.IsAny<Book>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to create Book")), dbException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar Exceção e Rollback se UoW CommitAsync Falhar (Book)")]
    [Trait("Book", "Create - Handler - Failure")]
    public async Task Handle_Should_ThrowExceptionAndRollback_When_CommitAsyncThrowsException()
    {
        // Arrange
        var command = CreateValidCommand();
        var validGender = CreateValidGender(command.GenderId);
        var validPublisher = CreateValidPublisher(command.PublisherId);
        var commitException = new Exception("Commit transaction error");

        _mockGenderRepository.Setup(r => r.GetByIdAsync(command.GenderId)).ReturnsAsync(validGender);
        _mockPublisherRepository.Setup(r => r.GetByIdAsync(command.PublisherId)).ReturnsAsync(validPublisher);
        _mockBookRepository.Setup(repo => repo.AddAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(commitException);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Erro ao criar livro");
        exception.InnerException.Should().BeSameAs(commitException);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.AddAsync(It.IsAny<Book>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockLogger.Verify(
          x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to create Book")), commitException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact(DisplayName = "Handle Deve Lançar Exceção e Rollback se Mediator Publish Falhar (Book)")]
    [Trait("Book", "Create - Handler - Failure")]
    public async Task Handle_Should_ThrowExceptionAndRollback_When_PublishThrowsException()
    {
        // Arrange
        var command = CreateValidCommand();
        var validGender = CreateValidGender(command.GenderId);
        var validPublisher = CreateValidPublisher(command.PublisherId);
        var mediatorException = new Exception("Mediator publish error");
        Book? capturedBook = null;
        var dummyEvent = new Mock<IDomainEvent>().Object;

        _mockGenderRepository.Setup(r => r.GetByIdAsync(command.GenderId)).ReturnsAsync(validGender);
        _mockPublisherRepository.Setup(r => r.GetByIdAsync(command.PublisherId)).ReturnsAsync(validPublisher);
        _mockBookRepository.Setup(repo => repo.AddAsync(It.IsAny<Book>()))
            .Callback<Book>(b => {
                capturedBook = b;
                AddDummyEvent(capturedBook!, dummyEvent);
            })
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(uow => uow.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockMediator.Setup(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(mediatorException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Erro ao criar livro");
        exception.InnerException.Should().BeSameAs(mediatorException);
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBookRepository.Verify(repo => repo.AddAsync(It.IsAny<Book>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(m => m.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockLogger.Verify(
           x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error to create Book")), mediatorException, It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
}