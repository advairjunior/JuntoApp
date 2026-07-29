# Projeto Encontros - Decisoes Tecnicas

## Objetivo do documento

Este documento registra decisoes tecnicas importantes do Projeto Encontros.

Ele deve ser usado para preservar contexto, justificar escolhas e evitar que decisoes relevantes se percam durante a evolucao do produto.

Cada decisao deve explicar o contexto, a escolha feita, a justificativa, as consequencias e os documentos impactados.

## Status das decisoes

Possiveis status:

- Proposta
- Aprovada
- Substituida
- Cancelada

## DT-001 - Implementar a v0.1 como backend-only

**Status:** Aprovada

**Data:** 2026-07-01

### Contexto

O Projeto Encontros foi inicialmente planejado com backend em ASP.NET Core e aplicativo mobile em .NET MAUI.

Durante a avaliacao do ambiente mobile, foi identificado que o desenvolvimento com .NET MAUI exigiria instalacao e configuracao adicional de workloads, Android SDK, emulador Android e dependencias relacionadas.

Tambem foi identificado que o publico inicial do produto usa majoritariamente iPhone.

Como a versao `0.1 - Fundacao` depende principalmente de autenticacao, grupos, convites e membros, o mobile nao e necessario para validar a arquitetura e as regras centrais da primeira entrega.

### Decisao

A versao `0.1` sera implementada primeiro como **backend-only**.

O desenvolvimento mobile sera adiado ate que o backend da fundacao esteja funcional, testado e documentado.

O cliente mobile prioritario futuro sera **iOS**.

A decisao de tecnologia mobile foi registrada posteriormente em `DT-005 - Usar .NET MAUI com foco inicial em iOS`.

### Justificativa

Implementar o backend primeiro reduz complexidade inicial e evita configurar ferramentas mobile antes de haver uma API estavel.

Como o publico inicial usa iPhone, investir cedo em Android ou em um fluxo mobile multiplataforma pode gerar trabalho que nao entrega valor imediato.

Backend-only na `v0.1` permite validar:

- arquitetura
- banco de dados
- autenticacao
- seguranca
- regras de grupo
- convites
- permissoes
- testes automatizados

Depois disso, o app iOS podera consumir uma API mais madura.

### Consequencias positivas

- Menor complexidade inicial.
- Menos configuracao de ambiente antes da hora.
- Backend mais estavel antes do mobile.
- Menor risco de retrabalho nas telas.
- Foco maior em seguranca, privacidade e regras de grupo.
- Decisao mobile podera ser tomada com mais informacao.

### Consequencias negativas

- Usuarios nao terao interface mobile na primeira etapa tecnica.
- Fluxos de UX mobile serao validados depois.
- Algumas decisoes de API podem precisar de ajuste quando o app iOS for desenhado.

### Impacto no escopo

A versao `0.1` continua com o mesmo escopo funcional:

- cadastro
- login
- renovacao de sessao
- logout
- grupos
- convites
- membros

A diferenca e que a primeira implementacao sera feita sem app mobile.

### Documentos impactados

Devem ser revisados ou atualizados:

- `v0.1-plano-implementacao.md`

Podem precisar de revisao futura:

- `02-arquitetura-inicial.md`
- `v0.1-fundacao.md`

### Criterio de revisao futura

Esta decisao devera ser revisada quando:

- o backend da `v0.1` estiver pronto
- a API principal estiver testada
- for iniciar o desenvolvimento do cliente iOS
- houver necessidade real de suportar Android

## DT-002 - Escrever codigo e nomes internos em portugues

**Status:** Aprovada

**Data:** 2026-07-01

### Contexto

O Projeto Encontros sera desenvolvido por uma equipe que trabalha em portugues e deseja preservar clareza de dominio no proprio codigo.

Como o produto tem regras de convivencia, grupos, convites, encontros e memorias, os nomes do dominio ficam mais claros quando usam a linguagem natural do produto.

### Decisao

Todo codigo proprio do projeto deve usar portugues em:

- namespaces
- nomes de projetos internos
- pastas de dominio
- classes
- metodos
- propriedades
- campos
- enums
- parametros
- contratos de API definidos pelo projeto

