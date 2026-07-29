# Projeto Encontros - Regras de Produto

## Objetivo do documento

Este documento define as regras gerais de produto do Projeto Encontros.

Ele existe para orientar decisoes de comportamento, limites, permissoes, privacidade, experiencia e consistencia funcional.

Este documento nao detalha todas as funcionalidades do backlog. O detalhamento de cada funcionalidade, com objetivo, descricao, criterios de aceite, prioridade e versao, devera ser feito em `04-backlog-inicial.md`.

## Referencias oficiais

As regras deste documento devem respeitar:

- `00-visao-produto.md`
- `01-roadmap-versoes.md`
- `02-arquitetura-inicial.md`
- `05-proposta-modulos-futuros.md`

Se alguma regra entrar em conflito com a visao do produto, a visao deve prevalecer.

## Regra principal do produto

O Projeto Encontros existe para ajudar pessoas a criar encontros privados, convidar as pessoas certas, organizar presencas e guardar memorias desses momentos.

Qualquer funcionalidade, tela, notificacao, fluxo ou decisao de produto deve fortalecer pelo menos um destes pilares:

- organizacao
- encontros
- memorias
- amizade
- simplicidade
- experiencia do usuario

Se uma ideia nao fortalecer nenhum desses pilares, ela deve ser questionada antes de ser aceita.

## Identidade e limites

O Projeto Encontros nao deve se tornar:

- rede social
- aplicativo de mensagens
- feed publico
- plataforma de seguidores
- substituto de WhatsApp, Discord, Telegram ou Instagram
- produto voltado a consumo excessivo de tela

Funcionalidades com caracteristicas de rede social devem ser rejeitadas ou redesenhadas para preservar a identidade do produto.

Exemplos de funcionalidades que devem ser evitadas:

- seguidores
- perfis publicos
- feed aberto
- curtidas publicas
- comentarios publicos
- ranking de popularidade
- recomendacao de grupos publicos
- mensagens diretas como funcionalidade central

## Privacidade

O produto deve ser privado por padrao.

Regras:

- um encontro deve ser visivel apenas para seu criador, organizadores e participantes autorizados
- dados de um encontro nao devem aparecer para usuarios externos
- a existencia de um encontro privado nao deve ser exposta para quem nao foi convidado
- memorias devem pertencer ao encontro em que foram registradas
- fotos, videos e postagens devem respeitar o controle de acesso do encontro
- usuarios nao devem acessar dados de encontros dos quais nao participam
- convites de encontro devem ser controlados e rastreaveis
- grupos, turmas ou listas de pessoas frequentes nao devem conceder acesso automatico a todos os encontros
- informacoes pessoais devem ser exibidas somente quando houver necessidade clara

Privacidade nao e uma funcionalidade futura. Ela e parte central da proposta do produto.

## Usuarios

Todo usuario deve possuir uma identidade propria no aplicativo.

Regras:

- um usuario deve conseguir criar conta
- um usuario deve conseguir autenticar-se com seguranca
- um usuario pode participar de mais de um encontro
- um usuario deve visualizar apenas encontros dos quais participa ou para os quais foi convidado
- um usuario pode ter papeis diferentes em encontros diferentes
- um usuario nao deve ser obrigado a expor informacoes publicas desnecessarias

Dados minimos do usuario devem ser priorizados no inicio.

Dados adicionais so devem ser solicitados quando tiverem valor claro para o produto.

## Encontros como unidade central

Encontro e a unidade central do Projeto Encontros.

Regras:

- todo encontro deve ter um criador responsavel
- todo encontro deve possuir titulo claro
- todo encontro deve ter data e horario
- encontro pode ter local
- encontro pode ter descricao simples
- encontro deve possuir participantes controlados por convite ou permissao equivalente
- toda presenca deve pertencer a um encontro
- toda memoria deve pertencer a um encontro
- todo mural ou postagem deve pertencer a um encontro
- toda lista colaborativa deve pertencer a um encontro
- um encontro deve ser privado por padrao

O encontro deve representar um momento real que as pessoas pretendem viver ou ja viveram juntas.

O produto nao deve incentivar descoberta publica de encontros.

## Grupos, turmas e pessoas frequentes

Grupos, turmas ou circulos de pessoas podem existir no futuro como apoio para facilitar convites recorrentes.

Eles nao devem ser tratados como a unidade central do produto.

Regras:

- um grupo, turma ou circulo nao deve ser obrigatorio para criar um encontro
- participar de um grupo, turma ou circulo nao deve dar acesso automatico a todos os encontros
- cada encontro deve continuar controlando seus proprios participantes
- o produto nao deve expor encontros privados para todo um grupo quando o organizador convidou apenas algumas pessoas
- qualquer funcionalidade de grupo deve existir para reduzir trabalho de convite, nao para substituir a privacidade do encontro

## Papeis e permissoes

As permissoes devem comecar simples.

Papeis iniciais recomendados:

- organizador do encontro
- participante do encontro

Papel futuro possivel:

- coorganizador do encontro

### Organizador do encontro

O organizador do encontro e o responsavel principal pelo encontro.

Regras iniciais:

- pode editar dados principais do encontro
- pode convidar participantes
- pode remover participantes
- pode gerenciar convites
- pode cancelar ou concluir o encontro quando essa funcionalidade existir

### Participante do encontro

O participante faz parte de um encontro especifico.

Regras iniciais:

- pode visualizar o encontro para o qual foi convidado ou aceito
- pode responder presenca
- pode visualizar participantes do encontro quando autorizado
- pode participar de memorias, mural e listas do encontro quando essas funcionalidades existirem

### Coorganizador do encontro

O coorganizador deve ser adicionado apenas quando houver necessidade real.

Adicionar coorganizadores cedo demais pode criar complexidade desnecessaria.

## Convites

Convites devem permitir entrada controlada em encontros privados.

Regras:

- um usuario so deve entrar em um encontro por convite ou permissao valida
- convites devem estar associados a um encontro
- convites devem ter estado identificavel
- convites podem expirar, caso essa decisao seja adotada
- convites aceitos nao devem ser reutilizados de forma insegura
- convites nao devem expor dados sensiveis do encontro para pessoas externas

Estados candidatos de convite:

- pendente
- aceito
- recusado
- expirado
- cancelado

A regra final de expiracao devera ser detalhada no backlog quando a funcionalidade for planejada.

## Encontros

Encontro e o principal objeto do produto.

Regras:

- encontro deve ter data e horario
- encontro pode ter local
- encontro pode ter descricao simples
- encontro deve possuir um estado
- encontro deve ter participantes proprios
- participantes convidados podem confirmar presenca
- usuarios externos nao podem visualizar encontros para os quais nao foram convidados
- encontro pode ter uma origem opcional em grupo, turma ou circulo futuro, mas isso nao deve ser obrigatorio

Estados candidatos de encontro:

- planejado
- confirmado
- cancelado
- realizado

O estado final devera ser definido com cuidado para manter o fluxo simples.

Encontros podem ter mural ou memorias proprias, mas nao devem virar conversas longas nem feeds publicos.

## Presencas

Confirmacao de presenca deve ser simples.

Regras:

- apenas participantes convidados ou autorizados do encontro podem confirmar presenca
- a presenca deve estar associada a um encontro
- cada participante deve ter no maximo uma resposta ativa por encontro
- a resposta do participante pode ser alterada enquanto o encontro permitir
- a lista de presencas deve ser visivel apenas para participantes autorizados do encontro

Estados candidatos de presenca:

- convidado
- confirmado
- talvez
- nao vai

Para a primeira versao de encontros, pode ser suficiente iniciar apenas com confirmado e nao confirmado.

## Memorias

Memorias devem registrar momentos vividos em encontros reais.

Regras:

- memorias devem estar associadas a um encontro
- fotos e videos devem respeitar permissoes do encontro
- memorias nao devem ser publicas por padrao
- memorias devem ser organizadas para facilitar consulta futura
- memorias devem preservar contexto, como encontro, data ou descricao

Memorias nao devem virar feed publico.

Interacoes sociais sobre memorias devem ser avaliadas com rigor para nao desviar a identidade do produto.

## Listas colaborativas

Listas colaborativas devem ajudar na preparacao dos encontros.

Regras:

- listas devem estar associadas a um encontro
- itens devem ter descricao clara
- itens podem possuir responsavel
- itens podem possuir estado de conclusao
- a lista deve reduzir confusao e combinados perdidos

Listas nao devem se transformar em sistema complexo de tarefas.

Funcionalidades como pagamentos, compras e divisao financeira devem ser avaliadas futuramente e nao fazem parte da regra inicial.

## Linha do tempo

A linha do tempo deve contar a historia privada dos encontros do usuario.

Regras:

- deve mostrar encontros e memorias que o usuario tem permissao para acessar
- deve respeitar privacidade
- deve priorizar encontros e memorias relevantes
- deve facilitar revisitar momentos passados
- nao deve funcionar como feed publico
- nao deve usar algoritmos de engajamento social

