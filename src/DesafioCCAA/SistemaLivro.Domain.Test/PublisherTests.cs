using SistemaLivro.Domain.Domains.Books.Entities;

namespace SistemaLivro.Domain.Test;

public class PublisherTests
{

    [Fact(DisplayName = "Criar Publisher Válido via Create com Nome e Descrição Válidos")]
    [Trait("Publisher", "Validation - Create")]
    public void Create_Should_Be_Valid_When_Name_And_Description_Are_Valid()
    {
        // Arrange
        var validName = "Editora Válida Ltda.";
        var validDescription = "Descrição da editora.";
        // Act
        var publisher = Publisher.Create(validName, validDescription);
        // Assert
        Assert.True(publisher.IsValid());
        Assert.Empty(publisher.Errors);
    }

    [Fact(DisplayName = "Criar Publisher Inválido via Create com Nome Nulo")]
    [Trait("Publisher", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_Name_Is_Null()
    {
        // Arrange
        string nullName = null;
        var validDescription = "Descrição ok";
        // Act
        var publisher = Publisher.Create(nullName, validDescription);
        // Assert
        Assert.False(publisher.IsValid());
        Assert.Contains(publisher.Errors, error => error.Contains("nome é obrigatório"));
    }

    [Fact(DisplayName = "Criar Publisher Inválido via Create com Nome Vazio")]
    [Trait("Publisher", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_Name_Is_Empty()
    {
        // Arrange
        var emptyName = "";
        var validDescription = "Descrição ok";
        // Act
        var publisher = Publisher.Create(emptyName, validDescription);
        // Assert
        Assert.False(publisher.IsValid());
        Assert.Contains(publisher.Errors, error => error.Contains("nome é obrigatório"));
    }

    [Fact(DisplayName = "Criar Publisher Inválido via Create com Nome Excedendo Tamanho Máximo")]
    [Trait("Publisher", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_Name_Exceeds_MaxLength()
    {
        // Arrange
        var longName = new string('A', 151);
        var validDescription = "Descrição ok";
        // Act
        var publisher = Publisher.Create(longName, validDescription);
        // Assert
        Assert.False(publisher.IsValid());
        Assert.Contains(publisher.Errors, error => error.Contains("não pode exceder 150 caracteres"));
    }

    [Fact(DisplayName = "Criar Publisher Válido via Create com Nome no Tamanho Máximo")]
    [Trait("Publisher", "Validation - Create")]
    public void Create_Should_Be_Valid_When_Name_Is_At_MaxLength()
    {
        // Arrange
        var exactName = new string('A', 150);
        var validDescription = "Descrição ok";
        // Act
        var publisher = Publisher.Create(exactName, validDescription);
        // Assert
        Assert.True(publisher.IsValid());
        Assert.Empty(publisher.Errors);
    }

    [Fact(DisplayName = "Criar Publisher Inválido via Create com Descrição Excedendo Tamanho Máximo")]
    [Trait("Publisher", "Validation - Create")]
    public void Create_Should_Be_Invalid_When_Description_Exceeds_MaxLength()
    {
        // Arrange
        var validName = "Editora Válida";
        var longDescription = new string('B', 4001);
        // Act
        var publisher = Publisher.Create(validName, longDescription);
        // Assert
        Assert.False(publisher.IsValid());
        Assert.Contains(publisher.Errors, error => error.Contains("não pode exceder 4000 caracteres"));
    }

    [Fact(DisplayName = "Criar Publisher Válido via Create com Descrição no Tamanho Máximo")]
    [Trait("Publisher", "Validation - Create")]
    public void Create_Should_Be_Valid_When_Description_Is_At_MaxLength()
    {
        // Arrange
        var validName = "Editora Válida";
        var exactDescription = new string('B', 4000);
        // Act
        var publisher = Publisher.Create(validName, exactDescription);
        // Assert
        Assert.True(publisher.IsValid());
        Assert.Empty(publisher.Errors);
    }

    [Fact(DisplayName = "Criar Publisher Válido via Create com Descrição Nula")]
    [Trait("Publisher", "Validation - Create")]
    public void Create_Should_Be_Valid_When_Description_Is_Null()
    {
        // Arrange
        var validName = "Editora Válida";
        string nullDescription = null;
        // Act
        var publisher = Publisher.Create(validName, nullDescription);
        // Assert
        Assert.True(publisher.IsValid());
        Assert.Empty(publisher.Errors);
    }

    [Fact(DisplayName = "Criar Publisher Válido via Create com Descrição Vazia")]
    [Trait("Publisher", "Validation - Create")]
    public void Create_Should_Be_Valid_When_Description_Is_Empty()
    {
        // Arrange
        var validName = "Editora Válida";
        var emptyDescription = "";
        // Act
        var publisher = Publisher.Create(validName, emptyDescription);
        // Assert
        Assert.True(publisher.IsValid());
        Assert.Empty(publisher.Errors);
    }

    private Guid ValidId => Guid.NewGuid();
    private string ValidName => "Nome Editora Construtor";
    private string ValidDesc => "Desc Editora Construtor";
    private DateTime ValidCreatedAt => DateTime.UtcNow.AddDays(-1);
    private DateTime? ValidUpdatedAt => null;
    private DateTime? ValidDeletedAt => null;
    private bool ValidIsDeleted => false;


    [Fact(DisplayName = "Criar Publisher Inválido via Construtor com Id Vazio")]
    [Trait("Publisher", "Validation - Constructor - Base")]
    public void Constructor_Should_Be_Invalid_When_Id_Is_Empty()
    {
        // Arrange
        var invalidId = Guid.Empty;
        // Act
        var publisher = new Publisher(invalidId, ValidName, ValidDesc, ValidCreatedAt, ValidUpdatedAt, ValidDeletedAt, ValidIsDeleted);
        // Assert
        Assert.False(publisher.IsValid());
        Assert.Contains(publisher.Errors, error => error.Contains("Id cannot be empty"));
    }

    [Fact(DisplayName = "Criar Publisher Inválido via Construtor com CreatedAt Default")]
    [Trait("Publisher", "Validation - Constructor - Base")]
    public void Constructor_Should_Be_Invalid_When_CreatedAt_Is_Default()
    {
        // Arrange
        var defaultCreatedAt = default(DateTime);
        // Act
        var publisher = new Publisher(ValidId, ValidName, ValidDesc, defaultCreatedAt, ValidUpdatedAt, ValidDeletedAt, ValidIsDeleted);
        // Assert
        Assert.False(publisher.IsValid());
        Assert.Contains(publisher.Errors, error => error.Contains("creation date must be provided"));
    }

    [Fact(DisplayName = "Criar Publisher Inválido via Construtor com CreatedAt Anterior a 1900")]
    [Trait("Publisher", "Validation - Constructor - Base")]
    public void Constructor_Should_Be_Invalid_When_CreatedAt_Is_Before_1900()
    {
        // Arrange
        var earlyCreatedAt = new DateTime(1899, 12, 31);
        // Act
        var publisher = new Publisher(ValidId, ValidName, ValidDesc, earlyCreatedAt, ValidUpdatedAt, ValidDeletedAt, ValidIsDeleted);
        // Assert
        Assert.False(publisher.IsValid());
        Assert.Contains(publisher.Errors, error => error.Contains("creation date must be between 1900 and 3000"));
    }

    [Fact(DisplayName = "Criar Publisher Inválido via Construtor com CreatedAt Posterior a 3000")]
    [Trait("Publisher", "Validation - Constructor - Base")]
    public void Constructor_Should_Be_Invalid_When_CreatedAt_Is_After_3000()
    {
        // Arrange
        var lateCreatedAt = new DateTime(3001, 1, 1);
        // Act
        var publisher = new Publisher(ValidId, ValidName, ValidDesc, lateCreatedAt, ValidUpdatedAt, ValidDeletedAt, ValidIsDeleted);
        // Assert
        Assert.False(publisher.IsValid());
        Assert.Contains(publisher.Errors, error => error.Contains("creation date must be between 1900 and 3000"));
    }

    [Fact(DisplayName = "Criar Publisher Inválido via Construtor com UpdatedAt Anterior a 1900")]
    [Trait("Publisher", "Validation - Constructor - Base")]
    public void Constructor_Should_Be_Invalid_When_UpdatedAt_Is_Before_1900()
    {
        // Arrange
        var earlyUpdatedAt = new DateTime(1899, 12, 31);
        var createdAt = new DateTime(1950, 1, 1);
        // Act
        var publisher = new Publisher(ValidId, ValidName, ValidDesc, createdAt, earlyUpdatedAt, ValidDeletedAt, ValidIsDeleted);
        // Assert
        Assert.False(publisher.IsValid());
        Assert.Contains(publisher.Errors, error => error.Contains("UpdatedAt must be a valid date between 1900 and 3000 or null"));
    }

    [Fact(DisplayName = "Criar Publisher Inválido via Construtor com UpdatedAt Posterior a 3000")]
    [Trait("Publisher", "Validation - Constructor - Base")]
    public void Constructor_Should_Be_Invalid_When_UpdatedAt_Is_After_3000()
    {
        // Arrange
        var lateUpdatedAt = new DateTime(3001, 1, 1);
        // Act
        var publisher = new Publisher(ValidId, ValidName, ValidDesc, ValidCreatedAt, lateUpdatedAt, ValidDeletedAt, ValidIsDeleted);
        // Assert
        Assert.False(publisher.IsValid());
        Assert.Contains(publisher.Errors, error => error.Contains("UpdatedAt must be a valid date between 1900 and 3000 or null"));
    }

    [Fact(DisplayName = "Criar Publisher Inválido via Construtor com DeletedAt Anterior a 1900")]
    [Trait("Publisher", "Validation - Constructor - Base")]
    public void Constructor_Should_Be_Invalid_When_DeletedAt_Is_Before_1900()
    {
        // Arrange
        var earlyDeletedAt = new DateTime(1899, 12, 31);
        var createdAt = new DateTime(1950, 1, 1);
        // Act
        var publisher = new Publisher(ValidId, ValidName, ValidDesc, createdAt, ValidUpdatedAt, earlyDeletedAt, true);
        // Assert
        Assert.False(publisher.IsValid());
        Assert.Contains(publisher.Errors, error => error.Contains("DeletedAt must be a valid date between 1900 and 3000 or null"));
    }

    [Fact(DisplayName = "Criar Publisher Inválido via Construtor com DeletedAt Posterior a 3000")]
    [Trait("Publisher", "Validation - Constructor - Base")]
    public void Constructor_Should_Be_Invalid_When_DeletedAt_Is_After_3000()
    {
        // Arrange
        var lateDeletedAt = new DateTime(3001, 1, 1);
        // Act
        var publisher = new Publisher(ValidId, ValidName, ValidDesc, ValidCreatedAt, ValidUpdatedAt, lateDeletedAt, true);
        // Assert
        Assert.False(publisher.IsValid());
        Assert.Contains(publisher.Errors, error => error.Contains("DeletedAt must be a valid date between 1900 and 3000 or null"));
    }

    [Fact(DisplayName = "Criar Publisher Válido via Construtor com Todos Parâmetros Válidos")]
    [Trait("Publisher", "Validation - Constructor")]
    public void Constructor_Should_Be_Valid_When_All_Parameters_Are_Valid()
    {
        // Arrange & Act
        var publisher = new Publisher(ValidId, ValidName, ValidDesc, ValidCreatedAt, ValidUpdatedAt, ValidDeletedAt, ValidIsDeleted);
        // Assert
        Assert.True(publisher.IsValid());
        Assert.Empty(publisher.Errors);
    }
}