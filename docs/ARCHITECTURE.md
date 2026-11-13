# Scelte architetturali

## 1.0.0 — 2026-11-10

- **ASP.NET Core su .NET 10 e SDK MCP ufficiale**: lo SDK gestisce protocollo, schemi JSON e trasporti; non duplicare queste responsabilità con endpoint REST personalizzati.
- **HTTP stateless e stdio selezionabili**: HTTP dimostra un endpoint reale; stdio semplifica l'uso locale in Copilot. La logica dei tool è unica e non dipende dal trasporto. Nessuna sessione applicativa è necessaria.
- **Tool puri, senza dipendenze esterne**: una demo riproducibile non deve richiedere database, chiavi o servizi a pagamento. L'IVA illustra un risultato strutturato utile a un modello.
- **Importi decimal e validazione esplicita**: evitare errori di rappresentazione binaria e rifiutare importi fuori intervallo o frazioni di centesimo. La sola descrizione JSON Schema non sostituisce la validazione.
- **Loopback e host limitati**: il server non autenticato è solo locale; un deployment reale richiede un progetto di sicurezza aggiuntivo.
- **Test unitari e di protocollo**: verificare sia le regole del calcolo sia discovery, serializzazione, errori e funzionamento dei due trasporti. Il test HTTP usa TestServer; quello stdio avvia un processo .NET reale.