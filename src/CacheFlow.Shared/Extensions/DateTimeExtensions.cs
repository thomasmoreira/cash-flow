using System.Globalization;

namespace CacheFlow.Shared.Extensions;

public static class DateTimeExtensions
{
    public static DateTime FormatAndParse(this DateTime date, string format = "yyyy-MM-dd HH:mm:ss", IFormatProvider provider = null)
    {
        provider ??= CultureInfo.InvariantCulture;
        string formatted = date.ToString(format, provider);

        if (DateTime.TryParseExact(formatted, format, provider, DateTimeStyles.None, out DateTime parsed))
        {
            return parsed;
        }
        else
        {
            // Se a conversão falhar, retorne o valor original (ou pode lançar uma exceção, conforme sua necessidade)
            return date;
        }
    }
}
