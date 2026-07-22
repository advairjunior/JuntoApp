import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/cliente_http_autenticado.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/combinados/modelos/item_do_encontro.dart';

abstract interface class IRepositorioDeCombinados {
  Future<List<ItemDoEncontro>> listeAsync(String identificadorDoEncontro);

  Future<void> crieAsync({
    required String identificadorDoEncontro,
    required String descricao,
    String? identificadorDoResponsavel,
  });

  Future<void> editeAsync({
    required String identificadorDoEncontro,
    required String identificadorDoItem,
    required String descricao,
    String? identificadorDoResponsavel,
  });

  Future<void> removaAsync({
    required String identificadorDoEncontro,
    required String identificadorDoItem,
  });

  Future<void> altereSituacaoAsync({
    required String identificadorDoEncontro,
    required String identificadorDoItem,
    required bool resolva,
  });
}

final provedorDoRepositorioDeCombinados =
    Provider<IRepositorioDeCombinados>((Ref referencia) {
  return RepositorioDeCombinados(
    referencia.watch(provedorDoClienteHttpAutenticado),
  );
});

final provedorDosCombinados = FutureProvider.autoDispose
    .family<List<ItemDoEncontro>, String>((Ref referencia, String encontro) {
  return referencia.watch(provedorDoRepositorioDeCombinados).listeAsync(
        encontro,
      );
});

class RepositorioDeCombinados implements IRepositorioDeCombinados {
  RepositorioDeCombinados(this._clienteHttp);

  final Dio _clienteHttp;

  @override
  Future<List<ItemDoEncontro>> listeAsync(
    String identificadorDoEncontro,
  ) async {
    try {
      Response<dynamic> resposta = await _clienteHttp.get<dynamic>(
        '/api/encontros/$identificadorDoEncontro/itens',
      );
      List<dynamic> dados = resposta.data as List<dynamic>;

      return dados
          .map(
            (dynamic item) => ItemDoEncontro.deJson(
              Map<String, dynamic>.from(item as Map<dynamic, dynamic>),
            ),
          )
          .toList();
    } on DioException catch (excecao) {
      throw _convertaExcecao(
          excecao, 'Não foi possível carregar os combinados.');
    }
  }

  @override
  Future<void> crieAsync({
    required String identificadorDoEncontro,
    required String descricao,
    String? identificadorDoResponsavel,
  }) async {
    try {
      await _clienteHttp.post<dynamic>(
        '/api/encontros/$identificadorDoEncontro/itens',
        data: <String, dynamic>{
          'descricao': descricao,
          'identificadorDoUsuarioResponsavel': identificadorDoResponsavel,
        },
      );
    } on DioException catch (excecao) {
      throw _convertaExcecao(excecao, 'Não foi possível criar o combinado.');
    }
  }

  @override
  Future<void> editeAsync({
    required String identificadorDoEncontro,
    required String identificadorDoItem,
    required String descricao,
    String? identificadorDoResponsavel,
  }) async {
    try {
      await _clienteHttp.put<dynamic>(
        '/api/encontros/$identificadorDoEncontro/itens/$identificadorDoItem',
        data: <String, dynamic>{
          'descricao': descricao,
          'identificadorDoUsuarioResponsavel': identificadorDoResponsavel,
        },
      );
    } on DioException catch (excecao) {
      throw _convertaExcecao(excecao, 'Não foi possível editar o combinado.');
    }
  }

  @override
  Future<void> removaAsync({
    required String identificadorDoEncontro,
    required String identificadorDoItem,
  }) async {
    try {
      await _clienteHttp.delete<dynamic>(
        '/api/encontros/$identificadorDoEncontro/itens/$identificadorDoItem',
      );
    } on DioException catch (excecao) {
      throw _convertaExcecao(excecao, 'Não foi possível excluir o combinado.');
    }
  }

  @override
  Future<void> altereSituacaoAsync({
    required String identificadorDoEncontro,
    required String identificadorDoItem,
    required bool resolva,
  }) async {
    try {
      String acao = resolva ? 'resolver' : 'pendente';
      await _clienteHttp.post<dynamic>(
        '/api/encontros/$identificadorDoEncontro/itens/$identificadorDoItem/$acao',
      );
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Não foi possível atualizar o combinado.',
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