Excecoes sao permitidas apenas para nomes exigidos por frameworks, bibliotecas, arquivos convencionais ou termos tecnicos amplamente estabelecidos, como `Program.cs`, `appsettings.json`, `Guid`, `Task`, `CancellationToken`, `JWT`, `HTTP`, `API` e `DTO`.

Padroes de escrita C# aprovados:

- nao usar `var`
- usar `new()` sempre que o tipo ja estiver explicito e isso nao prejudicar a clareza
- preferir construtores primarios quando possivel
- nao transformar parametros de construtores primarios em propriedades publicas sem necessidade
- sempre usar chaves em blocos de controle

### Justificativa

A linguagem do codigo deve refletir a linguagem do produto.

Isso melhora leitura, reduz traducao mental entre documentacao e implementacao, e ajuda a manter o dominio compreensivel durante muitos anos.

### Consequencias positivas

- Mais alinhamento entre produto, documentacao e codigo.
- Menor ambiguidade nas regras de negocio.
- Facilidade para novos membros entenderem o dominio.
- Contratos internos mais proximos da linguagem dos usuarios e da equipe.

### Consequencias negativas

- Alguns termos tecnicos do ecossistema .NET continuarao em ingles por obrigatoriedade.
- Bibliotecas externas podem misturar nomes em ingles com codigo proprio em portugues.
- Exemplos externos precisarao ser adaptados antes de entrar no projeto.

### Documentos impactados

- `../produto/02-arquitetura-inicial.md`
- `v0.1-plano-implementacao.md`
- `v0.1-decisoes-implementacao.md`
- `../../AGENTS.md`

### Criterio de revisao futura

Esta decisao so deve ser revista se a equipe mudar oficialmente o idioma principal de desenvolvimento do projeto.

## DT-003 - Tratar WhatsApp como canal futuro de compartilhamento de convite

**Status:** Aprovada

**Data:** 2026-07-01

### Contexto

O convite por e-mail foi definido como formato oficial da `v0.1` porque permite uma fundacao simples, rastreavel e vinculada a identidade do usuario.

Durante a revisao de prontidao para o cliente iOS, foi levantada a possibilidade de enviar convite pelo WhatsApp, ja que esse canal reduz atrito para grupos de amigos e pessoas que ja combinam encontros por ele.

### Decisao

O WhatsApp nao sera tratado como integracao obrigatoria da `v0.1`.

Na evolucao futura, ele podera ser usado como canal de compartilhamento de convite, desde que o aceite continue protegido por regras do backend.

Na `v0.1`, o convite permanece vinculado a e-mail.

### Justificativa

O WhatsApp tem valor real para a experiencia do usuario, mas uma implementacao apressada pode transformar o convite em link encaminhavel sem controle.

O Projeto Encontros nao e aplicativo de mensagens e nao deve substituir WhatsApp, Discord, Telegram ou Instagram. Ele deve complementar esses canais sem abrir mao da privacidade dos grupos.

### Consequencias positivas

- Mantem a `v0.1` simples e segura.
- Evita crescimento de escopo antes da base estar fechada.
- Preserva o controle de entrada em grupos privados.
- Permite desenhar uma experiencia futura mais natural para iOS.

### Consequencias negativas

- O convite por e-mail pode ter mais atrito no primeiro uso.
- O compartilhamento pelo WhatsApp ainda precisara de desenho de produto e regras tecnicas.
- O app iOS pode precisar de uma melhoria logo apos a fundacao para reduzir esse atrito.

### Documentos impactados

- `v0.1-prontidao-ios.md`
- `v0.1-plano-implementacao.md`
- `../produto/04-backlog-inicial.md`

### Criterio de revisao futura

Esta decisao devera ser revisada quando:

- o cliente iOS for iniciado
- houver desenho do fluxo de convite no app
- for proposta uma versao com convite compartilhavel
- forem definidas regras de expiracao, revogacao e uso do convite compartilhado

## DT-004 - Retornar acesso negado para recursos privados inexistentes ou inacessiveis

**Status:** Aprovada

**Data:** 2026-07-01

### Contexto

O Projeto Encontros lida com grupos privados, convites e membros.

Quando um usuario tenta acessar um grupo, convite ou membro que nao pertence a ele, retornar detalhes diferentes para "nao existe" e "existe, mas voce nao pode acessar" pode vazar informacao sobre recursos privados.

### Decisao

