# Projeto Encontros - Spike Mobile iOS com .NET MAUI

## Objetivo do documento

Este documento define o spike tecnico para validar o primeiro cliente mobile do Projeto Encontros com .NET MAUI, foco inicial em iOS e Android preservado como possibilidade futura.

O spike nao e a implementacao completa do aplicativo.

## Referencias oficiais

Este spike segue:

- `../produto/00-visao-produto.md`
- `../produto/02-arquitetura-inicial.md`
- `../produto/03-regras-de-produto.md`
- `../versoes/v0.1-fundacao.md`
- `v0.1-prontidao-ios.md`
- `decisoes-tecnicas.md`

Decisao tecnica principal:

- `DT-005 - Usar .NET MAUI com foco inicial em iOS`

## Objetivo do spike

Validar se .NET MAUI e viavel para construir o primeiro cliente iOS do Projeto Encontros com simplicidade, boa experiencia e caminho futuro para Android.

O spike deve responder:

- conseguimos criar e executar um app .NET MAUI?
- conseguimos preparar build iOS?
- conseguimos consumir a API real da `v0.1`?
- conseguimos guardar tokens com seguranca?
- conseguimos montar navegacao simples?
- conseguimos criar estados de carregamento, erro e vazio?
- conseguimos manter a base preparada para Android sem atrasar iOS?
- conseguimos registrar claramente os problemas encontrados e a decisao de continuidade?

## Resultado esperado

Ao final do spike, a equipe deve decidir uma das opcoes:

- seguir com .NET MAUI para o app mobile
- ajustar a arquitetura mobile antes de continuar
- pausar MAUI e reavaliar SwiftUI nativo, caso haja bloqueio serio

## Escopo do spike

O spike deve implementar apenas um fluxo minimo.

Fluxo recomendado:

1. abrir app
2. verificar se existe sessao local
3. fazer login
4. armazenar tokens com seguranca
5. chamar `GET /api/usuarios/eu`
6. chamar `GET /api/grupos`
7. exibir lista de grupos ou estado vazio
8. permitir sair da conta removendo tokens locais e chamando logout

Fluxo alternativo aceitavel:

1. abrir app
2. fazer login
3. listar convites pendentes com `GET /api/convites`
4. exibir lista ou estado vazio

## Fora do escopo

Nao faz parte do spike:

- publicar na App Store
- publicar no Google Play
- implementar todas as telas da `v0.1`
- implementar cadastro completo com polimento final
- implementar criacao de grupo definitiva
- implementar convite por WhatsApp
- implementar notificacoes push
- implementar encontros
- implementar memorias
- polir Android
- criar design system completo
- resolver identidade visual final do produto

## Pre-requisitos de ambiente

### Windows

Necessario:

- .NET SDK usado pelo projeto
- Visual Studio com suporte a .NET MAUI ou ferramenta equivalente
- workload .NET MAUI instalado
- acesso ao backend local
- acesso ao PostgreSQL local via Docker
- URL local da API configuravel por ambiente

Situacao atual observada:

- workload MAUI instalado em 2026-07-02
- Android SDK nao esta configurado neste ambiente
- Android permanece desabilitado no build comum e pode ser habilitado com `HabilitarAndroid=true`

### iOS

Necessario:

- Mac acessivel para build iOS
- Xcode instalado no Mac
- simulador iOS configurado
- acesso SSH ou pareamento equivalente entre Windows e Mac, se o desenvolvimento ocorrer no Windows
- conta Apple Developer apenas quando for necessario instalar em dispositivo fisico ou distribuir build

Observacao:

- build iOS com .NET MAUI exige ferramentas Apple em um Mac

### Android futuro

Para preservar Android como possibilidade futura, o spike deve evitar dependencias desnecessariamente exclusivas de iOS.

Android nao precisa ser polido no spike.

## Estrutura inicial proposta

Projeto futuro sugerido:

```text
src/
  ProjetoEncontros.Aplicativo/
```

Pastas conceituais:

```text
ProjetoEncontros.Aplicativo/
  Configuracao/
  Telas/
  Modelos/
  Estados/
  Servicos/
  Componentes/
  Navegacao/
  Recursos/
```

