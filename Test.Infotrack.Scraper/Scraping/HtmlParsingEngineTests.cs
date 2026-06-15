using AwesomeAssertions;
using Infotrack.Scraper.Configuration;
using Infotrack.Scraper.Scraping;

namespace Test.Infotrack.Scraper.Scraping;

public sealed class HtmlParsingEngineTests
{
    private static readonly ParsingRules DefaultRules = new()
    {
        ContainerSelector = ".listing",
        Fields =
        [
            new FieldRule { Field = "Name",    Selector = ".listing-name" },
            new FieldRule { Field = "Address", Selector = ".listing-address" },
            new FieldRule { Field = "Phone",   Selector = ".listing-phone" }
        ]
    };

    private const string SingleListingHtml = """
        <html><body>
          <div class="listing">
            <span class="listing-name">Smith &amp; Co Solicitors</span>
            <span class="listing-address">10 High Street, London</span>
            <span class="listing-phone">020 7946 0958</span>
          </div>
        </body></html>
        """;

    [Fact]
    public void Parse_SingleContainer_ExtractsAllFields()
    {
        var engine  = new HtmlParsingEngine();
        var records = engine.Parse(SingleListingHtml, DefaultRules);

        records.Should().HaveCount(1);

        var fields = records[0].Fields;
        fields["Name"].Should().Be("Smith & Co Solicitors");
        fields["Address"].Should().Be("10 High Street, London");
        fields["Phone"].Should().Be("020 7946 0958");
    }

    [Fact]
    public void Parse_NoContainerMatch_ReturnsEmptyList()
    {
        const string html = "<html><body><p>No listings here.</p></body></html>";
        var engine  = new HtmlParsingEngine();
        var records = engine.Parse(html, DefaultRules);

        records.Should().BeEmpty();
    }

    [Fact]
    public void Parse_MissingField_DefaultsToEmptyString()
    {
        const string html = """
            <html><body>
              <div class="listing">
                <span class="listing-name">Lone Wolf Legal</span>
              </div>
            </body></html>
            """;
        var engine  = new HtmlParsingEngine();
        var records = engine.Parse(html, DefaultRules);

        records[0].Fields["Address"].Should().Be(string.Empty);
        records[0].Fields["Phone"].Should().Be(string.Empty);
    }

    [Fact]
    public void Parse_TagNameSelector_ExtractsDescriptionParagraph()
    {
        const string html = """
            <html><body>
              <div class="result-item">
                <p>Specialists in residential conveyancing across London.</p>
              </div>
            </body></html>
            """;
        var rules = new ParsingRules
        {
            ContainerSelector = ".result-item",
            Fields = [ new FieldRule { Field = "Description", Selector = "p" } ]
        };

        var records = new HtmlParsingEngine().Parse(html, rules);

        records[0].Fields["Description"].Should().Be("Specialists in residential conveyancing across London.");
    }

    [Fact]
    public void Parse_StopAt_TruncatesBeforeNoiseChild()
    {
        const string html = """
            <html><body>
              <div class="result-item">
                <span class="h2">Amphlett Lissimore<div class="greentick" title="quality marks"></div><span class="rev-results">(665)</span></span>
              </div>
            </body></html>
            """;
        var rules = new ParsingRules
        {
            ContainerSelector = ".result-item",
            Fields = [ new FieldRule { Field = "Name", Selector = ".h2", StopAt = ".greentick" } ]
        };

        var records = new HtmlParsingEngine().Parse(html, rules);

        records[0].Fields["Name"].Should().Be("Amphlett Lissimore");
    }

    [Fact]
    public void Parse_ChildSelector_DrillsIntoNestedElement()
    {
        const string html = """
            <html><body>
              <div class="result-item">
                <div class="phone-block mobile-hidden">
                  <span>Phone:</span>
                  <a rel="noindex" href="tel:02077012219">020 7701 2219</a>
                </div>
              </div>
            </body></html>
            """;
        var rules = new ParsingRules
        {
            ContainerSelector = ".result-item",
            Fields = [ new FieldRule { Field = "Phone", Selector = ".phone-block", ChildSelector = "a" } ]
        };

        var records = new HtmlParsingEngine().Parse(html, rules);

        records[0].Fields["Phone"].Should().Be("020 7701 2219");
    }

