# Projeto Encontros - Proposta de Modulos Futuros

## Objetivo do documento

Este documento analisa e organiza ideias de evolucao para o Junto depois da v1.0 publica.

Ele registra uma direcao aprovada de produto, experiencia, arquitetura conceitual e roadmap. A aprovacao desta direcao nao autoriza implementacao: cada versao ainda depende de documento proprio, decisoes pendentes e autorizacao explicita.

## Status

Status: **aprovada como direcao futura de produto**.

Data da proposta: `2026-07-16`.

Este documento nao reabre a v0.7 e nao altera o escopo atual da v1.0.

As versoes v1.1 a v1.4 foram incorporadas ao roadmap oficial. Os modulos permanecem planejados e nao devem ser implementados antes de suas condicoes de entrada serem atendidas.

## Referencias

- `00-visao-produto.md`
- `01-roadmap-versoes.md`
- `02-arquitetura-inicial.md`
- `03-regras-de-produto.md`
- `04-backlog-inicial.md`
- `../versoes/v0.7-pessoas-frequentes-e-tipos-de-encontro.md`
- `../tecnico/decisoes-tecnicas.md`
- `../../AGENTS.md`

## Decisao executiva recomendada

As ideias analisadas possuem valor, mas nenhuma deve ser implementada antes da v1.0.

A v1.0 deve permanecer dedicada a:

- estabilidade.
- usabilidade.
- seguranca e privacidade.
- desempenho.
- qualidade do Flutter Web/PWA.
- validacao em aparelhos reais.
- preparacao para publicacao.

Adicionar agora votacao, gamificacao ou modos especializados aumentaria o risco do lancamento e dificultaria descobrir se a experiencia essencial esta realmente pronta.

Ordem recomendada depois da v1.0:

| Versao | Direcao | Entrega principal |
|---|---|---|
| v1.0 | Produto publico inicial | Estabilizacao, sem novos modulos desta proposta |
| v1.1 | Encontros musicais | Repertorio do Encontro e setlist simples |
| v1.2 | Diversao durante o encontro | Karaoke do Encontro |
| v1.3 | Fechamento afetivo | Destaques do Encontro em formato experimental |
| v1.4 | Historia pessoal | Marcos do Perfil, condicionados a validacao anterior |
| v1.5+ | Evolucoes validadas | Recursos avancados somente com evidencia de uso |

Essa ordem prioriza utilidade organizacional antes de mecanismos sociais. Repertorio resolve um problema concreto com menor risco. Karaoke reutiliza aprendizados de musica e uso durante o encontro. Destaques e Marcos ficam depois porque exigem validacao cuidadosa contra competicao, exclusao e uso compulsivo.

## Principios comuns aos novos modulos

Todos os modulos devem seguir estas regras:

- encontro continua sendo a unidade central e o limite de privacidade.
- nenhum modulo cria perfil publico, comunidade ou acesso global.
- tipo de encontro pode sugerir um recurso, mas nunca ativa-lo automaticamente.
- todo recurso especializado e opcional.
- o encontro continua completo sem qualquer recurso especializado.
- modulos aparecem dentro do encontro, nao como novas abas globais.
- feed registra apenas acontecimentos relevantes, sem narrar cada clique.
- memoria apresenta resumos positivos do que aconteceu.
- nenhuma funcionalidade deve premiar volume de uso ou tempo de tela.
- toda consulta valida participacao ativa no encontro.
- nenhuma informacao pode revelar a existencia de encontro privado a terceiros.

## Limites contra gamificacao toxica

O produto nao deve introduzir:

- pontos.
- moedas.
- niveis.
- missoes diarias.
- sequencias de presenca.
- ranking entre pessoas.
- podios.
- porcentagens comparativas.
- contagem publica de reconhecimentos.
- notificacoes de popularidade.
- recompensa por quantidade de publicacoes ou fotos.
- avaliacao de aparencia, talento ou personalidade.
- categorias negativas, mesmo apresentadas como brincadeira.

A medida de sucesso deve ser:

> O encontro ficou mais facil de organizar ou mais gostoso de relembrar?

Tempo de tela, quantidade de acoes e retorno compulsivo nao sao medidas de sucesso para estes modulos.

## Direcao arquitetural comum

A recomendacao inicial e preservar o monolito modular atual.

Cada modulo futuro deve possuir regras e dados proprios, vinculados por `IdentificadorDoEncontro`, sem transformar `Encontro` em uma entidade gigante.

Nao criar:

- microservico por modulo.
- banco separado.
- tabela generica com dados dos modulos em JSON.
- subclasses como `EncontroMusical` ou `EncontroDeKaraoke`.
- colunas de karaoke, repertorio ou destaques diretamente em `Encontro`.
- ativacao automatica de modulo com base no tipo.

O feed e a memoria devem consumir projecoes dos modulos. Eles nao devem se tornar proprietarios dos dados de votacao, repertorio ou karaoke.

Os modelos e endpoints deste documento sao conceituais. A arquitetura definitiva de cada modulo deve ser validada antes de sua versao ser implementada.

## Ideia 1 - Destaques do Encontro

### Nome recomendado

**Destaques do Encontro**.

