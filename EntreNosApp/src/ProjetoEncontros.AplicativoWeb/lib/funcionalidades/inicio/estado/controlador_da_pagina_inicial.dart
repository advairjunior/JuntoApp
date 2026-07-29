import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/dados/repositorio_de_encontros.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/dados/repositorio_da_pagina_inicial.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/estado/estado_da_pagina_inicial.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/encontro_resumo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/convite_do_encontro_resumo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/usuario_atual.dart';

final provedorDoControladorDaPaginaInicial = StateNotifierProvider.autoDispose<
    ControladorDaPaginaInicial, EstadoDaPaginaInicial>((Ref referencia) {
  return ControladorDaPaginaInicial(
    referencia.watch(provedorDoRepositorioDaPaginaInicial),
    referencia.watch(provedorDoRepositorioDeEncontros),
  );
});

class ControladorDaPaginaInicial extends StateNotifier<EstadoDaPaginaInicial> {
  ControladorDaPaginaInicial(
    this._repositorio, [
    this._repositorioDeEncontros,
  ]) : super(const EstadoDaPaginaInicial.carregando()) {
    unawaited(carregueAsync());
  }

  final IRepositorioDaPaginaInicial _repositorio;
  final IRepositorioDeEncontros? _repositorioDeEncontros;

  Future<void> carregueAsync() async {
    state = const EstadoDaPaginaInicial.carregando();

    try {
      Future<List<ConviteDoEncontroResumo>> futuroDosConvites =
          _repositorio is IRepositorioDeConvitesDaPaginaInicial
              ? (_repositorio as IRepositorioDeConvitesDaPaginaInicial)
                  .listeConvitesPendentesAsync()
              : Future<List<ConviteDoEncontroResumo>>.value(
                  <ConviteDoEncontroResumo>[],
                );
      List<dynamic> respostas = await Future.wait<dynamic>(<Future<dynamic>>[
        _repositorio.obtenhaUsuarioAtualAsync(),
        _repositorio.listeProximosEncontrosAsync(),
        futuroDosConvites,
      ]);
      UsuarioAtual usuarioAtual = respostas[0] as UsuarioAtual;
      List<EncontroResumo> encontros = respostas[1] as List<EncontroResumo>;
      List<ConviteDoEncontroResumo> convitesPendentes =
          respostas[2] as List<ConviteDoEncontroResumo>;

      state = EstadoDaPaginaInicial(
        situacao: SituacaoDaPaginaInicial.carregada,
        usuarioAtual: usuarioAtual,
        encontros: encontros,
        convitesPendentes: convitesPendentes,
      );
    } on ExcecaoDaApi catch (excecao) {
      state = EstadoDaPaginaInicial(
        situacao: SituacaoDaPaginaInicial.falhou,
        mensagemDeErro: excecao.mensagem,
      );
    } catch (_) {
      state = const EstadoDaPaginaInicial(
        situacao: SituacaoDaPaginaInicial.falhou,
        mensagemDeErro: 'Não foi possível carregar a página inicial.',
      );
    }
  }

  Future<bool> respondaConviteAsync({
    required String identificadorDoEncontro,
    required String situacao,
  }) async {
    if (_repositorioDeEncontros == null ||
        state.identificadorDoConviteEmAtualizacao != null) {
      return false;
    }

    EstadoDaPaginaInicial estadoAtual = state;
    state = EstadoDaPaginaInicial(
      situacao: SituacaoDaPaginaInicial.carregada,
      usuarioAtual: estadoAtual.usuarioAtual,
      encontros: estadoAtual.encontros,
      convitesPendentes: estadoAtual.convitesPendentes,
      identificadorDoConviteEmAtualizacao: identificadorDoEncontro,
    );

    try {
      await _repositorioDeEncontros.respondaPresencaAsync(
        identificador: identificadorDoEncontro,
        situacao: situacao,
      );
      await carregueAsync();
      return true;
    } on ExcecaoDaApi catch (excecao) {
      state = EstadoDaPaginaInicial(
        situacao: SituacaoDaPaginaInicial.carregada,
        usuarioAtual: estadoAtual.usuarioAtual,
        encontros: estadoAtual.encontros,
        convitesPendentes: estadoAtual.convitesPendentes,
        mensagemDeErro: excecao.mensagem,
      );
      return false;
    }
  }
}
