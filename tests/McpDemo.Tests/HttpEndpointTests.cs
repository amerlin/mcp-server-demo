using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using ModelContextProtocol.Client;
using Xunit;

namespace McpDemo.Tests;

public sealed class HttpEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HttpEndpointTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Health_ReturnsOk()
    {
        using var httpClient = _factory.CreateClient();
        using var response = await httpClient.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("ok", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Mcp_DiscoversAndInvokesToolOverHttp()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var httpClient = _factory.CreateClient();
        var transport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
            TransportMode = HttpTransportMode.StreamableHttp
        }, httpClient);
        await using var client = await McpClient.CreateAsync(transport, cancellationToken: timeout.Token);

        var tools = await client.ListToolsAsync(cancellationToken: timeout.Token);
        Assert.Equal(2, tools.Count);
        var result = await client.CallToolAsync("calcola_iva",
            new Dictionary<string, object?> { ["imponibile"] = 200m, ["aliquota"] = 10m },
            cancellationToken: timeout.Token);

        Assert.False(result.IsError is true);
        Assert.NotNull(result.StructuredContent);
        Assert.Equal(220m, result.StructuredContent.Value.GetProperty("totale").GetDecimal());
    }

    [Fact]
    public async Task Http_RejectsUntrustedHosts()
    {
        using var httpClient = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Host = "untrusted.example";
        using var response = await httpClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}