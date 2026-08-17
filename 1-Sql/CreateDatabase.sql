CREATE DATABASE ValuationAssetDB;

USE ValuationAssetDB;

GO

-- 1. Entity: CompanyAsset (Cadastro da Empresa/FII)
CREATE TABLE CompanyAsset (
    StockTicker VARCHAR(10) PRIMARY KEY, -- ASAI3
    CompanyName VARCHAR(100),            -- ASSAI ON NM
    AssetType VARCHAR(20),               -- ON NM
    MarketSector VARCHAR(100),           -- Comércio e Distribuição
    IndustryGroup VARCHAR(100)           -- Alimentos
);
GO

-- 2. Entity: FinancialStatement (Balanços e DRE)
CREATE TABLE FinancialStatement (
    StatementId INT IDENTITY(1,1) PRIMARY KEY,
    StockTicker VARCHAR(10) REFERENCES CompanyAsset(StockTicker),
    StatementDate DATE,
    
    -- Balance Sheet (Patrimônio)
    TotalAssets DECIMAL(20,2),           -- Ativo Total
    LiquidAssets DECIMAL(20,2),          -- Disponibilidades
    CurrentAssets DECIMAL(20,2),         -- Ativo Circulante
    GrossDebt DECIMAL(20,2),             -- Dívida Bruta
    NetDebt DECIMAL(20,2),               -- Dívida Líquida
    TotalEquity DECIMAL(20,2),           -- Patrimônio Líquido
    
    -- Income Statement 12 Months (DRE 12m)
    YearlyRevenue DECIMAL(20,2),         -- Receita Líquida 12m
    YearlyEbit DECIMAL(20,2),            -- EBIT 12m
    YearlyProfit DECIMAL(20,2),          -- Lucro Líquido 12m
    
    -- Income Statement 3 Months (DRE 3m)
    QuarterlyRevenue DECIMAL(20,2),      -- Receita Líquida 3m
    QuarterlyEbit DECIMAL(20,2),         -- EBIT 3m
    QuarterlyProfit DECIMAL(20,2),       -- Lucro Líquido 3m
    
    SharesCount BIGINT,                  -- Número de Ações
    
    UNIQUE(StockTicker, StatementDate)
);
GO

-- 3. Entity: MarketQuote (Cotações e Valor de Mercado)
CREATE TABLE MarketQuote (
    QuoteId INT IDENTITY(1,1) PRIMARY KEY,
    StockTicker VARCHAR(10) REFERENCES CompanyAsset(StockTicker),
    ReferenceDate DATE,                  -- Data da coleta
    
    ClosingPrice DECIMAL(10,2),          -- Cotação atual
    AverageVolume DECIMAL(20,2),         -- Volume médio (2m)
    MarketValue DECIMAL(20,2),           -- Valor de mercado
    FirmValue DECIMAL(20,2),             -- Valor da firma (Enterprise Value)
    
    UNIQUE(StockTicker, ReferenceDate)
);
GO

-- 4. Entity: MarketIndicator (Indicadores Fundamentalistas)
CREATE TABLE MarketIndicator (
    IndicatorId INT IDENTITY(1,1) PRIMARY KEY,
    StockTicker VARCHAR(10) REFERENCES CompanyAsset(StockTicker),
    ReferenceDate DATE,
    
    -- Valuation Indicators
    PriceEarnings DECIMAL(10,2),         -- P/L
    PriceBook DECIMAL(10,2),             -- P/VP
    EnterpriseEbitda DECIMAL(10,2),      -- EV/EBITDA
    DividendYield DECIMAL(5,4),          -- Div. Yield
    
    -- Per Share Indicators
    EarningsShare DECIMAL(10,2),         -- LPA (Lucro Por Ação)
    BookShare DECIMAL(10,2),             -- VPA (Valor Patrimonial por Ação)
    
    -- Profitability Indicators
    CapitalReturn DECIMAL(5,4),          -- ROIC
    EquityReturn DECIMAL(5,4),           -- ROE
    NetMargin DECIMAL(5,4),              -- Margem Líquida
    
    UNIQUE(StockTicker, ReferenceDate)
);
GO
