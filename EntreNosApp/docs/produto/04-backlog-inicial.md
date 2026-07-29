# Projeto Encontros - Backlog Inicial

## Objetivo do documento

Este documento organiza o backlog inicial do Projeto Encontros.

Cada funcionalidade descrita aqui deve possuir objetivo, descricao, regras de negocio, criterios de aceite, prioridade e versao planejada.

Este documento deve ser usado como referencia antes de iniciar qualquer implementacao.

## Referencias oficiais

Este backlog deve respeitar:

- `00-visao-produto.md`
- `01-roadmap-versoes.md`
- `02-arquitetura-inicial.md`
- `03-regras-de-produto.md`

Se uma funcionalidade entrar em conflito com a visao ou com as regras do produto, ela deve ser revisada antes de ser implementada.

## Criterios de prioridade

### P0 - Essencial

Funcionalidade necessaria para que a versao entregue seu valor principal.

Sem ela, a versao nao deve ser considerada concluida.

### P1 - Importante

Funcionalidade importante para melhorar a experiencia, completar o fluxo ou reduzir atrito.

Pode ser ajustada ou adiada se houver risco para a entrega da versao.

### P2 - Complementar

Funcionalidade util, mas nao essencial para validar a versao.

Deve ser implementada apenas se nao comprometer simplicidade, qualidade ou prazo.

## Regras gerais do backlog

- Toda funcionalidade deve pertencer a uma versao.
- Nenhuma funcionalidade futura deve ser implementada antes da versao planejada.
- Toda funcionalidade deve fortalecer pelo menos um pilar do produto.
- Funcionalidades que aproximem o produto de rede social ou aplicativo de mensagens devem ser questionadas.
- Regras de negocio devem ser validadas antes da implementacao.
- Criterios de aceite devem ser usados pelo QA para validar a entrega.

## Versao 0.1 - Fundacao do produto

### BE-001 - Cadastro de usuario

**Objetivo:** Permitir que uma pessoa crie sua identidade no Projeto Encontros.

**Descricao:** O usuario deve conseguir criar uma conta para acessar grupos privados e participar da organizacao de encontros.

**Regras de negocio:**

- O usuario deve informar dados minimos para criar a conta.
- O e-mail deve ser unico.
- A senha deve ser armazenada com hash seguro.
- Dados publicos desnecessarios nao devem ser solicitados no inicio.
- O usuario criado nao deve pertencer automaticamente a nenhum grupo.

**Criterios de aceite:**

- Deve ser possivel criar uma conta com dados validos.
- Nao deve ser possivel criar duas contas com o mesmo e-mail.
- Dados obrigatorios ausentes devem retornar erro claro.
- Senha nao deve ser armazenada em texto puro.
- O usuario criado deve conseguir autenticar-se depois do cadastro.

**Prioridade:** P0

**Versao:** 0.1

### BE-002 - Login de usuario

**Objetivo:** Permitir que o usuario acesse sua conta com seguranca.

**Descricao:** O usuario deve conseguir autenticar-se usando credenciais validas e receber tokens de acesso.

**Regras de negocio:**

- Login deve exigir credenciais validas.
- Credenciais invalidas nao devem informar se o e-mail existe.
- A autenticacao deve usar JWT.
- O refresh token deve ser criado de forma segura.
- O usuario autenticado deve acessar apenas seus proprios dados e grupos.

**Criterios de aceite:**

- Usuario com credenciais validas deve conseguir autenticar-se.
- Usuario com credenciais invalidas deve receber erro padronizado.
- Login deve retornar access token e refresh token.
- Access token deve permitir acesso a endpoints privados.
- Erros de login nao devem expor informacoes sensiveis.

**Prioridade:** P0

**Versao:** 0.1

### BE-003 - Renovacao e revogacao de sessao

**Objetivo:** Manter o usuario autenticado com seguranca sem exigir login constante.

**Descricao:** O sistema deve permitir renovacao de access token por refresh token e revogacao de sessao quando necessario.

**Regras de negocio:**

- Refresh token deve estar associado a um usuario.
- Refresh token expirado ou revogado nao deve gerar novo access token.
- Logout deve revogar o refresh token ativo.
- Tokens devem evitar exposicao de dados sensiveis.

**Criterios de aceite:**

- Deve ser possivel renovar access token com refresh token valido.
- Refresh token invalido deve retornar erro.
- Logout deve impedir reutilizacao do refresh token.
- Usuario nao autenticado nao deve acessar endpoints privados.

**Prioridade:** P0

**Versao:** 0.1

### BE-004 - Criacao de grupo

**Objetivo:** Permitir que o usuario crie um espaco privado para seu grupo.

**Descricao:** O usuario autenticado deve conseguir criar um grupo com nome e informacoes basicas.

**Regras de negocio:**

- Apenas usuarios autenticados podem criar grupos.
- Grupo deve possuir nome claro.
- Criador do grupo deve tornar-se dono do grupo.
- Grupo deve ser privado por padrao.
- Grupo deve iniciar sem exposicao publica.

**Criterios de aceite:**

- Usuario autenticado deve conseguir criar grupo com dados validos.
- Usuario nao autenticado nao deve criar grupo.
- Grupo criado deve aparecer na lista de grupos do criador.
- Criador deve ser registrado como dono do grupo.
- Nome ausente ou invalido deve retornar erro claro.

**Prioridade:** P0

**Versao:** 0.1

### BE-005 - Listagem dos grupos do usuario

**Objetivo:** Permitir que o usuario encontre rapidamente seus grupos.

**Descricao:** O usuario autenticado deve visualizar apenas os grupos dos quais participa.

**Regras de negocio:**

- A lista deve retornar somente grupos do usuario autenticado.
- Grupos de outros usuarios nao devem ser exibidos.
- A listagem deve ser simples e preparada para paginacao quando necessario.
- Cada item deve conter apenas informacoes essenciais.

**Criterios de aceite:**

