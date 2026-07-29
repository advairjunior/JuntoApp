# Projeto Encontros - Roadmap de Versoes

## Objetivo do documento

Este documento define a evolucao planejada do Projeto Encontros por versoes.

O roadmap existe para garantir que o produto evolua de forma natural, entregando valor real ao usuario em cada etapa, sem pular fases e sem implementar funcionalidades futuras antes da hora.

Este documento nao substitui o backlog detalhado. As funcionalidades com objetivo, regras de negocio, criterios de aceite, prioridade e versao de implementacao deverao ser detalhadas em `04-backlog-inicial.md`.

## Referencia principal

Todas as versoes descritas neste documento devem respeitar a visao definida em `00-visao-produto.md`.

O Projeto Encontros deve preservar sua identidade:

> O aplicativo onde organizamos encontros privados e guardamos nossas melhores lembrancas.

Nenhuma versao deve transformar o produto em rede social, aplicativo de mensagens ou plataforma de consumo excessivo de conteudo.

## Principios do roadmap

O roadmap deve seguir estes principios:

1. Cada versao deve entregar valor claro ao usuario.
2. Cada versao deve fortalecer pelo menos um pilar do produto.
3. Nenhuma versao deve depender de funcionalidades futuras para fazer sentido.
4. Funcionalidades devem ser adicionadas apenas quando estiverem alinhadas a organizacao, encontros, memorias, amizade, simplicidade ou experiencia do usuario.
5. A primeira versao deve ser simples, utilizavel e coerente com a identidade do produto.
6. Funcionalidades sociais, publicas ou com risco de desviar a essencia do produto devem ser avaliadas com rigor.
7. O produto deve evoluir com consistencia, evitando overengineering e complexidade prematura.

## Estrategia de evolucao

O Projeto Encontros deve nascer pequeno, privado e util.

A evolucao recomendada e:

1. Primeiro, permitir que pessoas criem encontros privados e convidem participantes.
2. Depois, permitir que cada encontro organize presencas e detalhes essenciais.
3. Em seguida, permitir que os participantes preservem memorias desses encontros.
4. Depois, melhorar a organizacao dos encontros com listas, responsabilidades e historico.
5. Entao, adicionar apoio para pessoas frequentes, turmas ou circulos sem tornar isso obrigatorio.
6. Por fim, amadurecer o produto com estatisticas, notificacoes inteligentes e melhorias de longo prazo.

Essa estrategia evita que o produto comece grande demais e protege sua simplicidade.

## Versao 0.1 - Fundacao do produto

### Objetivo

Criar a base minima para que o Projeto Encontros exista como produto privado.

Esta versao deve validar a estrutura essencial: usuarios, autenticacao, acesso seguro e primeira base de convivencia.

### Valor para o usuario

O usuario consegue criar sua conta, autenticar-se e reconhecer que existe um espaco privado para organizar encontros reais.

### Funcionalidades principais

- cadastro de usuario
- login
- autenticacao segura
- estrutura inicial de acesso privado
- base tecnica para convites e participantes
- preparacao para encontros privados

### Pilares fortalecidos

- organizacao
- amizade
- simplicidade
- experiencia do usuario

### Fora do escopo desta versao

- fotos e videos
- linha do tempo
- listas de compras
- estatisticas
- setlists
- notificacoes avancadas
- funcionalidades especificas por tipo de encontro

### Criterio de sucesso

Um usuario deve conseguir acessar o produto de forma segura e entender que ele sera usado para criar encontros privados e guardar memorias.

## Versao 0.2 - Organizacao de encontros

### Objetivo

Permitir que usuarios criem encontros reais com informacoes claras, convites controlados e confirmacao de presenca.

Esta e a primeira versao em que o produto entrega diretamente sua proposta central de organizar encontros.

### Valor para o usuario

O usuario consegue marcar um encontro, convidar as pessoas certas, saber quando e onde ele acontecera e acompanhar quem pretende participar.

### Funcionalidades principais

- criacao de encontro
- edicao de encontro
- cancelamento de encontro
- convite para encontro
- data e horario do encontro
- local do encontro
- descricao simples
- confirmacao de presenca
- lista de participantes confirmados
- visualizacao dos proximos encontros do usuario

### Pilares fortalecidos

- encontros
- organizacao
- amizade
- simplicidade
- experiencia do usuario

### Fora do escopo desta versao

- chat do encontro
- comentarios longos
- feed publico
- album de fotos
- listas de compras
- notificacoes complexas

### Criterio de sucesso

Um usuario deve conseguir organizar um encontro real com poucos passos e saber quem participara, sem precisar criar um grupo antes.

## Versao 0.3 - Memorias dos encontros

### Objetivo

