using EasyStock.Web.Extensions;

namespace EasyStock.Tests.Unit;

public class DateTimeExtensionsTests
{
    [Theory]
    [InlineData(2026, 1, 15, 5)]
    [InlineData(2026, 7, 15, 4)]
    public void StartOfDayUtc_AppliesTheMontrealOffsetIncludingDaylightSaving(int year, int month, int day, int expectedUtcHour)
    {
        var result = new DateOnly(year, month, day).StartOfDayUtc();

        Assert.Equal(new DateTime(year, month, day, expectedUtcHour, 0, 0, DateTimeKind.Utc), result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    [Fact]
    public void ToDisplay_ConvertsUtcToMontrealTime()
    {
        var utc = new DateTime(2026, 7, 15, 16, 30, 0, DateTimeKind.Utc);

        Assert.Equal("2026-07-15 12:30", utc.ToDisplay());
    }
}