A linha do tempo deve reforcar nostalgia, amizade e pertencimento, nao competicao por atencao.

## Notificacoes

Notificacoes devem ser uteis e moderadas.

Regras:

- notificacoes devem ajudar o usuario a participar melhor dos encontros
- notificacoes devem evitar excesso de interrupcoes
- notificacoes devem estar ligadas a acoes importantes
- notificacoes nao devem ser usadas para engajamento artificial
- notificacoes devem respeitar configuracoes do usuario quando existirem

Notificacoes permitidas inicialmente:

- convite para encontro
- novo encontro
- lembrete antes do encontro
- alteracao relevante no encontro
- responsabilidade atribuida ao usuario

Notificacoes proibidas ou desencorajadas:

- "voce esta perdendo algo" sem acao real
- alertas de popularidade
- alertas para gerar uso compulsivo
- notificacoes baseadas em curtidas ou competicao social

## Modulos opcionais do encontro

Recursos especializados devem permanecer subordinados a um encontro.

Regras:

- todo modulo especializado deve ser opcional
- modulo deve ser ativado por acao explicita do organizador
- encontro deve continuar completo e utilizavel sem modulo especializado
- tipo do encontro pode sugerir recurso, mas nunca ativa-lo automaticamente
- tipo nao pode alterar permissoes, inscrever participantes ou criar dados de outro modulo
- ativar modulo nao amplia as permissoes do encontro
- toda consulta ou operacao deve validar a autorizacao atual no encontro
- usuario removido deve perder acesso aos dados e resumos do modulo
- participacao anterior, grupo ou registro historico nao concede acesso atual
- modulo nao deve criar perfil publico, descoberta de conteudo ou acesso fora do encontro
- modulo especializado nao deve exigir nova aba global
- desativacao, encerramento e exclusao devem possuir comportamento explicito para dados historicos
- dados do modulo nao podem revelar direta ou indiretamente a existencia de encontro privado

Publicacoes automaticas devem ser poucas, privadas e ligadas a acontecimentos relevantes.

Recusas, ausencias, pulos, alteracoes operacionais e retirada de consentimento nao devem ser publicados.

## Repertorio e conteudo musical

Repertorio deve organizar musica sem transformar o produto em repositorio de conteudo protegido.

Regras:

- repertorio pertence a um unico encontro
- apenas participantes autorizados podem consultar
- participantes podem sugerir musicas quando a versao permitir
- organizador controla o setlist final na primeira entrega
- associar pessoa como cantora ou instrumentista exige participacao ativa e consentimento compativel
- associacao falsa, ironica ou constrangedora deve poder ser contestada e corrigida
- remocao do encontro impede novas associacoes da pessoa
- historico deve definir de forma clara quando preservar, remover ou anonimizar um nome
- ordem do setlist deve ser consistente mesmo com alteracoes simultaneas

O produto pode armazenar:

- nome da musica
- artista ou categoria
- tom
- capotraste
- participantes e funcoes autorizadas
- links externos
- observacoes proprias e curtas

O produto nao deve copiar, importar, raspar, reproduzir, anexar ou armazenar sem autorizacao:

- letras de terceiros
- cifras de terceiros
- partituras de terceiros
- videos de terceiros
- outros conteudos protegidos

Observacoes nao podem ser usadas para contornar essa proibicao por meio de conteudo dividido em partes.

Links externos devem:

- usar esquema seguro permitido
- abrir apenas depois de acao consciente do usuario
- nao receber token, nome de participante ou contexto privado desnecessario
- nao gerar previa automatica que consulte o destino sem necessidade e informacao adequada
- permitir tratamento de destino malicioso, redirecionamento abusivo ou rastreamento indevido

Conteudo potencialmente infrator ou malicioso deve possuir processo de denuncia, bloqueio e remocao antes da publicacao do modulo.

## Karaoke e consentimento

Karaoke deve apoiar a diversao sem obrigar, avaliar ou constranger participantes.

Regras:

