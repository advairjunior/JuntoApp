# Projeto Encontros - Arquitetura Inicial

## Objetivo do documento

Este documento define a arquitetura tecnica inicial do Projeto Encontros.

Ele deve orientar as decisoes de estrutura, tecnologias, camadas, responsabilidades, seguranca, dados, API, frontend, infraestrutura e evolucao tecnica do produto.

Este documento nao substitui regras de negocio nem backlog. As regras do produto deverao ser descritas em `03-regras-de-produto.md`, e as funcionalidades detalhadas deverao ser descritas em `04-backlog-inicial.md`.

## Referencias do produto

A arquitetura deve servir a visao definida em `00-visao-produto.md` e suportar a evolucao planejada em `01-roadmap-versoes.md`.

O objetivo tecnico nao e criar a arquitetura mais sofisticada possivel. O objetivo e criar uma base simples, organizada, segura e preparada para evoluir durante muitos anos.

## Principios arquiteturais

A arquitetura do Projeto Encontros deve seguir estes principios:

1. Simplicidade antes de complexidade.
2. Manutencao antes de conveniencia imediata.
3. Escalabilidade progressiva, sem overengineering.
4. Separacao clara de responsabilidades.
5. Baixo acoplamento entre camadas.
6. Alta coesao dentro de cada modulo.
7. Regras de negocio protegidas de detalhes externos.
8. Seguranca e privacidade desde o inicio.
9. Testabilidade como criterio de desenho.
10. Evolucao por versoes, sem implementar futuro antes da hora.

## Stack oficial

### Linguagem

- C#

### Backend

- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- FluentValidation
- JWT
- Refresh Token
- Swagger
- xUnit

### Frontend oficial

- Flutter Web/PWA
- experiencia mobile-first e responsiva
- Safari no iPhone, Chrome no Android e navegadores desktop
- Riverpod para estado e GoRouter para navegacao

### Infraestrutura e operacao

- Docker
- GitHub
- GitHub Actions
- Azure
- Firebase Cloud Messaging
- Cloudflare R2

## Estilo arquitetural

O backend deve seguir Clean Architecture.

A regra principal e que as regras de negocio nao devem depender de banco de dados, framework web, servicos externos, armazenamento de arquivos ou detalhes de infraestrutura.

As dependencias devem apontar para dentro:

```text
Api -> Aplicacao -> Dominio
Infraestrutura -> Aplicacao -> Dominio
```

O dominio deve ser o centro da aplicacao.

## Estrutura inicial da solucao

A estrutura inicial recomendada para o backend e:

```text
src/
  ProjetoEncontros.Api/
  ProjetoEncontros.Aplicacao/
  ProjetoEncontros.Dominio/
  ProjetoEncontros.Infraestrutura/

tests/
  ProjetoEncontros.TestesUnidade/
  ProjetoEncontros.TestesIntegracao/
```

A estrutura oficial do frontend e:

```text
src/
  ProjetoEncontros.AplicativoWeb/
```

O frontend consome exclusivamente os contratos HTTP da API. A separacao entre `Api`, `Aplicacao`, `Dominio`, `Infraestrutura`, `AplicativoWeb` e testes deve ser preservada.

## Responsabilidades das camadas

### Dominio

A camada `Dominio` deve conter o nucleo das regras de negocio.

Responsabilidades:

- entidades
- value objects
- regras de dominio
- eventos de dominio, quando necessarios
- contratos de dominio quando fizerem sentido
- validacoes que fazem parte da regra de negocio

Nao deve conter:

- Entity Framework Core
- ASP.NET Core
- chamadas HTTP
- acesso a arquivos
- envio de notificacoes
- detalhes de banco de dados
- DTOs de API

Exemplos iniciais de conceitos de dominio:

- usuario
- grupo
- membro do grupo
- convite
- encontro
- presenca
- memoria
- item de lista

Nem todos esses conceitos precisam ser implementados na primeira versao. Eles apenas indicam a direcao natural do dominio.

### Aplicacao

A camada `Aplicacao` deve coordenar casos de uso.

Responsabilidades:

