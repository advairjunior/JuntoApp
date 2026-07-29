import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/conteudo_responsivo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/indicador_de_situacao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/configuracao/configuracao_do_ambiente.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/foto_de_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/imagem_privada.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/repositorio_de_imagens_privadas.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/raios_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/componentes/folha_de_resposta_de_presenca.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/componentes/folha_de_origem_da_imagem.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/estado/controlador_do_detalhe_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/estado/estado_do_detalhe_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_detalhado.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/participante_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/servicos/seletor_de_imagem.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/servicos/servico_de_localizacao.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/estado/controlador_da_pagina_inicial.dart';

class TelaDeDetalheDoEncontro extends ConsumerWidget {
  const TelaDeDetalheDoEncontro({
    required this.identificadorDoEncontro,
    super.key,
  });

  final String identificadorDoEncontro;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    EstadoDoDetalheDoEncontro estado = ref.watch(
      provedorDoControladorDoDetalheDoEncontro(identificadorDoEncontro),
    );

    return Scaffold(
      body: SafeArea(
        child: ConteudoResponsivo(
          filho: switch (estado.situacao) {
            SituacaoDoDetalheDoEncontro.carregando =>
              const Center(child: CircularProgressIndicator()),
            SituacaoDoDetalheDoEncontro.falhou => _ErroDoDetalhe(
                mensagem: estado.mensagemDeErro ??
                    'Não foi possível carregar o encontro.',
                aoVoltar: () => _volte(context),
                aoTentarNovamente: () => ref
                    .read(
                      provedorDoControladorDoDetalheDoEncontro(
                        identificadorDoEncontro,
                      ).notifier,
                    )
                    .carregueAsync(),
              ),
            SituacaoDoDetalheDoEncontro.carregado => _ConteudoDoDetalhe(
                encontro: estado.encontro!,
                estaAtualizandoPresenca: estado.estaAtualizandoPresenca,
                estaExecutandoAcaoDoOrganizador:
                    estado.estaExecutandoAcaoDoOrganizador,
                mensagemDeErro: estado.mensagemDeErro,
                mensagemDeSucesso: estado.mensagemDeSucesso,
                aoVoltar: () => _volte(context),
                aoRecarregar: () => ref
                    .read(
                      provedorDoControladorDoDetalheDoEncontro(
                        identificadorDoEncontro,
                      ).notifier,
                    )
                    .carregueAsync(),
                aoAlterarPresenca: (String situacao) async {
                  bool respondeu = await ref
                      .read(
                        provedorDoControladorDoDetalheDoEncontro(
                          identificadorDoEncontro,
                        ).notifier,
                      )
                      .respondaPresencaAsync(situacao);

                  if (respondeu) {
                    await ref
                        .read(provedorDoControladorDaPaginaInicial.notifier)
                        .carregueAsync();
                  }

                  return respondeu;
                },
                aoEditar: () async {
                  bool? encontroFoiEditado = await context.push<bool>(
                    '/encontros/$identificadorDoEncontro/editar',
                  );

                  if (encontroFoiEditado == true && context.mounted) {
                    await ref
                        .read(
                          provedorDoControladorDoDetalheDoEncontro(
                            identificadorDoEncontro,
                          ).notifier,
                        )
                        .carregueAsync();
                    await ref
                        .read(provedorDoControladorDaPaginaInicial.notifier)
                        .carregueAsync();
                  }
                },
                aoCancelar: () async {
                  bool cancelou = await ref
                      .read(
                        provedorDoControladorDoDetalheDoEncontro(
                          identificadorDoEncontro,
                        ).notifier,
                      )
                      .canceleEncontroAsync();

                  if (cancelou) {
                    await ref
                        .read(provedorDoControladorDaPaginaInicial.notifier)
                        .carregueAsync();
                  }

                  return cancelou;
                },
                aoMarcarComoRealizado: () async {
                  bool marcouComoRealizado = await ref
                      .read(
                        provedorDoControladorDoDetalheDoEncontro(
                          identificadorDoEncontro,
                        ).notifier,
                      )
                      .marqueEncontroComoRealizadoAsync();

                  if (marcouComoRealizado) {
                    await ref
                        .read(provedorDoControladorDaPaginaInicial.notifier)
                        .carregueAsync();
                  }

                  return marcouComoRealizado;
                },
                aoAlterarCapa: () async {
                  String recursoAnterior =
                      ConfiguracaoDoAmbiente.crieUrlAbsoluta(
                    estado.encontro?.urlDaImagemDeCapa,
                  );
                  EnumeradorDeOrigemDaImagem? origem =
                      await escolhaOrigemDaImagemAsync(
                    context,
                    titulo: 'Imagem do encontro',
                  );

                  if (origem == null || !context.mounted) {
                    return;
                  }

                  ImagemSelecionada? imagem = await ref
                      .read(provedorDoSeletorDeImagem)
                      .selecionePorOrigemAsync(origem);

                  if (imagem == null || !context.mounted) {
                    return;
                  }

                  if (imagem.conteudo.lengthInBytes > 5 * 1024 * 1024) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(
                        content: Text(
                          'A imagem do encontro não pode ultrapassar 5 MB.',
                        ),
                      ),
                    );
                    return;
                  }

                  bool alterou = await ref
                      .read(
                        provedorDoControladorDoDetalheDoEncontro(
                          identificadorDoEncontro,
                        ).notifier,
                      )
                      .altereImagemDeCapaAsync(
                        nomeDoArquivo: imagem.nome,
                        tipoDeConteudo: imagem.tipoDeConteudo,
                        conteudo: imagem.conteudo,
                      );

                  if (alterou) {
                    _invalideCapas(
                      ref,
                      identificadorDoEncontro,
                      recursoAnterior,
                    );
                  }
                },
                aoRemoverCapa: () async {
                  String recursoAnterior =
                      ConfiguracaoDoAmbiente.crieUrlAbsoluta(
                    estado.encontro?.urlDaImagemDeCapa,
                  );
                  bool removeu = await ref
                      .read(
                        provedorDoControladorDoDetalheDoEncontro(
                          identificadorDoEncontro,
                        ).notifier,
                      )
                      .removaImagemDeCapaAsync();

                  if (removeu) {
                    _invalideCapas(
                      ref,
                      identificadorDoEncontro,
                      recursoAnterior,
                    );
                  }

                  return removeu;
                },
                aoAbrirParticipantes: () => context.push<void>(
                  '/encontros/$identificadorDoEncontro/participantes',
                ),
                aoAbrirMidias: () => context.push<void>(
                  '/encontros/$identificadorDoEncontro/midias',
                ),
                aoAbrirCombinados: () => context.push<void>(
                  '/encontros/$identificadorDoEncontro/combinados',
                ),
              ),
          },
        ),
      ),
    );
  }

  void _volte(BuildContext context) {
    if (context.canPop()) {
      context.pop();
      return;
    }

    context.go('/inicio');
  }

  void _invalideCapas(
    WidgetRef ref,
    String identificadorDoEncontro,
    String recursoAnterior,
  ) {
    if (recursoAnterior.isNotEmpty) {
      ref.invalidate(provedorDosBytesDaImagemPrivada(recursoAnterior));
    }

    EstadoDoDetalheDoEncontro estadoAtual = ref.read(
      provedorDoControladorDoDetalheDoEncontro(identificadorDoEncontro),
    );
    String recursoAtual = ConfiguracaoDoAmbiente.crieUrlAbsoluta(
      estadoAtual.encontro?.urlDaImagemDeCapa,
    );

    if (recursoAtual.isNotEmpty && recursoAtual != recursoAnterior) {
      ref.invalidate(provedorDosBytesDaImagemPrivada(recursoAtual));
    }

    ref.invalidate(provedorDoControladorDaPaginaInicial);
  }
}

