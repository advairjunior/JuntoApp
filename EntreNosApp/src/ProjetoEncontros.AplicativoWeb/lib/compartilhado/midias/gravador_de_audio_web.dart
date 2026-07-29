import 'dart:async';
import 'dart:js_interop';
import 'dart:typed_data';

import 'package:projeto_encontros_aplicativo_web/compartilhado/midias/contrato_do_gravador_de_audio.dart';
import 'package:web/web.dart' as web;

IGravadorDeAudio crieGravadorDeAudio() {
  return GravadorDeAudioWeb();
}

class GravadorDeAudioWeb implements IGravadorDeAudio {
  static const List<String> _tiposPreferenciais = <String>[
    'audio/mp4;codecs=mp4a.40.2',
    'audio/mp4',
    'audio/webm;codecs=opus',
    'audio/webm',
  ];

  web.MediaStream? _fluxoDoMicrofone;
  web.MediaRecorder? _gravador;
  final List<web.Blob> _partes = <web.Blob>[];
  Completer<void>? _conclusao;

  @override
  bool get estaDisponivel => _tiposPreferenciais.any(
        (String tipoDeConteudo) =>
            web.MediaRecorder.isTypeSupported(tipoDeConteudo),
      );

  @override
  Future<void> inicieAsync() async {
    await canceleAsync();

    web.MediaStream fluxo = await web.window.navigator.mediaDevices
        .getUserMedia(
          web.MediaStreamConstraints(
            audio: true.toJS,
            video: false.toJS,
          ),
        )
        .toDart;
    String? tipoDeConteudo = _obtenhaTipoDeConteudoSuportado();

    if (tipoDeConteudo == null) {
      for (web.MediaStreamTrack faixa in fluxo.getTracks().toDart) {
        faixa.stop();
      }

      throw UnsupportedError(
        'Este navegador não oferece um formato de áudio compatível.',
      );
    }

    web.MediaRecorder gravador = web.MediaRecorder(
      fluxo,
      web.MediaRecorderOptions(
        mimeType: tipoDeConteudo,
        audioBitsPerSecond: 96000,
      ),
    );

    _fluxoDoMicrofone = fluxo;
    _gravador = gravador;
    _partes.clear();
    _conclusao = Completer<void>();

    gravador.ondataavailable = ((web.Event evento) {
      web.BlobEvent eventoDeDados = evento as web.BlobEvent;

      if (eventoDeDados.data.size > 0) {
        _partes.add(eventoDeDados.data);
      }
    }).toJS;
    gravador.onstop = ((web.Event _) {
      _encerreFluxoDoMicrofone();

      if (_conclusao?.isCompleted == false) {
        _conclusao!.complete();
      }
    }).toJS;
    gravador.onerror = ((web.Event _) {
      _encerreFluxoDoMicrofone();

      if (_conclusao?.isCompleted == false) {
        _conclusao!.completeError(
          StateError('O navegador interrompeu a gravação de áudio.'),
        );
      }
    }).toJS;
    gravador.start();
  }

  @override
  Future<AudioGravado?> finalizeAsync(Duration duracao) async {
    web.MediaRecorder? gravador = _gravador;

    if (gravador == null || gravador.state == 'inactive') {
      return null;
    }

    gravador.stop();
    await _conclusao?.future;

    if (_partes.isEmpty) {
      _limpe();
      return null;
    }

    String tipoDeConteudo = _normalizeTipoDeConteudo(gravador.mimeType);
    web.Blob arquivo = web.Blob(
      _partes.map((web.Blob parte) => parte as JSAny).toList().toJS,
      web.BlobPropertyBag(type: tipoDeConteudo),
    );
    JSArrayBuffer buffer = await arquivo.arrayBuffer().toDart;
    Uint8List bytes = Uint8List.view(buffer.toDart);
    String extensao = tipoDeConteudo == 'audio/mp4' ? 'm4a' : 'webm';
    AudioGravado audio = AudioGravado(
      bytes: Uint8List.fromList(bytes),
      nomeDoArquivo: 'audio-${DateTime.now().millisecondsSinceEpoch}.$extensao',
      tipoDeConteudo: tipoDeConteudo,
      duracao: duracao,
    );

    _limpe();
    return audio;
  }

  @override
  Future<void> canceleAsync() async {
    web.MediaRecorder? gravador = _gravador;

    if (gravador != null && gravador.state != 'inactive') {
      gravador.stop();

      try {
        await _conclusao?.future;
      } on Object {
        // O cancelamento descarta qualquer falha produzida pelo navegador.
      }
    } else {
      _encerreFluxoDoMicrofone();
    }

    _limpe();
  }

  @override
  void descarte() {
    unawaited(canceleAsync());
  }

  String? _obtenhaTipoDeConteudoSuportado() {
    for (String tipoDeConteudo in _tiposPreferenciais) {
      if (web.MediaRecorder.isTypeSupported(tipoDeConteudo)) {
        return tipoDeConteudo;
      }
    }

    return null;
  }

  String _normalizeTipoDeConteudo(String tipoDeConteudo) {
    String tipoNormalizado = tipoDeConteudo.split(';').first.trim();

    if (tipoNormalizado == 'audio/mp4') {
      return 'audio/mp4';
    }

    return 'audio/webm';
  }

  void _encerreFluxoDoMicrofone() {
    web.MediaStream? fluxo = _fluxoDoMicrofone;

    if (fluxo == null) {
      return;
    }

    for (web.MediaStreamTrack faixa in fluxo.getTracks().toDart) {
      faixa.stop();
    }

    _fluxoDoMicrofone = null;
  }

  void _limpe() {
    _gravador = null;
    _conclusao = null;
    _partes.clear();
  }
}
