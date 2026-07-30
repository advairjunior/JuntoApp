import 'dart:async';
import 'dart:js_interop';
import 'dart:typed_data';

import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/servicos/seletor_de_imagem.dart';
import 'package:web/web.dart' as web;

Future<ImagemSelecionada?> abraDialogoDaCameraAsync() async {
  final Completer<ImagemSelecionada?> completer =
      Completer<ImagemSelecionada?>();

  final _ControladorDaCamera controlador = _ControladorDaCamera();
  await controlador.inicieAsync(completer);

  return completer.future;
}

class _ControladorDaCamera {
  web.HTMLVideoElement? _videoElement;
  web.MediaStream? _stream;
  bool _ehFrontal = false;
  bool _estaAlternando = false;
  bool _estaCapturando = false;

  web.HTMLElement? _overlay;
  web.HTMLButtonElement? _botaoCapturar;
  web.HTMLButtonElement? _botaoAlternar;
  web.HTMLElement? _indicadorDeCarregamento;

  Future<void> inicieAsync(Completer<ImagemSelecionada?> completer) async {
    _crieOverlay(completer);

    try {
      await _inicieComCameraPreferencialAsync();
    } catch (_) {
      _destrua();
      completer.complete(null);
    }
  }

  void _crieOverlay(Completer<ImagemSelecionada?> completer) {
    final web.HTMLElement overlay =
        web.document.createElement('div') as web.HTMLElement;
    overlay.id = 'camera-overlay';
    overlay.style
      ..position = 'fixed'
      ..top = '0'
      ..left = '0'
      ..width = '100%'
      ..height = '100%'
      ..backgroundColor = '#000'
      ..zIndex = '99999'
      ..display = 'flex'
      ..flexDirection = 'column'
      ..alignItems = 'center'
      ..justifyContent = 'center';
    _overlay = overlay;

    final web.HTMLVideoElement video = web.HTMLVideoElement();
    video.autoplay = true;
    video.playsInline = true;
    video.muted = true;
    video.style
      ..width = '100%'
      ..height = '100%'
      ..objectFit = 'cover'
      ..position = 'absolute'
      ..top = '0'
      ..left = '0';
    _videoElement = video;
    overlay.appendChild(video);

    final web.HTMLElement carregando =
        web.document.createElement('div') as web.HTMLElement;
    carregando.style
      ..position = 'absolute'
      ..top = '50%'
      ..left = '50%'
      ..transform = 'translate(-50%, -50%)'
      ..color = '#fff'
      ..fontSize = '16px'
      ..fontFamily = 'sans-serif';
    carregando.textContent = 'Abrindo câmera…';
    _indicadorDeCarregamento = carregando;
    overlay.appendChild(carregando);

    final web.HTMLElement barra =
        web.document.createElement('div') as web.HTMLElement;
    barra.style
      ..position = 'absolute'
      ..bottom = '40px'
      ..left = '0'
      ..right = '0'
      ..display = 'flex'
      ..alignItems = 'center'
      ..justifyContent = 'center'
      ..gap = '32px';
    overlay.appendChild(barra);

    final web.HTMLButtonElement btnFechar = web.HTMLButtonElement();
    btnFechar.textContent = '×';
    btnFechar.ariaLabel = 'Fechar câmera';
    _estilizeBotaoRedondo(
      btnFechar,
      tamanho: '44px',
      fundo: 'rgba(0,0,0,0.55)',
    );
    btnFechar.style
      ..position = 'absolute'
      ..top = '20px'
      ..left = '20px';
    btnFechar.addEventListener(
      'click',
      (web.Event _) {
        _destrua();
        if (!completer.isCompleted) {
          completer.complete(null);
        }
      }.toJS,
    );
    overlay.appendChild(btnFechar);

    final web.HTMLButtonElement btnAlternar = web.HTMLButtonElement();
    btnAlternar.textContent = '↻';
    btnAlternar.ariaLabel = 'Alternar câmera';
    _estilizeBotaoRedondo(
      btnAlternar,
      tamanho: '52px',
      fundo: 'rgba(255,255,255,0.15)',
    );
    btnAlternar.addEventListener(
      'click',
      (web.Event _) {
        if (!_estaAlternando) {
          _alterneAsync(completer);
        }
      }.toJS,
    );
    _botaoAlternar = btnAlternar;
    barra.appendChild(btnAlternar);

    final web.HTMLButtonElement btnCapturar = web.HTMLButtonElement();
    btnCapturar.ariaLabel = 'Tirar foto';
    _estilizeBotaoRedondo(
      btnCapturar,
      tamanho: '72px',
      fundo: '#ffffff',
    );
    btnCapturar.style
      ..border = '5px solid rgba(255,255,255,0.45)'
      ..boxShadow = 'inset 0 0 0 3px #111111';
    btnCapturar.addEventListener(
      'click',
      (web.Event _) {
        if (!_estaCapturando && !_estaAlternando) {
          _captureAsync(completer);
        }
      }.toJS,
    );
    _botaoCapturar = btnCapturar;
    barra.appendChild(btnCapturar);

    web.document.body!.appendChild(overlay);
  }

