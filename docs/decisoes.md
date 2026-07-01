# Decisões de Design — TaskFlow API

> Este documento registra as decisões arquiteturais tomadas antes da implementação,
> seguindo o fluxo SDD (Specification-Driven Development).
> O objetivo não é descrever o que foi construído, mas **justificar o porquê** de cada escolha.

---

## 1. Arquitetura Geral

**Decisão:** Clean Architecture + DDD + CQRS com MediatR.

**Justificativa:**
- **Clean Architecture** garante isolamento total entre as camadas. Uma mudança de banco de dados,
  cache ou integração externa não quebra a camada de domínio ou aplicação.
- **DDD** organiza o código por contexto de negócio (Projects, Tasks), não por tipo técnico
  (Controllers, Services). Facilita a leitura e evolução do módulo.
- **CQRS** separa intenções de leitura e gravação em handlers distintos, tornando cada operação
  explícita, testável e com responsabilidade única.
- **MediatR** é o padrão de mercado para CQRS em .NET, utilizado na arquitetura de referência
  oficial da Microsoft (eShopOnContainers). Permite um pipeline de comportamentos (validação,
  logging) desacoplado dos handlers.

**Alternativas consideradas e rejeitadas:**
- **Minimal APIs** — descartadas. Controllers permitem melhor organização por responsabilidade (SRP),
  são mais legíveis para revisão técnica e suportam a separação em um controller por operação.
- **MVC com Services** — descartado. Services tendem a crescer em "God Services" sem a disciplina
  do CQRS. Handlers isolados por operação são mais fáceis de testar e de evoluir.
- **Mediator próprio** — considerado. MediatR foi mantido por ser o padrão de mercado amplamente
  reconhecido e por estar presente na arquitetura de referência oficial da Microsoft.

**Camadas e responsabilidades:**

```
TaskFlow.Api              → Entrada HTTP. Controllers finos — apenas recebem request,
                            disparam comando/query via MediatR e retornam response.

TaskFlow.Application      → Orquestração. Commands, Queries, Handlers, Validators, DTOs.
                            Conhece o domínio mas não conhece infraestrutura.

TaskFlow.Domain           → Núcleo do negócio. Entidades, enums, interfaces de repositório.
                            Zero dependência externa — puro C#.

TaskFlow.Infra.Persistence → Implementação de persistência. EF Core, DbContexts,
                             implementações dos repositórios. Conhece o domínio,
                             domínio não conhece ela.

TaskFlow.Utils            → Código utilitário reutilizável (extensions, constantes).
                            Candidato a publicação como pacote NuGet interno.
```

---

## 2. Estrutura de Dados

**Decisão:** Duas entidades — `Project` e `TaskItem` — com relacionamento 1:N.

### Modelo de dados

```
Project (1) ──────────── (N) TaskItem
```

**Project:**
| Campo | Tipo C# | Observação |
|---|---|---|
| Id | `Guid` | Gerado pela aplicação via `Guid.NewGuid()` |
| Name | `string` | Obrigatório, máx. 100 caracteres |
| Description | `string?` | Opcional, nullable |
| Status | `ProjectStatus` | Enum — `Active` (padrão) \| `Archived` |
| CreatedAt | `DateTime` | UTC, gerado automaticamente na criação |

**TaskItem:**
| Campo | Tipo C# | Observação |
|---|---|---|
| Id | `Guid` | Gerado pela aplicação via `Guid.NewGuid()` |
| Title | `string` | Obrigatório, máx. 200 caracteres |
| Description | `string?` | Opcional, nullable |
| Status | `TaskStatus` | Enum — `Pending` (padrão) \| `InProgress` \| `Done` |
| Priority | `TaskPriority` | Enum — `Low` \| `Medium` \| `High`. Obrigatório |
| CreatedAt | `DateTime` | UTC, gerado automaticamente na criação |
| CompletedAt | `DateTime?` | UTC, nullable — preenchido automaticamente ao transicionar para `Done` |
| ProjectId | `Guid` | Chave estrangeira para `Project` |

### Decisões de implementação do modelo

**UUIDs gerados pela aplicação**, não pelo banco:
- Garante que o ID é conhecido antes de persistir — handlers podem retornar o ID sem round-trip ao banco.
- Compatível com eventual migração para banco distribuído.

