import 'dart:typed_data';

import 'package:flutter/material.dart';

class AudioPrivado extends StatelessWidget {
  const AudioPrivado({
    required this.recurso,
    required this.tipoDeConteudo,
    super.key,
  });

  final String recurso;
  final String tipoDeConteudo;

  @override
  Widget build(BuildContext context) {
    return const _AudioIndisponivel();
  }
}

class AudioComBytes extends StatelessWidget {
  const AudioComBytes({
    required this.bytes,
    required this.tipoDeConteudo,
    super.key,
  });

  final Uint8List bytes;
  final String tipoDeConteudo;

  @override
  Widget build(BuildContext context) {
    return const _AudioIndisponivel();
  }
}

class _AudioIndisponivel extends StatelessWidget {
  const _AudioIndisponivel();

  @override
  Widget build(BuildContext context) {
    return const SizedBox(
      height: 48,
      child: Center(child: Icon(Icons.mic_none_rounded)),
    );
  }
}
