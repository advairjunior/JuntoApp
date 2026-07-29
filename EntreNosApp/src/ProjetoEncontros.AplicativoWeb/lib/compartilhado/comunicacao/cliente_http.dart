import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/configurador_do_adaptador_http.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/configuracao/configuracao_do_ambiente.dart';

final provedorDoClienteHttp = Provider<Dio>((Ref referencia) {
  return crieClienteHttp();
});

Dio crieClienteHttp() {
  Dio cliente = Dio(
    BaseOptions(
      baseUrl: ConfiguracaoDoAmbiente.urlDaApi,
      connectTimeout: const Duration(seconds: 15),
      receiveTimeout: const Duration(seconds: 30),
      headers: const <String, String>{'Accept': 'application/json'},
    ),
  );

  configureAdaptadorHttp(cliente);

  return cliente;
}
