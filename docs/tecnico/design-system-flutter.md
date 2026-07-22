# Projeto Encontros - Design System Flutter

## Objetivo do documento

Este documento define a fundacao visual do aplicativo Flutter Web/PWA do Junto.

Ele orienta a refatoracao incremental das telas existentes sem alterar regras de negocio, contratos da API ou rotas.

## Direcao visual

O aplicativo deve transmitir:

- amizade e pertencimento
- encontros reais
- memorias afetivas
- noite e luz baixa
- simplicidade e sofisticacao

O visual oficial e escuro, acolhedor, mobile first e inspirado na clareza das interfaces do iOS.

O produto nao deve parecer um dashboard administrativo, sistema desktop, rede social publica ou aplicativo de mensagens generico.

## Diagnostico da base anterior

Os principais problemas encontrados foram:

- apenas cores e espacamentos estavam parcialmente centralizados
- havia 103 ocorrencias locais de decoracao, raio, botao ou scaffold
- tipografia possuia poucos niveis e excesso de peso visual
- superfícies tinham contraste hierarquico baixo
- raio de borda e preenchimento variavam entre telas equivalentes
- estados de vazio, erro e carregamento eram implementados separadamente
- o `NavigationBar` usava a aparencia padrao do Material
- o conteudo desktop nao possuia uma moldura visual propria do aplicativo
- componentes de dominio e componentes transversais ainda nao tinham um limite documentado

## Tokens oficiais

### Cores

As cores ficam centralizadas em `cores_do_aplicativo.dart`.

Grupos semanticos:

- fundos: externo, principal e secundario
- superficies: cartao, elevada e suave
- identidade: verde de destaque, verde principal e verde escuro
- apoio: ambar, coral e perigo
- textos: principal, secundario e terciario
- estrutura: borda suave, borda discreta e sobreposicao escura

Cores locais so devem existir quando representarem conteudo especifico e nao recorrente.

### Espacamentos

- minimo: `4`
- pequeno: `8`
- medio: `12`
- padrao: `16`
- grande: `24`
- extra grande: `32`

### Raios

- pequeno: `10`
- medio: `16`
- grande: `22`
- extra grande: `28`
- pilula: `999`

### Tipografia

- titulo grande: `28 / 700`
- titulo medio: `22 / 700`
- titulo de secao: `20 / 700`
- titulo de cartao: `18 / 700`
- corpo: `15 / 400`
- corpo secundario: `14 / 400`
- legenda: `12 / 500`
- acao: `15 / 600`

O texto nao deve depender apenas de peso ou cor para comunicar hierarquia.

### Profundidade

Sombras devem ser discretas e usadas somente para separar elementos realmente elevados, como dock e superficies modais.

Cards comuns usam principalmente diferenca de superficie e borda discreta.

## Componentes compartilhados

Componentes transversais iniciais:

- `EstruturaResponsivaDoAplicativo`
- `ConteudoResponsivo`
- `CartaoDoAplicativo`
- `CabecalhoDaPagina`
- `TituloDeSecao`
- `EstadoVazio`
- `IndicadorDeSituacao`
- `FotoDePerfil`
- `ImagemPrivada`

Um componente deve entrar em `compartilhado` quando for usado em pelo menos tres lugares ou quando representar uma necessidade transversal clara, como imagem privada ou estado vazio.

Cards de encontro, publicacoes, participantes e memorias continuam dentro de suas funcionalidades.

Componentes visuais nao acessam repositorios, Dio ou controladores de sessao. Eles recebem dados e callbacks.

## Shell responsivo

### Mobile

- ocupa toda a largura disponivel
- respeita areas seguras
- mantem dock fixo com alvos adequados para toque
- reserva espaco para o dock sem cobrir o fim das listas

### Desktop

- usa fundo externo escuro
- centraliza o aplicativo
- limita a area principal a `520 px`
- preserva a aparencia de aplicativo, nao de dashboard
- aplica borda e sombra discretas na moldura principal

