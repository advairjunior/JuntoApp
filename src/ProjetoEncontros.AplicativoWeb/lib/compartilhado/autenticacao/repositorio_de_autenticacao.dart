import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/resposta_de_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/cliente_http.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';

abstract interface class IRepositorioDeAutenticacao {
  Future<RespostaDeSessao> autentiqueAsync({
    required String numeroDeCelular,
    required String pin,
  });

  Future<void> cadastreAsync({
    required String nome,
    required String numeroDeCelular,
    required String pin,
  });

  Future<RespostaDeSessao> renoveSessaoAsync();

  Future<void> encerreSessaoAsync();
}

final provedorDoRepositorioDeAutenticacao =
    Provider<IRepositorioDeAutenticacao>((Ref referencia) {
  return RepositorioDeAutenticacao(referencia.watch(provedorDoClienteHttp));
});

class RepositorioDeAutenticacao implements IRepositorioDeAutenticacao {
  RepositorioDeAutenticacao(this._clienteHttp);

  final Dio _clienteHttp;

  @override
  Future<RespostaDeSessao> autentiqueAsync({
    required String numeroDeCelular,
    required String pin,
  }) async {
    try {
      Response<dynamic> resposta = await _clienteHttp.post<dynamic>(
        '/api/autenticacao/navegador/login',
        data: <String, String>{
          'numeroDeCelular': numeroDeCelular,
          'pin': pin,
        },
      );

      return RespostaDeSessao.deJson(
        Map<String, dynamic>.from(resposta.data as Map<dynamic, dynamic>),
      );
    } on DioException catch (excecao) {
      throw _convertaExcecao(excecao);
    }
  }

  @override
  Future<void> cadastreAsync({
    required String nome,
    required String numeroDeCelular,
    required String pin,
  }) async {
    try {
      await _clienteHttp.post<dynamic>(
        '/api/autenticacao/cadastro',
        data: <String, String>{
          'nome': nome,
          'numeroDeCelular': numeroDeCelular,
          'pin': pin,
        },
      );
    } on DioException catch (excecao) {
      throw _convertaExcecao(excecao);
    }
  }

  @override
  Future<RespostaDeSessao> renoveSessaoAsync() async {
    try {
      Response<dynamic> resposta = await _clienteHttp.post<dynamic>(
        '/api/autenticacao/navegador/renovar-sessao',
      );

      return RespostaDeSessao.deJson(
        Map<String, dynamic>.from(resposta.data as Map<dynamic, dynamic>),
      );
    } on DioException catch (excecao) {
      throw _convertaExcecao(excecao);
    }
  }

  @override
  Future<void> encerreSessaoAsync() async {
    try {
      await _clienteHttp.post<dynamic>('/api/autenticacao/navegador/sair');
    } on DioException catch (excecao) {
      throw _convertaExcecao(excecao);
    }
  }

  ExcecaoDaApi _convertaExcecao(DioException excecao) {
    int? codigoHttp = excecao.response?.statusCode;
    dynamic corpo = excecao.response?.data;
    String? mensagem;

    if (corpo is Map<dynamic, dynamic>) {
      dynamic mensagemRecebida = corpo['mensagem'];

      if (mensagemRecebida is String) {
        mensagem = mensagemRecebida;
      }
    }

    return ExcecaoDaApi(
      codigoHttp: codigoHttp,
      mensagem: mensagem ??
          (codigoHttp == null
              ? 'Não foi possível acessar o servidor.'
              : 'Não foi possível concluir a operação.'),
    );
  }
}