Os nomes `Premios` e `Trofeus` sugerem competicao e vencedores. `Destaques` comunica reconhecimento positivo e contextual.

### Problema e valor

Acontecimentos divertidos e contribuicoes pessoais podem desaparecer quando o encontro termina.

Destaques do Encontro existe para registrar quem tornou aquele momento especial, transformando reconhecimento em memoria afetiva sem criar popularidade publica.

Pilares fortalecidos:

- amizade.
- memorias.
- encontros.

Prioridade recomendada: P1 experimental.

Versao recomendada: v1.3.

### Escopo inicial

Incluir:

- ativacao opcional pelo organizador.
- catalogo pequeno de categorias positivas.
- votacao secreta e opcional.
- uma escolha por categoria.
- resultado privado dentro do encontro.
- empate compartilhado.
- resumo na memoria do encontro.

Adiar:

- categorias personalizadas.
- comentarios em resultados.
- compartilhamento externo.
- notificacoes repetidas.
- historico de vitorias.
- contagem publica de votos.
- comparacao entre encontros.

Rejeitar:

- categorias negativas.
- ranking geral.
- podio.
- lista de pessoas sem votos.
- resultado ao vivo.
- autovoto.

### Categorias iniciais sugeridas

- Fez todo mundo rir.
- Cuidou do grupo.
- Registrou bons momentos.
- Deu uma forca.
- Animou o encontro.
- Compartilhou seu talento.
- Puxou a cantoria.
- Mestre da cozinha.

Categorias ligadas a musica devem aparecer apenas quando houver contexto musical. Expressoes que possam ser usadas com ironia devem passar por validacao de UX antes de entrar no catalogo.

### Regras de negocio

- Destaques so podem ser abertos quando o encontro estiver `Realizado`.
- Encontro cancelado nao permite votacao.
- Apenas pessoas com participacao real elegivel no encontro podem votar e ser reconhecidas.
- Confirmacao de presenca representa intencao e nao comprova, sozinha, participacao real.
- O criterio de participacao real deve ser resolvido antes da implementacao da v1.3.
- A lista de elegiveis deve ser congelada quando a votacao abrir.
- Participante pode optar por nao votar e por nao ser elegivel.
- Cada participante pode votar no maximo uma vez por categoria.
- Participante nao pode votar em si mesmo.
- Voto pode ser alterado ate o encerramento.
- Votos individuais sao secretos, inclusive para o organizador.
- Resultado so aparece depois do encerramento.
- Organizador pode abrir, encerrar ou cancelar a dinamica, mas nunca editar votos.
- Empates sao aceitos sem desempate artificial.
- Quantidade minima fixa de votos nao garante sigilo.
- Resultado deve ser suprimido quando tamanho, composicao ou distribuicao permitirem inferir votos ou pessoas nao reconhecidas.
- A janela inicial recomendada e de 72 horas e deve ser encerrada pelo sistema.
- Organizador pode cancelar a dinamica, mas cancelamento nao publica resultado.
- Encerramento antecipado pelo organizador nao deve ser permitido na primeira versao.
- Depois de encerrada, a edicao fica imutavel.
- O resultado nao mostra quantidade de votos, porcentagem ou derrotados.
- Retencao, auditoria, correcao e exclusao dos votos devem ser definidas antes da implementacao.

### UX sugerida

Pergunta principal:

> Quem tornou este encontro especial para voce?

Fluxo:

`Memoria do encontro -> Reconhecer alguem -> Escolher categoria -> Escolher pessoa -> Confirmar`

A votacao deve ser curta, uma categoria por vez, com a opcao `Prefiro nao votar` sempre visivel.

Nao criar aba global nem tela de ranking.

### Telas e componentes necessarios

1. **Cartao Destaques** na memoria do encontro realizado.
   Existe para apresentar uma acao opcional de reconhecimento.
2. **Painel de configuracao** para o organizador escolher categorias e abrir a votacao.
3. **Painel de votacao** com categoria, pessoas elegiveis e confirmacao.
4. **Secao de resultado** dentro da memoria.

Nao e necessaria uma tela independente de resultados na primeira versao.

### Modelo de dados inicial

`EdicaoDeDestaquesDoEncontro`

- Identificador.
- IdentificadorDoEncontro.
- Situacao: Preparacao, Aberta, Encerrada ou Cancelada.
- IdentificadorDoUsuarioQueCriou.
- AbertaEm.
- EncerradaEm.
- CriadaEm.

`CategoriaDeDestaque`

- Identificador.
- IdentificadorDaEdicao.
- CodigoDaCategoria.
- NomeExibido.
- Ordem.

`ElegivelAosDestaques`

- IdentificadorDaEdicao.
- IdentificadorDoUsuario.
- PodeVotar.
- PodeReceberVoto.

`VotoEmDestaque`

- Identificador.
- IdentificadorDaCategoria.
- IdentificadorDoUsuarioQueVotou.
- IdentificadorDoUsuarioEscolhido.
- CriadoEm.
- AtualizadoEm.

Restricoes importantes:

- uma edicao por encontro.
- um registro de elegibilidade por usuario e edicao.
- um voto por categoria e votante.
- votante e escolhido devem pertencer a mesma edicao.