- Usuario autenticado deve visualizar seus grupos.
- Usuario sem grupos deve receber estado vazio adequado.
- Usuario nao deve visualizar grupos de terceiros.
- Usuario nao autenticado nao deve acessar a listagem.

**Prioridade:** P0

**Versao:** 0.1

### BE-006 - Visualizacao basica de grupo

**Objetivo:** Permitir que membros entendam o espaco principal do grupo.

**Descricao:** Um membro deve conseguir abrir um grupo e visualizar suas informacoes basicas.

**Regras de negocio:**

- Apenas membros do grupo podem visualizar seus detalhes.
- A tela deve priorizar informacoes essenciais.
- Dados sensiveis do grupo nao devem ser expostos para nao membros.
- O grupo deve servir como ponto de partida para encontros e memorias futuras.

**Criterios de aceite:**

- Membro do grupo deve visualizar dados basicos do grupo.
- Usuario externo deve receber erro de acesso.
- Grupo inexistente deve retornar erro adequado.
- Informacoes exibidas devem ser suficientes para identificar o grupo.

**Prioridade:** P0

**Versao:** 0.1

### BE-007 - Convite para grupo

**Objetivo:** Permitir entrada controlada de novos membros em grupos privados.

**Descricao:** O dono do grupo deve conseguir convidar uma pessoa para participar do grupo.

**Regras de negocio:**

- Apenas usuario com permissao deve convidar membros.
- Convite deve pertencer a um grupo.
- Convite deve possuir estado.
- Convite nao deve expor dados sensiveis do grupo.
- Convite aceito nao deve ser reutilizado de forma insegura.

**Criterios de aceite:**

- Dono do grupo deve conseguir criar convite.
- Usuario convidado deve conseguir listar convites pendentes vinculados ao seu e-mail.
- Membro sem permissao nao deve convidar se a regra definida nao permitir.
- Convite criado deve ficar com estado pendente.
- Usuario externo nao deve acessar detalhes privados do grupo pelo convite.

**Prioridade:** P0

**Versao:** 0.1

### BE-008 - Aceite de convite

**Objetivo:** Permitir que uma pessoa entre em um grupo privado por convite valido.

**Descricao:** O usuario convidado deve conseguir aceitar um convite e tornar-se membro do grupo.

**Regras de negocio:**

- Convite deve estar valido para ser aceito.
- Convite aceito deve alterar seu estado.
- Usuario que aceita o convite deve virar membro do grupo.
- Um usuario nao deve ser duplicado como membro do mesmo grupo.
- Convite invalido, expirado ou cancelado nao deve permitir entrada.

**Criterios de aceite:**

- Usuario com convite valido deve entrar no grupo.
- Convite aceito deve mudar de estado.
- Grupo deve aparecer na lista do usuario apos aceite.
- Convite invalido deve retornar erro claro.
- Usuario ja membro nao deve ser duplicado.

**Prioridade:** P0

**Versao:** 0.1

### BE-009 - Gerenciamento basico de membros

**Objetivo:** Permitir controle inicial dos participantes do grupo.

**Descricao:** O dono do grupo deve conseguir visualizar membros e remover participantes quando necessario.

**Regras de negocio:**

- Apenas usuario com permissao deve remover membros.
- O grupo deve manter pelo menos um responsavel.
- Um usuario removido nao deve acessar dados privados do grupo.
- Remocao deve preservar integridade do historico quando aplicavel.

**Criterios de aceite:**

- Dono deve visualizar membros do grupo.
- Dono deve conseguir remover membro comum.
- Membro removido nao deve mais visualizar o grupo.
- Usuario sem permissao nao deve remover membros.
- Sistema nao deve permitir deixar grupo sem responsavel.

**Prioridade:** P1

**Versao:** 0.1

## Versao 0.2 - Organizacao de encontros

### BE-010 - Criacao de encontro

**Objetivo:** Permitir que o grupo organize um encontro real.

**Descricao:** Um membro autorizado deve criar um encontro com data, horario, local opcional e descricao simples.

**Regras de negocio:**

- Encontro deve pertencer a um grupo.
- Apenas membros do grupo podem criar encontros.
- Encontro deve ter data e horario.
- Local pode ser opcional.
- Descricao deve ser simples e objetiva.
- Encontro deve iniciar em estado planejado.

**Criterios de aceite:**

- Membro do grupo deve conseguir criar encontro valido.
- Usuario externo nao deve criar encontro no grupo.
- Data e horario ausentes devem retornar erro.
- Encontro criado deve aparecer nos proximos encontros do grupo.
- Encontro deve preservar o grupo ao qual pertence.

**Prioridade:** P0

**Versao:** 0.2

### BE-011 - Edicao de encontro

**Objetivo:** Permitir ajustes quando informacoes do encontro mudarem.

**Descricao:** Um membro autorizado deve conseguir alterar dados basicos de um encontro.

**Regras de negocio:**

- Apenas membros autorizados podem editar encontros.
- Alteracoes devem manter dados obrigatorios validos.
- Encontros cancelados ou realizados podem ter edicao limitada.
- Alteracoes relevantes poderao gerar notificacao em versao futura.

**Criterios de aceite:**

- Usuario autorizado deve conseguir editar dados validos.
- Usuario externo nao deve editar encontro.
- Alteracoes invalidas devem retornar erro.
- Dados atualizados devem aparecer nos detalhes do encontro.

**Prioridade:** P1

**Versao:** 0.2

### BE-012 - Cancelamento de encontro

**Objetivo:** Permitir que o grupo registre quando um encontro nao acontecera.

**Descricao:** Um encontro planejado deve poder ser cancelado sem apagar seu historico.

**Regras de negocio:**

- Apenas membro autorizado pode cancelar encontro.
- Cancelamento deve alterar estado do encontro.
- Encontro cancelado nao deve aparecer como proximo encontro ativo.
- Historico do encontro cancelado deve ser preservado.

