CREATE DATABASE ValuationAssetDB;
GO

USE ValuationAssetDB;
GO

-- 1. Entity: CompanyAsset (Cadastro da Empresa/FII)
CREATE TABLE CompanyAsset (
    StockTicker VARCHAR(10) PRIMARY KEY,
    CompanyName VARCHAR(100),
    AssetType VARCHAR(20),
    MarketSector VARCHAR(100),
    IndustryGroup VARCHAR(100)
);
GO

-- 2. Entity: FinancialStatement (Balanços e DRE)
CREATE TABLE FinancialStatement (
    StatementId INT IDENTITY(1,1) PRIMARY KEY,
    StockTicker VARCHAR(10) REFERENCES CompanyAsset(StockTicker),
    StatementDate DATE,
    
    TotalAssets DECIMAL(18,2),
    LiquidAssets DECIMAL(18,2),
    CurrentAssets DECIMAL(18,2),
    GrossDebt DECIMAL(18,2),
    NetDebt DECIMAL(18,2),
    TotalEquity DECIMAL(18,2),
    
    YearlyRevenue DECIMAL(18,2),
    YearlyEbit DECIMAL(18,2),
    YearlyProfit DECIMAL(18,2),
    
    QuarterlyRevenue DECIMAL(18,2),
    QuarterlyEbit DECIMAL(18,2),
    QuarterlyProfit DECIMAL(18,2),
    
    SharesCount BIGINT,
    
    UNIQUE(StockTicker, StatementDate)
);
GO

-- 3. Entity: MarketQuote (Cotações e Valor de Mercado)
CREATE TABLE MarketQuote (
    QuoteId INT IDENTITY(1,1) PRIMARY KEY,
    StockTicker VARCHAR(10) REFERENCES CompanyAsset(StockTicker),
    ReferenceDate DATE,
    
    ClosingPrice DECIMAL(18,2),
    AverageVolume DECIMAL(18,2),
    MarketValue DECIMAL(18,2),
    FirmValue DECIMAL(18,2),
    
    UNIQUE(StockTicker, ReferenceDate)
);
GO

-- 4. Entity: MarketIndicator (Indicadores Fundamentalistas - Precisão aumentada)
CREATE TABLE MarketIndicator (
    IndicatorId INT IDENTITY(1,1) PRIMARY KEY,
    StockTicker VARCHAR(10) REFERENCES CompanyAsset(StockTicker),
    ReferenceDate DATE,
    
    PriceEarnings DECIMAL(18,4),
    PriceBook DECIMAL(18,4),
    EnterpriseEbitda DECIMAL(18,4),
    DividendYield DECIMAL(18,4),
    
    EarningsShare DECIMAL(18,4),
    BookShare DECIMAL(18,4),
    
    CapitalReturn DECIMAL(18,4),
    EquityReturn DECIMAL(18,4),
    NetMargin DECIMAL(18,4),
    
    UNIQUE(StockTicker, ReferenceDate)
);
GO

-- 5. Entity: ExecutionLog (Controle de Execução do Worker)
CREATE TABLE ExecutionLog (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    ExecutionTime DATETIME2 NOT NULL,
    ProcessStatus VARCHAR(50) NOT NULL,
    LogMessage VARCHAR(MAX) NULL,
    RecordsAffected INT DEFAULT 0
);
GO