**Enums armazenados como strings no SQLite:**
- Legibilidade direta no banco (`"Active"`, `"Pending"`) em vez de inteiros opacos.
- Evita bugs de mapeamento ao adicionar novos valores de enum.

**Timestamps em UTC (`DateTime.UtcNow`):**
- Evita ambiguidade de fuso horário. A conversão para local é responsabilidade do cliente.

**`CompletedAt` setado pela camada de domínio:**
- A entidade `TaskItem` encapsula a regra — ao transicionar para `Done`, o próprio método de domínio preenche `CompletedAt`. Nenhum handler precisa conhecer essa regra.

**EF Core configurado via Fluent API (`IEntityTypeConfiguration<T>`):**
- Mantém as entidades de domínio limpas, sem atributos de infraestrutura (`[Column]`, `[MaxLength]`, etc.).
- Configurações de banco ficam isoladas na camada `Infra.Persistence`.

**Alternativas consideradas e rejeitadas:**
- **DataAnnotations nas entidades** — descartado. Acopla infraestrutura ao domínio, violando o isolamento da Clean Architecture.
- **IDs inteiros auto-increment** — descartado. UUIDs são requisito do spec e eliminam dependência de sequência do banco.

---

## 3. Estrutura de Pastas por Contexto

**Decisão:** Organizar por contexto de negócio em todas as camadas, não por tipo técnico.

**Justificativa:** Facilita a navegação — ao trabalhar em "Projects", todos os arquivos
relacionados estão no mesmo caminho em todas as camadas.

**Exemplo do padrão adotado:**

```
TaskFlow.Application/
└── {Contexto}/
    ├── Commands/
    │   └── {Contexto}{Acao}/
    │       ├── {Contexto}{Acao}Command.cs
    │       ├── {Contexto}{Acao}Handler.cs
    │       └── {Contexto}{Acao}Validator.cs
    ├── Queries/
    │   └── {Contexto}{Acao}/
    │       ├── {Contexto}{Acao}Query.cs
    │       └── {Contexto}{Acao}Handler.cs
    └── DTOs/
        ├── {Contexto}Request.cs
        └── {Contexto}Response.cs
```

O contexto vem primeiro para agrupar visualmente todas as operações do mesmo domínio
— `ProjectCreate`, `ProjectUpdate`, `ProjectList` ficam juntos ao ordenar alfabeticamente.

O mesmo contexto se reflete em `Domain`, `Infra.Persistence` e `Api`.

---

## 4. Controllers com Responsabilidade Única (SOLID)

**Decisão:** Um controller por operação — `{Contexto}CreatorController`, `{Contexto}SearchController`, etc.

**Justificativa:** Segue o princípio SRP (Single Responsibility). Cada controller tem
uma única razão para mudar. Evita o anti-pattern de "God Controller" com 8+ actions
e múltiplas dependências injetadas.

**Exemplo do padrão:**
```
Api/{Contexto}/
├── {Contexto}CreatorController.cs    → POST
├── {Contexto}SearchController.cs     → GET (lista e por ID)
├── {Contexto}UpdaterController.cs    → PATCH
└── {Contexto}DeleterController.cs    → DELETE
```

Consistente com a nomenclatura `{Contexto}{Acao}` adotada em toda a solução.

---

## 5. Pipeline de Validação com MediatR

**Decisão:** Validação centralizada via `IPipelineBehavior<TRequest, TResponse>` com FluentValidation.

**Justificativa:** O controller não valida — ele delega. O pipeline do MediatR intercepta
toda requisição (Commands e Queries) antes de chegar ao Handler.
O `ValidationBehavior` executa apenas quando há validators registrados para aquele tipo
(tipicamente Commands). O `LoggingBehavior` age em todas as requisições.
Se a validação falha, lança exceção capturada pelo middleware de erros e
convertida em `ValidationProblemDetails` (400) nativo do ASP.NET Core.

**Fluxo:**
```
Controller → MediatR.Send(Command | Query)
               └→ LoggingBehavior      (todas as requisições — loga request/response)
                    └→ ValidationBehavior   (apenas requests com validators registrados)
                         └→ Handler     (executa regra de negócio)
```

**Alternativas consideradas e rejeitadas:**
- **DataAnnotations** — descartado. Validações ficam espalhadas nos DTOs, sem contexto
  de negócio. FluentValidation centraliza as regras no validator correspondente ao Command,
  mantendo os DTOs limpos.

