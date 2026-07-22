import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/cliente_http_autenticado.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/publicacoes/modelos/publicacao_do_encontro.dart';

abstract interface class IRepositorioDePublicacoesDoEncontro {
  Future<List<PublicacaoDoEncontro>> listeAsync(String identificadorDoEncontro);

  Future<PublicacaoDoEncontro> publiqueAsync({
    required String identificadorDoEncontro,
    required String texto,
  });
}

final provedorDoRepositorioDePublicacoesDoEncontro =
    Provider<IRepositorioDePublicacoesDoEncontro>((Ref referencia) {
  return RepositorioDePublicacoesDoEncontro(
    referencia.watch(provedorDoClienteHttpAutenticado),
  );
});

class RepositorioDePublicacoesDoEncontro
    implements IRepositorioDePublicacoesDoEncontro {
  RepositorioDePublicacoesDoEncontro(this._clienteHttp);

  final Dio _clienteHttp;

  @override
  Future<List<PublicacaoDoEncontro>> listeAsync(
    String identificadorDoEncontro,
  ) async {
    try {
      Response<dynamic> resposta = await _clienteHttp.get<dynamic>(
        '/api/encontros/$identificadorDoEncontro/publicacoes',
      );
      List<dynamic> dados = resposta.data as List<dynamic>;

      return dados
          .map(
            (dynamic item) => PublicacaoDoEncontro.deJson(
              Map<String, dynamic>.from(item as Map<dynamic, dynamic>),
            ),
          )
          .toList();
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Não foi possível carregar os momentos deste encontro.',
      );
    }
  }

  @override
  Future<PublicacaoDoEncontro> publiqueAsync({
    required String identificadorDoEncontro,
    required String texto,
  }) async {
    try {
      Response<dynamic> resposta = await _clienteHttp.post<dynamic>(
        '/api/encontros/$identificadorDoEncontro/publicacoes',
        data: <String, String>{'texto': texto},
      );
      Map<String, dynamic> dados =
          Map<String, dynamic>.from(resposta.data as Map<dynamic, dynamic>);

      return PublicacaoDoEncontro.deJson(dados);
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Não foi possível publicar este momento.',
      );
    }
  }

  ExcecaoDaApi _convertaExcecao(DioException excecao, String mensagemPadrao) {
    int? codigoHttp = excecao.response?.statusCode;
    dynamic corpo = excecao.response?.data;
    String? mensagem;

    if (corpo is Map<dynamic, dynamic> && corpo['mensagem'] is String) {
      mensagem = corpo['mensagem'] as String;
    }

    return ExcecaoDaApi(
      codigoHttp: codigoHttp,
      mensagem: mensagem ??
          (codigoHttp == null
              ? 'Não foi possível acessar o servidor.'
              : mensagemPadrao),
    );
  }
}