class _ConteudoDoDetalhe extends StatelessWidget {
  const _ConteudoDoDetalhe({
    required this.encontro,
    required this.estaAtualizandoPresenca,
    required this.estaExecutandoAcaoDoOrganizador,
    required this.aoVoltar,
    required this.aoRecarregar,
    required this.aoAlterarPresenca,
    required this.aoEditar,
    required this.aoCancelar,
    required this.aoMarcarComoRealizado,
    required this.aoAlterarCapa,
    required this.aoRemoverCapa,
    required this.aoAbrirParticipantes,
    required this.aoAbrirMidias,
    required this.aoAbrirCombinados,
    this.mensagemDeErro,
    this.mensagemDeSucesso,
  });

  final EncontroDetalhado encontro;
  final bool estaAtualizandoPresenca;
  final bool estaExecutandoAcaoDoOrganizador;
  final String? mensagemDeErro;
  final String? mensagemDeSucesso;
  final VoidCallback aoVoltar;
  final Future<void> Function() aoRecarregar;
  final Future<bool> Function(String situacao) aoAlterarPresenca;
  final Future<void> Function() aoEditar;
  final Future<bool> Function() aoCancelar;
  final Future<bool> Function() aoMarcarComoRealizado;
  final Future<void> Function() aoAlterarCapa;
  final Future<bool> Function() aoRemoverCapa;
  final Future<void> Function() aoAbrirParticipantes;
  final Future<void> Function() aoAbrirMidias;
  final Future<void> Function() aoAbrirCombinados;

  @override
  Widget build(BuildContext context) {
    return RefreshIndicator(
      onRefresh: aoRecarregar,
      child: ListView(
        key: const Key('lista-dos-detalhes-do-encontro'),
        physics: const AlwaysScrollableScrollPhysics(),
        children: <Widget>[
          _CabecalhoDoDetalhe(
            encontro: encontro,
            aoVoltar: aoVoltar,
            estaExecutando: estaExecutandoAcaoDoOrganizador,
            aoAlterarCapa: aoAlterarCapa,
            aoRemoverCapa: () => _confirmeRemocaoDaCapaAsync(context),
          ),
          if (mensagemDeErro != null) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.medio),
            Text(
              mensagemDeErro!,
              textAlign: TextAlign.center,
              style: const TextStyle(color: CoresDoAplicativo.coral),
            ),
          ],
          if (mensagemDeSucesso != null) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.medio),
            Text(
              mensagemDeSucesso!,
              textAlign: TextAlign.center,
              style: const TextStyle(
                color: CoresDoAplicativo.verdeDestaque,
              ),
            ),
          ],
          const SizedBox(height: EspacamentosDoAplicativo.grande),
          _ResumoDoEncontro(encontro: encontro),
          const SizedBox(height: EspacamentosDoAplicativo.padrao),
          _PresencaDoUsuario(
            encontro: encontro,
            estaAtualizando: estaAtualizandoPresenca,
            aoAlterar: () => _abraOpcoesDePresencaAsync(context),
          ),
          const SizedBox(height: EspacamentosDoAplicativo.padrao),
          _QuemVai(
            participantes: encontro.participantes,
            aoAbrirParticipantes: aoAbrirParticipantes,
          ),
          if (encontro.preferenciasDoAniversario?.temAlgumaInformacao ??
              false) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.padrao),
            _ResumoDasPreferenciasDoAniversario(
              encontro: encontro,
              aoTocar: () => _abraDetalhesDoAniversarioAsync(context),
            ),
          ],
          const SizedBox(height: EspacamentosDoAplicativo.padrao),
          _NavegacaoDoEncontro(
            aoAbrirCombinados: aoAbrirCombinados,
            aoAbrirMidias: aoAbrirMidias,
          ),
          if (encontro.podeEditar || encontro.podeCancelar) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.padrao),
            _EntradaDeGerenciamento(
              estaExecutando: estaExecutandoAcaoDoOrganizador,
              aoTocar: () => _abraGerenciamentoAsync(context),
            ),
          ],
          const SizedBox(height: EspacamentosDoAplicativo.grande),
        ],
      ),
    );
  }

  Future<void> _abraOpcoesDePresencaAsync(BuildContext context) async {
    if (estaAtualizandoPresenca ||
        encontro.situacao.toLowerCase() != 'planejado') {
      return;
    }

    String? situacao = await mostreFolhaDeRespostaDePresencaAsync(context);

    if (situacao != null) {
      await aoAlterarPresenca(situacao);
    }
  }

  Future<void> _abraDetalhesDoAniversarioAsync(
    BuildContext context,
  ) async {
    await showModalBottomSheet<void>(
      context: context,
      backgroundColor: CoresDoAplicativo.fundoDoCartao,
      isScrollControlled: true,
      showDragHandle: true,
      useSafeArea: true,
      builder: (BuildContext contextoDaFolha) {
        return SingleChildScrollView(
          padding: const EdgeInsets.fromLTRB(
            EspacamentosDoAplicativo.grande,
            0,
            EspacamentosDoAplicativo.grande,
            EspacamentosDoAplicativo.grande,
          ),
          child: _DetalhesDasPreferenciasDoAniversario(
            encontro: encontro,
          ),
        );
      },
    );
  }

  Future<void> _abraGerenciamentoAsync(BuildContext context) async {
    if (estaExecutandoAcaoDoOrganizador) {
      return;
    }

    String? acao = await showModalBottomSheet<String>(
      context: context,
      backgroundColor: CoresDoAplicativo.fundoDoCartao,
      isScrollControlled: true,
      showDragHandle: true,
      useSafeArea: true,
      builder: (BuildContext contextoDaFolha) {
        return SingleChildScrollView(
          child: _AcoesDoOrganizador(
            podeEditar: encontro.podeEditar,
            podeCancelar: encontro.podeCancelar,
            estaExecutando: estaExecutandoAcaoDoOrganizador,
            podeMarcarComoRealizado: encontro.podeEditar &&
                encontro.situacao.toLowerCase() == 'planejado',
            aoEditar: () async {
              Navigator.of(contextoDaFolha).pop('editar');
            },
            aoCancelar: () async {
              Navigator.of(contextoDaFolha).pop('cancelar');
            },
            aoMarcarComoRealizado: () async {
              Navigator.of(contextoDaFolha).pop('realizar');
            },
          ),
        );
      },
    );

    if (!context.mounted) {
      return;
    }

    switch (acao) {
      case 'editar':
        await aoEditar();
      case 'realizar':
        await _confirmeRealizacaoAsync(context);
      case 'cancelar':
        await _confirmeCancelamentoAsync(context);
    }
  }

  Future<void> _confirmeCancelamentoAsync(BuildContext context) async {
    bool? deveCancelar = await showDialog<bool>(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('Cancelar encontro?'),
          content: const Text(
            'Os participantes verão que este encontro foi cancelado. Esta ação não pode ser desfeita.',
          ),
          actions: <Widget>[
            TextButton(
              onPressed: () => context.pop(false),
              child: const Text('Manter encontro'),
            ),
            FilledButton(
              key: const Key('confirmar-cancelamento'),
              onPressed: () => context.pop(true),
              style: FilledButton.styleFrom(
                backgroundColor: CoresDoAplicativo.coral,
              ),
              child: const Text('Cancelar encontro'),
            ),
          ],
        );
      },
    );

    if (deveCancelar == true) {
      await aoCancelar();
    }
  }

  Future<void> _confirmeRealizacaoAsync(BuildContext context) async {
    bool? deveMarcarComoRealizado = await showDialog<bool>(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('Marcar encontro como realizado?'),
          content: const Text(
            'O encontro será encerrado e ficará disponível no histórico.',
          ),
          actions: <Widget>[
            TextButton(
              onPressed: () => context.pop(false),
              child: const Text('Agora não'),
            ),
            FilledButton.icon(
              key: const Key('confirmar-encontro-realizado'),
              onPressed: () => context.pop(true),
              icon: const Icon(Icons.task_alt_rounded),
              label: const Text('Marcar como realizado'),
            ),
          ],
        );
      },
    );

    if (deveMarcarComoRealizado == true) {
      await aoMarcarComoRealizado();
    }
  }

  Future<void> _confirmeRemocaoDaCapaAsync(BuildContext context) async {
    bool? deveRemover = await showDialog<bool>(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('Remover imagem de capa?'),
          content: const Text(
            'O encontro voltará a usar a imagem padrão.',
          ),
          actions: <Widget>[
            TextButton(
              onPressed: () => context.pop(false),
              child: const Text('Manter imagem'),
            ),
            FilledButton(
              key: const Key('confirmar-remocao-da-capa'),
              onPressed: () => context.pop(true),
              style: FilledButton.styleFrom(
                backgroundColor: CoresDoAplicativo.coral,
              ),
              child: const Text('Remover imagem'),
            ),
          ],
        );
      },
    );

    if (deveRemover == true) {
      await aoRemoverCapa();
    }
  }
}

