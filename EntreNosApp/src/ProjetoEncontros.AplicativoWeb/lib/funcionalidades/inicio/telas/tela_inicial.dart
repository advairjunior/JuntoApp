import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/acessibilidade/identificadores_semanticos.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cabecalho_da_pagina.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cartao_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/conteudo_responsivo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/indicador_de_situacao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/titulo_de_secao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/configuracao/configuracao_do_ambiente.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/foto_de_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/imagem_privada.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/raios_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/estado/controlador_da_pagina_inicial.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/estado/estado_da_pagina_inicial.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/convite_do_encontro_resumo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/encontro_resumo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/usuario_atual.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/notificacoes/dados/repositorio_de_notificacoes.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/notificacoes/modelos/notificacao_do_usuario.dart';

class TelaInicial extends ConsumerWidget {
  const TelaInicial({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    EstadoDaPaginaInicial estado = ref.watch(
      provedorDoControladorDaPaginaInicial,
    );
    AsyncValue<ListaDeNotificacoes> notificacoes = ref.watch(
      provedorDaListaDeNotificacoes,
    );

    return ConteudoResponsivo(
      preenchimento: const EdgeInsets.fromLTRB(
        EspacamentosDoAplicativo.padrao,
        EspacamentosDoAplicativo.padrao,
        EspacamentosDoAplicativo.padrao,
        EspacamentosDoAplicativo.grande,
      ),
      filho: switch (estado.situacao) {
        SituacaoDaPaginaInicial.carregando => const _CarregamentoDaPagina(),
        SituacaoDaPaginaInicial.falhou => _ErroDaPagina(
            mensagem: estado.mensagemDeErro ??
                'Não foi possível carregar a página inicial.',
            aoTentarNovamente: () => ref
                .read(provedorDoControladorDaPaginaInicial.notifier)
                .carregueAsync(),
          ),
        SituacaoDaPaginaInicial.carregada => _ConteudoDaPagina(
            usuarioAtual: estado.usuarioAtual!,
            encontros: estado.encontros,
            convitesPendentes: estado.convitesPendentes,
            identificadorDoConviteEmAtualizacao:
                estado.identificadorDoConviteEmAtualizacao,
            mensagemDeErro: estado.mensagemDeErro,
            quantidadeDeNotificacoesNaoLidas:
                notificacoes.valueOrNull?.quantidadeNaoLida ?? 0,
            aoAbrirNotificacoes: () async {
              await context.push<void>('/notificacoes');
              ref.invalidate(provedorDaListaDeNotificacoes);
            },
            aoRecarregar: () => ref
                .read(provedorDoControladorDaPaginaInicial.notifier)
                .carregueAsync(),
            aoAbrirEncontro: (String identificador) async {
              await context.push('/encontros/$identificador');

              if (context.mounted) {
                await ref
                    .read(provedorDoControladorDaPaginaInicial.notifier)
                    .carregueAsync();
              }
            },
            aoResponderConvite: ({
              required String identificadorDoEncontro,
              required String situacao,
            }) async {
              bool respondeu = await ref
                  .read(provedorDoControladorDaPaginaInicial.notifier)
                  .respondaConviteAsync(
                    identificadorDoEncontro: identificadorDoEncontro,
                    situacao: situacao,
                  );

              if (context.mounted) {
                EstadoDaPaginaInicial estadoAtual =
                    ref.read(provedorDoControladorDaPaginaInicial);
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(
                    content: Text(
                      respondeu
                          ? 'Presença atualizada.'
                          : estadoAtual.mensagemDeErro ??
                              'Não foi possível responder ao convite.',
                    ),
                  ),
                );
              }

              return respondeu;
            },
          ),
      },
    );
  }
}

class _ConteudoDaPagina extends StatelessWidget {
  const _ConteudoDaPagina({
    required this.usuarioAtual,
    required this.encontros,
    required this.convitesPendentes,
    required this.identificadorDoConviteEmAtualizacao,
    required this.aoRecarregar,
    required this.aoAbrirEncontro,
    required this.quantidadeDeNotificacoesNaoLidas,
    required this.aoAbrirNotificacoes,
    required this.aoResponderConvite,
    this.mensagemDeErro,
  });

