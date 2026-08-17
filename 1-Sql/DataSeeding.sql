USE ValuationAssetDB;
GO

-- Inserindo os ativos iniciais na tabela CompanyAsset
INSERT INTO CompanyAsset (StockTicker, CompanyName, AssetType, MarketSector, IndustryGroup)
VALUES 
    ('ASAI3', 'ASSAI ON NM', 'ON NM', 'Comércio e Distribuição', 'Alimentos'),
    ('ITUB4', 'ITAUUNIBANCO PN N1', 'PN N1', 'Financeiro e Outros', 'Intermediários Financeiros'),
    ('PETR4', 'PETROBRAS PN N2', 'PN N2', 'Petróleo, Gás e Biocombustíveis', 'Petróleo, Gás e Biocombustíveis'),
    ('VALE3', 'VALE ON NM', 'ON NM', 'Materiais Básicos', 'Mineração'),
    ('WEGE3', 'WEG ON NM', 'ON NM', 'Bens Industriais', 'Máquinas e Equipamentos'),
    ('CVCB3', 'CVC BRASIL ON NM', 'ON NM', 'Consumo Cíclico', 'Viagens e Lazer'),
    ('LREN3', 'LOJAS RENNER ON NM', 'ON NM', 'Consumo Cíclico', 'Comércio');
GO