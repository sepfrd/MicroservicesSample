using System.Security.Cryptography;
using System.Text;

namespace ToDoListManager.Common.Helpers;

public static class PasswordHelper
{
    public static async Task<string> GetHashStringAsync(this string inputString)
    {
        using HashAlgorithm algorithm = SHA512.Create();

        return Get(await algorithm.ComputeHashAsync(new MemoryStream(Encoding.UTF8.GetBytes(inputString))));
    }

    private static string Get(IReadOnlyCollection<byte> array)
    {
        var hex = new StringBuilder(array.Count * 2);

        foreach (var b in array)
        {
            hex.Append($"{b:x2}");
        }

        return hex.ToString();
    }
}