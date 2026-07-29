import 'dart:js_interop';
import 'dart:typed_data';
import 'dart:ui_web' as interface_do_usuario_web;

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/repositorio_de_imagens_privadas.dart';
import 'package:web/web.dart' as web;

class AudioPrivado extends ConsumerWidget {
  const AudioPrivado({
    required this.recurso,
    required this.tipoDeConteudo,
    super.key,
  });

  final String recurso;
  final String tipoDeConteudo;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    AsyncValue<Uint8List?> audio = ref.watch(
      provedorDosBytesDaImagemPrivada(recurso),
    );

    return audio.when(
      data: (Uint8List? bytes) {
        if (bytes == null) {
          return const _AudioIndisponivel();
        }

        return AudioComBytes(
          bytes: bytes,
          tipoDeConteudo: tipoDeConteudo,
        );
      },
      loading: () => const SizedBox(
        height: 48,
        child: Center(child: CircularProgressIndicator()),
      ),
      error: (_, __) => const _AudioIndisponivel(),
    );
  }
}

class AudioComBytes extends StatefulWidget {
  const AudioComBytes({
    required this.bytes,
    required this.tipoDeConteudo,
    super.key,
  });

  final Uint8List bytes;
  final String tipoDeConteudo;

  @override
  State<AudioComBytes> createState() => _EstadoDoAudioComBytes();
}

class _EstadoDoAudioComBytes extends State<AudioComBytes> {
  late final String _tipoDaVisualizacao;
  late final String _urlTemporaria;

  @override
  void initState() {
    super.initState();
    _tipoDaVisualizacao =
        'audio-privado-${identityHashCode(this)}-${DateTime.now().microsecondsSinceEpoch}';
    web.Blob arquivo = web.Blob(
      <JSAny>[widget.bytes.toJS].toJS,
      web.BlobPropertyBag(type: widget.tipoDeConteudo),
    );
    _urlTemporaria = web.URL.createObjectURL(arquivo);
    web.HTMLAudioElement elemento = web.HTMLAudioElement()
      ..src = _urlTemporaria
      ..controls = true
      ..preload = 'metadata';
    elemento.style
      ..width = '100%'
      ..height = '44px';

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
    return SizedBox(
      height: 48,
      child: HtmlElementView(viewType: _tipoDaVisualizacao),
    );
  }
}

class _AudioIndisponivel extends StatelessWidget {
  const _AudioIndisponivel();

  @override
  Widget build(BuildContext context) {
    return const SizedBox(
      height: 48,
      child: Center(
        child: Icon(
          Icons.mic_off_outlined,
          color: Colors.white54,
        ),
      ),
    );
  }
}
