import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/cliente_http_autenticado.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/identificador_da_operacao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_criado.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_detalhado.dart';

abstract interface class IRepositorioDeEncontros {
  Future<EncontroCriado> crieEncontroAsync({
    required String titulo,
    required DateTime inicioEm,
    String? descricao,
    String? local,
    double? latitude,
    double? longitude,
    String? tipo,
  });

  Future<EncontroDetalhado> obtenhaEncontroAsync(String identificador);

  Future<void> editeEncontroAsync({
    required String identificador,
    required String titulo,
    required DateTime inicioEm,
    String? descricao,
    String? local,
    double? latitude,
    double? longitude,
    String? tipo,
  });

  Future<void> canceleEncontroAsync(String identificador);

  Future<String?> altereImagemDeCapaAsync({
    required String identificador,
    required String nomeDoArquivo,
    required String tipoDeConteudo,
    required Uint8List conteudo,
  });

  Future<void> removaImagemDeCapaAsync(String identificador);

  Future<void> convidePessoaAsync({
    required String identificador,
    required String email,
  });

  Future<void> convidePessoaFrequenteAsync({
    required String identificador,
    required String identificadorDoUsuario,
  });

  Future<String> respondaPresencaAsync({
    required String identificador,
    required String situacao,
  });
}

abstract interface class IRepositorioDeRealizacaoDeEncontro {
  Future<void> marqueEncontroComoRealizadoAsync(String identificador);
}

final provedorDoRepositorioDeEncontros =
    Provider<IRepositorioDeEncontros>((Ref referencia) {
  return RepositorioDeEncontros(
    referencia.watch(provedorDoClienteHttpAutenticado),
  );
});

