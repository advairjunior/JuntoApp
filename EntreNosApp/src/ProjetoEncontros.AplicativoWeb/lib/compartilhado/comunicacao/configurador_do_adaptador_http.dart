import 'package:dio/dio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/configurador_do_adaptador_http_padrao.dart'
    if (dart.library.html) 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/configurador_do_adaptador_http_web.dart';

void configureAdaptadorHttp(Dio cliente) {
  configureAdaptadorHttpDaPlataforma(cliente);
}
