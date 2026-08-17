# 📈 ValuationAsset - Backend Data Platform

Plataforma backend robusta projetada para coletar, processar e servir dados financeiros e fundamentalistas da B3. O sistema utiliza **Clean Architecture**, **DDD (Domain-Driven Design)** e **CQRS** para garantir escalabilidade e manutenibilidade.

---

## 🏛️ Arquitetura do Sistema

O projeto é dividido em camadas que garantem o desacoplamento das regras de negócio em relação à persistência e à interface:

1. **ValuationAsset.Api (REST):** Focada em *Queries*. Oferece endpoints para consulta de dados e análises de valuation.
2. **ValuationAsset.Worker (Background Service):** Focado em *Commands*. Executa a varredura automática da B3, persistindo dados com consistência transacional.
3. **Infrastructure (Data & Scraping):** Implementa o acesso a dados via **EF Core** e **Dapper**, além do motor de *Scraping* que lê a B3 em tempo real.
4. **Application:** Contém os casos de uso e a lógica de cálculo (ex: fórmula de Benjamin Graham).
5. **Domain:** Entidades puras e contratos de repositório, sem dependências externas.

---

## ⚙️ Funcionalidades Principais

* **Descoberta Dinâmica de Ativos:** O sistema varre a página de resultados da B3 (Fundamentus) periodicamente e cadastra novos ativos automaticamente na base de dados.
* **Valuation Automático (Graham):** Endpoint exclusivo que calcula o Preço Justo de Graham e a Margem de Segurança, ordenando automaticamente os ativos da mais atrativa para a mais cara.
* **Resiliência Numérica:** O banco de dados utiliza precisão `DECIMAL(18,4)` para evitar *Arithmetic Overflow* ao processar números bilionários ou indicadores de alta precisão.
* **Arquitetura CQRS:** Separação total de escrita (Worker) e leitura (API), permitindo performance otimizada com Dapper para consultas complexas.

---

## 🚀 Como Executar

### Pré-requisitos
* .NET 8 SDK
* SQL Server (ajustado para precisão decimal 18,4)

### 1. Configuração do Banco
Execute os scripts na pasta `/1-Sql/` para criar a base `ValuationAssetDB` com as tabelas otimizadas. Atualize sua *Connection String* nos arquivos `appsettings.json` da **API** e do **Worker**:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ValuationAssetDB;User Id=sa;Password=SuaSenha;TrustServerCertificate=True;"
}

### 2. Rodando a API (Consultas)
####  Abra dois terminais na pasta /2-Backend/:
#### Para iniciar a API (Swagger):

```bash
cd src/ValuationAsset.Api
dotnet run
```

#### A documentação interativa (Swagger) estará disponível em http://localhost:5000/swagger.

### 3. Rodando o Worker (Processamento)
#### Abra um novo terminal, navegue até a pasta do Worker e inicie o serviço:
#### Para iniciar o Worker (Sincronização):

```bash
cd src/ValuationAsset.Worker
dotnet run
```

### Os logs de execução aparecerão no console a cada minuto, indicando se os dados foram atualizados ou se o ciclo foi ignorado por ausência de dados novos.

### 📂 Estrutura de Diretórios

```Plaintext
├── 1-Sql/
│   └── CreateDatabase/                     # Scripts SQL de criação do banco e Scripts SQL (DDL/DML)
└── 2-Backend/
    ├── src/
    │   ├── ValuationAsset.Domain/          # Entidades e Interfaces do Domínio e a Camada pura (Entidades/Interfaces)
    │   ├── ValuationAsset.Application/     # Casos de uso (Commands/Queries) e a Casos de uso (Commands/Queries/Handlers
    │   ├── ValuationAsset.Infrastructure/  # EF Core, Dapper, Scraping Services e a Repositórios (Dapper/EF Core)
    │   ├── ValuationAsset.Api/             # Controllers REST API e a Endpoints REST
    │   └── ValuationAsset.Worker/          # Background Services (Worker de 1 hora) e a Background Service (Scraper B3)
    └── tests/                              # Testes Unitários e de Arquitetura
        ├── ValuationAsset.UnitTests/         # Testes unitários
        ├── ValuationAsset.BddTests/          # Testes BDD
        └── ValuationAsset.ArchitectureTests/ # Testes de arquitetura (NetArchTest)
```

## 🛠️ Tecnologias
### Framework: .NET 8 (C#)
### ORM: EF Core 8 & Dapper
### Mediator: MediatR
### Database: SQL Server

#### Este arquivo agora reflete exatamente o estado atual do seu projeto, incluindo as correções de arquitetura, a precisão decimal ajustada e as novas funcionalidades de scraping e análise de Graham.

