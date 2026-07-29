import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/cliente_http_autenticado.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/convites_por_link/modelos/convite_por_link.dart';

abstract interface class IRepositorioDeConvitesPorLink {
  Future<ConvitePorLinkCriado> crieAsync(String identificadorDoEncontro);

  Future<void> revogueAsync(String identificadorDoEncontro);

  Future<ConvitePorLinkDetalhado> consulteAsync(String token);

  Future<AceiteDoConvitePorLink> aceiteAsync(String token);
}

final provedorDoRepositorioDeConvitesPorLink =
    Provider<IRepositorioDeConvitesPorLink>((Ref referencia) {
  return RepositorioDeConvitesPorLink(
    referencia.watch(provedorDoClienteHttpAutenticado),
  );
});

class RepositorioDeConvitesPorLink implements IRepositorioDeConvitesPorLink {
  RepositorioDeConvitesPorLink(this._clienteHttp);

  final Dio _clienteHttp;

  @override
  Future<ConvitePorLinkCriado> crieAsync(
    String identificadorDoEncontro,
  ) async {
    try {
      Response<dynamic> resposta = await _clienteHttp.post<dynamic>(
        '/api/encontros/$identificadorDoEncontro/convites-por-link',
      );

      return ConvitePorLinkCriado.deJson(_convertaMapa(resposta.data));
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Não foi possível criar o link do convite.',
      );
    }
  }

  @override
  Future<void> revogueAsync(String identificadorDoEncontro) async {
    try {
      await _clienteHttp.delete<dynamic>(
        '/api/encontros/$identificadorDoEncontro/convites-por-link',
      );
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Não foi possível desativar o link.',
      );
    }
  }

  @override
  Future<ConvitePorLinkDetalhado> consulteAsync(String token) async {
    try {
      Response<dynamic> resposta = await _clienteHttp.post<dynamic>(
        '/api/convites-de-encontro/consultar',
        data: <String, String>{'token': token},
      );

      return ConvitePorLinkDetalhado.deJson(_convertaMapa(resposta.data));
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Este convite não está mais disponível.',
      );
    }
  }

  @override
  Future<AceiteDoConvitePorLink> aceiteAsync(String token) async {
    try {
      Response<dynamic> resposta = await _clienteHttp.post<dynamic>(
        '/api/convites-de-encontro/aceitar',
        data: <String, String>{'token': token},
      );

      return AceiteDoConvitePorLink.deJson(_convertaMapa(resposta.data));
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Não foi possível entrar neste encontro.',
      );
    }
  }

  Map<String, dynamic> _convertaMapa(dynamic dados) {
    return Map<String, dynamic>.from(dados as Map<dynamic, dynamic>);
  }

  ExcecaoDaApi _convertaExcecao(
    DioException excecao,
    String mensagemPadrao,
  ) {
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
