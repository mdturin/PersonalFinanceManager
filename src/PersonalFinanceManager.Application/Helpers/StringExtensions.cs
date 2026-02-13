using System.Security.Cryptography;
using System.Text;

namespace PersonalFinanceManager.Application.Helpers;

public static class StringExtensions
{
    public static string ToCheckSum(this string input)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    }
}
