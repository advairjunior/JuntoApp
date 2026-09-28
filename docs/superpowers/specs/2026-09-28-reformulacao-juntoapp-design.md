# Reformulacao do JuntoApp

## Objetivo

Transformar o JuntoApp em um diario compartilhado de encontros, sem repetir conversa, chamadas, notificacoes ou organizacao que o WhatsApp ja oferece. O aplicativo deve registrar quem esteve junto, quando, onde e em quais fotos, gerando linha do tempo, estatisticas e retrospectivas.

O produto deve continuar pequeno, funcionar no navegador do celular e ter baixo custo de armazenamento. Nao havera integracao empresarial nem leitura automatica do WhatsApp.

## Escopo principal

O novo nucleo tera:

- perfis simples;
- grupos recorrentes;
- encontros de grupo e encontros avulsos;
- participantes, data e localizacao;
- fotos compactadas e marcacoes de pessoas;
- linha do tempo;
- estatisticas de grupos, pessoas e do proprio usuario;
- importacao incremental de conversas exportadas pelo WhatsApp.

Serao retirados do produto:

- feed e publicacoes;
- respostas e comentarios;
- notificacoes internas e preferencias de notificacao;
- listas de itens;
- audios e videos;
- registro de chamadas;
- memoria como cadastro separado;
- qualquer tentativa de substituir as conversas do WhatsApp.

## Conceitos do dominio

### Usuario

Representa uma pessoa uma unica vez em todo o aplicativo. Possui identificador permanente, nome, celular privado, PIN protegido e foto opcional. O celular pode mudar sem alterar o identificador ou o historico.

### Grupo

Representa um conjunto recorrente de pessoas, como o Creu. Um grupo e criado uma vez e acumula varios encontros ao longo do tempo.

### Encontro

Representa algo que aconteceu ou esta planejado para uma data. Pode pertencer a um grupo ou ser avulso. Possui participantes, data, localizacao e fotos.

Um encontro realizado ja e apresentado como memoria na linha do tempo. Nao existe conversao nem entidade separada de memoria.

### Pessoa

A area Pessoas mostra a relacao entre o usuario atual e cada pessoa com quem ele compartilhou encontros, atravessando grupos e encontros avulsos. A mesma pessoa nao e duplicada ao participar de varios grupos.

## Fluxos

### Grupo recorrente

O grupo e cadastrado uma vez com seus integrantes. Cada acontecimento gera um encontro separado dentro dele. Duas idas ao mesmo espetinho em semanas diferentes sao dois encontros do mesmo grupo.

### Encontro avulso

Aniversarios, viagens ou churrascos isolados podem ser criados diretamente, sem obrigar a criacao de um grupo. O convite e compartilhado por link no WhatsApp.

### Importacao incremental

Um integrante exporta manualmente a conversa do WhatsApp e seleciona o arquivo no JuntoApp. O arquivo e analisado no navegador e nao e armazenado no servidor.

O aplicativo usa regras simples para sugerir possiveis encontros a partir de datas, frases, locais e midias. Nenhuma sugestao e salva sem confirmacao humana. Se nada for identificado, o encontro pode ser informado manualmente.

Para cada grupo, o sistema guarda apenas marcadores tecnicos da importacao: data, periodo coberto, identificacao do arquivo e identificacao da ultima mensagem processada. Uma nova exportacao continua desse ponto e ignora conteudo ja processado.

Se o arquivo nao contiver o ultimo ponto conhecido, o aplicativo avisa que pode existir um intervalo ausente. Arquivos repetidos e possiveis encontros duplicados tambem geram aviso.

## Pessoas e privacidade

A pagina de uma pessoa pode mostrar:

- total, primeiro e ultimo encontro compartilhado;
- proximo encontro planejado em comum;
- grupos e encontros avulsos compartilhados;
- locais e tipos de encontro mais frequentes;
- fotos em que ambos participaram;
- linha do tempo e frequencia por periodo;
- aniversario opcional.

Esses dados consideram todos os contextos compartilhados pelas duas pessoas. O usuario nao ve grupos, encontros, fotos ou estatisticas da outra pessoa quando ele nao participou daquele contexto.