**Criterios de aceite:**

- Usuario autorizado deve conseguir cancelar encontro.
- Encontro cancelado deve exibir estado correto.
- Encontro cancelado nao deve aceitar confirmacoes novas se a regra assim definir.
- Usuario externo nao deve cancelar encontro.

**Prioridade:** P1

**Versao:** 0.2

### BE-013 - Confirmacao de presenca

**Objetivo:** Permitir que o grupo saiba quem pretende participar do encontro.

**Descricao:** Membros do grupo devem conseguir confirmar ou remover sua presenca em um encontro.

**Regras de negocio:**

- Apenas membros do grupo podem confirmar presenca.
- Cada membro deve ter apenas uma resposta ativa por encontro.
- A presenca deve estar associada ao encontro.
- A primeira versao pode usar apenas confirmado e nao confirmado.
- Usuario externo nao pode confirmar presenca.

**Criterios de aceite:**

- Membro deve conseguir confirmar presenca.
- Membro deve conseguir alterar sua resposta.
- Lista de presencas deve refletir alteracoes.
- Usuario externo nao deve confirmar presenca.
- Nao deve haver presenca duplicada do mesmo membro no mesmo encontro.

**Prioridade:** P0

**Versao:** 0.2

### BE-014 - Listagem de proximos encontros

**Objetivo:** Permitir que o grupo veja rapidamente o que esta planejado.

**Descricao:** O grupo deve ter uma lista simples de encontros futuros ou ativos.

**Regras de negocio:**

- Apenas membros do grupo podem visualizar encontros do grupo.
- Encontros cancelados nao devem ser tratados como ativos.
- A lista deve priorizar encontros mais proximos.
- A listagem deve ser preparada para paginacao quando necessario.

**Criterios de aceite:**

- Membro deve visualizar proximos encontros do grupo.
- Usuario externo nao deve visualizar encontros.
- Encontros devem aparecer ordenados de forma previsivel.
- Grupo sem encontros deve exibir estado vazio.

**Prioridade:** P0

**Versao:** 0.2

## Versao 0.3 - Memorias dos encontros

### BE-015 - Marcacao de encontro como realizado

**Objetivo:** Separar encontros futuros de encontros que ja viraram memoria.

**Descricao:** Um encontro deve poder ser marcado como realizado para separar historico de proximos encontros.

**Regras de negocio:**

- Apenas membro autorizado pode marcar encontro como realizado.
- Encontro realizado deve permanecer no historico.
- Memorias podem ser adicionadas antes, durante ou depois do encontro por participantes autorizados.
- Encontro cancelado nao deve ser marcado como realizado sem regra especifica.

**Criterios de aceite:**

- Encontro planejado deve poder ser marcado como realizado.
- Encontro realizado deve aparecer no historico.
- Encontro realizado deve permitir acesso ao album do encontro.
- Usuario externo nao deve alterar estado do encontro.

**Prioridade:** P0

**Versao:** 0.3

### BE-016 - Album de memorias do encontro

**Objetivo:** Preservar lembrancas associadas ao encontro correto.

**Descricao:** Cada encontro deve possuir um espaco para fotos, videos e descricoes simples de memoria.

**Regras de negocio:**

- Album deve pertencer a um encontro.
- Apenas participantes autorizados do encontro podem visualizar o album.
- Memorias devem preservar contexto do encontro.
- Album nao deve funcionar como feed publico.

**Criterios de aceite:**

- Participante deve acessar album do encontro antes, durante ou depois do evento.
- Usuario externo nao deve acessar album.
- Album vazio deve exibir estado adequado.
- Memorias adicionadas devem aparecer associadas ao encontro correto.

**Prioridade:** P0

**Versao:** 0.3

### BE-017 - Envio de fotos e videos

**Objetivo:** Permitir que membros guardem registros visuais dos encontros.

**Descricao:** Membros do grupo devem conseguir enviar fotos e videos para o album de um encontro.

**Regras de negocio:**

- Apenas participantes autorizados do encontro podem enviar midias.
- Arquivos devem ser associados ao encontro correto.
- Tipo e tamanho de arquivo devem ser validados.
- Arquivos devem respeitar permissoes do encontro.
- Armazenamento devera usar Cloudflare R2 quando implementado.

**Criterios de aceite:**

- Membro deve conseguir enviar arquivo valido.
- Arquivo invalido deve ser rejeitado com erro claro.
- Midia enviada deve aparecer no album correto.
- Usuario externo nao deve enviar midia.
- Midia nao deve ser publica por padrao.

**Prioridade:** P0

**Versao:** 0.3

### BE-018 - Historico de encontros passados

**Objetivo:** Permitir que o grupo encontre encontros ja realizados.

**Descricao:** O grupo deve conseguir visualizar encontros passados em uma lista organizada.

**Regras de negocio:**

- Apenas membros podem visualizar historico do grupo.
- Encontros realizados devem aparecer no historico.
- Encontros cancelados podem aparecer separados ou sinalizados.
- A lista deve ser ordenada de forma previsivel.

**Criterios de aceite:**

- Membro deve visualizar encontros passados.
- Usuario externo nao deve visualizar historico.
- Encontro realizado deve continuar abrindo suas memorias.
- Grupo sem historico deve exibir estado vazio.

**Prioridade:** P1

**Versao:** 0.3

## Versao 0.4 - Organizacao colaborativa

### BE-019 - Lista de itens do encontro

**Objetivo:** Reduzir esquecimentos e combinados perdidos antes de um encontro.

**Descricao:** Um encontro deve poder ter uma lista simples de itens ou combinados.

**Regras de negocio:**

- Lista deve pertencer a um encontro.
- Apenas membros do grupo podem visualizar a lista.
- Itens devem ter descricao clara.
- Lista deve permanecer simples e nao virar sistema complexo de tarefas.

**Criterios de aceite:**

