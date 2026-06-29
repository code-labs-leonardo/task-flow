# Revisões das Sugestões de IA

> Este é o arquivo mais importante do registro de uso de IA.
> Documenta o que foi revisado, corrigido ou rejeitado das sugestões
> geradas pelo assistente (Claude Code — Claude Sonnet 4.6).
> Registros honestos de correção valem mais do que o que a IA acertou.

---

## 1. openapi.yaml gerado em inglês — CORRIGIDO

**Sugestão da IA:** gerou o contrato com toda a documentação em inglês.

**Problema:** o tech lead e o avaliador do desafio são brasileiros. A doc em
português facilita revisão e demonstra atenção ao contexto do projeto.

**Correção aplicada:** solicitei retradução completa para português. Mantivemos
apenas os nomes técnicos em inglês: property names dos schemas (`name`, `title`,
`status`, etc.), enum values (`pending`, `in_progress`, `done`, `active`, `archived`)
e operationIds — pois fazem parte do contrato técnico e mudar causaria inconsistência
com a implementação .NET.

---

## 2. Regra 4 ausente no openapi.yaml — CORRIGIDO

**Sugestão da IA:** gerou o openapi.yaml sem refletir a regra de negócio 4 do PDF:
> "Não é permitido excluir tarefas com status `in_progress` ou `done`."

A IA havia mapeado apenas o cenário de tarefa com status `done`.

**Correção aplicada:** acrescentei o exemplo 422 `tarefaEmAndamento` para o cenário
`in_progress` além do `tarefaConcluida` para `done` no endpoint `DELETE /tarefas/{id}`.
Os dois exemplos nomeados foram adicionados explicitamente no schema de responses.

---

## 3. Regra 5 — interpretação fraca da transição de status — CORRIGIDO

**Sugestão da IA:** interpretou a regra 5 do PDF como apenas proibição de **retrocesso**
de status (`done → pending`, `in_progress → pending`). A transição direta `pending → done`
não foi bloqueada na versão inicial.

**Minha análise:** relei o PDF — o texto diz "deve seguir o fluxo: pending → in_progress → done".
A palavra "deve" torna o caminho sequencial **mandatório**, não apenas "sugerido".
Pular uma etapa também viola o fluxo, mesmo que não seja um retrocesso.

**Correção aplicada:** adicionei a regra explicitamente como uma decisão de design
(seção 12 do `docs/decisoes.md`) e incluí o exemplo 422 `pularEtapaParaDone` no
endpoint `PATCH /tarefas/{id}`. A IA aceitou a justificativa após eu apresentar
o trecho do PDF.

---

## 4. `completedAt` ausente como `readOnly` no schema — CORRIGIDO

**Sugestão da IA:** o campo `completedAt` estava presente no schema de request do
`PATCH /tarefas/{id}`, permitindo que o cliente enviasse esse valor.

**Problema:** `completedAt` deve ser gerenciado exclusivamente pela API — é preenchido
automaticamente quando a tarefa transiciona para `done`. Aceitar esse campo do cliente
viola o encapsulamento do domínio.

**Correção aplicada:** `completedAt` foi removido dos schemas de request e marcado
como `readOnly: true` nos schemas de response. A resposta 400 foi adicionada
explicitamente para o caso em que o cliente envie esse campo.

---

## 5. Schemathesis documentado como "opcional" — CORRIGIDO

**Sugestão da IA:** na seção 13 do `docs/decisoes.md` (Estratégia de Testes),
documentou o Schemathesis como uma "opção futura" para validação de contrato.

**Problema:** o PDF exige validação de responses contra o schema OpenAPI como
requisito do desafio — não é algo opcional para o futuro.
Além disso, Schemathesis é uma ferramenta Python — introduzir dependência externa
fora do ecossistema .NET para um projeto ASP.NET Core é uma decisão arquitetural
que precisa de justificativa, não uma sugestão genérica.

**Correção aplicada:** Schemathesis foi explicitamente **rejeitado** na decisão,
com justificativa documentada. A alternativa nativa (`Microsoft.OpenApi + NJsonSchema`)
foi promovida como a solução definitiva, com a stack completa descrita.

---

## 6. Seção "Estrutura de Dados" completamente ausente — CORRIGIDO

**Sugestão da IA:** criou o `docs/decisoes.md` com 11 seções cobrindo arquitetura,
padrões, nomenclatura e testes — mas **omitiu completamente** a seção sobre
Estrutura de Dados.