Nomes proprios do projeto devem permanecer em portugues.

Excecoes aceitaveis:

- arquivos exigidos pelo MAUI
- nomes exigidos por bibliotecas
- siglas tecnicas
- tipos padrao do .NET

## Diretrizes de implementacao mobile

- usar C#
- usar .NET MAUI
- usar MVVM simples
- escrever codigo proprio em portugues
- nao usar `var`
- usar `new()` quando o tipo ja estiver explicito
- preferir construtores primarios quando fizer sentido
- sempre usar chaves em blocos de controle
- manter telas pequenas
- reutilizar componentes
- nao duplicar regra de negocio do backend
- tratar backend como fonte final de regra e permissao
- priorizar uso com uma mao

## Contratos da API usados no spike

### Login

```text
POST /api/autenticacao/login
```

Uso:

- obter access token
- obter refresh token
- obter data de expiracao do access token

### Usuario atual

```text
GET /api/usuarios/eu
```

Uso:

- validar sessao
- obter nome e e-mail para tela inicial

### Grupos

```text
GET /api/grupos
GET /api/grupos/{identificadorDoGrupo}
GET /api/grupos/{identificadorDoGrupo}/membros
DELETE /api/grupos/{identificadorDoGrupo}/membros/{identificadorDoMembro}
```

Uso:

- exibir grupos do usuario autenticado
- validar estado vazio
- abrir detalhe basico do grupo
- exibir membros do grupo
- permitir remocao basica de membro comum pelo dono

### Convites

```text
POST /api/grupos/{identificadorDoGrupo}/convites
GET /api/convites
POST /api/convites/{identificadorDoConvite}/aceitar
POST /api/convites/{identificadorDoConvite}/recusar
```

Uso:

- permitir que o dono convide uma pessoa por e-mail
- validar fluxo de entrada em grupo por convite pendente
- validar lista vazia de convites
- permitir aceite ou recusa de convite pelo app

### Logout

```text
POST /api/autenticacao/sair
```

Uso:

- revogar refresh token no backend
- limpar tokens locais

## Armazenamento seguro

O spike deve validar armazenamento seguro para:

- access token
- refresh token
- data de expiracao do access token

Regras:

- token nao deve ser salvo em arquivo comum
- token nao deve aparecer em log
- logout deve remover tokens locais
- falha de renovacao deve limpar sessao e voltar para login

## Estados obrigatorios

Cada tela do spike deve prever:

- carregando
- vazio
- erro
- conteudo

Mensagens devem ser simples e humanas.

O app nao deve expor detalhes tecnicos como stack trace, excecao ou nome de classe.

## UX minima do spike

### Tela de entrada

Objetivo:

- permitir login simples.

Campos:

- e-mail
- senha

Acoes:

- entrar

Estados:

- carregando
- credenciais invalidas
- erro inesperado

### Tela inicial

Objetivo:

- mostrar que o usuario entrou no Projeto Encontros.

Conteudo:

- nome do usuario
- lista de grupos
- indicacao de convites pendentes, se adotado no fluxo

Estados:

- carregando grupos
- lista vazia
- erro de sessao
- erro de conexao

### Saida da conta

Objetivo:

- validar encerramento seguro da sessao.

Comportamento:

- chamar backend para logout
- limpar tokens locais
- voltar para tela de entrada

## Criterios de aceite

O spike sera considerado aprovado quando:

- projeto .NET MAUI for criado com nome interno em portugues
- app compilar no ambiente local configurado
- caminho de build iOS estiver validado ou bloqueio estiver documentado
- login consumir a API real
- tokens forem armazenados de forma segura
- app chamar `GET /api/usuarios/eu`
- app chamar pelo menos um endpoint de lista, preferencialmente `GET /api/grupos`
- estado vazio for exibido corretamente
- erro de credenciais for tratado sem vazar detalhes internos
- URL da API estiver configuravel por ambiente e nao fixa de forma definitiva
- navegacao entre pelo menos duas telas estiver validada, se o fluxo exigir
- logout limpar sessao local
- codigo proprio seguir as diretrizes do projeto
- Android permanecer tecnicamente possivel, mesmo sem polimento
- achados e recomendacao final do spike forem registrados