- Membro deve conseguir criar item na lista.
- Membro deve visualizar itens do encontro.
- Usuario externo nao deve acessar a lista.
- Item sem descricao valida deve ser rejeitado.

**Prioridade:** P0

**Versao:** 0.4

### BE-020 - Responsavel por item

**Objetivo:** Deixar claro quem combinou de levar ou resolver cada item.

**Descricao:** Um item da lista deve poder ser atribuido a um membro do grupo.

**Regras de negocio:**

- Responsavel deve ser membro do grupo.
- Um item pode ter responsavel opcional.
- Responsabilidade deve ser visivel para membros do grupo.
- Alteracoes devem preservar clareza do combinado.

**Criterios de aceite:**

- Deve ser possivel atribuir item a membro do grupo.
- Nao deve ser possivel atribuir item a usuario externo.
- Deve ser possivel alterar responsavel.
- Item sem responsavel deve continuar valido.

**Prioridade:** P1

**Versao:** 0.4

### BE-021 - Marcacao de item como resolvido

**Objetivo:** Ajudar o grupo a acompanhar o que ja foi preparado.

**Descricao:** Membros devem conseguir marcar itens da lista como resolvidos.

**Regras de negocio:**

- Apenas membros do grupo podem alterar estado do item.
- Item deve ter estado simples.
- Estado deve ser visivel para o grupo.
- O fluxo deve exigir poucos toques.

**Criterios de aceite:**

- Membro deve marcar item como resolvido.
- Membro deve desfazer marcacao se necessario.
- Estado atualizado deve aparecer para membros.
- Usuario externo nao deve alterar item.

**Prioridade:** P1

**Versao:** 0.4

## Versao 0.5 - Linha do tempo do grupo

### BE-022 - Linha do tempo privada

**Objetivo:** Transformar encontros e memorias em historia organizada do grupo.

**Descricao:** O grupo deve possuir uma linha do tempo privada com encontros realizados e memorias associadas.

**Regras de negocio:**

- Linha do tempo deve pertencer ao grupo.
- Apenas membros podem visualizar.
- Conteudo deve ser privado.
- A ordem deve priorizar tempo e contexto, nao engajamento.
- A linha do tempo nao deve ser feed publico.

**Criterios de aceite:**

- Membro deve visualizar linha do tempo do grupo.
- Usuario externo nao deve visualizar linha do tempo.
- Encontros realizados devem aparecer ordenados.
- Memorias devem estar associadas ao encontro correto.

**Prioridade:** P0

**Versao:** 0.5

### BE-023 - Filtros simples da linha do tempo

**Objetivo:** Facilitar encontrar momentos passados sem complexidade.

**Descricao:** A linha do tempo deve permitir filtros simples por periodo ou tipo de conteudo.

**Regras de negocio:**

- Filtros devem ser simples.
- Filtros nao devem exigir configuracao complexa.
- Resultado deve respeitar permissoes do grupo.
- Busca avancada deve ficar para versao futura.

**Criterios de aceite:**

- Membro deve filtrar por periodo quando disponivel.
- Filtro deve retornar apenas conteudo do grupo.
- Estado vazio deve ser exibido quando nao houver resultados.
- Usuario externo nao deve usar filtros do grupo.

**Prioridade:** P1

**Versao:** 0.5

## Versao 0.6 - Notificacoes essenciais

### BE-024 - Notificacao de convite para grupo

**Objetivo:** Avisar o usuario quando ele for convidado para um grupo.

**Descricao:** O usuario deve receber uma notificacao util quando houver convite pendente.

**Regras de negocio:**

- Notificacao deve estar associada a um convite real.
- Notificacao nao deve expor dados sensiveis do grupo.
- Notificacao deve evitar linguagem de engajamento artificial.
- Usuario deve conseguir agir sobre o convite.

**Criterios de aceite:**

- Usuario convidado deve receber notificacao.
- Notificacao deve abrir fluxo de convite.
- Convite invalido nao deve gerar acao valida.
- Conteudo da notificacao deve ser claro e moderado.

**Prioridade:** P0

**Versao:** 0.6

### BE-025 - Lembrete de encontro

**Objetivo:** Ajudar o usuario a nao esquecer encontros importantes.

**Descricao:** O usuario deve receber lembrete antes de um encontro do grupo.

**Regras de negocio:**

- Lembrete deve estar ligado a encontro real.
- Apenas membros do grupo devem receber lembrete.
- Encontros cancelados nao devem gerar lembrete.
- O lembrete deve ser util e nao excessivo.

**Criterios de aceite:**

- Membro deve receber lembrete antes do encontro.
- Usuario externo nao deve receber lembrete.
- Encontro cancelado nao deve gerar lembrete.
- Notificacao deve abrir detalhes do encontro.

**Prioridade:** P0

**Versao:** 0.6

### BE-026 - Configuracoes simples de notificacao

**Objetivo:** Dar controle basico ao usuario sobre interrupcoes.

**Descricao:** O usuario deve conseguir controlar notificacoes essenciais quando a funcionalidade estiver disponivel.

**Regras de negocio:**

- Configuracoes devem ser simples.
- Usuario deve controlar suas proprias preferencias.
- Preferencias nao devem afetar outros membros.
- Algumas notificacoes criticas podem exigir regra especifica futura.

**Criterios de aceite:**

- Usuario deve visualizar preferencias de notificacao.
- Usuario deve alterar preferencias proprias.
- Preferencias devem ser respeitadas em novas notificacoes.
- Interface deve evitar complexidade desnecessaria.

**Prioridade:** P1

**Versao:** 0.6

## Versao 0.7 - Pessoas frequentes e tipos de encontro

Status: **concluida e oficialmente fechada em `2026-07-16`**.

### BE-027 - Pessoas frequentes e convite rapido

**Objetivo:** Reduzir o trabalho de convidar pessoas recorrentes sem criar grupos obrigatorios.

**Descricao:** O usuario deve visualizar sugestoes derivadas de encontros anteriores e convidar uma pessoa frequente por uma acao explicita.

