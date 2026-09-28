# Fundacao Enxuta e Identidade por Celular - Plano de Implementacao

> **Para agentes executores:** SUB-SKILL OBRIGATORIA: use `superpowers:subagent-driven-development` (recomendado) ou `superpowers:executing-plans` para executar este plano tarefa por tarefa. Os passos usam caixas de selecao (`- [ ]`) para acompanhamento.

**Objetivo:** Entregar a primeira etapa executavel do novo JuntoApp: identidade por celular e PIN, recuperacao administrativa, sessao existente preservada e uma navegacao minima com Inicio, Grupos, Pessoas e Perfil.

**Arquitetura:** A nova identidade sera adicionada ao dominio atual antes da remocao dos modulos antigos, mantendo compatibilidade temporaria para que cada commit compile. A API continuara usando JWT e cookie `HttpOnly` de atualizacao; o Flutter Web passara a enviar celular e PIN. As rotas de funcionalidades antigas ficarao desativadas nesta etapa, mas seus arquivos so serao removidos no plano seguinte junto com a nova linha de base do banco.

**Tecnologias:** .NET 10, ASP.NET Core Minimal APIs, Entity Framework Core, PostgreSQL, BCrypt, xUnit, Flutter Web/PWA, Riverpod, GoRouter e Playwright.

**Especificacao:** `docs/superpowers/specs/2026-09-28-reformulacao-juntoapp-design.md`

## Restricoes globais

- Todo codigo novo ou alterado deve usar portugues conforme `AGENTS.md`.
- Nao usar `var`; usar `new()` quando o tipo ja estiver explicito; sempre usar chaves.
- O numero aceito nesta primeira versao e um celular brasileiro: 11 algarismos, com DDD, aceitando `+55` e pontuacao na entrada e persistindo em E.164 (`+55XXXXXXXXXXX`).
- O PIN deve conter exatamente seis algarismos e deve ser persistido somente como hash BCrypt.
- O celular e privado; somente o proprio usuario e o administrador do sistema podem receber seu valor completo.
- O primeiro usuario cadastrado torna-se administrador do sistema. Cadastros seguintes sao usuarios comuns.
- Limitar cadastro e login a cinco tentativas por endereco IP a cada 15 minutos.
- Preservar a renovacao de sessao por cookie `HttpOnly`, `Secure` em producao e `SameSite=Strict`.
- Nao descartar, sobrescrever nem incluir por acidente as alteracoes locais existentes no checkout original; executar em worktree isolada criada a partir do commit da especificacao.
- Nao implementar grupos, encontros, importacao do WhatsApp, fotos ou estatisticas neste plano; as quatro abas podem usar estados vazios coerentes.

## Foco da revisao

- Celular com espacos, parenteses, hifen ou `+55` deve normalizar para o mesmo valor e detectar duplicidade.
- Celular estrangeiro, incompleto ou que nao represente movel brasileiro deve falhar sem persistir usuario.
- PIN nao numerico, com cinco ou sete digitos deve falhar no cliente e no servidor.
- Sexta tentativa de login no mesmo IP dentro de 15 minutos deve receber HTTP 429 sem consultar credenciais.
- Usuario comum tentando recuperar a conta de outra pessoa deve receber HTTP 403 e nenhuma alteracao deve ser salva.

---

### Tarefa 1: Identidade de dominio por celular e PIN

**Arquivos:**
- Criar: `src/ProjetoEncontros.Dominio/Usuarios/NumeroDeCelular.cs`
- Criar: `src/ProjetoEncontros.Dominio/Usuarios/PapelDoUsuario.cs`
- Modificar: `src/ProjetoEncontros.Dominio/Usuarios/Usuario.cs`
- Criar: `tests/ProjetoEncontros.TestesUnidade/Dominio/Usuarios/TestesDeNumeroDeCelular.cs`
- Criar: `tests/ProjetoEncontros.TestesUnidade/Dominio/Usuarios/TestesDeUsuario.cs`

