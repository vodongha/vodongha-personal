using NUnit.Framework;
using Shouldly;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Shared.Tests;

[TestFixture]
public class BlogPostDefaultsTests
{
    [Test]
    public void Default_StringProperties_AreEmpty()
    {
        BlogPost post = new();

        post.Title.ShouldBe(string.Empty);
        post.Slug.ShouldBe(string.Empty);
        post.Summary.ShouldBe(string.Empty);
        post.Content.ShouldBe(string.Empty);
        post.Tags.ShouldBe(string.Empty);
    }

    [Test]
    public void Default_NullableProperties_AreNull()
    {
        BlogPost post = new();

        post.TitleEn.ShouldBeNull();
        post.SummaryEn.ShouldBeNull();
        post.ContentEn.ShouldBeNull();
        post.CoverImageUrl.ShouldBeNull();
        post.UpdatedAt.ShouldBeNull();
    }

    [Test]
    public void Default_IsPublished_IsFalse()
    {
        BlogPost post = new();

        post.IsPublished.ShouldBeFalse();
    }

    [Test]
    public void Default_ViewCount_IsZero()
    {
        BlogPost post = new();

        post.ViewCount.ShouldBe(0);
    }
}

[TestFixture]
public class ContactMessageDefaultsTests
{
    [Test]
    public void Default_StringProperties_AreEmpty()
    {
        ContactMessage msg = new();

        msg.Name.ShouldBe(string.Empty);
        msg.Email.ShouldBe(string.Empty);
        msg.Subject.ShouldBe(string.Empty);
        msg.Message.ShouldBe(string.Empty);
    }

    [Test]
    public void Default_IsRead_IsFalse()
    {
        ContactMessage msg = new();

        msg.IsRead.ShouldBeFalse();
    }

    [Test]
    public void SetProperties_ValuesAreStored()
    {
        ContactMessage msg = new()
        {
            Name = "Alice",
            Email = "alice@example.com",
            Subject = "Hello",
            Message = "Test body"
        };

        msg.Name.ShouldBe("Alice");
        msg.Email.ShouldBe("alice@example.com");
        msg.Subject.ShouldBe("Hello");
        msg.Message.ShouldBe("Test body");
    }
}
