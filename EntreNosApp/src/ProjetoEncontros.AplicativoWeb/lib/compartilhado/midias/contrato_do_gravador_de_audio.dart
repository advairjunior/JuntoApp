import 'dart:typed_data';

class AudioGravado {
  const AudioGravado({
    required this.bytes,
    required this.nomeDoArquivo,
    required this.tipoDeConteudo,
    required this.duracao,
  });

  final Uint8List bytes;
  final String nomeDoArquivo;
  final String tipoDeConteudo;
  final Duration duracao;
}

abstract interface class IGravadorDeAudio {
  bool get estaDisponivel;

  Future<void> inicieAsync();

  Future<AudioGravado?> finalizeAsync(Duration duracao);

  Future<void> canceleAsync();

  void descarte();
}
