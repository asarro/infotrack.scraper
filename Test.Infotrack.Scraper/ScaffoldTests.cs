using AwesomeAssertions;

namespace Test.Infotrack.Scraper;

// Placeholder tests proving the test harness, AwesomeAssertions, and (via the
// project reference) the API project all wire up. No domain behaviour is asserted
// yet — real scenarios arrive with the scraping logic.
public class ScaffoldTests
{
    [Fact]
    public void Harness_Runs()
    {
        true.Should().BeTrue();
    }
}