Permitir que os participantes preservem registros dos encontros realizados.

Esta versao inicia o pilar de memorias, conectando fotos, videos e lembrancas a momentos especificos do encontro.

### Valor para o usuario

Os participantes conseguem voltar a um encontro passado e encontrar lembrancas daquele momento em um espaco organizado.

### Funcionalidades principais

- encerramento ou marcacao de encontro como realizado
- album do encontro
- envio de fotos
- envio de videos
- descricao ou legenda simples de memoria
- visualizacao de memorias por encontro
- historico basico de encontros passados

### Pilares fortalecidos

- memorias
- amizade
- encontros
- experiencia do usuario

### Fora do escopo desta versao

- feed publico
- curtidas publicas
- seguidores
- comentarios com comportamento de rede social
- edicao avancada de midia
- estatisticas de engajamento

### Criterio de sucesso

Um usuario deve conseguir encontrar um encontro passado e reviver suas principais lembrancas sem depender de conversas antigas ou arquivos espalhados.

## Versao 0.4 - Organizacao colaborativa

### Objetivo

Melhorar a preparacao dos encontros com listas, responsabilidades e combinados dos participantes.

Esta versao ajuda os participantes a transformar planejamento em execucao.

### Valor para o usuario

Os participantes conseguem organizar o que precisa ser levado, quem sera responsavel por cada item e quais combinados fazem parte do encontro.

### Funcionalidades principais

- lista de itens do encontro
- responsavel por item
- marcacao de item como resolvido
- observacoes simples por item
- lista de combinados
- visualizacao das pendencias do encontro

### Pilares fortalecidos

- organizacao
- encontros
- simplicidade
- experiencia do usuario

### Fora do escopo desta versao

- pagamentos
- divisao financeira automatica
- marketplace
- compras dentro do aplicativo
- automacoes complexas de tarefas

### Criterio de sucesso

Os participantes devem conseguir preparar um encontro com menos confusao, reduzindo esquecimentos e combinados perdidos.

## Versao 0.5 - Linha do tempo dos encontros

### Objetivo

Criar uma linha do tempo privada com os encontros e memorias mais importantes do usuario.

Esta versao transforma registros isolados em historia pessoal e compartilhada dos encontros vividos.

### Valor para o usuario

O usuario consegue visualizar sua trajetoria de encontros ao longo do tempo e revisitar momentos importantes com facilidade.

### Funcionalidades principais

- linha do tempo de encontros do usuario
- agrupamento por encontros
- destaque de memorias importantes
- filtros simples por periodo
- visualizacao de encontros passados
- acesso rapido aos albums dos encontros

### Pilares fortalecidos

- memorias
- amizade
- organizacao
- experiencia do usuario

### Fora do escopo desta versao

- feed publico
- exploracao de encontros externos
- compartilhamento aberto
- algoritmos de recomendacao social

### Criterio de sucesso

O usuario deve conseguir navegar pela propria historia de encontros de forma simples, privada e emocionalmente relevante.

## Versao 0.6 - Notificacoes essenciais

### Objetivo

Adicionar notificacoes que ajudem o usuario a lembrar de encontros e responsabilidades sem estimular uso excessivo do aplicativo.

### Valor para o usuario

O usuario recebe lembretes uteis sobre encontros e pendencias importantes, sem excesso de interrupcoes.

### Funcionalidades principais

- notificacao de convite para encontro
- notificacao de novo encontro
- lembrete antes do encontro
- notificacao de alteracao relevante no encontro
- lembrete de item sob responsabilidade do usuario
- configuracoes simples de notificacao

### Pilares fortalecidos

- organizacao
- encontros
- simplicidade
- experiencia do usuario

### Fora do escopo desta versao

- notificacoes de engajamento artificial
- notificacoes para gerar retorno compulsivo
- campanhas promocionais
- notificacoes baseadas em curtidas ou popularidade

### Criterio de sucesso

As notificacoes devem ajudar o usuario a participar melhor dos encontros, sem transformar o aplicativo em uma fonte constante de distracao.

## Versao 0.7 - Pessoas frequentes e tipos de encontro

Status: **concluida e oficialmente fechada em `2026-07-16`**.

### Objetivo

Adicionar recursos opcionais para pessoas frequentes e tipos especificos de encontro, sem prejudicar a simplicidade do fluxo principal.

O documento detalhado da versao e `../versoes/v0.7-pessoas-frequentes-e-tipos-de-encontro.md`.

### Valor para o usuario

Usuarios com encontros recorrentes conseguem reutilizar pessoas e informacoes sem que todos sejam obrigados a criar grupos fixos.

### Funcionalidades entregues

- pessoas frequentes para convite rapido
- tipos opcionais de encontro

