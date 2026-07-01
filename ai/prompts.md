# Prompts Utilizados

> Registro dos prompts de maior impacto técnico usados com Claude Code (Claude Sonnet 4.6).
> O objetivo não é listar tudo que foi perguntado, mas documentar os prompts
> onde houve intenção deliberada de extrair análise, revisão crítica ou conhecimento especializado.

---

## 1. Leitura estruturada do desafio

**Contexto do prompt:**
Antes de qualquer decisão, precisava garantir que o assistente leu o PDF com as mesmas
"lentes" que eu usaria como dev senior — não apenas listar endpoints, mas identificar
regras implícitas, ambiguidades.

**Prompt:**
> "Você deve ser um especialista, reveja os contratos do PDF e revise as restrições
> e as regras de negócio. Avalie de forma individual a parte de restrições de negócio."

**Por que funciona:**
Pedi avaliação individual por restrição, não uma análise geral. Isso força o modelo
a tratar cada regra como um caso isolado e reduz o risco de omissão silenciosa.

**Produziu:**
Análise granular das 5 regras, identificando 3 gaps reais no contrato OpenAPI gerado
e 1 ambiguidade crítica que levou à decisão sobre a transição `pending → done`.

---

## 2. Vetting de tecnologia nova — .NET Aspire

**Contexto do prompt:**
Queria avaliar o .NET Aspire como stack antes de rejeitar ou adotar — não aceitar
sugestão sem entender o que está sendo trazido para o projeto.

**Prompt:**
> "Por que devo usar o Aspire e o que ele tem de ganho?"

**Por que funciona:**
A pergunta força uma justificativa, não uma descrição. "O que é?" abre espaço
para resposta genérica; "por que devo usar e o que ganho?" obriga o modelo a
contextualizar o valor dentro do cenário concreto. A partir da resposta,
eu mesmo tirei a conclusão sobre adequação ao projeto.

**Produziu:**
Explicação objetiva do .NET Aspire (orquestração para aplicações distribuídas,
não um framework de API). Resultado: tecnologia rejeitada — overhead desnecessário
para o escopo do desafio.

---

## 3. Geração do contrato OpenAPI com critério SDD

**Contexto do prompt:**
A geração do `openapi.yaml` foi o primeiro artefato do projeto — e precisava sair
certo, porque no SDD ele é a fonte da verdade para toda a implementação.

**Prompt:**
> "Começando pelo openapi.yaml — o PDF define que o contrato deve ser a fonte
> da verdade antes de qualquer implementação. Com base nas regras e endpoints
> do documento, gere o contrato revisando cada restrição de negócio individualmente."

**Por que funciona:**
Contextualizei a metodologia SDD antes de pedir geração e exigi granularidade
por restrição. Isso evita que o modelo gere um contrato genérico — cada regra
do PDF precisa ter representação explícita no YAML.

**Produziu:**
Rascunho completo com todos os endpoints, schemas e responses. Estava em inglês —
solicitei retradução para português na sequência (decisão documentada em `decisoes.md`).

---

## 4. Revisão direcionada das regras de negócio

**Contexto do prompt:**
Após ter o contrato em mãos, fiz uma segunda passagem focada exclusivamente nas
restrições — colando as 5 regras do PDF diretamente no prompt para comparação direta.

**Prompt:**
> "Avalie agora o openapi de forma individual a parte de restrições de negócio."
> *(+ as 5 regras do PDF coladas no prompt)*

**Por que funciona:**
Forcing function: com as regras e o contrato na mesma janela de contexto, o modelo
não pode "lembrar" de uma versão incorreta. A comparação é direta e verificável.

**Produziu:**
Identificação de 2 gaps concretos e 1 ambiguidade na regra 5 sobre transição de status.
Correções aplicadas diretamente no `openapi.yaml`.

---

## 5. Decisão sobre regra de transição — análise de ambiguidade

**Contexto do prompt:**
A IA interpretou a regra 5 apenas como proibição de retrocesso. Eu discordava —
e precisava apresentar o argumento antes de aceitar ou rejeitar a sugestão.

**Prompt:**
> "Conforme está no documento, a transição deve seguir o fluxo definido —
> ir para done só é permitido se passar pelo in_progress. As demais foram corrigidas, certo?"
> *(compartilhando o print do trecho do PDF como evidência)*

**Por que funciona:**
Ancorei a regra no próprio documento antes de pedir confirmação. A IA havia
interpretado apenas como proibição de retrocesso — ao apresentar o trecho do PDF
como evidência, forcei a reavaliação com base na fonte, não na interpretação prévia.

**Produziu:**
Confirmação da interpretação com justificativa: "deve seguir o fluxo" é prescritivo,
não apenas permissivo. Transição `pending → done` bloqueada e documentada na seção 12
do `decisoes.md` e no `openapi.yaml` com exemplo 422 nomeado.