A largura de `520 px` segue o briefing oficial do produto. Uma navegacao lateral podera ser reavaliada se o produto ganhar fluxos que realmente precisem de uma superficie desktop ampla.

## Navegacao principal

O dock possui apenas destinos funcionais:

1. Inicio
2. Memorias
3. Perfil

As telas internas de encontro permanecem fora do dock e usam navegacao contextual.

O dock possui:

- superficie elevada translucida
- desfoque sutil
- contorno discreto
- cantos de `28 px`
- icone e rotulo sempre visiveis
- indicador animado pelo proprio `NavigationBar`
- cor verde para o destino ativo

Transicoes que mantenham duas arvores do `go_router` simultaneamente nao devem ser usadas, pois podem duplicar `GlobalKey`.

## Arquivos da fundacao

### Tema

- `lib/compartilhado/tema/cores_do_aplicativo.dart`
- `lib/compartilhado/tema/espacamentos_do_aplicativo.dart`
- `lib/compartilhado/tema/raios_do_aplicativo.dart`
- `lib/compartilhado/tema/tipografia_do_aplicativo.dart`
- `lib/compartilhado/tema/sombras_do_aplicativo.dart`
- `lib/compartilhado/tema/tema_do_aplicativo.dart`

### Componentes e shell

- `lib/compartilhado/componentes/estrutura_responsiva_do_aplicativo.dart`
- `lib/compartilhado/componentes/conteudo_responsivo.dart`
- `lib/compartilhado/componentes/cartao_do_aplicativo.dart`
- `lib/compartilhado/componentes/cabecalho_da_pagina.dart`
- `lib/compartilhado/componentes/titulo_de_secao.dart`
- `lib/compartilhado/componentes/estado_vazio.dart`
- `lib/compartilhado/componentes/indicador_de_situacao.dart`
- `lib/compartilhado/navegacao/estrutura_com_navegacao.dart`

## Etapas de aplicacao

### Etapa 1 - Fundacao

- [x] paleta semantica
- [x] tipografia
- [x] raios
- [x] espacamentos
- [x] sombras
- [x] temas globais de controles
- [x] componentes transversais essenciais

### Etapa 2 - Shell

- [x] moldura responsiva
- [x] dock com tres destinos reais
- [x] area segura
- [x] largura desktop controlada
- [x] navegacao e rotas preservadas

### Etapa 3 - Entrada, inicio e perfil

- [x] refinar entrada e cadastro
- [x] aplicar cabecalho e secoes na home
- [x] refinar cartoes de encontros
- [x] transformar perfil em conta e configuracoes

### Etapa 4 - Experiencia do encontro

- [x] refinar feed e compositor
- [x] refinar informacoes
- [x] refinar participantes
- [x] refinar galeria

### Etapa 5 - Memorias

- [x] implementar linha do tempo visual
- [x] criar cards editoriais
- [x] integrar galerias privadas existentes

### Etapa 6 - Notificacoes e combinados

- [x] aplicar componentes do design system
- [x] preservar eventos automaticos discretos no feed

## Criterios de qualidade

- nao haver overflow horizontal em `320 px`
- manter alvos interativos com pelo menos `44 px`
- preservar foco visivel e tooltips em botoes de icone
- diferenciar carregamento, vazio e erro
- nao depender apenas de cor para estado
- manter imagens privadas autenticadas
- preservar login, logout, deep links e rotas
- executar `flutter analyze`, `flutter test` e `flutter build web` apos cada etapa
- validar teclado, barras dinamicas e areas seguras no Safari real do iPhone antes da publicacao

## Dependencias

As dependencias atuais sao suficientes.

Nao adicionar bibliotecas de responsividade, outro roteador, outro gerenciador de estado, bibliotecas visuais completas ou pacotes de animacao sem necessidade comprovada.