### Funcionalidades para reavaliacao futura

- modelos simples de encontro
- setlists para encontros musicais
- musicas tocadas em encontros
- desafios para encontros de jogos, estudos ou esportes
- registros simples de atividades
- campos opcionais por tipo de encontro

### Pilares fortalecidos

- organizacao
- encontros
- memorias
- simplicidade

### Fora do escopo desta versao

- transformar o produto em ferramenta profissional complexa
- criar grupos obrigatorios para todos os usuarios
- transformar pessoas frequentes em participantes automaticos
- adicionar telas especificas sem necessidade validada

### Criterio de sucesso

O criterio foi atendido: pessoas frequentes e tipo opcional aumentaram o valor para encontros recorrentes sem criar grupos obrigatorios, participacao automatica ou perda de privacidade por encontro.

## Versao 1.0 - Produto publico inicial

### Objetivo

Consolidar uma primeira versao publica, estavel e coerente do Projeto Encontros.

Esta versao deve reunir as capacidades essenciais para lancamento: encontros privados, convites, participantes, confirmacao de presenca, memorias, organizacao colaborativa, linha do tempo e notificacoes essenciais.

### Valor para o usuario

O usuario consegue usar o Projeto Encontros como ferramenta principal para organizar momentos reais com outras pessoas e preservar sua historia.

### Funcionalidades principais

- experiencia completa e revisada das versoes anteriores
- ajustes de usabilidade
- melhorias de performance
- revisao de seguranca
- revisao de privacidade
- refinamento visual
- documentacao de suporte basica
- preparacao para publicacao nas lojas

### Pilares fortalecidos

- organizacao
- encontros
- memorias
- amizade
- simplicidade
- experiencia do usuario

### Fora do escopo desta versao

- rede social aberta
- mensagens diretas
- comunidades publicas
- monetizacao avancada
- recursos complexos ainda nao validados

### Criterio de sucesso

O produto deve estar pronto para ser usado por pessoas reais, com experiencia simples, privada, estavel e alinhada a identidade definida em `00-visao-produto.md`.

## Versao 1.0.1 - Localizacao fixa do encontro

Status: **em implementacao**.

### Objetivo

Manter o destino combinado como informacao fixa e privada do encontro.

### Valor para o usuario

Os convidados abrem o ponto correto no mapa sem procurar uma localizacao perdida em mensagens.

### Funcionalidades principais

- local textual preservado
- captura pontual da posicao atual pelo organizador
- abertura do ponto no mapa pelos participantes autorizados
- ausencia de rastreamento em tempo real

### Criterio de sucesso

Um participante autorizado deve encontrar e abrir o destino atual do encontro em poucos toques, mesmo depois de novas publicacoes.

O detalhamento esta em `../versoes/v1.0.1-localizacao-fixa-do-encontro.md`.

## Versao 1.1 - Repertorio do Encontro

Status: **planejada para depois da v1.0**.

### Objetivo

Organizar musicas, participantes e ordem de apresentacao em encontros musicais sem transformar o produto em uma ferramenta profissional complexa.

O detalhamento inicial desta direcao esta em `05-proposta-modulos-futuros.md`.

### Valor para o usuario

Grupos deixam de espalhar nomes de musicas, tons, links, responsaveis e ordem de execucao em mensagens desconectadas.

### Funcionalidades principais

- repertorio privado por encontro
- musicas com nome e detalhes opcionais
- participantes que cantam ou tocam
- setlist ordenado
- marcacao de musica tocada ou pulada
- resumo do setlist na memoria do encontro
- sugestao contextual para encontros musicais, sem ativacao automatica

### Pilares fortalecidos

- organizacao
- encontros
- memorias
- simplicidade

### Fora do escopo desta versao

- biblioteca musical global
- letras ou cifras copiadas de terceiros
- transposicao automatica
- integracao direta com YouTube ou sites de cifras
- estatisticas de desempenho musical
- telas obrigatorias para encontros sem musica

### Criterio de sucesso

Um grupo musical deve preparar e registrar seu repertorio com menos trabalho, enquanto encontros que nao usam musica permanecem simples e inalterados.

## Versao 1.2 - Karaoke do Encontro

Status: **planejada e dependente dos aprendizados da v1.1**.

### Objetivo

Apoiar a organizacao do karaoke durante o encontro de forma divertida, voluntaria e inclusiva.

### Valor para o usuario

O grupo consegue organizar pessoas, musicas e ordem de apresentacao sem sorteios improvisados, repeticoes confusas ou controles paralelos.

### Funcionalidades principais

- sessao de karaoke vinculada ao encontro
- inscricao voluntaria de participantes
- cadastro simples de musicas
- fila e sorteio justo
- troca de musica
- pulo sem penalidade
- duplas com consentimento
- resumo privado na memoria do encontro