Resultados podem ser calculados no encerramento. Materializacao deve ser considerada somente se houver necessidade real de desempenho.

### Endpoints conceituais

- `POST /api/encontros/{identificadorDoEncontro}/destaques`
- `GET /api/encontros/{identificadorDoEncontro}/destaques`
- `POST /api/encontros/{identificadorDoEncontro}/destaques/categorias`
- `DELETE /api/encontros/{identificadorDoEncontro}/destaques/categorias/{identificadorDaCategoria}`
- `POST /api/encontros/{identificadorDoEncontro}/destaques/abrir`
- `PUT /api/encontros/{identificadorDoEncontro}/destaques/categorias/{identificadorDaCategoria}/meu-voto`
- `DELETE /api/encontros/{identificadorDoEncontro}/destaques/categorias/{identificadorDaCategoria}/meu-voto`
- `POST /api/encontros/{identificadorDoEncontro}/destaques/encerrar`
- `GET /api/encontros/{identificadorDoEncontro}/destaques/resultados`

A API nunca deve retornar a identidade de quem votou.

### Integracao com feed, memorias e perfil

Feed:

- publicar no maximo um aviso quando a votacao abrir.
- publicar um resumo quando a votacao encerrar.
- nunca publicar cada voto.

Memorias:

- exibir uma secao `Destaques deste encontro`.
- nao duplicar resultado como memoria comum.

Perfil:

- na v1.3, o resultado permanece no encontro.
- na v1.4, um destaque pode originar um Marco do Perfil se a privacidade estiver resolvida.
- o usuario deve escolher se deseja exibir esse Marco.
- o Marco nao pode revelar encontro, data ou quantidade de votos para pessoas sem acesso.

### Riscos e cuidados

- exclusao de quem nao recebe reconhecimento.
- uso ironico de categorias.
- manipulacao em encontros pequenos.
- descoberta indireta de encontro privado pelo perfil.
- transformacao da experiencia em concurso de popularidade.

Mitigacoes obrigatorias:

- catalogo controlado.
- votos secretos.
- participacao minima.
- ausencia de placar.
- possibilidade de nao participar.
- resultado restrito ao encontro.

## Ideia 2 - Marcos do Perfil

### Nome recomendado

**Marcos do Perfil**, apresentado ao usuario como **Meus Marcos**.

`Conquistas` e `badges` sugerem acumulacao e desempenho. `Marcos` comunica acontecimentos significativos da historia da pessoa no produto.

### Problema e valor

O perfil ainda expressa pouco da historia que a pessoa construiu nos encontros.

Marcos existe para dar personalidade ao perfil e ajudar o usuario a revisitar sua propria trajetoria, sem transformar convivencia em produtividade social.

Pilares fortalecidos:

- memorias.
- amizade.
- experiencia do usuario.

Prioridade recomendada: P2.

Versao recomendada: v1.4, condicionada a pesquisa e validacao da v1.3.

### Tipos de Marcos

Marcos automaticos iniciais:

- Primeiro encontro.
- Primeira memoria compartilhada.
- Primeiro encontro organizado.
- Primeiro combinado concluido.
- Primeiro repertorio preparado.
- Primeira participacao no karaoke.

Marcos contextuais futuros:

- reconhecimento positivo recebido em Destaques do Encontro.
- primeira apresentacao musical registrada.
- primeira viagem guardada, quando esse contexto existir.

### Itens rejeitados

- Nunca falta.
- Presenca constante como sequencia.
- Maior organizador.
- Pessoa que mais publicou fotos.
- Pessoa que mais resolveu combinados.
- Melhor cantor automatico.
- metas por quantidade.
- raridade global.
- barras de progresso.
- lista publica de Marcos bloqueados.

Ausencias podem decorrer de saude, trabalho, distancia ou limites pessoais. O produto nao deve transformar isso em falha.

### Regras de negocio

- Marcos devem representar fatos verificaveis e positivos.
- Concessao automatica deve ser deterministica e idempotente.
- Marcos nao geram pontos, niveis ou ranking.
- O proprio usuario visualiza todos os seus Marcos.
- O usuario escolhe no maximo tres Marcos para destacar.
- Outras pessoas visualizam apenas os Marcos destacados e dentro de contexto privado compartilhado.
- Marco nao pode revelar encontro ao qual o observador nao possui acesso.
- Marco originado de encontro deve manter referencia privada para auditoria.
- Um Marco invalidado na origem deve deixar de ser exibido quando aplicavel.
- Marcos secretos ficam fora da primeira entrega.
- Marcos futuros ainda nao obtidos nao aparecem como checklist.
- Nao enviar notificacoes insistentes para incentivar desbloqueio.

### UX sugerida

Fluxo:

`Perfil -> Meus Marcos -> Ver historia ou escolher destaques`

O desbloqueio deve aparecer como confirmacao discreta depois de uma acao real, sem confete obrigatorio e sem interromper o fluxo principal.

### Telas e componentes necessarios

1. **Previa no Perfil** com ate tres Marcos escolhidos.
2. **Tela Meus Marcos** com nome, explicacao curta, data e origem acessivel quando autorizada.
3. **Painel Escolher destaques** para ordenar ate tres itens.

