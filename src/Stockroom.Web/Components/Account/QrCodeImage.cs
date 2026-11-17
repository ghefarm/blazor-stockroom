using QRCoder;

namespace Stockroom.Web.Components.Account;

public static class QrCodeImage
{
    /// <summary>
    /// Returns the QR code of a text as a data address for an img
    /// element. Made on the server, so the page needs no script.
    /// </summary>
    public static string CreateDataUrl(string text)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(
            text, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data).GetGraphic(5);
        return "data:image/png;base64," + Convert.ToBase64String(png);
    }
}