Na `v0.1`, recursos privados inexistentes para o usuario atual ou inacessiveis por permissao devem ser tratados como acesso negado.

A API pode retornar `403 Forbidden` nesses casos.

### Justificativa

Essa escolha protege a privacidade dos grupos e evita confirmacao indireta de existencia de recursos privados.

Para o cliente iOS, a mensagem deve ser simples: a acao nao esta disponivel ou o recurso nao pode ser acessado pelo usuario atual.

### Consequencias positivas

- Reduz vazamento de informacao.
- Simplifica a regra de seguranca da fundacao.
- Mantem autorizacao por grupo como responsabilidade do backend.

### Consequencias negativas

- O app iOS nao diferencia todos os casos de recurso inexistente e falta de permissao.
- Algumas mensagens de erro podem ser menos especificas para o usuario.

### Documentos impactados

- `v0.1-prontidao-ios.md`
- `v0.1-plano-implementacao.md`

### Criterio de revisao futura

Esta decisao podera ser revista se houver necessidade de UX mais especifica sem comprometer privacidade.

## DT-005 - Usar .NET MAUI com foco inicial em iOS

**Status:** Aprovada

**Data:** 2026-07-02

### Contexto

O publico inicial do Projeto Encontros usa majoritariamente iPhone, mas existe interesse em manter uma possibilidade real de Android no futuro.

A equipe ja esta construindo o backend em C# e .NET. Escolher SwiftUI nativo facilitaria uma experiencia iOS pura, mas exigiria Swift, Xcode e uma segunda implementacao futura se Android se tornar necessario.

.NET MAUI permite desenvolver aplicativos para iOS e Android com C# e uma base compartilhada, mas ainda exige acesso a um Mac com Xcode para compilar e assinar builds iOS.

### Decisao

O primeiro cliente mobile sera construido com **.NET MAUI com foco inicial em iOS**.

A estrategia sera:

- priorizar experiencia, testes e validacao no iPhone
- manter Android como possibilidade futura
- evitar tentar polir iOS e Android ao mesmo tempo no primeiro ciclo
- usar MVVM simples
- reutilizar componentes e servicos de consumo da API
- validar Android periodicamente apenas para evitar acoplamento excessivo ao iOS

SwiftUI nativo fica como alternativa futura apenas se o spike inicial em .NET MAUI revelar bloqueio serio de produtividade, build, publicacao ou experiencia.

### Justificativa

Essa decisao reduz a quantidade de tecnologias novas no projeto e aproveita o conhecimento em C#/.NET ja usado no backend.

Tambem preserva uma rota mais barata para Android no futuro, sem obrigar o projeto a lancar duas plataformas desde o inicio.

Para o usuario, o foco inicial continua correto: entregar primeiro uma boa experiencia no iPhone, onde esta o publico inicial.

### Consequencias positivas

- Menor troca de contexto entre backend e mobile.
- Possibilidade de reaproveitar conhecimento, padroes e bibliotecas .NET.
- Caminho mais simples para Android futuro do que SwiftUI puro.
- Uma base mobile compartilhada pode reduzir retrabalho.
- Permite validar iOS primeiro sem abandonar multiplataforma.

### Consequencias negativas

- Build iOS continua exigindo Mac com Xcode.
- Alguns ajustes visuais e comportamentais poderao ser especificos por plataforma.
- A experiencia iOS pode exigir cuidado extra para parecer natural.
- Android nao deve ser prometido como entregue no primeiro ciclo mobile.

### Plano de validacao

Antes de implementar o app completo, deve ser feito um spike curto de cliente mobile.

O spike deve validar:

- criacao de projeto .NET MAUI
- build iOS
- consumo real da API
- tela simples de login ou listagem inicial
- armazenamento seguro de tokens
- navegacao basica
- estados de carregamento, erro e vazio
- verificacao Android simples, sem polimento de produto

### Referencias externas

- `https://learn.microsoft.com/en-us/dotnet/maui/what-is-maui`
- `https://learn.microsoft.com/en-us/dotnet/maui/supported-platforms`
- `https://learn.microsoft.com/en-us/dotnet/maui/ios/pair-to-mac`
- `https://developer.apple.com/xcode/`

### Documentos impactados

- `v0.1-prontidao-ios.md`
- `v0.1-plano-implementacao.md`
- `../produto/02-arquitetura-inicial.md`
- `../versoes/v0.1-fundacao.md`