**Interfaces:**
- Produz: `NumeroDeCelular.Crie(string valor)`, `NumeroDeCelular.Valor`, `Usuario.CrieComCelularEPin(Guid identificador, string nome, NumeroDeCelular numeroDeCelular, string hashDoPin, PapelDoUsuario papel, DateTimeOffset criadoEm)`, `Usuario.AltereNumeroDeCelular(NumeroDeCelular numeroDeCelular)` e `Usuario.AltereHashDoPin(string hashDoPin)`.
- Produz: `PapelDoUsuario.Pessoa` e `PapelDoUsuario.AdministradorDoSistema`; `Usuario.EhAdministradorDoSistema` deve ser derivado do papel.
- Compatibilidade temporaria: membros ligados a e-mail permanecem compilando ate o plano de remocao, mas as novas fabricas e rotas nao os utilizam.

- [ ] **Passo 1: Escrever os testes falhos de normalizacao do celular**

Cobrir entradas `62999998888`, `(62) 99999-8888` e `+55 62 99999-8888`, todas resultando em `+5562999998888`; cobrir vazio, dez digitos, DDD invalido, terceiro algarismo diferente de `9` e numero estrangeiro.

- [ ] **Passo 2: Executar os testes de numero e confirmar a falha**

Executar: `dotnet test tests/ProjetoEncontros.TestesUnidade/ProjetoEncontros.TestesUnidade.csproj --filter FullyQualifiedName~TestesDeNumeroDeCelular`

Esperado: falha porque `NumeroDeCelular` ainda nao existe.

- [ ] **Passo 3: Implementar `NumeroDeCelular`**

Remover apenas caracteres de formatacao permitidos, normalizar o DDI `55`, validar DDD entre `11` e `99`, exigir nono digito movel e persistir o formato E.164.

- [ ] **Passo 4: Escrever os testes falhos da nova fabrica de usuario**

Verificar papel, `EhAdministradorDoSistema`, hash obrigatorio, alteracao do celular sem troca do identificador e alteracao do hash do PIN.

- [ ] **Passo 5: Implementar a identidade nova em `Usuario`**

Adicionar `NumeroDeCelular`, `HashDoPin`, `Papel` e os metodos definidos em Interfaces. Manter somente a compatibilidade minima necessaria para o codigo legado compilar ate a proxima etapa.

- [ ] **Passo 6: Executar os testes de dominio**

Executar: `dotnet test tests/ProjetoEncontros.TestesUnidade/ProjetoEncontros.TestesUnidade.csproj --filter FullyQualifiedName~Dominio.Usuarios`

Esperado: todos aprovados.

- [ ] **Passo 7: Commit da tarefa**

Commit: `feat: adiciona identidade por celular e PIN`

### Tarefa 2: Cadastro e autenticacao por celular

**Arquivos:**
- Criar: `src/ProjetoEncontros.Aplicacao/Usuarios/Interfaces/IServicoDeHashDePin.cs`
- Modificar: `src/ProjetoEncontros.Aplicacao/Usuarios/Interfaces/IRepositorioDeUsuarios.cs`
- Modificar: `src/ProjetoEncontros.Aplicacao/Usuarios/CasosDeUso/CadastroDeUsuario.cs`
- Modificar: `src/ProjetoEncontros.Aplicacao/Autenticacao/CasosDeUso/AutenticacaoDeUsuario.cs`
- Modificar: `src/ProjetoEncontros.Aplicacao/Usuarios/Contratos/CadastreUsuarioComando.cs`
- Modificar: `src/ProjetoEncontros.Aplicacao/Usuarios/Contratos/UsuarioCadastradoResposta.cs`
- Modificar: `src/ProjetoEncontros.Aplicacao/Autenticacao/Contratos/AutentiqueUsuarioComando.cs`
- Modificar: `src/ProjetoEncontros.Aplicacao/Usuarios/Contratos/UsuarioAtualResposta.cs`
- Modificar: `tests/ProjetoEncontros.TestesUnidade/Aplicacao/Usuarios/TestesDeCadastroDeUsuario.cs`
- Modificar: `tests/ProjetoEncontros.TestesUnidade/Aplicacao/Autenticacao/TestesDeAutenticacaoDeUsuario.cs`

