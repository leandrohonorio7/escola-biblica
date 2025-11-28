# Escola Bíblica Dominical - Sistema de Gestão

Sistema SPA para gestão de escola bíblica dominical com recursos de multi-tenancy, competições configuráveis e funcionalidade offline.

## Tecnologias

- **Frontend**: Blazor WebAssembly + PWA + MudBlazor
- **Backend**: .NET 8 Web API + ASP.NET Core Identity
- **Banco**: Azure SQL Database Serverless
- **Hospedagem**: Azure Static Web Apps + Azure Functions
- **CI/CD**: GitHub Actions

## Arquitetura

- **Clean Architecture** com separação clara de responsabilidades
- **Multi-tenant** com isolamento de dados por igreja
- **PWA** com cache offline inteligente
- **Sistema flexível de competições** baseado em regras configuráveis

## Funcionalidades

- Gestão de alunos e professores
- Controle de presença
- Sistema de competições gamificadas
- Relatórios e dashboards
- Sincronização offline
- Configurações por igreja

## Como executar

```bash
# Restaurar pacotes
dotnet restore

# Executar API
cd src/EscolaBiblica.WebAPI
dotnet run

# Executar Blazor (em terminal separado)
cd src/EscolaBiblica.BlazorWASM  
dotnet run

# Executar testes
dotnet test
```