- comandos
- consultas
- DTOs de entrada e saida da aplicacao
- validacoes de aplicacao
- interfaces para infraestrutura
- orquestracao de regras de negocio
- controle de permissoes em nivel de caso de uso

Nao deve conter:

- detalhes de banco
- detalhes de framework web
- implementacoes de servicos externos
- regras visuais do aplicativo

Exemplos de casos de uso iniciais:

- cadastrar usuario
- autenticar usuario
- criar grupo
- convidar membro
- aceitar convite
- listar grupos do usuario
- criar encontro
- confirmar presenca

### Infraestrutura

A camada `Infraestrutura` deve implementar detalhes externos.

Responsabilidades:

- Entity Framework Core
- PostgreSQL
- repositorios, quando necessarios
- migrations
- armazenamento de arquivos
- integracoes externas
- servicos de notificacao
- implementacoes de interfaces da camada Aplicacao

Nao deve conter:

- regras de negocio centrais
- decisoes de produto
- regras de interface

### Api

A camada `Api` deve expor o backend por HTTP.

Responsabilidades:

- controllers ou endpoints
- configuracao de autenticacao
- configuracao de injecao de dependencia
- configuracao de Swagger
- middlewares
- filtros
- tratamento padronizado de erros
- versionamento de API quando necessario

Nao deve conter:

- regra de negocio
- acesso direto ao banco
- logica complexa de aplicacao

### AplicativoWeb

O frontend Flutter deve ser organizado por funcionalidade e manter estado, dados, telas, componentes e servicos claramente separados.

Responsabilidades:

- telas
- componentes reutilizaveis
- controladores e estados
- navegacao
- estados de carregamento, erro e vazio
- consumo da API
- armazenamento local minimo quando necessario

O frontend nao deve duplicar regras de negocio que pertencem ao backend.

Regras simples de experiencia, formatacao e validacao imediata podem existir no frontend para melhorar usabilidade, mas a decisao final deve permanecer no backend.

## Organizacao por dominios funcionais

O Projeto Encontros deve crescer por dominios funcionais, nao por telas isoladas.

Dominios iniciais previstos:

- identidade e acesso
- grupos
- membros
- convites
- encontros
- presencas
- memorias
- listas colaborativas
- notificacoes

Cada dominio deve evoluir apenas quando sua versao correspondente for planejada no roadmap.

## Banco de dados

O banco oficial sera PostgreSQL.

O acesso ao banco devera ser feito com Entity Framework Core.

### Diretrizes

- usar migrations controladas
- evitar alterar dados manualmente fora de processos claros
- preservar integridade referencial
- usar indices quando houver necessidade real de consulta
- evitar modelagem generica demais no inicio
- preferir nomes claros para tabelas, colunas e relacionamentos
- proteger dados privados dos grupos

### Modelagem inicial esperada

A modelagem inicial deve priorizar a versao `0.1 - Fundacao do produto`.

Entidades candidatas para a fundacao:

- Usuario
- Grupo
- MembroDoGrupo
- ConviteDoGrupo
- TokenDeAtualizacao

Entidades de encontros, memorias, listas e notificacoes devem ser adicionadas apenas quando suas versoes forem trabalhadas.

## API

A API deve ser clara, previsivel e segura.

### Diretrizes

- usar JSON como formato principal
- retornar erros padronizados
- validar entradas antes de executar casos de uso
- evitar endpoints que exponham dados de outros grupos
- proteger endpoints privados com JWT
- manter contratos simples
- documentar endpoints com Swagger
- evitar criar endpoints futuros antes da necessidade real

### Organizacao inicial sugerida

Endpoints iniciais devem estar alinhados a versao `0.1`.

Areas iniciais:

- autenticacao
- usuarios
- grupos
- convites
- membros

Endpoints de encontros, memorias, listas e notificacoes devem ser adicionados nas versoes correspondentes.

## Autenticacao e autorizacao

A autenticacao deve usar JWT com Refresh Token.

### Diretrizes

- access tokens devem ter vida curta
- refresh tokens devem ser armazenados com seguranca
- refresh tokens devem poder ser revogados
- endpoints privados devem exigir autenticacao
- a autorizacao deve validar pertencimento ao grupo
- um usuario nao pode acessar dados de grupos dos quais nao participa