**Interfaces:**
- Consome: tipos da Tarefa 1.
- Produz: `IRepositorioDeUsuarios.ExisteComNumeroDeCelularAsync(NumeroDeCelular, CancellationToken)`, `ObtenhaPorNumeroDeCelularAsync(NumeroDeCelular, CancellationToken)` e `ExisteAlgumAsync(CancellationToken)`.
- Produz: `CadastreUsuarioComando(string Nome, string NumeroDeCelular, string Pin)` e `AutentiqueUsuarioComando(string NumeroDeCelular, string Pin)`.
- Produz: `IServicoDeHashDePin.GereHash(string pin)` e `Verifique(string pin, string hashDoPin)`.

- [ ] **Passo 1: Alterar os testes de cadastro para celular e PIN**

Fixar as assercoes: primeiro cadastro recebe papel de administrador; segundo recebe papel de pessoa; celular duplicado normalizado falha; PIN diferente de seis algarismos falha; a resposta nunca contem hash.

- [ ] **Passo 2: Executar o teste de cadastro e confirmar a falha**

Executar: `dotnet test tests/ProjetoEncontros.TestesUnidade/ProjetoEncontros.TestesUnidade.csproj --filter FullyQualifiedName~TestesDeCadastroDeUsuario`

Esperado: falha por contratos ainda baseados em e-mail e senha.

- [ ] **Passo 3: Implementar repositorio, contratos e cadastro**

Determinar o papel com `ExisteAlgumAsync`; gerar o hash pelo servico de PIN; rejeitar numero duplicado antes de adicionar o usuario.

- [ ] **Passo 4: Alterar os testes de autenticacao para celular e PIN**

Fixar as assercoes de sessao existente, usuario ausente/inativo, PIN incorreto e renovacao de token sem alteracao do comportamento atual.

- [ ] **Passo 5: Executar o teste de autenticacao e confirmar a falha**

Executar: `dotnet test tests/ProjetoEncontros.TestesUnidade/ProjetoEncontros.TestesUnidade.csproj --filter FullyQualifiedName~TestesDeAutenticacaoDeUsuario`

Esperado: falha porque a busca ainda usa e-mail.

- [ ] **Passo 6: Implementar autenticacao por numero normalizado e PIN**

Manter a mensagem unica `Celular ou PIN invalidos.` para usuario inexistente, inativo ou PIN incorreto.

- [ ] **Passo 7: Executar os testes da aplicacao**

Executar: `dotnet test tests/ProjetoEncontros.TestesUnidade/ProjetoEncontros.TestesUnidade.csproj --filter "FullyQualifiedName~Aplicacao.Usuarios|FullyQualifiedName~Aplicacao.Autenticacao"`

Esperado: todos aprovados.

- [ ] **Passo 8: Commit da tarefa**

Commit: `feat: autentica usuarios com celular e PIN`

### Tarefa 3: Alteracao e recuperacao de acesso