Cada usuario ve somente os grupos dos quais participa. Um integrante pode consultar os demais membros do grupo, mas as informacoes relacionais continuam limitadas ao historico que compartilham.

## Acesso

O primeiro acesso solicita nome, celular e PIN. O servidor cria o identificador permanente e mantem a sessao conectada no aparelho.

Em outro aparelho, o usuario entra com celular e PIN. Enquanto estiver conectado, pode trocar o numero sem perder dados. Se perder completamente o acesso, o administrador pode recuperar a conta e substituir o numero.

O celular e privado, unico para acesso e nunca e usado como identificador das estatisticas. Perfis duplicados podem ser unidos pelo administrador preservando os relacionamentos.

O administrador do sistema cuida apenas da recuperacao de contas e da uniao de perfis. Administradores de grupo cuidam dos integrantes e encontros do proprio grupo, mas nao podem alterar contas pessoais.

## Fotos e armazenamento

Somente imagens serao aceitas. Elas serao compactadas antes do envio e verificadas para evitar repeticao. O JuntoApp guarda versoes otimizadas; os originais permanecem no celular ou no WhatsApp.

Nao existe limite fixo por encontro. Cada grupo possui uma cota total visivel. Se a cota estiver cheia, o encontro ainda pode ser salvo sem novas fotos.

As fotos ficam diretamente vinculadas ao encontro e podem marcar participantes.

## Navegacao

A navegacao principal tera quatro areas:

- **Inicio:** linha do tempo e proximos encontros;
- **Grupos:** grupos recorrentes, integrantes, encontros e atualizacao pelo WhatsApp;
- **Pessoas:** relacionamentos individuais considerando todos os contextos compartilhados;
- **Perfil:** conta, seguranca e retrospectiva pessoal.

Nao existira aba separada de Memorias, central de notificacoes nem chat interno.

## Tratamento de falhas

- Arquivo invalido ou formato desconhecido: explicar o problema e permitir registro manual.
- Nenhuma sugestao encontrada: nao tratar como erro; oferecer registro manual.
- Arquivo repetido: impedir novo processamento e informar quando ele foi processado.
- Intervalo possivelmente ausente: informar as datas e permitir continuar.
- Foto invalida, repetida ou acima da cota: preservar o encontro e informar apenas a falha da foto.
- Falha durante uma confirmacao: nao criar encontros parciais nem duplicados.

## Estrutura e reinicio dos dados

Os dados atuais de producao podem ser descartados. A nova estrutura persistira somente usuarios, grupos, membros, encontros, participantes, fotos, localizacoes e controles de importacao.

As alteracoes locais de codigo existentes devem ser preservadas antes da reformulacao. O reinicio dos dados nao autoriza apagar trabalho de codigo sem antes protege-lo no Git.

## Sequencia de entrega

1. Proteger o trabalho local existente e criar a nova linha de base do banco.
2. Simplificar identidade, acesso e navegacao.
3. Entregar grupos, encontros, participantes, localizacoes e fotos.
4. Entregar Pessoas, estatisticas e linha do tempo.
5. Entregar importacao incremental e sugestoes de encontros.
6. Entregar cotas, retrospectivas e acabamentos.

Cada etapa deve manter o aplicativo executavel e testado antes da seguinte.

## Validacao

Os testes devem cobrir:

- acesso, troca de celular e recuperacao por administrador;
- identidade unica em varios grupos;
- encontros de grupo e avulsos;
- privacidade das informacoes relacionais;
- estatisticas entre pessoas e por grupo;
- importacoes consecutivas, atrasadas, repetidas e invalidas;
- deteccao de encontros duplicados;
- compactacao, repeticao e cota de fotos;
- autorizacao de leitura e alteracao de grupos e encontros.

## Criterio de sucesso

O JuntoApp sera considerado enxuto quando o WhatsApp continuar sendo o lugar de conversa e combinacao, enquanto o JuntoApp registrar encontros com pouco trabalho e produzir uma historia confiavel dos grupos e das relacoes entre pessoas.
