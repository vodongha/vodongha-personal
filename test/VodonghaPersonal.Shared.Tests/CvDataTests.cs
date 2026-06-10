using NUnit.Framework;
using Shouldly;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Shared.Tests;

[TestFixture]
public class CvDataTests
{
    private CvData _cv = null!;

    [SetUp]
    public void SetUp()
    {
        _cv = new CvData(
            Name: "Vo Dong Ha",
            Title: "Software Engineer",
            Email: "ha@example.com",
            Phone: "+84 123 456 789",
            Location: "Ho Chi Minh City",
            GitHub: "https://github.com/vodongha",
            LinkedIn: "https://linkedin.com/in/vodongha",
            Bio: "Building modern web apps.",
            AvatarUrl: "https://example.com/avatar.jpg",
            Skills: [],
            Experiences: [],
            Educations: [],
            Projects: []
        );
    }

    [Test]
    public void Constructor_SetsAllProperties()
    {
        _cv.Name.ShouldBe("Vo Dong Ha");
        _cv.Title.ShouldBe("Software Engineer");
        _cv.Email.ShouldBe("ha@example.com");
        _cv.Phone.ShouldBe("+84 123 456 789");
        _cv.Location.ShouldBe("Ho Chi Minh City");
        _cv.GitHub.ShouldBe("https://github.com/vodongha");
        _cv.LinkedIn.ShouldBe("https://linkedin.com/in/vodongha");
        _cv.Bio.ShouldBe("Building modern web apps.");
        _cv.AvatarUrl.ShouldBe("https://example.com/avatar.jpg");
    }

    [Test]
    public void Constructor_EmptyLists_AreEmpty()
    {
        _cv.Skills.ShouldBeEmpty();
        _cv.Experiences.ShouldBeEmpty();
        _cv.Educations.ShouldBeEmpty();
        _cv.Projects.ShouldBeEmpty();
    }

    [Test]
    public void WithExpression_UpdatesName()
    {
        CvData updated = _cv with { Name = "New Name" };

        updated.Name.ShouldBe("New Name");
        updated.Email.ShouldBe(_cv.Email);
    }

    [Test]
    public void EqualityByValue_SameData_AreEqual()
    {
        CvData copy = _cv with { };

        copy.ShouldBe(_cv);
    }

    [Test]
    public void EqualityByValue_DifferentName_AreNotEqual()
    {
        CvData other = _cv with { Name = "Other" };

        other.ShouldNotBe(_cv);
    }
}
