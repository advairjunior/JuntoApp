import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/cliente_http_autenticado.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/pessoas_frequentes/modelos/pessoa_frequente.dart';

abstract interface class IRepositorioDePessoasFrequentes {
  Future<List<PessoaFrequente>> listeAsync();
}

final provedorDoRepositorioDePessoasFrequentes =
    Provider<IRepositorioDePessoasFrequentes>((Ref referencia) {
  return RepositorioDePessoasFrequentes(
    referencia.watch(provedorDoClienteHttpAutenticado),
  );
});

final provedorDasPessoasFrequentes =
    FutureProvider.autoDispose<List<PessoaFrequente>>((Ref referencia) {
  return referencia
      .watch(provedorDoRepositorioDePessoasFrequentes)
      .listeAsync();
});

class RepositorioDePessoasFrequentes
    implements IRepositorioDePessoasFrequentes {
  RepositorioDePessoasFrequentes(this._clienteHttp);

  final Dio _clienteHttp;

  @override
  Future<List<PessoaFrequente>> listeAsync() async {
    try {
      Response<dynamic> resposta =
          await _clienteHttp.get<dynamic>('/api/pessoas-frequentes');
      List<dynamic> dados = resposta.data as List<dynamic>;

      return dados
          .map(
            (dynamic item) => PessoaFrequente.deJson(
              Map<String, dynamic>.from(item as Map<dynamic, dynamic>),
            ),
          )
          .toList();
    } on DioException catch (excecao) {
      int? codigoHttp = excecao.response?.statusCode;

      throw ExcecaoDaApi(
        codigoHttp: codigoHttp,
        mensagem: codigoHttp == null
            ? 'Não foi possível acessar o servidor.'
            : 'Não foi possível carregar as pessoas frequentes.',
      );
    }
  }
}
