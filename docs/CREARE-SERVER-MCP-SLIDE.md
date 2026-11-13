---
marp: true
theme: default
paginate: true
style: |
  section {
    font-family: Aptos, "Segoe UI", sans-serif;
    color: #17243b;
    background: #f5f8fc;
    padding: 48px 58px;
    font-size: 24px;
  }
  h1, h2 {
    color: #1457a6;
  }
  h1 {
    font-size: 2em;
  }
  h2 {
    font-size: 1.55em;
  }
  strong {
    color: #103f78;
  }
  code {
    color: #0c477f;
    background: #e7eef8;
  }
  pre {
    font-size: 0.72em;
    line-height: 1.25;
    background: #eaf0f8;
    border-left: 5px solid #3478c8;
    padding: 14px 18px;
  }
  section.lead {
    background: linear-gradient(135deg, #102d52, #1766ad);
    color: #ffffff;
    text-align: center;
  }
  section.lead h1, section.lead strong {
    color: #ffffff;
  }
---

<!-- _class: lead -->

# Creare un server MCP

## C# · .NET 10 · SDK MCP ufficiale

Dalla funzione C# al tool che un client AI può scoprire e invocare

---

## MCP: chi fa cosa?

Il server **non fa eseguire codice al modello**: il client MCP orchestra la chiamata e il server esegue il tool.

**Utente** → **Client MCP + modello** → **Server MCP** → **Tool C#**

1. Il client chiede quali tool sono disponibili (`tools/list`).
2. Sceglie un tool e invia gli argomenti (`tools/call`).
3. Il server valida, esegue e restituisce il risultato.
4. Il client usa il risultato nella conversazione.

---

## I componenti essenziali

| Componente | Responsabilità |
| --- | --- |
| SDK MCP | Protocollo, schemi, dispatch e serializzazione |
| Trasporto | Collega client e server: `stdio` oppure HTTP |
| Tool | Funzione C# con nome, descrizione, input e output |
| Client MCP | Scopre i tool e ne richiede l'esecuzione |

**Regola di progettazione:** separa la logica dei tool dal trasporto, così gli stessi tool possono essere riusati.

---

## 1 · Aggiungi un tool

Lo SDK ricava lo schema dagli attributi e dalla firma C#.

```csharp
[McpServerToolType]
public sealed class GreetingTools
{
    [McpServerTool(Name = "saluta", ReadOnly = true, Idempotent = true)]
    [Description("Restituisce un saluto in italiano.")]
    public static string Saluta(
        [Description("Nome da salutare.")] string nome)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length > 80)
            throw new McpException("Il nome deve contenere da 1 a 80 caratteri.");

        return $"Ciao, {nome.Trim()}!";
    }
}
```

---

## 2 · Input affidabili, output chiari

- **Descrivi** tool e parametri: aiuti il client a scegliere correttamente.
- **Valida nel server**: lo schema non sostituisce i controlli in C#.
- **Segnala gli errori attesi** con `McpException` e messaggi sicuri.
- **Scegli un output utile**: testo semplice o tipo C# serializzabile.
- Per dati composti, usa un risultato tipizzato; la demo espone `VatResult` come contenuto strutturato.
- Aggiungi test per input validi, limiti ed errori.

---

## 3 · Scegli il trasporto

### `stdio`

- Il client avvia il server come processo figlio.
- Comunicazione tramite input/output standard.
- **stdout è riservato al protocollo**; i log vanno su stderr.
- Ideale per integrazione locale, ad esempio in VS Code.

### Streamable HTTP

- Il server ASP.NET Core resta in ascolto.
- Il client MCP si collega all'endpoint, per esempio `/mcp`.
- Non è una normale rotta REST per invocare tool dal browser.
- Adatto a un processo server avviato separatamente.

**Un processo, un trasporto; la logica dei tool resta la stessa.**

---

## 4 · Registra i tool e avvia

Esempio HTTP con ASP.NET Core:

```csharp
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<GreetingTools>();

var app = builder.Build();
app.MapMcp("/mcp");
await app.RunAsync();
```

Per `stdio`, sostituisci `.WithHttpTransport()` con `.WithStdioServerTransport()` e crea l'host con `Host.CreateApplicationBuilder`.

---

## 5 · Collega, prova, proteggi

### Verifica

```powershell
dotnet restore
dotnet build
dotnet test
```

Prova discovery (`tools/list`), chiamata (`tools/call`), validazione degli input e trasporto scelto.

### Prima di esporlo in rete

- Configura autenticazione, autorizzazione e HTTPS.
- Limita origini e richieste; registra gli eventi in modo sicuro.
- Non esporre una demo senza autenticazione su Internet.
- Le annotazioni dei tool non sono controlli di sicurezza.

**Approfondimenti:** `CREARE-SERVER-MCP.md` · `../README.md` · `ARCHITECTURE.md`