**Problema:** o PDF lista como **primeiro item** obrigatório do documento de decisões:
> "Estrutura de dados escolhida e o motivo."
Era a seção mais importante e estava ausente.

**Correção aplicada:** após pente-fino final contra o PDF, identifiquei a lacuna
e adicionei a Seção 2 com:
- Modelo de entidades (`Project` e `TaskItem`) com todos os campos, tipos C# e observações
- Decisões de implementação: UUIDs pela aplicação, enums como strings no SQLite,
  timestamps em UTC, `completedAt` gerenciado pelo domínio, configuração via Fluent API
- Alternativas rejeitadas: DataAnnotations nas entidades, IDs inteiros auto-increment

---

## 7. Banco de testes documentado incorretamente — CORRIGIDO

**Sugestão da IA:** na seção de testes, documentou `UseInMemoryDatabase` do EF Core
como banco para os testes de contrato.

**Problema:** `UseInMemoryDatabase` usa um provider diferente do SQLite — não valida
constraints, não suporta operações que o SQLite suporta, e pode mascarar erros que
só aparecem com o banco real. É o anti-pattern clássico em testes de integração .NET.

**Correção aplicada:** substituído por SQLite em modo `:memory:` (`Data Source=:memory:`),
que mantém o mesmo provider do ambiente de produção, garantindo consistência entre
testes e prod. A justificativa foi documentada explicitamente na seção 13 do `decisoes.md`.

---

## 8. .NET Aspire sugerido desnecessariamente — REJEITADO

**Sugestão da IA:** quando questionada sobre tecnologias modernas do .NET para o projeto,
apresentou .NET Aspire como opção viável.

**Meu questionamento:** "O Aspire é um front? O que é e o que faz?"

**Análise após explicação:** .NET Aspire é um framework de orquestração para aplicações
distribuídas — dashboard, service discovery, distributed tracing. Para uma API com
módulo único (Projects e Tasks), é overhead significativo sem benefício real no
escopo do desafio.

**Decisão:** rejeitado. Ficamos com ASP.NET Core Web API sem Aspire.

---

## 9. Mensagem de commit acima do limite de 72 caracteres — CORRIGIDO

**Sugestão da IA:** propôs a mensagem de commit:
`docs(decisoes): add architectural decision records with rejected alternatives and test strategy`
que tem **95 caracteres** na linha de subject.

**Problema:** Conventional Commits recomendam subject line com máximo de 72 caracteres.
Ferramentas de git log e interfaces como GitHub truncam linhas acima desse limite.

**Correção aplicada:** mensagem encurtada para:
`docs(decisoes): add ADR with data model, architecture and test strategy`
que tem exatamente 69 caracteres — dentro do limite, mantendo os termos técnicos relevantes.

---

## 10. Numeração de seções descontrolada — CORRIGIDO

**Problema técnico:** ao inserir a Seção 2 (Estrutura de Dados) após o documento já ter
sido criado com seções numeradas de 1 a 11, todas as seções subsequentes ficaram com
numeração incorreta. A seção que era "3" deveria virar "4", e assim por diante até "14".

**Correção aplicada:** resequenciamento manual de todas as seções afetadas.
A IA inicialmente tentou fazer isso via Edit mas precisou de múltiplas passagens
para acertar todas as ocorrências — incluindo títulos de seção e referências cruzadas.

---

## 11. Paginação ausente nos endpoints de listagem — CORRIGIDO

**Sugestão da IA:** implementou os endpoints `GET /projetos` e `GET /projetos/:id/tarefas`
retornando todos os registros como array plano, sem limite ou metadados de navegação.

**Problema:** retornar todos os registros sem paginação é um anti-pattern para produção —
com volume crescente, o tempo de resposta e o consumo de memória degradam linearmente.
O spec mínimo do PDF não exige paginação, mas isso não justifica uma API que não é
operável em produção real.

**Correção aplicada:** adicionados os query params opcionais `pageNumber` (default 1) e
`pageSize` (default 100, máx 100) em ambos os endpoints. Resposta alterada de array plano
para envelope `PagedResponse<T>` com campos `items`, `pageNumber`, `pageSize`, `totalItems`
e `totalPages`. Openapi.yaml atualizado com os novos schemas `PaginatedProjectResponse`,
`PaginatedTaskResponse` e o componente `PageMeta` compartilhado.

---