  final UsuarioAtual usuarioAtual;
  final List<EncontroResumo> encontros;
  final List<ConviteDoEncontroResumo> convitesPendentes;
  final String? identificadorDoConviteEmAtualizacao;
  final String? mensagemDeErro;
  final Future<void> Function() aoRecarregar;
  final Future<void> Function(String identificador) aoAbrirEncontro;
  final int quantidadeDeNotificacoesNaoLidas;
  final Future<void> Function() aoAbrirNotificacoes;
  final Future<bool> Function({
    required String identificadorDoEncontro,
    required String situacao,
  }) aoResponderConvite;

  @override
  Widget build(BuildContext context) {
    EncontroResumo? proximoEncontro = encontros.firstOrNull;
    List<EncontroResumo> outrosEncontros = encontros.skip(1).toList();

    return RefreshIndicator(
      onRefresh: aoRecarregar,
      child: ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        children: <Widget>[
          CabecalhoDaPagina(
            titulo: 'Olá, ${usuarioAtual.primeiroNome}',
            inicio: FotoDePerfil(
              key: const Key('foto-do-usuario-na-inicial'),
              url: usuarioAtual.urlDaFotoDePerfil,
              iniciais: usuarioAtual.iniciais,
              dimensao: 54,
              tamanhoDasIniciais: 19,
            ),
            acoes: <Widget>[
              _AcaoDeNotificacoes(
                quantidadeNaoLida: quantidadeDeNotificacoesNaoLidas,
                aoAbrir: aoAbrirNotificacoes,
              ),
              Semantics(
                identifier: IdentificadoresSemanticos.inicioCriarEncontro,
                child: IconButton.filled(
                  onPressed: () => context.push('/encontros/novo'),
                  tooltip: 'Criar encontro',
                  icon: const Icon(Icons.add_rounded),
                ),
              ),
            ],
          ),
          const SizedBox(height: EspacamentosDoAplicativo.extraGrande),
          if (convitesPendentes.isNotEmpty) ...<Widget>[
            TituloDeSecao(
              titulo: 'Convites pendentes',
              subtitulo: convitesPendentes.length == 1
                  ? 'Um encontro aguarda sua resposta'
                  : '${convitesPendentes.length} encontros aguardam sua resposta',
            ),
            const SizedBox(height: EspacamentosDoAplicativo.medio),
            ...convitesPendentes.map(
              (ConviteDoEncontroResumo convite) => Padding(
                padding: const EdgeInsets.only(
                  bottom: EspacamentosDoAplicativo.medio,
                ),
                child: _ConvitePendente(
                  convite: convite,
                  estaAtualizando: identificadorDoConviteEmAtualizacao ==
                      convite.identificadorDoEncontro,
                  temAtualizacaoEmAndamento:
                      identificadorDoConviteEmAtualizacao != null,
                  aoAbrir: () =>
                      aoAbrirEncontro(convite.identificadorDoEncontro),
                  aoResponder: (String situacao) => aoResponderConvite(
                    identificadorDoEncontro: convite.identificadorDoEncontro,
                    situacao: situacao,
                  ),
                ),
              ),
            ),
            if (mensagemDeErro != null)
              Padding(
                padding: const EdgeInsets.only(
                  bottom: EspacamentosDoAplicativo.medio,
                ),
                child: Text(
                  mensagemDeErro!,
                  textAlign: TextAlign.center,
                  style: const TextStyle(color: CoresDoAplicativo.coral),
                ),
              ),
            const SizedBox(height: EspacamentosDoAplicativo.medio),
          ],
          if (proximoEncontro == null) ...<Widget>[
            const TituloDeSecao(titulo: 'Próximo encontro'),
            const SizedBox(height: EspacamentosDoAplicativo.medio),
            const CartaoDoAplicativo(
              filho: EstadoVazio(
                icone: Icons.event_available_outlined,
                titulo: 'Nenhum encontro marcado',
                descricao:
                    'Quando um novo momento for combinado, ele aparecerá aqui.',
              ),
            ),
          ] else ...<Widget>[
            const TituloDeSecao(titulo: 'Próximo encontro'),
            const SizedBox(height: EspacamentosDoAplicativo.medio),
            _EncontroEmDestaque(
              encontro: proximoEncontro,
              aoAbrir: () => aoAbrirEncontro(proximoEncontro.identificador),
            ),
            if (outrosEncontros.isNotEmpty) ...<Widget>[
              const SizedBox(height: EspacamentosDoAplicativo.grande),
              const TituloDeSecao(titulo: 'Outros encontros'),
              const SizedBox(height: EspacamentosDoAplicativo.medio),
              _ListaDeEncontros(
                encontros: outrosEncontros,
                aoAbrir: aoAbrirEncontro,
              ),
            ],
          ],
          const SizedBox(height: EspacamentosDoAplicativo.grande),
        ],
      ),
    );
  }
}

