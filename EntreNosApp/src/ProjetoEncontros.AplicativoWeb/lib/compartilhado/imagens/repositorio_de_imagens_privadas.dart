import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/cliente_http_autenticado.dart';

abstract interface class IRepositorioDeImagensPrivadas {
  Future<Uint8List?> obtenhaAsync(String recurso);
}

final provedorDoRepositorioDeImagensPrivadas =
    Provider<IRepositorioDeImagensPrivadas>((Ref referencia) {
  return RepositorioDeImagensPrivadas(
    referencia.watch(provedorDoClienteHttpAutenticado),
  );
});

final provedorDosBytesDaImagemPrivada =
    FutureProvider.autoDispose.family<Uint8List?, String>(
  (Ref referencia, String recurso) {
    return referencia
        .watch(provedorDoRepositorioDeImagensPrivadas)
        .obtenhaAsync(recurso);
  },
);

final class RepositorioDeImagensPrivadas
    implements IRepositorioDeImagensPrivadas {
  RepositorioDeImagensPrivadas(this._cliente);

  final Dio _cliente;

  @override
  Future<Uint8List?> obtenhaAsync(String recurso) async {
    if (recurso.trim().isEmpty) {
      return null;
    }

    try {
      Response<List<int>> resposta = await _cliente.get<List<int>>(
        recurso,
        options: Options(
          responseType: ResponseType.bytes,
          headers: const <String, String>{'Cache-Control': 'no-store'},
        ),
      );
      List<int>? dados = resposta.data;

      return dados == null || dados.isEmpty ? null : Uint8List.fromList(dados);
    } on DioException {
      return null;
    }
  }
}
