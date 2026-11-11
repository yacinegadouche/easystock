using System.Globalization;

namespace EasyStock.Web.Extensions;

public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo DisplayTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("America/Toronto");

    public static string ToDisplay(this DateTime utcDateTime) =>
        TimeZoneInfo
            .ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc), DisplayTimeZone)
            .ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
}