**Regras de negocio:**

- Pessoa frequente nao recebe acesso automatico.
- Apenas o organizador pode realizar o convite.
- Sugestoes devem considerar apenas encontros acessiveis ao usuario.
- Convites duplicados devem ser rejeitados.

**Criterios de aceite:**

- Usuario deve visualizar pessoas frequentes elegiveis.
- Usuario deve confirmar o convite rapido.
- Pessoa nao convidada deve continuar sem acesso ao encontro.
- Convite manual por e-mail deve continuar disponivel.

**Prioridade:** P0

**Versao:** 0.7

**Status:** Concluido

### BE-028 - Tipo opcional de encontro

**Objetivo:** Ajudar o usuario a reconhecer e organizar encontros sem adicionar etapas obrigatorias.

**Descricao:** O encontro pode receber uma classificacao simples e opcional, exibida nos pontos essenciais da experiencia.

**Regras de negocio:**

- Tipo deve ser opcional.
- Valor vazio deve ser tratado como ausencia de tipo.
- Tipo nao deve alterar permissoes, convites ou presencas.
- Encontros existentes devem continuar validos sem tipo.

**Criterios de aceite:**

- Usuario deve criar encontro com ou sem tipo.
- Organizador deve alterar ou remover o tipo.
- Tipo deve aparecer nos detalhes quando informado.
- Remocao deve persistir ao reabrir o encontro.

**Prioridade:** P1

**Versao:** 0.7

**Status:** Concluido

## Versao 1.0 - Produto publico inicial

### BE-029 - Revisao geral de usabilidade

**Objetivo:** Garantir que a experiencia esteja simples antes do lancamento publico.

**Descricao:** Revisar fluxos principais para reduzir atrito, passos desnecessarios e inconsistencias visuais.

**Regras de negocio:**

- Fluxos principais devem priorizar poucos toques.
- Telas devem ter objetivo claro.
- Estados de vazio, erro e carregamento devem existir nos fluxos principais.
- Melhorias nao devem mudar a identidade do produto.

**Criterios de aceite:**

- Fluxos de conta, grupo, convite, encontro e memoria devem ser revisados.
- Problemas criticos de usabilidade devem ser registrados.
- Ajustes aprovados devem preservar simplicidade.
- QA deve validar fluxos principais.

**Prioridade:** P0

**Versao:** 1.0

### BE-030 - Revisao de seguranca e privacidade

**Objetivo:** Garantir que grupos e memorias estejam protegidos antes do lancamento.

**Descricao:** Revisar autenticacao, autorizacao, acesso por grupo, midias, logs e dados pessoais.

**Regras de negocio:**

- Usuario nao pode acessar dados de grupos externos.
- Logs nao devem expor dados sensiveis.
- Midias devem respeitar controle de acesso.
- Tokens devem seguir regras de seguranca definidas.

**Criterios de aceite:**

- Fluxos privados devem exigir autenticacao.
- Acesso entre grupos deve ser bloqueado.
- Testes ou validacoes devem cobrir cenarios criticos.
- Riscos encontrados devem ser tratados antes da publicacao.

**Prioridade:** P0

**Versao:** 1.0

### BE-031 - Preparacao para publicacao

**Objetivo:** Preparar o produto para uso por grupos reais.

**Descricao:** Consolidar ajustes finais de produto, suporte basico, estabilidade, performance e publicacao nas lojas.

**Regras de negocio:**

- Produto deve estar coerente com a visao oficial.
- Funcionalidades essenciais devem estar completas.
- Experiencia deve ser estavel em fluxos principais.
- Publicacao deve ocorrer apenas apos validacao de qualidade.

**Criterios de aceite:**

- Fluxos principais devem estar funcionando.
- Documentacao essencial deve estar atualizada.
- Problemas criticos devem estar resolvidos.
- Produto deve estar pronto para teste com usuarios reais.

**Prioridade:** P0

**Versao:** 1.0

## Versao 1.1 - Repertorio do Encontro

### BE-032 - Repertorio privado e musicas do encontro

**Objetivo:** Centralizar a preparacao musical no encontro.

**Descricao:** Participantes autorizados devem consultar um repertorio opcional com musicas e detalhes progressivos.

**Regras de negocio:**

- Repertorio deve pertencer a um unico encontro.
- Encontro continua completo sem repertorio.
- Apenas participantes autorizados podem acessar.
- Nome da musica e o unico campo obrigatorio.
- Tom, capotraste, artista, links e observacoes sao opcionais.
- Links devem usar HTTPS.
- Letras e cifras de terceiros nao devem ser armazenadas.

**Criterios de aceite:**

- Organizador deve ativar o repertorio explicitamente.
- Participante autorizado deve visualizar as musicas.
- Musica deve ser criada apenas com nome.
- Campos opcionais devem poder ser alterados.
- Usuario externo nao deve acessar o repertorio.

**Prioridade:** P0

**Versao:** 1.1

**Status:** Planejado

### BE-033 - Participantes e funcoes nas musicas

**Objetivo:** Deixar claro quem canta, quem toca e qual instrumento sera utilizado.

**Descricao:** Musicas podem associar participantes do encontro a funcoes musicais opcionais.

**Regras de negocio:**

- Somente participantes ativos do encontro podem ser associados.
- Uma pessoa pode cantar, tocar ou exercer ambas as funcoes.
- Instrumento e opcional e pertence a funcao de tocar.
- Remover uma participacao nao remove a musica.
- Remocao do participante do encontro deve impedir novas associacoes.

**Criterios de aceite:**

- Organizador deve selecionar participantes da lista do encontro.
- Uma musica deve aceitar mais de um cantor ou instrumentista.
- Funcao e instrumento devem aparecer nos detalhes.
- Associacao invalida deve ser rejeitada.

**Prioridade:** P1

**Versao:** 1.1

**Status:** Planejado