---

## 6. Aprendizado sobre ADR e SDD

**Contexto do prompt:**
Primeira vez usando SDD em um projeto novo — precisava entender o propósito do
`docs/decisoes.md` antes de escrever qualquer linha dele.

**Prompt:**
> "No SDD, o docs/decisoes.md deve registrar decisões arquiteturais com justificativa —
> o porquê de cada escolha, não o que foi construído. Considerando as decisões já tomadas
> para esse projeto (arquitetura, banco, nomenclatura, testes), estruture o documento
> nesse formato."

**Por que funciona:**
Defini o critério de qualidade antes de pedir a estrutura: ADR é justificativa, não
descrição. Isso garante que o documento saia orientado ao "porquê" desde a primeira
versão, sem precisar reescrever depois.

**Produziu:**
Diferenciação clara entre ADR (o porquê) e documentação técnica (o quê).
Estrutura inicial com 11 seções orientadas a justificativas, não a descrições.

---

## 7. Iteração crítica — "Tem algo que poderia ser melhorado?"

**Contexto do prompt:**
Após ter o rascunho do `decisoes.md`, não aceitei como versão final.
Solicitei revisão crítica antes de considerar o documento fechado.

**Prompt:**
> "Tem algo que poderia ser melhorado?"

**Por que funciona:**
Pergunta aberta com expectativa implícita de crítica — não de validação.
O modelo tem tendência a confirmar o que foi produzido; essa formulação
contraria isso, dando permissão explícita para encontrar falhas.

**Produziu:**
3 gaps identificados: ausência de alternativas rejeitadas, imprecisão no pipeline
do MediatR, e seção de testes completamente faltando. Todos aplicados.

---

## 8. Scaffolding da solução guiado pela spec

**Contexto do prompt:**
Com a Etapa 1 completa e commitada, era hora de iniciar a implementação.
No SDD, a estrutura do código deve espelhar as decisões já documentadas —
não o contrário. Criar os projetos e classes antes de ter a spec seria
implementar às cegas.

**Prompt:**
> "A especificação está completa e commitada. Inicie a Etapa 2 do SDD:
> primeiro crie toda a estrutura de projetos da solution (Domain, Application,
> Infra.Persistence, Utils, ContractTests) com referências entre camadas e pacotes —
> depois crie as classes seguindo exatamente o modelo de dados e convenções
> definidas em docs/decisoes.md. O contrato openapi.yaml é a fonte da verdade."

**Por que funciona:**
Separei estrutura de código em dois passos explícitos. Referências e pacotes
corretos antes de qualquer classe evita mover arquivos depois.
Ancorar em `decisoes.md` e no `openapi.yaml` garante que a implementação
segue a spec — não que a spec segue a implementação.

**Produziu:**
Criação completa de todos os projetos, referências entre camadas, pacotes NuGet
e classes — alinhados ao contrato OpenAPI e às decisões arquiteturais definidas
na Etapa 1.

---

## 9. Revisão de escopo e contratos HTTP pós-scaffolding

**Contexto do prompt:**
Com toda a estrutura gerada, era necessário revisar criticamente antes de prosseguir.
Código gerado por IA tende a ser sintaticamente correto mas semanticamente incorreto —
especialmente em detalhes de contrato HTTP que parecem certos mas divergem do spec.

**Prompt:**
> "Antes de seguir com README, Dockerfile e testes, revisa todo o código gerado:
> verifica se as libs estão dentro do escopo especificado no PDF, se as regras de
> negócio estão implementadas corretamente e se os contratos HTTP — status codes,
> field names, enum values — estão alinhados com o openapi.yaml."

**Por que funciona:**
Especifiquei três dimensões de revisão: escopo de libs, lógica de negócio e contrato HTTP.
Pedir as três juntas força cobertura ampla sem deixar gaps.
Ancorar em "alinhado com o openapi.yaml" garante que a fonte da verdade
é o spec — não o que a IA acha que está certo.

**Produziu:**
Identificação do bug crítico: `TaskItemStatus.InProgress.ToString()` → `"inprogress"`
enquanto o spec exige `"in_progress"`. Detectado antes dos testes serem escritos,
evitando que todos os testes de status falhassem sem causa aparente.

---

## 10. Pente fino final — validação cruzada com o PDF

**Contexto do prompt:**
Antes de commitar o `decisoes.md`, fiz a validação final contra o documento original
— tratando o PDF como o árbitro, não o assistente.

**Prompt:**
> "Faz só um novo pente fino, que é validação geral do decisoes vs os prints
> e documentação @user-history/desafio-sdd.pdf"