### Pilares fortalecidos

- organizacao
- encontros
- memorias
- amizade

### Fora do escopo desta versao

- notas de desempenho
- ranking de cantores
- vencedor do karaoke
- reproducao de videos no aplicativo
- inscricao automatica de participantes
- publicacao de recusas, trocas ou pulos no feed

### Criterio de sucesso

O grupo deve conduzir o karaoke sem controlar a ordem manualmente, sem constranger participantes e sem transformar a atividade em competicao.

## Versao 1.3 - Destaques do Encontro

Status: **planejada como experiencia controlada e condicionada a pesquisa com usuarios**.

### Objetivo

Criar um fechamento afetivo depois do encontro por meio de reconhecimentos positivos, secretos e opcionais.

### Valor para o usuario

Participantes conseguem registrar quem tornou aquele momento especial, preservando esse reconhecimento como parte da memoria do encontro.

### Funcionalidades principais

- catalogo pequeno de categorias positivas
- votacao secreta e opcional
- um voto por participante e categoria
- resultado somente depois do encerramento
- empate compartilhado
- resultado privado na memoria do encontro

### Pilares fortalecidos

- amizade
- memorias
- encontros

### Fora do escopo desta versao

- categorias negativas
- categorias personalizadas sem moderacao
- autovoto
- contagem publica de votos
- ranking, podio ou comparacao entre pessoas
- compartilhamento publico
- notificacoes de popularidade

### Criterio de sucesso

Participantes devem perceber a experiencia como reconhecimento e afeto, nunca como concurso, exclusao ou avaliacao social.

### Condicao para implementacao

A versao so deve ser implementada depois de pesquisa confirmar que as categorias, o anonimato e a apresentacao dos resultados nao geram constrangimento ou competicao.

## Versao 1.4 - Marcos do Perfil

Status: **planejada e condicionada aos resultados da v1.3**.

### Objetivo

Representar acontecimentos importantes da historia do usuario sem transformar o perfil em placar ou sistema de produtividade social.

### Valor para o usuario

O perfil ganha personalidade e ajuda a pessoa a revisitar sua trajetoria de encontros, memorias e contribuicoes reais.

### Funcionalidades principais

- poucos Marcos automaticos e verificaveis
- tela privada `Meus Marcos`
- escolha de ate tres Marcos destacados
- privacidade contextual para visualizacao por outras pessoas
- Marcos originados de Destaques somente quando seguros

### Pilares fortalecidos

- memorias
- amizade
- experiencia do usuario

### Fora do escopo desta versao

- pontos e niveis
- sequencias de presenca
- metas por quantidade
- ranking entre usuarios
- raridade global
- barras de progresso
- perfil publico
- notificacoes para estimular desbloqueios

### Criterio de sucesso

O perfil deve ganhar historia e personalidade sem revelar encontros privados, incentivar spam ou comparar pessoas.

### Condicao para implementacao

A versao so deve prosseguir se a experiencia de Destaques nao apresentar sinais de exclusao, competicao ou uso compulsivo.

## Funcionalidades futuras para avaliacao

As ideias abaixo podem fazer sentido no futuro, mas nao devem ser implementadas antes de validacao e priorizacao em versoes posteriores:

- estatisticas dos encontros
- retrospectiva anual
- exportacao de memorias
- backup avancado
- convites por link com controles adicionais
- modelos de encontros
- integracao com calendario
- busca avancada em memorias
- permissoes avancadas por participante
- recursos para eventos maiores

Essas funcionalidades devem ser avaliadas individualmente em `04-backlog-inicial.md` ou em documentos futuros de backlog.

## Riscos de produto

O roadmap deve evitar os seguintes riscos:

- transformar o produto em rede social
- competir diretamente com aplicativos de mensagem
- adicionar funcionalidades demais antes da base estar validada
- criar fluxos longos para tarefas simples
- priorizar recursos tecnicos sem valor claro ao usuario
- adicionar recursos especificos que prejudiquem encontros simples
- estimular uso excessivo da tela

## Relacao com outros documentos

Este documento depende da visao definida em `00-visao-produto.md`.

As decisoes de arquitetura necessarias para suportar este roadmap deverao ser descritas em `02-arquitetura-inicial.md`.

As regras de negocio gerais do produto deverao ser descritas em `03-regras-de-produto.md`.

O detalhamento das funcionalidades, prioridades e criterios de aceite devera ser descrito em `04-backlog-inicial.md`.

Se o roadmap for alterado, os documentos `03-regras-de-produto.md` e `04-backlog-inicial.md` poderao precisar de revisao para manter consistencia.