Nao mostrar total acumulado, nivel, pontos ou comparacao.

### Modelo de dados inicial

O catalogo inicial deve ficar versionado no codigo. Nao criar painel administrativo antes de haver necessidade.

`MarcoDoUsuario`

- Identificador.
- IdentificadorDoUsuario.
- CodigoDoMarco.
- TipoDaOrigem.
- IdentificadorDaOrigem.
- ConcedidoEm.
- EstaAtivo.

`MarcoDestacadoDoUsuario`

- IdentificadorDoUsuario.
- IdentificadorDoMarco.
- Ordem.

Restricao de idempotencia:

- usuario, codigo do Marco, tipo da origem e identificador da origem devem ser unicos em conjunto.

### Endpoints conceituais

- `GET /api/usuarios/eu/marcos`
- `PUT /api/usuarios/eu/marcos-destacados`
- `GET /api/encontros/{identificadorDoEncontro}/participantes/{identificadorDoUsuario}/marcos-destacados`

O ultimo endpoint usa o encontro como contexto de autorizacao e omite origem privada.

### Integracao com feed, memorias e Destaques

Feed:

- nao publicar desbloqueios pessoais no feed por padrao.

Memorias:

- Marco pode abrir sua origem apenas quando o usuario possui acesso a ela.
- a memoria continua sendo dona da apresentacao contextual.

Destaques:

- um reconhecimento pode originar Marco somente depois da v1.3 ser validada.
- nao mostrar quantidade de vezes ou ranking.

### Riscos e cuidados

- spam para desbloquear Marcos.
- comparacao entre pessoas.
- exposicao de habitos e encontros privados.
- criterios injustos por dados historicos incompletos.
- crescimento descontrolado do catalogo.

Comecar com poucos Marcos unicos e sem progresso percentual.

## Ideia 3 - Karaoke do Encontro

### Nome recomendado

**Karaoke do Encontro**.

`Modo Karaoke` pode ser usado como texto de acao, mas o recurso continua subordinado ao encontro.

### Problema e valor

Organizar ordem, pessoas e musicas durante um karaoke costuma gerar confusao e interrupcoes.

O modulo existe para conduzir a atividade com leveza e guardar um resumo positivo do que foi cantado.

Pilares fortalecidos:

- organizacao.
- encontros.
- memorias.
- amizade.

Prioridade recomendada: P1.

Versao recomendada: v1.2.

### Escopo inicial

Incluir:

- ativacao opcional.
- inscricao voluntaria de participantes.
- cadastro simples de musicas.
- fila e sorteio justo.
- troca de musica.
- pulo sem penalidade.
- duplas com consentimento.
- marcacao de apresentacao realizada.
- resumo final privado.

Adiar:

- avaliacao de desempenho.
- placar.
- premiacao automatica.
- integracao com catalogos externos.
- reproducao de video dentro do aplicativo.
- tempo real com SignalR, salvo necessidade validada.

### Regras de sorteio

- Apenas participantes ativos que se inscreveram explicitamente entram no sorteio.
- Participacao no encontro nao inscreve automaticamente no karaoke.
- Participante pode sair da fila sem justificativa.
- Dupla exige consentimento das duas pessoas.
- Todas as pessoas disponiveis devem ter uma vez antes de qualquer repeticao.
- A mesma pessoa nao deve ser repetida imediatamente.
- Musica nao deve repetir enquanto houver alternativa disponivel.
- Ao esgotar pessoas ou musicas, um novo ciclo pode iniciar.
- Participante pode trocar a musica antes de confirmar.
- Pular nao gera penalidade, Marco negativo ou publicacao no feed.
- Organizador pode ajustar a fila, mas a alteracao deve ser visivel.
- Sorteio confirmado nao pode ser refeito silenciosamente.
- Acoes simultaneas devem usar controle otimista de concorrencia.
- Inscricao, dupla, identificacao no feed, identificacao na memoria e concessao de Marco exigem consentimentos separados.
- Consentimento deve ser explicito, informado, especifico e revogavel.
- O ciclo de retirada e correcao de registros ja publicados deve ser definido antes da implementacao.

### UX sugerida

Uma unica tela muda conforme o estado:

1. **Preparacao**: pessoas inscritas, musicas e acao `Montar fila`.
2. **Em andamento**: quem canta agora, musica atual, proxima pessoa e acoes grandes.
3. **Encerrado**: resumo das apresentacoes e acao para abrir a memoria.

Acoes durante o encontro:

- Sortear.
- Cantou.
- Trocar musica.
- Pular.
- Encerrar.

Adicionar musica, trocar musica e consultar fila podem usar paineis inferiores em vez de novas telas.

### Telas e componentes necessarios

1. **Cartao Karaoke** dentro do encontro quando ativado.
2. **Tela Karaoke** com os tres estados.
3. **Painel de inscricao** para cada participante escolher se quer cantar.
4. **Painel de musica** para cadastro ou troca.
5. **Resumo** incorporado a memoria.

### Modelo de dados inicial

`SessaoDeKaraoke`