- participar do encontro nao implica inscricao no karaoke
- pessoa nao inscrita nao pode ser sorteada ou adicionada a apresentacao
- inscricao deve ser explicita, informada e revogavel
- participar de dupla exige consentimento das duas pessoas
- consentimento para inscricao, dupla, feed, memoria e Marco deve ser tratado separadamente
- silencio nao representa consentimento
- recusar, sair, trocar musica ou pular nao exige justificativa
- recusa, saida, troca ou pulo nao gera exposicao, penalidade, notificacao social ou Marco negativo
- consentimento para dupla pode ser retirado antes da apresentacao sem expor motivo ou autoria da recusa
- identificacao no resumo final depende de consentimento especifico
- retirada de consentimento deve possuir prazo e efeito definidos antes da implementacao
- organizador nao pode associar participante ou marcar apresentacao falsa sem mecanismo de correcao
- ajustes manuais na fila devem ser visiveis para evitar favorecimento silencioso
- acoes simultaneas nao podem corromper fila, ordem ou apresentacao atual

Karaoke nunca deve produzir:

- nota de desempenho
- ranking
- vencedor
- comparacao de talento
- avaliacao de aparencia ou personalidade
- estatistica publica de quantidade cantada

## Destaques do Encontro

Destaques devem representar reconhecimento afetivo, nao premiacao ou concurso de popularidade.

Regras:

- recurso deve ser opcional para o encontro e para cada participante
- pessoa pode recusar votar e receber votos sem exposicao ou perda funcional
- categorias devem ser positivas, controladas e avaliadas contra ironia, humilhacao, discriminacao e duplo sentido
- categorias negativas sao proibidas, mesmo apresentadas como brincadeira
- avaliacoes de aparencia, talento ou personalidade sao proibidas
- voto individual deve permanecer secreto para participantes e organizadores
- API, feed e logs nao devem expor quem votou em quem
- suporte comum nao deve possuir acesso ao conteudo individual dos votos
- participante nao pode votar em si mesmo
- organizador nao pode criar ou editar voto de terceiro
- resultado ao vivo e proibido
- contagem, porcentagem, ranking, lista de nao reconhecidos e desempate artificial sao proibidos
- empate deve ser aceito sem declarar um vencedor unico
- resultado deve permanecer restrito ao encontro
- resultado nao pode originar Marco ou exibicao no perfil sem consentimento adicional

Quantidade minima fixa de votos nao garante sigilo.

Resultado deve ser suprimido quando tamanho, composicao, recusas ou distribuicao permitirem inferir votos ou identificar pessoas nao reconhecidas.

Antes da implementacao, a versao deve definir:

- criterio de participacao real no encontro
- protecao contra inferencia em grupos pequenos
- janela minima de votacao
- regras de encerramento antecipado e cancelamento
- efeito da retirada de elegibilidade sobre votos existentes
- retencao, auditoria, correcao e exclusao dos votos

Confirmacao de presenca representa intencao e nao deve, sozinha, ser tratada como prova definitiva de participacao real para Destaques.

## Marcos do Perfil

Marcos devem representar acontecimentos positivos da historia do usuario, nunca produtividade social.

Regras:

- Marcos devem corresponder a fatos positivos e verificaveis
- concessao automatica deve ser deterministica, idempotente e corrigivel
- Marco pode ser invalidado quando sua origem deixar de ser valida
- usuario deve visualizar todos os proprios Marcos ativos
- exibicao a terceiros deve ser escolha explicita e revogavel do titular
- apenas Marcos selecionados pelo usuario podem aparecer para terceiros
- exibicao a terceiros deve ocorrer somente em contexto privado compartilhado e autorizado
- compartilhar um encontro nao autoriza revelar Marcos originados de outros encontros
- Marco nao pode revelar encontro, religiao, atividade, local, data, frequencia ou relacionamento privado a pessoa sem acesso
- perder acesso a origem deve bloquear seus detalhes sem revelar qual era o encontro
- Marco derivado de Destaques exige consentimento especifico
- Marco derivado de Destaques nao pode revelar votos, quantidade ou recorrencia
- criterios nao podem prejudicar pessoas por ausencia, acessibilidade, saude, distancia ou entrada recente
- dados historicos incompletos nao podem gerar comparacao injusta

Nao deve existir:

- perfil publico de Marcos
- busca global de Marcos
- total publico
- raridade
- historico comparativo
- lista publica de Marcos bloqueados
- checklist para estimular desbloqueios

Exclusao de conta, exclusao de encontro, correcao da origem e retirada de consentimento devem possuir efeito documentado antes da implementacao.

## Limites contra gamificacao

O Projeto Encontros nao deve transformar convivencia em pontuacao ou desempenho.

Sao proibidos:

