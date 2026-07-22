import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/cliente_http_autenticado.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/encontro_resumo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/convite_do_encontro_resumo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/usuario_atual.dart';

abstract interface class IRepositorioDaPaginaInicial {
  Future<UsuarioAtual> obtenhaUsuarioAtualAsync();

  Future<List<EncontroResumo>> listeProximosEncontrosAsync();
}

abstract interface class IRepositorioDeConvitesDaPaginaInicial {
  Future<List<ConviteDoEncontroResumo>> listeConvitesPendentesAsync();
}

final provedorDoRepositorioDaPaginaInicial =
    Provider<IRepositorioDaPaginaInicial>((Ref referencia) {
  return RepositorioDaPaginaInicial(
    referencia.watch(provedorDoClienteHttpAutenticado),
  );
});

class RepositorioDaPaginaInicial
    implements
        IRepositorioDaPaginaInicial,
        IRepositorioDeConvitesDaPaginaInicial {
  RepositorioDaPaginaInicial(this._clienteHttp);

  final Dio _clienteHttp;

  @override
  Future<UsuarioAtual> obtenhaUsuarioAtualAsync() async {
    try {
      Response<dynamic> resposta =
          await _clienteHttp.get<dynamic>('/api/usuarios/eu');
      Map<String, dynamic> dados =
          Map<String, dynamic>.from(resposta.data as Map<dynamic, dynamic>);

      return UsuarioAtual.deJson(dados);
    } on DioException catch (excecao) {
      throw _convertaExcecao(excecao);
    }
  }

  @override
  Future<List<EncontroResumo>> listeProximosEncontrosAsync() async {
    try {
      Response<dynamic> resposta =
          await _clienteHttp.get<dynamic>('/api/encontros/proximos');
      List<dynamic> dados = resposta.data as List<dynamic>;

      return dados
          .map(
            (dynamic item) => EncontroResumo.deJson(
              Map<String, dynamic>.from(item as Map<dynamic, dynamic>),
            ),
          )
          .toList();
    } on DioException catch (excecao) {
      throw _convertaExcecao(excecao);
    }
  }

  @override
  Future<List<ConviteDoEncontroResumo>> listeConvitesPendentesAsync() async {
    try {
      Response<dynamic> resposta =
          await _clienteHttp.get<dynamic>('/api/encontros/convites');
      List<dynamic> dados = resposta.data as List<dynamic>;

      return dados
          .map(
            (dynamic item) => ConviteDoEncontroResumo.deJson(
              Map<String, dynamic>.from(item as Map<dynamic, dynamic>),
            ),
          )
          .where(
            (ConviteDoEncontroResumo convite) =>
                convite.situacao.toLowerCase() == 'convidado',
          )
          .toList();
    } on DioException catch (excecao) {
      throw _convertaExcecao(excecao);
    }
  }

  ExcecaoDaApi _convertaExcecao(DioException excecao) {
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
              : 'Não foi possível carregar seus encontros.'),
    );
  }
}
