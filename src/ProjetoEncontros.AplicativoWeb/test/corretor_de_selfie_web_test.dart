@TestOn('browser')
library;

import 'dart:async';
import 'dart:js_interop';
import 'dart:typed_data';

import 'package:flutter_test/flutter_test.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/servicos/corretor_de_selfie_web.dart';
import 'package:web/web.dart' as web;

void main() {
  test(
    'limita as dimensões da selfie antes de recodificar no navegador',
    () async {
      Uint8List selfie = await _crieImagemAsync(largura: 2400, altura: 1200);

      Uint8List resultado = await corrijaEspelhamentoDaSelfieAsync(
        selfie,
        'image/jpeg',
      );
      ({int largura, int altura}) dimensoes =
          await _obtenhaDimensoesAsync(resultado);

      expect(dimensoes.largura, 1920);
      expect(dimensoes.altura, 960);
    },
  );
}

Future<Uint8List> _crieImagemAsync({
  required int largura,
  required int altura,
}) async {
  web.HTMLCanvasElement tela = web.HTMLCanvasElement()
    ..width = largura
    ..height = altura;
  web.CanvasRenderingContext2D contexto = tela.context2D;
  contexto.fillStyle = '#663399'.toJS;
  contexto.fillRect(0, 0, largura, altura);

  web.Blob arquivo = await _convertaEmBlobAsync(tela);
  JSArrayBuffer buffer = await arquivo.arrayBuffer().toDart;
  return Uint8List.view(buffer.toDart);
}

Future<({int largura, int altura})> _obtenhaDimensoesAsync(
  Uint8List conteudo,
) async {
  web.Blob arquivo = web.Blob(
    <JSAny>[conteudo.toJS].toJS,
    web.BlobPropertyBag(type: 'image/jpeg'),
  );
  String enderecoTemporario = web.URL.createObjectURL(arquivo);

  try {
    web.HTMLImageElement imagem = web.HTMLImageElement()
      ..src = enderecoTemporario;
    await imagem.decode().toDart;
    return (largura: imagem.naturalWidth, altura: imagem.naturalHeight);
  } finally {
    web.URL.revokeObjectURL(enderecoTemporario);
  }
}

Future<web.Blob> _convertaEmBlobAsync(web.HTMLCanvasElement tela) {
  Completer<web.Blob> resultado = Completer<web.Blob>();
  tela.toBlob(
    ((web.Blob? arquivo) {
      if (arquivo == null) {
        resultado.completeError(StateError('Falha ao criar imagem de teste.'));
        return;
      }

      resultado.complete(arquivo);
    }).toJS,
    'image/jpeg',
    0.9.toJS,
  );
  return resultado.future;
}
