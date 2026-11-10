# Convenzioni del progetto

- Demo MCP in C# / ASP.NET Core su .NET 10, con SDK ufficiale ModelContextProtocol.AspNetCore.
- Documentazione SDK: https://github.com/modelcontextprotocol/csharp-sdk e https://csharp.sdk.modelcontextprotocol.io/.
- Specifica MCP: https://modelcontextprotocol.io/specification/latest.
- Mantenere i commenti didattici e la documentazione in italiano.
- I tool sono in `src/McpDemo/Tools`; validarvi gli input e usare `McpException` solo con messaggi sicuri.
- In modalità stdio non scrivere mai log su stdout: quel canale è riservato al protocollo.
- Non aggiungere credenziali, chiamate a servizi esterni o persistenza alla demo senza necessità.
- Verifica: `dotnet build` e `dotnet test`. Aggiornare README e CHANGELOG per modifiche significative.