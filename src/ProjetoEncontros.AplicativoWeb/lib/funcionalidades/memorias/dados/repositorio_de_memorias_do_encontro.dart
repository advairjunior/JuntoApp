import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/cliente_http_autenticado.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/identificador_da_operacao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/memoria_do_encontro.dart';

abstract interface class IRepositorioDeMemoriasDoEncontro {
  Future<List<MemoriaDoEncontro>> listeAsync(String identificadorDoEncontro);

  Future<MemoriaDoEncontro> publiqueImagemAsync({
    required String identificadorDoEncontro,
    required String nomeDoArquivo,
    required String tipoDeConteudo,
    required Uint8List conteudo,
    String? legenda,
  });

  Future<void> removaAsync({
    required String identificadorDoEncontro,
    required String identificadorDaMemoria,
  });
}

final provedorDoRepositorioDeMemoriasDoEncontro =
    Provider<IRepositorioDeMemoriasDoEncontro>((Ref referencia) {
  return RepositorioDeMemoriasDoEncontro(
    referencia.watch(provedorDoClienteHttpAutenticado),
  );
});

final provedorDasMemoriasDoEncontro =
    FutureProvider.autoDispose.family<List<MemoriaDoEncontro>, String>(
  (Ref referencia, String identificadorDoEncontro) {
    return referencia
        .watch(provedorDoRepositorioDeMemoriasDoEncontro)
        .listeAsync(identificadorDoEncontro);
  },
);

class RepositorioDeMemoriasDoEncontro
    implements IRepositorioDeMemoriasDoEncontro {
  RepositorioDeMemoriasDoEncontro(this._clienteHttp);

  final Dio _clienteHttp;

  @override
  Future<List<MemoriaDoEncontro>> listeAsync(
    String identificadorDoEncontro,
  ) async {
    try {
      Response<dynamic> resposta = await _clienteHttp.get<dynamic>(
        '/api/encontros/$identificadorDoEncontro/memorias',
      );
      List<dynamic> dados = resposta.data as List<dynamic>;

      return dados
          .map(
            (dynamic item) => MemoriaDoEncontro.deJson(
              Map<String, dynamic>.from(item as Map<dynamic, dynamic>),
            ),
          )
          .toList()
        ..sort(
          (MemoriaDoEncontro primeira, MemoriaDoEncontro segunda) =>
              segunda.criadoEm.compareTo(primeira.criadoEm),
        );
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Não foi possível carregar as memórias deste encontro.',
      );
    }
  }

  @override
  Future<MemoriaDoEncontro> publiqueImagemAsync({
    required String identificadorDoEncontro,
    required String nomeDoArquivo,
    required String tipoDeConteudo,
    required Uint8List conteudo,
    String? legenda,
  }) async {
    String identificadorDaOperacao = crieIdentificadorDaOperacao();

    FormData crieFormulario() {
      return FormData.fromMap(<String, dynamic>{
        'arquivo': MultipartFile.fromBytes(
          conteudo,
          filename: nomeDoArquivo,
          contentType: DioMediaType.parse(tipoDeConteudo),
        ),
        if (legenda != null && legenda.trim().isNotEmpty)
          'legenda': legenda.trim(),
      });
    }

    try {
      Response<dynamic> resposta = await _clienteHttp.post<dynamic>(
        '/api/encontros/$identificadorDoEncontro/memorias',
        data: crieFormulario(),
        options: Options(
          headers: <String, dynamic>{
            'Idempotency-Key': identificadorDaOperacao,
          },
          extra: <String, dynamic>{
            'recrie_corpo_da_requisicao': crieFormulario,
          },
        ),
      );
      Map<String, dynamic> dados =
          Map<String, dynamic>.from(resposta.data as Map<dynamic, dynamic>);

      return MemoriaDoEncontro.deJson(dados);
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Não foi possível compartilhar esta foto.',
      );
    }
  }

  @override
  Future<void> removaAsync({
    required String identificadorDoEncontro,
    required String identificadorDaMemoria,
  }) async {
    try {
      await _clienteHttp.delete<dynamic>(
        '/api/encontros/$identificadorDoEncontro/memorias/$identificadorDaMemoria',
      );
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Não foi possível remover esta foto.',
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
