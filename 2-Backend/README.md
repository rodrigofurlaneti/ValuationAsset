# 📈 ValuationAsset - Backend Data Platform

Plataforma backend responsável por coletar, validar, processar e servir dados financeiros e fundamentalistas de ações e FIIs. 

O sistema é construído sobre os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, utilizando o padrão **CQRS** para segregar operações de leitura (API) e escrita (Worker).

---

## 🏛️ Arquitetura do Sistema

O projeto está dividido em duas aplicações principais que compartilham o mesmo domínio:

1. **ValuationAsset.Api (RESTful API):** Camada de apresentação focada em *Queries*. Fornece os dados processados para frontends ou outros consumidores de forma rápida e escalável.
2. **ValuationAsset.Worker (Background Service):** Serviço rodando em segundo plano responsável pelos *Commands*. Ele executa a raspagem/coleta de dados, validação e persistência no banco de dados de forma periódica.

### Divisão de Camadas (Clean Architecture)

* **`Domain`**: Contém o coração da aplicação. Entidades (`CompanyAsset`, `FinancialStatement`, `MarketQuote`, etc.), *Value Objects*, *Interfaces* de repositórios e regras de negócio. Não possui dependências externas.
* **`Application`**: Orquestra os casos de uso utilizando CQRS. Contém os manipuladores de comandos (ex: `SyncMarketDataCommand`) e consultas (ex: `GetAssetValuationQuery`).
* **`Infrastructure`**: Implementação técnica. Repositórios (SQL Server), integrações externas (HTTP Clients para *scraping*) e configurações de banco de dados.
* **`Presentation`**: Controladores da API REST e o host do Worker Service.

---

## ⚙️ Fluxo do Worker de Sincronização

O `ValuationAsset.Worker` é executado de forma cíclica **a cada 1 minuto**. Para garantir performance e integridade, ele segue uma esteira rigorosa de validação antes de qualquer inserção no banco:

### Regras de Processamento (Pipeline)

1. **Tracking de Execução:** O processo se inicia lendo a tabela de controle (ex: `SyncExecutionLog`) para resgatar a data/hora do último processamento bem-sucedido.
2. **Coleta de Dados:** Realiza a extração do *snapshot* atual do mercado a partir da fonte de dados (ex: Fundamentus).
3. **Validação de Delta (Has New Data?):** 
   * Compara os dados recém-coletados com o último registro válido no banco (verificando a data do balanço e variações na cotação diária).
   * **Caminho A (Sem novidades):** O Worker aborta a atualização, registra no log que não houve alterações e entra em repouso até o próximo ciclo.
   * **Caminho B (Dados novos/atualizados):** O Worker avança para a etapa de persistência.
4. **Atualização Transacional:** Todos os dados novos (`MarketQuote`, `MarketIndicator`, `FinancialStatement`) são atualizados utilizando transações no banco de dados (tudo ou nada) para garantir a consistência relacional.
5. **Registro de Sucesso:** Atualiza a tabela `SyncExecutionLog` com o timestamp atual e o status final (`SUCCESS`).

---

## 🛠️ Tecnologias Utilizadas

* **Framework:** .NET 8 (C#)
* **Banco de Dados:** Microsoft SQL Server
* **Arquitetura:** Clean Architecture, DDD, CQRS
* **Bibliotecas Principais:**
  * `MediatR` (Mensageria in-memory para CQRS)
  * `Entity Framework Core` / `Dapper` (Acesso a dados)
  * `Microsoft.Extensions.Hosting` (BackgroundService para o Worker)
  * `HtmlAgilityPack` ou `PuppeteerSharp` (Scraping de dados)

---

## 🚀 Como Executar o Projeto

### Pré-requisitos
* SDK do .NET 8+
* SQL Server (local ou via Docker)

### 1. Configuração do Banco de Dados
Certifique-se de rodar os scripts SQL (disponíveis na pasta `/1-Sql/CreateDatabase/`) para gerar o banco de dados `ValuationAssetDB` e suas respectivas tabelas.

Atualize a *Connection String* no arquivo `appsettings.json` tanto na API quanto no Worker:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ValuationAssetDB;User Id=sa;Password=SuaSenhaForte;TrustServerCertificate=True;"
}
```

### 2. Rodando a API (Consultas)
####  Navegue até a pasta da API e inicie a aplicação:
```bash
cd src/ValuationAsset.Api
dotnet run
```

A documentação interativa (Swagger) estará disponível em http://localhost:5000/swagger.

### 3. Rodando o Worker (Processamento)
Abra um novo terminal, navegue até a pasta do Worker e inicie o serviço:
```bash
cd src/ValuationAsset.Worker
dotnet run
```

### Os logs de execução aparecerão no console a cada minuto, indicando se os dados foram atualizados ou se o ciclo foi ignorado por ausência de dados novos.

### 📂 Estrutura de Diretórios

```Plaintext
├── 1-Sql/
│   └── CreateDatabase/                     # Scripts SQL de criação do banco
└── 2-Backend/
    ├── src/
    │   ├── ValuationAsset.Domain/          # Entidades e Interfaces do Domínio
    │   ├── ValuationAsset.Application/     # Casos de uso (Commands/Queries)
    │   ├── ValuationAsset.Infrastructure/  # EF Core, Dapper, Scraping Services
    │   ├── ValuationAsset.Api/             # Controllers REST API
    │   └── ValuationAsset.Worker/          # Background Services (Worker de 1 min)
    └── tests/
        ├── ValuationAsset.UnitTests/         # Testes unitários
        ├── ValuationAsset.BddTests/          # Testes BDD
        └── ValuationAsset.ArchitectureTests/ # Testes de arquitetura (NetArchTest)
```

