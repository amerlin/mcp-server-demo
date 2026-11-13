# Demo MCP Server con .NET 10

Server **Model Context Protocol (MCP)** realizzato in C# con **ASP.NET Core / .NET 10** e lo SDK ufficiale **ModelContextProtocol.AspNetCore 2.4.0**.

Espone due tool didattici, utilizzabili da GitHub Copilot e da altri client MCP. Non richiede chiavi API, database o servizi esterni.

## 1. Come funziona

MCP permette a un'applicazione AI di scoprire e invocare funzioni messe a disposizione da un server. Il modello non esegue direttamente il codice C#: è il client MCP a inviare la richiesta al server, che valida i parametri, esegue il tool e restituisce il risultato.

Per una guida passo passo alla creazione di un server MCP in C# e .NET, vedere [CREARE-SERVER-MCP.md](docs/CREARE-SERVER-MCP.md).

Il flusso è:

1. Il client stabilisce la comunicazione e negozia le capacità del server tramite lo SDK.
2. Con `tools/list` scopre i tool, le descrizioni e gli schemi degli argomenti.
3. Con `tools/call` richiede l'esecuzione di un tool.
4. Il server restituisce testo oppure dati strutturati. Il client può quindi utilizzarli nella risposta all'utente.

Gli attributi `[McpServerTool]` e `[Description]` sui metodi descrivono i tool e i parametri al client. La registrazione `.WithTools<DemoTools>()` li rende disponibili. Non servono controller REST o un parser JSON-RPC scritto a mano.

### Tool disponibili

| Tool | Parametri | Risultato |
| --- | --- | --- |
| `saluta` | `nome`: stringa da 1 a 80 caratteri, senza caratteri di controllo | Saluto in italiano |
| `calcola_iva` | `imponibile`: numero da 0 a 1.000.000 con massimo 2 decimali; `aliquota`: numero da 0 a 100, predefinito 22 | Imponibile, aliquota, IVA, totale e valuta EUR |

Esempio: `calcola_iva` con `imponibile = 100` e `aliquota = 22` restituisce:

```json
{
  "imponibile": 100,
  "aliquota": 22,
  "iva": 22,
  "totale": 122,
  "valuta": "EUR"
}
```

Il calcolo usa `decimal`; l'IVA è arrotondata a due decimali con `MidpointRounding.AwayFromZero`. È un esempio didattico, non un motore fiscale.

### Due modalità di comunicazione

- **Streamable HTTP**, modalità predefinita: il server ascolta su `http://127.0.0.1:5050/mcp`. È adatto a client che si collegano a un processo già avviato. La demo usa richieste stateless, senza sessioni persistenti.
- **stdio**, con `--stdio`: il client avvia il server come processo figlio e comunica attraverso stdin/stdout. Non viene aperta una porta HTTP. Questa è la modalità già configurata per VS Code.

Si sceglie una sola modalità per processo; i tool sono gli stessi in entrambe.

## 2. Prerequisiti

