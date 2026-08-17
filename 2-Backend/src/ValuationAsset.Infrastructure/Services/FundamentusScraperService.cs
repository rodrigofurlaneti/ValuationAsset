using HtmlAgilityPack;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ValuationAsset.Application.Interfaces;
using ValuationAsset.Domain.Entities;

namespace ValuationAsset.Infrastructure.Services;

public class FundamentusScraperService : IMarketScraperService
{
    private readonly HttpClient _httpClient;

    public FundamentusScraperService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
    }
    public async Task<List<string>> GetAllActiveTickersAsync(CancellationToken cancellationToken)
    {
        var tickers = new List<string>();
        string url = "https://www.fundamentus.com.br/resultado.php";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode) return tickers;

            var htmlBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var htmlString = System.Text.Encoding.GetEncoding("ISO-8859-1").GetString(htmlBytes);

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlString);

            // O Fundamentus lista os papéis dentro de uma tabela HTML onde a primeira coluna (link) contém o ticker
            // Exemplo de xpath para pegar os links dos papéis na tabela de resultado
            var nodes = doc.DocumentNode.SelectNodes("//table[@id='resultado']//tbody//tr//td[1]//a");

            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var ticker = node.InnerText?.Trim();
                    if (!string.IsNullOrEmpty(ticker))
                    {
                        tickers.Add(ticker);
                    }
                }
            }
        }
        catch (Exception)
        {
            // Retorna a lista vazia em caso de falha de rede
        }

        return tickers;
    }
    public async Task<ScrapedMarketData?> ScrapeAssetDataAsync(string ticker, CancellationToken cancellationToken)
    {
        string url = $"https://www.fundamentus.com.br/detalhes.php?papel={ticker}";

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;

            var htmlBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var htmlString = System.Text.Encoding.GetEncoding("ISO-8859-1").GetString(htmlBytes);

            if (htmlString.Contains("Nenhum papel encontrado") || string.IsNullOrWhiteSpace(htmlString))
                return null;

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlString);

            var today = DateTime.UtcNow.Date;

            // 1. Preenchendo a Empresa
            var company = new CompanyAsset
            {
                StockTicker = ticker,
                CompanyName = ExtractValue(doc, "Empresa") ?? "N/A",
                AssetType = ExtractValue(doc, "Tipo") ?? "N/A",
                MarketSector = ExtractValue(doc, "Setor") ?? "N/A",
                IndustryGroup = ExtractValue(doc, "Subsetor") ?? "N/A"
            };

            // 2. Preenchendo a Cotação
            var quote = new MarketQuote
            {
                ReferenceDate = ParseDate(ExtractValue(doc, "Data últ cot")) ?? today,
                ClosingPrice = ParseDecimal(ExtractValue(doc, "Cotação")),
                AverageVolume = ParseDecimal(ExtractValue(doc, "Vol $ méd (2m)")),
                MarketValue = ParseDecimal(ExtractValue(doc, "Valor de mercado")),
                FirmValue = ParseDecimal(ExtractValue(doc, "Valor da firma"))
            };

            // 3. Preenchendo os Indicadores
            var indicator = new MarketIndicator
            {
                ReferenceDate = quote.ReferenceDate,
                PriceEarnings = ParseDecimal(ExtractValue(doc, "P/L")),
                PriceBook = ParseDecimal(ExtractValue(doc, "P/VP")),
                EnterpriseEbitda = ParseDecimal(ExtractValue(doc, "EV / EBITDA")),
                DividendYield = ParseDecimal(ExtractValue(doc, "Div. Yield")) / 100,
                EarningsShare = ParseDecimal(ExtractValue(doc, "LPA")),
                BookShare = ParseDecimal(ExtractValue(doc, "VPA")),
                CapitalReturn = ParseDecimal(ExtractValue(doc, "ROIC")) / 100,
                EquityReturn = ParseDecimal(ExtractValue(doc, "ROE")) / 100,
                NetMargin = ParseDecimal(ExtractValue(doc, "Marg. Líquida")) / 100
            };

            // 4. Preenchendo o Balanço
            var statement = new FinancialStatement
            {
                StatementDate = ParseDate(ExtractValue(doc, "Últ balanço processado")) ?? quote.ReferenceDate,
                TotalAssets = ParseDecimal(ExtractValue(doc, "Ativo")),
                LiquidAssets = ParseDecimal(ExtractValue(doc, "Disponibilidades")),
                CurrentAssets = ParseDecimal(ExtractValue(doc, "Ativo Circulante")),
                TotalEquity = ParseDecimal(ExtractValue(doc, "Patrim. Líq")),
                GrossDebt = ParseDecimal(ExtractValue(doc, "Dív. Bruta")),
                NetDebt = ParseDecimal(ExtractValue(doc, "Dív. Líquida")),
                SharesCount = (long)ParseDecimal(ExtractValue(doc, "Nro. Ações")),

                // Pega os 4 blocos de DRE. Os campos têm nomes idênticos no site, então pegamos pela ordem de aparição no HTML
                YearlyRevenue = ParseDecimal(ExtractValueByOrder(doc, "Receita Líquida", 1)),
                YearlyEbit = ParseDecimal(ExtractValueByOrder(doc, "EBIT", 1)),
                YearlyProfit = ParseDecimal(ExtractValueByOrder(doc, "Lucro Líquido", 1)),

                QuarterlyRevenue = ParseDecimal(ExtractValueByOrder(doc, "Receita Líquida", 2)),
                QuarterlyEbit = ParseDecimal(ExtractValueByOrder(doc, "EBIT", 2)),
                QuarterlyProfit = ParseDecimal(ExtractValueByOrder(doc, "Lucro Líquido", 2))
            };

            return new ScrapedMarketData(company, statement, quote, indicator);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Busca um span contendo o texto (label) e pega o próximo nó de texto ou a tag adjacente.
    /// Padrão do site: <span class="txt">Label</span> <span class="txt">Valor</span>
    /// </summary>
    private string? ExtractValue(HtmlDocument doc, string label)
    {
        var node = doc.DocumentNode.SelectSingleNode($"//span[contains(text(), '{label}')]/../following-sibling::td/span");

        // Em algumas estruturas o texto não está dentro de um span dentro do td
        if (node == null)
            node = doc.DocumentNode.SelectSingleNode($"//span[contains(text(), '{label}')]/../following-sibling::td");

        return node?.InnerText?.Trim();
    }

    /// <summary>
    /// Utilizado quando existem múltiplas labels com o mesmo nome na página (ex: "Receita Líquida" em 12 meses e em 3 meses).
    /// </summary>
    private string? ExtractValueByOrder(HtmlDocument doc, string label, int index)
    {
        var nodes = doc.DocumentNode.SelectNodes($"//span[contains(text(), '{label}')]/../following-sibling::td/span");
        if (nodes == null)
            nodes = doc.DocumentNode.SelectNodes($"//span[contains(text(), '{label}')]/../following-sibling::td");

        if (nodes != null && nodes.Count >= index)
        {
            return nodes[index - 1].InnerText?.Trim();
        }
        return null;
    }

    private decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "-" || value == "N/D") return 0;

        // Remove R$, %, pontos de milhar e troca vírgula por ponto para conversão nativa do .NET
        value = value.Replace("R$", "").Replace("%", "").Replace(".", "").Replace(",", ".").Trim();

        decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result);
        return result;
    }

    private DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "-") return null;

        if (DateTime.TryParseExact(value.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            return date;

        return null;
    }
}