**Arquivos:**
- Criar: `src/ProjetoEncontros.Aplicacao/Usuarios/Contratos/AltereNumeroDeCelularComando.cs`
- Criar: `src/ProjetoEncontros.Aplicacao/Usuarios/Contratos/AlterePinComando.cs`
- Criar: `src/ProjetoEncontros.Aplicacao/Usuarios/Contratos/RecupereAcessoDoUsuarioComando.cs`
- Criar: `src/ProjetoEncontros.Aplicacao/Usuarios/CasosDeUso/AltereNumeroDeCelular.cs`
- Criar: `src/ProjetoEncontros.Aplicacao/Usuarios/CasosDeUso/AlterePin.cs`
- Criar: `src/ProjetoEncontros.Aplicacao/Usuarios/CasosDeUso/RecupereAcessoDoUsuario.cs`
- Modificar: `src/ProjetoEncontros.Aplicacao/Configuracoes/ConfiguracaoDaAplicacao.cs`
- Criar: `tests/ProjetoEncontros.TestesUnidade/Aplicacao/Usuarios/TestesDeGerenciamentoDeAcesso.cs`

**Interfaces:**
- Consome: repositorio e hash de PIN da Tarefa 2.
- Produz: `AltereNumeroDeCelular.AltereAsync(AltereNumeroDeCelularComando, CancellationToken)`, `AlterePin.AltereAsync(AlterePinComando, CancellationToken)` e `RecupereAcessoDoUsuario.RecupereAsync(RecupereAcessoDoUsuarioComando, CancellationToken)`.

- [ ] **Passo 1: Escrever testes falhos para alteracao do proprio acesso**

Cobrir troca de celular com PIN atual correto, rejeicao de celular ja usado, PIN atual incorreto, troca de PIN e revogacao de todos os tokens de atualizacao do usuario apos alteracao.

- [ ] **Passo 2: Executar os testes e confirmar a falha**

Executar: `dotnet test tests/ProjetoEncontros.TestesUnidade/ProjetoEncontros.TestesUnidade.csproj --filter FullyQualifiedName~TestesDeGerenciamentoDeAcesso`

Esperado: falha porque os casos de uso ainda nao existem.

- [ ] **Passo 3: Implementar alteracao do celular e do PIN**

Validar o PIN atual, unicidade do novo numero e revogar sessoes existentes antes de salvar numa unica unidade de trabalho.

- [ ] **Passo 4: Acrescentar testes falhos de recuperacao administrativa**

Cobrir administrador redefinindo celular e PIN temporario; usuario comum recebe negacao; alvo inexistente falha; celular de terceiro nao pode ser usado.

- [ ] **Passo 5: Implementar recuperacao administrativa**

Autorizar pelo `PapelDoUsuario`, alterar os dados do alvo, revogar seus tokens e nunca retornar o hash.

- [ ] **Passo 6: Executar os testes da tarefa**

Executar: `dotnet test tests/ProjetoEncontros.TestesUnidade/ProjetoEncontros.TestesUnidade.csproj --filter FullyQualifiedName~TestesDeGerenciamentoDeAcesso`

Esperado: todos aprovados.

- [ ] **Passo 7: Commit da tarefa**

Commit: `feat: permite recuperar acesso sem perder o perfil`

### Tarefa 4: Persistencia, API e protecao contra tentativas

