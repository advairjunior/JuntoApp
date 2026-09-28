import 'dart:async';
import 'dart:js_interop';
import 'dart:math' as matematica;
import 'dart:typed_data';

import 'package:web/web.dart' as web;

const int _maiorDimensaoPermitida = 1920;

Future<Uint8List> corrijaEspelhamentoDaSelfieAsync(
  Uint8List conteudo,
  String tipoDeConteudo,
) async {
  web.Blob arquivo = web.Blob(
    <JSAny>[conteudo.toJS].toJS,
    web.BlobPropertyBag(type: tipoDeConteudo),
  );
  String enderecoTemporario = web.URL.createObjectURL(arquivo);

  try {
    web.HTMLImageElement imagem = web.HTMLImageElement()
      ..src = enderecoTemporario;
    await imagem.decode().toDart;

    int maiorDimensaoOriginal = matematica.max(
      imagem.naturalWidth,
      imagem.naturalHeight,
    );
    double escala = maiorDimensaoOriginal > _maiorDimensaoPermitida
        ? _maiorDimensaoPermitida / maiorDimensaoOriginal
        : 1;
    int larguraProcessada = (imagem.naturalWidth * escala).round();
    int alturaProcessada = (imagem.naturalHeight * escala).round();

    web.HTMLCanvasElement tela = web.HTMLCanvasElement()
      ..width = larguraProcessada
      ..height = alturaProcessada;
    web.CanvasRenderingContext2D contexto = tela.context2D;
    contexto.translate(tela.width, 0);
    contexto.scale(-1, 1);
    contexto.drawImage(imagem, 0, 0, larguraProcessada, alturaProcessada);

    Completer<web.Blob> resultado = Completer<web.Blob>();
    tela.toBlob(
      ((web.Blob? arquivoCorrigido) {
        if (arquivoCorrigido == null) {
          resultado.completeError(
            StateError('Não foi possível corrigir a selfie.'),
          );
          return;
        }

        resultado.complete(arquivoCorrigido);
      }).toJS,
      tipoDeConteudo,
      0.98.toJS,
    );

    web.Blob arquivoCorrigido = await resultado.future;
    JSArrayBuffer buffer = await arquivoCorrigido.arrayBuffer().toDart;
    return Uint8List.view(buffer.toDart);
  } catch (_) {
    return conteudo;
  } finally {
    web.URL.revokeObjectURL(enderecoTemporario);
  }
}
