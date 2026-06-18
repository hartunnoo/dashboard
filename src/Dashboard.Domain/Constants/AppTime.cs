namespace Dashboard.Domain.Constants;

public static class AppTime
{
    public static readonly TimeZoneInfo BruneiTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Singapore");

    public static DateTime NowInBrunei() =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BruneiTimeZone);

    public static DateTime TodayInBrunei() => NowInBrunei().Date;
}
