namespace Martina.Tests.Utils;

public class DateTimeOffsetTests
{
    [Fact]
    public void UnixTimeStampTest()
    {
        DateTimeOffset first = new(new DateTime(2025, 5, 1));
        DateTimeOffset second = DateTimeOffset.FromUnixTimeSeconds(first.ToUnixTimeSeconds());
        second = TimeZoneInfo.ConvertTime(second, TimeZoneInfo.Local);

        Assert.Equal(first, second);
    }
}