**Arquivos:**
- Criar: `src/ProjetoEncontros.Infraestrutura/Seguranca/ServicoDeHashDePin.cs`
- Preservar temporariamente: `src/ProjetoEncontros.Infraestrutura/Seguranca/ServicoDeHashDeSenha.cs`, usado apenas pelo codigo legado ate sua remocao no plano seguinte
- Modificar: `src/ProjetoEncontros.Infraestrutura/Dados/Repositorios/RepositorioDeUsuarios.cs`
- Modificar: `src/ProjetoEncontros.Infraestrutura/Dados/Mapeamentos/MapeamentoDeUsuario.cs`
- Modificar: `src/ProjetoEncontros.Infraestrutura/Configuracoes/ConfiguracaoDaInfraestrutura.cs`
- Criar por EF: migracao `V2IdentidadePorCelular` em `src/ProjetoEncontros.Infraestrutura/Dados/Migracoes/`
- Modificar: `src/ProjetoEncontros.Api/Contratos/Autenticacao/RequisicaoDeCadastro.cs`
- Modificar: `src/ProjetoEncontros.Api/Contratos/Autenticacao/RequisicaoDeLogin.cs`
- Modificar: `src/ProjetoEncontros.Api/Contratos/Autenticacao/RespostaDeCadastro.cs`
- Criar: `src/ProjetoEncontros.Api/Contratos/Usuarios/RequisicaoDeAlteracaoDoCelular.cs`
- Criar: `src/ProjetoEncontros.Api/Contratos/Usuarios/RequisicaoDeAlteracaoDoPin.cs`
- Criar: `src/ProjetoEncontros.Api/Contratos/Usuarios/RequisicaoDeRecuperacaoDeAcesso.cs`
- Modificar: `src/ProjetoEncontros.Api/Rotas/RotasDeAutenticacao.cs`
- Modificar: `src/ProjetoEncontros.Api/Rotas/RotasDeUsuarios.cs`
- Modificar: `src/ProjetoEncontros.Api/Configuracoes/ConfiguracaoDaApi.cs`
- Modificar: `src/ProjetoEncontros.Api/Program.cs`
- Modificar: `tests/ProjetoEncontros.TestesIntegracao/TestesDeAutenticacaoDoNavegador.cs`
- Criar: `tests/ProjetoEncontros.TestesIntegracao/TestesDeGerenciamentoDeAcesso.cs`

**Interfaces:**
- Produz HTTP: `POST /api/autenticacao/cadastro`, `POST /api/autenticacao/login`, `POST /api/autenticacao/navegador/login`, `PUT /api/usuarios/eu/celular`, `PUT /api/usuarios/eu/pin` e `PUT /api/usuarios/{identificador:guid}/recuperar-acesso`.
- Os tres endpoints de autenticacao consomem `numeroDeCelular` e `pin`; o endpoint administrativo exige JWT com papel `AdministradorDoSistema`.

- [ ] **Passo 1: Escrever testes de integracao falhos dos contratos HTTP**

Testar cadastro e login com numero formatado, retorno do proprio celular em `/api/usuarios/eu`, ausencia de e-mail nas respostas, duplicidade HTTP 400 e credenciais invalidas HTTP 400 conforme o middleware atual.

- [ ] **Passo 2: Executar os testes HTTP e confirmar a falha**

Executar: `dotnet test tests/ProjetoEncontros.TestesIntegracao/ProjetoEncontros.TestesIntegracao.csproj --filter "FullyQualifiedName~AutenticacaoDoNavegador|FullyQualifiedName~GerenciamentoDeAcesso"`

Esperado: falha por contratos antigos.

- [ ] **Passo 3: Implementar persistencia e gerar a migracao**

Criar indices unicos para `numero_de_celular`; mapear `hash_do_pin` e `papel`; tornar colunas legadas anulaveis apenas durante a transicao. Gerar a migracao com `dotnet ef migrations add V2IdentidadePorCelular --project src/ProjetoEncontros.Infraestrutura --startup-project src/ProjetoEncontros.Api`.

- [ ] **Passo 4: Implementar contratos e rotas HTTP**

Mapear os comandos das Tarefas 2 e 3, manter o cookie atual e emitir o papel no JWT para a politica administrativa.

- [ ] **Passo 5: Escrever e executar o teste falho do limite de tentativas**

Adicionar seis requisicoes de login do mesmo IP: as cinco primeiras seguem o fluxo normal e a sexta retorna HTTP 429. Executar o filtro `FullyQualifiedName~LimiteDeTentativasDeEntrada` e confirmar a falha.

- [ ] **Passo 6: Implementar a politica `Entrada` de limite fixo**

Registrar `AddRateLimiter`, particionar pelo IP encaminhado validado, configurar cinco requisicoes por 15 minutos, fila zero e aplicar somente a cadastro e login. Chamar `UseRateLimiter` antes da autorizacao.

- [ ] **Passo 7: Executar testes de integracao e migracao local**

