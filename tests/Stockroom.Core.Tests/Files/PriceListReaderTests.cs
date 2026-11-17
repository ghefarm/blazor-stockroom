using System.Text;
using Stockroom.Core.Files;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Files;

public class PriceListReaderTests
{
    private const string SupplierFile =
        "SKU;Name;Price\r\n"
        + "TP-050;Tape, cr\u00E8me;2,59\r\n"
        + "BX-S;\"Box; small\";0,89\r\n"
        + "GL-M;\"Gloves \"\"M\"\",\r\nsize 8\";4.90\r\n"
        + "\r\n";

    [Test]
    public void Read_UnderstandsBothEncodingsAndQuotes()
    {
        var withBom = CsvText.EncodeWithBom(SupplierFile);
        var latin1 = Encoding.Latin1.GetBytes(SupplierFile);

        var fromBom = PriceListReader.Read(withBom);
        var fromLatin1 = PriceListReader.Read(latin1);
        var rows = CsvParser.Parse(CsvText.Decode(latin1), ';');

        PriceChange[] expected =
            [new("TP-050", 259), new("BX-S", 89), new("GL-M", 490)];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(fromBom.Changes, Is.EqualTo(expected));
            Assert.That(fromBom.Errors, Is.Empty);
            Assert.That(fromLatin1.Changes, Is.EqualTo(expected));
            Assert.That(rows, Has.Count.EqualTo(4));
            Assert.That(rows[1][1], Is.EqualTo("Tape, cr\u00E8me"));
            Assert.That(rows[3][1],
                Is.EqualTo("Gloves \"M\",\nsize 8"));
        }
    }

    [Test]
    public void Read_ReportsEveryBadRowAndTheMark()
    {
        var bad = Encoding.Latin1.GetBytes(
            "SKU;Price\n;1,00\nTP-050;2,59\nTP-050;2,60\n"
            + "BX-S;0,899\nGL-M;-4,90\n");
        var noPrice = Encoding.Latin1.GetBytes("SKU;Cost\nBX-S;1\n");

        // The mistake: the mark read as Latin-1 becomes text.
        var withBom = CsvText.EncodeWithBom("SKU;Price\n");
        var wrong = Encoding.Latin1.GetString(withBom);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(PriceListReader.Read(bad).Errors, Is.EqualTo(
            [
                "Row 2: the SKU is missing.",
                "Row 4: TP-050 appears twice.",
                "Row 5: \"0,899\" is not a price.",
                "Row 6: \"-4,90\" is not a price.",
            ]));
            Assert.That(PriceListReader.Read(noPrice).Errors,
                Has.One.StartsWith("The first row needs"));
            Assert.That(wrong,
                Does.StartWith("\u00EF\u00BB\u00BFSKU"));
        }
    }
}
