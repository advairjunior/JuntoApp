import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cartao_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/configuracao/configuracao_do_ambiente.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/foto_de_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/imagem_privada.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/midias/video_privado.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/componentes/folha_de_origem_da_imagem.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/estado/controlador_do_detalhe_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/estado/estado_do_detalhe_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/participante_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/servicos/seletor_de_imagem.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/componentes/seletor_de_pessoas_na_midia.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/dados/repositorio_de_memorias_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/memoria_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/midia_da_memoria.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/pessoa_marcada_na_midia.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/publicacoes/telas/tela_de_momentos_do_encontro.dart';

class TelaDeMidiasDoEncontro extends ConsumerStatefulWidget {
  const TelaDeMidiasDoEncontro({
    required this.identificadorDoEncontro,
    super.key,
  });

  final String identificadorDoEncontro;

  @override
  ConsumerState<TelaDeMidiasDoEncontro> createState() =>
      _EstadoDaTelaDeMidiasDoEncontro();
}

class _EstadoDaTelaDeMidiasDoEncontro
    extends ConsumerState<TelaDeMidiasDoEncontro> {
  bool _estaSelecionandoMidias = false;

  @override
  Widget build(BuildContext context) {
    AsyncValue<List<MemoriaDoEncontro>> memorias = ref.watch(
      provedorDasMemoriasDoEncontro(widget.identificadorDoEncontro),
    );
    EstadoDoDetalheDoEncontro detalhe = ref.watch(
      provedorDoControladorDoDetalheDoEncontro(
        widget.identificadorDoEncontro,
      ),
    );
    bool podeAdicionar =
        detalhe.situacao == SituacaoDoDetalheDoEncontro.carregado &&
            detalhe.encontro!.situacao.toLowerCase() != 'cancelado';

    return Scaffold(
      backgroundColor: CoresDoAplicativo.fundoPrincipal,
      appBar: AppBar(
        title: const Text('Mídias do encontro'),
        leading: IconButton(
          tooltip: 'Voltar',
          onPressed: () => context.pop(),
          icon: const Icon(Icons.arrow_back_ios_new_rounded),
        ),
        actions: <Widget>[
          if (podeAdicionar)
            IconButton(
              key: const Key('adicionar-publicacao-nas-memorias'),
              tooltip: 'Adicionar publicação',
              onPressed:
                  _estaSelecionandoMidias ? null : _adicionePublicacaoAsync,
              icon: _estaSelecionandoMidias
                  ? const SizedBox.square(
                      dimension: 20,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Icon(Icons.add_photo_alternate_outlined),
            ),
        ],
      ),
      body: _EstruturaDaGaleria(
        filho: SafeArea(
          child: memorias.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (_, __) => _ErroDaGaleria(
              aoTentarNovamente: () => ref.invalidate(
                provedorDasMemoriasDoEncontro(widget.identificadorDoEncontro),
              ),
            ),
            data: (List<MemoriaDoEncontro> itens) {
              List<MemoriaDoEncontro> publicacoes = itens
                  .where(
                    (MemoriaDoEncontro memoria) =>
                        memoria.midias.isNotEmpty &&
                        _ehMidiaVisual(memoria.midias.first),
                  )
                  .toList();

              if (publicacoes.isEmpty) {
                return const _GaleriaVazia();
              }

              return LayoutBuilder(
                builder: (BuildContext context, BoxConstraints limites) {
                  return RefreshIndicator(
                    onRefresh: () async {
                      ref.invalidate(
                        provedorDasMemoriasDoEncontro(
                          widget.identificadorDoEncontro,
                        ),
                      );
                      await ref.read(
                        provedorDasMemoriasDoEncontro(
                          widget.identificadorDoEncontro,
                        ).future,
                      );
                    },
                    child: GridView.builder(
                      key: const Key('grade-de-midias'),
                      cacheExtent: 800,
                      padding: const EdgeInsets.all(
                        EspacamentosDoAplicativo.minimo,
                      ),
                      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                        crossAxisCount: _calculeQuantidadeDeColunas(
                          limites.maxWidth,
                        ),
                        mainAxisSpacing: EspacamentosDoAplicativo.minimo,
                        crossAxisSpacing: EspacamentosDoAplicativo.minimo,
                      ),
                      itemCount: publicacoes.length,
                      itemBuilder: (BuildContext context, int indice) {
                        return _MiniaturaDaGaleria(
                          publicacao: publicacoes[indice],
                        );
                      },
                    ),
                  );
                },
              );
            },
          ),
        ),
      ),
    );
  }

  int _calculeQuantidadeDeColunas(double largura) {
    if (largura < 600) {
      return 3;
    }

    if (largura < 900) {
      return 5;
    }

    return 6;
  }

  bool _ehMidiaVisual(MidiaDaMemoria midia) {
    String tipoDeConteudo = midia.tipoDeConteudo.toLowerCase();
    return tipoDeConteudo.startsWith('image/') ||
        tipoDeConteudo.startsWith('video/');
  }

  Future<void> _adicionePublicacaoAsync() async {
    EnumeradorDeOrigemDaImagem? origem = await escolhaOrigemDaImagemAsync(
      context,
      titulo: 'Adicionar às memórias',
    );

    if (origem == null || !mounted) {
      return;
    }

    setState(() {
      _estaSelecionandoMidias = true;
    });

    try {
      List<MidiaSelecionada> midias = await ref
          .read(provedorDoSeletorDeImagem)
          .selecioneMidiasPorOrigemAsync(origem);

      if (!mounted) {
        return;
      }

      setState(() {
        _estaSelecionandoMidias = false;
      });

      if (midias.isEmpty) {
        return;
      }

      if (midias.length > 10) {
        _mostreAviso('Selecione no máximo 10 mídias por publicação.');
        return;
      }

      if (midias.any(
        (MidiaSelecionada midia) =>
            midia.conteudo.lengthInBytes > 10 * 1024 * 1024,
      )) {
        _mostreAviso('Cada foto ou vídeo pode ter no máximo 10 MB.');
        return;
      }

      bool? publicou = await showDialog<bool>(
        context: context,
        barrierDismissible: false,
        builder: (BuildContext context) {
          return DialogoDeNovaPublicacao(
            midias: midias,
            participantes: ref
                    .read(
                      provedorDoControladorDoDetalheDoEncontro(
                        widget.identificadorDoEncontro,
                      ),
                    )
                    .encontro
                    ?.participantes ??
                const <ParticipanteDoEncontro>[],
            legendaInicial: '',
            aoPublicar: (
              String legenda,
              Map<int, List<String>> marcacoesPorIndiceDaMidia,
            ) =>
                _publiqueMidiasAsync(
              midias,
              legenda,
              marcacoesPorIndiceDaMidia,
            ),
          );
        },
      );

      if (publicou == true && mounted) {
        ref.invalidate(
          provedorDasMemoriasDoEncontro(widget.identificadorDoEncontro),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _estaSelecionandoMidias = false;
        });
      }
    }
  }

  Future<bool> _publiqueMidiasAsync(
    List<MidiaSelecionada> midias,
    String legenda,
    Map<int, List<String>> marcacoesPorIndiceDaMidia,
  ) async {
    try {
      await ref
          .read(provedorDoRepositorioDeMemoriasDoEncontro)
          .publiqueMidiasAsync(
            identificadorDoEncontro: widget.identificadorDoEncontro,
            midias: midias,
            legenda: legenda,
            marcacoesPorIndiceDaMidia: marcacoesPorIndiceDaMidia,
          );
      return true;
    } on ExcecaoDaApi catch (excecao) {
      if (mounted) {
        _mostreAviso(excecao.mensagem);
      }
      return false;
    }
  }

  void _mostreAviso(String mensagem) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(mensagem)),
    );
  }
}

