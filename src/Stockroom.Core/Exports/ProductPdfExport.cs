using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Exports;

public static class ProductPdfExport
{
    public static byte[] Create(
        IReadOnlyList<Product> products, DateOnly today)
    {
        var document = new Document();
        document.Styles[StyleNames.Normal]!.Font.Size = 10;

        var section = document.AddSection();
        section.PageSetup = document.DefaultPageSetup.Clone();
        section.PageSetup.PageFormat = PageFormat.Letter;

        var title = section.AddParagraph(
            $"Products, {BusinessFormats.FormatDate(today)}");
        title.Format.Font.Size = 16;
        title.Format.Font.Bold = true;
        title.Format.SpaceAfter = Unit.FromPoint(10);

        FillTable(section.AddTable(), products);

        var footer = section.Footers.Primary.AddParagraph();
        footer.Format.Alignment = ParagraphAlignment.Center;
        footer.AddText("Page ");
        footer.AddPageField();
        footer.AddText(" of ");
        footer.AddNumPagesField();

        return Render(document);
    }

    private static void FillTable(
        Table table, IReadOnlyList<Product> products)
    {
        table.AddColumn(Unit.FromPoint(80));
        table.AddColumn(Unit.FromPoint(260));
        table.AddColumn(Unit.FromPoint(80));
        table.AddColumn(Unit.FromPoint(60));

        // A heading row is repeated on every page.
        var header = table.AddRow();
        header.HeadingFormat = true;
        header.Format.Font.Bold = true;
        header.Borders.Bottom.Width = 1;
        AddCells(header, "SKU", "Name", "Price", "Stock");

        foreach (var p in products)
        {
            AddCells(table.AddRow(), p.Sku, p.Name,
                Money.Format(p.PriceCents),
                BusinessFormats.FormatQuantity(p.StockQuantity));
        }
    }

    private static void AddCells(Row row, params string[] texts)
    {
        for (var i = 0; i < texts.Length; i++)
        {
            row.Cells[i].AddParagraph(texts[i]);
        }

        // Numbers line up on the right.
        row.Cells[2].Format.Alignment = ParagraphAlignment.Right;
        row.Cells[3].Format.Alignment = ParagraphAlignment.Right;
    }

    private static byte[] Render(Document document)
    {
        // Lays out the paragraphs and tables on PDFsharp pages.
        var renderer = new PdfDocumentRenderer
        {
            Document = document,
        };
        renderer.RenderDocument();

        using var stream = new MemoryStream();
        renderer.PdfDocument.Save(stream, false);
        return stream.ToArray();
    }
}
