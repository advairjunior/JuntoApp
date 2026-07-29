import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/cliente_http_autenticado.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/identificador_da_operacao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/usuario_atual.dart';

abstract interface class IRepositorioDoPerfil {
  Future<UsuarioAtual> altereFotoAsync({
    required String nomeDoArquivo,
    required String tipoDeConteudo,
    required Uint8List conteudo,
  });

  Future<UsuarioAtual> removaFotoAsync();
}

abstract interface class IRepositorioDeEdicaoDoPerfil {
  Future<UsuarioAtual> altereNomeAsync(String nome);
}

extension EdicaoDoPerfil on IRepositorioDoPerfil {
  Future<UsuarioAtual> altereNomeAsync(String nome) {
    IRepositorioDoPerfil repositorio = this;

    if (repositorio is IRepositorioDeEdicaoDoPerfil) {
      return (repositorio as IRepositorioDeEdicaoDoPerfil)
          .altereNomeAsync(nome);
    }

    throw StateError('O repositório não permite editar o nome.');
  }
}

final provedorDoRepositorioDoPerfil = Provider<IRepositorioDoPerfil>(
  (Ref referencia) {
    return RepositorioDoPerfil(
      referencia.watch(provedorDoClienteHttpAutenticado),
    );
  },
);

class RepositorioDoPerfil
    implements IRepositorioDoPerfil, IRepositorioDeEdicaoDoPerfil {
  RepositorioDoPerfil(this._clienteHttp);

  final Dio _clienteHttp;

  @override
  Future<UsuarioAtual> altereNomeAsync(String nome) async {
    try {
      Response<dynamic> resposta = await _clienteHttp.put<dynamic>(
        '/api/usuarios/eu',
        data: <String, dynamic>{'nome': nome.trim()},
      );

      return _convertaUsuario(resposta.data);
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Não foi possível alterar seu nome.',
      );
    }
  }

  @override
  Future<UsuarioAtual> altereFotoAsync({
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
        '/api/usuarios/eu/foto',
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

      return _convertaUsuario(resposta.data);
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Não foi possível alterar sua foto de perfil.',
      );
    }
  }

  @override
  Future<UsuarioAtual> removaFotoAsync() async {
    try {
      Response<dynamic> resposta = await _clienteHttp.delete<dynamic>(
        '/api/usuarios/eu/foto',
      );

      return _convertaUsuario(resposta.data);
    } on DioException catch (excecao) {
      throw _convertaExcecao(
        excecao,
        'Não foi possível remover sua foto de perfil.',
      );
    }
  }

  UsuarioAtual _convertaUsuario(dynamic dados) {
    return UsuarioAtual.deJson(
      Map<String, dynamic>.from(dados as Map<dynamic, dynamic>),
    );
  }

  ExcecaoDaApi _convertaExcecao(
    DioException excecao,
    String mensagemPadrao,
  ) {
    dynamic corpo = excecao.response?.data;
    String? mensagem;

    if (corpo is Map<dynamic, dynamic> && corpo['mensagem'] is String) {
      mensagem = corpo['mensagem'] as String;
    }

    return ExcecaoDaApi(
      codigoHttp: excecao.response?.statusCode,
      mensagem: mensagem ?? mensagemPadrao,
    );
  }
}
