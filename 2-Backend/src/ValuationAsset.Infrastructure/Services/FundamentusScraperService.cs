using System;
using System.Collections.Generic;
using System.Text;

namespace ValuationAsset.Infrastructure.Services
{
    public class FundamentusScraperService : IMarketScraperService
    {
        private readonly HttpClient _httpClient;

        public FundamentusScraperService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // O Fundamentus usa um user-agent específico, caso contrário pode bloquear o request
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }

        public async Task<ScrapedMarketData?> ScrapeAssetDataAsync(string ticker, CancellationToken cancellationToken)
        {
            string url = $"https://www.fundamentus.com.br/detalhes.php?papel={ticker}";

            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode) return null;

                // O Fundamentus usa enconding ISO-8859-1, precisamos ler os bytes primeiro
                var htmlBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                var htmlString = System.Text.Encoding.GetEncoding("ISO-8859-1").GetString(htmlBytes);

                var doc = new HtmlDocument();
                doc.LoadHtml(htmlString);

                // Verifica se a página retornou um erro (ex: papel não encontrado)
                if (htmlString.Contains("Nenhum papel encontrado")) return null;

                var today = DateTime.UtcNow.Date;

                // 1. Preenchendo a Empresa
                var company = new CompanyAsset
                {
                    StockTicker = ticker,
                    // Nota: Em um cenário real, você usaria XPath (doc.DocumentNode.SelectSingleNode) 
                    // para pegar o valor exato de cada célula da tabela HTML do Fundamentus.
                    // Exemplo simulado da extração:
                    CompanyName = ExtractValue(doc, "Empresa") ?? "N/A",
                    AssetType = "ON NM",
                    MarketSector = "Comércio e Distribuição",
                    IndustryGroup = "Alimentos"
                };

                // 2. Preenchendo a Cotação
                var quote = new MarketQuote
                {
                    ReferenceDate = today,
                    ClosingPrice = ParseDecimal(ExtractValue(doc, "Cotação")),
                    AverageVolume = ParseDecimal(ExtractValue(doc, "Vol $ méd (2m)")),
                    MarketValue = ParseDecimal(ExtractValue(doc, "Valor de mercado")),
                    FirmValue = ParseDecimal(ExtractValue(doc, "Valor da firma"))
                };

                // 3. Preenchendo os Indicadores
                var indicator = new MarketIndicator
                {
                    ReferenceDate = today,
                    PriceEarnings = ParseDecimal(ExtractValue(doc, "P/L")),
                    PriceBook = ParseDecimal(ExtractValue(doc, "P/VP")),
                    DividendYield = ParseDecimal(ExtractValue(doc, "Div. Yield")) / 100, // Transforma 1.3% em 0.0130
                    EarningsShare = ParseDecimal(ExtractValue(doc, "LPA")),
                    BookShare = ParseDecimal(ExtractValue(doc, "VPA")),
                    NetMargin = ParseDecimal(ExtractValue(doc, "Marg. Líquida")) / 100
                };

                // 4. Preenchendo o Balanço
                var statement = new FinancialStatement
                {
                    StatementDate = ParseDate(ExtractValue(doc, "Últ balanço processado")) ?? today,
                    TotalAssets = ParseDecimal(ExtractValue(doc, "Ativo")),
                    LiquidAssets = ParseDecimal(ExtractValue(doc, "Disponibilidades")),
                    CurrentAssets = ParseDecimal(ExtractValue(doc, "Ativo Circulante")),
                    TotalEquity = ParseDecimal(ExtractValue(doc, "Patrim. Líq")),
                    GrossDebt = ParseDecimal(ExtractValue(doc, "Dív. Bruta")),
                    NetDebt = ParseDecimal(ExtractValue(doc, "Dív. Líquida")),
                    SharesCount = (long)ParseDecimal(ExtractValue(doc, "Nro. Ações"))
                };

                return new ScrapedMarketData(company, statement, quote, indicator);
            }
            catch (Exception)
            {
                // Em produção, adicione um ILogger aqui
                return null;
            }
        }

        // Métodos auxiliares para lidar com a formatação brasileira (R$) do site
        private string? ExtractValue(HtmlDocument doc, string label)
        {
            // Lógica de XPath omitida por brevidade. 
            // Na prática: busca o <span> que contém a label e pega o <td> seguinte.
            return "10,50"; // Retorno mockado para não quebrar a compilação
        }

        private decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "-") return 0;
            value = value.Replace(".", "").Replace(",", ".").Replace("%", ""); // Remove milhar e ajusta decimal
            decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result);
            return result;
        }

        private DateTime? ParseDate(string? value)
        {
            if (DateTime.TryParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                return date;
            return null;
        }
    }
}
