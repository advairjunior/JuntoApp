import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estrutura_responsiva_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/configuracao/configuracao_do_ambiente.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/imagem_privada.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/foto_de_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/raios_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/componentes/folha_de_resposta_de_presenca.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/componentes/folha_de_origem_da_imagem.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_detalhado.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/servicos/seletor_de_imagem.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/dados/repositorio_de_memorias_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/estado/controlador_da_pagina_inicial.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/publicacoes/estado/controlador_dos_momentos_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/publicacoes/estado/estado_dos_momentos_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/publicacoes/modelos/publicacao_do_encontro.dart';

class TelaDeMomentosDoEncontro extends ConsumerStatefulWidget {
  const TelaDeMomentosDoEncontro({
    required this.identificadorDoEncontro,
    this.soliciteRespostaDePresenca = false,
    super.key,
  });

  final String identificadorDoEncontro;
  final bool soliciteRespostaDePresenca;

  @override
  ConsumerState<TelaDeMomentosDoEncontro> createState() =>
      _EstadoDaTelaDeMomentosDoEncontro();
}

class _EstadoDaTelaDeMomentosDoEncontro
    extends ConsumerState<TelaDeMomentosDoEncontro> {
  static const List<String> _emojis = <String>[
    '😀',
    '😂',
    '❤️',
    '👏',
    '🎉',
    '🔥',
    '🙏',
    '👍',
  ];

  final TextEditingController _controladorDoTexto = TextEditingController();
  final ScrollController _controladorDaRolagem = ScrollController();
  bool _seletorDeEmojiEstaVisivel = false;
  ImagemSelecionada? _imagemSelecionada;
  int _quantidadeAnteriorDePublicacoes = -1;
  bool _respostaDePresencaFoiSolicitada = false;

  @override
  void dispose() {
    _controladorDoTexto.dispose();
    _controladorDaRolagem.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    EstadoDosMomentosDoEncontro estado = ref.watch(
      provedorDoControladorDosMomentosDoEncontro(
        widget.identificadorDoEncontro,
      ),
    );

    if (estado.publicacoes.length != _quantidadeAnteriorDePublicacoes) {
      _quantidadeAnteriorDePublicacoes = estado.publicacoes.length;
      WidgetsBinding.instance.addPostFrameCallback((Duration duracao) {
        _roleParaOFinal();
      });
    }

    if (widget.soliciteRespostaDePresenca &&
        !_respostaDePresencaFoiSolicitada &&
        estado.situacao == SituacaoDosMomentosDoEncontro.carregado &&
        estado.encontro!.situacao.toLowerCase() == 'planejado') {
      _respostaDePresencaFoiSolicitada = true;
      WidgetsBinding.instance.addPostFrameCallback((Duration duracao) {
        _soliciteRespostaDePresencaAsync();
      });
    }

    return Scaffold(
      resizeToAvoidBottomInset: true,
      body: EstruturaResponsivaDoAplicativo(
        filho: DecoratedBox(
          decoration: const BoxDecoration(
            gradient: LinearGradient(
              begin: Alignment.topCenter,
              end: Alignment.bottomCenter,
              colors: <Color>[
                CoresDoAplicativo.fundoSecundario,
                CoresDoAplicativo.fundoPrincipal,
              ],
            ),
          ),
          child: SafeArea(
            child: switch (estado.situacao) {
              SituacaoDosMomentosDoEncontro.carregando =>
                const Center(child: CircularProgressIndicator()),
              SituacaoDosMomentosDoEncontro.falhou => _ErroDosMomentos(
                  mensagem: estado.mensagemDeErro ??
                      'Não foi possível abrir este encontro.',
                  aoVoltar: _volte,
                  aoTentarNovamente: () => ref
                      .read(
                        provedorDoControladorDosMomentosDoEncontro(
                          widget.identificadorDoEncontro,
                        ).notifier,
                      )
                      .carregueAsync(),
                ),
              SituacaoDosMomentosDoEncontro.carregado => Column(
                  children: <Widget>[
                    _CabecalhoDosMomentos(
                      encontro: estado.encontro!,
                      aoVoltar: _volte,
                      aoAbrirInformacoes: _abraInformacoes,
                    ),
                    if (estado.encontro!.situacao.toLowerCase() == 'cancelado')
                      const _AvisoDeEncontroCancelado(),
                    if (estado.mensagemDeErro != null)
                      _MensagemDeErro(
                        mensagem: estado.mensagemDeErro!,
                        aoTentarNovamente: () => ref
                            .read(
                              provedorDoControladorDosMomentosDoEncontro(
                                widget.identificadorDoEncontro,
                              ).notifier,
                            )
                            .carregueAsync(),
                      ),
                    Expanded(
                      child: RefreshIndicator(
                        onRefresh: () => ref
                            .read(
                              provedorDoControladorDosMomentosDoEncontro(
                                widget.identificadorDoEncontro,
                              ).notifier,
                            )
                            .carregueAsync(),
                        child: _ListaDeMomentos(
                          controladorDaRolagem: _controladorDaRolagem,
                          publicacoes: estado.publicacoes,
                          aoRemover: _confirmeRemocaoDaMemoriaAsync,
                        ),
                      ),
                    ),
                    if (estado.encontro!.situacao.toLowerCase() !=
                        'cancelado') ...<Widget>[
                      if (_seletorDeEmojiEstaVisivel)
                        _SeletorDeEmoji(
                          emojis: _emojis,
                          aoSelecionar: _adicioneEmoji,
                        ),
                      _CompositorDeMomento(
                        controladorDoTexto: _controladorDoTexto,
                        estaPublicando: estado.estaPublicando,
                        imagemSelecionada: _imagemSelecionada,
                        aoSelecionarImagem: _selecioneImagemAsync,
                        aoRemoverImagem: _removaImagemSelecionada,
                        aoAlternarEmojis: () {
                          setState(() {
                            _seletorDeEmojiEstaVisivel =
                                !_seletorDeEmojiEstaVisivel;
                          });
                        },
                        aoPublicar: _publiqueAsync,
                      ),
                    ],
                  ],
                ),
            },
          ),
        ),
      ),
    );
  }

  void _volte() {
    if (context.canPop()) {
      context.pop();
      return;
    }

    context.go('/inicio');
  }

  Future<void> _abraInformacoes() async {
    await context.push<void>(
      '/encontros/${widget.identificadorDoEncontro}/informacoes',
    );

    if (mounted) {
      await ref
          .read(
            provedorDoControladorDosMomentosDoEncontro(
              widget.identificadorDoEncontro,
            ).notifier,
          )
          .carregueAsync();
    }
  }

  Future<void> _soliciteRespostaDePresencaAsync() async {
    if (!mounted) {
      return;
    }

    String? situacao = await mostreFolhaDeRespostaDePresencaAsync(context);

    if (situacao == null || !mounted) {
      return;
    }

    bool respondeu = await ref
        .read(
          provedorDoControladorDosMomentosDoEncontro(
            widget.identificadorDoEncontro,
          ).notifier,
        )
        .respondaPresencaAsync(situacao);

    if (respondeu) {
      await ref
          .read(provedorDoControladorDaPaginaInicial.notifier)
          .carregueAsync();
    }
  }

  void _adicioneEmoji(String emoji) {
    String textoAtual = _controladorDoTexto.text;
    _controladorDoTexto.text =
        textoAtual.trim().isEmpty ? emoji : '${textoAtual.trimRight()} $emoji';
    _controladorDoTexto.selection = TextSelection.collapsed(
      offset: _controladorDoTexto.text.length,
    );
  }

  Future<void> _publiqueAsync() async {
    String texto = _controladorDoTexto.text;
    ImagemSelecionada? imagem = _imagemSelecionada;

    if (texto.trim().isEmpty && imagem == null) {
      return;
    }

    int tamanhoMaximo = imagem == null ? 1000 : 280;

    if (texto.trim().length > tamanhoMaximo) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            imagem == null
                ? 'A publicação não pode ultrapassar 1000 caracteres.'
                : 'A legenda não pode ultrapassar 280 caracteres.',
          ),
        ),
      );
      return;
    }

    ControladorDosMomentosDoEncontro controlador = ref.read(
      provedorDoControladorDosMomentosDoEncontro(
        widget.identificadorDoEncontro,
      ).notifier,
    );
    bool publicou = imagem == null
        ? await controlador.publiqueAsync(texto)
        : await controlador.publiqueImagemAsync(imagem, texto);

    if (publicou && mounted) {
      _controladorDoTexto.clear();
      setState(() {
        _seletorDeEmojiEstaVisivel = false;
        _imagemSelecionada = null;
      });
      _roleParaOFinal();
    }
  }

  Future<void> _selecioneImagemAsync() async {
    EnumeradorDeOrigemDaImagem? origem = await escolhaOrigemDaImagemAsync(
      context,
      titulo: 'Foto do encontro',
    );

    if (origem == null || !mounted) {
      return;
    }

    ImagemSelecionada? imagem = await ref
        .read(provedorDoSeletorDeImagem)
        .selecionePorOrigemAsync(origem);

    if (!mounted || imagem == null) {
      return;
    }

    if (imagem.conteudo.lengthInBytes > 10 * 1024 * 1024) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('A foto não pode ultrapassar 10 MB.')),
      );
      return;
    }

    setState(() {
      _imagemSelecionada = imagem;
      _seletorDeEmojiEstaVisivel = false;
    });
  }

  void _removaImagemSelecionada() {
    setState(() {
      _imagemSelecionada = null;
    });
  }

  Future<bool> _confirmeRemocaoDaMemoriaAsync(
    PublicacaoDoEncontro publicacao,
  ) async {
    bool confirmou = await showDialog<bool>(
          context: context,
          builder: (BuildContext contextoDoDialogo) {
            return AlertDialog(
              title: const Text('Remover foto?'),
              content: const Text(
                'A foto será removida do mural e da galeria deste encontro.',
              ),
              actions: <Widget>[
                TextButton(
                  onPressed: () => Navigator.of(contextoDoDialogo).pop(false),
                  child: const Text('Cancelar'),
                ),
                FilledButton(
                  key: const Key('confirmar-remocao-da-memoria'),
                  onPressed: () => Navigator.of(contextoDoDialogo).pop(true),
                  child: const Text('Remover'),
                ),
              ],
            );
          },
        ) ??
        false;

    if (!confirmou || !mounted) {
      return false;
    }

    bool removeu = await ref
        .read(
          provedorDoControladorDosMomentosDoEncontro(
            widget.identificadorDoEncontro,
          ).notifier,
        )
        .removaMemoriaAsync(publicacao.identificador);

    if (removeu) {
      ref.invalidate(
        provedorDasMemoriasDoEncontro(widget.identificadorDoEncontro),
      );
    }

    return removeu;
  }

  void _roleParaOFinal() {
    if (!_controladorDaRolagem.hasClients) {
      return;
    }

    _controladorDaRolagem.animateTo(
      _controladorDaRolagem.position.maxScrollExtent,
      duration: const Duration(milliseconds: 250),
      curve: Curves.easeOut,
    );
  }
}