---

## 6. Tratamento de Erros

**Decisão:** `ProblemDetails` e `ValidationProblemDetails` nativos do ASP.NET Core (RFC 7807),
com handler de exceções via `UseExceptionHandler(options => options.Run(...))`.

**Justificativa:** Padrão nativo desde .NET 7, sem dependência de bibliotecas externas.
Garante consistência no formato de todos os erros da API.

| Situação | Status | Tipo |
|---|---|---|
| Campo inválido ou ausente | 400 | `ValidationProblemDetails` |
| Recurso não encontrado | 404 | `ProblemDetails` |
| Violação de regra de negócio | 422 | `ProblemDetails` |

Erros de negócio (422) retornam `detail` com mensagem explicativa específica,
conforme definido no contrato `openapi.yaml`.

**Implementação:** forma clássica via `RequestDelegate` —
`app.UseExceptionHandler(options => options.Run(...))` — por ser independente de
ambiente (`Development` vs `Production`). Detalhes e correção em `ai/revisoes.md §15`.

**Referência:** [Handle errors in ASP.NET Core — Microsoft Docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)

---

## 7. Persistência — Dois DbContexts

**Decisão:** `TaskFlowWriteDbContext` para gravação e `TaskFlowReadDbContext` para leitura.

**Justificativa:** Separa intenções em código — queries nunca ativam change tracking
acidentalmente. Permite escalar para um banco de leitura separado (ex: PostgreSQL com
read replica) apenas alterando a connection string `TaskFlowRead`, sem tocar nos handlers.

```json
"ConnectionStrings": {
  "TaskFlowWrite": "Data Source=/app/data/taskflow.db",
  "TaskFlowRead":  "Data Source=/app/data/taskflow.db"
}
```

Hoje apontam para o mesmo arquivo SQLite. A separação é arquitetural, não operacional.

**Alternativas consideradas e rejeitadas:**
- **DbContext único com `.AsNoTracking()` por query** — descartado. Depende de disciplina
  do desenvolvedor em cada handler. Dois contextos tornam a separação estrutural e garantida
  pelo compilador — uma Query que recebe `TaskFlowWriteDbContext` não compila.

---

## 8. Banco de Dados — SQLite

**Decisão:** SQLite com arquivo em `./data/taskflow.db`.

**Justificativa:** O desafio aceita SQLite ou in-memory. SQLite foi escolhido por ser
mais próximo de um banco real (persiste dados entre reinicializações, suporta migrations),
mantendo a simplicidade de setup — sem container de banco de dados separado.

O arquivo é montado via volume Docker, garantindo que os dados sobrevivam ao ciclo
de vida do container.

**Alternativas consideradas e rejeitadas:**
- **In-memory** — descartado. Não persiste dados entre reinicializações e não suporta
  migrations, tornando os testes menos representativos de um ambiente real.
- **PostgreSQL** — considerado para o Docker. Descartado para manter o setup simples
  e dentro do escopo do desafio. A separação de DbContexts já prepara a migração futura.

---

## 9. Configuração e Ambiente

**Decisão:** `appsettings` por ambiente + `.env` injetado via Docker.

**Justificativa:** Segue o padrão nativo do ASP.NET Core. O `.env` não é lido diretamente
pelo .NET — é injetado pelo Docker como variáveis de ambiente, que o `IConfiguration`
lê automaticamente. Nenhuma lib adicional necessária.

```
appsettings.json               → configuração base (sem secrets)
appsettings.Development.json   → desenvolvimento local
appsettings.Production.json    → produção (valores sensíveis via env vars do Docker)
.env.example                   → template commitado no repositório
.env                           → valores reais (gitignored)
```

---

## 10. Nomenclatura — Entidade `TaskItem`

**Decisão:** A entidade de domínio "Tarefa" foi nomeada `TaskItem`.

**Justificativa:** `Task` é um tipo reservado do .NET (`System.Threading.Tasks.Task`).
Usar `Task` como nome de entidade causaria ambiguidade de namespace e necessidade
de aliases em todo o projeto. `TaskItem` é o nome de mercado adotado nesse cenário.

---

## 11. Regra de Negócio Adicional — Projeto Arquivado Não Pode Ser Reativado