## 12. Bloco CountAsync + Skip/Take duplicado nos repositórios — CORRIGIDO

**Sugestão da IA:** ao implementar paginação, gerou o bloco `CountAsync + Skip/Take +
ToListAsync` idêntico em `ProjectRepository` e `TaskItemRepository`.

**Problema:** código duplicado em repositórios tem o mesmo risco de qualquer duplicação:
a próxima correção (ex: adicionar ordenação configurável, timeout, cache) precisaria ser
aplicada em N lugares — e o segundo lugar é o que vai ser esquecido.

**Minha análise:** instinto inicial foi "coloca no projeto Utils". Mas `CountAsync` e
`ToListAsync` são métodos de extensão do `Microsoft.EntityFrameworkCore` — adicionar
EF Core como dependência no Utils violaria o propósito da camada (candidato a NuGet
independente, conforme decisoes.md). A solução correta é uma extension `internal` dentro
do próprio `Infra.Persistence`, onde EF Core já é dependência.

**Correção aplicada:** `QueryableExtensions.ToPagedAsync<T>()` criada em
`Infra.Persistence/Extensions/` como `internal static`. Repositórios reduzidos a:
```csharp
return await query.OrderBy(p => p.CreatedAt).ToPagedAsync(pageNumber, pageSize, ct);
```

---

## 13. Path incorreto no `appsettings.Development.json` — CORRIGIDO

**Sugestão da IA:** configurou o connection string com `Data Source=../../../data/taskflow.db`
(3 níveis acima), que a partir de `src/TaskFlow.Api/` sobe para `code-labs-leonardo/data/` —
diretório inexistente na estrutura do projeto.

**Problema:** o banco de dados está em `task-flow/data/taskflow.db`. Para chegar lá a partir
de `src/TaskFlow.Api/` basta subir **2** níveis (`../../data/taskflow.db`). Com 3 níveis,
a aplicação falharia ao tentar abrir o banco ao rodar localmente — erro silencioso porque
o SQLite cria um arquivo vazio se o diretório existir.

**Correção aplicada:** `../../../data/taskflow.db` → `../../data/taskflow.db` em ambas as
connection strings (`TaskFlowWrite` e `TaskFlowRead`).

---

## 14. FK ausente em `TaskItemConfiguration` — CORRIGIDO

**Sugestão da IA:** ao gerar a configuração Fluent API para `TaskItem`, configurou a propriedade
`ProjectId` com `.IsRequired()` mas sem declarar o relacionamento com `Project`.

**Problema:** sem `HasOne<Project>().WithMany().HasForeignKey(t => t.ProjectId)`, o EF Core
não emite a constraint de chave estrangeira no SQLite. Isso significa que `ProjectId` seria
uma coluna obrigatória sem integridade referencial — seria possível criar tarefas com
`ProjectId` apontando para projetos inexistentes sem que o banco rejeitasse a operação.

**Correção aplicada:** adicionado o bloco completo de relacionamento com
`.OnDelete(DeleteBehavior.Restrict)` — mantém o comportamento já definido na regra de negócio
de exclusão (DELETE /projetos só deve ser bloqueado se houver tarefas associadas, o que a
constraint FK com Restrict garante no nível do banco).

---

## Resumo Geral

| # | Problema | Tipo | Status |
|---|---|---|---|
| 1 | Documentação em inglês | Corrigido | ✓ |
| 2 | Regra 4 de exclusão ausente | Corrigido | ✓ |
| 3 | Interpretação fraca da regra 5 | Corrigido | ✓ |
| 4 | `completedAt` aceito em request | Corrigido | ✓ |
| 5 | Schemathesis como "opcional" | Corrigido | ✓ |
| 6 | Seção "Estrutura de Dados" ausente | Corrigido | ✓ |
| 7 | Banco de testes errado (InMemory) | Corrigido | ✓ |
| 8 | .NET Aspire sugerido | Rejeitado | ✓ |
| 9 | Commit acima de 72 chars | Corrigido | ✓ |
| 10 | Numeração de seções quebrada | Corrigido | ✓ |
| 11 | Paginação ausente nos endpoints de listagem | Corrigido | ✓ |
| 12 | `CountAsync + Skip/Take` duplicado nos repositórios | Corrigido | ✓ |
| 13 | Path com 3 níveis no `appsettings.Development.json` | Corrigido | ✓ |
| 14 | FK `ProjectId → Project` ausente em `TaskItemConfiguration` | Corrigido | ✓ |
