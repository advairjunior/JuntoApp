import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cartao_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estrutura_responsiva_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/configuracao/configuracao_do_ambiente.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/imagem_privada.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/raios_do_aplicativo.dart';
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
      body: EstruturaResponsivaDoAplicativo(
        filho: SafeArea(
          child: memorias.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (_, __) => _ErroDaGaleria(
              aoTentarNovamente: () => ref.invalidate(
                provedorDasMemoriasDoEncontro(identificadorDoEncontro),
              ),
            ),
            data: (List<MemoriaDoEncontro> itens) {
              List<_ItemDaGaleria> midias = itens
                  .expand(
                    (MemoriaDoEncontro memoria) => memoria.midias.map(
                      (MidiaDaMemoria midia) => _ItemDaGaleria(memoria, midia),
                    ),
                  )
                  .toList();

              if (midias.isEmpty) {
                return const _GaleriaVazia();
              }

              return RefreshIndicator(
                onRefresh: () async {
                  ref.invalidate(
                    provedorDasMemoriasDoEncontro(identificadorDoEncontro),
                  );
                  await ref.read(
                    provedorDasMemoriasDoEncontro(
                      identificadorDoEncontro,
                    ).future,
                  );
                },
                child: GridView.builder(
                  padding: const EdgeInsets.all(
                    EspacamentosDoAplicativo.padrao,
                  ),
                  gridDelegate: const SliverGridDelegateWithMaxCrossAxisExtent(
                    maxCrossAxisExtent: 180,
                    mainAxisSpacing: EspacamentosDoAplicativo.pequeno,
                    crossAxisSpacing: EspacamentosDoAplicativo.pequeno,
                  ),
                  itemCount: midias.length,
                  itemBuilder: (BuildContext context, int indice) {
                    return _MiniaturaDaGaleria(item: midias[indice]);
                  },
                ),
              );
            },
          ),
        ),
      ),
    );
  }
}

class _ItemDaGaleria {
  const _ItemDaGaleria(this.memoria, this.midia);

  final MemoriaDoEncontro memoria;
  final MidiaDaMemoria midia;
}

class _MiniaturaDaGaleria extends ConsumerWidget {
  const _MiniaturaDaGaleria({required this.item});

  final _ItemDaGaleria item;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    String recurso = ConfiguracaoDoAmbiente.crieUrlAbsoluta(item.midia.url);

    return Semantics(
      button: true,
      label: 'Abrir foto de ${item.memoria.nomeDoAutor}',
      child: InkWell(
        key: Key('midia-${item.midia.identificador}'),
        onTap: () => _abraImagem(context, ref, recurso),
        borderRadius: BorderRadius.circular(RaiosDoAplicativo.medio),
        child: ClipRRect(
          borderRadius: BorderRadius.circular(RaiosDoAplicativo.medio),
          child: ImagemPrivada(
            recurso: recurso,
            construaSubstituta: (_) => const ColoredBox(
              color: CoresDoAplicativo.fundoDoCartao,
              child: Center(child: Icon(Icons.broken_image_outlined)),
            ),
          ),
        ),
      ),
    );
  }

  Future<void> _abraImagem(
    BuildContext context,
    WidgetRef ref,
    String recurso,
  ) {
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
              title: Text(item.memoria.nomeDoAutor),
              leading: IconButton(
                tooltip: 'Fechar',
                onPressed: () => Navigator.of(context).pop(),
                icon: const Icon(Icons.close_rounded),
              ),
              actions: <Widget>[
                if (item.memoria.usuarioAtual)
                  IconButton(
                    key: const Key('remover-memoria-da-galeria'),
                    tooltip: 'Remover foto',
                    onPressed: () => _removaAsync(context, ref),
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
                SafeArea(
                  top: false,
                  child: Padding(
                    padding: const EdgeInsets.all(
                      EspacamentosDoAplicativo.padrao,
                    ),
                    child: Column(
                      children: <Widget>[
                        if (item.memoria.legenda != null &&
                            item.memoria.legenda!.trim().isNotEmpty)
                          Text(
                            item.memoria.legenda!,
                            textAlign: TextAlign.center,
                            style: const TextStyle(color: Colors.white),
                          ),
                        const SizedBox(
                          height: EspacamentosDoAplicativo.minimo,
                        ),
                        Text(
                          DateFormat('dd/MM/yyyy • HH:mm').format(
                            item.memoria.criadoEm,
                          ),
                          style: const TextStyle(color: Colors.white60),
                        ),
                      ],
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

  Future<void> _removaAsync(BuildContext context, WidgetRef ref) async {
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
                  key: const Key('confirmar-remocao-da-galeria'),
                  onPressed: () => Navigator.of(contextoDoDialogo).pop(true),
                  child: const Text('Remover'),
                ),
              ],
            );
          },
        ) ??
        false;

    if (!confirmou || !context.mounted) {
      return;
    }

    try {
      await ref.read(provedorDoRepositorioDeMemoriasDoEncontro).removaAsync(
            identificadorDoEncontro: item.memoria.identificadorDoEncontro,
            identificadorDaMemoria: item.memoria.identificador,
          );
      ref.invalidate(
        provedorDasMemoriasDoEncontro(
          item.memoria.identificadorDoEncontro,
        ),
      );

      if (context.mounted) {
        Navigator.of(context).pop();
      }
    } on Exception {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Não foi possível remover a foto.')),
        );
      }
    }
  }
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