## Criterios de rejeicao

O spike deve bloquear continuidade se:

- nao for possivel configurar build iOS com esforco razoavel
- MAUI gerar complexidade maior que SwiftUI para o fluxo minimo
- armazenamento seguro de tokens nao puder ser validado
- consumo da API exigir mudancas grandes no backend
- experiencia basica no iPhone ficar ruim mesmo no fluxo minimo
- Android futuro exigir reescrita quase total da base

## Riscos

### Dependencia de Mac

Risco:

- nao haver Mac disponivel para build iOS.

Mitigacao:

- validar ambiente antes de criar telas definitivas
- documentar bloqueio caso Mac nao esteja disponivel

### Polir Android cedo demais

Risco:

- tentar resolver duas plataformas antes de validar valor no iPhone.

Mitigacao:

- Android deve ser apenas verificacao tecnica no spike

### UI parecer generica

Risco:

- a primeira tela parecer um app administrativo sem identidade de produto.

Mitigacao:

- UX deve comunicar simplicidade, grupos privados e convivio real

### Regra duplicada no app

Risco:

- mobile implementar regra que pertence ao backend.

Mitigacao:

- app valida campos para usabilidade, mas backend decide regra final

## Checklist do spike

### Ambiente

- [ ] Confirmar .NET SDK.
- [x] Instalar workload .NET MAUI.
- [ ] Confirmar Visual Studio ou ferramenta equivalente.
- [ ] Confirmar acesso ao backend local.
- [ ] Confirmar Docker/PostgreSQL local.
- [ ] Confirmar disponibilidade de Mac com Xcode para build iOS.

### Projeto

- [x] Criar projeto `ProjetoEncontros.Aplicativo`.
- [x] Adicionar projeto na solution.
- [x] Organizar pastas iniciais.
- [x] Configurar alvo iOS.
- [x] Manter Android como alvo futuro quando possivel.

### API

- [x] Configurar base URL por ambiente.
- [x] Centralizar base URL inicial da API.
- [x] Implementar cliente HTTP.
- [x] Implementar login.
- [x] Implementar usuario atual.
- [x] Implementar listagem de grupos.
- [x] Implementar criacao basica de grupo para validar primeiro uso.
- [x] Implementar detalhe basico de grupo.
- [x] Implementar listagem de membros do grupo.
- [x] Implementar remocao basica de membro comum pelo dono.
- [x] Implementar listagem de convites, se esse for o fluxo escolhido.
- [x] Implementar criacao de convite por e-mail no detalhe do grupo.
- [x] Implementar aceite de convite pelo app.
- [x] Implementar recusa de convite pelo app.
- [x] Implementar logout.

### Sessao

- [x] Armazenar tokens com seguranca.
- [x] Ler sessao ao abrir app.
- [x] Limpar sessao ao sair.
- [x] Tratar token invalido.
- [x] Renovar access token expirado com refresh token.

### UX

- [x] Criar tela de login.
- [x] Criar fluxo de cadastro simples.
- [x] Criar tela inicial.
- [x] Criar tela de detalhe basico de grupo.
- [x] Exibir convites pendentes na tela inicial.
- [x] Permitir envio de convite no detalhe do grupo para dono.
- [x] Permitir aceite e recusa de convite na tela inicial.
- [x] Permitir remocao basica de membro comum no detalhe do grupo.
- [x] Criar estado carregando.
- [x] Criar estado vazio.
- [x] Criar estado de erro.
- [x] Permitir criacao basica do primeiro grupo no estado vazio.
- [ ] Validar uso com uma mao.

### Validacao

- [x] Rodar build local.
- [ ] Validar build iOS ou registrar bloqueio.
- [ ] Validar execucao em simulador iOS, se disponivel.
- [ ] Validar execucao em dispositivo fisico, se disponivel.
- [ ] Rodar fluxo de login contra API real.
- [ ] Criar primeiro grupo pelo app.
- [ ] Validar logout.
- [ ] Validar retomada de sessao ao reabrir app.
- [ ] Validar renovacao de sessao expirada.
- [ ] Registrar achados e decisao final do spike.