**Por que funciona:**
Especifiquei o PDF como fonte de referência e pedi comparação explícita.
Isso força o modelo a não avaliar o documento por si mesmo — e sim contra
um critério externo fixo.

**Produziu:**
Identificação da lacuna crítica: a seção "Estrutura de dados escolhida" estava
completamente ausente — era o **primeiro item obrigatório** do PDF.
Sem esse pente fino, o documento teria ido ao avaliador incompleto.

---

## 11. Identificação de code smell — Command com muitos parâmetros

**Contexto do prompt:**
Após a geração do scaffolding, o `TaskUpdateCommand` chegou a 6 parâmetros
posicionais no record constructor. O código compilava e funcionava — o problema
era de manutenibilidade, não funcional.

**Prompt:**
> "O TaskUpdateCommand tem 6 params — em multi equipe é conflito na certa.
> Documenta no decisoes.md a decisão de usar DTO de entrada dentro dos commands
> para facilitar e minificar params. Depois refatora os commands com muitos params
> para seguir esse padrão."

**Por que funciona:**
Identificar o problema pelo impacto operacional concreto ("conflito em multi equipe")
é mais eficaz do que descrever o sintoma técnico ("muitos parâmetros").
A ordem importa: pedir a documentação antes da refatoração garante que a decisão
arquitetural fica registrada — não apenas o código muda.
O prompt deixa a fronteira clara: o que refatorar (commands com muitos params)
e o padrão esperado (DTO separado), sem prescrever nomes ou estrutura interna.

**Produziu:**
Criação do record `TaskUpdateInput` encapsulando o payload da atualização,
simplificação do `TaskUpdateCommand` para `(Guid Id, TaskUpdateInput Data)`,
atualização do validator com `.WithName()` para preservar os nomes de campo
corretos nos erros de validação, e documentação da decisão na seção 15 do `decisoes.md`.

---

## 12. Paginação nos endpoints de listagem — além do mínimo especificado

**Contexto do prompt:**
Durante a revisão dos endpoints GET de listagem, identifiquei que o spec mínimo do PDF
não exige paginação — mas retornar todos os registros sem limite é um anti-pattern real
para qualquer API que vai para produção.

**Prompt:**
> "Os endpoints GET /projetos e GET /projetos/:id/tarefas estão retornando todos os
> registros sem paginação. Para produção isso é um problema real. Adiciona paginação
> com pageNumber (default 1) e pageSize (default 100, máx 100) como query params opcionais.
> A resposta deve envolver os itens em um envelope com metadados: items, pageNumber,
> pageSize, totalItems e totalPages. Documenta no decisoes.md e atualiza o openapi.yaml."

**Por que funciona:**
Apresentar o impacto concreto ("retornar todos os registros sem limite é anti-pattern para produção")
antes de pedir a feature direciona a solução para a decisão arquitetural correta, não apenas
para "adicionar page e limit". Definir o contrato do response no prompt (campos de metadados)
evita que a IA escolha um formato arbitrário que depois exigiria refatoração.
Pedir atualização simultânea do openapi.yaml e do decisoes.md garante que a decisão e
o contrato ficam sincronizados — não apenas o código muda.

**Produziu:**
`PagedResponse<T>` genérico reutilizável, `Math.Clamp` defensivo no handler para evitar
pageSize abusivo sem quebrar a experiência do cliente, schemas `PaginatedProjectResponse`
e `PaginatedTaskResponse` no openapi.yaml com o componente `PageMeta` compartilhado,
e documentação com alternativas rejeitadas (cursor, Link header, X-Total-Count) na seção 16.

---

## 13. Eliminação de duplicação — extensão de paginação em `IQueryable<T>`

**Contexto do prompt:**
Após implementar paginação nos dois repositórios, o bloco `CountAsync + Skip/Take +
ToListAsync` ficou duplicado. Código repetido em repositórios tem o mesmo problema que
código repetido em qualquer camada: a próxima correção precisa ser aplicada em N lugares.

**Prompt:**
> "O bloco CountAsync + Skip/Take + ToListAsync se repete nos dois repositórios.
> Extrai para uma extension method em IQueryable<T> para que qualquer repositório
> futuro use o mesmo código. Atenção: CountAsync e ToListAsync são de EF Core —
> documenta onde ficou e por que não foi no projeto Utils."

**Por que funciona:**
Nomear o problema (duplicação) e o risco concreto (N lugares para corrigir futuramente)
orienta a solução para além de "cria um método". A segunda parte do prompt — "documenta
onde ficou e por que não foi no Utils" — força o modelo a justificar a decisão de
localização, não apenas implementar e seguir. Isso produz rastreabilidade arquitetural.