class _EstruturaDaGaleria extends StatelessWidget {
  const _EstruturaDaGaleria({required this.filho});

  final Widget filho;

  @override
  Widget build(BuildContext context) {
    return ColoredBox(
      color: CoresDoAplicativo.fundoExterno,
      child: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 1100),
          child: ColoredBox(
            color: CoresDoAplicativo.fundoPrincipal,
            child: filho,
          ),
        ),
      ),
    );
  }
}

class _MiniaturaDaGaleria extends StatelessWidget {
  const _MiniaturaDaGaleria({required this.publicacao});

  final MemoriaDoEncontro publicacao;

  @override
  Widget build(BuildContext context) {
    MidiaDaMemoria capa = publicacao.midias.first;
    String recurso = ConfiguracaoDoAmbiente.crieUrlAbsoluta(capa.url);
    bool ehVideo = _ehVideo(capa);

    return Semantics(
      button: true,
      label: 'Abrir publicação de ${publicacao.nomeDoAutor}',
      child: Material(
        color: CoresDoAplicativo.transparente,
        child: InkWell(
          key: Key('midia-${capa.identificador}'),
          onTap: () => _abraVisualizador(context),
          child: Stack(
            fit: StackFit.expand,
            children: <Widget>[
              ClipRRect(
                borderRadius: BorderRadius.circular(
                  EspacamentosDoAplicativo.minimo,
                ),
                child: ehVideo
                    ? VideoPrivado(
                        recurso: recurso,
                        tipoDeConteudo: capa.tipoDeConteudo,
                        exibaControles: false,
                      )
                    : ImagemPrivada(
                        recurso: recurso,
                        construaSubstituta: (_) => const _MidiaIndisponivel(),
                      ),
              ),
              if (ehVideo)
                const Positioned(
                  top: EspacamentosDoAplicativo.pequeno,
                  right: EspacamentosDoAplicativo.pequeno,
                  child: Icon(
                    Icons.play_circle_fill_rounded,
                    color: Colors.white,
                    size: 22,
                    shadows: <Shadow>[
                      Shadow(color: Colors.black87, blurRadius: 8),
                    ],
                  ),
                ),
              if (publicacao.midias.length > 1)
                const Positioned(
                  top: EspacamentosDoAplicativo.pequeno,
                  left: EspacamentosDoAplicativo.pequeno,
                  child: Icon(
                    Icons.collections_rounded,
                    color: Colors.white,
                    size: 21,
                    shadows: <Shadow>[
                      Shadow(color: Colors.black87, blurRadius: 8),
                    ],
                  ),
                ),
              if (publicacao.midias.any(
                (MidiaDaMemoria midia) => midia.pessoasMarcadas.isNotEmpty,
              ))
                const Positioned(
                  right: EspacamentosDoAplicativo.pequeno,
                  bottom: EspacamentosDoAplicativo.pequeno,
                  child: Icon(
                    Icons.person_pin_circle_outlined,
                    color: Colors.white,
                    size: 20,
                    shadows: <Shadow>[
                      Shadow(color: Colors.black87, blurRadius: 8),
                    ],
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _abraVisualizador(BuildContext context) {
    return mostrePublicacaoDaMemoriaAsync(context, publicacao);
  }
}

Future<void> mostrePublicacaoDaMemoriaAsync(
  BuildContext context,
  MemoriaDoEncontro publicacao, {
  String? identificadorDaPessoaMarcada,
  String? identificadorDaMidiaInicial,
}) {
  int indiceInicial = publicacao.midias.indexWhere(
    (MidiaDaMemoria midia) =>
        midia.identificador == identificadorDaMidiaInicial ||
        midia.pessoasMarcadas.any(
          (PessoaMarcadaNaMidia pessoa) =>
              pessoa.identificadorDoUsuario == identificadorDaPessoaMarcada,
        ),
  );

  return showDialog<void>(
    context: context,
    useSafeArea: false,
    builder: (BuildContext context) {
      return Dialog.fullscreen(
        backgroundColor: Colors.black,
        child: _VisualizadorDaPublicacao(
          publicacao: publicacao,
          indiceInicial: indiceInicial < 0 ? 0 : indiceInicial,
        ),
      );
    },
  );
}

class _VisualizadorDaPublicacao extends ConsumerStatefulWidget {
  const _VisualizadorDaPublicacao({
    required this.publicacao,
    required this.indiceInicial,
  });

  final MemoriaDoEncontro publicacao;
  final int indiceInicial;

  @override
  ConsumerState<_VisualizadorDaPublicacao> createState() =>
      _EstadoDoVisualizadorDaPublicacao();
}

class _EstadoDoVisualizadorDaPublicacao
    extends ConsumerState<_VisualizadorDaPublicacao> {
  late final PageController _controlador;
  late MemoriaDoEncontro _publicacao;
  late int _indiceAtual;
  bool _estaAtualizandoMarcacoes = false;

  @override
  void initState() {
    super.initState();
    _publicacao = widget.publicacao;
    _indiceAtual = widget.indiceInicial;
    _controlador = PageController(initialPage: widget.indiceInicial);
  }

  @override
  void dispose() {
    _controlador.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    EstadoDoDetalheDoEncontro detalhe = ref.watch(
      provedorDoControladorDoDetalheDoEncontro(
        _publicacao.identificadorDoEncontro,
      ),
    );
    List<ParticipanteDoEncontro> participantes =
        detalhe.encontro?.participantes ?? const <ParticipanteDoEncontro>[];
    MidiaDaMemoria midiaAtual = _publicacao.midias[_indiceAtual];

    return Scaffold(
      backgroundColor: Colors.black,
      appBar: AppBar(
        backgroundColor: Colors.black,
        foregroundColor: Colors.white,
        title: const Text('Publicação'),
        leading: IconButton(
          tooltip: 'Fechar',
          onPressed: () => Navigator.of(context).pop(),
          icon: const Icon(Icons.close_rounded),
        ),
        actions: <Widget>[
          if (_publicacao.usuarioAtual)
            IconButton(
              key: const Key('remover-memoria-da-galeria'),
              tooltip: 'Remover mídia',
              onPressed: _removaAsync,
              icon: const Icon(Icons.delete_outline_rounded),
            ),
        ],
      ),
      body: Column(
        children: <Widget>[
          _CabecalhoDaPublicacao(publicacao: _publicacao),
          Expanded(
            child: Stack(
              children: <Widget>[
                PageView.builder(
                  key: const Key('visualizador-de-midias'),
                  controller: _controlador,
                  itemCount: _publicacao.midias.length,
                  onPageChanged: (int indice) {
                    setState(() {
                      _indiceAtual = indice;
                    });
                  },
                  itemBuilder: (BuildContext context, int indice) {
                    return _MidiaAmpliada(
                      midia: _publicacao.midias[indice],
                    );
                  },
                ),
                if (_indiceAtual > 0)
                  Positioned(
                    left: EspacamentosDoAplicativo.pequeno,
                    top: 0,
                    bottom: 0,
                    child: Center(
                      child: IconButton.filledTonal(
                        key: const Key('midia-anterior'),
                        tooltip: 'Mídia anterior',
                        onPressed: () => _naveguePara(_indiceAtual - 1),
                        icon: const Icon(Icons.chevron_left_rounded),
                      ),
                    ),
                  ),
                if (_indiceAtual < _publicacao.midias.length - 1)
                  Positioned(
                    right: EspacamentosDoAplicativo.pequeno,
                    top: 0,
                    bottom: 0,
                    child: Center(
                      child: IconButton.filledTonal(
                        key: const Key('proxima-midia'),
                        tooltip: 'Próxima mídia',
                        onPressed: () => _naveguePara(_indiceAtual + 1),
                        icon: const Icon(Icons.chevron_right_rounded),
                      ),
                    ),
                  ),
                if (_publicacao.midias.length > 1)
                  Positioned(
                    top: EspacamentosDoAplicativo.pequeno,
                    left: 0,
                    right: 0,
                    child: Center(
                      child: DecoratedBox(
                        decoration: BoxDecoration(
                          color: Colors.black.withValues(alpha: 0.65),
                          borderRadius: BorderRadius.circular(99),
                        ),
                        child: Padding(
                          padding: const EdgeInsets.symmetric(
                            horizontal: EspacamentosDoAplicativo.pequeno,
                            vertical: EspacamentosDoAplicativo.minimo,
                          ),
                          child: Text(
                            '${_indiceAtual + 1}/'
                            '${_publicacao.midias.length}',
                            style: const TextStyle(color: Colors.white),
                          ),
                        ),
                      ),
                    ),
                  ),
              ],
            ),
          ),
          _PessoasMarcadasNaMidia(
            pessoas: midiaAtual.pessoasMarcadas,
            podeEditar: _publicacao.podeEditarMarcacoes &&
                participantes.isNotEmpty &&
                !_estaAtualizandoMarcacoes,
            estaAtualizando: _estaAtualizandoMarcacoes,
            aoAbrirPessoa: (PessoaMarcadaNaMidia pessoa) {
              context.push<void>(
                '/pessoas/${pessoa.identificadorDoUsuario}',
              );
            },
            aoEditar: () => _editeMarcacoesAsync(
              midiaAtual,
              participantes,
            ),
          ),
          _LegendaDaPublicacao(publicacao: _publicacao),
        ],
      ),
    );
  }

  void _naveguePara(int indice) {
    _controlador.animateToPage(
      indice,
      duration: const Duration(milliseconds: 220),
      curve: Curves.easeOutCubic,
    );
  }

  Future<void> _removaAsync() async {
    bool confirmou = await showDialog<bool>(
          context: context,
          builder: (BuildContext contextoDoDialogo) {
            return AlertDialog(
              title: const Text('Remover publicação?'),
              content: const Text(
                'Todas as mídias desta publicação serão removidas do encontro.',
              ),
              actions: <Widget>[
                TextButton(
                  onPressed: () => Navigator.of(contextoDoDialogo).pop(false),
                  child: const Text('Cancelar'),
                ),
                FilledButton(
                  key: const Key('confirmar-remocao-da-galeria'),
                  onPressed: () => Navigator.of(contextoDoDialogo).pop(true),
                  child: const Text('Remover'),
                ),
              ],
            );
          },
        ) ??
        false;

    if (!confirmou || !mounted) {
      return;
    }

    try {
      await ref.read(provedorDoRepositorioDeMemoriasDoEncontro).removaAsync(
            identificadorDoEncontro: _publicacao.identificadorDoEncontro,
            identificadorDaMemoria: _publicacao.identificador,
          );
      ref.invalidate(
        provedorDasMemoriasDoEncontro(
          _publicacao.identificadorDoEncontro,
        ),
      );

      if (mounted) {
        Navigator.of(context).pop();
      }
    } on Exception {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Não foi possível remover a mídia.')),
        );
      }
    }
  }

  Future<void> _editeMarcacoesAsync(
    MidiaDaMemoria midia,
    List<ParticipanteDoEncontro> participantes,
  ) async {
    List<String>? identificadores = await mostreSeletorDePessoasNaMidiaAsync(
      context,
      participantes: participantes,
      identificadoresSelecionados: midia.pessoasMarcadas
          .map(
            (PessoaMarcadaNaMidia pessoa) => pessoa.identificadorDoUsuario,
          )
          .toSet(),
    );

    if (identificadores == null || !mounted) {
      return;
    }

    setState(() {
      _estaAtualizandoMarcacoes = true;
    });

    try {
      await ref
          .read(provedorDoRepositorioDeMemoriasDoEncontro)
          .atualizeMarcacoesAsync(
            identificadorDoEncontro: _publicacao.identificadorDoEncontro,
            identificadorDaMemoria: _publicacao.identificador,
            identificadorDaMidia: midia.identificador,
            identificadoresDosUsuarios: identificadores,
          );

      Set<String> selecionados = identificadores.toSet();
      List<PessoaMarcadaNaMidia> pessoas = participantes
          .where(
            (ParticipanteDoEncontro participante) => selecionados.contains(
              participante.identificadorDoUsuario,
            ),
          )
          .map(
            (ParticipanteDoEncontro participante) => PessoaMarcadaNaMidia(
              identificadorDoUsuario: participante.identificadorDoUsuario,
              nome: participante.nome,
              urlDaFotoDePerfil: participante.urlDaFotoDePerfil,
            ),
          )
          .toList();
      List<MidiaDaMemoria> midias = List<MidiaDaMemoria>.from(
        _publicacao.midias,
      );
      midias[_indiceAtual] = midia.copieComPessoasMarcadas(pessoas);

      setState(() {
        _publicacao = _publicacao.copieComMidias(midias);
        _estaAtualizandoMarcacoes = false;
      });
      ref.invalidate(
        provedorDasMemoriasDoEncontro(_publicacao.identificadorDoEncontro),
      );
    } on ExcecaoDaApi catch (excecao) {
      if (mounted) {
        setState(() {
          _estaAtualizandoMarcacoes = false;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(excecao.mensagem)),
        );
      }
    }
  }
}

class _PessoasMarcadasNaMidia extends StatelessWidget {
  const _PessoasMarcadasNaMidia({
    required this.pessoas,
    required this.podeEditar,
    required this.estaAtualizando,
    required this.aoAbrirPessoa,
    required this.aoEditar,
  });

  final List<PessoaMarcadaNaMidia> pessoas;
  final bool podeEditar;
  final bool estaAtualizando;
  final ValueChanged<PessoaMarcadaNaMidia> aoAbrirPessoa;
  final VoidCallback aoEditar;

  @override
  Widget build(BuildContext context) {
    if (pessoas.isEmpty && !podeEditar && !estaAtualizando) {
      return const SizedBox.shrink();
    }

    return SafeArea(
      top: false,
      bottom: false,
      child: Padding(
        padding: const EdgeInsets.symmetric(
          horizontal: EspacamentosDoAplicativo.padrao,
          vertical: EspacamentosDoAplicativo.pequeno,
        ),
        child: Row(
          children: <Widget>[
            Expanded(
              child: pessoas.isEmpty
                  ? const Text(
                      'Ninguém marcado nesta mídia',
                      style: TextStyle(color: Colors.white60),
                    )
                  : SizedBox(
                      height: 42,
                      child: ListView.separated(
                        scrollDirection: Axis.horizontal,
                        itemCount: pessoas.length,
                        separatorBuilder: (_, __) => const SizedBox(
                          width: EspacamentosDoAplicativo.pequeno,
                        ),
                        itemBuilder: (BuildContext context, int indice) {
                          PessoaMarcadaNaMidia pessoa = pessoas[indice];
                          return ActionChip(
                            key: Key(
                              'abrir-pessoa-marcada-'
                              '${pessoa.identificadorDoUsuario}',
                            ),
                            avatar: FotoDePerfil(
                              url: pessoa.urlDaFotoDePerfil,
                              iniciais: pessoa.iniciais,
                              dimensao: 28,
                            ),
                            label: Text(pessoa.nome),
                            onPressed: () => aoAbrirPessoa(pessoa),
                          );
                        },
                      ),
                    ),
            ),
            if (podeEditar || estaAtualizando) ...<Widget>[
              const SizedBox(width: EspacamentosDoAplicativo.pequeno),
              IconButton(
                key: const Key('editar-pessoas-marcadas-na-midia'),
                tooltip: 'Editar pessoas marcadas',
                onPressed: podeEditar ? aoEditar : null,
                icon: estaAtualizando
                    ? const SizedBox.square(
                        dimension: 18,
                        child: CircularProgressIndicator(
                          strokeWidth: 2,
                          color: Colors.white,
                        ),
                      )
                    : const Icon(Icons.edit_outlined, color: Colors.white),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

class _CabecalhoDaPublicacao extends StatelessWidget {
  const _CabecalhoDaPublicacao({required this.publicacao});

  final MemoriaDoEncontro publicacao;

  @override
  Widget build(BuildContext context) {
    String nome = publicacao.nomeDoAutor.trim();
    String iniciais = nome.isEmpty ? '?' : nome[0].toUpperCase();

    return Padding(
      padding: const EdgeInsets.all(EspacamentosDoAplicativo.padrao),
      child: Row(
        children: <Widget>[
          FotoDePerfil(
            url: publicacao.urlDaFotoDePerfilDoAutor,
            iniciais: iniciais,
            dimensao: 42,
          ),
          const SizedBox(width: EspacamentosDoAplicativo.pequeno),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: <Widget>[
                Text(
                  publicacao.nomeDoAutor,
                  style: const TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                Text(
                  DateFormat(
                    "d 'de' MMMM 'de' y, HH:mm",
                    'pt_BR',
                  ).format(publicacao.criadoEm),
                  style: const TextStyle(
                    color: Colors.white60,
                    fontSize: 12,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _MidiaAmpliada extends StatelessWidget {
  const _MidiaAmpliada({required this.midia});

  final MidiaDaMemoria midia;

  @override
  Widget build(BuildContext context) {
    bool ehVideo = _ehVideo(midia);
    String recurso = ConfiguracaoDoAmbiente.crieUrlAbsoluta(midia.url);

    if (ehVideo) {
      return VideoPrivado(
        recurso: recurso,
        tipoDeConteudo: midia.tipoDeConteudo,
        exibaControles: true,
      );
    }

    return InteractiveViewer(
      minScale: 0.8,
      maxScale: 4,
      child: Center(
        child: ImagemPrivada(
          recurso: recurso,
          ajuste: BoxFit.contain,
          construaSubstituta: (_) => const _MidiaIndisponivel(ampliada: true),
        ),
      ),
    );
  }
}

class _LegendaDaPublicacao extends StatelessWidget {
  const _LegendaDaPublicacao({required this.publicacao});

  final MemoriaDoEncontro publicacao;

  @override
  Widget build(BuildContext context) {
    String? legenda = publicacao.legenda;

    if (legenda == null || legenda.trim().isEmpty) {
      return const SizedBox.shrink();
    }

    return SafeArea(
      top: false,
      child: Padding(
        padding: const EdgeInsets.fromLTRB(
          EspacamentosDoAplicativo.padrao,
          EspacamentosDoAplicativo.pequeno,
          EspacamentosDoAplicativo.padrao,
          EspacamentosDoAplicativo.padrao,
        ),
        child: Align(
          alignment: Alignment.centerLeft,
          child: Text.rich(
            TextSpan(
              children: <InlineSpan>[
                TextSpan(
                  text: '${publicacao.nomeDoAutor}: ',
                  style: const TextStyle(fontWeight: FontWeight.w600),
                ),
                TextSpan(text: legenda),
              ],
            ),
            style: const TextStyle(color: Colors.white),
          ),
        ),
      ),
    );
  }
}

class _MidiaIndisponivel extends StatelessWidget {
  const _MidiaIndisponivel({this.ampliada = false});

  final bool ampliada;

  @override
  Widget build(BuildContext context) {
    return ColoredBox(
      color: ampliada ? Colors.black : CoresDoAplicativo.fundoDoCartao,
      child: Center(
        child: Icon(
          Icons.broken_image_outlined,
          size: ampliada ? 58 : 30,
          color: ampliada ? Colors.white54 : CoresDoAplicativo.textoTerciario,
        ),
      ),
    );
  }
}

bool _ehVideo(MidiaDaMemoria midia) {
  return midia.tipoDeConteudo.toLowerCase().startsWith('video/');
}

class _GaleriaVazia extends StatelessWidget {
  const _GaleriaVazia();

  @override
  Widget build(BuildContext context) {
    return const Center(
      child: EstadoVazio(
        icone: Icons.photo_library_outlined,
        titulo: 'Nenhuma foto compartilhada ainda',
        descricao: 'As fotos publicadas no mural aparecerão aqui.',
      ),
    );
  }
}

class _ErroDaGaleria extends StatelessWidget {
  const _ErroDaGaleria({required this.aoTentarNovamente});

  final VoidCallback aoTentarNovamente;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(EspacamentosDoAplicativo.padrao),
        child: CartaoDoAplicativo(
          filho: EstadoVazio(
            icone: Icons.cloud_off_outlined,
            titulo: 'Não foi possível carregar as mídias',
            descricao: 'Verifique sua conexão e tente novamente.',
            acao: FilledButton(
              onPressed: aoTentarNovamente,
              child: const Text('Tentar novamente'),
            ),
          ),
        ),
      ),
    );
  }
}
