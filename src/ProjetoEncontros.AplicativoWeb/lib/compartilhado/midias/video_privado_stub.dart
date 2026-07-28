import 'dart:typed_data';

import 'package:flutter/material.dart';

class VideoPrivado extends StatelessWidget {
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
  Widget build(BuildContext context) {
    return const ColoredBox(
      color: Colors.black,
      child: Center(
        child: Icon(
          Icons.play_circle_outline_rounded,
          color: Colors.white70,
          size: 58,
        ),
      ),
    );
  }
}

class VideoComBytes extends StatelessWidget {
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
  Widget build(BuildContext context) {
    return const ColoredBox(
      color: Colors.black,
      child: Center(
        child: Icon(
          Icons.play_circle_outline_rounded,
          color: Colors.white70,
          size: 58,
        ),
      ),
    );
  }
}
