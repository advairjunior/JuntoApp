import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/controlador_de_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/estado_da_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/comunicacao/cliente_http.dart';

final provedorDoClienteHttpAutenticado = Provider<Dio>((Ref referencia) {
  Dio cliente = crieClienteHttp();

  cliente.interceptors.add(
    InterceptorsWrapper(
      onRequest: (
        RequestOptions requisicao,
        RequestInterceptorHandler manipulador,
      ) {
        EstadoDaSessao sessao = referencia.read(provedorDoControladorDeSessao);

        if (sessao.tokenDeAcesso != null) {
          requisicao.headers['Authorization'] =
              'Bearer ${sessao.tokenDeAcesso}';
        }

        manipulador.next(requisicao);
      },
      onError: (
        DioException excecao,
        ErrorInterceptorHandler manipulador,
      ) async {
        bool usuarioNaoEstaAutenticado = excecao.response?.statusCode == 401;
        bool requisicaoJaFoiRepetida =
            excecao.requestOptions.extra['sessao_renovada'] == true;

        if (!usuarioNaoEstaAutenticado || requisicaoJaFoiRepetida) {
          manipulador.next(excecao);
          return;
        }

        bool sessaoFoiRenovada = await referencia
            .read(provedorDoControladorDeSessao.notifier)
            .renoveSessaoAsync();

        if (!sessaoFoiRenovada) {
          manipulador.next(excecao);
          return;
        }

        EstadoDaSessao sessao = referencia.read(provedorDoControladorDeSessao);
        RequestOptions novaRequisicao = excecao.requestOptions;
        Object? Function()? recrieCorpoDaRequisicao = novaRequisicao
            .extra['recrie_corpo_da_requisicao'] as Object? Function()?;

        if (novaRequisicao.data is FormData &&
            recrieCorpoDaRequisicao == null) {
          manipulador.next(excecao);
          return;
        }

        if (recrieCorpoDaRequisicao != null) {
          novaRequisicao.data = recrieCorpoDaRequisicao();
        }

        novaRequisicao.extra['sessao_renovada'] = true;
        novaRequisicao.headers['Authorization'] =
            'Bearer ${sessao.tokenDeAcesso}';

        try {
          Response<dynamic> resposta = await cliente.fetch<dynamic>(
            novaRequisicao,
          );
          manipulador.resolve(resposta);
        } on DioException catch (novaExcecao) {
          manipulador.next(novaExcecao);
        }
      },
    ),
  );

  return cliente;
});
