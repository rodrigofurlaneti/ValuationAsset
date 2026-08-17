USE [ValuationAssetDB];
GO

-- 1. Apaga as tabelas "filhas" que dependem de CompanyAsset
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FinancialStatement]') AND type in (N'U'))
    DROP TABLE [dbo].[FinancialStatement];

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MarketQuote]') AND type in (N'U'))
    DROP TABLE [dbo].[MarketQuote];

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MarketIndicator]') AND type in (N'U'))
    DROP TABLE [dbo].[MarketIndicator];

-- 2. Apaga a tabela do Worker
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExecutionLog]') AND type in (N'U'))
    DROP TABLE [dbo].[ExecutionLog];

-- 3. Agora sim, apaga a tabela "pai"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CompanyAsset]') AND type in (N'U'))
    DROP TABLE [dbo].[CompanyAsset];
GO