### Permissoes iniciais

Na versao inicial, as permissoes devem ser simples.

Papeis candidatos:

- dono do grupo
- administrador do grupo
- membro do grupo

Caso a complexidade nao seja necessaria na primeira versao, o produto pode iniciar apenas com dono e membro.

A decisao final devera ser detalhada em `03-regras-de-produto.md`.

## Armazenamento de arquivos

O armazenamento de arquivos devera usar Cloudflare R2 quando o produto chegar as funcionalidades de memorias.

Na versao `0.1`, arquivos nao fazem parte do escopo.

Quando fotos e videos forem implementados, a arquitetura devera considerar:

- upload seguro
- limite de tamanho
- tipo de arquivo permitido
- associacao de arquivos a encontros
- controle de acesso por grupo
- exclusao logica ou fisica conforme regra definida
- custos de armazenamento

## Notificacoes

As notificacoes deverao usar Firebase Cloud Messaging quando a versao correspondente for implementada.

Na arquitetura inicial, notificacoes nao devem ser implementadas antes da necessidade de produto.

Quando forem adicionadas, devem seguir o principio de nao estimular uso excessivo da tela.

Tipos de notificacao permitidos inicialmente:

- convite para grupo
- novo encontro
- lembrete antes do encontro
- alteracao relevante no encontro
- responsabilidade atribuida ao usuario

Notificacoes de engajamento artificial devem ser evitadas.

## Frontend Flutter Web/PWA

O Flutter Web/PWA e o unico frontend oficial e mantido do Projeto Encontros. O projeto esta em `src/ProjetoEncontros.AplicativoWeb` e consome a API ASP.NET Core sem acesso direto ao banco ou a detalhes de infraestrutura.

A experiencia deve priorizar telas pequenas e uso pelo Safari no iPhone, preservando compatibilidade com navegadores Android e desktop. A distribuicao nativa pelas lojas podera ser avaliada futuramente a partir do Flutter, sem fazer parte da v1.0.

### Diretrizes de UX tecnica

- reutilizar componentes
- evitar duplicacao de telas
- manter navegacao simples
- priorizar uso com uma mao
- representar estados de carregamento, vazio e erro
- minimizar quantidade de toques
- evitar interfaces complexas sem necessidade

### Organizacao sugerida

```text
ProjetoEncontros.AplicativoWeb/
  lib/
    compartilhado/
    funcionalidades/
  assets/
  test/
  web/
```

Cada funcionalidade pode conter modelos, dados, estado, telas, componentes e servicos conforme sua necessidade. Componentes compartilhados so devem ser criados quando houver reutilizacao real.

## Testes

Testes devem acompanhar a evolucao do produto conforme risco e importancia da funcionalidade.

### Tipos de teste

- testes unitarios para regras de dominio
- testes unitarios para casos de uso
- testes de integracao para API e banco
- testes de validacao para regras importantes
- testes manuais documentados para fluxos em navegadores e aparelhos reais, quando necessario

### Diretrizes

- regras de negocio importantes devem ter testes
- correcoes de bugs devem incluir teste quando possivel
- fluxos criticos de autenticacao, grupos e convites devem ser testados
- testes nao devem depender de dados externos instaveis

## Seguranca e privacidade

O Projeto Encontros lida com grupos privados e memorias pessoais. Por isso, seguranca e privacidade devem ser consideradas desde o inicio.

### Diretrizes

- proteger endpoints privados
- validar pertencimento ao grupo antes de retornar dados
- evitar exposicao de dados sensiveis em logs
- armazenar senhas com hash seguro
- proteger refresh tokens
- validar arquivos antes de aceitar upload
- limitar acesso a fotos e videos por grupo
- evitar retornos excessivos de dados pessoais

Privacidade nao deve ser tratada como recurso futuro. Ela faz parte da identidade do produto.

## Performance e escalabilidade

A arquitetura deve ser preparada para crescer, mas sem complexidade prematura.

### Diretrizes iniciais

- paginar listagens
- evitar consultas que carreguem dados demais
- usar indices conforme necessidade real
- evitar processamento pesado no frontend
- manter payloads de API pequenos
- separar armazenamento de arquivos do banco relacional
- monitorar pontos criticos antes de otimizar prematuramente

