import 'package:flutter/foundation.dart';

abstract final class ConfiguracaoDoAmbiente {
  static String get urlDaApi {
    const String urlConfigurada = String.fromEnvironment('URL_DA_API');

    if (urlConfigurada.trim().isNotEmpty) {
      return urlConfigurada.trim();
    }

    if (!kIsWeb) {
      return 'http://localhost:5281';
    }

    Uri enderecoDoAplicativo = Uri.base;
    bool ambienteEhLocal = enderecoDoAplicativo.host == 'localhost' ||
        enderecoDoAplicativo.host == '127.0.0.1';

    if (ambienteEhLocal) {
      return Uri(
        scheme: enderecoDoAplicativo.scheme,
        host: enderecoDoAplicativo.host,
        port: 5281,
      ).origin;
    }

    return enderecoDoAplicativo.origin;
  }

  static String crieUrlAbsoluta(String? caminho) {
    if (caminho == null || caminho.trim().isEmpty) {
      return '';
    }

    Uri endereco = Uri.parse(caminho);

    if (endereco.hasScheme) {
      return endereco.toString();
    }

    return Uri.parse(urlDaApi).resolve(caminho).toString();
  }
}