  void _estilizeBotaoRedondo(
    web.HTMLButtonElement el, {
    required String tamanho,
    required String fundo,
  }) {
    el.style
      ..width = tamanho
      ..height = tamanho
      ..borderRadius = '50%'
      ..border = '2px solid rgba(255,255,255,0.35)'
      ..backgroundColor = fundo
      ..color = '#fff'
      ..fontSize = '24px'
      ..cursor = 'pointer'
      ..display = 'flex'
      ..alignItems = 'center'
      ..justifyContent = 'center'
      ..padding = '0'
      ..zIndex = '100000';
  }

  Future<void> _inicieComCameraPreferencialAsync() async {
    try {
      await _inicieStreamAsync();
    } catch (_) {
      _ehFrontal = true;
      await _inicieStreamAsync();
    }
  }

  Future<void> _inicieStreamAsync() async {
    await _encerreStreamAtualAsync();

    final web.MediaStream novoStream =
        await _obtenhaStreamAsync(frontal: _ehFrontal);
    _stream = novoStream;

    final web.HTMLVideoElement? video = _videoElement;
    if (video != null) {
      video.srcObject = novoStream;
      video.style.transform = _ehFrontal ? 'scaleX(-1)' : 'none';
      await video.play().toDart;
    }

    _indicadorDeCarregamento?.style.display = 'none';
  }

  Future<web.MediaStream> _obtenhaStreamAsync({required bool frontal}) async {
    final String facingMode = frontal ? 'user' : 'environment';
    final web.MediaStreamConstraints constraints = web.MediaStreamConstraints(
      video: web.MediaTrackConstraints(
        facingMode: facingMode.toJS,
      ),
      audio: false.toJS,
    );

    return web.window.navigator.mediaDevices.getUserMedia(constraints).toDart;
  }

  Future<void> _encerreStreamAtualAsync() async {
    final web.MediaStream? streamAnterior = _stream;
    if (streamAnterior != null) {
      for (web.MediaStreamTrack faixa in streamAnterior.getTracks().toDart) {
        faixa.stop();
      }
      _stream = null;
      _videoElement?.srcObject = null;
    }
  }

  Future<void> _alterneAsync(Completer<ImagemSelecionada?> completer) async {
    _estaAlternando = true;
    _atualizeEstadoDosBotoes();
    _indicadorDeCarregamento
      ?..textContent = 'Alternando câmera…'
      ..style.display = 'block';

    try {
      _ehFrontal = !_ehFrontal;
      await _inicieStreamAsync();
    } catch (_) {
      _ehFrontal = !_ehFrontal;
      try {
        await _inicieStreamAsync();
      } catch (_) {
        _destrua();
        if (!completer.isCompleted) {
          completer.complete(null);
        }
        return;
      }
    } finally {
      _estaAlternando = false;
      _atualizeEstadoDosBotoes();
    }
  }

  Future<void> _captureAsync(Completer<ImagemSelecionada?> completer) async {
    _estaCapturando = true;
    _atualizeEstadoDosBotoes();

    final web.HTMLVideoElement? video = _videoElement;
    if (video == null) {
      _estaCapturando = false;
      _atualizeEstadoDosBotoes();
      return;
    }

    final int largura = video.videoWidth;
    final int altura = video.videoHeight;

    if (largura == 0 || altura == 0) {
      _estaCapturando = false;
      _atualizeEstadoDosBotoes();
      return;
    }

    final web.HTMLCanvasElement canvas = web.HTMLCanvasElement()
      ..width = largura
      ..height = altura;
    final web.CanvasRenderingContext2D ctx = canvas.context2D;

    // O espelho pertence somente ao CSS do video; o canvas recebe o quadro real.
    ctx.drawImage(video, 0, 0);

    final Completer<web.Blob> blobCompleter = Completer<web.Blob>();
    canvas.toBlob(
      ((web.Blob? blob) {
        if (blob == null) {
          blobCompleter.completeError(
            StateError('Falha ao gerar blob da imagem.'),
          );
          return;
        }
        blobCompleter.complete(blob);
      }).toJS,
      'image/jpeg',
      0.92.toJS,
    );

    try {
      final web.Blob blob = await blobCompleter.future;
      final JSArrayBuffer buffer = await blob.arrayBuffer().toDart;
      final Uint8List bytes = Uint8List.view(buffer.toDart);

      _destrua();

      if (!completer.isCompleted) {
        completer.complete(
          ImagemSelecionada(
            nome: 'foto_${DateTime.now().millisecondsSinceEpoch}.jpg',
            tipoDeConteudo: 'image/jpeg',
            conteudo: bytes,
          ),
        );
      }
    } catch (_) {
      _destrua();
      if (!completer.isCompleted) {
        completer.complete(null);
      }
    }
  }

  void _atualizeEstadoDosBotoes() {
    bool estaOcupada = _estaAlternando || _estaCapturando;
    _botaoAlternar?.disabled = estaOcupada;
    _botaoCapturar?.disabled = estaOcupada;
    _botaoAlternar?.style.opacity = estaOcupada ? '0.4' : '1';
    _botaoCapturar?.style.opacity = estaOcupada ? '0.6' : '1';
  }

  void _destrua() {
    final web.MediaStream? stream = _stream;
    if (stream != null) {
      for (web.MediaStreamTrack faixa in stream.getTracks().toDart) {
        faixa.stop();
      }
      _stream = null;
    }
    _videoElement?.srcObject = null;
    _overlay?.remove();
    _overlay = null;
  }
}