Executar: `dotnet test tests/ProjetoEncontros.TestesIntegracao/ProjetoEncontros.TestesIntegracao.csproj --filter "FullyQualifiedName~Autenticacao|FullyQualifiedName~GerenciamentoDeAcesso|FullyQualifiedName~LimiteDeTentativas"`

Esperado: todos aprovados.

- [ ] **Passo 8: Commit da tarefa**

Commit: `feat: expoe acesso seguro por celular na API`

### Tarefa 5: Entrada e cadastro no Flutter Web

**Arquivos:**
- Modificar: `src/ProjetoEncontros.AplicativoWeb/lib/compartilhado/autenticacao/repositorio_de_autenticacao.dart`
- Modificar: `src/ProjetoEncontros.AplicativoWeb/lib/compartilhado/autenticacao/controlador_de_sessao.dart`
- Modificar: `src/ProjetoEncontros.AplicativoWeb/lib/compartilhado/acessibilidade/identificadores_semanticos.dart`
- Modificar: `src/ProjetoEncontros.AplicativoWeb/lib/funcionalidades/entrada/telas/tela_de_entrada.dart`
- Modificar: `src/ProjetoEncontros.AplicativoWeb/lib/funcionalidades/entrada/telas/tela_de_cadastro.dart`
- Modificar: `src/ProjetoEncontros.AplicativoWeb/lib/funcionalidades/inicio/modelos/usuario_atual.dart`
- Modificar: `src/ProjetoEncontros.AplicativoWeb/lib/funcionalidades/perfil/telas/tela_de_perfil.dart`
- Modificar: `src/ProjetoEncontros.AplicativoWeb/test/aplicativo_test.dart`

**Interfaces:**
- Consome: contratos HTTP da Tarefa 4.
- Produz: `IRepositorioDeAutenticacao.autentiqueAsync({required String numeroDeCelular, required String pin})` e `cadastreAsync({required String nome, required String numeroDeCelular, required String pin})`.

- [ ] **Passo 1: Escrever testes de widget falhos para celular e PIN**

Verificar campos e rotulos, teclado de telefone, PIN obscurecido e numerico, validacao de seis digitos, envio das chaves JSON corretas e ausencia de e-mail/senha.

- [ ] **Passo 2: Executar os testes e confirmar a falha**

Executar em `src/ProjetoEncontros.AplicativoWeb`: `flutter test test/aplicativo_test.dart`

Esperado: falha porque as telas ainda exibem e-mail e senha.

- [ ] **Passo 3: Implementar repositorio, controlador e telas**

Reutilizar o visual atual; substituir apenas campos, validacoes, semantica e textos necessarios. O perfil mostra o celular somente ao proprio usuario.

- [ ] **Passo 4: Executar testes e analise estatica**

Executar: `flutter test` e `flutter analyze`.

Esperado: ambos concluem sem falhas.

- [ ] **Passo 5: Commit da tarefa**

Commit: `feat: simplifica entrada com celular e PIN`

### Tarefa 6: Navegacao minima do novo produto

**Arquivos:**
- Modificar: `src/ProjetoEncontros.AplicativoWeb/lib/compartilhado/navegacao/estrutura_com_navegacao.dart`
- Modificar: `src/ProjetoEncontros.AplicativoWeb/lib/compartilhado/navegacao/rotas_do_aplicativo.dart`
- Criar: `src/ProjetoEncontros.AplicativoWeb/lib/funcionalidades/grupos/telas/tela_de_grupos.dart`
- Modificar: `src/ProjetoEncontros.AplicativoWeb/lib/funcionalidades/inicio/telas/tela_inicial.dart`
- Modificar: `src/ProjetoEncontros.AplicativoWeb/lib/funcionalidades/pessoas_frequentes/telas/tela_de_pessoas.dart`
- Modificar: `src/ProjetoEncontros.AplicativoWeb/test/aplicativo_test.dart`
- Modificar: `src/ProjetoEncontros.Api/Program.cs`

