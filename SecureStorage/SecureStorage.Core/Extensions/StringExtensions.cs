using System.Text;

namespace SecureStorage.Core.Extensions;


public static class StringExtensions
{
    /// <summary>
    /// Преобразует строку в массив байт (UTF-8).
    /// </summary>
    public static byte[] ToBytes(this string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }

    /// <summary>
    /// Преобразует массив байт в строку (UTF-8).
    /// </summary>
    public static string FromBytes(this byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    public static byte[] FromBase64String(this string base64)
    {
        return Convert.FromBase64String(base64);
    }

    public static string ToBase64String(this byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }
}