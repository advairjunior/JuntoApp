import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/midias/contrato_do_gravador_de_audio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/midias/gravador_de_audio.dart';

final provedorDoGravadorDeAudio = Provider<IGravadorDeAudio>((Ref referencia) {
  IGravadorDeAudio gravador = crieGravadorDeAudio();
  referencia.onDispose(gravador.descarte);
  return gravador;
});