- Identificador.
- IdentificadorDoEncontro.
- Situacao: Preparacao, EmAndamento ou Encerrada.
- PermiteDupla.
- Versao.
- IniciadaEm.
- EncerradaEm.

`MusicaDoKaraoke`

- Identificador.
- IdentificadorDaSessao.
- Nome.
- Artista.
- LinkExterno.
- EstaAtiva.

`InscritoNoKaraoke`

- IdentificadorDaSessao.
- IdentificadorDoUsuario.
- AceitaDupla.
- EstaAtivo.

`ApresentacaoDoKaraoke`

- Identificador.
- IdentificadorDaSessao.
- IdentificadorDaMusica.
- Ordem.
- Situacao: Aguardando, Realizada, Pulada ou Cancelada.
- SorteadaEm.
- ConcluidaEm.

`CantorDaApresentacao`

- IdentificadorDaApresentacao.
- IdentificadorDoUsuario.

### Endpoints conceituais

- `POST /api/encontros/{identificadorDoEncontro}/karaoke`
- `GET /api/encontros/{identificadorDoEncontro}/karaoke`
- `POST /api/encontros/{identificadorDoEncontro}/karaoke/musicas`
- `PUT /api/encontros/{identificadorDoEncontro}/karaoke/musicas/{identificadorDaMusica}`
- `DELETE /api/encontros/{identificadorDoEncontro}/karaoke/musicas/{identificadorDaMusica}`
- `PUT /api/encontros/{identificadorDoEncontro}/karaoke/minha-inscricao`
- `POST /api/encontros/{identificadorDoEncontro}/karaoke/iniciar`
- `POST /api/encontros/{identificadorDoEncontro}/karaoke/sortear`
- `PUT /api/encontros/{identificadorDoEncontro}/karaoke/apresentacoes/{identificadorDaApresentacao}/musica`
- `POST /api/encontros/{identificadorDoEncontro}/karaoke/apresentacoes/{identificadorDaApresentacao}/realizar`
- `POST /api/encontros/{identificadorDoEncontro}/karaoke/apresentacoes/{identificadorDaApresentacao}/pular`
- `POST /api/encontros/{identificadorDoEncontro}/karaoke/encerrar`

Atualizacao periodica e controle de versao sao suficientes para o primeiro teste. SignalR deve ser considerado apenas se a experiencia real demonstrar necessidade.

### Integracao com feed, memorias e Marcos

Feed:

- registrar inicio da sessao.
- registrar apresentacao realizada, se os participantes aceitarem aparecer.
- registrar resumo no encerramento.
- nao registrar troca, recusa ou pulo.

Memorias:

- guardar ordem realizada e musicas cantadas.
- incluir apenas participantes que aceitaram aparecer no resumo.

Marcos:

- primeira participacao e primeira organizacao podem gerar Marcos futuros.
- nao gerar `Melhor cantor` automaticamente.

### Riscos e cuidados

- constranger pessoa sorteada.
- expor quem recusou ou pulou.
- conflitos por acoes simultaneas.
- tela dificil de operar durante o encontro.
- excesso de eventos no feed.

Consentimento, botoes grandes, fila clara e resumo positivo sao obrigatorios.

## Ideia 4 - Repertorio do Encontro

### Nome recomendado

**Repertorio do Encontro**.

### Diferenca entre Repertorio e Setlist

- Repertorio e o conjunto de musicas consideradas ou preparadas para o encontro.
- Setlist e a selecao ordenada das musicas que serao executadas.

O modulo se chama Repertorio. Setlist e uma visao ordenada dentro dele, nao outro modulo ou item de navegacao principal.

### Problema e valor

Musicas, tons, links, ordem e responsabilidades costumam ficar espalhados em mensagens.

Repertorio do Encontro existe para organizar ensaios, cantatas, cultos, rodas de violao e outros encontros musicais, alem de preservar o que realmente foi tocado.

Pilares fortalecidos:

- organizacao.
- encontros.
- memorias.
- simplicidade.

Prioridade recomendada: P0 entre as ideias analisadas.

Versao recomendada: v1.1.

### Escopo inicial

Incluir:

- nome da musica obrigatorio.
- artista ou categoria opcional.
- tom.
- capotraste.
- quem canta.
- quem toca e instrumento.
- link de video.
- link de cifra.
- observacao curta.
- ordenacao do setlist.
- marcacao como tocada ou pulada.

Adiar:

- biblioteca musical global.
- letras completas.
- importacao ou copia de cifras.
- anexos de partituras.
- transposicao automatica.
- integracao com YouTube ou sites de cifra.
- busca automatica de metadados.
- estatisticas de execucao.

### Regras de negocio

- Repertorio e opcional e pertence a um unico encontro.
- Participantes autorizados podem visualizar.
- Participante pode sugerir musica.
- Organizador controla ordem, remocao e setlist final na primeira versao.
- Apenas participantes do encontro podem ser associados a uma musica.
- Nome da musica e o unico campo obrigatorio.
- Tom e capotraste sao opcionais.
- Links devem usar HTTPS.
- Observacao deve possuir limite curto.
- Ordem do setlist deve ser atualizada de forma atomica.
- A mesma musica pode aparecer novamente quando houver reprise intencional.
- Musica marcada como tocada permanece no resumo do encontro.
- O aplicativo nao copia nem armazena letras ou cifras de terceiros.

