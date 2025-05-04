using DesafioCCAA.Domain.Domains.Books.ValueObjects;

namespace DesafioCCAA.Domain.Test;

public class CoverImageTests
{
    private string ValidFileName() => $"imagem_valida{GetRandomAllowedExtension()}";
    private string ValidPath() => "/caminho/para/imagem";
    private string GetRandomAllowedExtension()
    {
        var random = new Random();
        return CoverImage.AllowedExtensions[random.Next(CoverImage.AllowedExtensions.Length)];
    }


    [Fact(DisplayName = "Criar CoverImage Válido com Nome e Caminho Válidos")]
    [Trait("CoverImage", "Validation")]
    public void Constructor_Should_Be_Valid_When_FileName_And_Path_Are_Valid()
    {
        // Arrange
        var validFileName = ValidFileName();
        var validPath = ValidPath();

        // Act
        var coverImage = new CoverImage(validFileName, validPath);

        // Assert
        Assert.True(coverImage.IsValid());
        Assert.Empty(coverImage.Errors);
    }

    [Fact(DisplayName = "Criar CoverImage Inválido com FileName Vazio")]
    [Trait("CoverImage", "Validation")]
    public void Constructor_Should_Be_Invalid_When_FileName_Is_Empty()
    {
        // Arrange
        var emptyFileName = "";
        var validPath = ValidPath();

        // Act
        var coverImage = new CoverImage(emptyFileName, validPath);

        // Assert
        Assert.False(coverImage.IsValid());
        Assert.Contains(coverImage.Errors, e => e.Contains("Nome do arquivo é obrigatório"));
    }

    [Theory(DisplayName = "Criar CoverImage Inválido com Extensão Inválida")]
    [Trait("CoverImage", "Validation")]
    [InlineData("imagem.txt")]
    [InlineData("imagem.doc")]
    [InlineData("imagem.pdf")]
    [InlineData("imagem_sem_extensao")]
    [InlineData("imagem.")]
    public void Constructor_Should_Be_Invalid_When_FileName_Has_Invalid_Extension(string invalidFileName)
    {
        // Arrange
        var validPath = ValidPath();

        // Act
        var coverImage = new CoverImage(invalidFileName, validPath);

        // Assert
        Assert.False(coverImage.IsValid());
        Assert.Contains(coverImage.Errors, e => e.StartsWith("Extensão inválida. Permitido:"));
    }

    [Theory(DisplayName = "Criar CoverImage Válido com Extensões Permitidas (Case Insensitive)")]
    [Trait("CoverImage", "Validation")]
    [InlineData("imagem.jpg")]
    [InlineData("imagem.JPG")]
    [InlineData("imagem.jpeg")]
    [InlineData("imagem.JPEG")]
    [InlineData("imagem.png")]
    [InlineData("imagem.PNG")]
    [InlineData("imagem.gif")]
    [InlineData("imagem.GIF")]
    public void Constructor_Should_Be_Valid_When_FileName_Has_Allowed_Extension(string validFileName)
    {
        // Arrange
        var validPath = ValidPath();

        // Act
        var coverImage = new CoverImage(validFileName, validPath);

        // Assert
        Assert.DoesNotContain(coverImage.Errors, e => e.StartsWith("Extensão inválida."));
        Assert.True(coverImage.IsValid());
        Assert.Empty(coverImage.Errors);
    }

    [Fact(DisplayName = "Criar CoverImage Inválido com Path Nulo")]
    [Trait("CoverImage", "Validation")]
    public void Constructor_Should_Be_Invalid_When_Path_Is_Null()
    {
        // Arrange
        var validFileName = ValidFileName();
        string nullPath = null;

        // Act
        var coverImage = new CoverImage(validFileName, nullPath);

        // Assert
        Assert.False(coverImage.IsValid());
        Assert.Contains(coverImage.Errors, e => e.Contains("Caminho da imagem é obrigatório"));
    }

    [Fact(DisplayName = "Criar CoverImage Inválido com Path Vazio")]
    [Trait("CoverImage", "Validation")]
    public void Constructor_Should_Be_Invalid_When_Path_Is_Empty()
    {
        // Arrange
        var validFileName = ValidFileName();
        var emptyPath = "";

        // Act
        var coverImage = new CoverImage(validFileName, emptyPath);

        // Assert
        Assert.False(coverImage.IsValid());
        Assert.Contains(coverImage.Errors, e => e.Contains("Caminho da imagem é obrigatório"));
    }
}
