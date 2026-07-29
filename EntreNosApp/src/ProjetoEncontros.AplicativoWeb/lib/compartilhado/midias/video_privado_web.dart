import 'dart:js_interop';
import 'dart:typed_data';
import 'dart:ui_web' as interface_do_usuario_web;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/repositorio_de_imagens_privadas.dart';
import 'package:web/web.dart' as web;

class VideoPrivado extends ConsumerWidget {
  const VideoPrivado({
    required this.recurso,
    required this.tipoDeConteudo,
    required this.exibaControles,
    super.key,
  });

  final String recurso;
  final String tipoDeConteudo;
  final bool exibaControles;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    AsyncValue<Uint8List?> video = ref.watch(
      provedorDosBytesDaImagemPrivada(recurso),
    );

    return video.when(
      data: (Uint8List? bytes) {
        if (bytes == null) {
          return const _VideoIndisponivel();
        }

        return VideoComBytes(
          bytes: bytes,
          tipoDeConteudo: tipoDeConteudo,
          exibaControles: exibaControles,
        );
      },
      loading: () => const ColoredBox(
        color: Colors.black,
        child: Center(child: CircularProgressIndicator()),
      ),
      error: (_, __) => const _VideoIndisponivel(),
    );
  }
}

class VideoComBytes extends StatefulWidget {
  const VideoComBytes({
    required this.bytes,
    required this.tipoDeConteudo,
    required this.exibaControles,
    super.key,
  });

  final Uint8List bytes;
  final String tipoDeConteudo;
  final bool exibaControles;

  @override
  State<VideoComBytes> createState() => _EstadoDoVideoComBytes();
}

class _EstadoDoVideoComBytes extends State<VideoComBytes> {
  late final String _tipoDaVisualizacao;
  late final String _urlTemporaria;

  @override
  void initState() {
    super.initState();
    _tipoDaVisualizacao =
        'video-privado-${identityHashCode(this)}-${DateTime.now().microsecondsSinceEpoch}';
    web.Blob arquivo = web.Blob(
      <JSAny>[widget.bytes.toJS].toJS,
      web.BlobPropertyBag(type: widget.tipoDeConteudo),
    );
    _urlTemporaria = web.URL.createObjectURL(arquivo);
    web.HTMLVideoElement elemento = web.HTMLVideoElement()
      ..src = _urlTemporaria
      ..controls = widget.exibaControles
      ..muted = !widget.exibaControles
      ..playsInline = true
      ..preload = 'metadata';
    elemento.style
      ..width = '100%'
      ..height = '100%'
      ..objectFit = 'cover'
      ..backgroundColor = 'black';

    interface_do_usuario_web.platformViewRegistry.registerViewFactory(
      _tipoDaVisualizacao,
      (int identificador) => elemento,
    );
  }

  @override
  void dispose() {
    web.URL.revokeObjectURL(_urlTemporaria);
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return HtmlElementView(viewType: _tipoDaVisualizacao);
  }
}

class _VideoIndisponivel extends StatelessWidget {
  const _VideoIndisponivel();

  @override
  Widget build(BuildContext context) {
    return const ColoredBox(
      color: Colors.black,
      child: Center(
        child: Icon(
          Icons.videocam_off_outlined,
          color: Colors.white54,
          size: 52,
        ),
      ),
    );
  }
}
