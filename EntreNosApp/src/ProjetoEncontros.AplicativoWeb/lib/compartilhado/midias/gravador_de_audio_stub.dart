import 'package:projeto_encontros_aplicativo_web/compartilhado/midias/contrato_do_gravador_de_audio.dart';

IGravadorDeAudio crieGravadorDeAudio() {
  return GravadorDeAudioIndisponivel();
}

class GravadorDeAudioIndisponivel implements IGravadorDeAudio {
  @override
  bool get estaDisponivel => false;

  @override
  Future<void> inicieAsync() {
    throw UnsupportedError('A gravação de áudio não está disponível.');
  }

  @override
  Future<AudioGravado?> finalizeAsync(Duration duracao) async {
    return null;
  }

  @override
  Future<void> canceleAsync() async {}

  @override
  void descarte() {}
}
