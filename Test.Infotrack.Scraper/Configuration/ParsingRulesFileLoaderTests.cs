using AwesomeAssertions;
using Infotrack.Scraper.Configuration;
using Infotrack.Scraper.Utils;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Test.Infotrack.Scraper.Configuration;

public sealed class ParsingRulesFileLoaderTests
{
    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string ApplicationName { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = "";
        public string EnvironmentName { get; set; } = "";
    }

    [Fact]
    public void PostConfigure_WhenFileExists_PopulatesParsingRules()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(Path.Combine(tempDir, "Resources"));
        File.WriteAllText(
            Path.Combine(tempDir, "Resources", "test.rules.json"),
            """{"ContainerSelector":".item","Fields":[{"Field":"Name","Selector":".name"}]}""");

        var env    = new FakeHostEnvironment { ContentRootPath = tempDir };
        var loader = new ParsingRulesFileLoader(env);
        var options = new List<TargetSiteOptions>
        {
            new() { Name = "test", TargetUrl = "http://test.com", ParsingRulesFile = "test.rules.json" }
        };

        loader.PostConfigure(null, options);

        options[0].ParsingRules.Should().NotBeNull();
        var rules = options[0].ParsingRules!;
        rules.ContainerSelector.Should().Be(".item");
        rules.Fields.Should().HaveCount(1);
        rules.Fields[0].Field.Should().Be("Name");
    }

    [Fact]
    public void PostConfigure_WhenFileNotSpecified_LeavesParsingRulesNull()
    {
        var env    = new FakeHostEnvironment { ContentRootPath = Path.GetTempPath() };
        var loader = new ParsingRulesFileLoader(env);
        var options = new List<TargetSiteOptions>
        {
            new() { Name = "test", TargetUrl = "http://test.com" }
        };

        loader.PostConfigure(null, options);

        options[0].ParsingRules.Should().BeNull();
    }
}