## Registro de resultado

Ao concluir o spike, criar uma secao de resultado neste documento contendo:

- data da execucao
- ambiente usado
- comandos executados
- telas ou fluxos validados
- problemas encontrados
- decisao final: continuar MAUI, ajustar abordagem ou reavaliar alternativa
- proximas acoes aprovadas

### Resultado parcial - 2026-07-02

Ambiente usado:

- Windows
- .NET SDK 10.0.301
- workload MAUI 10.0.20/10.0.100

Comandos executados:

```text
dotnet workload install maui
dotnet new maui -n ProjetoEncontros.Aplicativo -o src\ProjetoEncontros.Aplicativo -f net10.0 --no-restore
dotnet restore src\ProjetoEncontros.Aplicativo\ProjetoEncontros.Aplicativo.csproj
dotnet build src\ProjetoEncontros.Aplicativo\ProjetoEncontros.Aplicativo.csproj --no-restore
dotnet sln ProjetoEncontros.sln add src\ProjetoEncontros.Aplicativo\ProjetoEncontros.Aplicativo.csproj
dotnet build ProjetoEncontros.sln --no-restore
dotnet test ProjetoEncontros.sln --no-build
dotnet build ProjetoEncontros.sln --no-restore
dotnet test ProjetoEncontros.sln --no-build
dotnet build ProjetoEncontros.sln --no-restore
dotnet test ProjetoEncontros.sln --no-build
dotnet build ProjetoEncontros.sln --no-restore
dotnet test ProjetoEncontros.sln --no-build
```

Resultados:

- workload MAUI instalado
- projeto `ProjetoEncontros.Aplicativo` criado
- tela inicial do template substituida por `TelaDeEntrada`
- navegacao inicial criada como `EstruturaDeNavegacao`
- projeto adicionado a solution
- build do projeto mobile aprovado
- build da solution aprovado
- testes da solution aprovados
- 50 testes de unidade aprovados
- 5 testes de integracao aprovados
- apos correcao da fabrica dos testes de integracao, o `DbContext` de testes passou a forcar o banco `projeto_encontros_testes`
- protecao contra reinicio acidental do banco principal foi mantida
- configuracao inicial da URL da API centralizada em `ConfiguracaoDaApi`
- cliente HTTP mobile implementado
- login mobile implementado contra `POST /api/autenticacao/login`
- armazenamento seguro de access token, refresh token e expiracao implementado com `SecureStorage`
- tela inicial mobile chama `GET /api/usuarios/eu`
- tela inicial mobile chama `GET /api/grupos`
- tela inicial mobile chama `POST /api/grupos` para criacao basica do primeiro grupo
- tela de entrada mobile chama `POST /api/autenticacao/cadastro` e faz login automatico apos cadastro
- tela de detalhe do grupo chama `GET /api/grupos/{identificadorDoGrupo}`
- tela de detalhe do grupo chama `GET /api/grupos/{identificadorDoGrupo}/membros`
- tela de detalhe do grupo chama `DELETE /api/grupos/{identificadorDoGrupo}/membros/{identificadorDoMembro}` para remocao basica de membro comum
- tela de detalhe do grupo chama `GET /api/convites` e filtra convites pendentes relacionados ao grupo atual
- tela de detalhe do grupo chama `POST /api/grupos/{identificadorDoGrupo}/convites` para convidar por e-mail
- tela inicial chama `GET /api/convites` para exibir convites pendentes do usuario
- tela inicial chama `POST /api/convites/{identificadorDoConvite}/aceitar` para aceitar convite
- tela inicial chama `POST /api/convites/{identificadorDoConvite}/recusar` para recusar convite
- logout mobile chama `POST /api/autenticacao/sair` e limpa a sessao local
- estados basicos de carregamento, erro e vazio implementados nas telas mobile
- URL da API pode ser sobrescrita pela variavel de ambiente `PROJETO_ENCONTROS_API_URL`
- app verifica sessao local ao abrir e navega para inicio quando ha sessao salva
- access token expirado e renovado com `POST /api/autenticacao/renovar-sessao`
- erro 401 ou 403 em chamadas autenticadas limpa sessao local e retorna para entrada
- estado vazio de grupos agora possui acao simples para criar grupo sem depender do Swagger
- criacao de grupo deixou de aparecer automaticamente no estado vazio
- botao `+` da tela inicial passou a exibir o painel de criacao de grupo
- usuario pode criar novo grupo pelo `+` mesmo quando ja possui grupos
- card de grupo na tela inicial agora abre o detalhe basico do grupo
- detalhe do grupo exibe informacoes basicas, membros e estado vazio de convites
- detalhe do grupo exibe situacao do membro e acao de remover quando o usuario atual e dono
- detalhe do grupo exibe acao de convite por e-mail para usuario com papel de dono
- convites pendentes aparecem na tela inicial acima da lista de grupos
- aceitar convite remove o convite pendente e recarrega a lista de grupos
- recusar convite remove o convite pendente da tela inicial
- ocorrencias antigas de inicializacao com `[]` foram substituidas por `new()` para manter a diretriz de nomenclatura/codigo
- tema visual inicial aplicado ao app MAUI com fundo creme, texto azul escuro, acao primaria verde, acentos coral e dourado
- tela de entrada e tela inicial foram ajustadas para comunicar melhor grupos privados, encontros e memorias
- apos teste visual no Windows, campos escuros e visual excessivamente administrativo foram corrigidos
- telas passaram a esconder a barra padrao do Shell, centralizar conteudo em largura proxima de celular e usar campos claros arredondados
- cards principais receberam maior arredondamento, melhor respiro e detalhes visuais inspirados na proposta enviada
- logo inicial foi adicionada a partir da imagem de referencia enviada pelo usuario
- telas passaram a exibir `Junto` como nome visual experimental do app
- titulo do aplicativo passou a exibir `Junto` no cliente mobile