### BE-034 - Ordenacao e execucao do setlist

**Objetivo:** Organizar a ordem planejada e registrar o que realmente foi tocado.

**Descricao:** O setlist deve ser uma selecao ordenada de musicas do repertorio do encontro.

**Regras de negocio:**

- Organizador controla inclusao, remocao e ordem na primeira versao.
- Reordenacao deve ser atomica.
- Item pode estar Planejado, Tocado ou Pulado.
- Reprise intencional da mesma musica deve ser permitida.
- Acoes concorrentes nao podem criar ordens duplicadas.
- Marcar como tocada ou pulada deve ser idempotente.

**Criterios de aceite:**

- Organizador deve montar e reordenar o setlist.
- Ordem deve persistir ao reabrir o encontro.
- Item deve poder ser marcado como tocado ou pulado.
- Participante deve acompanhar o setlist atualizado.

**Prioridade:** P0

**Versao:** 1.1

**Status:** Planejado

### BE-035 - Resumo do setlist na memoria

**Objetivo:** Preservar o repertorio realizado como parte da historia do encontro.

**Descricao:** A memoria deve projetar o setlist final sem duplicar os dados como uma memoria comum.

**Regras de negocio:**

- Resumo deve separar musicas tocadas, planejadas e puladas.
- Apenas usuarios com acesso ao encontro podem consultar.
- Links externos abrem somente por acao consciente.
- Edicoes posteriores nao devem falsificar o registro do que foi executado.

**Criterios de aceite:**

- Memoria deve exibir o setlist realizado.
- Musicas tocadas devem ter destaque sobre as nao executadas.
- Usuario externo nao deve acessar o resumo.
- Feed nao deve receber uma publicacao para cada alteracao de musica.

**Prioridade:** P1

**Versao:** 1.1

**Status:** Planejado

### BE-036 - Sugestao contextual de repertorio

**Objetivo:** Ajudar encontros musicais a descobrirem o repertorio sem aumentar o formulario de criacao.

**Descricao:** O tipo do encontro pode sugerir a ativacao do repertorio depois que o encontro for criado.

**Regras de negocio:**

- Tipo nunca ativa repertorio automaticamente.
- Sugestao deve ser discreta e dispensavel.
- Usuario pode usar repertorio em encontro de qualquer tipo.
- Ignorar a sugestao nao reduz funcionalidades do encontro.
- Sugestao nao deve reaparecer de forma insistente.

**Criterios de aceite:**

- Encontro musical deve apresentar uma sugestao simples.
- Organizador deve aceitar ou dispensar a sugestao.
- Encontro sem tipo deve continuar podendo ativar repertorio.
- Escolha nao deve alterar participantes ou permissoes.

**Prioridade:** P2

**Versao:** 1.1

**Status:** Planejado

## Versao 1.2 - Karaoke do Encontro

### BE-037 - Sessao e inscricao voluntaria no karaoke

**Objetivo:** Preparar o karaoke sem inscrever ou constranger pessoas automaticamente.

**Descricao:** O encontro pode possuir uma sessao opcional com musicas e participantes inscritos voluntariamente.

**Regras de negocio:**

- Deve existir no maximo uma sessao ativa por encontro.
- Participar do encontro nao implica inscricao no karaoke.
- Apenas participantes confirmados podem se inscrever.
- Usuario pode entrar ou sair sem justificativa.
- Sessao encerrada permanece privada e consultavel.
- Usuario externo nao pode visualizar inscritos ou musicas.

**Criterios de aceite:**

- Organizador deve criar a sessao explicitamente.
- Participante deve escolher se quer cantar.
- Inscricao deve poder ser retirada antes da apresentacao.
- Sessao encerrada deve bloquear alteracoes.

**Prioridade:** P0

**Versao:** 1.2

**Status:** Planejado

### BE-038 - Fila e sorteio justo

**Objetivo:** Distribuir oportunidades sem repeticoes injustas ou discussoes sobre a ordem.

**Descricao:** O sistema deve montar e avancar uma fila baseada em participantes inscritos e musicas disponiveis.

**Regras de negocio:**

- Todos os inscritos disponiveis participam antes de uma repeticao.
- A mesma pessoa nao deve repetir imediatamente.
- Musica nao deve repetir enquanto houver alternativa.
- Sorteio confirmado nao pode ser refeito silenciosamente.
- Novo ciclo pode iniciar quando as alternativas forem esgotadas.
- Operacoes simultaneas devem usar controle otimista de concorrencia.

**Criterios de aceite:**

- Sorteio deve escolher apenas inscrito elegivel.
- Fila deve permanecer consistente depois de atualizacao concorrente.
- Participantes devem visualizar pessoa e musica atuais.
- Regras de repeticao devem possuir testes deterministas.

**Prioridade:** P0

**Versao:** 1.2

**Status:** Planejado

### BE-039 - Conducao das apresentacoes

**Objetivo:** Manter o karaoke fluido durante troca, pulo e conclusao de apresentacoes.

**Descricao:** O organizador deve conduzir os estados da fila por acoes simples e visiveis.

**Regras de negocio:**

- Apresentacao pode ser Realizada, Pulada ou Cancelada.
- Musica pode ser trocada antes da confirmacao.
- Pulo nao gera penalidade, Marco negativo ou publicacao.
- Ajuste manual deve ficar visivel aos participantes.
- Transicao invalida deve ser rejeitada.
- Repeticao da mesma acao nao deve corromper o estado.

**Criterios de aceite:**

- Organizador deve marcar apresentacao como realizada.
- Participante deve trocar musica ou pular sem exposicao.
- Fila deve avancar corretamente em cada estado.
- Feed nao deve publicar recusas, trocas ou pulos.

**Prioridade:** P0

**Versao:** 1.2

**Status:** Planejado

### BE-040 - Duplas com consentimento

**Objetivo:** Permitir apresentacoes em dupla preservando autonomia.