### Criterio de revisao futura

Esta decisao devera ser revista se:

- o spike em .NET MAUI falhar por bloqueio tecnico relevante
- o produto decidir abandonar Android por prazo longo
- a qualidade da experiencia iOS ficar abaixo do aceitavel
- houver necessidade estrategica de um app iOS totalmente nativo

## DT-006 - Normalizar datas de encontros em UTC no PostgreSQL

**Status:** Aprovada

**Data:** 2026-07-07

### Contexto

A API de encontros recebe `DateTimeOffset` com offset de fuso horario, por exemplo `2027-07-18T16:00:00-03:00`.

Durante os testes de integracao da `v0.2`, a criacao de encontro retornou erro interno ao persistir uma data com offset diferente de UTC em coluna PostgreSQL `timestamp with time zone`.

O PostgreSQL armazena esse tipo como instante no tempo, e o provedor Npgsql trabalha de forma mais segura com valores normalizados em UTC.

### Decisao

As datas persistidas em encontros e presencas devem ser normalizadas para UTC no mapeamento EF Core antes da gravacao no PostgreSQL.

Os contratos da API continuam aceitando `DateTimeOffset` com offset, e a comparacao de proximos encontros continua baseada no instante em UTC.

### Justificativa

Normalizar datas no limite da persistencia evita erro de gravacao, preserva comparacoes consistentes e mantem a regra tecnica de ordenar e filtrar proximos encontros por instante real.

Essa decisao tambem evita espalhar conversoes manuais pelos casos de uso, mantendo a regra concentrada na infraestrutura.

### Consequencias positivas

- Criacao de encontros funciona com horarios enviados pelo aplicativo usando offset local.
- Consultas por proximos encontros ficam consistentes em UTC.
- A regra fica centralizada nos mapeamentos EF Core.
- Evita erro interno ao salvar `DateTimeOffset` com offset diferente de zero no PostgreSQL.

### Consequencias negativas

- O offset original enviado pelo cliente nao e preservado como informacao propria.
- Se no futuro o produto precisar exibir o fuso original escolhido pelo usuario, sera necessario adicionar um campo especifico de fuso horario ou preferencia regional.

### Documentos impactados

- `v0.2-plano-implementacao.md`

### Criterio de revisao futura

Esta decisao deve ser revista se o produto passar a exigir exibicao fiel do fuso horario original escolhido no momento da criacao do encontro.

## DT-007 - Tratar Encontro como agregado principal do produto

**Status:** Aprovada

**Data:** 2026-07-08

### Contexto

Durante a evolucao da `v0.2`, foi identificado que o modelo centralizado em grupos nao representa bem cenarios reais do produto.

Um mesmo circulo de pessoas pode combinar encontros diferentes com participantes diferentes. Em alguns casos, o encontro precisa ser privado ate mesmo em relacao a outras pessoas que participam do mesmo grupo social mais amplo.

Exemplo: um encontro em apartamento com vagas limitadas nao deve ser exibido para todos os amigos de um grupo recorrente.

### Decisao

O `Encontro` passa a ser o agregado principal do produto.

Convites, participantes, presencas, mural, midias, memorias e listas devem pertencer diretamente ao encontro.

O conceito de grupo, turma ou circulo podera existir no futuro apenas como apoio para convite recorrente, mas nao deve ser obrigatorio para criar encontro nem conceder acesso automatico aos encontros.

Tecnicamente, a arquitetura deve evoluir para usar `ParticipanteDoEncontro` como entidade responsavel por concentrar:

- vinculo entre usuario e encontro
- papel no encontro
- situacao do convite
- resposta de presenca
- base de autorizacao para visualizar detalhes, mural, midias e memorias

### Justificativa

Essa decisao preserva a privacidade real dos encontros e torna o fluxo principal mais simples para o usuario.

O usuario quer criar um compromisso real, convidar pessoas especificas e acompanhar presencas. Exigir grupo antes disso cria atrito e pode expor encontros para pessoas que nao deveriam saber deles.

### Consequencias positivas

- O fluxo principal fica mais direto: criar encontro, convidar participantes, confirmar presenca e guardar memorias.
- A privacidade fica alinhada ao encontro, que e onde o risco real acontece.
- O aplicativo deixa de depender de grupos obrigatorios.
- O app mobile pode exibir a tela inicial por encontros do usuario.
- Grupos, turmas ou pessoas frequentes podem ser adicionados depois sem alterar a identidade central.