- **SDK .NET 10**, non soltanto il runtime: [download ufficiale](https://dotnet.microsoft.com/download/dotnet/10.0).
- Visual Studio Code e GitHub Copilot per la prova in chat (facoltativi).
- C# Dev Kit per il debug in VS Code (consigliato anche nelle estensioni del workspace).
- Accesso a NuGet per il primo ripristino delle dipendenze.

Il progetto seleziona l'ultimo SDK stabile .NET 10 installato attraverso [global.json](global.json). Non richiede una specifica patch: è consigliata la patch stabile più recente della serie 10.0.

## 3. Compilazione e test

Aprire un terminale nella cartella principale del repository:

```powershell
dotnet --list-sdks
dotnet restore
dotnet build
dotnet test
```

I test verificano validazione, arrotondamento, scoperta dei tool, esecuzione tramite un vero processo stdio, endpoint HTTP tramite il server di test ASP.NET Core e rifiuto di host non autorizzati.

## 4. Utilizzo da VS Code e GitHub Copilot

La configurazione pronta è in [.vscode/mcp.json](.vscode/mcp.json).

Per una procedura passo passo dedicata alla prova dei tool in VS Code, vedere [TEST-VS-CODE.md](docs/TEST-VS-CODE.md).

1. Eseguire `dotnet build` almeno una volta, in configurazione Debug. La configurazione MCP usa `--no-build` per non mescolare l'output della compilazione con i messaggi del protocollo su stdout.
2. Aprire [.vscode/mcp.json](.vscode/mcp.json) e selezionare **Start / Avvia** sopra il server `demo-dotnet10-iva`. In alternativa usare **MCP: List Servers** dalla palette dei comandi e scegliere il server.
3. Confermare l'attendibilità del server, se richiesta da VS Code.
4. Aprire Copilot Chat in modalità **Agent / Agente** e verificare che i tool del server siano abilitati nel selettore degli strumenti.
5. Provare una richiesta esplicita, ad esempio:
   - «Usa il tool MCP saluta per salutare Mario».
   - «Usa il tool MCP calcola_iva per un imponibile di 100 euro con IVA al 22%».
   - «Usa calcola_iva su 250 euro con aliquota 10% e spiegami il risultato».
6. Autorizzare l'invocazione del tool se il client lo richiede.

Non occorre avviare separatamente il server HTTP: in questa configurazione VS Code gestisce il processo stdio. Dopo modifiche al codice, ricompilare e riavviare il server dal menu MCP. Per arrestarlo usare **Stop / Arresta** nello stesso menu.

### Collegare Copilot alla modalità HTTP, in alternativa

Prima avviare il server come descritto nella prossima sezione, poi sostituire la configurazione del server con:

```json
{
  "servers": {
    "demo-dotnet10-iva-http": {
      "type": "http",
      "url": "http://127.0.0.1:5050/mcp"
    }
  }
}
```

Usare una sola delle due configurazioni per evitare di vedere due copie degli stessi tool. In HTTP VS Code non avvia né arresta il processo ASP.NET Core.

## 5. Avviare il server HTTP

Dalla cartella principale:

```powershell
dotnet run --project src/McpDemo --no-launch-profile
```

Indirizzi disponibili:

| Indirizzo | Scopo |
| --- | --- |
| `http://127.0.0.1:5050/mcp` | Endpoint MCP Streamable HTTP, commentato nel codice |
| `http://127.0.0.1:5050/health` | Endpoint REST di verifica: risponde con `{"status":"ok"}` |

Per fermare il processo premere **Ctrl+C** nel terminale. Per scegliere una porta diversa:

```powershell
dotnet run --project src/McpDemo --no-launch-profile -- --urls http://127.0.0.1:5051
```

Aggiornare di conseguenza l'URL del client. Aprire `/mcp` in un browser **non** esegue un tool: non è una pagina web né un endpoint REST tradizionale. Non sono presenti Swagger o rotte come `/saluta`.

### Debug in VS Code

Selezionare **Debug MCP (HTTP)** in Esegui e debug e premere **F5**. La configurazione compila il progetto e avvia ASP.NET Core; si possono impostare breakpoint in [Program.cs](src/McpDemo/Program.cs) e [DemoTools.cs](src/McpDemo/Tools/DemoTools.cs), quindi collegarsi con un client HTTP. Non avviare contemporaneamente due server sulla porta 5050.

## 6. Provare con MCP Inspector

[MCP Inspector](https://github.com/modelcontextprotocol/inspector) è un'interfaccia dedicata alla scoperta e all'esecuzione manuale dei tool. Richiede anche una versione Node.js LTS compatibile con Inspector; Node.js non serve al server .NET.

1. Avviare il server HTTP come sopra.
2. In un secondo terminale avviare Inspector:

   ```powershell
   npx @modelcontextprotocol/inspector@latest
   ```

3. Aprire l'URL locale mostrato da Inspector, mantenendo il token di sessione generato. Non disabilitare l'autenticazione del proxy Inspector.
4. Scegliere **Streamable HTTP** e inserire `http://127.0.0.1:5050/mcp` come URL del server.
5. Collegarsi, aprire **Tools**, richiedere la lista ed eseguire `saluta` o `calcola_iva`.

Esempio di argomenti per `calcola_iva`:

```json
{ "imponibile": 100, "aliquota": 22 }
```

Un imponibile negativo produce un risultato MCP con `isError: true` e un messaggio di validazione controllato.

### Cosa invia un client MCP

Esempio del corpo JSON-RPC di una chiamata al tool, dopo aver stabilito la comunicazione e negoziato il protocollo:

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "tools/call",
  "params": {
    "name": "calcola_iva",
    "arguments": { "imponibile": 100, "aliquota": 22 }
  }
}
```

Questo è il messaggio del protocollo, non una richiesta HTTP completa: header, negoziazione della versione e gestione delle risposte vanno delegati a un client MCP. Il test [HttpEndpointTests.cs](tests/McpDemo.Tests/HttpEndpointTests.cs) mostra anche come collegarsi usando lo SDK C# ufficiale.

## 7. Struttura e punti da modificare

- [src/McpDemo/Program.cs](src/McpDemo/Program.cs): configurazione dei trasporti, registrazione dei tool ed endpoint commentati.
- [src/McpDemo/Tools/DemoTools.cs](src/McpDemo/Tools/DemoTools.cs): implementazione dei tool e risposta tipizzata del calcolo IVA.
- [src/McpDemo/appsettings.json](src/McpDemo/appsettings.json): indirizzo locale, host accettati e log.
- [.vscode/mcp.json](.vscode/mcp.json): collegamento stdio per Copilot.
- [.vscode/launch.json](.vscode/launch.json): debug HTTP con F5.
- [tests/McpDemo.Tests](tests/McpDemo.Tests): test unitari e di integrazione.
- [flow.md](docs/flow.md): diagramma e descrizione del flusso di funzionamento.
- [ARCHITECTURE.md](docs/ARCHITECTURE.md): motivazioni delle scelte principali.
- [CHANGELOG.md](docs/CHANGELOG.md): cronologia delle modifiche.
- [Scarica la presentazione PowerPoint](CREARE-SERVER-MCP.pptx).

Per aggiungere un tool: creare un metodo in `DemoTools`, applicare `[McpServerTool]` e `[Description]`, descrivere i parametri, validare gli input, aggiungere test e ricompilare. Un metodo in una nuova classe va anche registrato con `.WithTools<NuovaClasse>()` in entrambi i rami di avvio.

## 8. Sicurezza e limiti

- Demo locale **senza autenticazione**: ascolta soltanto sul loopback; gli host ammessi sono limitati. Non pubblicarla direttamente su Internet o sulla rete aziendale.
- Non sono configurati CORS permissivi. Per browser su un'altra origine progettare una allowlist esplicita e la validazione dell'origine, senza disabilitare le protezioni.
- Prima di una pubblicazione reale: HTTPS, autenticazione/autorizzazione MCP appropriata, controllo dell'origine, rate limiting, osservabilità e configurazione del reverse proxy.
- Non vengono salvati dati e i tool non hanno effetti collaterali. Le annotazioni `ReadOnly` e `Idempotent` sono indicazioni al client, non meccanismi di autorizzazione.
- I log stdio vengono scritti su stderr, mai su stdout. Non aggiungere `Console.WriteLine` al server.
- Non inserire password o token nel codice o nella configurazione MCP versionata; usare variabili d'ambiente o un secret store quando necessario.

## 9. Problemi comuni

| Problema | Soluzione |
| --- | --- |
| SDK .NET non trovato | Installare l'SDK .NET 10 e riaprire terminale e VS Code |
| Copilot non vede i tool | Verificare che il server sia avviato, che i tool siano abilitati e che la chat supporti MCP |
| Avvio stdio fallito dopo il clone | Eseguire `dotnet build` in Debug, poi riavviare il server MCP |
| Modifiche non visibili in Copilot | Ricompilare e riavviare il server per ricaricare codice e definizioni |
| Porta 5050 occupata | Arrestare l'altro processo oppure specificare un'altra porta e aggiornare il client |
| `/mcp` dà errore nel browser | Usare un client MCP; usare `/health` per una semplice verifica HTTP |
| Host HTTP rifiutato | La demo accetta solo nomi loopback; non sostituire `AllowedHosts` con `*` |

## Riferimenti

- [SDK C# ufficiale MCP](https://github.com/modelcontextprotocol/csharp-sdk)
- [Documentazione SDK C#](https://csharp.sdk.modelcontextprotocol.io/)
- [Specifica MCP](https://modelcontextprotocol.io/specification/latest)
- [MCP in VS Code](https://code.visualstudio.com/docs/copilot/customization/mcp-servers)