- pontos
- moedas
- niveis
- experiencia
- sequencias
- missoes
- podios
- raridade
- barras de progresso
- metas por volume
- rankings explicitos ou disfarcados
- comparacao por presenca, encontros, publicacoes, fotos, votos, musicas, tarefas ou tempo de uso
- recompensa por retorno frequente ou permanencia na tela
- ordenacao visual por popularidade

Notificacoes nao podem criar urgencia, medo de perder algo ou pressao para votar, participar ou obter Marcos.

Experimentos nao podem reintroduzir essas mecanicas com outro nome.

Metricas de sucesso devem medir utilidade, seguranca, satisfacao e qualidade das memorias, nunca uso compulsivo ou volume artificial de acoes.

## Experiencia do usuario

A experiencia deve ser simples, clara e humana.

Regras:

- toda acao comum deve exigir poucos toques
- telas devem ter objetivo claro
- informacoes essenciais devem aparecer primeiro
- o produto deve ser utilizavel por pessoas comuns
- estados de vazio, erro e carregamento devem ser pensados desde o inicio
- fluxos longos devem ser questionados
- complexidade visual deve ser evitada
- componentes devem ser reutilizados
- telas duplicadas devem ser evitadas

O produto deve ser facil de usar com uma mao no mobile.

## Conteudo e tom

O tom do produto deve transmitir:

- amizade
- organizacao
- simplicidade
- cuidado
- privacidade
- nostalgia
- leveza

Textos devem ser claros, curtos e humanos.

O produto deve evitar linguagem tecnica para usuarios finais.

## Localizacao do encontro

- `Local` e a fonte oficial e permanente do destino combinado.
- O local pode ser apenas textual ou conter um ponto geografico fixo.
- A posicao do aparelho so pode ser solicitada depois de acao explicita do organizador.
- A captura e unica e nao representa localizacao em tempo real.
- Somente participantes autorizados visualizam descricao e coordenadas.
- Somente o organizador altera ou remove a localizacao.
- Falha ou recusa da permissao nao pode impedir o preenchimento textual.
- O feed pode informar que o local mudou, mas nao substitui a informacao oficial.
- Rastreamento, localizacao em segundo plano e historico de deslocamento estao fora do produto.

O detalhamento da entrega esta em `../versoes/v1.0.1-localizacao-fixa-do-encontro.md`.

## Validacao de novas funcionalidades

Toda nova funcionalidade deve responder:

- Qual problema do usuario ela resolve?
- Qual pilar do produto ela fortalece?
- Em qual versao ela deve entrar?
- Ela simplifica ou complica a experiencia?
- Ela preserva privacidade?
- Ela preserva a identidade do produto?
- Ela pode ser explicada de forma simples?
- Existe uma alternativa menor e mais simples?

Se a resposta nao for clara, a funcionalidade deve ser adiada.

## Regras de versao

O produto deve evoluir por versoes.

Regras:

- nunca pular versoes
- nunca implementar funcionalidade futura antes da hora
- cada versao deve entregar valor real
- cada funcionalidade deve pertencer a uma versao
- mudancas de escopo devem atualizar o roadmap quando necessario
- mudancas de regra devem atualizar este documento quando necessario
- mudancas de funcionalidade devem atualizar o backlog quando necessario

## Regras de documentacao

Toda funcionalidade deve ser documentada antes da implementacao.

Regras:

- documentacao deve ser escrita em Markdown
- documentos devem ficar em `docs`
- cada documento deve ter objetivo claro
- duplicacao de informacao deve ser evitada
- documentos relacionados devem ser referenciados
- documentacao superficial deve ser evitada

Quando uma decisao alterar outro documento, os documentos afetados devem ser informados.

## Criterios para rejeitar uma ideia

Uma ideia deve ser rejeitada, redesenhada ou adiada quando:

- transformar o produto em rede social
- competir diretamente com aplicativos de mensagem
- estimular uso excessivo de tela
- aumentar complexidade sem valor claro
- nao fortalecer nenhum pilar do produto
- prejudicar privacidade
- exigir muitas etapas para uma acao simples
- depender de funcionalidades futuras para fazer sentido
- criar custo tecnico alto sem necessidade atual
- fugir da identidade definida em `00-visao-produto.md`

## Relacao com outros documentos

Este documento depende de `00-visao-produto.md`, `01-roadmap-versoes.md`, `02-arquitetura-inicial.md` e `05-proposta-modulos-futuros.md`.

O detalhamento das funcionalidades devera ser feito em `04-backlog-inicial.md`.

Se estas regras forem alteradas, o roadmap e o backlog poderao precisar de revisao para manter consistencia.
