# Skills Delegadas à IA

> Registro das áreas de conhecimento e tarefas delegadas ao assistente de IA
> (Claude Code — Claude Sonnet 4.6) durante o desenvolvimento do TaskFlow.

---

## Leitura e interpretação do desafio

Deleguei à IA a leitura e extração das regras do PDF do desafio, identificando
endpoints, entidades, campos, restrições de negócio e requisitos de entrega.
Usei o resultado como base de verificação, não como verdade absoluta — revisei
manualmente cada ponto extraído.

## Geração do contrato OpenAPI

Deleguei a geração do rascunho inicial do `openapi.yaml` com base nas regras
do PDF. A IA produziu a estrutura base com schemas, paths e responses.
O conteúdo foi revisado criticamente — várias correções foram necessárias
(documentadas em `ai/revisoes.md`).

## Sugestões de arquitetura e estrutura do projeto

Solicitei sugestões de estrutura de projeto para .NET seguindo Clean Architecture
+ DDD + CQRS. A IA apresentou opções (incluindo .NET Aspire) que foram avaliadas
e filtradas com base no contexto do desafio.

## Criação do docs/decisoes.md

Deleguei a estrutura e escrita inicial do documento de decisões arquiteturais.
A IA organizou as seções e justificativas. O conteúdo foi revisado e corrigido
em múltiplas rodadas — incluindo a identificação de seção crítica faltante
("Estrutura de dados") e a correção do banco de testes.

## Comandos CLI e setup do projeto

Deleguei os comandos `dotnet new`, `dotnet sln add` e configuração inicial do
projeto. Validei os comandos antes de executar e corrigi o path de criação
da solução para garantir que o `.sln` ficasse na raiz do repositório.

## Conhecimento técnico sobre o ecossistema .NET

Consultei a IA sobre MediatR, `IPipelineBehavior`, dual DbContext, SQLite com
Docker, configuração de ambiente com `appsettings` e `.env`. Em cada caso,
questionei se a solução era padrão Microsoft ou biblioteca de terceiros antes
de aceitar.

## Revisão crítica do openapi.yaml

Solicitei revisão individual das 5 restrições de negócio contra o contrato gerado.
A IA identificou lacunas que eu confirmei e algumas que eu contestei com base
na leitura direta do PDF.
