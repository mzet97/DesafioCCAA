using SistemaLivro.Domain.Domains.Books;
using SistemaLivro.Domain.Domains.Books.Fakes;
using SistemaLivro.Domain.Domains.Books.ValueObjects;

namespace SistemaLivro.Domain.Test;

public class BookTests
{
    private string ValidTitle() => "Título do Livro Válido";
    private string ValidAuthor() => "Autor Válido";
    private string ValidSynopsis() => "Sinopse válida para o livro.";
    private string ValidISBN() => "978-3-16-148410-0";
    private CoverImage ValidCoverImage() => FakeCoverImage.GetValid(1).First();
    private Guid ValidGenderId() => Guid.NewGuid();
    private Guid ValidPublisherId() => Guid.NewGuid();
    private Guid ValidUserCreatedId() => Guid.NewGuid();

    [Fact(DisplayName = "Criar Book Válido via Create com todos os dados válidos")]
    [Trait("Book", "Validation - Create")]
    public void Create_Should_Be_Valid_When_All_Data_Is_Valid()
    {
        // Arrange & Act
        var book = Book.Create(
            ValidTitle(),
            ValidAuthor(),
            ValidSynopsis(),
            ValidISBN(),
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId());

        // Assert
        Assert.True(book.IsValid());
        Assert.Empty(book.Errors);
    }