class RepositorioDeEncontros
    implements IRepositorioDeEncontros, IRepositorioDeRealizacaoDeEncontro {
  RepositorioDeEncontros(this._clienteHttp);

  final Dio _clienteHttp;

  @override
  Future<EncontroCriado> crieEncontroAsync({
    required String titulo,
    required DateTime inicioEm,
    String? descricao,
    String? local,
    double? latitude,
    double? longitude,
    String? tipo,
  }) async {
    try {
      Response<dynamic> resposta = await _clienteHttp.post<dynamic>(
        '/api/encontros',
        data: <String, dynamic>{
          'titulo': titulo,
          'descricao': descricao,
          'local': local,
          'localizacao': _crieLocalizacao(
            local: local,
            latitude: latitude,
            longitude: longitude,
          ),
          'inicioEm': inicioEm.toUtc().toIso8601String(),
          'tipo': tipo,
        },
      );
      Map<String, dynamic> dados =
          Map<String, dynamic>.from(resposta.data as Map<dynamic, dynamic>);

      return EncontroCriado.deJson(dados);
    } on DioException catch (excecao) {
      throw _convertaExcecao(excecao);
    }
  }

  @override
  Future<EncontroDetalhado> obtenhaEncontroAsync(String identificador) async {
    try {
      Response<dynamic> resposta =
          await _clienteHttp.get<dynamic>('/api/encontros/$identificador');
      Map<String, dynamic> dados =
          Map<String, dynamic>.from(resposta.data as Map<dynamic, dynamic>);

      return EncontroDetalhado.deJson(dados);
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        mensagemPadrao: 'Não foi possível carregar o encontro.',
      );
    }
  }

  @override
  Future<void> editeEncontroAsync({
    required String identificador,
    required String titulo,
    required DateTime inicioEm,
    String? descricao,
    String? local,
    double? latitude,
    double? longitude,
    String? tipo,
  }) async {
    try {
      await _clienteHttp.put<dynamic>(
        '/api/encontros/$identificador',
        data: <String, dynamic>{
          'titulo': titulo,
          'descricao': descricao,
          'local': local,
          'localizacao': _crieLocalizacao(
            local: local,
            latitude: latitude,
            longitude: longitude,
          ),
          'inicioEm': inicioEm.toUtc().toIso8601String(),
          'tipo': tipo,
        },
      );
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        mensagemPadrao: 'Não foi possível editar o encontro.',
      );
    }
  }

  @override
  Future<void> canceleEncontroAsync(String identificador) async {
    try {
      await _clienteHttp.post<dynamic>(
        '/api/encontros/$identificador/cancelar',
      );
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        mensagemPadrao: 'Não foi possível cancelar o encontro.',
      );
    }
  }

  @override
  Future<void> marqueEncontroComoRealizadoAsync(String identificador) async {
    try {
      await _clienteHttp.post<dynamic>(
        '/api/encontros/$identificador/realizar',
      );
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        mensagemPadrao: 'Não foi possível marcar o encontro como realizado.',
      );
    }
  }

  @override
  Future<String?> altereImagemDeCapaAsync({
    required String identificador,
    required String nomeDoArquivo,
    required String tipoDeConteudo,
    required Uint8List conteudo,
  }) async {
    try {
      String identificadorDaOperacao = crieIdentificadorDaOperacao();

      FormData crieFormulario() {
        return FormData.fromMap(<String, dynamic>{
          'arquivo': MultipartFile.fromBytes(
            conteudo,
            filename: nomeDoArquivo,
            contentType: DioMediaType.parse(tipoDeConteudo),
          ),
        });
      }

      Response<dynamic> resposta = await _clienteHttp.put<dynamic>(
        '/api/encontros/$identificador/imagem-capa',
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

      return dados['urlDaImagemDeCapa'] as String?;
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        mensagemPadrao: 'Não foi possível alterar a imagem do encontro.',
      );
    }
  }

  @override
  Future<void> removaImagemDeCapaAsync(String identificador) async {
    try {
      await _clienteHttp.delete<dynamic>(
        '/api/encontros/$identificador/imagem-capa',
      );
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        mensagemPadrao: 'Não foi possível remover a imagem do encontro.',
      );
    }
  }

  @override
  Future<void> convidePessoaAsync({
    required String identificador,
    required String email,
  }) async {
    try {
      await _clienteHttp.post<dynamic>(
        '/api/encontros/$identificador/convites',
        data: <String, String>{'email': email},
      );
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        mensagemPadrao: 'Não foi possível enviar o convite.',
      );
    }
  }

  @override
  Future<void> convidePessoaFrequenteAsync({
    required String identificador,
    required String identificadorDoUsuario,
  }) async {
    try {
      await _clienteHttp.post<dynamic>(
        '/api/encontros/$identificador/convites/usuarios',
        data: <String, String>{
          'identificadorDoUsuarioConvidado': identificadorDoUsuario,
        },
      );
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        mensagemPadrao: 'Não foi possível enviar o convite.',
      );
    }
  }

  @override
  Future<String> respondaPresencaAsync({
    required String identificador,
    required String situacao,
  }) async {
    try {
      Response<dynamic> resposta = await _clienteHttp.put<dynamic>(
        '/api/encontros/$identificador/presenca',
        data: <String, String>{'situacao': situacao},
      );
      Map<String, dynamic> dados =
          Map<String, dynamic>.from(resposta.data as Map<dynamic, dynamic>);

      return dados['situacao'] as String;
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        mensagemPadrao: 'Não foi possível atualizar sua presença.',
      );
    }
  }

  ExcecaoDaApi _convertaExcecao(
    DioException excecao, {
    String mensagemPadrao = 'Não foi possível criar o encontro.',
  }) {
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

  Map<String, dynamic>? _crieLocalizacao({
    required String? local,
    required double? latitude,
    required double? longitude,
  }) {
    if (local == null || local.trim().isEmpty) {
      return null;
    }

    return <String, dynamic>{
      'descricao': local.trim(),
      'latitude': latitude,
      'longitude': longitude,
    };
  }
}