**Interfaces:**
- Produz rotas autenticadas `/inicio`, `/grupos`, `/pessoas` e `/perfil`.
- Produz dock na ordem Inicio, Grupos, Pessoas e Perfil.

- [ ] **Passo 1: Escrever testes de widget falhos da nova navegacao**

Verificar os quatro itens na ordem definida, selecao por rota, navegacao para `/grupos` e ausencia de Memorias e Notificacoes no dock.

- [ ] **Passo 2: Executar o teste e confirmar a falha**

Executar em `src/ProjetoEncontros.AplicativoWeb`: `flutter test test/aplicativo_test.dart`

Esperado: falha porque o dock ainda contem Memorias e nao contem Grupos.

- [ ] **Passo 3: Implementar a estrutura minima**

Criar estados vazios uteis: Inicio explica que encontros aparecerao ali; Grupos permite apenas visualizar o estado vazio; Pessoas preserva a tela sem dados; Perfil preserva saida e dados da conta. Remover da tabela de rotas os acessos a feed, notificacoes, itens e memorias antigas.

- [ ] **Passo 4: Desativar rotas antigas da API**

Em `Program.cs`, mapear somente autenticacao, usuarios, saude e aplicativo web nesta etapa. Nao apagar implementacoes antigas ainda.

- [ ] **Passo 5: Executar testes Flutter e backend**

Executar: `flutter test`, `flutter analyze`, `dotnet test ProjetoEncontros.sln --no-restore` e `dotnet build ProjetoEncontros.sln --no-restore`.

Esperado: todos concluem sem falhas.

- [ ] **Passo 6: Commit da tarefa**

Commit: `refactor: reduz JuntoApp ao novo nucleo navegavel`

### Tarefa 7: Verificacao integrada da primeira etapa

**Arquivos:**
- Modificar: `tests/ProjetoEncontros.TestesNavegador/testes/sessao_e_encontro.spec.js` e renomear para `sessao_e_navegacao.spec.js`
- Modificar: `README.md` somente na secao de execucao local para trocar credenciais de exemplo por celular e PIN.

**Interfaces:**
- Consome: API e Flutter das tarefas anteriores.
- Produz: fluxo navegavel de cadastro, entrada, restauracao de sessao, quatro abas e saida.

- [ ] **Passo 1: Atualizar o teste de navegador**

Cobrir cadastro do primeiro usuario, login por celular formatado, restauracao apos recarregar, navegacao pelas quatro abas, celular visivel apenas no proprio Perfil e encerramento da sessao.

- [ ] **Passo 2: Executar o teste de navegador e corrigir somente falhas da etapa**

Executar pela raiz: `pwsh -File scripts/execute-testes-navegador.ps1`.

Esperado: todos os cenarios aprovados.

- [ ] **Passo 3: Executar a verificacao completa**

Executar: `dotnet test ProjetoEncontros.sln --no-restore`, `dotnet build ProjetoEncontros.sln --no-restore`, `flutter test` e `flutter analyze` no projeto Flutter, seguidos do teste de navegador.

Esperado: zero falhas, zero erros de compilacao e zero diagnosticos do analisador.

- [ ] **Passo 4: Revisar o diff contra esta especificacao**

Confirmar que nenhum arquivo de conversa, dado de producao ou alteracao local do checkout original entrou no branch; confirmar que grupos/encontros/importacao nao foram implementados antecipadamente.

- [ ] **Passo 5: Commit final da etapa**

Commit: `test: valida fundacao enxuta do JuntoApp`

## Planos seguintes

1. Grupos recorrentes, encontros avulsos, participantes e nova linha de base limpa do banco.
2. Pessoas, privacidade relacional, linha do tempo e estatisticas.
3. Fotos compactadas, cotas por grupo e marcacoes.
4. Importacao incremental do WhatsApp e retrospectivas.
