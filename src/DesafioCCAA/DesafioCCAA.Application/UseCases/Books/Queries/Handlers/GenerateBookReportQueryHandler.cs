using DesafioCCAA.Application.UseCases.Books.ViewModels;
using DesafioCCAA.Domain.Exceptions;
using DesafioCCAA.Domain.Services.Interfaces;
using DesafioCCAA.Shared.Responses;
using MediatR;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DesafioCCAA.Application.UseCases.Books.Queries.Handlers;

public sealed class GenerateBookReportQueryHandler
    : IRequestHandler<GenerateBookReportQuery, BaseResult<byte[]>>
{
    private readonly IMediator _mediator;
    private readonly IFileService _fileService;
    private readonly IApplicationUserService _userService;
    private readonly ILogger<GenerateBookReportQueryHandler> _logger;

    public GenerateBookReportQueryHandler(
        IMediator mediator,
        IFileService fileService,
        IApplicationUserService applicationUserService,
        ILogger<GenerateBookReportQueryHandler> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _userService = applicationUserService ?? throw new ArgumentNullException(nameof(applicationUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BaseResult<byte[]>> Handle(GenerateBookReportQuery request, CancellationToken ct)
    {
        var user = await _userService.FindByIdAsync(request.Id);
        if (user is null)
        {
            _logger.LogError("User not found for report generation (ID: {UserId})", request.Id);
            throw new NotFoundException("Usuário não encontrado");
        }

        var resultBooks = await _mediator.Send(new GetByIdUserCreatedBook(request.Id), ct);

        if (resultBooks?.Success is not true || resultBooks.Data is null || resultBooks.Data.Count() == 0)
        {
            _logger.LogWarning("No books found for user (ID: {UserId}) to generate report", request.Id);
            return  new BaseResult<byte[]>(Array.Empty<byte>(), false, "Nenhum livro encontrado para gerar o relatório.");
        }

        IEnumerable<BookViewModel> books = resultBooks.Data;
        var images = await LoadImagesAsync(books, ct);

        _logger.LogInformation("Generating PDF report for user {UserId} with {BookCount} books",
            request.Id, books.Count());

        var pdfBytes = BuildPdf(user.UserName ?? "N/A", books, images);

        _logger.LogInformation("PDF report generated successfully for user {UserId}. Size: {PdfSize} bytes",
            request.Id, pdfBytes.Length);

        return new BaseResult<byte[]>(pdfBytes);
    }

    #region Helpers
    private async Task<Dictionary<Guid, byte[]>> LoadImagesAsync(IEnumerable<BookViewModel>? books, CancellationToken ct)
    {
        var images = new Dictionary<Guid, byte[]>();

        foreach (var book in books)
        {
            if (book.CoverImage?.FileName is not { Length: > 0 })
                continue;

            try
            {
                var base64 = await _fileService.GetFileBase64Async(book.CoverImage.FileName);
                if (!string.IsNullOrWhiteSpace(base64))
                    images[book.Id] = Convert.FromBase64String(base64);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex,
                    "Cover image not found for book {BookId}: {FileName}", book.Id, book.CoverImage.FileName);
            }
            catch (FormatException ex)
            {
                _logger.LogWarning(ex,
                    "Cover image Base64 inválida para o livro {BookId}: {FileName}", book.Id, book.CoverImage.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao processar a capa do livro {BookId}: {FileName}", book.Id, book.CoverImage.FileName);
            }
        }

        return images;
    }

    private static byte[] BuildPdf(
        string userName,
        IEnumerable<BookViewModel>? books,
        IReadOnlyDictionary<Guid, byte[]> images)
    {
        const float TextPadding = 4;
        const float FontSize = 9;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4.Landscape());

                #region Cabeçalho
                page.Header()
                    .PaddingBottom(15)
                    .Column(col =>
                    {
                        col.Spacing(5);
                        col.Item().Text($"Relatório de Livros – {DateTime.Now:dd/MM/yyyy HH:mm}")
                                  .FontSize(16).SemiBold().FontColor(Colors.Grey.Darken3);
                        col.Item().Text($"Usuário: {userName}")
                                  .FontSize(10).Italic();
                        col.Item().PaddingTop(5)
                                  .LineHorizontal(0.5f)
                                  .LineColor(Colors.Grey.Lighten1);
                    });
                #endregion

                #region Tabela
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(65);   // Capa
                        c.RelativeColumn(2.5f); // Título
                        c.RelativeColumn(2f);   // Autor
                        c.RelativeColumn(3.5f); // Sinopse
                        c.ConstantColumn(90);   // ISBN
                        c.RelativeColumn(1.5f); // Gênero
                        c.RelativeColumn(1.5f); // Editora
                    });

                    // Cabeçalho
                    static void HeaderCell(TableDescriptor tbl, string text) =>
                        tbl.Cell().HeaderCell().Text(text).SemiBold().FontSize(9);

                    HeaderCell(table, "Capa");
                    HeaderCell(table, "Título");
                    HeaderCell(table, "Autor");
                    HeaderCell(table, "Sinopse");
                    HeaderCell(table, "ISBN");
                    HeaderCell(table, "Gênero");
                    HeaderCell(table, "Editora");

                    // Linhas
                    var rowIndex = 0;
                    foreach (var book in books)
                    {
                        var bg = rowIndex++ % 2 == 1 ? Colors.Grey.Lighten5 : Colors.White;
                        AddRow(book, images.TryGetValue(book.Id, out var img) ? img : null, bg);
                    }

                    // --- função local para adicionar linha ---
                    void AddRow(BookViewModel b, byte[]? cover, string background)
                    {
                        table.Cell().BaseCell(background)
                                    .MinHeight(60)             // altura mínima na célula
                                    .Element(inner =>
                                    {
                                        if (cover?.Length > 0)
                                            inner.Image(cover).FitArea();
                                        else
                                            inner.AlignCenter().AlignMiddle()
                                                 .Text("s/img").FontSize(7).FontColor(Colors.Grey.Medium);
                                    });

                        table.Cell().BaseCell(background, TextPadding)
                                    .Element(e => e.AlignMiddle().Text(b.Title ?? "-").FontSize(FontSize));
                        table.Cell().BaseCell(background, TextPadding)
                                    .Element(e => e.AlignMiddle().Text(b.Author ?? "-").FontSize(FontSize));
                        table.Cell().BaseCell(background, TextPadding)
                                    .Element(e => e.AlignTop().Text(b.Synopsis ?? "-").FontSize(FontSize - 1));
                        table.Cell().BaseCell(background, TextPadding)
                                    .Element(e => e.AlignMiddle().Text(b.ISBN ?? "-").FontSize(FontSize));
                        table.Cell().BaseCell(background, TextPadding)
                                    .Element(e => e.AlignMiddle().Text(b.GenderName ?? "-").FontSize(FontSize));
                        table.Cell().BaseCell(background, TextPadding)
                                    .Element(e => e.AlignMiddle().Text(b.PublisherName ?? "-").FontSize(FontSize));
                    }
                });
                #endregion

                #region Rodapé
                page.Footer()
                    .PaddingTop(10)
                    .Column(col =>
                    {
                        col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                        col.Item().PaddingTop(5).AlignCenter().Text(x =>
                        {
                            x.Span("Página ").FontSize(8);
                            x.CurrentPageNumber().FontSize(8);
                            x.Span(" / ").FontSize(8);
                            x.TotalPages().FontSize(8);
                        });
                    });
                #endregion
            });
        });

        return doc.GeneratePdf();
    }
    #endregion
}

#region Extensões QuestPDF
internal static class PdfCellExtensions
{
    private static readonly Color BorderColor = Colors.Grey.Lighten2;

    public static IContainer BaseCell(this IContainer c, string bg, float padding = 2) =>
        c.Background(bg)
         .BorderBottom(1).BorderColor(BorderColor)
         .Padding(padding);

    public static IContainer HeaderCell(this IContainer c) =>
        c.Background(Colors.Grey.Lighten3)
         .PaddingVertical(4)
         .PaddingHorizontal(6)
         .AlignCenter()
         .BorderBottom(1.5f).BorderColor(Colors.Grey.Medium);
}
#endregion