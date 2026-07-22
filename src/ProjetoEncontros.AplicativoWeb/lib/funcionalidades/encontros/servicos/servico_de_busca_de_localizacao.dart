import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/cliente_http_autenticado.dart';

class ResultadoDaBuscaDeLocalizacao {
  const ResultadoDaBuscaDeLocalizacao({
    required this.descricao,
    required this.latitude,
    required this.longitude,
  });

  final String descricao;
  final double latitude;
  final double longitude;

  factory ResultadoDaBuscaDeLocalizacao.deJson(
    Map<String, dynamic> json,
  ) {
    return ResultadoDaBuscaDeLocalizacao(
      descricao: json['descricao'] as String,
      latitude: (json['latitude'] as num).toDouble(),
      longitude: (json['longitude'] as num).toDouble(),
    );
  }
}

abstract interface class IServicoDeBuscaDeLocalizacao {
  Future<List<ResultadoDaBuscaDeLocalizacao>> busqueAsync(String termo);
}

final provedorDoServicoDeBuscaDeLocalizacao =
    Provider<IServicoDeBuscaDeLocalizacao>((Ref referencia) {
  return ServicoDeBuscaDeLocalizacao(
    referencia.watch(provedorDoClienteHttpAutenticado),
  );
});

class ServicoDeBuscaDeLocalizacao implements IServicoDeBuscaDeLocalizacao {
  ServicoDeBuscaDeLocalizacao(this._cliente);

  final Dio _cliente;

  @override
  Future<List<ResultadoDaBuscaDeLocalizacao>> busqueAsync(String termo) async {
    String termoNormalizado = termo.trim();

    if (termoNormalizado.length < 3) {
      return <ResultadoDaBuscaDeLocalizacao>[];
    }

    Response<dynamic> resposta = await _cliente.post<dynamic>(
      '/api/localizacoes/busca',
      data: <String, dynamic>{'termo': termoNormalizado},
    );
    List<dynamic> dados = resposta.data as List<dynamic>? ?? <dynamic>[];

    return dados
        .whereType<Map<String, dynamic>>()
        .map(ResultadoDaBuscaDeLocalizacao.deJson)
        .toList(growable: false);
  }
}
