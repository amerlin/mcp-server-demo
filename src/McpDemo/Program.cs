using McpDemo.Tools;
using ModelContextProtocol.AspNetCore;

// Un solo trasporto per processo: HTTP per impostazione predefinita,
// oppure stdio quando il client (ad esempio VS Code) avvia il server con --stdio.
var useStdio = args.Contains("--stdio", StringComparer.OrdinalIgnoreCase);
var hostArgs = args.Where(arg => !arg.Equals("--stdio", StringComparison.OrdinalIgnoreCase)).ToArray();

if (useStdio)
{
    var stdioBuilder = Host.CreateApplicationBuilder(hostArgs);

    // stdout è riservato ai messaggi JSON-RPC: tutti i log vanno su stderr.
    stdioBuilder.Logging.ClearProviders();
    stdioBuilder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
    stdioBuilder.Services.AddMcpServer()
        .WithStdioServerTransport()
        .WithTools<DemoTools>();

    await stdioBuilder.Build().RunAsync();
    return;
}

var builder = WebApplication.CreateBuilder(hostArgs);

builder.Services.AddMcpServer()
    // La demo non richiede sessioni o notifiche spontanee verso il client.
    .WithHttpTransport(options => options.SessionMode = HttpServerSessionMode.Stateless)
    .WithTools<DemoTools>();

var app = builder.Build();

// Endpoint MCP Streamable HTTP: lo SDK gestisce il protocollo e il dispatch
// dei messaggi JSON-RPC, inclusi la scoperta dei tool (tools/list) e la loro
// esecuzione (tools/call). Non è un endpoint REST: i parametri del tool
// vengono inviati nel corpo JSON-RPC, non come query string.
// In modalità stateless ogni richiesta è indipendente; aprire /mcp nel browser
// non esegue un tool. Usare un client MCP, Copilot oppure MCP Inspector.
app.MapMcp("/mcp");

// Endpoint REST separato, utile soltanto per verificare che il processo risponda.
// Non verifica l'esecuzione di un tool o lo stato di servizi esterni.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

await app.RunAsync();

// Rende accessibile l'entry point ai test HTTP con WebApplicationFactory.
public partial class Program;