class _CabecalhoDosMomentos extends StatelessWidget {
  const _CabecalhoDosMomentos({
    required this.encontro,
    required this.aoVoltar,
    required this.aoAbrirInformacoes,
  });

  final EncontroDetalhado encontro;
  final VoidCallback aoVoltar;
  final VoidCallback aoAbrirInformacoes;

  @override
  Widget build(BuildContext context) {
    String complemento = <String>[
      DateFormat('dd/MM/yyyy • HH:mm').format(encontro.inicioEm),
      if (encontro.local != null && encontro.local!.trim().isNotEmpty)
        encontro.local!,
    ].join(' • ');

    return Material(
      color: CoresDoAplicativo.fundoPrincipal.withValues(alpha: 0.94),
      child: Padding(
        padding: const EdgeInsets.symmetric(
          horizontal: EspacamentosDoAplicativo.pequeno,
          vertical: EspacamentosDoAplicativo.pequeno,
        ),
        child: Row(
          children: <Widget>[
            IconButton(
              tooltip: 'Voltar',
              onPressed: aoVoltar,
              icon: const Icon(Icons.arrow_back_ios_new_rounded),
            ),
            Expanded(
              child: InkWell(
                key: const Key('abrir-informacoes-do-encontro'),
                borderRadius: BorderRadius.circular(
                  RaiosDoAplicativo.pequeno,
                ),
                onTap: aoAbrirInformacoes,
                child: Padding(
                  padding: const EdgeInsets.symmetric(vertical: 4),
                  child: Row(
                    children: <Widget>[
                      _MiniaturaDoEncontro(encontro: encontro),
                      const SizedBox(width: EspacamentosDoAplicativo.medio),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: <Widget>[
                            Text(
                              encontro.titulo,
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: Theme.of(context)
                                  .textTheme
                                  .titleMedium
                                  ?.copyWith(fontWeight: FontWeight.w700),
                            ),
                            const SizedBox(height: 2),
                            Text(
                              complemento,
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: const TextStyle(
                                color: CoresDoAplicativo.textoSecundario,
                                fontSize: 12,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),
            IconButton(
              tooltip: 'Informações do encontro',
              onPressed: aoAbrirInformacoes,
              icon: const Icon(Icons.info_outline_rounded),
            ),
          ],
        ),
      ),
    );
  }
}

class _MiniaturaDoEncontro extends StatelessWidget {
  const _MiniaturaDoEncontro({required this.encontro});

  final EncontroDetalhado encontro;

  @override
  Widget build(BuildContext context) {
    String url = ConfiguracaoDoAmbiente.crieUrlAbsoluta(
      encontro.urlDaImagemDeCapa,
    );

    return ClipRRect(
      borderRadius: BorderRadius.circular(RaiosDoAplicativo.medio),
      child: SizedBox.square(
        dimension: 48,
        child: url.isEmpty
            ? const ColoredBox(
                color: CoresDoAplicativo.fundoDoCartaoSuave,
                child: Icon(Icons.people_alt_outlined),
              )
            : ImagemPrivada(
                recurso: url,
                construaSubstituta: (_) => const ColoredBox(
                  color: CoresDoAplicativo.fundoDoCartaoSuave,
                  child: Icon(Icons.people_alt_outlined),
                ),
              ),
      ),
    );
  }
}

class _ListaDeMomentos extends StatelessWidget {
  const _ListaDeMomentos({
    required this.controladorDaRolagem,
    required this.publicacoes,
    required this.aoRemover,
  });

  final ScrollController controladorDaRolagem;
  final List<PublicacaoDoEncontro> publicacoes;
  final Future<bool> Function(PublicacaoDoEncontro publicacao) aoRemover;

  @override
  Widget build(BuildContext context) {
    if (publicacoes.isEmpty) {
      return ListView(
        controller: controladorDaRolagem,
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(EspacamentosDoAplicativo.grande),
        children: const <Widget>[
          SizedBox(height: 96),
          EstadoVazio(
            icone: Icons.forum_outlined,
            titulo: 'Ainda não há momentos por aqui.',
            descricao:
                'Compartilhe uma mensagem ou foto para começar a história deste encontro.',
          ),
        ],
      );
    }

    return ListView.separated(
      controller: controladorDaRolagem,
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.symmetric(
        horizontal: EspacamentosDoAplicativo.padrao,
        vertical: EspacamentosDoAplicativo.grande,
      ),
      itemCount: publicacoes.length,
      separatorBuilder: (BuildContext context, int indice) =>
          const SizedBox(height: EspacamentosDoAplicativo.medio),
      itemBuilder: (BuildContext context, int indice) {
        PublicacaoDoEncontro publicacao = publicacoes[indice];

        if (publicacao.ehAtualizacaoDoSistema) {
          return _AtualizacaoDoSistema(publicacao: publicacao);
        }

        return _MomentoDeParticipante(
          publicacao: publicacao,
          aoRemover: aoRemover,
        );
      },
    );
  }
}

class _AtualizacaoDoSistema extends StatelessWidget {
  const _AtualizacaoDoSistema({required this.publicacao});

  final PublicacaoDoEncontro publicacao;

  @override
  Widget build(BuildContext context) {
    return Center(
      key: Key('publicacao-${publicacao.identificador}'),
      child: Container(
        constraints: const BoxConstraints(maxWidth: 420),
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 7),
        decoration: BoxDecoration(
          color: CoresDoAplicativo.fundoSecundario,
          borderRadius: BorderRadius.circular(16),
        ),
        child: Text(
          '${publicacao.texto ?? ''} • ${DateFormat('dd/MM HH:mm').format(publicacao.publicadoEm)}',
          textAlign: TextAlign.center,
          style: const TextStyle(
            color: CoresDoAplicativo.textoTerciario,
            fontSize: 12,
          ),
        ),
      ),
    );
  }
}

class _MomentoDeParticipante extends StatelessWidget {
  const _MomentoDeParticipante({
    required this.publicacao,
    required this.aoRemover,
  });

  final PublicacaoDoEncontro publicacao;
  final Future<bool> Function(PublicacaoDoEncontro publicacao) aoRemover;

  @override
  Widget build(BuildContext context) {
    Widget balao = Container(
      key: Key('publicacao-${publicacao.identificador}'),
      constraints: const BoxConstraints(maxWidth: 430),
      padding: const EdgeInsets.all(EspacamentosDoAplicativo.medio),
      decoration: BoxDecoration(
        color: publicacao.usuarioAtual
            ? CoresDoAplicativo.fundoDaMensagemAtual
            : CoresDoAplicativo.fundoDoCartao,
        borderRadius: BorderRadius.only(
          topLeft: const Radius.circular(RaiosDoAplicativo.medio),
          topRight: const Radius.circular(RaiosDoAplicativo.medio),
          bottomLeft: Radius.circular(
            publicacao.usuarioAtual ? RaiosDoAplicativo.medio : 4,
          ),
          bottomRight: Radius.circular(
            publicacao.usuarioAtual ? 4 : RaiosDoAplicativo.medio,
          ),
        ),
        border: Border.all(color: CoresDoAplicativo.bordaSuave),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: <Widget>[
          Row(
            mainAxisSize: MainAxisSize.min,
            children: <Widget>[
              Flexible(
                child: Text(
                  publicacao.usuarioAtual ? 'Você' : publicacao.nomeDoAutor,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    color: CoresDoAplicativo.verdeDestaque,
                    fontSize: 14,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ),
              const SizedBox(width: EspacamentosDoAplicativo.medio),
              Text(
                DateFormat('dd/MM HH:mm').format(publicacao.publicadoEm),
                style: const TextStyle(
                  color: CoresDoAplicativo.textoTerciario,
                  fontSize: 12,
                ),
              ),
            ],
          ),
          if (publicacao.texto != null &&
              publicacao.texto!.trim().isNotEmpty) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.minimo),
            Text(publicacao.texto!),
          ],
          if (publicacao.ehImagem) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.pequeno),
            _ImagemDoMomento(
              publicacao: publicacao,
              aoRemover: () => aoRemover(publicacao),
            ),
          ],
          if (publicacao.ehImagem) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.minimo),
            const Text(
              'Toque para ampliar',
              style: TextStyle(
                color: CoresDoAplicativo.textoTerciario,
                fontSize: 12,
              ),
            ),
          ],
        ],
      ),
    );

    return Row(
      mainAxisAlignment: publicacao.usuarioAtual
          ? MainAxisAlignment.end
          : MainAxisAlignment.start,
      crossAxisAlignment: CrossAxisAlignment.start,
      children: <Widget>[
        if (!publicacao.usuarioAtual) ...<Widget>[
          _AvatarDoAutor(publicacao: publicacao),
          const SizedBox(width: EspacamentosDoAplicativo.pequeno),
        ],
        Flexible(child: balao),
      ],
    );
  }
}

class _ImagemDoMomento extends StatelessWidget {
  const _ImagemDoMomento({
    required this.publicacao,
    required this.aoRemover,
  });