class _AcoesDoOrganizador extends StatelessWidget {
  const _AcoesDoOrganizador({
    required this.podeEditar,
    required this.podeCancelar,
    required this.podeMarcarComoRealizado,
    required this.estaExecutando,
    required this.aoEditar,
    required this.aoCancelar,
    required this.aoMarcarComoRealizado,
  });

  final bool podeEditar;
  final bool podeCancelar;
  final bool podeMarcarComoRealizado;
  final bool estaExecutando;
  final Future<void> Function() aoEditar;
  final Future<void> Function() aoCancelar;
  final Future<void> Function() aoMarcarComoRealizado;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(
        EspacamentosDoAplicativo.grande,
        0,
        EspacamentosDoAplicativo.grande,
        EspacamentosDoAplicativo.grande,
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: <Widget>[
          Text(
            'Gerenciar encontro',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: EspacamentosDoAplicativo.minimo),
          const Text(
            'Escolha o que deseja fazer como organizador.',
            style: TextStyle(color: CoresDoAplicativo.textoSecundario),
          ),
          const SizedBox(height: EspacamentosDoAplicativo.medio),
          _SuperficieAgrupada(
            filho: Column(
              children: <Widget>[
                if (podeEditar)
                  _LinhaDeAcao(
                    key: const Key('editar-encontro'),
                    icone: Icons.edit_outlined,
                    titulo: 'Editar encontro',
                    subtitulo: 'Alterar informações e detalhes',
                    aoTocar: estaExecutando ? null : aoEditar,
                  ),
                if (podeEditar && podeMarcarComoRealizado)
                  const _DivisorAgrupado(),
                if (podeMarcarComoRealizado)
                  _LinhaDeAcao(
                    key: const Key('marcar-encontro-como-realizado'),
                    icone: Icons.task_alt_rounded,
                    titulo: 'Marcar como realizado',
                    subtitulo: 'Mover este encontro para suas memórias',
                    cor: CoresDoAplicativo.verdeDestaque,
                    estaExecutando: estaExecutando,
                    aoTocar: estaExecutando ? null : aoMarcarComoRealizado,
                  ),
                if ((podeEditar || podeMarcarComoRealizado) && podeCancelar)
                  const _DivisorAgrupado(),
                if (podeCancelar)
                  _LinhaDeAcao(
                    key: const Key('cancelar-encontro'),
                    icone: Icons.event_busy_outlined,
                    titulo: 'Cancelar encontro',
                    subtitulo: 'Informar aos participantes que não acontecerá',
                    cor: CoresDoAplicativo.coral,
                    estaExecutando: estaExecutando,
                    aoTocar: estaExecutando ? null : aoCancelar,
                  ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _EntradaDeGerenciamento extends StatelessWidget {
  const _EntradaDeGerenciamento({
    required this.estaExecutando,
    required this.aoTocar,
  });

  final bool estaExecutando;
  final VoidCallback aoTocar;

  @override
  Widget build(BuildContext context) {
    return _SecaoAgrupada(
      titulo: 'Organização',
      corDoTitulo: CoresDoAplicativo.textoSecundario,
      filho: _SuperficieAgrupada(
        filho: _LinhaDeAcao(
          key: const Key('gerenciar-encontro'),
          icone: Icons.tune_rounded,
          titulo: 'Gerenciar encontro',
          subtitulo: 'Editar, concluir ou cancelar este encontro',
          estaExecutando: estaExecutando,
          aoTocar: estaExecutando ? null : aoTocar,
        ),
      ),
    );
  }
}

class _CabecalhoDoDetalhe extends StatelessWidget {
  const _CabecalhoDoDetalhe({
    required this.encontro,
    required this.aoVoltar,
    required this.estaExecutando,
    required this.aoAlterarCapa,
    required this.aoRemoverCapa,
  });

  final EncontroDetalhado encontro;
  final VoidCallback aoVoltar;
  final bool estaExecutando;
  final Future<void> Function() aoAlterarCapa;
  final Future<void> Function() aoRemoverCapa;

  @override
  Widget build(BuildContext context) {
    String urlDaImagem = ConfiguracaoDoAmbiente.crieUrlAbsoluta(
      encontro.urlDaImagemDeCapa,
    );

    double larguraDaTela = MediaQuery.sizeOf(context).width;
    double alturaDoHero = (larguraDaTela * 0.76).clamp(236.0, 288.0);

    return Material(
      color: Colors.transparent,
      child: ClipRRect(
        borderRadius: BorderRadius.circular(RaiosDoAplicativo.extraGrande),
        child: SizedBox(
          width: double.infinity,
          height: alturaDoHero,
          child: Stack(
            fit: StackFit.expand,
            children: <Widget>[
              if (urlDaImagem.isEmpty)
                const _CapaPadraoDoEncontro()
              else
                ImagemPrivada(
                  recurso: urlDaImagem,
                  construaSubstituta: (_) => const _CapaPadraoDoEncontro(),
                ),
              const DecoratedBox(
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    begin: Alignment.topCenter,
                    end: Alignment.bottomCenter,
                    stops: <double>[0, 0.42, 1],
                    colors: <Color>[
                      Color(0x59000000),
                      Color(0x16000000),
                      Color(0xF2050D0B),
                    ],
                  ),
                ),
              ),
              Positioned.fill(
                child: Material(
                  color: Colors.transparent,
                  child: InkWell(
                    key: const Key('visualizar-capa'),
                    onTap: () => _aoTocarNaCapaAsync(context, urlDaImagem),
                  ),
                ),
              ),
              Positioned(
                top: EspacamentosDoAplicativo.medio,
                left: EspacamentosDoAplicativo.medio,
                child: _BotaoTranslucidoDoHero(
                  tooltip: 'Voltar',
                  icone: Icons.arrow_back_ios_new_rounded,
                  aoTocar: aoVoltar,
                ),
              ),
              const Positioned(
                top: 24,
                left: 72,
                right: 72,
                child: Text(
                  'Informações do encontro',
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  textAlign: TextAlign.center,
                  style: TextStyle(
                    color: CoresDoAplicativo.textoPrincipal,
                    fontSize: 13,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
              if (encontro.podeEditar)
                Positioned(
                  top: EspacamentosDoAplicativo.medio,
                  right: EspacamentosDoAplicativo.medio,
                  child: _BotaoTranslucidoDoHero(
                    key: const Key('gerenciar-capa'),
                    tooltip: 'Alterar imagem de capa',
                    icone: Icons.camera_alt_outlined,
                    aoTocar: estaExecutando
                        ? null
                        : () => _abraOpcoesDaCapaAsync(
                              context,
                              urlDaImagem.isNotEmpty,
                            ),
                  ),
                ),
              Positioned(
                left: EspacamentosDoAplicativo.grande,
                right: EspacamentosDoAplicativo.grande,
                bottom: EspacamentosDoAplicativo.grande,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisSize: MainAxisSize.min,
                  children: <Widget>[
                    Wrap(
                      spacing: EspacamentosDoAplicativo.pequeno,
                      runSpacing: EspacamentosDoAplicativo.pequeno,
                      children: <Widget>[
                        _EtiquetaDeSituacao(situacao: encontro.situacao),
                        if (encontro.tipo != null &&
                            encontro.tipo!.trim().isNotEmpty)
                          IndicadorDeSituacao(
                            key: const Key('tipo-do-encontro-no-detalhe'),
                            texto: encontro.tipo!,
                            cor: CoresDoAplicativo.verdeDestaque,
                            icone: Icons.category_outlined,
                          ),
                      ],
                    ),
                    const SizedBox(height: EspacamentosDoAplicativo.medio),
                    Text(
                      encontro.titulo,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                      style:
                          Theme.of(context).textTheme.headlineMedium?.copyWith(
                                color: CoresDoAplicativo.textoPrincipal,
                                fontWeight: FontWeight.w700,
                                height: 1.08,
                              ),
                    ),
                    if (encontro.descricao != null &&
                        encontro.descricao!.trim().isNotEmpty) ...<Widget>[
                      const SizedBox(
                        height: EspacamentosDoAplicativo.pequeno,
                      ),
                      Text(
                        encontro.descricao!,
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                        style: const TextStyle(
                          color: CoresDoAplicativo.textoSecundario,
                          height: 1.35,
                        ),
                      ),
                    ],
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _aoTocarNaCapaAsync(
    BuildContext context,
    String urlDaImagem,
  ) async {
    if (urlDaImagem.isEmpty) {
      if (encontro.podeEditar) {
        await _abraOpcoesDaCapaAsync(context, false);
      }

      return;
    }

    await showDialog<void>(
      context: context,
      useSafeArea: false,
      builder: (BuildContext contextoDoDialogo) {
        return Dialog.fullscreen(
          backgroundColor: Colors.black,
          child: Scaffold(
            backgroundColor: Colors.black,
            appBar: AppBar(
              backgroundColor: Colors.black,
              foregroundColor: Colors.white,
              title: Text(encontro.titulo),
              leading: IconButton(
                tooltip: 'Fechar',
                onPressed: () => Navigator.of(contextoDoDialogo).pop(),
                icon: const Icon(Icons.close_rounded),
              ),
              actions: <Widget>[
                if (encontro.podeEditar)
                  IconButton(
                    tooltip: 'Alterar imagem de capa',
                    onPressed: () {
                      Navigator.of(contextoDoDialogo).pop();
                      _abraOpcoesDaCapaAsync(context, true);
                    },
                    icon: const Icon(Icons.edit_outlined),
                  ),
              ],
            ),
            body: InteractiveViewer(
              minScale: 0.8,
              maxScale: 4,
              child: Center(
                child: ImagemPrivada(
                  recurso: urlDaImagem,
                  ajuste: BoxFit.contain,
                  construaSubstituta: (_) => const Icon(
                    Icons.broken_image_outlined,
                    size: 52,
                    color: Colors.white54,
                  ),
                ),
              ),
            ),
          ),
        );
      },
    );
  }

  Future<void> _abraOpcoesDaCapaAsync(
    BuildContext context,
    bool temImagemDeCapa,
  ) async {
    String? acao = await showModalBottomSheet<String>(
      context: context,
      backgroundColor: CoresDoAplicativo.fundoDoCartao,
      showDragHandle: true,
      builder: (BuildContext contextoDaFolha) {
        return SafeArea(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(16, 0, 16, 20),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: <Widget>[
                Text(
                  'Imagem do encontro',
                  style: Theme.of(context).textTheme.titleLarge,
                ),
                const SizedBox(height: EspacamentosDoAplicativo.minimo),
                const Text(
                  'Escolha como deseja apresentar este encontro.',
                  style: TextStyle(color: CoresDoAplicativo.textoSecundario),
                ),
                const SizedBox(height: EspacamentosDoAplicativo.medio),
                ListTile(
                  key: const Key('alterar-capa'),
                  leading: const Icon(Icons.add_photo_alternate_outlined),
                  title: const Text('Escolher nova imagem'),
                  onTap: () => Navigator.of(contextoDaFolha).pop('alterar'),
                ),
                if (temImagemDeCapa)
                  ListTile(
                    key: const Key('remover-capa'),
                    leading: const Icon(
                      Icons.hide_image_outlined,
                      color: CoresDoAplicativo.coral,
                    ),
                    title: const Text(
                      'Remover imagem',
                      style: TextStyle(color: CoresDoAplicativo.coral),
                    ),
                    onTap: () => Navigator.of(contextoDaFolha).pop('remover'),
                  ),
              ],
            ),
          ),
        );
      },
    );

    if (acao == 'alterar') {
      await aoAlterarCapa();
    } else if (acao == 'remover') {
      await aoRemoverCapa();
    }
  }
}

class _EtiquetaDeSituacao extends StatelessWidget {
  const _EtiquetaDeSituacao({required this.situacao});

  final String situacao;

  @override
  Widget build(BuildContext context) {
    return IndicadorDeSituacao(
      texto: situacao,
      cor: _corDaSituacao(),
      icone: _iconeDaSituacao(),
    );
  }

  Color _corDaSituacao() {
    return switch (situacao.toLowerCase()) {
      'cancelado' => CoresDoAplicativo.coral,
      'realizado' => CoresDoAplicativo.verdeDestaque,
      _ => CoresDoAplicativo.ambar,
    };
  }

  IconData _iconeDaSituacao() {
    return switch (situacao.toLowerCase()) {
      'cancelado' => Icons.event_busy_outlined,
      'realizado' => Icons.task_alt_rounded,
      _ => Icons.event_available_outlined,
    };
  }
}

class _CapaPadraoDoEncontro extends StatelessWidget {
  const _CapaPadraoDoEncontro();

  @override
  Widget build(BuildContext context) {
    return const DecoratedBox(
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: <Color>[
            CoresDoAplicativo.fundoDoCartaoSuave,
            Color(0xFF5A3B12),
          ],
        ),
      ),
      child: Center(
        child: Icon(
          Icons.celebration_outlined,
          size: 58,
          color: CoresDoAplicativo.ambar,
        ),
      ),
    );
  }
}

class _BotaoTranslucidoDoHero extends StatelessWidget {
  const _BotaoTranslucidoDoHero({
    required this.tooltip,
    required this.icone,
    required this.aoTocar,
    super.key,
  });

  final String tooltip;
  final IconData icone;
  final VoidCallback? aoTocar;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        color: CoresDoAplicativo.fundoPrincipal.withValues(alpha: 0.78),
        border: Border.all(
          color: CoresDoAplicativo.textoPrincipal.withValues(alpha: 0.14),
        ),
      ),
      child: IconButton(
        tooltip: tooltip,
        onPressed: aoTocar,
        icon: Icon(icone, size: 20),
        color: CoresDoAplicativo.textoPrincipal,
        disabledColor: CoresDoAplicativo.textoTerciario,
      ),
    );
  }
}

class _ResumoDoEncontro extends ConsumerWidget {
  const _ResumoDoEncontro({required this.encontro});

  final EncontroDetalhado encontro;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    bool temCoordenadas = encontro.localizacao?.temCoordenadas ?? false;

    return _SecaoAgrupada(
      titulo: 'Quando e onde',
      filho: _SuperficieAgrupada(
        filho: Column(
          children: <Widget>[
            _LinhaDeInformacao(
              icone: Icons.calendar_today_outlined,
              rotulo: 'Data',
              valor: DateFormat("EEEE, dd 'de' MMMM", 'pt_BR')
                  .format(encontro.inicioEm),
            ),
            _LinhaDeInformacao(
              icone: Icons.schedule_rounded,
              rotulo: 'Horário',
              valor: DateFormat('HH:mm', 'pt_BR').format(encontro.inicioEm),
            ),
            _LinhaDeInformacao(
              icone: Icons.location_on_outlined,
              rotulo: 'Local',
              valor: encontro.local == null || encontro.local!.trim().isEmpty
                  ? 'Local a definir'
                  : encontro.local!,
              aoPressionar: temCoordenadas
                  ? () => _abraLocalizacaoNoMapaAsync(context, ref)
                  : null,
              textoDaAcao: temCoordenadas ? 'Abrir no mapa' : null,
            ),
            _LinhaDeInformacao(
              icone: Icons.groups_outlined,
              rotulo: 'Presenças',
              valor:
                  '${encontro.quantidadeDeConfirmados} confirmados  ·  ${encontro.quantidadeDeTalvez} talvez  ·  ${encontro.quantidadeDeAusentes} não vão',
              exibaDivisor: false,
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _abraLocalizacaoNoMapaAsync(
    BuildContext context,
    WidgetRef ref,
  ) async {
    double? latitude = encontro.localizacao?.latitude;
    double? longitude = encontro.localizacao?.longitude;

    if (latitude == null || longitude == null) {
      return;
    }

    await showModalBottomSheet<void>(
      context: context,
      showDragHandle: true,
      builder: (BuildContext contextoDaFolha) {
        return SafeArea(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(
              EspacamentosDoAplicativo.padrao,
              0,
              EspacamentosDoAplicativo.padrao,
              EspacamentosDoAplicativo.padrao,
            ),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: <Widget>[
                const Text(
                  'Abrir localização',
                  style: TextStyle(fontSize: 20, fontWeight: FontWeight.w700),
                ),
                const SizedBox(height: EspacamentosDoAplicativo.minimo),
                const Text(
                  'Escolha o aplicativo que deseja usar para chegar ao encontro.',
                  style: TextStyle(color: CoresDoAplicativo.textoSecundario),
                ),
                const SizedBox(height: EspacamentosDoAplicativo.padrao),
                _OpcaoDeAplicativoDeMapa(
                  chave: const Key('abrir-no-google-maps'),
                  icone: Icons.map_outlined,
                  titulo: 'Google Maps',
                  aoTocar: () {
                    Navigator.of(contextoDaFolha).pop();
                    _abraNoAplicativoDeMapaAsync(
                      context,
                      ref,
                      AplicativoDeMapa.googleMaps,
                      latitude,
                      longitude,
                    );
                  },
                ),
                _OpcaoDeAplicativoDeMapa(
                  chave: const Key('abrir-no-apple-maps'),
                  icone: Icons.navigation_outlined,
                  titulo: 'Mapas da Apple',
                  aoTocar: () {
                    Navigator.of(contextoDaFolha).pop();
                    _abraNoAplicativoDeMapaAsync(
                      context,
                      ref,
                      AplicativoDeMapa.appleMaps,
                      latitude,
                      longitude,
                    );
                  },
                ),
                _OpcaoDeAplicativoDeMapa(
                  chave: const Key('abrir-no-waze'),
                  icone: Icons.directions_car_outlined,
                  titulo: 'Waze',
                  aoTocar: () {
                    Navigator.of(contextoDaFolha).pop();
                    _abraNoAplicativoDeMapaAsync(
                      context,
                      ref,
                      AplicativoDeMapa.waze,
                      latitude,
                      longitude,
                    );
                  },
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  Future<void> _abraNoAplicativoDeMapaAsync(
    BuildContext context,
    WidgetRef ref,
    AplicativoDeMapa aplicativo,
    double latitude,
    double longitude,
  ) async {
    try {
      bool abriu =
          await ref.read(provedorDoServicoDeLocalizacao).abraNoMapaAsync(
                latitude: latitude,
                longitude: longitude,
                aplicativo: aplicativo,
                descricao: encontro.local,
              );

      if (!abriu && context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Não foi possível abrir a localização no mapa.'),
          ),
        );
      }
    } on Object {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Não foi possível abrir a localização no mapa.'),
          ),
        );
      }
    }
  }
}

class _OpcaoDeAplicativoDeMapa extends StatelessWidget {
  const _OpcaoDeAplicativoDeMapa({
    required this.chave,
    required this.icone,
    required this.titulo,
    required this.aoTocar,
  });

  final Key chave;
  final IconData icone;
  final String titulo;
  final VoidCallback aoTocar;

  @override
  Widget build(BuildContext context) {
    return ListTile(
      key: chave,
      minTileHeight: 52,
      contentPadding: EdgeInsets.zero,
      leading: Icon(icone, color: CoresDoAplicativo.azulInteracao),
      title: Text(titulo),
      trailing: const Icon(Icons.open_in_new_rounded, size: 18),
      onTap: aoTocar,
    );
  }
}

class _ResumoDasPreferenciasDoAniversario extends StatelessWidget {
  const _ResumoDasPreferenciasDoAniversario({
    required this.encontro,
    required this.aoTocar,
  });

  final EncontroDetalhado encontro;
  final VoidCallback aoTocar;

  @override
  Widget build(BuildContext context) {
    List<String> medidas = <String>[
      if (_temTexto(encontro.preferenciasDoAniversario?.numeroDoCalcado))
        'Calçado ${encontro.preferenciasDoAniversario!.numeroDoCalcado}',
      if (_temTexto(encontro.preferenciasDoAniversario?.tamanhoDaCamiseta))
        'Camiseta ${encontro.preferenciasDoAniversario!.tamanhoDaCamiseta}',
      if (_temTexto(encontro.preferenciasDoAniversario?.tamanhoDaCalca))
        'Calça ${encontro.preferenciasDoAniversario!.tamanhoDaCalca}',
    ];
    int quantidadeDeInformacoesDePresente = <String?>[
      encontro.preferenciasDoAniversario?.sugestoesDePresente,
      encontro.preferenciasDoAniversario?.coisasQueGostariaDeGanhar,
    ].where(_temTexto).length;

    String resumo = medidas.isEmpty
        ? 'Informações de presente disponíveis'
        : medidas.join(' · ');
    String complemento = switch (quantidadeDeInformacoesDePresente) {
      0 => 'Consulte as preferências cadastradas',
      1 => '1 informação de presente cadastrada',
      _ =>
        '$quantidadeDeInformacoesDePresente informações de presente cadastradas',
    };

    return _SecaoAgrupada(
      titulo: 'Ideias para o aniversariante',
      corDoTitulo: CoresDoAplicativo.ambar,
      filho: _CartaoDeInformacaoAdicional(
        key: const Key('resumo-das-preferencias-do-aniversario'),
        icone: Icons.redeem_outlined,
        titulo: 'Presentes e tamanhos',
        resumo: resumo,
        complemento: complemento,
        aoTocar: aoTocar,
      ),
    );
  }

  bool _temTexto(String? texto) {
    return texto != null && texto.trim().isNotEmpty;
  }
}

class _CartaoDeInformacaoAdicional extends StatelessWidget {
  const _CartaoDeInformacaoAdicional({
    required this.icone,
    required this.titulo,
    required this.resumo,
    required this.complemento,
    required this.aoTocar,
    super.key,
  });

  final IconData icone;
  final String titulo;
  final String resumo;
  final String complemento;
  final VoidCallback aoTocar;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        borderRadius: BorderRadius.circular(RaiosDoAplicativo.grande),
        onTap: aoTocar,
        child: Padding(
          padding: const EdgeInsets.symmetric(
            horizontal: EspacamentosDoAplicativo.padrao,
            vertical: EspacamentosDoAplicativo.medio,
          ),
          child: Row(
            children: <Widget>[
              _IconeDeLinha(icone: icone, cor: CoresDoAplicativo.ambar),
              const SizedBox(width: EspacamentosDoAplicativo.medio),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: <Widget>[
                    Text(
                      titulo,
                      style: const TextStyle(fontWeight: FontWeight.w700),
                    ),
                    const SizedBox(height: EspacamentosDoAplicativo.minimo),
                    Text(
                      resumo,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        color: CoresDoAplicativo.textoSecundario,
                        fontSize: 13,
                      ),
                    ),
                    const SizedBox(height: EspacamentosDoAplicativo.minimo),
                    Text(
                      complemento,
                      style: const TextStyle(
                        color: CoresDoAplicativo.textoTerciario,
                        fontSize: 12,
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: EspacamentosDoAplicativo.pequeno),
              const Icon(
                Icons.chevron_right_rounded,
                color: CoresDoAplicativo.textoTerciario,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _DetalhesDasPreferenciasDoAniversario extends StatelessWidget {
  const _DetalhesDasPreferenciasDoAniversario({required this.encontro});

  final EncontroDetalhado encontro;

  @override
  Widget build(BuildContext context) {
    return Column(
      key: const Key('detalhes-das-preferencias-do-aniversario'),
      crossAxisAlignment: CrossAxisAlignment.start,
      children: <Widget>[
        Text(
          'Ideias para o aniversariante',
          style: Theme.of(context).textTheme.titleLarge,
        ),
        const SizedBox(height: EspacamentosDoAplicativo.minimo),
        const Text(
          'Informações para ajudar na escolha de um presente.',
          style: TextStyle(color: CoresDoAplicativo.textoSecundario),
        ),
        const SizedBox(height: EspacamentosDoAplicativo.grande),
        Wrap(
          spacing: EspacamentosDoAplicativo.pequeno,
          runSpacing: EspacamentosDoAplicativo.pequeno,
          children: <Widget>[
            if (_temTexto(
              encontro.preferenciasDoAniversario?.numeroDoCalcado,
            ))
              _MedidaDoAniversariante(
                icone: Icons.straighten,
                rotulo: 'Calçado',
                valor: encontro.preferenciasDoAniversario!.numeroDoCalcado!,
              ),
            if (_temTexto(
              encontro.preferenciasDoAniversario?.tamanhoDaCamiseta,
            ))
              _MedidaDoAniversariante(
                icone: Icons.checkroom_outlined,
                rotulo: 'Camiseta',
                valor: encontro.preferenciasDoAniversario!.tamanhoDaCamiseta!,
              ),
            if (_temTexto(
              encontro.preferenciasDoAniversario?.tamanhoDaCalca,
            ))
              _MedidaDoAniversariante(
                icone: Icons.checkroom_outlined,
                rotulo: 'Calça',
                valor: encontro.preferenciasDoAniversario!.tamanhoDaCalca!,
              ),
          ],
        ),
        if (_temTexto(
          encontro.preferenciasDoAniversario?.sugestoesDePresente,
        )) ...<Widget>[
          const SizedBox(height: EspacamentosDoAplicativo.grande),
          _TextoDasPreferencias(
            titulo: 'Sugestões de presente',
            texto: encontro.preferenciasDoAniversario!.sugestoesDePresente!,
          ),
        ],
        if (_temTexto(
          encontro.preferenciasDoAniversario?.coisasQueGostariaDeGanhar,
        )) ...<Widget>[
          const SizedBox(height: EspacamentosDoAplicativo.grande),
          _TextoDasPreferencias(
            titulo: 'Gostaria de ganhar',
            texto:
                encontro.preferenciasDoAniversario!.coisasQueGostariaDeGanhar!,
          ),
        ],
      ],
    );
  }

  bool _temTexto(String? texto) {
    return texto != null && texto.trim().isNotEmpty;
  }
}

class _MedidaDoAniversariante extends StatelessWidget {
  const _MedidaDoAniversariante({
    required this.icone,
    required this.rotulo,
    required this.valor,
  });

  final IconData icone;
  final String rotulo;
  final String valor;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: EspacamentosDoAplicativo.medio,
        vertical: EspacamentosDoAplicativo.pequeno,
      ),
      decoration: BoxDecoration(
        color: CoresDoAplicativo.fundoDoCartaoSuave,
        borderRadius: BorderRadius.circular(RaiosDoAplicativo.medio),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: <Widget>[
          Icon(icone, size: 18, color: CoresDoAplicativo.ambar),
          const SizedBox(width: EspacamentosDoAplicativo.pequeno),
          Text('$rotulo: $valor'),
        ],
      ),
    );
  }
}

class _TextoDasPreferencias extends StatelessWidget {
  const _TextoDasPreferencias({
    required this.titulo,
    required this.texto,
  });

  final String titulo;
  final String texto;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: <Widget>[
        Text(
          titulo,
          style: const TextStyle(
            color: CoresDoAplicativo.ambar,
            fontWeight: FontWeight.w700,
          ),
        ),
        const SizedBox(height: EspacamentosDoAplicativo.minimo),
        Text(
          texto,
          style: const TextStyle(
            color: CoresDoAplicativo.textoSecundario,
            height: 1.4,
          ),
        ),
      ],
    );
  }
}

class _PresencaDoUsuario extends StatelessWidget {
  const _PresencaDoUsuario({
    required this.encontro,
    required this.estaAtualizando,
    required this.aoAlterar,
  });

  final EncontroDetalhado encontro;
  final bool estaAtualizando;
  final VoidCallback aoAlterar;

  @override
  Widget build(BuildContext context) {
    ParticipanteDoEncontro? participante = encontro.participanteAtual;
    String situacao = participante?.situacao ?? 'Convidado';
    bool podeAlterar = encontro.situacao.toLowerCase() == 'planejado';

    return _SecaoAgrupada(
      titulo: 'Minha presença',
      corDoTitulo: CoresDoAplicativo.verdeDestaque,
      filho: _SuperficieAgrupada(
        filho: Material(
          color: Colors.transparent,
          child: InkWell(
            borderRadius: BorderRadius.circular(RaiosDoAplicativo.grande),
            onTap: podeAlterar && !estaAtualizando ? aoAlterar : null,
            child: Padding(
              padding: const EdgeInsets.all(EspacamentosDoAplicativo.padrao),
              child: Row(
                children: <Widget>[
                  Container(
                    width: 40,
                    height: 40,
                    decoration: BoxDecoration(
                      shape: BoxShape.circle,
                      color: _corDaSituacao(situacao).withValues(alpha: 0.12),
                    ),
                    child: Icon(
                      _iconeDaSituacao(situacao),
                      color: _corDaSituacao(situacao),
                      size: 22,
                    ),
                  ),
                  const SizedBox(width: EspacamentosDoAplicativo.medio),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: <Widget>[
                        Text(
                          _tituloDaSituacao(situacao),
                          style: const TextStyle(fontWeight: FontWeight.w700),
                        ),
                        const SizedBox(height: EspacamentosDoAplicativo.minimo),
                        Text(
                          situacao,
                          style: TextStyle(
                            color: _corDaSituacao(situacao),
                            fontSize: 12,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ],
                    ),
                  ),
                  if (estaAtualizando)
                    const SizedBox.square(
                      dimension: 22,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  else if (podeAlterar)
                    TextButton(
                      key: const Key('alterar-presenca'),
                      onPressed: aoAlterar,
                      child: const Text('Alterar'),
                    ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  String _tituloDaSituacao(String situacao) {
    return switch (situacao.toLowerCase()) {
      'confirmado' => 'Você vai participar',
      'talvez' => 'Você talvez participe',
      'naovai' => 'Você não vai participar',
      _ => 'Responda ao convite',
    };
  }

  IconData _iconeDaSituacao(String situacao) {
    return switch (situacao.toLowerCase()) {
      'confirmado' => Icons.check_circle_outline_rounded,
      'talvez' => Icons.help_outline_rounded,
      'naovai' => Icons.cancel_outlined,
      _ => Icons.mail_outline_rounded,
    };
  }

  Color _corDaSituacao(String situacao) {
    return switch (situacao.toLowerCase()) {
      'confirmado' => CoresDoAplicativo.verdeDestaque,
      'talvez' => CoresDoAplicativo.ambar,
      'naovai' => CoresDoAplicativo.coral,
      _ => CoresDoAplicativo.textoSecundario,
    };
  }
}

class _QuemVai extends StatelessWidget {
  const _QuemVai({
    required this.participantes,
    required this.aoAbrirParticipantes,
  });

  final List<ParticipanteDoEncontro> participantes;
  final VoidCallback aoAbrirParticipantes;

  @override
  Widget build(BuildContext context) {
    List<ParticipanteDoEncontro> participantesConfirmados = participantes
        .where(
          (ParticipanteDoEncontro participante) =>
              participante.situacao.toLowerCase() == 'confirmado',
        )
        .toList();

    return _SecaoAgrupada(
      titulo: 'Quem vai',
      corDoTitulo: CoresDoAplicativo.roxoSuave,
      filho: Material(
        color: Colors.transparent,
        child: InkWell(
          key: const Key('abrir-participantes'),
          borderRadius: BorderRadius.circular(RaiosDoAplicativo.grande),
          onTap: aoAbrirParticipantes,
          child: Padding(
            padding: const EdgeInsets.symmetric(
              horizontal: EspacamentosDoAplicativo.padrao,
              vertical: EspacamentosDoAplicativo.medio,
            ),
            child: participantesConfirmados.isEmpty
                ? const Row(
                    children: <Widget>[
                      _IconeDeLinha(
                        icone: Icons.people_outline_rounded,
                        cor: CoresDoAplicativo.roxoSuave,
                      ),
                      SizedBox(width: EspacamentosDoAplicativo.medio),
                      Expanded(
                        child: Text(
                          'Ninguém confirmou ainda.',
                          style: TextStyle(
                            color: CoresDoAplicativo.textoSecundario,
                          ),
                        ),
                      ),
                      Icon(
                        Icons.chevron_right_rounded,
                        color: CoresDoAplicativo.textoTerciario,
                      ),
                    ],
                  )
                : Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: <Widget>[
                      Row(
                        children: <Widget>[
                          _AvataresDosParticipantes(
                            participantes: participantesConfirmados,
                          ),
                          const Spacer(),
                          const Icon(
                            Icons.chevron_right_rounded,
                            color: CoresDoAplicativo.textoTerciario,
                          ),
                        ],
                      ),
                      const SizedBox(
                        height: EspacamentosDoAplicativo.pequeno,
                      ),
                      Text(
                        _crieResumoDosNomes(participantesConfirmados),
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                        style: const TextStyle(
                          color: CoresDoAplicativo.textoSecundario,
                          fontSize: 13,
                          height: 1.3,
                        ),
                      ),
                    ],
                  ),
          ),
        ),
      ),
    );
  }

  String _crieResumoDosNomes(
    List<ParticipanteDoEncontro> participantesConfirmados,
  ) {
    const int quantidadeDeNomesVisiveis = 3;
    List<String> nomes = participantesConfirmados
        .take(quantidadeDeNomesVisiveis)
        .map((ParticipanteDoEncontro participante) => participante.nome)
        .toList();
    int quantidadeRestante = participantesConfirmados.length - nomes.length;

    if (quantidadeRestante == 0) {
      return nomes.join(', ');
    }

    String complemento = quantidadeRestante == 1
        ? 'e mais 1 pessoa'
        : 'e mais $quantidadeRestante pessoas';
    return '${nomes.join(', ')} $complemento';
  }
}

class _AvataresDosParticipantes extends StatelessWidget {
  const _AvataresDosParticipantes({required this.participantes});

  final List<ParticipanteDoEncontro> participantes;

  @override
  Widget build(BuildContext context) {
    const int quantidadeMaximaDeAvatares = 5;
    const double dimensao = 40;
    const double deslocamento = 28;
    List<ParticipanteDoEncontro> participantesVisiveis =
        participantes.take(quantidadeMaximaDeAvatares).toList();
    int quantidadeRestante =
        participantes.length - participantesVisiveis.length;
    int quantidadeDeElementos =
        participantesVisiveis.length + (quantidadeRestante > 0 ? 1 : 0);
    double largura = dimensao + ((quantidadeDeElementos - 1) * deslocamento);

    return SizedBox(
      key: const Key('avatares-dos-participantes-confirmados'),
      width: largura,
      height: dimensao,
      child: Stack(
        children: <Widget>[
          for (int indice = 0; indice < participantesVisiveis.length; indice++)
            Positioned(
              left: indice * deslocamento,
              child: Tooltip(
                message: participantesVisiveis[indice].nome,
                child: FotoDePerfil(
                  url: participantesVisiveis[indice].urlDaFotoDePerfil,
                  iniciais: participantesVisiveis[indice].iniciais,
                  dimensao: dimensao,
                  tamanhoDasIniciais: 13,
                ),
              ),
            ),
          if (quantidadeRestante > 0)
            Positioned(
              left: participantesVisiveis.length * deslocamento,
              child: Container(
                key: const Key('quantidade-restante-de-participantes'),
                width: dimensao,
                height: dimensao,
                alignment: Alignment.center,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: CoresDoAplicativo.fundoElevado,
                  border: Border.all(
                    color: CoresDoAplicativo.roxoSuave,
                  ),
                ),
                child: Text(
                  '+$quantidadeRestante',
                  style: const TextStyle(
                    color: CoresDoAplicativo.roxoSuave,
                    fontSize: 12,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }
}

class _NavegacaoDoEncontro extends StatelessWidget {
  const _NavegacaoDoEncontro({
    required this.aoAbrirCombinados,
    required this.aoAbrirMidias,
  });

  final VoidCallback aoAbrirCombinados;
  final VoidCallback aoAbrirMidias;

  @override
  Widget build(BuildContext context) {
    return _SecaoAgrupada(
      titulo: 'Explorar encontro',
      corDoTitulo: CoresDoAplicativo.azulSuave,
      filho: _SuperficieAgrupada(
        filho: Column(
          children: <Widget>[
            _LinhaNavegavel(
              key: const Key('abrir-combinados'),
              icone: Icons.checklist_rounded,
              titulo: 'Combinados',
              subtitulo: 'Veja o que cada pessoa ficou de fazer',
              aoTocar: aoAbrirCombinados,
            ),
            const _DivisorAgrupado(),
            _LinhaNavegavel(
              key: const Key('abrir-midias'),
              icone: Icons.photo_library_outlined,
              titulo: 'Mídias compartilhadas',
              subtitulo: 'Fotos e momentos publicados no encontro',
              aoTocar: aoAbrirMidias,
            ),
          ],
        ),
      ),
    );
  }
}

class _SecaoAgrupada extends StatelessWidget {
  const _SecaoAgrupada({
    required this.titulo,
    required this.filho,
    this.corDoTitulo = CoresDoAplicativo.textoSecundario,
  });

  final String titulo;
  final Widget filho;
  final Color corDoTitulo;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: <Widget>[
        Padding(
          padding: const EdgeInsets.only(
            left: EspacamentosDoAplicativo.pequeno,
            bottom: EspacamentosDoAplicativo.pequeno,
          ),
          child: Text(
            titulo,
            style: TextStyle(
              color: corDoTitulo,
              fontSize: 13,
              fontWeight: FontWeight.w600,
            ),
          ),
        ),
        filho,
      ],
    );
  }
}

class _SuperficieAgrupada extends StatelessWidget {
  const _SuperficieAgrupada({required this.filho});

  final Widget filho;

  @override
  Widget build(BuildContext context) {
    return filho;
  }
}

class _LinhaNavegavel extends StatelessWidget {
  const _LinhaNavegavel({
    required this.icone,
    required this.titulo,
    required this.subtitulo,
    required this.aoTocar,
    super.key,
  });

  final IconData icone;
  final String titulo;
  final String subtitulo;
  final VoidCallback aoTocar;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: aoTocar,
        child: Padding(
          padding: const EdgeInsets.symmetric(
            horizontal: EspacamentosDoAplicativo.padrao,
            vertical: EspacamentosDoAplicativo.medio,
          ),
          child: Row(
            children: <Widget>[
              _IconeDeLinha(icone: icone),
              const SizedBox(width: EspacamentosDoAplicativo.medio),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: <Widget>[
                    Text(
                      titulo,
                      style: const TextStyle(fontWeight: FontWeight.w600),
                    ),
                    const SizedBox(height: EspacamentosDoAplicativo.minimo),
                    Text(
                      subtitulo,
                      style: const TextStyle(
                        color: CoresDoAplicativo.textoTerciario,
                        fontSize: 12,
                        height: 1.25,
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: EspacamentosDoAplicativo.pequeno),
              const Icon(
                Icons.chevron_right_rounded,
                color: CoresDoAplicativo.textoTerciario,
                size: 20,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _LinhaDeAcao extends StatelessWidget {
  const _LinhaDeAcao({
    required this.icone,
    required this.titulo,
    required this.subtitulo,
    required this.aoTocar,
    this.cor = CoresDoAplicativo.textoSecundario,
    this.estaExecutando = false,
    super.key,
  });

  final IconData icone;
  final String titulo;
  final String subtitulo;
  final Color cor;
  final bool estaExecutando;
  final VoidCallback? aoTocar;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: aoTocar,
        child: Padding(
          padding: const EdgeInsets.symmetric(
            horizontal: EspacamentosDoAplicativo.padrao,
            vertical: EspacamentosDoAplicativo.medio,
          ),
          child: Row(
            children: <Widget>[
              _IconeDeLinha(icone: icone, cor: cor),
              const SizedBox(width: EspacamentosDoAplicativo.medio),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: <Widget>[
                    Text(
                      titulo,
                      style: TextStyle(
                        color: cor == CoresDoAplicativo.coral
                            ? CoresDoAplicativo.coral
                            : CoresDoAplicativo.textoPrincipal,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    const SizedBox(height: EspacamentosDoAplicativo.minimo),
                    Text(
                      subtitulo,
                      style: const TextStyle(
                        color: CoresDoAplicativo.textoTerciario,
                        fontSize: 12,
                        height: 1.25,
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: EspacamentosDoAplicativo.pequeno),
              if (estaExecutando)
                SizedBox.square(
                  dimension: 20,
                  child: CircularProgressIndicator(
                    strokeWidth: 2,
                    color: cor,
                  ),
                )
              else
                const Icon(
                  Icons.chevron_right_rounded,
                  color: CoresDoAplicativo.textoTerciario,
                  size: 20,
                ),
            ],
          ),
        ),
      ),
    );
  }
}

class _IconeDeLinha extends StatelessWidget {
  const _IconeDeLinha({
    required this.icone,
    this.cor = CoresDoAplicativo.textoSecundario,
  });

  final IconData icone;
  final Color cor;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: 36,
      height: 36,
      child: Icon(icone, color: cor, size: 19),
    );
  }
}

class _DivisorAgrupado extends StatelessWidget {
  const _DivisorAgrupado();

  @override
  Widget build(BuildContext context) {
    return const Padding(
      padding: EdgeInsets.only(left: 64),
      child: Divider(height: 1, color: CoresDoAplicativo.bordaDiscreta),
    );
  }
}

class _LinhaDeInformacao extends StatelessWidget {
  const _LinhaDeInformacao({
    required this.icone,
    required this.rotulo,
    required this.valor,
    this.exibaDivisor = true,
    this.aoPressionar,
    this.textoDaAcao,
  });

  final IconData icone;
  final String rotulo;
  final String valor;
  final bool exibaDivisor;
  final VoidCallback? aoPressionar;
  final String? textoDaAcao;

  @override
  Widget build(BuildContext context) {
    return Column(
      children: <Widget>[
        Material(
          color: Colors.transparent,
          child: InkWell(
            onTap: aoPressionar,
            child: Padding(
              padding: const EdgeInsets.symmetric(
                horizontal: EspacamentosDoAplicativo.padrao,
                vertical: EspacamentosDoAplicativo.medio,
              ),
              child: Row(
                children: <Widget>[
                  _IconeDeLinha(icone: icone),
                  const SizedBox(width: EspacamentosDoAplicativo.medio),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: <Widget>[
                        Text(
                          rotulo,
                          style: const TextStyle(
                            color: CoresDoAplicativo.textoTerciario,
                            fontSize: 11,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                        const SizedBox(height: EspacamentosDoAplicativo.minimo),
                        Text(
                          valor,
                          style: const TextStyle(
                            color: CoresDoAplicativo.textoPrincipal,
                            fontSize: 15,
                            height: 1.25,
                          ),
                        ),
                        if (textoDaAcao != null) ...<Widget>[
                          const SizedBox(
                              height: EspacamentosDoAplicativo.minimo),
                          Text(
                            textoDaAcao!,
                            style: const TextStyle(
                              color: CoresDoAplicativo.verdeDestaque,
                              fontSize: 12,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                  if (aoPressionar != null)
                    const Icon(
                      Icons.open_in_new_rounded,
                      color: CoresDoAplicativo.verdeDestaque,
                      size: 18,
                    ),
                ],
              ),
            ),
          ),
        ),
        if (exibaDivisor) const _DivisorAgrupado(),
      ],
    );
  }
}

class _ErroDoDetalhe extends StatelessWidget {
  const _ErroDoDetalhe({
    required this.mensagem,
    required this.aoVoltar,
    required this.aoTentarNovamente,
  });

  final String mensagem;
  final VoidCallback aoVoltar;
  final Future<void> Function() aoTentarNovamente;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: <Widget>[
          const Icon(
            Icons.event_busy_outlined,
            size: 48,
            color: CoresDoAplicativo.coral,
          ),
          const SizedBox(height: EspacamentosDoAplicativo.medio),
          Text(mensagem, textAlign: TextAlign.center),
          const SizedBox(height: EspacamentosDoAplicativo.padrao),
          FilledButton.icon(
            onPressed: aoTentarNovamente,
            icon: const Icon(Icons.refresh_rounded),
            label: const Text('Tentar novamente'),
          ),
          TextButton(onPressed: aoVoltar, child: const Text('Voltar')),
        ],
      ),
    );
  }
}
