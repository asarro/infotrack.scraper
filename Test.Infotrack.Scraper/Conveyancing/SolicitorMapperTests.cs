using AwesomeAssertions;
using Infotrack.Scraper.Conveyancing;
using Infotrack.Scraper.Scraping;

namespace Test.Infotrack.Scraper.Conveyancing;

public sealed class SolicitorMapperTests
{
    [Fact]
    public void Map_AllFieldsPresent_ReturnsSolicitorWithAllFields()
    {
        var record = new ExtractedRecord(new Dictionary<string, string>
        {
            ["Name"]        = "Smith & Co",
            ["Address"]     = "1 High St",
            ["Phone"]       = "01234 567890",
            ["Description"] = "Expert conveyancing services",
            ["Website"]     = "https://smithandco.example.com"
        });

        var solicitor = SolicitorMapper.Map(record);

        solicitor.Name.Should().Be("Smith & Co");
        solicitor.Address.Should().Be("1 High St");
        solicitor.Phone.Should().Be("01234 567890");
        solicitor.Description.Should().Be("Expert conveyancing services");
        solicitor.Website.Should().Be("https://smithandco.example.com");
    }

    [Fact]
    public void Map_NameMissing_ReturnsSolicitorWithEmptyName()
    {
        var record = new ExtractedRecord(new Dictionary<string, string>
        {
            ["Address"] = "1 High St",
            ["Phone"]   = "01234 567890"
        });

        var solicitor = SolicitorMapper.Map(record);

        solicitor.Name.Should().Be(string.Empty);
    }

    [Fact]
    public void Map_OptionalFieldsMissing_ReturnsSolicitorWithNullOptionalFields()
    {
        var record = new ExtractedRecord(new Dictionary<string, string>
        {
            ["Name"] = "Smith & Co"
        });

        var solicitor = SolicitorMapper.Map(record);

        solicitor.Address.Should().BeNull();
        solicitor.Phone.Should().BeNull();
        solicitor.Description.Should().BeNull();
        solicitor.Website.Should().BeNull();
    }
}
