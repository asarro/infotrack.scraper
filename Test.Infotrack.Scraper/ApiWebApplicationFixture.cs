using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Test.Infotrack.Scraper;

public sealed class ApiWebApplicationFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
        => builder.UseEnvironment("Development");
}

[CollectionDefinition(nameof(ApiCollection))]
public class ApiCollection : ICollectionFixture<ApiWebApplicationFixture> { }