### Consequencias negativas

- A implementacao atual baseada em grupos precisara de transicao.
- Rotas antigas de grupos precisarao ser mantidas temporariamente ou descontinuadas com cuidado.
- A migracao de dados devera criar participantes de encontro a partir dos dados existentes.
- Testes de autorizacao precisarao ser reforcados para evitar vazamento entre encontros.

### Documentos impactados

- `../produto/01-roadmap-versoes.md`
- `../produto/03-regras-de-produto.md`
- `../versoes/v0.2.3-encontro-como-centro.md`
- `v0.2.3-plano-transicao-encontro-como-centro.md`

Podem precisar de revisao:

- `../produto/02-arquitetura-inicial.md`
- `../produto/04-backlog-inicial.md`

### Criterio de revisao futura

Esta decisao devera ser revista apenas se for comprovado, com uso real, que encontros sem grupo obrigatorio tornam a experiencia menos clara ou criam problemas de organizacao maiores do que resolvem.

## DT-008 - Usar armazenamento local em desenvolvimento para imagens de perfil e encontro

**Status:** Aprovada

**Data:** 2026-07-10

### Contexto

A experiencia mobile passou a exigir fotos de perfil e imagem de capa do encontro.

O produto ainda nao possui storage definitivo em nuvem configurado, mas precisa validar a experiencia visual e o fluxo de upload no ambiente local.

### Decisao

Durante o desenvolvimento, imagens enviadas pela API serao salvas em `wwwroot/arquivos`.

As fotos de perfil usam `/arquivos/perfis`.

As imagens de capa do encontro usam `/arquivos/encontros`.

Os contratos devem armazenar e retornar URLs relativas, que o cliente mobile normaliza para URL absoluta.

### Justificativa

Essa decisao permite validar rapidamente upload, remocao, exibicao no mobile e fallback visual sem bloquear a evolucao da UX por uma decisao de infraestrutura definitiva.

O armazenamento fica encapsulado em interfaces da aplicacao, permitindo trocar a implementacao por Cloudflare R2, Azure Blob Storage ou outro provedor sem alterar as regras de dominio.

### Consequencias positivas

- Fluxo visual pode ser testado imediatamente.
- Evita dependencia externa para desenvolvimento local.
- Mantem storage isolado por interface.
- Permite evoluir para storage definitivo depois.

### Consequencias negativas

- Arquivos locais nao sao adequados para producao distribuida.
- Deploys podem perder arquivos se o diretorio nao for persistente.
- Sera necessario migrar para storage externo antes de uso real em producao.

### Documentos impactados

- `../versoes/v0.2.6-experiencia-do-encontro.md`
- `../produto/02-arquitetura-inicial.md`

### Criterio de revisao futura

Esta decisao deve ser revista antes da publicacao em producao ou quando a funcionalidade de midias/memorias exigir armazenamento duravel.

## DT-009 - Adotar Flutter Web/PWA como novo frontend principal

**Status:** Aprovada

**Data:** 2026-07-14

### Contexto

O frontend atual em .NET MAUI permitiu validar os fluxos do produto, mas a evolucao visual passou a exigir esforco desproporcional para atingir a experiencia moderna, sofisticada e fluida desejada para o Junto.

A publicacao inicial exclusiva nas lojas tambem criaria uma barreira financeira e operacional. O produto precisa ser acessivel por link no Safari do iPhone, em navegadores Android e no computador, sem depender da App Store.

### Decisao

Um novo frontend sera criado em Flutter, com Flutter Web/PWA como alvo inicial principal.

O projeto Flutter ficara separado em `src/ProjetoEncontros.AplicativoWeb` e sera organizado por funcionalidade. Ele consumira a API ASP.NET Core existente e nao acessara diretamente o banco ou a infraestrutura.

O backend, PostgreSQL, Docker, regras de dominio, autenticacao JWT, endpoints e testes existentes serao preservados.

O projeto .NET MAUI nao sera removido. Ele ficara congelado como referencia funcional e visual durante a migracao, sem receber novas funcionalidades salvo decisao explicita.

O frontend Flutter devera:

- priorizar Flutter Web e PWA.
- funcionar no Safari do iPhone, Chrome no Android e navegadores desktop.
- manter experiencia mobile-first e responsiva.
- usar interface dark premium alinhada a identidade do produto.
- manter regras de negocio e autorizacao no backend.
- preservar privacidade por encontro.
- usar nomes de codigo em portugues, exceto convencoes obrigatorias do ecossistema Flutter.

Em producao, frontend e API deverao preferencialmente ser publicados sob a mesma origem, com a API acessivel por `/api`. Em desenvolvimento, o backend devera liberar apenas origens locais explicitamente configuradas.

A autenticacao web nao devera persistir refresh token em `localStorage`. A estrategia preferencial sera manter o access token em memoria e o refresh token em cookie `HttpOnly`, `Secure` e com politica `SameSite` adequada, preservando o fluxo Bearer nos endpoints funcionais.

### Justificativa

Flutter permite reconstruir a experiencia com maior controle visual e manter um caminho futuro para aplicativos nativos, enquanto Flutter Web/PWA reduz a barreira inicial de distribuicao.

A separacao preserva os investimentos realizados no backend e evita duplicacao de regras de negocio. A migracao incremental reduz risco e permite validar cada jornada antes de considerar o MAUI descontinuado.

### Consequencias positivas

- Acesso ao produto por link, sem instalacao obrigatoria.
- Compatibilidade inicial com iPhone, Android e computador.
- Maior liberdade para construir a identidade visual desejada.
- Reaproveitamento integral da API e das regras de negocio.
- Possibilidade futura de gerar aplicativos Flutter para as lojas.
- Frontend organizado por funcionalidade e independente do MAUI.

### Consequencias negativas

- Reescrita completa da camada de apresentacao.
- Convivencia temporaria entre dois frontends.
- Necessidade de tratar CORS, URLs web, recarregamento e historico do navegador.
- Necessidade de uma estrategia de autenticacao especifica para navegador.
- Limitacoes de PWA e notificacoes no Safari precisam ser comunicadas e testadas.
- Bundle inicial, cache e upload de midias exigem validacao em aparelhos reais.

### Riscos obrigatorios antes da producao

- Midias privadas nao poderao permanecer publicamente acessiveis apenas por URL estatica.
- Cache da PWA nao podera armazenar respostas autenticadas ou arquivos privados.
- Refresh token nao podera ficar acessivel ao codigo JavaScript.
- Troca de usuario, logout, expiracao e renovacao de sessao deverao ser testados no navegador.
- A experiencia devera ser validada no Safari real do iPhone, inclusive teclado, areas seguras, upload e instalacao.

### Documentos impactados

- `../produto/00-visao-produto.md`
- `../produto/01-roadmap-versoes.md`
- `../produto/02-arquitetura-inicial.md`
- `v0.7-plano-implementacao.md`
- `spike-mobile-ios-maui.md`

### Criterio de revisao futura

Esta decisao devera ser revista apenas se o spike Flutter Web demonstrar impedimento relevante no Safari do iPhone, falha de seguranca sem mitigacao viavel ou experiencia significativamente inferior nos fluxos essenciais.

O MAUI so podera ser removido depois que as jornadas criticas estiverem implementadas e validadas no Flutter Web/PWA.

## DT-010 - Entregar imagens por rotas privadas e manter leitura hibrida durante a migracao

**Status:** Aceita

**Data:** 2026-07-14, revisada em 2026-07-21

### Contexto

Capas e midias eram gravadas em `wwwroot` e servidas por middleware estatico. A autorizacao protegia o upload e a listagem, mas qualquer pessoa que conhecesse a URL conseguia baixar o arquivo sem JWT e sem participar do encontro.

### Decisao