### UX sugerida

Fluxo:

`Encontro musical -> Adicionar repertorio -> Adicionar musicas -> Ordenar setlist -> Marcar como tocada -> Revisitar na memoria`

A criacao de musica pede apenas o nome. A acao `Adicionar detalhes` revela os campos opcionais.

Uma unica tela pode ter duas visualizacoes:

- Repertorio.
- Setlist.

### Telas e componentes necessarios

1. **Cartao Repertorio** dentro do encontro quando ativado.
2. **Tela Repertorio** com lista de musicas e setlist.
3. **Painel de musica** para criar ou editar detalhes.
4. **Seletor de participantes** para quem canta e quem toca.
5. **Resumo do setlist realizado** dentro da memoria.

Nao criar aba global, biblioteca compartilhada ou tela separada por instrumento.

### Modelo de dados inicial

`RepertorioDoEncontro`

- Identificador.
- IdentificadorDoEncontro.
- CriadoEm.
- AtualizadoEm.

`MusicaDoRepertorio`

- Identificador.
- IdentificadorDoRepertorio.
- IdentificadorDoUsuarioQueAdicionou.
- Nome.
- ArtistaOuCategoria.
- Tom.
- Capotraste.
- LinkDaCifra.
- LinkDoVideo.
- Observacoes.
- CriadaEm.
- AtualizadaEm.

`ParticipacaoNaMusica`

- IdentificadorDaMusica.
- IdentificadorDoUsuario.
- Funcao: Canta ou Toca.
- Instrumento.

`ItemDoSetlist`

- Identificador.
- IdentificadorDoRepertorio.
- IdentificadorDaMusica.
- Ordem.
- Situacao: Planejada, Tocada ou Pulada.
- ExecutadaEm.

### Endpoints conceituais

- `POST /api/encontros/{identificadorDoEncontro}/repertorio`
- `GET /api/encontros/{identificadorDoEncontro}/repertorio`
- `POST /api/encontros/{identificadorDoEncontro}/repertorio/musicas`
- `PUT /api/encontros/{identificadorDoEncontro}/repertorio/musicas/{identificadorDaMusica}`
- `DELETE /api/encontros/{identificadorDoEncontro}/repertorio/musicas/{identificadorDaMusica}`
- `PUT /api/encontros/{identificadorDoEncontro}/repertorio/musicas/{identificadorDaMusica}/participantes`
- `POST /api/encontros/{identificadorDoEncontro}/setlist/itens`
- `PUT /api/encontros/{identificadorDoEncontro}/setlist/ordem`
- `DELETE /api/encontros/{identificadorDoEncontro}/setlist/itens/{identificadorDoItem}`
- `POST /api/encontros/{identificadorDoEncontro}/setlist/itens/{identificadorDoItem}/tocar`
- `POST /api/encontros/{identificadorDoEncontro}/setlist/itens/{identificadorDoItem}/pular`

### Integracao com feed, memorias e Marcos

Feed:

- publicar quando o repertorio for preparado.
- publicar um resumo do setlist ao encerrar.
- nao publicar cada edicao de tom, ordem ou participante.

Memorias:

- guardar setlist final.
- separar musicas tocadas das planejadas e puladas.
- permitir abrir links somente por acao consciente do usuario.

Marcos:

- primeira preparacao de repertorio ou primeira participacao musical podem gerar Marcos futuros.
- nao criar contagem competitiva de musicas.

### Direitos autorais e links externos

- armazenar somente metadados, links e observacoes proprias.
- nao copiar letras ou cifras de terceiros.
- nao raspar sites.
- nao incorporar video automaticamente.
- nao usar API do YouTube na primeira versao.
- nao enviar dados privados do encontro a servico externo sem acao consciente.

### Riscos e cuidados

- transformar o produto em ferramenta profissional complexa.
- violacao de direitos autorais.
- formulario longo.
- conflitos na ordenacao.
- permissoes excessivas para edicao.

O MVP deve continuar pequeno: nome obrigatorio, detalhes progressivos e controle claro do setlist.

## Ideia 5 - Tipo de Encontro e sugestoes contextuais

### Situacao atual

O nome oficial permanece **Tipo de Encontro**.

A classificacao opcional foi entregue na v0.7. Essa versao esta fechada e nao deve ser reaberta.

### Analise

O tipo faz sentido como classificacao e contexto, nao como heranca tecnica ou ativador automatico de telas.

Regra recomendada:

> O tipo pode sugerir um recurso; nunca deve ativa-lo, mudar permissoes ou impor configuracoes.

### Taxonomia recomendada para revisao futura

- Aniversario.
- Churrasco.
- Karaoke.
- Musica ou ensaio.
- Futebol ou esporte.
- Jogos.
- Estudo.
- Viagem.
- Outro.

`Simples` nao precisa ser um tipo. Deixar o campo vazio representa um encontro sem classificacao.

Tipos antigos nao devem ser removidos antes da v1.0. A normalizacao deve preservar dados existentes.

### Sugestoes por contexto

