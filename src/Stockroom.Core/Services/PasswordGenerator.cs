using System.Security.Cryptography;

namespace Stockroom.Core.Services;

public static class PasswordGenerator
{
    // No 0, O, 1, l or I: an administrator reads the password out.
    private const string Lower = "abcdefghijkmnopqrstuvwxyz";
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Digits = "23456789";
    private const string Symbols = "!#$%+-=?@";
    private const int Length = 16;

    public static string Create()
    {
        // One character of each kind first, so every rule is met.
        var text = string.Concat(
            RandomNumberGenerator.GetString(Lower, 1),
            RandomNumberGenerator.GetString(Upper, 1),
            RandomNumberGenerator.GetString(Digits, 1),
            RandomNumberGenerator.GetString(Symbols, 1),
            RandomNumberGenerator.GetString(
                Lower + Upper + Digits + Symbols, Length - 4));

        var chars = text.ToCharArray();
        RandomNumberGenerator.Shuffle<char>(chars);
        return new string(chars);
    }
}
