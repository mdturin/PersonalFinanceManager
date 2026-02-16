using System.Security.Cryptography;
using System.Text;

namespace PersonalFinanceManager.Application.Helpers;

public static class StringExtensions
{
    public static string ToCheckSum(this string input)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    }

    public static string ToNormalizeString(this string input, char divider = '-')
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Trim and convert to lower
        input = input.Trim().ToLowerInvariant();

        var sb = new StringBuilder();
        bool previousWasDivider = false;

        foreach (var ch in input)
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
                previousWasDivider = false;
            }
            else
            {
                if (!previousWasDivider)
                {
                    sb.Append(divider);
                    previousWasDivider = true;
                }
            }
        }

        // Remove leading/trailing divider
        return sb.ToString().Trim(divider);
    }
}
