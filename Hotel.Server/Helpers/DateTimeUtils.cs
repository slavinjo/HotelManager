using System;

namespace Hotel.Server.Helpers;

public class DateTimeUtils
{
    public static long ConvertToUnixTimestamp(DateTime date)
    {
        var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        var diff = date.ToUniversalTime() - origin;
        return Convert.ToInt64(Math.Floor(diff.TotalSeconds));
    }
}