- Musica ou ensaio: sugerir `Adicionar repertorio`.
- Karaoke: sugerir `Preparar karaoke`.
- Churrasco: sugerir combinados sobre o que levar.
- Aniversario: destacar criacao de memorias depois do encontro.
- Viagem: nao criar roteiro ou custos sem escopo proprio.
- Outros tipos: nenhuma mudanca obrigatoria.

O usuario pode usar repertorio em um encontro religioso, karaoke em um aniversario ou nenhum modulo em encontro com tipo definido.

### Regras de negocio

- Tipo continua opcional.
- Tipo nao altera acesso, participantes, presenca ou notificacoes por si so.
- Escolher um tipo nao cria dados de outro modulo.
- Sugestao aparece depois da criacao, nao aumenta o formulario principal.
- Sugestao pode ser ignorada.
- Modulo so existe depois de ativacao explicita.
- Nao criar tabela generica de recursos enquanto nao houver necessidade comprovada.
- Cada modulo especializado indica sua propria ativacao pela existencia de seu agregado.

### UX sugerida

Tipo permanece como seletor opcional na criacao e edicao.

Depois de criar o encontro, uma sugestao discreta pode aparecer uma vez nos detalhes. Ela deve possuir uma acao principal e a opcao de dispensar.

Nao criar tela de tipos, subtipos ou configuracao de modulos.

### Modelo e endpoint candidatos

Preservar o campo atual na v1.0 e revisar sua normalizacao antes de adicionar modulos.

Se o catalogo precisar ser compartilhado entre API e Flutter, considerar:

- codigo canonico do tipo.
- descricao curta apenas para `Outro`.
- `GET /api/tipos-de-encontro` para retornar codigos, nomes e sugestoes disponiveis.

Nao criar tabela de tipos enquanto o catalogo for pequeno e controlado pelo produto.

## Integracao transversal com feed e memorias

### Publicacoes de sistema

Os novos modulos precisam diferenciar publicacao humana de acontecimento automatico.

Evolucao conceitual candidata para `PublicacaoDoEncontro`:

- TipoDaPublicacao.
- TipoDaOrigem.
- IdentificadorDaOrigem.
- Texto como representacao legivel congelada.

Nao armazenar payload JSON generico no feed.

Cada modulo deve gerar poucos acontecimentos:

- abertura ou encerramento de Destaques.
- inicio, apresentacao confirmada e fim do karaoke.
- repertorio preparado e setlist realizado.

### Resumo da memoria

Dados especializados nao devem virar falsas memorias comuns.

A memoria pode usar uma consulta consolidada para apresentar:

- midias e publicacoes.
- Destaques.
- setlist executado.
- resumo do karaoke.
- principais acontecimentos.

Endpoint conceitual candidato:

- `GET /api/encontros/{identificadorDoEncontro}/resumo`

Cada modulo continua sendo proprietario de seus dados. O resumo apenas projeta informacoes que o usuario ja pode acessar.

## Estados e acessibilidade comuns

Todos os modulos devem reutilizar o padrao visual atual e os componentes do encontro.

Estados obrigatorios:

- carregamento localizado no cartao do recurso.
- estado vazio curto com uma unica acao.
- erro que preserva dados digitados.
- acesso negado sem encerrar sessao valida.
- estado encerrado consultavel e claramente bloqueado para edicao.

Diretrizes:

- uso confortavel com uma mao.
- alvos de toque adequados.
- contraste suficiente.
- estado nunca comunicado somente por cor.
- textos curtos e nao tecnicos.
- movimento reduzido respeitado.
- confirmacao apenas para acao destrutiva ou coletiva.
- botoes grandes nos fluxos usados durante o encontro.

## Roadmap futuro recomendado

### v1.0 - Produto publico inicial

Objetivo:

- publicar a experiencia essencial com estabilidade e privacidade.

Escopo relacionado a esta proposta:

- nenhum modulo novo.
- observar uso real do tipo entregue na v0.7.
- revisar contrato e valores existentes de tipo, se necessario para estabilidade.
- manter tipos sem efeito sobre autorizacao.

Criterio de sucesso:

- produto essencial pronto para uso real, sem ampliar o escopo funcional.

### v1.1 - Repertorio do Encontro

Objetivo:

- organizar encontros musicais sem criar ferramenta profissional pesada.

Escopo:

- P0: repertorio privado por encontro.
- P0: musicas com nome e detalhes opcionais.
- P0: setlist ordenado.
- P1: participantes que cantam ou tocam.
- P1: marcacao de tocada ou pulada.
- P1: resumo na memoria.
- P2: sugestao contextual baseada no tipo.

Criterio de sucesso:

- grupo prepara e registra musicas com menos trabalho, enquanto encontros sem musica permanecem inalterados.

### v1.2 - Karaoke do Encontro

Objetivo:

- apoiar a atividade em tempo real de forma divertida, inclusiva e simples.

Escopo:

- P0: sessao, inscricao voluntaria e musicas.
- P0: fila e sorteio justo.
- P0: trocar, pular e concluir.
- P1: duplas com consentimento.
- P1: resumo privado na memoria.
- P2: selecao a partir do repertorio, depois de validacao.

Criterio de sucesso:

- grupo conduz o karaoke sem controlar ordem manualmente e sem avaliar desempenho.

