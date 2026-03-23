namespace ZeyticAuth.AspNetCore.Authentication.Tests;

public class ZeyticAuthUtilsTests_GetExpiresAt
{
    [Fact]
    public void ShouldReturnFutureDate()
    {
        // Add 1 second to the current time, should be in the future since tests are fast
        var expiresAtString = ZeyticAuthUtils.GetExpiresAt(1);
        var expiresAt = DateTimeOffset.Parse(expiresAtString);
        var now = DateTimeOffset.UtcNow;
        Assert.True(expiresAt > now);
    }
}

public class ZeyticAuthUtilsTests_IsExpired
{
    [Fact]
    public void ShouldReturnTrueIfTimeStringIsNullOrEmpty()
    {
        Assert.True(ZeyticAuthUtils.IsExpired(null));
        Assert.True(ZeyticAuthUtils.IsExpired(string.Empty));
    }

    [Fact]
    public void ShouldReturnTrueIfTimeStringIsNotParsable()
    {
        Assert.True(ZeyticAuthUtils.IsExpired("foo"));
    }

    [Fact]
    public void ShouldReturnTrueIfTimeStringIsExpired()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(-1);
        Assert.True(ZeyticAuthUtils.IsExpired(expiresAt.ToString("o")));
    }

    [Fact]
    public void ShouldReturnFalseIfTimeStringIsNotExpired()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(1);
        Assert.False(ZeyticAuthUtils.IsExpired(expiresAt.ToString("o")));
    }
}
