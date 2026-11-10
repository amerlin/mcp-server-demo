using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace McpDemo.Tools;

/// <summary>Tool dimostrativi, senza database, chiamate esterne o effetti collaterali.</summary>
[McpServerToolType]
public sealed class DemoTools
{
    /// <summary>
    /// Esempio minimo: lo SDK ricava lo schema degli argomenti dalla firma C#.
    /// Le descrizioni sono visibili al client e aiutano il modello a scegliere il tool.
    /// </summary>
    [McpServerTool(Name = "saluta", ReadOnly = true, Idempotent = true)]
    [Description("Restituisce un saluto in italiano per il nome indicato.")]
    public static string Saluta(
        [Description("Nome da salutare, da 1 a 80 caratteri, senza caratteri di controllo.")] string nome)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length > 80 || nome.Any(char.IsControl))
        {
            // McpException espone un messaggio controllato al client senza stack trace.
            throw new McpException("Il nome deve contenere da 1 a 80 caratteri e non deve contenere caratteri di controllo.");
        }

        return $"Ciao, {nome.Trim()}! Questo saluto arriva da un server MCP in .NET 10.";
    }

    /// <summary>
    /// Calcola un preventivo semplice usando decimal per evitare l'approssimazione
    /// binaria dei double negli importi. L'IVA viene arrotondata a due decimali
    /// con la regola AwayFromZero; il totale è la somma di imponibile e IVA arrotondati.
    /// </summary>
    [McpServerTool(Name = "calcola_iva", ReadOnly = true, Idempotent = true, UseStructuredContent = true)]
    [Description("Calcola IVA e totale in EUR a partire da un imponibile e da un'aliquota percentuale. Esempio didattico, non consulenza fiscale.")]
    public static VatResult CalcolaIva(
        [Description("Importo imponibile in EUR, da 0 a 1000000, con al massimo due decimali.")] decimal imponibile,
        [Description("Aliquota IVA percentuale, da 0 a 100. Il valore predefinito è 22.")] decimal aliquota = 22m)
    {
        if (imponibile < 0 || imponibile > 1_000_000m || decimal.Round(imponibile, 2) != imponibile)
        {
            throw new McpException("L'imponibile deve essere compreso tra 0 e 1000000 EUR e avere al massimo due decimali.");
        }

        if (aliquota < 0 || aliquota > 100)
        {
            throw new McpException("L'aliquota deve essere compresa tra 0 e 100.");
        }

        var iva = decimal.Round(imponibile * aliquota / 100m, 2, MidpointRounding.AwayFromZero);
        return new VatResult(imponibile, aliquota, iva, imponibile + iva, "EUR");
    }
}

/// <summary>Risposta tipizzata, esposta anche come contenuto strutturato MCP.</summary>
public sealed record VatResult(decimal Imponibile, decimal Aliquota, decimal Iva, decimal Totale, string Valuta);