    [Fact]
    public void Parse_ChildSelectorOnAddress_ExtractsAddressTag()
    {
        const string html = """
            <html><body>
              <div class="result-item">
                <a href="/amphlett-lissimore.html" class="link-map">
                  <i class="fa fa-map-marker"></i>
                  <address>52 Grosvenor Gardens, Victoria, London SW1W 0AU</address>
                </a>
              </div>
            </body></html>
            """;
        var rules = new ParsingRules
        {
            ContainerSelector = ".result-item",
            Fields = [ new FieldRule { Field = "Address", Selector = ".link-map", ChildSelector = "address" } ]
        };

        var records = new HtmlParsingEngine().Parse(html, rules);

        records[0].Fields["Address"].Should().Be("52 Grosvenor Gardens, Victoria, London SW1W 0AU");
    }

    [Fact]
    public void Parse_AttributeWithAttributeFilter_ExtractsWebsiteHref()
    {
        const string html = """
            <html><body>
              <div class="result-item">
                <ul class="list-item">
                  <li class="red-color"><a rel="noindex nofollow" href="/enquiry-form.asp"><i class="fa fa-envelope"></i>Email</a></li>
                  <li><a target="_blank" href="https://www.qualitysolicitors.com/solicitors-whitechapel" rel="nofollow"><i class="fa fa-globe"></i>Website</a></li>
                </ul>
              </div>
            </body></html>
            """;
        var rules = new ParsingRules
        {
            ContainerSelector = ".result-item",
            Fields =
            [
                new FieldRule
                {
                    Field           = "Website",
                    Selector        = "a",
                    Attribute       = "href",
                    AttributeFilter = new AttributeFilter { Name = "target", Value = "_blank" }
                }
            ]
        };

        var records = new HtmlParsingEngine().Parse(html, rules);

        records[0].Fields["Website"].Should().Be("https://www.qualitysolicitors.com/solicitors-whitechapel");
    }

    [Fact]
    public void Parse_FullSolicitorComResultItem_ExtractsAllFiveFields()
    {
        const string html = """
            <html><body>
              <div class="result-item">
                <span class="h2">Test Solicitors Ltd<div class="greentick" title="quality marks"></div></span>
                <div class="phone-block mobile-hidden">
                  <span>Phone:</span>
                  <a rel="noindex" href="tel:02000000000">020 0000 0000</a>
                </div>
                <a href="/test-solicitors.html" class="link-map">
                  <address>1 Test Street, London</address>
                </a>
                <p>Expert conveyancing solicitors serving London.</p>
                <ul class="list-item">
                  <li><a target="_blank" href="https://www.test-solicitors.co.uk" rel="nofollow"><i class="fa fa-globe"></i>Website</a></li>
                </ul>
              </div>
            </body></html>
            """;

        var rules = new ParsingRules
        {
            ContainerSelector = ".result-item",
            Fields =
            [
                new FieldRule { Field = "Name",        Selector = ".h2",         StopAt = ".greentick" },
                new FieldRule { Field = "Phone",       Selector = ".phone-block", ChildSelector = "a" },
                new FieldRule { Field = "Address",     Selector = ".link-map",    ChildSelector = "address" },
                new FieldRule { Field = "Description", Selector = "p" },
                new FieldRule { Field = "Website",     Selector = "a", Attribute = "href",
                    AttributeFilter = new AttributeFilter { Name = "target", Value = "_blank" } }
            ]
        };

        var records = new HtmlParsingEngine().Parse(html, rules);

        records.Should().HaveCount(1);
        records[0].Fields["Name"].Should().Be("Test Solicitors Ltd");
        records[0].Fields["Phone"].Should().Be("020 0000 0000");
        records[0].Fields["Address"].Should().Be("1 Test Street, London");
        records[0].Fields["Description"].Should().Be("Expert conveyancing solicitors serving London.");
        records[0].Fields["Website"].Should().Be("https://www.test-solicitors.co.uk");
    }

    [Fact]
    public void Parse_AttributeFilterNoMatch_FieldIsEmptyString()
    {
        const string html = """
            <html><body>
              <div class="result-item">
                <ul class="list-item">
                  <li class="red-color"><a rel="noindex nofollow" href="/enquiry-form.asp">Email</a></li>
                </ul>
              </div>
            </body></html>
            """;
        var rules = new ParsingRules
        {
            ContainerSelector = ".result-item",
            Fields =
            [
                new FieldRule
                {
                    Field           = "Website",
                    Selector        = "a",
                    Attribute       = "href",
                    AttributeFilter = new AttributeFilter { Name = "target", Value = "_blank" }
                }
            ]
        };

        var records = new HtmlParsingEngine().Parse(html, rules);

        records[0].Fields["Website"].Should().Be(string.Empty);
    }
}
