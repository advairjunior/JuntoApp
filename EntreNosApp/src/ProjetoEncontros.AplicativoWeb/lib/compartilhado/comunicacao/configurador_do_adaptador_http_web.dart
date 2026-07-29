import 'package:dio/browser.dart';
import 'package:dio/dio.dart';

void configureAdaptadorHttpDaPlataforma(Dio cliente) {
  BrowserHttpClientAdapter adaptador = BrowserHttpClientAdapter();
  adaptador.withCredentials = true;
  cliente.httpClientAdapter = adaptador;
}
