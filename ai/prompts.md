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

## 8. Pente fino final — validação cruzada com o PDF

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