- Capas e midias de memorias devem ser gravadas fora de `wwwroot`.
- Toda leitura deve usar uma rota autenticada que valide o acesso ao recurso.
- A API deve retornar referencias para essas rotas protegidas, nunca caminhos fisicos.
- O Flutter Web deve baixar os bytes pelo cliente HTTP autenticado e renderizar a imagem em memoria.
- As respostas devem usar `Cache-Control: private, no-store` e `X-Content-Type-Options: nosniff`.
- O conteudo real deve ser compativel com JPEG, PNG ou WEBP, alem do tipo declarado.
- Durante a migracao, a infraestrutura pode ler arquivos legados em `wwwroot`, mas as rotas estaticas de encontros e memorias permanecem bloqueadas.
- Fotos de perfil nao podem ser expostas por middleware estatico. O proprio usuario sempre pode ler sua foto; outro usuario somente pode le-la enquanto ambos participarem de ao menos um mesmo encontro.
- Em `Production`, novos arquivos devem ser gravados no R2. A leitura e a exclusao devem encaminhar referencias locais ao armazenamento local e referencias `/arquivos/r2/` ao R2, sem tentativa sequencial entre provedores.
- A leitura no R2 deve conferir finalidade, usuario responsavel, recurso e encontro registrados no inventario antes de abrir o objeto.

### Justificativa

A privacidade e definida pela relacao entre pessoas e encontros. Portanto, autenticacao generica ou imprevisibilidade do nome do arquivo nao substituem a verificacao de que o usuario ainda pode acessar o encontro ou a foto solicitada.

Rotas autenticadas preservam revogacao imediata, evitam janela de acesso de URLs assinadas e permitem manter armazenamento local no desenvolvimento sem acoplar o frontend ao caminho fisico.

### Consequencias positivas

- Usuario externo ou removido nao consegue reutilizar uma URL conhecida.
- O mesmo modelo de autorizacao protege capa, memoria, imagem de destaque e foto de perfil.
- O armazenamento pode migrar para Azure Blob privado sem alterar a experiencia do produto.
- Arquivos antigos podem ser migrados gradualmente.
- Referencias locais continuam legiveis durante a migracao para o R2 sem expor caminhos fisicos ao Flutter.

### Consequencias negativas

- `Image.network` nao pode ser usado para midias privadas com Bearer.
- O Flutter precisa manter componente de imagem autenticada e cache apenas em memoria.
- O cliente MAUI legado nao renderizara as novas referencias sem adaptacao; ele permanece congelado durante a migracao oficial para Flutter.
- Arquivos legados exigem limpeza operacional posterior.

### Documentos impactados

- `02-arquitetura-inicial.md`
- `v0.7-plano-implementacao.md`
- `../versoes/v0.3-memorias-dos-encontros.md`

### Criterio de revisao futura

Revisar quando houver mais de uma instancia da API ou necessidade de CDN. Nesse momento, avaliar Azure Blob privado mantendo a autorizacao por encontro e, apenas se necessario, URLs assinadas de curtissima duracao.

## DT-011 - Descontinuar o MAUI e manter Flutter Web/PWA como frontend unico

**Status:** Aceita

**Data:** 2026-07-16

### Contexto

A migracao iniciada pela DT-009 atingiu paridade nas jornadas mantidas pelo cliente MAUI. A auditoria final identificou quatro comportamentos exclusivos no legado: edicao do nome do perfil, marcacao de encontro realizado, resposta rapida a convites pendentes e captura explicita pela camera.

Esses comportamentos foram implementados no Flutter e protegidos por analise estatica, testes automatizados e compilacao Web de producao. Manter dois frontends aumentaria o custo de manutencao, confundiria a arquitetura oficial e permitiria divergencia de comportamento antes da publicacao.

### Decisao

- Flutter Web/PWA em `src/ProjetoEncontros.AplicativoWeb` passa a ser o unico frontend oficial e mantido.
- O projeto `src/ProjetoEncontros.Aplicativo` e sua referencia na solution sao removidos.
- A publicacao inicial permanece orientada a navegador e PWA.
- Uma distribuicao futura para App Store ou Google Play devera partir do Flutter e exigir decisao propria.
- O workload MAUI instalado na maquina nao sera removido, pois pode pertencer a outros projetos.
- A validacao em Safari de iPhone real permanece obrigatoria como gate de publicacao da v1.0, mas nao exige a permanencia do codigo legado.

Esta decisao substitui a tecnologia definida pela DT-005 e conclui a convivencia temporaria prevista pela DT-009. As decisoes anteriores permanecem no documento como historico.

### Justificativa

Um unico frontend reduz duplicacao, simplifica testes e publicacao e concentra os ajustes de experiencia no produto que sera realmente entregue. O Flutter preserva acesso por link no iPhone, Android e computador e ainda permite avaliar clientes nativos no futuro.

