namespace Licitaciones.Web.Helpers;

public static class CostaRicaDateTimeExtensions
{
    private static readonly TimeZoneInfo CostaRicaTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");

    public static DateTime ToCostaRicaTime(this DateTime dateTime)
    {
        var utcDateTime = dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };

        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, CostaRicaTimeZone);
    }

    public static DateTime ToCostaRicaTime(this DateTimeOffset dateTimeOffset)
    {
        return TimeZoneInfo.ConvertTime(dateTimeOffset, CostaRicaTimeZone).DateTime;
    }
}