**Produziu:**
`QueryableExtensions.ToPagedAsync<T>()` em `Infra.Persistence/Extensions/` (internal),
com justificativa documentada: `CountAsync`/`ToListAsync` são métodos de extensão do
EF Core — colocar no Utils adicionaria dependência de EF Core a um projeto candidato a
NuGet independente, violando o propósito da camada. Repositórios reduzidos a uma linha:
`return await query.OrderBy(...).ToPagedAsync(pageNumber, pageSize, ct);`

---

## 14. Testes de integração (contrato) com xUnit + WebApplicationFactory

**Contexto do prompt:**
Com a implementação completa, era necessário criar os testes de contrato exigidos pelo PDF.
O projeto já tinha o `TaskFlow.ContractTests` configurado com os pacotes certos (`xunit`,
`Microsoft.AspNetCore.Mvc.Testing`) mas sem nenhum arquivo de teste. O objetivo era cobrir
os três cenários obrigatórios: criação de recursos (201), regras de negócio (422) e recursos
inexistentes (404) — todas as 5 regras do PDF.

**Prompt:**
> "Podemos seguir com os testes esperados conforme a documentação @user-history/desafio-sdd.pdf"

**Por que funciona:**
O prompt é intencionalmente curto porque o contexto acumulado já estava carregado: o PDF estava
presente, o código da implementação tinha sido lido na mesma sessão, e o padrão de projeto era
conhecido. Pedir "conforme a documentação" força o modelo a derivar os casos de teste das
regras de negócio do PDF — não de achismos sobre o que testar.

**Antes de escrever qualquer teste, confirmei:**
Xunit ou integração? — Confirmei que a abordagem é **testes de integração com xUnit +
WebApplicationFactory**: pipeline ASP.NET Core completo em processo, EF Core + SQLite real
(arquivo temp por suíte), `HttpClient` real. Não é unitário, não usa mock de repositório.
O PDF chama de "testes de contrato" porque valida aderência ao `openapi.yaml`, mas a
implementação técnica é integração ponta a ponta.

**Produziu:**
- `TaskFlowFactory` — `WebApplicationFactory<Program>` com SQLite em arquivo temp único por
  classe de teste (`IClassFixture<T>`), garantindo isolamento entre suítes
- `ContractDtos` + `ApiHelpers` — tipos de resposta e helpers (`CreateProjectAsync`,
  `CreateTaskAsync`, `UpdateTaskAsync`) para eliminar setup duplicado nos testes
- 19 testes cobrindo todas as 5 regras de negócio + criações 201 + recursos inexistentes 404:
  - Regra 1: arquivar projeto com tarefa `in_progress` → 422
  - Regra 2: excluir tarefa `in_progress` ou `done` → 422
  - Regra 3: `completedAt` rejeitado manualmente → 400; preenchido automático ao ir para `done`
  - Regra 4: criar tarefa em projeto arquivado → 422
  - Regra 5: pular `in_progress` (pending → done) → 422; retroceder status → 422

---

## 15. Revisão crítica dos testes contra o PDF

**Contexto do prompt:**
Após os 19 testes passarem (19/19), o questionamento foi: os testes criados realmente
saem da spec do PDF, ou apenas passam por coincidência com a implementação?

**Prompt:**
> "Com base aos testes criados, a saída é com base a @user-history/desafio-sdd.pdf?
> Existe algum teste que quebraria ou não sairia a saída desejada?"

**Por que funciona:**
Separar "testes passando" de "testes corretos" é uma distinção crítica. Um teste pode passar
porque a implementação está errada de um jeito que o teste não detecta. Pedir uma revisão
cruzada com o PDF força a comparação entre o que o teste valida e o que o PDF exige —
não apenas se o test runner reportou verde.

**Análise produzida:**

Nenhum teste quebraria com a implementação atual — os 19 passam e cada um valida exatamente
o cenário para o qual foi escrito. O mapeamento regra a regra confirmou aderência ao PDF:
todas as 5 regras de negócio (422), criação de recursos (201) e recursos inexistentes (404)
estão cobertos — os três grupos obrigatórios do PDF.

**Gaps identificados (fora do "ao menos" do PDF, não exigidos):**
- `GET /projetos`, `GET /projetos/{id}`, `GET /projetos/{id}/tarefas` não testados —
  sem cobertura de listagem e busca por ID
- Resposta não validada contra o schema OpenAPI — o pacote `NJsonSchema` está no csproj
  mas não é usado; os testes verificam comportamento (status code + campos específicos),
  não conformidade de schema completo
- Campos de resposta nos cenários de erro não validados — apenas o status code HTTP é
  verificado nos retornos 422/404, sem inspecionar o corpo `ProblemDetails`

**Decisão:** gaps documentados e aceitos. O PDF exige "ao menos" os três grupos e todos
estão cobertos. A cobertura de GET e validação de schema são melhorias futuras, não
lacunas que comprometem a entrega.
