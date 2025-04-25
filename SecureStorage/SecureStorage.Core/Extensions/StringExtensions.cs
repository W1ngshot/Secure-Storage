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
}