**Descricao:** Duas pessoas podem compartilhar uma apresentacao somente depois de consentimento explicito.

**Regras de negocio:**

- Dupla exige aceite das duas pessoas.
- Recusa nao aparece no feed ou na memoria.
- Retirada de consentimento antes da apresentacao desfaz a dupla.
- Pessoa nao inscrita nao pode ser adicionada silenciosamente.

**Criterios de aceite:**

- Convite para dupla deve poder ser aceito ou recusado.
- Dupla so deve entrar na fila depois dos dois aceites.
- Retirada deve preservar a fila sem expor motivo.

**Prioridade:** P1

**Versao:** 1.2

**Status:** Planejado

### BE-041 - Resumo privado do karaoke

**Objetivo:** Guardar uma lembranca positiva do que foi cantado.

**Descricao:** A memoria do encontro deve apresentar musicas e apresentacoes realizadas sem destacar recusas ou pulos.

**Regras de negocio:**

- Resumo inclui somente apresentacoes realizadas.
- Participante aparece apenas com consentimento.
- Trocas, recusas e pulos nao devem ter destaque.
- Resumo permanece privado no encontro.
- Dados devem ser projetados da sessao, sem memoria duplicada.

**Criterios de aceite:**

- Encerramento deve produzir resumo consultavel.
- Musicas realizadas devem aparecer em ordem.
- Participante que retirar consentimento nao deve aparecer nominalmente.
- Usuario externo nao deve acessar o resumo.

**Prioridade:** P1

**Versao:** 1.2

**Status:** Planejado

## Versao 1.3 - Destaques do Encontro

Esta versao depende de pesquisa previa que confirme que a experiencia e percebida como reconhecimento, nao como concurso.

### BE-042 - Edicao de Destaques e elegibilidade

**Objetivo:** Criar uma dinamica de reconhecimento com regras estaveis e seguras.

**Descricao:** Um encontro realizado pode possuir uma edicao opcional de Destaques com categorias positivas controladas.

**Regras de negocio:**

- Destaques so podem abrir em encontro Realizado.
- Deve existir no maximo uma edicao por encontro.
- Elegiveis devem ser participantes ativos e confirmados.
- Elegibilidade deve ser congelada na abertura.
- Participante pode optar por nao votar e por nao receber votos.
- Somente categorias positivas do catalogo podem ser usadas.
- Categorias personalizadas ficam fora da primeira versao.

**Criterios de aceite:**

- Organizador deve configurar e abrir a edicao.
- Encontro cancelado deve rejeitar abertura.
- Usuario externo nao deve acessar elegiveis.
- Alteracao posterior de participantes nao deve mudar silenciosamente a edicao aberta.

**Prioridade:** P0

**Versao:** 1.3

**Status:** Planejado sob validacao de produto

### BE-043 - Votacao secreta e opcional

**Objetivo:** Permitir reconhecimento sem pressao, exposicao ou autovoto.

**Descricao:** Cada elegivel pode escolher uma pessoa por categoria enquanto a votacao estiver aberta.

**Regras de negocio:**

- Deve existir no maximo um voto por votante e categoria.
- Autovoto deve ser rejeitado.
- Voto pode ser alterado ou removido enquanto a edicao estiver aberta.
- Votar e opcional.
- A API nunca revela quem votou em quem.
- Organizador nao pode criar ou editar voto de terceiros.
- Logs nao devem registrar conteudo sensivel do voto.

**Criterios de aceite:**

- Participante deve votar, alterar e remover o proprio voto.
- Segundo voto na mesma categoria deve substituir ou ser rejeitado de forma consistente.
- Identidade do votante nao deve aparecer em resposta ou feed.
- Voto depois do encerramento deve ser rejeitado.

**Prioridade:** P0

**Versao:** 1.3

**Status:** Planejado sob validacao de produto

### BE-044 - Encerramento e resultado protegido

**Objetivo:** Entregar reconhecimento sem produzir placar ou lista de perdedores.

**Descricao:** Resultados sao calculados apenas no encerramento e exibem somente pessoas reconhecidas.

**Regras de negocio:**

- Resultado so aparece depois do encerramento.
- Categoria exige pelo menos tres votantes.
- Empates sao compartilhados sem desempate artificial.
- Resultado nao mostra contagem, porcentagem ou pessoas sem votos.
- Encerramento torna votos e resultados imutaveis.
- Cancelamento nao publica resultado.

**Criterios de aceite:**

- Resultado ao vivo deve permanecer indisponivel.
- Categoria sem participacao minima nao deve expor resultado.
- Empatados devem receber o mesmo reconhecimento.
- Organizador nao deve alterar o resultado.

**Prioridade:** P0

**Versao:** 1.3

**Status:** Planejado sob validacao de produto

### BE-045 - Destaques na memoria do encontro

**Objetivo:** Preservar o reconhecimento no contexto da lembranca que o originou.

**Descricao:** A memoria deve projetar os resultados encerrados de forma privada e sem duplicar votos.

**Regras de negocio:**

- Resultado aparece somente para quem acessa o encontro.
- Votos individuais e quantidades nunca aparecem.
- Destaques nao viram publicacao publica ou memoria independente.
- Resultado cancelado ou insuficiente nao deve ser exibido.

**Criterios de aceite:**

- Memoria deve mostrar categoria e pessoa reconhecida.
- Usuario externo nao deve descobrir o resultado ou o encontro.
- Feed deve receber no maximo um resumo de encerramento.

**Prioridade:** P1

**Versao:** 1.3

**Status:** Planejado sob validacao de produto

## Versao 1.4 - Marcos do Perfil

Esta versao depende de a v1.3 nao apresentar sinais de competicao, exclusao ou spam.

### BE-046 - Concessao automatica e idempotente de Marcos

**Objetivo:** Preservar acontecimentos significativos da trajetoria do usuario.

**Descricao:** O sistema concede poucos Marcos por fatos positivos e verificaveis, sem premiar volume de uso.

