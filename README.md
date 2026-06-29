# TaskFlow API

REST API para gerenciamento de projetos e tarefas, construída em **ASP.NET Core (.NET 10)** seguindo a metodologia **Specification-Driven Development (SDD)** — spec first com OpenAPI 3.0, testes de contrato com xUnit + WebApplicationFactory.

---

## Pré-requisitos

| Ferramenta | Versão mínima |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | **10.0** |
| [Docker](https://docs.docker.com/get-docker/) *(opcional)* | 24+ |
| [Docker Compose](https://docs.docker.com/compose/) *(opcional)* | 2.20+ |

Verifique sua versão instalada:

```bash
dotnet --version
```

---

## Rodar a aplicação

**1. Crie o arquivo de variáveis de ambiente de desenvolvimento** a partir do exemplo:

```bash
cp .env.example .env.development
```

> O arquivo `.env.development` já está no `.gitignore` — nunca é commitado.

**2. Rode a aplicação:**

```bash
dotnet run --project src/TaskFlow.Api
```

A API estará disponível em `http://localhost:5054`.

O banco SQLite é criado automaticamente em `data/taskflow.db` na primeira execução.

---

## Executar os testes de contrato

```bash
dotnet test tests/TaskFlow.ContractTests
```

Os testes usam um banco SQLite em memória temporário isolado por classe — nenhuma configuração extra necessária.

---

## Docker

### Build e execução com Docker Compose

**1. Crie o arquivo de variáveis de ambiente** a partir do exemplo:

```bash
cp .env.example .env.development
```

**2. Suba os containers:**

```bash
docker compose --env-file .env.development up --build
```

A API estará disponível em `http://localhost:5054` (porta definida em `API_PORT` no `.env.development`).

Os dados do SQLite são persistidos no volume `taskflow-data`.

### Somente build da imagem

```bash
docker build -t taskflow-api .
```

---

## Estrutura do projeto

```
task-flow/
├── src/
│   ├── TaskFlow.Api/               # Controllers, middleware, Program.cs
│   ├── TaskFlow.Application/       # Commands, Queries, Validators (CQRS + MediatR)
│   ├── TaskFlow.Domain/            # Entidades e regras de negócio
│   ├── TaskFlow.Infra.Persistence/ # EF Core, DbContexts, Migrations
│   └── TaskFlow.Utils/             # Utilitários compartilhados
├── tests/
│   └── TaskFlow.ContractTests/     # Testes de contrato (xUnit + WebApplicationFactory)
├── ai/                             # Registro de uso de IA (SDD)
├── docs/                           # Decisões de design
└── openapi.yaml                    # Contrato da API (fonte da verdade)
```

---

## Endpoints

| Método | Path | Descrição |
|---|---|---|
| `POST` | `/projetos` | Criar projeto |
| `GET` | `/projetos` | Listar projetos (filtro: `?status=active\|archived`) |
| `GET` | `/projetos/:id` | Buscar projeto por ID |
| `PATCH` | `/projetos/:id` | Atualizar projeto (nome, descrição, status) |
| `POST` | `/projetos/:id/tarefas` | Criar tarefa em um projeto |
| `GET` | `/projetos/:id/tarefas` | Listar tarefas (filtro: `?status=...&priority=...`) |
| `PATCH` | `/tarefas/:id` | Atualizar tarefa (título, descrição, status, prioridade) |
| `DELETE` | `/tarefas/:id` | Excluir tarefa |