Caching, filas e arquitetura distribuida devem ser avaliados apenas quando houver necessidade real.

## Infraestrutura inicial

### Ambiente local

O ambiente local deve ser simples para desenvolvimento.

Componentes esperados:

- API ASP.NET Core
- PostgreSQL via Docker
- cliente Flutter Web/PWA

### CI/CD

O GitHub Actions devera ser usado para automatizar verificacoes.

Pipeline inicial recomendado:

- restaurar dependencias
- compilar solucao
- executar testes
- validar qualidade basica do codigo

Publicacao automatica deve ser planejada apenas quando houver ambiente e processo definidos.

### Azure

Azure sera a plataforma prevista para hospedagem da API e servicos relacionados.

A decisao exata dos recursos Azure devera ser feita quando a arquitetura de deploy for detalhada.

## Padroes de codigo

O codigo devera seguir:

- Clean Code
- SOLID
- Dependency Injection
- Repository Pattern apenas quando necessario
- validacoes explicitas
- separacao de responsabilidades

Diretrizes especificas:

- escrever codigo proprio em portugues
- usar nomes em portugues para namespaces, classes, metodos, propriedades, campos, enums e contratos proprios da API
- aceitar ingles apenas quando for exigencia de framework, biblioteca, arquivo convencional ou sigla tecnica estabelecida
- nao usar `var`
- usar `new()` sempre que o tipo ja estiver explicito e isso nao prejudicar a clareza
- preferir construtores primarios quando possivel, sem transformar parametros em propriedades publicas desnecessarias
- sempre usar chaves em blocos de controle
- evitar classes grandes
- evitar metodos longos
- preferir nomes claros
- explicar decisoes importantes quando houver trade-off

## Repository Pattern

Repository Pattern deve ser usado quando trouxer valor real para separar regras de aplicacao dos detalhes de persistencia.

Nao deve ser aplicado automaticamente em todos os casos apenas por habito.

Quando Entity Framework Core ja atender bem a necessidade dentro da camada Infraestrutura, o uso de repositorios deve ser avaliado com cuidado para evitar abstracao inutil.

Recomendacao inicial:

- usar interfaces na camada Aplicacao quando o caso de uso precisar de persistencia
- implementar essas interfaces na camada Infraestrutura
- manter consultas simples e explicitas
- evitar repositorios genericos sem valor de dominio

## Decisoes adiadas

As decisoes abaixo nao precisam ser fechadas neste momento:

- estrategia final de deploy em Azure
- modelo completo de permissoes avancadas
- limites finais de armazenamento de fotos e videos
- politica de retencao de arquivos
- estrategia de monetizacao
- integracao com calendario
- estatisticas do grupo
- retrospectiva anual

Essas decisoes devem ser tomadas apenas quando suas versoes forem planejadas.

## Riscos arquiteturais

Os principais riscos tecnicos iniciais sao:

- criar arquitetura complexa demais antes do produto validar valor
- misturar regra de negocio com controllers
- duplicar regra de negocio no frontend
- expor dados privados entre grupos
- criar modelagem generica demais
- implementar funcionalidades futuras antes da hora
- tratar arquivos de midia sem estrategia de custo e seguranca
- criar notificacoes que prejudiquem a filosofia do produto

## Criterios de qualidade arquitetural

Uma decisao tecnica deve ser aceita quando:

- respeita a visao do produto
- suporta a versao atual do roadmap
- nao implementa futuro desnecessario
- melhora manutencao ou clareza
- preserva seguranca e privacidade
- pode ser testada
- nao aumenta complexidade sem justificativa
- mantem separacao entre dominio, aplicacao, infraestrutura, API e mobile

## Relacao com outros documentos

Este documento depende de `00-visao-produto.md` e `01-roadmap-versoes.md`.

As regras de negocio deverao ser descritas em `03-regras-de-produto.md`.

As funcionalidades detalhadas deverao ser descritas em `04-backlog-inicial.md`.

Se a arquitetura inicial for alterada, o backlog podera precisar de revisao para refletir impacto em APIs, banco de dados, telas, testes e prioridades.