  final PublicacaoDoEncontro publicacao;
  final Future<bool> Function() aoRemover;

  @override
  Widget build(BuildContext context) {
    String recurso = ConfiguracaoDoAmbiente.crieUrlAbsoluta(
      publicacao.urlDaMidia,
    );

    return Semantics(
      button: true,
      label: 'Ampliar foto compartilhada por ${publicacao.nomeDoAutor}',
      child: InkWell(
        key: Key('abrir-midia-${publicacao.identificador}'),
        onTap: () => _abraImagemAmpliada(context),
        borderRadius: BorderRadius.circular(12),
        child: ClipRRect(
          borderRadius: BorderRadius.circular(12),
          child: AspectRatio(
            aspectRatio: 4 / 3,
            child: ImagemPrivada(
              recurso: recurso,
              construaSubstituta: (_) => const ColoredBox(
                color: CoresDoAplicativo.fundoSecundario,
                child: Center(
                  child: Icon(
                    Icons.broken_image_outlined,
                    color: CoresDoAplicativo.textoTerciario,
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  Future<void> _abraImagemAmpliada(BuildContext context) {
    String recurso = ConfiguracaoDoAmbiente.crieUrlAbsoluta(
      publicacao.urlDaMidia,
    );

    return showDialog<void>(
      context: context,
      useSafeArea: false,
      builder: (BuildContext context) {
        return Dialog.fullscreen(
          backgroundColor: Colors.black,
          child: Scaffold(
            backgroundColor: Colors.black,
            appBar: AppBar(
              backgroundColor: Colors.black,
              foregroundColor: Colors.white,
              title: Text(
                publicacao.usuarioAtual ? 'Sua foto' : publicacao.nomeDoAutor,
              ),
              leading: IconButton(
                tooltip: 'Fechar',
                onPressed: () => Navigator.of(context).pop(),
                icon: const Icon(Icons.close_rounded),
              ),
              actions: <Widget>[
                if (publicacao.usuarioAtual)
                  IconButton(
                    key: const Key('remover-memoria'),
                    tooltip: 'Remover foto',
                    onPressed: () async {
                      bool removeu = await aoRemover();

                      if (removeu && context.mounted) {
                        Navigator.of(context).pop();
                      }
                    },
                    icon: const Icon(Icons.delete_outline_rounded),
                  ),
              ],
            ),
            body: Column(
              children: <Widget>[
                Expanded(
                  child: InteractiveViewer(
                    minScale: 0.8,
                    maxScale: 4,
                    child: Center(
                      child: ImagemPrivada(
                        recurso: recurso,
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
                if (publicacao.texto != null &&
                    publicacao.texto!.trim().isNotEmpty)
                  SafeArea(
                    top: false,
                    child: Padding(
                      padding: const EdgeInsets.all(
                        EspacamentosDoAplicativo.padrao,
                      ),
                      child: Text(
                        publicacao.texto!,
                        style: const TextStyle(color: Colors.white),
                      ),
                    ),
                  ),
              ],
            ),
          ),
        );
      },
    );
  }
}

class _AvatarDoAutor extends StatelessWidget {
  const _AvatarDoAutor({required this.publicacao});

  final PublicacaoDoEncontro publicacao;

  @override
  Widget build(BuildContext context) {
    String inicial = publicacao.nomeDoAutor.trim().isEmpty
        ? '?'
        : publicacao.nomeDoAutor.trim()[0].toUpperCase();

    return FotoDePerfil(
      url: publicacao.urlDaFotoDePerfilDoAutor,
      iniciais: inicial,
      dimensao: 36,
    );
  }
}

class _CompositorDeMomento extends StatelessWidget {
  const _CompositorDeMomento({
    required this.controladorDoTexto,
    required this.estaPublicando,
    required this.imagemSelecionada,
    required this.aoSelecionarImagem,
    required this.aoRemoverImagem,
    required this.aoAlternarEmojis,
    required this.aoPublicar,
  });

  final TextEditingController controladorDoTexto;
  final bool estaPublicando;
  final ImagemSelecionada? imagemSelecionada;
  final VoidCallback aoSelecionarImagem;
  final VoidCallback aoRemoverImagem;
  final VoidCallback aoAlternarEmojis;
  final VoidCallback aoPublicar;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        color: CoresDoAplicativo.fundoSecundario,
        border: const Border(
          top: BorderSide(color: CoresDoAplicativo.bordaDiscreta),
        ),
        boxShadow: <BoxShadow>[
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.22),
            blurRadius: 18,
            offset: const Offset(0, -5),
          ),
        ],
      ),
      child: SafeArea(
        top: false,
        child: Padding(
          padding: const EdgeInsets.all(EspacamentosDoAplicativo.pequeno),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: <Widget>[
              if (imagemSelecionada != null) ...<Widget>[
                _PreviaDaImagemSelecionada(
                  imagem: imagemSelecionada!,
                  aoRemover: aoRemoverImagem,
                ),
                const SizedBox(height: EspacamentosDoAplicativo.pequeno),
              ],
              Row(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: <Widget>[
                  IconButton(
                    key: const Key('selecionar-foto'),
                    tooltip: 'Adicionar foto',
                    onPressed: estaPublicando ? null : aoSelecionarImagem,
                    icon: const Icon(Icons.add_photo_alternate_outlined),
                  ),
                  IconButton(
                    key: const Key('alternar-emojis'),
                    tooltip: 'Emojis',
                    onPressed: estaPublicando ? null : aoAlternarEmojis,
                    icon: const Icon(Icons.sentiment_satisfied_alt_rounded),
                  ),
                  Expanded(
                    child: TextField(
                      key: const Key('texto-da-nova-publicacao'),
                      controller: controladorDoTexto,
                      enabled: !estaPublicando,
                      minLines: 1,
                      maxLines: 4,
                      maxLength: imagemSelecionada == null ? 1000 : 280,
                      buildCounter: (
                        BuildContext context, {
                        required int currentLength,
                        required bool isFocused,
                        required int? maxLength,
                      }) {
                        return null;
                      },
                      textInputAction: TextInputAction.newline,
                      decoration: InputDecoration(
                        hintText: imagemSelecionada == null
                            ? 'Compartilhe algo com o encontro'
                            : 'Adicione uma legenda',
                        filled: true,
                        fillColor: CoresDoAplicativo.fundoDoCartao,
                        contentPadding: const EdgeInsets.symmetric(
                          horizontal: 14,
                          vertical: 11,
                        ),
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(
                            RaiosDoAplicativo.pilula,
                          ),
                          borderSide: BorderSide.none,
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(width: EspacamentosDoAplicativo.pequeno),
                  IconButton.filled(
                    key: const Key('publicar-momento'),
                    tooltip: 'Publicar',
                    onPressed: estaPublicando ? null : aoPublicar,
                    icon: estaPublicando
                        ? const SizedBox.square(
                            dimension: 20,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : const Icon(Icons.send_rounded),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _PreviaDaImagemSelecionada extends StatelessWidget {
  const _PreviaDaImagemSelecionada({
    required this.imagem,
    required this.aoRemover,
  });

  final ImagemSelecionada imagem;
  final VoidCallback aoRemover;

  @override
  Widget build(BuildContext context) {
    return Align(
      alignment: Alignment.centerLeft,
      child: Stack(
        clipBehavior: Clip.none,
        children: <Widget>[
          ClipRRect(
            borderRadius: BorderRadius.circular(12),
            child: Image.memory(
              imagem.conteudo,
              key: const Key('previa-da-foto'),
              width: 96,
              height: 96,
              fit: BoxFit.cover,
            ),
          ),
          Positioned(
            top: -8,
            right: -8,
            child: IconButton.filledTonal(
              key: const Key('remover-foto-selecionada'),
              tooltip: 'Remover foto',
              onPressed: aoRemover,
              icon: const Icon(Icons.close_rounded, size: 18),
            ),
          ),
        ],
      ),
    );
  }
}

class _SeletorDeEmoji extends StatelessWidget {
  const _SeletorDeEmoji({
    required this.emojis,
    required this.aoSelecionar,
  });

  final List<String> emojis;
  final ValueChanged<String> aoSelecionar;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 54,
      child: ListView.separated(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(
          horizontal: EspacamentosDoAplicativo.padrao,
          vertical: EspacamentosDoAplicativo.minimo,
        ),
        itemCount: emojis.length,
        separatorBuilder: (BuildContext context, int indice) =>
            const SizedBox(width: EspacamentosDoAplicativo.pequeno),
        itemBuilder: (BuildContext context, int indice) {
          String emoji = emojis[indice];

          return IconButton(
            key: Key('emoji-$indice'),
            tooltip: 'Adicionar $emoji',
            onPressed: () => aoSelecionar(emoji),
            icon: Text(emoji, style: const TextStyle(fontSize: 22)),
          );
        },
      ),
    );
  }
}

class _AvisoDeEncontroCancelado extends StatelessWidget {
  const _AvisoDeEncontroCancelado();

  @override
  Widget build(BuildContext context) {
    return const ColoredBox(
      color: Color(0xFF321713),
      child: Padding(
        padding: EdgeInsets.all(EspacamentosDoAplicativo.pequeno),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: <Widget>[
            Icon(Icons.event_busy_outlined, color: CoresDoAplicativo.coral),
            SizedBox(width: EspacamentosDoAplicativo.pequeno),
            Text('Este encontro foi cancelado e está somente para leitura.'),
          ],
        ),
      ),
    );
  }
}

class _MensagemDeErro extends StatelessWidget {
  const _MensagemDeErro({
    required this.mensagem,
    required this.aoTentarNovamente,
  });

  final String mensagem;
  final VoidCallback aoTentarNovamente;

  @override
  Widget build(BuildContext context) {
    return MaterialBanner(
      content: Text(mensagem),
      backgroundColor: const Color(0xFF321713),
      actions: <Widget>[
        TextButton(
          onPressed: aoTentarNovamente,
          child: const Text('Tentar novamente'),
        ),
      ],
    );
  }
}

class _ErroDosMomentos extends StatelessWidget {
  const _ErroDosMomentos({
    required this.mensagem,
    required this.aoVoltar,
    required this.aoTentarNovamente,
  });

  final String mensagem;
  final VoidCallback aoVoltar;
  final VoidCallback aoTentarNovamente;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(EspacamentosDoAplicativo.grande),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: <Widget>[
            const Icon(
              Icons.cloud_off_outlined,
              size: 42,
              color: CoresDoAplicativo.coral,
            ),
            const SizedBox(height: EspacamentosDoAplicativo.medio),
            Text(mensagem, textAlign: TextAlign.center),
            const SizedBox(height: EspacamentosDoAplicativo.padrao),
            FilledButton(
              onPressed: aoTentarNovamente,
              child: const Text('Tentar novamente'),
            ),
            TextButton(onPressed: aoVoltar, child: const Text('Voltar')),
          ],
        ),
      ),
    );
  }
}
