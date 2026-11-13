# Testare il server MCP con Visual Studio Code

Questa guida descrive come collegare il server MCP demo a Visual Studio Code e provare i tool `saluta` e `calcola_iva` con GitHub Copilot Chat. La configurazione usa **stdio**: VS Code avvia il server come processo figlio, quindi non serve avviare prima il server HTTP.

## Prerequisiti

- Windows con l'**SDK .NET 10** installato. Il runtime da solo non basta.
- Visual Studio Code aggiornato e GitHub Copilot abilitato per usare Copilot Chat.
- Il repository `mcp-server-demo` aperto come cartella principale in VS Code.
- Connessione a NuGet per il ripristino iniziale dei pacchetti.

Per verificare l'SDK, aprire **Terminale > Nuovo terminale** in VS Code ed eseguire dalla cartella principale:

```powershell
dotnet --list-sdks
```

Deve comparire una versione `10.0.x`. Il file `global.json` seleziona l'SDK stabile .NET 10 disponibile.

## 1. Compilare il server

Nel terminale integrato, dalla cartella principale del repository, eseguire:

```powershell
dotnet restore
dotnet build
```

La configurazione MCP usa `--no-build`: compilare prima del primo avvio e dopo ogni modifica al codice. La compilazione deve terminare senza errori.

## 2. Avviare il server MCP in VS Code

1. Aprire il file `.vscode/mcp.json` nel workspace. Contiene già il server `demo-dotnet10-iva` e lo avvia con `dotnet run` in modalità stdio.
2. Se richiesto, scegliere **Trust / Attendibile** per autorizzare l'avvio del server locale.
3. Selezionare **Start / Avvia** sopra la configurazione `demo-dotnet10-iva`.
   - In alternativa, aprire la Command Palette con `Ctrl+Shift+P`, eseguire **MCP: List Servers**, selezionare `demo-dotnet10-iva` e avviarlo.
4. Verificare che lo stato del server risulti avviato. Se VS Code chiede conferma per l'esecuzione di un tool, autorizzare la chiamata durante le prove.

Non avviare contemporaneamente il server HTTP per questa prova: la configurazione in `.vscode/mcp.json` gestisce direttamente il processo stdio.

## 3. Abilitare i tool nella chat

1. Aprire **Copilot Chat**.
2. Selezionare la modalità **Agent / Agente**.
3. Aprire il selettore degli strumenti della chat e controllare che il server `demo-dotnet10-iva` e i suoi tool siano abilitati.
4. Se i tool non compaiono, controllare che il server sia avviato e aggiornare/riaprire il selettore.

I nomi dei tool disponibili sono `saluta` e `calcola_iva`.

## 4. Provare `saluta`

Inviare in chat una richiesta esplicita, per esempio:

> Usa il tool MCP `saluta` con il nome `Mario`.

Il risultato atteso dal tool è:

```text
Ciao, Mario! Questo saluto arriva da un server MCP in .NET 10.
```

Si può provare anche un nome diverso. Il tool rifiuta nomi vuoti, più lunghi di 80 caratteri o contenenti caratteri di controllo.

## 5. Provare `calcola_iva`

Inviare:

> Usa il tool MCP `calcola_iva` con imponibile 100 e aliquota 22.

Il risultato include i seguenti valori:

```json
{
  "imponibile": 100,
  "aliquota": 22,
  "iva": 22,
  "totale": 122,
  "valuta": "EUR"
}
```

Provare anche:

> Usa il tool MCP `calcola_iva` con imponibile 250 e aliquota 10.

Il risultato atteso è IVA `25` e totale `275`. Se non si specifica l'aliquota, il tool usa il valore predefinito `22`.

Per verificare la validazione, si può chiedere un imponibile negativo: il tool deve restituire un errore controllato invece di eseguire il calcolo.

## 6. Arrestare o riavviare il server

- Per arrestare il processo, usare **Stop / Arresta** nella configurazione MCP o dalla vista MCP di VS Code.
- Dopo aver modificato il codice, eseguire di nuovo `dotnet build`, poi arrestare e riavviare il server per caricare la nuova compilazione.
- Se la chat non vede ancora i tool, controllare nuovamente che siano abilitati nel selettore degli strumenti.

## Problemi comuni

| Problema | Cosa fare |
| --- | --- |
| `dotnet` non trovato o SDK incompatibile | Installare l'SDK .NET 10, chiudere e riaprire VS Code, poi ricontrollare con `dotnet --list-sdks`. |
| Il server non si avvia | Eseguire `dotnet build` nel terminale dalla cartella principale e leggere l'errore mostrato. Verificare anche che il workspace aperto sia la radice del repository. |
| Il server si avvia ma i tool non compaiono | Aprire `.vscode/mcp.json`, controllare lo stato del server e abilitare i tool nel selettore di Copilot Chat. |
| Il tool non viene chiamato | Usare Copilot Chat in modalità Agent e formulare una richiesta esplicita, indicando il nome del tool. |
| Le modifiche non si riflettono | Ricompilare con `dotnet build`, quindi arrestare e riavviare il server MCP. |

Per ulteriori dettagli sul progetto, sulla modalità HTTP e su MCP Inspector, vedere [README.md](README.md).