**Decisão:** Alterar o `status` de um projeto de `archived` para `active` é proibido — retorna 422.

**Justificativa:** O PDF define apenas a regra de arquivamento (Regra 1: não arquivar com tarefas
`in_progress`), mas não especifica o comportamento ao tentar reverter um projeto arquivado.

A decisão foi **proibir a reativação** (422 com mensagem explicativa) pelos seguintes motivos:
- O PDF não prevê reativação — qualquer implementação seria especulação de requisito.
- Um sistema que permite arquivar e reativar livremente perde o significado do estado `archived`
  como estado terminal de ciclo de vida.
- A opção silenciosa (aceitar mas não fazer nada) foi descartada por ser o pior dos cenários:
  o cliente recebe 200 e presume que a operação foi executada.

A regra foi implementada no método `Project.Activate()` da entidade de domínio, não no handler,
seguindo o princípio de que regras de negócio pertencem ao domínio. Ver `ai/revisoes.md §16`.

---

## 12. Regra de Negócio Adicional — Transição de Status

**Decisão:** A transição `pending → done` direta é **proibida**.

**Justificativa:** O PDF especifica o fluxo `pending → in_progress → done` como obrigatório
("deve seguir o fluxo"). A palavra "deve" torna o caminho sequencial mandatório —
pular `in_progress` viola o contrato definido, mesmo que não seja uma operação
de retrocesso. Essa interpretação foi adotada como decisão de design e está refletida
no `openapi.yaml` com exemplo de 422 específico para esse caso.

---

## 13. Estratégia de Testes

**Decisão:** Testes de contrato com `WebApplicationFactory` + xUnit + `System.Net.Http.Json`
+ `Microsoft.OpenApi` + `NJsonSchema` para validação dos responses contra o schema OpenAPI.

**Justificativa:** O desafio exige explicitamente que os responses sejam validados contra
o schema `openapi.yaml`. Isso vai além de verificar status codes — garante que o body
retornado está em conformidade com o contrato definido antes da implementação.

`WebApplicationFactory` sobe a aplicação em memória com todas as camadas reais
(controllers, pipeline MediatR, validações, banco), garantindo que o que é testado
é o mesmo que vai para produção.

`Microsoft.OpenApi` lê o `openapi.yaml` e `NJsonSchema` valida o JSON retornado
contra o schema correspondente ao endpoint — qualquer desvio de contrato quebra o teste.

**Stack de testes completa:**
```
xUnit
  + WebApplicationFactory     → sobe a API real em memória
  + System.Net.Http.Json      → serialização/desserialização HTTP
  + Microsoft.OpenApi         → leitura e parsing do openapi.yaml
  + NJsonSchema               → validação do response body contra o schema
```

**O que os testes cobrem:**
| Cenário | Status esperado |
|---|---|
| Criação de recursos válidos | 201 |
| Campos obrigatórios ausentes | 400 |
| Recurso inexistente | 404 |
| Violação de regra de negócio | 422 |
| Transição de status inválida | 422 |
| Exclusão de tarefa não-pending | 422 |
| Arquivar projeto com in_progress | 422 |

**Alternativas consideradas e rejeitadas:**
- **Testes unitários para validar contrato** — rejeitado. Testes unitários com mocks
  validam a lógica isolada, não o contrato HTTP. Um mock que retorna 422 não prova
  que a API real retorna 422 com o body correto no formato ProblemDetails.
- **Schemathesis** — rejeitado para este projeto. Ferramenta externa (Python), exige
  setup adicional fora do ecossistema .NET. `Microsoft.OpenApi + NJsonSchema` oferece
  a mesma validação de schema de forma nativa dentro do projeto xUnit, sem dependência
  de ferramentas externas.

**Banco nos testes:** SQLite em modo `:memory:` (`Data Source=:memory:`) — mantém o mesmo
provider do ambiente de produção (mesmas constraints, mesmas regras do SQLite), garantindo
que os testes não passem por diferenças de comportamento entre providers.
`UseInMemoryDatabase` do EF Core foi descartado por não ser SQLite — não valida constraints
e pode mascarar erros que só aparecem com o banco real.

---

## 14. Docker — API em Container

**Decisão:** API dockerizada via `docker-compose` com volume para o SQLite.

