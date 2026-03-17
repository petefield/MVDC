using MVDC.FullTime.Helpers;
using Xunit;

namespace MVDC.FullTime.Tests.Helpers;

public class StringHelperTests
{
    [Fact]
    public void RemoveWhitespace_TrimsLeadingAndTrailingSpaces()
    {
        var result = StringHelper.RemoveWhitespace("  hello world  ");
        Assert.Equal("hello world", result);
    }

    [Fact]
    public void RemoveWhitespace_RemovesNewlines()
    {
        // Newlines are removed (not replaced with spaces), then \s+ collapses remaining whitespace
        var result = StringHelper.RemoveWhitespace("hello\nworld\r\nfoo");
        Assert.Equal("helloworldfoo", result);
    }

    [Fact]
    public void RemoveWhitespace_CollapsesMultipleSpaces()
    {
        var result = StringHelper.RemoveWhitespace("hello    world     foo");
        Assert.Equal("hello world foo", result);
    }

    [Fact]
    public void RemoveWhitespace_HandlesComplexWhitespace()
    {
        var result = StringHelper.RemoveWhitespace("  hello  \n  world  \r\n  foo  ");
        Assert.Equal("hello world foo", result);
    }

    [Fact]
    public void RemoveWhitespace_EmptyString_ReturnsEmpty()
    {
        var result = StringHelper.RemoveWhitespace("");
        Assert.Equal("", result);
    }

    [Fact]
    public void RemoveWhitespace_OnlyWhitespace_ReturnsEmpty()
    {
        var result = StringHelper.RemoveWhitespace("   \n  \r\n  ");
        Assert.Equal("", result);
    }
}