    [Fact(DisplayName = "Criar Book Inválido via Create com Title Nulo")]
    [Trait("Book", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_Title_Is_Null()
    {
        // Arrange & Act
        var book = Book.Create(
            null,
            ValidAuthor(),
            ValidSynopsis(),
            ValidISBN(),
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId());

        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, e => e.Contains("título é obrigatório"));
    }

    [Fact(DisplayName = "Criar Book Inválido via Create com Title Vazio")]
    [Trait("Book", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_Title_Is_Empty()
    {
        // Arrange & Act
        var book = Book.Create(
            "",
            ValidAuthor(),
            ValidSynopsis(),
            ValidISBN(),
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId());

        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, e => e.Contains("título é obrigatório"));
    }

    [Fact(DisplayName = "Criar Book Inválido via Create com Title excedendo MaxLength")]
    [Trait("Book", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_Title_Exceeds_MaxLength()
    {
        // Arrange & Act
        var longTitle = new string('T', 151);
        var book = Book.Create(
            longTitle,
            ValidAuthor(),
            ValidSynopsis(),
            ValidISBN(),
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId());

        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, e => e.Contains("título não pode exceder 150"));
    }

    [Fact(DisplayName = "Criar Book Inválido via Create com Author Nulo")]
    [Trait("Book", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_Author_Is_Null()
    {
        // Arrange & Act
        var book = Book.Create(
            ValidTitle(),
            null,
            ValidSynopsis(),
            ValidISBN(),
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId());
        
        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, e => e.Contains("autor é obrigatório"));
    }

    [Fact(DisplayName = "Criar Book Inválido via Create com Author Vazio")]
    [Trait("Book", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_Author_Is_Empty()
    {
        // Arrange & Act
        var book = Book.Create(
            ValidTitle(),
            "",
            ValidSynopsis(),
            ValidISBN(),
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId());

        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, e => e.Contains("autor é obrigatório"));
    }

    [Fact(DisplayName = "Criar Book Inválido via Create com Author excedendo MaxLength")]
    [Trait("Book", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_Author_Exceeds_MaxLength()
    {
        // Arrange & Act
        var longAuthor = new string('A', 151);
        var book = Book.Create(
            ValidTitle(),
            longAuthor,
            ValidSynopsis(),
            ValidISBN(),
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId());

        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, e => e.Contains("autor não pode exceder 150"));
    }

    [Fact(DisplayName = "Criar Book Inválido via Create com Synopsis Nula")]
    [Trait("Book", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_Synopsis_Is_Null()
    {
        // Arrange & Act
        var book = Book.Create(
            ValidTitle(),
            ValidAuthor(),
            null,
            ValidISBN(),
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId());

        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, e => e.Contains("sinopse é obrigatória"));
    }

    [Fact(DisplayName = "Criar Book Inválido via Create com Synopsis Vazia")]
    [Trait("Book", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_Synopsis_Is_Empty()
    {
        // Arrange & Act
        var book = Book.Create(
            ValidTitle(),
            ValidAuthor(),
            "",
            ValidISBN(),
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId());

        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, e => e.Contains("sinopse é obrigatória"));
    }

    [Fact(DisplayName = "Criar Book Inválido via Create com Synopsis excedendo MaxLength")]
    [Trait("Book", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_Synopsis_Exceeds_MaxLength()
    {
        // Arrange & Act
        var longSynopsis = new string('S', 4001);
        var book = Book.Create(
            ValidTitle(),
            ValidAuthor(),
            longSynopsis,
            ValidISBN(),
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId());

        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, e => e.Contains("sinopse não pode exceder 4000"));
    }

    [Fact(DisplayName = "Criar Book Inválido via Create com ISBN Nulo")]
    [Trait("Book", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_ISBN_Is_Null()
    {
        // Arrange & Act
        var book = Book.Create(
            ValidTitle(),
            ValidAuthor(),
            ValidSynopsis(),
            null,
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId());

        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, e => e.Contains("ISBN é obrigatório"));
    }

    [Fact(DisplayName = "Criar Book Inválido via Create com ISBN Vazio")]
    [Trait("Book", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_ISBN_Is_Empty()
    {
        // Arrange & Act
        var book = Book.Create(
            ValidTitle(),
            ValidAuthor(),
            ValidSynopsis(),
            "",
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId());

        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, e => e.Contains("ISBN é obrigatório"));
    }

    [Theory(DisplayName = "Criar Book Inválido via Create com ISBN Inválido")]
    [Trait("Book", "Validation - Create")]
    [InlineData("1234567890")]
    [InlineData("978-1-2345-6789-0")]
    [InlineData("ABCDEFGHIJ")]
    public void Create_Should_Be_Invalid_When_ISBN_Is_Invalid(string invalidIsbn)
    {
        // Arrange & Act
        var book = Book.Create(
            ValidTitle(),
            ValidAuthor(),
            ValidSynopsis(),
            invalidIsbn,
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId());

        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, e => e.Contains("Formato de ISBN inválido"));
    }

    [Theory(DisplayName = "Criar Book Válido via Create com ISBN Válido")]
    [Trait("Book", "Validation - Create")]
    [InlineData("0-306-40615-2")]
    [InlineData("978-0-306-40615-7")]
    public void Create_Should_Be_Valid_When_ISBN_Is_Valid(string validIsbn)
    {
        // Arrange & Act
        var book = Book.Create(
            ValidTitle(),
            ValidAuthor(),
            ValidSynopsis(),
            validIsbn,
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId());

        // Assert
        Assert.True(book.IsValid());
        Assert.Empty(book.Errors);
    }

    [Fact(DisplayName = "Criar Book Inválido via Create com GenderId Vazio")]
    [Trait("Book", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_GenderId_Is_Empty()
    {
        // Arrange & Act
        var book = Book.Create(
            ValidTitle(),
            ValidAuthor(),
            ValidSynopsis(),
            ValidISBN(),
            ValidCoverImage(),
            Guid.Empty,
            ValidPublisherId(),
            ValidUserCreatedId());

        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, e => e.Contains("gênero é obrigatório"));
    }

    [Fact(DisplayName = "Criar Book Inválido via Create com PublisherId Vazio")]
    [Trait("Book", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_PublisherId_Is_Empty()
    {
        // Arrange & Act
        var book = Book.Create(
            ValidTitle(),
            ValidAuthor(),
            ValidSynopsis(),
            ValidISBN(),
            ValidCoverImage(),
            ValidGenderId(),
            Guid.Empty,
            ValidUserCreatedId());

        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, e => e.Contains("editora é obrigatória"));
    }

    [Fact(DisplayName = "Criar Book Inválido via Create com UserCreatedId Vazio")]
    [Trait("Book", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_UserCreatedId_Is_Empty()
    {
        // Arrange & Act
        var book = Book.Create(
            ValidTitle(),
            ValidAuthor(),
            ValidSynopsis(),
            ValidISBN(),
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            Guid.Empty);

        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, e => e.Contains("usuário criador é obrigatório"));
    }


    private DateTime ValidBaseCreatedAt => DateTime.UtcNow.AddDays(-1);
    private DateTime? ValidBaseUpdatedAt => null;
    private DateTime? ValidBaseDeletedAt => null;
    private bool ValidBaseIsDeleted => false;

    [Fact(DisplayName = "Criar Book Inválido via Construtor com Id Vazio")]
    [Trait("Book", "Validation - Constructor - Base")]
    public void Constructor_Should_Be_Invalid_When_Id_Is_Empty()
    {
        // Arrange
        var invalidId = Guid.Empty;
        // Act
        var book = new Book(
            invalidId,
            ValidTitle(),
            ValidAuthor(),
            ValidSynopsis(),
            ValidISBN(),
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId(),
            ValidBaseCreatedAt,
            ValidBaseUpdatedAt,
            ValidBaseDeletedAt,
            ValidBaseIsDeleted);
        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, error => error.Contains("Id cannot be empty"));
    }

    [Fact(DisplayName = "Criar Book Inválido via Construtor com CreatedAt Default")]
    [Trait("Book", "Validation - Constructor - Base")]
    public void Constructor_Should_Be_Invalid_When_CreatedAt_Is_Default()
    {
        // Arrange
        var defaultCreatedAt = default(DateTime);
        // Act
        var book = new Book(
            Guid.NewGuid(),
            ValidTitle(),
            ValidAuthor(),
            ValidSynopsis(),
            ValidISBN(),
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId(),
            defaultCreatedAt,
            ValidBaseUpdatedAt,
            ValidBaseDeletedAt,
            ValidBaseIsDeleted);
        // Assert
        Assert.False(book.IsValid());
        Assert.Contains(book.Errors, error => error.Contains("creation date must be provided"));
    }

    [Fact(DisplayName = "Criar Book Válido via Construtor com Todos Parâmetros Válidos")]
    [Trait("Book", "Validation - Constructor")]
    public void Constructor_Should_Be_Valid_When_All_Parameters_Are_Valid()
    {
        // Arrange & Act
        var book = new Book(
            Guid.NewGuid(),
            ValidTitle(),
            ValidAuthor(),
            ValidSynopsis(),
            ValidISBN(),
            ValidCoverImage(),
            ValidGenderId(),
            ValidPublisherId(),
            ValidUserCreatedId(),
            ValidBaseCreatedAt,
            ValidBaseUpdatedAt,
            ValidBaseDeletedAt,
            ValidBaseIsDeleted);
        // Assert
        Assert.True(book.IsValid());
        Assert.Empty(book.Errors);
    }

}