### Consequencias positivas

- Uma unica base de interface para evoluir e validar.
- Solution .NET dedicada ao backend e aos testes.
- Menor risco de implementar uma correcao em apenas um cliente.
- Documentacao, agentes e processo de publicacao alinhados ao produto real.

### Consequencias negativas

- O MAUI deixa de existir como referencia executavel.
- Comparacoes historicas dependem do controle de versao e da documentacao.
- Camera, instalacao PWA e comportamento do Safari ainda precisam de validacao em aparelhos reais antes da publicacao.

### Evidencias da remocao

- edicao do nome do perfil implementada no Flutter;
- realizacao de encontro implementada no Flutter;
- convites pendentes com resposta rapida implementados na pagina inicial;
- camera e galeria disponiveis para perfil, capa e publicacoes;
- analise estatica Flutter sem apontamentos;
- 38 testes Flutter aprovados;
- build Flutter Web de producao aprovado.

### Documentos impactados

- `../produto/02-arquitetura-inicial.md`
- `v1.0-plano-implementacao.md`
- `.codex/agents/mobile-specialist.toml`

### Criterio de revisao futura

Revisar somente se uma limitacao comprovada da PWA impedir uma jornada essencial ou quando houver decisao de distribuir aplicativos Flutter pelas lojas.

## DT-012 - Usar inventario persistente e upload intermediado pela API para o R2

**Status:** Aceita

**Data:** 2026-07-20

### Contexto

O piloto precisa armazenar fotos no Cloudflare R2 sem ultrapassar a cota interna de 8 GiB. Calcular o consumo listando o bucket antes de cada envio nao protege requisicoes concorrentes, e o envio direto pelo navegador exigiria reservas, confirmacao e conciliacao antes de oferecer uma vantagem relevante para arquivos pequenos.

### Decisao

- `Development` e `Homologacao` continuam usando armazenamento local e nao recebem credenciais R2.
- Somente `Production` usa bucket R2 privado.
- O PostgreSQL sera a autoridade do inventario, dos bytes ativos, das reservas e das exclusoes pendentes.
- A reserva de espaco sera atomica e impedira que requisicoes concorrentes ultrapassem `8.589.934.592` bytes.
- No piloto, o upload continuara passando pela API para preservar validacao de tamanho, tipo e assinatura da imagem.
- Chaves R2 serao imutaveis, imprevisiveis e nunca usarao o nome original do arquivo.
- A remocao visivel e a exclusao fisica serao idempotentes; a cota somente sera liberada depois da confirmacao da exclusao.
- URLs temporarias de leitura poderao ser adicionadas depois que inventario, cota e autorizacao estiverem validados.

### Justificativa

O banco resolve concorrencia sem adicionar Redis, broker ou Worker. O upload pela API e mais simples e permite validar os arquivos antes de gerar custo no R2. Essa escolha prioriza previsibilidade financeira, privacidade e recuperacao de falhas.

### Consequencias positivas

- Bloqueio confiavel antes do limite de 8 GiB.
- Exclusoes e reservas podem ser retentadas e conciliadas.
- Homologacao permanece gratuita e independente do R2.
- O Flutter nao recebe credenciais nem precisa mudar o fluxo imediatamente.

### Consequencias negativas

- A API consome banda durante uploads e leituras enquanto URLs temporarias nao forem adotadas.
- O inventario e as rotinas de conciliacao exigem novas tabelas e migracoes.
- Testes reais do R2 exigirao uma validacao controlada somente depois da aprovacao para criar o bucket.

### Documentos impactados

- `v1.0-guia-publicacao.md`
- `v1.0-plano-implementacao.md`

### Criterio de revisao futura

Revisar quando os arquivos do piloto ultrapassarem os limites atuais, houver mais de uma instancia da API ou a banda intermediada se tornar um custo relevante.

## Template para novas decisoes

```md
## DT-000 - Titulo da decisao

**Status:** Proposta

**Data:** AAAA-MM-DD

### Contexto

Descreva o problema, restricao ou oportunidade.

### Decisao

Descreva a escolha feita.

### Justificativa

Explique por que essa escolha foi feita.

### Consequencias positivas

- Item

### Consequencias negativas

- Item

### Documentos impactados

- Documento

### Criterio de revisao futura

Explique quando a decisao deve ser revista.
```