Problemas encontrados:

- instalacao do workload MAUI demorou mais que o tempo inicial da chamada
- Android SDK nao esta configurado no ambiente
- build Android foi desabilitado por padrao para nao bloquear o foco inicial em iOS
- Visual Studio tentou executar o artefato iOS no Windows quando `net10.0-ios` estava como primeiro alvo do projeto
- ordem dos alvos foi ajustada para deixar `net10.0-windows10.0.19041.0` primeiro no Windows
- `AddHttpClient` nao estava disponivel no projeto MAUI sem pacote adicional
- cliente HTTP foi registrado manualmente para manter o spike enxuto
- URL padrao da API ainda aponta para `http://localhost:5281`
- para iOS/Mac/dispositivo fisico, a URL deve ser sobrescrita por ambiente
- criacao de grupo no mobile ainda e fluxo basico de validacao, nao tela definitiva de gerenciamento
- nome oficial do produto ainda nao foi formalizado nos documentos de produto
- a tela passou a usar `Junto` visualmente por solicitacao do usuario, mas a decisao oficial do nome ainda deve ser registrada nos documentos de produto
- o tema escuro do Windows afetava `Entry` e `Editor`, deixando os campos pretos; o app foi forçado para tema claro e os campos foram encapsulados em bordas claras

Decisao parcial:

- continuar com .NET MAUI iOS-first
- manter Android como alvo futuro habilitavel por propriedade de build
- proxima etapa tecnica deve validar o fluxo completo no app Windows contra a API local e depois preparar execucao iOS com Mac pareado

## Proxima acao apos este documento

A proxima acao deve ser executar o aplicativo localmente contra a API.

Validar:

- API rodando em `http://localhost:5281`
- PostgreSQL local disponivel via Docker
- login com usuario existente
- criacao do primeiro grupo pelo estado vazio
- listagem de grupos apos login
- logout retornando para tela de entrada
- fechamento e reabertura do app mantendo sessao
- limpeza automatica da sessao quando o token for invalido