### v1.3 - Destaques do Encontro

Objetivo:

- criar um fechamento afetivo e positivo depois do encontro.

Escopo:

- P0: catalogo exclusivamente positivo.
- P0: votacao secreta e opcional.
- P0: resultado sem ranking ou contagem publica.
- P1: resultado na memoria.
- P2: pesquisa sobre categorias personalizadas, sem implementacao automatica.

Criterio de sucesso:

- participantes registram reconhecimentos sem constrangimento, comparacao ou competicao.

Condicao de lancamento:

- pesquisa com usuarios deve confirmar que a experiencia e percebida como afeto, nao como concurso.

### v1.4 - Marcos do Perfil

Objetivo:

- representar a historia da pessoa sem transformar perfil em placar.

Escopo:

- P0: poucos Marcos automaticos e verificaveis.
- P0: escolha de ate tres Marcos destacados.
- P0: privacidade contextual.
- P1: Marcos originados de Destaques validados.
- P2: novos Marcos baseados em dados confiaveis.

Fora do escopo:

- ranking.
- nivel.
- pontos.
- sequencias.
- metas de volume.
- perfil publico.

Criterio de sucesso:

- perfil ganha historia e personalidade sem revelar encontros privados ou incentivar comparacao.

Condicao de lancamento:

- somente prosseguir se a v1.3 nao apresentar sinais de exclusao, spam ou competicao.

### v1.5 e posteriores

Somente depois de uso real, avaliar:

- categorias personalizadas com moderacao.
- Marcos surpresa positivos.
- SignalR no karaoke.
- biblioteca reutilizavel de musicas.
- modelos de encontro.
- roteiro e custos de viagem.
- outros recursos contextuais.

## Riscos transversais

### Produto

- excesso de telas.
- desvio para rede social ou jogo de popularidade.
- recursos especializados dominarem a experiencia simples.
- notificacoes usadas para gerar engajamento artificial.

### Privacidade

- Marcos revelarem encontros privados.
- resultados de votacao exporem preferencias pessoais.
- links externos vazarem contexto.
- usuario removido continuar acessando dados de modulo.

### Tecnica

- agregado `Encontro` crescer demais.
- feed receber eventos sem estrutura.
- concorrencia em fila e setlist.
- consultas de memoria carregarem dados excessivos.
- concessao duplicada de Marcos.
- migracao de tipos livres para catalogo canonico.

### Legal

- armazenamento indevido de letras, cifras ou conteudo externo.
- incorporacao automatica de servicos sem consentimento.

## Decisoes aprovadas

Foram aprovadas como direcao futura:

1. A ordem v1.1 a v1.4 registrada no roadmap.
2. `Destaques do Encontro` como nome do reconhecimento positivo.
3. `Marcos do Perfil` como substituto de Conquistas.
4. Categorias personalizadas fora da primeira entrega de Destaques.
5. Nenhum modulo novo antes da estabilizacao da v1.0.
6. Repertorio do Encontro como primeiro modulo depois da v1.0.
7. Tipo de Encontro apenas sugere recursos e nunca os ativa automaticamente.

## Pendencias obrigatorias antes da implementacao

Estas pendencias nao alteram o roadmap, mas bloqueiam o inicio das versoes relacionadas:

1. Definir a matriz de permissoes de cada modulo.
2. Definir consentimentos e seus efeitos sobre feed, memoria e Marcos.
3. Definir preservacao ou anonimizacao de historico quando participante perde acesso.
4. Definir criterio confiavel de participacao real para Destaques.
5. Definir protecao contra inferencia de votos em grupos pequenos.
6. Definir retencao, auditoria, correcao e exclusao dos votos.
7. Fechar o catalogo canonico de tipos sem perder valores existentes.
8. Definir processo de denuncia e remocao de link ou conteudo musical indevido.

## Documentos impactados depois da aprovacao

Esta proposta altera apenas este documento enquanto estiver em analise.

Depois da aprovacao, revisar um documento por vez:

1. `01-roadmap-versoes.md` para oficializar v1.1 a v1.4.
2. `04-backlog-inicial.md` para criar itens com prioridade e criterios de aceite.
3. `03-regras-de-produto.md` para registrar limites de Destaques e Marcos.
4. `02-arquitetura-inicial.md` ou `../tecnico/decisoes-tecnicas.md` para formalizar agregados especializados.
5. criar documento da versao v1.1 somente quando sua implementacao for autorizada.

## Conclusao

As cinco ideias podem fortalecer o Junto quando permanecem subordinadas ao encontro e entram de forma progressiva.

Recomendacao final:

- Tipo de Encontro permanece simples e apenas sugere recursos.
- Repertorio entra primeiro por resolver organizacao concreta.
- Karaoke entra depois com participacao voluntaria e sem avaliacao.
- Premios tornam-se Destaques positivos, secretos e sem ranking.
- Conquistas tornam-se Marcos afetivos, privados e sem metas compulsivas.
- v1.0 continua exclusivamente dedicada a estabilizacao e publicacao.

A proposta entrega diversao e personalidade sem abandonar a identidade do produto: organizar encontros reais, fortalecer amizades e guardar memorias.
