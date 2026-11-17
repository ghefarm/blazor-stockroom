using System.Text;
using Stockroom.Core.Entities;
using Stockroom.Core.Exports;
using Stockroom.Core.Files;

namespace Stockroom.Core.Tests.Files;

public class CsvFieldsTests
{
    [Test]
    public void Export_WritesTheMarkAndQuotes()
    {
        Product box = new()
        {
            Sku = "BX-S", Name = "Box, \"small\"",
            PriceCents = 123450, StockQuantity = 1200,
        };

        var bytes = ProductCsvExport.Create([box]);
        var text = Encoding.UTF8.GetString(bytes.AsSpan(3));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(bytes[..3], Is.EqualTo(new byte[]
                { 0xEF, 0xBB, 0xBF }));
            Assert.That(text, Is.EqualTo(
                "SKU,Name,Price,Stock\r\n"
                + "BX-S,\"Box, \"\"small\"\"\",1234.50,1200\r\n"));
        }
    }

    [Test]
    public void ProtectText_StopsFormulasButNotNumbers()
    {
        string[] dangerous =
            ["=1+1", "+1", "-1", "@SUM(A1)", "\tx", "\rx"];
        Product evil = new()
        {
            Sku = "EV-1", Name = "=HYPERLINK(\"http://x.example\")",
            PriceCents = -1250,
        };

        var row = Encoding.UTF8.GetString(
            ProductCsvExport.Create([evil])).Split("\r\n")[1];

        using (Assert.EnterMultipleScope())
        {
            foreach (var text in dangerous)
            {
                Assert.That(CsvFields.ProtectText(text),
                    Is.EqualTo("'" + text));
            }

            Assert.That(CsvFields.ProtectText("Box"),
                Is.EqualTo("Box"));
            Assert.That(row, Is.EqualTo(
                "EV-1,\"'=HYPERLINK(\"\"http://x.example\"\")\""
                + ",-12.50,0"));
        }
    }
}