**Justificativa:** Garante ambiente reproduzível independente da máquina do desenvolvedor.
O volume `./data:/app/data` persiste o banco entre reinicializações do container.

```yaml
services:
  taskflow-api:
    build: .
    volumes:
      - ./data:/app/data
    env_file: .env
```

---

## 15. DTO de entrada para Commands com muitos parâmetros

**Decisão:** Commands com mais de 3 parâmetros de payload devem encapsular os dados
em um record de entrada dedicado (`*Input`), mantendo o Command com apenas `Id + Data`.

```csharp
// Antes — record flat com 6 params
public record TaskUpdateCommand(Guid Id, string? Title, string? Description,
    string? Status, string? Priority, string? CompletedAt) : IRequest<TaskResponse>;

// Depois — Command + DTO separado
public record TaskUpdateInput(string? Title, string? Description,
    string? Status, string? Priority, string? CompletedAt);

public record TaskUpdateCommand(Guid Id, TaskUpdateInput Data) : IRequest<TaskResponse>;
```

**Justificativa:**
Records com muitos parâmetros posicionais sofrem de dois problemas práticos.
Primeiro, qualquer adição ou reordenação de parâmetro quebra todos os call sites,
gerando conflito de merge em qualquer branch que toque o mesmo Command.
Segundo, o construtor posicional exige que o chamador conheça a ordem dos
parâmetros — acoplamento frágil que não aparece em code review.

O padrão `*Input` + `*Command(Id, Data)` resolve os dois: novos campos entram
no record `Input` sem tocar a assinatura do Command, e o handler acessa os dados
por nome (`request.Data.Title`), não por posição.

**Alternativa rejeitada:** usar `[FromBody]` diretamente como parâmetro do handler.
Isso misturaria a camada de transporte (HTTP) com a camada de aplicação (MediatR),
quebrando a separação de responsabilidades da Clean Architecture.

**Convenção adotada:** suffix `Input` para o record de payload (ex: `TaskUpdateInput`,
`ProjectUpdateInput`). O suffix `Request` fica reservado para os DTOs da camada API
(`TaskUpdateRequest` no controller), evitando conflito de nomes entre camadas.

---

## 16. Paginação nos endpoints de listagem

**Decisão:** `GET /projetos` e `GET /projetos/:id/tarefas` retornam um envelope paginado
(`PagedResponse<T>`) com metadados de navegação. Parâmetros opcionais via query string:
`pageNumber` (padrão 1) e `pageSize` (padrão 100, máx. 100).

```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 20,
  "totalItems": 42,
  "totalPages": 3
}
```

**Justificativa:** Os endpoints de listagem não têm paginação no spec mínimo do PDF, mas
retornar todos os registros sem limite é um anti-pattern para produção — um projeto com
centenas de tarefas degradaria tempo de resposta e consumo de memória linearmente.
O envelope paginado vai além do mínimo exigido, demonstra pensamento de produto e resolve
um problema real sem complexidade excessiva.

`pageNumber` e `pageSize` são parâmetros opcionais: clientes que não enviam recebem a
primeira página com 100 itens (comportamento razoável e backward-compatible). Clientes
que precisam de navegação explícita controlam via query string.

**Implementação:**
```
Repository: CountAsync + Skip/Take na mesma query (dois roundtrips, otimizável com
            COUNT(*) OVER() em SQLite se necessário no futuro)

Handler:    Math.Max(1, pageNumber) + Math.Clamp(pageSize, 1, 100) — validação
            defensiva sem validator dedicado. pageSize acima de 100 é silenciosamente
            clampado, sem erro, para não quebrar clientes que tentam valores maiores.

Response:   PagedResponse<T> genérico reutilizável em qualquer endpoint de listagem.
```

**Alternativas consideradas e rejeitadas:**
- **Cursor-based pagination** — descartado. Mais complexo de implementar e documentar.
  Offset pagination é suficiente para o volume esperado do desafio e mais familiar para APIs REST.
- **`Link` header (RFC 5988)** — descartado. Padrão GitHub/GitHub API. Mais difícil de consumir
  sem lib cliente. Envelope JSON é mais explícito e discoverável para quem lê o contrato OpenAPI.
- **`X-Total-Count` header** — descartado. Headers customizados não aparecem no schema OpenAPI
  automaticamente, reduzindo a aderência spec-first do projeto.