class _ConvitePendente extends StatelessWidget {
  const _ConvitePendente({
    required this.convite,
    required this.estaAtualizando,
    required this.temAtualizacaoEmAndamento,
    required this.aoAbrir,
    required this.aoResponder,
  });

  final ConviteDoEncontroResumo convite;
  final bool estaAtualizando;
  final bool temAtualizacaoEmAndamento;
  final VoidCallback aoAbrir;
  final Future<bool> Function(String situacao) aoResponder;

  @override
  Widget build(BuildContext context) {
    return CartaoDoAplicativo(
      preenchimento: const EdgeInsets.all(EspacamentosDoAplicativo.medio),
      filho: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: <Widget>[
          InkWell(
            onTap: temAtualizacaoEmAndamento ? null : aoAbrir,
            child: Row(
              children: <Widget>[
                const Icon(
                  Icons.mail_outline_rounded,
                  color: CoresDoAplicativo.ambar,
                ),
                const SizedBox(width: EspacamentosDoAplicativo.pequeno),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: <Widget>[
                      Text(
                        convite.titulo,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                      const SizedBox(
                        height: EspacamentosDoAplicativo.minimo,
                      ),
                      Text(
                        _descrevaConvite(convite),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        style: const TextStyle(
                          color: CoresDoAplicativo.textoSecundario,
                          fontSize: 13,
                        ),
                      ),
                    ],
                  ),
                ),
                if (estaAtualizando)
                  const SizedBox.square(
                    dimension: 18,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                else
                  const Icon(
                    Icons.chevron_right_rounded,
                    color: CoresDoAplicativo.textoTerciario,
                  ),
              ],
            ),
          ),
          const SizedBox(height: EspacamentosDoAplicativo.medio),
          Row(
            children: <Widget>[
              Expanded(
                child: FilledButton(
                  key: Key('convite-vou-${convite.identificadorDoEncontro}'),
                  onPressed: temAtualizacaoEmAndamento
                      ? null
                      : () => aoResponder('Confirmado'),
                  child: const Text('Vou'),
                ),
              ),
              const SizedBox(width: EspacamentosDoAplicativo.pequeno),
              Expanded(
                child: OutlinedButton(
                  key: Key(
                    'convite-talvez-${convite.identificadorDoEncontro}',
                  ),
                  onPressed: temAtualizacaoEmAndamento
                      ? null
                      : () => aoResponder('Talvez'),
                  child: const Text('Talvez'),
                ),
              ),
              const SizedBox(width: EspacamentosDoAplicativo.pequeno),
              Expanded(
                child: TextButton(
                  key: Key(
                    'convite-nao-vou-${convite.identificadorDoEncontro}',
                  ),
                  onPressed: temAtualizacaoEmAndamento
                      ? null
                      : () => aoResponder('NaoVai'),
                  child: const Text('Não vou'),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

String _descrevaConvite(ConviteDoEncontroResumo convite) {
  String data =
      DateFormat("dd/MM/yyyy '•' HH:mm", 'pt_BR').format(convite.inicioEm);
  String? local = convite.local;

  return local == null || local.trim().isEmpty ? data : '$data · $local';
}

class _AcaoDeNotificacoes extends StatelessWidget {
  const _AcaoDeNotificacoes({
    required this.quantidadeNaoLida,
    required this.aoAbrir,
  });

  final int quantidadeNaoLida;
  final VoidCallback aoAbrir;

  @override
  Widget build(BuildContext context) {
    return Stack(
      clipBehavior: Clip.none,
      children: <Widget>[
        IconButton(
          key: const Key('abrir-notificacoes'),
          tooltip: 'Notificações',
          onPressed: aoAbrir,
          icon: const Icon(Icons.notifications_none_rounded),
        ),
        if (quantidadeNaoLida > 0)
          Positioned(
            top: 5,
            right: 5,
            child: Semantics(
              label: '$quantidadeNaoLida notificações não lidas',
              child: Container(
                constraints: const BoxConstraints(minWidth: 16, minHeight: 16),
                padding: const EdgeInsets.symmetric(horizontal: 4),
                decoration: const BoxDecoration(
                  color: CoresDoAplicativo.coral,
                  shape: BoxShape.circle,
                ),
                alignment: Alignment.center,
                child: Text(
                  quantidadeNaoLida > 9 ? '9+' : '$quantidadeNaoLida',
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 9,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ),
            ),
          ),
      ],
    );
  }
}

class _EncontroEmDestaque extends StatelessWidget {
  const _EncontroEmDestaque({
    required this.encontro,
    required this.aoAbrir,
  });

  final EncontroResumo encontro;
  final VoidCallback aoAbrir;

  @override
  Widget build(BuildContext context) {
    String recurso = ConfiguracaoDoAmbiente.crieUrlAbsoluta(
      encontro.urlDaImagemDeCapa,
    );

    return Semantics(
      button: true,
      label: _descrevaEncontroParaAcessibilidade(encontro),
      child: ExcludeSemantics(
        child: Material(
          color: CoresDoAplicativo.transparente,
          child: InkWell(
            onTap: aoAbrir,
            borderRadius: BorderRadius.circular(RaiosDoAplicativo.grande),
            child: Ink(
              height: 300,
              decoration: BoxDecoration(
                color: CoresDoAplicativo.fundoDoCartao,
                borderRadius: BorderRadius.circular(RaiosDoAplicativo.grande),
                border: Border.all(color: CoresDoAplicativo.bordaDiscreta),
              ),
              child: ClipRRect(
                borderRadius: BorderRadius.circular(RaiosDoAplicativo.grande),
                child: Stack(
                  fit: StackFit.expand,
                  children: <Widget>[
                    if (recurso.isEmpty)
                      _FundoPadraoDoEncontro(inicioEm: encontro.inicioEm)
                    else
                      ImagemPrivada(
                        recurso: recurso,
                        construaSubstituta: (_) => _FundoPadraoDoEncontro(
                          inicioEm: encontro.inicioEm,
                        ),
                      ),
                    const DecoratedBox(
                      decoration: BoxDecoration(
                        gradient: LinearGradient(
                          begin: Alignment.topCenter,
                          end: Alignment.bottomCenter,
                          colors: <Color>[
                            Color(0x18000000),
                            Color(0x50000000),
                            Color(0xF2050D0B),
                          ],
                        ),
                      ),
                    ),
                    Positioned(
                      top: EspacamentosDoAplicativo.padrao,
                      left: EspacamentosDoAplicativo.padrao,
                      child: _DataDoEncontro(inicioEm: encontro.inicioEm),
                    ),
                    if (encontro.quantidadeDeNovidades > 0)
                      Positioned(
                        top: EspacamentosDoAplicativo.padrao,
                        right: EspacamentosDoAplicativo.padrao,
                        child: _IndicadorDeNovidades(
                          encontro: encontro,
                        ),
                      ),
                    Positioned(
                      left: EspacamentosDoAplicativo.padrao,
                      right: EspacamentosDoAplicativo.padrao,
                      bottom: EspacamentosDoAplicativo.padrao,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: <Widget>[
                          Text(
                            encontro.titulo,
                            maxLines: 2,
                            overflow: TextOverflow.ellipsis,
                            style: Theme.of(context).textTheme.headlineSmall,
                          ),
                          const SizedBox(
                            height: EspacamentosDoAplicativo.pequeno,
                          ),
                          _LinhaDoEncontro(
                            icone: Icons.schedule_rounded,
                            texto: DateFormat('HH:mm', 'pt_BR').format(
                              encontro.inicioEm,
                            ),
                          ),
                          if (encontro.local != null &&
                              encontro.local!.trim().isNotEmpty) ...<Widget>[
                            const SizedBox(
                              height: EspacamentosDoAplicativo.minimo,
                            ),
                            _LinhaDoEncontro(
                              icone: Icons.location_on_outlined,
                              texto: encontro.local!,
                            ),
                          ],
                          const SizedBox(
                            height: EspacamentosDoAplicativo.medio,
                          ),
                          IndicadorDeSituacao(
                            texto: encontro.usuarioAtualConfirmouPresenca
                                ? 'Você vai · ${_descrevaPresencas(encontro)}'
                                : _descrevaPresencas(encontro),
                            icone: encontro.usuarioAtualConfirmouPresenca
                                ? Icons.check_circle_outline_rounded
                                : Icons.people_outline_rounded,
                            cor: encontro.usuarioAtualConfirmouPresenca
                                ? CoresDoAplicativo.verdeDestaque
                                : CoresDoAplicativo.textoSecundario,
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _EncontroCompacto extends StatelessWidget {
  const _EncontroCompacto({
    required this.encontro,
    required this.aoAbrir,
  });

  final EncontroResumo encontro;
  final VoidCallback aoAbrir;

  @override
  Widget build(BuildContext context) {
    String recurso = ConfiguracaoDoAmbiente.crieUrlAbsoluta(
      encontro.urlDaImagemDeCapa,
    );

    return Semantics(
      button: true,
      label: _descrevaEncontroParaAcessibilidade(encontro),
      child: ExcludeSemantics(
        child: InkWell(
          onTap: aoAbrir,
          child: SizedBox(
            height: 106,
            child: Row(
              children: <Widget>[
                Padding(
                  padding: const EdgeInsets.all(
                    EspacamentosDoAplicativo.pequeno,
                  ),
                  child: ClipRRect(
                    borderRadius:
                        BorderRadius.circular(RaiosDoAplicativo.medio),
                    child: SizedBox(
                      width: 88,
                      child: recurso.isEmpty
                          ? _FundoPadraoDoEncontro(inicioEm: encontro.inicioEm)
                          : ImagemPrivada(
                              recurso: recurso,
                              construaSubstituta: (_) => _FundoPadraoDoEncontro(
                                inicioEm: encontro.inicioEm,
                              ),
                            ),
                    ),
                  ),
                ),
                Expanded(
                  child: Padding(
                    padding: const EdgeInsets.all(
                      EspacamentosDoAplicativo.medio,
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: <Widget>[
                        Row(
                          children: <Widget>[
                            Expanded(
                              child: Text(
                                encontro.titulo,
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis,
                                style: Theme.of(context).textTheme.titleMedium,
                              ),
                            ),
                            if (encontro.quantidadeDeNovidades > 0) ...<Widget>[
                              const SizedBox(
                                width: EspacamentosDoAplicativo.pequeno,
                              ),
                              _IndicadorDeNovidades(encontro: encontro),
                            ],
                          ],
                        ),
                        const SizedBox(
                            height: EspacamentosDoAplicativo.pequeno),
                        _LinhaDoEncontro(
                          icone: Icons.calendar_today_outlined,
                          texto: DateFormat("dd/MM/yyyy '•' HH:mm", 'pt_BR')
                              .format(encontro.inicioEm),
                        ),
                        if (encontro.local != null &&
                            encontro.local!.trim().isNotEmpty) ...<Widget>[
                          const SizedBox(
                              height: EspacamentosDoAplicativo.minimo),
                          _LinhaDoEncontro(
                            icone: Icons.location_on_outlined,
                            texto: encontro.local!,
                          ),
                        ],
                      ],
                    ),
                  ),
                ),
                const Padding(
                  padding:
                      EdgeInsets.only(right: EspacamentosDoAplicativo.medio),
                  child: Icon(
                    Icons.chevron_right_rounded,
                    color: CoresDoAplicativo.textoTerciario,
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _ListaDeEncontros extends StatelessWidget {
  const _ListaDeEncontros({
    required this.encontros,
    required this.aoAbrir,
  });

  final List<EncontroResumo> encontros;
  final Future<void> Function(String identificador) aoAbrir;

  @override
  Widget build(BuildContext context) {
    BorderRadius raio = BorderRadius.circular(RaiosDoAplicativo.grande);

    return DecoratedBox(
      decoration: BoxDecoration(
        color: CoresDoAplicativo.fundoDoCartao,
        borderRadius: raio,
        border: Border.all(color: CoresDoAplicativo.bordaDiscreta),
      ),
      child: ClipRRect(
        borderRadius: raio,
        child: Material(
          color: CoresDoAplicativo.transparente,
          child: Column(
            children: <Widget>[
              for (int indice = 0;
                  indice < encontros.length;
                  indice++) ...<Widget>[
                _EncontroCompacto(
                  encontro: encontros[indice],
                  aoAbrir: () => aoAbrir(encontros[indice].identificador),
                ),
                if (indice < encontros.length - 1)
                  const Divider(
                    height: 1,
                    indent: 112,
                    endIndent: EspacamentosDoAplicativo.medio,
                  ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}

class _IndicadorDeNovidades extends StatelessWidget {
  const _IndicadorDeNovidades({required this.encontro});

  final EncontroResumo encontro;

  @override
  Widget build(BuildContext context) {
    int quantidade = encontro.quantidadeDeNovidades;

    return Container(
      key: Key('novidades-${encontro.identificador}'),
      constraints: const BoxConstraints(minWidth: 28, minHeight: 24),
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: CoresDoAplicativo.coral,
        borderRadius: BorderRadius.circular(RaiosDoAplicativo.grande),
      ),
      alignment: Alignment.center,
      child: Text(
        quantidade > 99 ? '99+' : '$quantidade',
        style: const TextStyle(
          color: Colors.white,
          fontSize: 11,
          fontWeight: FontWeight.w700,
        ),
      ),
    );
  }
}

class _FundoPadraoDoEncontro extends StatelessWidget {
  const _FundoPadraoDoEncontro({required this.inicioEm});

  final DateTime inicioEm;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: <Color>[
            Color(0xFF24272C),
            Color(0xFF343037),
            Color(0xFF6A4615),
          ],
        ),
      ),
      child: Center(
        child: Icon(
          Icons.celebration_outlined,
          size: 42,
          color: CoresDoAplicativo.textoPrincipal.withValues(alpha: 0.75),
        ),
      ),
    );
  }
}

class _DataDoEncontro extends StatelessWidget {
  const _DataDoEncontro({required this.inicioEm});

  final DateTime inicioEm;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      decoration: BoxDecoration(
        color: CoresDoAplicativo.fundoPrincipal.withValues(alpha: 0.82),
        borderRadius: BorderRadius.circular(RaiosDoAplicativo.medio),
        border: Border.all(color: CoresDoAplicativo.bordaSuave),
      ),
      child: Column(
        children: <Widget>[
          Text(
            DateFormat('dd').format(inicioEm),
            style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w700),
          ),
          Text(
            DateFormat('MMM', 'pt_BR').format(inicioEm).replaceAll('.', ''),
            style: const TextStyle(
              color: CoresDoAplicativo.ambar,
              fontSize: 11,
              fontWeight: FontWeight.w700,
            ),
          ),
        ],
      ),
    );
  }
}

class _LinhaDoEncontro extends StatelessWidget {
  const _LinhaDoEncontro({required this.icone, required this.texto});

  final IconData icone;
  final String texto;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: <Widget>[
        Icon(icone, size: 15, color: CoresDoAplicativo.textoSecundario),
        const SizedBox(width: EspacamentosDoAplicativo.pequeno),
        Expanded(
          child: Text(
            texto,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(
              color: CoresDoAplicativo.textoSecundario,
              fontSize: 13,
            ),
          ),
        ),
      ],
    );
  }
}

String _descrevaPresencas(EncontroResumo encontro) {
  int quantidade = encontro.quantidadeDePresencasConfirmadas;

  return quantidade == 1
      ? '1 presença confirmada'
      : '$quantidade presenças confirmadas';
}

String _descrevaEncontroParaAcessibilidade(EncontroResumo encontro) {
  int quantidade = encontro.quantidadeDeNovidades;
  String descricaoDeNovidades = quantidade == 1
      ? ', 1 novidade'
      : quantidade > 1
          ? ', $quantidade novidades'
          : '';

  return 'Abrir encontro ${encontro.titulo}$descricaoDeNovidades';
}

class _CarregamentoDaPagina extends StatelessWidget {
  const _CarregamentoDaPagina();

  @override
  Widget build(BuildContext context) {
    return const Center(child: CircularProgressIndicator());
  }
}

class _ErroDaPagina extends StatelessWidget {
  const _ErroDaPagina({
    required this.mensagem,
    required this.aoTentarNovamente,
  });

  final String mensagem;
  final Future<void> Function() aoTentarNovamente;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: CartaoDoAplicativo(
        elevado: true,
        filho: EstadoVazio(
          icone: Icons.cloud_off_outlined,
          titulo: 'Não foi possível carregar',
          descricao: mensagem,
          acao: FilledButton.icon(
            onPressed: aoTentarNovamente,
            icon: const Icon(Icons.refresh_rounded),
            label: const Text('Tentar novamente'),
          ),
        ),
      ),
    );
  }
}