**Regras de negocio:**

- Concessao deve ser deterministica e idempotente.
- O mesmo fato nao concede o mesmo Marco duas vezes.
- Catalogo inicial deve ser pequeno e versionado.
- Marcos nao geram pontos, niveis, progresso ou ranking.
- Marcos como `Nunca falta` ou baseados em volume sao proibidos.
- Origem deve ser preservada de forma privada para auditoria.

**Criterios de aceite:**

- Fato elegivel deve conceder um unico Marco.
- Reprocessamento nao deve duplicar concessao.
- Fato nao elegivel nao deve criar Marco.
- Catalogo nao deve depender de painel administrativo inicial.

**Prioridade:** P0

**Versao:** 1.4

**Status:** Planejado sob validacao da v1.3

### BE-047 - Consulta privada e invalidacao de Marcos

**Objetivo:** Permitir que o usuario revisite sua historia com dados confiaveis.

**Descricao:** O usuario deve consultar os proprios Marcos ativos e abrir sua origem apenas quando ainda possuir acesso.

**Regras de negocio:**

- Usuario visualiza todos os proprios Marcos ativos.
- Marcos futuros nao aparecem como checklist.
- Marco invalidado na origem deixa de ser exibido quando aplicavel.
- Origem so abre com autorizacao atual.
- Remocao de acesso ao encontro deve preservar privacidade.

**Criterios de aceite:**

- Usuario deve listar os proprios Marcos.
- Outro usuario nao deve listar o historico completo.
- Origem inacessivel nao deve revelar encontro, data ou participantes.
- Marco inativo nao deve aparecer no perfil.

**Prioridade:** P0

**Versao:** 1.4

**Status:** Planejado sob validacao da v1.3

### BE-048 - Selecao de Marcos destacados

**Objetivo:** Dar expressao pessoal sem incentivar acumulacao ou comparacao.

**Descricao:** O usuario pode escolher e ordenar ate tres Marcos para a previa do perfil.

**Regras de negocio:**

- Apenas Marcos ativos do proprio usuario podem ser selecionados.
- Limite maximo e tres.
- Alteracao substitui a selecao de forma atomica.
- Usuario pode remover todos os destaques.
- Nao deve existir total publico de Marcos.

**Criterios de aceite:**

- Usuario deve selecionar, ordenar e remover destaques.
- Quarto Marco deve ser rejeitado ou exigir substituicao clara.
- Ordem deve persistir.
- Marco inativo deve sair da selecao.

**Prioridade:** P0

**Versao:** 1.4

**Status:** Planejado sob validacao da v1.3

### BE-049 - Visualizacao contextual de Marcos

**Objetivo:** Permitir reconhecimento entre pessoas sem criar perfil publico ou vazar encontros.

**Descricao:** Terceiros visualizam somente Marcos escolhidos dentro de um encontro compartilhado.

**Regras de negocio:**

- Terceiro precisa compartilhar encontro acessivel com o usuario.
- Resposta omite origem, data e encontro sem autorizacao.
- Nao deve existir endpoint de perfil publico pesquisavel.
- Marcos derivados de Destaques dependem da validacao explicita da v1.3.
- Visualizacao nao mostra quantidade, nivel ou raridade.

**Criterios de aceite:**

- Participante deve ver ate tres Marcos destacados no contexto permitido.
- Usuario externo nao deve consultar Marcos.
- Marco nao deve revelar encontro privado pela descricao.
- Remocao do contexto compartilhado deve revogar a visualizacao.

**Prioridade:** P0

**Versao:** 1.4

**Status:** Planejado sob validacao da v1.3

### BE-050 - Localizacao fixa do encontro

**Objetivo:** Manter o destino combinado acessivel nas informacoes permanentes do encontro.

**Descricao:** O organizador informa um nome ou endereco e pode anexar uma coordenada capturada uma unica vez. Participantes autorizados podem abrir o ponto no mapa.

**Regras de negocio:**

- local permanece opcional
- somente organizador altera ou remove
- coordenadas exigem descricao legivel e par completo
- captura depende de acao e permissao explicitas
- nao existe rastreamento em tempo real
- local textual continua funcionando sem coordenadas

**Criterios de aceite:**

- local persiste depois de recarregar o encontro
- participante autorizado abre o ponto no mapa
- permissao negada nao bloqueia preenchimento textual
- usuario externo nao acessa as coordenadas

**Prioridade:** P0

**Versao:** 1.0.1

**Status:** Em implementacao

## Funcionalidades futuras para avaliacao

As funcionalidades abaixo nao fazem parte do escopo inicial detalhado.

Elas podem ser avaliadas em versoes futuras:

- estatisticas do grupo
- retrospectiva anual
- exportacao de memorias
- backup avancado
- convites por link com controles adicionais
- compartilhamento de convite pelo WhatsApp com controle de acesso
- modelos de encontros
- registros de desafios ou atividades para jogos, estudos ou esportes
- campos opcionais por tipo de encontro
- integracao com calendario
- busca avancada em memorias
- permissoes avancadas por membro
- recursos para eventos maiores
- monetizacao

Cada uma dessas ideias devera passar pelos criterios definidos em `03-regras-de-produto.md` antes de entrar em uma versao.

## Dependencias entre documentos

Se este backlog for alterado, os seguintes documentos podem precisar de revisao:

- `01-roadmap-versoes.md`, quando a alteracao mudar versoes, ordem ou escopo.
- `02-arquitetura-inicial.md`, quando a alteracao exigir novas decisoes tecnicas.
- `03-regras-de-produto.md`, quando a alteracao mudar comportamento ou regra geral do produto.

## Observacoes finais

Este backlog e inicial.

Ele deve guiar a evolucao do Projeto Encontros, mas pode ser refinado conforme novas decisoes forem tomadas.

Qualquer refinamento deve preservar a identidade definida em `00-visao-produto.md`.
