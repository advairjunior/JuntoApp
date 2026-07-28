import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cartao_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/configuracao/configuracao_do_ambiente.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/foto_de_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/imagem_privada.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/midias/video_privado.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/dados/repositorio_de_memorias_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/memoria_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/midia_da_memoria.dart';

class TelaDeMidiasDoEncontro extends ConsumerWidget {
  const TelaDeMidiasDoEncontro({
    required this.identificadorDoEncontro,
    super.key,
  });

  final String identificadorDoEncontro;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    AsyncValue<List<MemoriaDoEncontro>> memorias = ref.watch(
      provedorDasMemoriasDoEncontro(identificadorDoEncontro),
    );

    return Scaffold(
      backgroundColor: CoresDoAplicativo.fundoPrincipal,
      appBar: AppBar(
        title: const Text('Mídias do encontro'),
        leading: IconButton(
          tooltip: 'Voltar',
          onPressed: () => context.pop(),
          icon: const Icon(Icons.arrow_back_ios_new_rounded),
        ),
      ),
      body: _EstruturaDaGaleria(
        filho: SafeArea(
          child: memorias.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (_, __) => _ErroDaGaleria(
              aoTentarNovamente: () => ref.invalidate(
                provedorDasMemoriasDoEncontro(identificadorDoEncontro),
              ),
            ),
            data: (List<MemoriaDoEncontro> itens) {
              List<MemoriaDoEncontro> publicacoes = itens
                  .where(
                    (MemoriaDoEncontro memoria) => memoria.midias.isNotEmpty,
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
                          identificadorDoEncontro,
                        ),
                      );
                      await ref.read(
                        provedorDasMemoriasDoEncontro(
                          identificadorDoEncontro,
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
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _abraVisualizador(BuildContext context) {
    return showDialog<void>(
      context: context,
      useSafeArea: false,
      builder: (BuildContext context) {
        return Dialog.fullscreen(
          backgroundColor: Colors.black,
          child: _VisualizadorDaPublicacao(publicacao: publicacao),
        );
      },
    );
  }
}

class _VisualizadorDaPublicacao extends ConsumerStatefulWidget {
  const _VisualizadorDaPublicacao({required this.publicacao});

  final MemoriaDoEncontro publicacao;

  @override
  ConsumerState<_VisualizadorDaPublicacao> createState() =>
      _EstadoDoVisualizadorDaPublicacao();
}

class _EstadoDoVisualizadorDaPublicacao
    extends ConsumerState<_VisualizadorDaPublicacao> {
  final PageController _controlador = PageController();
  int _indiceAtual = 0;

  @override
  void dispose() {
    _controlador.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
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
          if (widget.publicacao.usuarioAtual)
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
          _CabecalhoDaPublicacao(publicacao: widget.publicacao),
          Expanded(
            child: Stack(
              children: <Widget>[
                PageView.builder(
                  key: const Key('visualizador-de-midias'),
                  controller: _controlador,
                  itemCount: widget.publicacao.midias.length,
                  onPageChanged: (int indice) {
                    setState(() {
                      _indiceAtual = indice;
                    });
                  },
                  itemBuilder: (BuildContext context, int indice) {
                    return _MidiaAmpliada(
                      midia: widget.publicacao.midias[indice],
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
                if (_indiceAtual < widget.publicacao.midias.length - 1)
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
                if (widget.publicacao.midias.length > 1)
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
                            '${widget.publicacao.midias.length}',
                            style: const TextStyle(color: Colors.white),
                          ),
                        ),
                      ),
                    ),
                  ),
              ],
            ),
          ),
          _LegendaDaPublicacao(publicacao: widget.publicacao),
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
            identificadorDoEncontro: widget.publicacao.identificadorDoEncontro,
            identificadorDaMemoria: widget.publicacao.identificador,
          );
      ref.invalidate(
        provedorDasMemoriasDoEncontro(
          widget.publicacao.identificadorDoEncontro,
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
