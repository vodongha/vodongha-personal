using NUnit.Framework;
using Shouldly;
using VodonghaPersonal.Client.Components.Shared;

namespace VodonghaPersonal.Client.Tests;

[TestFixture]
public class ChatHubParserTests
{
    [Test]
    public void Parse_FullObject_ReturnsAllFields()
    {
        var obj = new
        {
            id = 42,
            content = "hello",
            isFromUser = true,
            sentAt = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc)
        };

        ChatHubParser.HubMessage result = ChatHubParser.Parse(obj);

        result.Id.ShouldBe(42);
        result.Content.ShouldBe("hello");
        result.IsFromUser.ShouldBeTrue();
        result.SentAt.ShouldBe(new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc));
    }

    [Test]
    public void Parse_MissingFields_ReturnsDefaults()
    {
        var obj = new { };

        ChatHubParser.HubMessage result = ChatHubParser.Parse(obj);

        result.Id.ShouldBe(0);
        result.Content.ShouldBe("");
        result.IsFromUser.ShouldBeFalse();
    }

    [Test]
    public void Parse_IsFromUserFalse_ReturnsFalse()
    {
        var obj = new { id = 1, content = "admin reply", isFromUser = false };

        ChatHubParser.HubMessage result = ChatHubParser.Parse(obj);

        result.IsFromUser.ShouldBeFalse();
    }

    [Test]
    public void Parse_EmptyContent_ReturnsEmptyString()
    {
        var obj = new { content = "" };

        ChatHubParser.HubMessage result = ChatHubParser.Parse(obj);

        result.Content.ShouldBe("");
    }
}
