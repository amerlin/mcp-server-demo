using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

namespace McpDemo.Tests;

public sealed class McpIntegrationTests
{
    [Fact]
    public async Task Stdio_DiscoversAndInvokesTools()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "McpDemo.slnx")))
        {
            root = root.Parent;
        }

        Assert.NotNull(root);
        var configuration = new DirectoryInfo(AppContext.BaseDirectory).Parent!.Name;
        var serverDll = Path.Combine(root.FullName, "src", "McpDemo", "bin", configuration, "net10.0", "McpDemo.dll");
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Command = "dotnet",
            Arguments = [serverDll, "--stdio"],
            WorkingDirectory = root.FullName,
            Name = "demo-integration-test"
        });

        await using var client = await McpClient.CreateAsync(transport, cancellationToken: timeout.Token);
        var tools = await client.ListToolsAsync(cancellationToken: timeout.Token);
        Assert.Contains(tools, tool => tool.Name == "saluta");
        Assert.Contains(tools, tool => tool.Name == "calcola_iva");

        var greeting = await client.CallToolAsync("saluta",
            new Dictionary<string, object?> { ["nome"] = "Mario" }, cancellationToken: timeout.Token);
        Assert.False(greeting.IsError is true);
        Assert.Contains("Ciao, Mario!", Assert.Single(greeting.Content.OfType<TextContentBlock>()).Text);

        var vat = await client.CallToolAsync("calcola_iva",
            new Dictionary<string, object?> { ["imponibile"] = 100m }, cancellationToken: timeout.Token);
        Assert.False(vat.IsError is true);
        Assert.NotNull(vat.StructuredContent);
        Assert.Equal(122m, vat.StructuredContent.Value.GetProperty("totale").GetDecimal());

        var invalid = await client.CallToolAsync("calcola_iva",
            new Dictionary<string, object?> { ["imponibile"] = -1m }, cancellationToken: timeout.Token);
        Assert.True(invalid.IsError);
        Assert.Contains("imponibile", Assert.Single(invalid.Content.OfType<TextContentBlock>()).Text);
    }
}