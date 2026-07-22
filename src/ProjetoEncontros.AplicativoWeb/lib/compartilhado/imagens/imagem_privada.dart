import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/repositorio_de_imagens_privadas.dart';

class ImagemPrivada extends ConsumerWidget {
  const ImagemPrivada({
    required this.recurso,
    required this.construaSubstituta,
    this.ajuste = BoxFit.cover,
    super.key,
  });

  final String recurso;
  final BoxFit ajuste;
  final WidgetBuilder construaSubstituta;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    AsyncValue<Uint8List?> imagem = ref.watch(
      provedorDosBytesDaImagemPrivada(recurso),
    );

    return imagem.when(
      data: (Uint8List? bytes) => bytes == null
          ? construaSubstituta(context)
          : Image.memory(
              bytes,
              fit: ajuste,
              gaplessPlayback: true,
              errorBuilder: (_, __, ___) => construaSubstituta(context),
            ),
      loading: () => construaSubstituta(context),
      error: (_, __) => construaSubstituta(context),
    );
  }
}
