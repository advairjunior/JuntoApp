import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/cliente_http_autenticado.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/notificacoes/modelos/notificacao_do_usuario.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/notificacoes/modelos/preferencia_de_notificacao.dart';

abstract interface class IRepositorioDeNotificacoes {
  Future<ListaDeNotificacoes> listeAsync();

  Future<void> marqueComoLidaAsync(String identificadorDaNotificacao);

  Future<PreferenciaDeNotificacao> obtenhaPreferenciasAsync();

  Future<PreferenciaDeNotificacao> atualizePreferenciasAsync(
    PreferenciaDeNotificacao preferencias,
  );
}

final provedorDoRepositorioDeNotificacoes =
    Provider<IRepositorioDeNotificacoes>((Ref referencia) {
  return RepositorioDeNotificacoes(
    referencia.watch(provedorDoClienteHttpAutenticado),
  );
});

final provedorDaListaDeNotificacoes =
    FutureProvider.autoDispose<ListaDeNotificacoes>((Ref referencia) {
  return referencia.watch(provedorDoRepositorioDeNotificacoes).listeAsync();
});

final provedorDasPreferenciasDeNotificacao =
    FutureProvider.autoDispose<PreferenciaDeNotificacao>((Ref referencia) {
  return referencia
      .watch(provedorDoRepositorioDeNotificacoes)
      .obtenhaPreferenciasAsync();
});

class RepositorioDeNotificacoes implements IRepositorioDeNotificacoes {
  RepositorioDeNotificacoes(this._clienteHttp);

  final Dio _clienteHttp;

  @override
  Future<ListaDeNotificacoes> listeAsync() async {
    try {
      Response<dynamic> resposta =
          await _clienteHttp.get<dynamic>('/api/notificacoes/');
      Map<String, dynamic> dados =
          Map<String, dynamic>.from(resposta.data as Map<dynamic, dynamic>);

      return ListaDeNotificacoes.deJson(dados);
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Não foi possível carregar suas notificações.',
      );
    }
  }

  @override
  Future<void> marqueComoLidaAsync(String identificadorDaNotificacao) async {
    try {
      await _clienteHttp.post<dynamic>(
        '/api/notificacoes/$identificadorDaNotificacao/lida',
      );
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Não foi possível marcar a notificação como lida.',
      );
    }
  }

  @override
  Future<PreferenciaDeNotificacao> obtenhaPreferenciasAsync() async {
    try {
      Response<dynamic> resposta =
          await _clienteHttp.get<dynamic>('/api/notificacoes/preferencias');
      Map<String, dynamic> dados =
          Map<String, dynamic>.from(resposta.data as Map<dynamic, dynamic>);

      return PreferenciaDeNotificacao.deJson(dados);
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Não foi possível carregar suas preferências.',
      );
    }
  }

  @override
  Future<PreferenciaDeNotificacao> atualizePreferenciasAsync(
    PreferenciaDeNotificacao preferencias,
  ) async {
    try {
      Response<dynamic> resposta = await _clienteHttp.put<dynamic>(
        '/api/notificacoes/preferencias',
        data: preferencias.paraJson(),
      );
      Map<String, dynamic> dados =
          Map<String, dynamic>.from(resposta.data as Map<dynamic, dynamic>);

      return PreferenciaDeNotificacao.deJson(dados);
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Não foi possível salvar suas preferências.',
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
