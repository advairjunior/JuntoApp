import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/cliente_http_autenticado.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/item_da_linha_do_tempo.dart';

enum FiltroDaLinhaDoTempo {
  todos('todos', 'Todos'),
  esteMes('este-mes', 'Este mês'),
  ultimosTresMeses('ultimos-tres-meses', '3 meses'),
  esteAno('este-ano', 'Este ano'),
  realizados('realizados', 'Realizados'),
  comMemorias('com-memorias', 'Com memórias');

  const FiltroDaLinhaDoTempo(this.valor, this.rotulo);

  final String valor;
  final String rotulo;
}

abstract interface class IRepositorioDaLinhaDoTempo {
  Future<LinhaDoTempo> listeAsync(FiltroDaLinhaDoTempo filtro);
}

final provedorDoRepositorioDaLinhaDoTempo =
    Provider<IRepositorioDaLinhaDoTempo>((Ref referencia) {
  return RepositorioDaLinhaDoTempo(
    referencia.watch(provedorDoClienteHttpAutenticado),
  );
});

final provedorDaLinhaDoTempo =
    FutureProvider.autoDispose.family<LinhaDoTempo, FiltroDaLinhaDoTempo>(
  (Ref referencia, FiltroDaLinhaDoTempo filtro) {
    return referencia
        .watch(provedorDoRepositorioDaLinhaDoTempo)
        .listeAsync(filtro);
  },
);

class RepositorioDaLinhaDoTempo implements IRepositorioDaLinhaDoTempo {
  RepositorioDaLinhaDoTempo(this._clienteHttp);

  final Dio _clienteHttp;

  @override
  Future<LinhaDoTempo> listeAsync(FiltroDaLinhaDoTempo filtro) async {
    try {
      Response<dynamic> resposta = await _clienteHttp.get<dynamic>(
        '/api/linha-do-tempo',
        queryParameters: <String, dynamic>{'filtro': filtro.valor},
      );
      Map<String, dynamic> dados =
          Map<String, dynamic>.from(resposta.data as Map<dynamic, dynamic>);

      return LinhaDoTempo.deJson(dados);
    } on DioException catch (excecao) {
      int? codigoHttp = excecao.response?.statusCode;
      dynamic corpo = excecao.response?.data;
      String? mensagem;

      if (corpo is Map<dynamic, dynamic> && corpo['mensagem'] is String) {
        mensagem = corpo['mensagem'] as String;
      }

      throw ExcecaoDaApi(
        codigoHttp: codigoHttp,
        mensagem: mensagem ??
            (codigoHttp == null
                ? 'Não foi possível acessar o servidor.'
                : 'Não foi possível carregar suas memórias.'),
      );
    }
  }
}
