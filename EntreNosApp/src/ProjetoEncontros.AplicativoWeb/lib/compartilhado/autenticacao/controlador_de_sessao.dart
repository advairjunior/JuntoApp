import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/estado_da_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/repositorio_de_autenticacao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/resposta_de_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';

final provedorDoControladorDeSessao =
    StateNotifierProvider<ControladorDeSessao, EstadoDaSessao>(
  (Ref referencia) {
    return ControladorDeSessao(
      referencia.watch(provedorDoRepositorioDeAutenticacao),
    );
  },
);

class ControladorDeSessao extends StateNotifier<EstadoDaSessao> {
  ControladorDeSessao(this._repositorioDeAutenticacao)
      : super(const EstadoDaSessao.restaurando()) {
    unawaited(restaureSessaoAsync());
  }

  final IRepositorioDeAutenticacao _repositorioDeAutenticacao;
  Future<bool>? _renovacaoEmAndamento;

  Future<void> restaureSessaoAsync() async {
    await renoveSessaoAsync();
  }

  Future<bool> renoveSessaoAsync() async {
    if (_renovacaoEmAndamento != null) {
      return _renovacaoEmAndamento!;
    }

    _renovacaoEmAndamento = _renoveSessaoInternamenteAsync();

    try {
      return await _renovacaoEmAndamento!;
    } finally {
      _renovacaoEmAndamento = null;
    }
  }

  Future<bool> autentiqueAsync({
    required String email,
    required String senha,
  }) async {
    state = state.copieCom(
      operacaoEstaEmAndamento: true,
      limpeMensagemDeErro: true,
    );

    try {
      RespostaDeSessao resposta =
          await _repositorioDeAutenticacao.autentiqueAsync(
        email: email,
        senha: senha,
      );
      _definaSessaoAutenticada(resposta);
      return true;
    } on ExcecaoDaApi catch (excecao) {
      state = const EstadoDaSessao(
        situacao: SituacaoDaSessao.naoAutenticada,
      ).copieCom(mensagemDeErro: excecao.mensagem);
      return false;
    }
  }

  Future<bool> cadastreAsync({
    required String nome,
    required String email,
    required String senha,
  }) async {
    state = state.copieCom(
      operacaoEstaEmAndamento: true,
      limpeMensagemDeErro: true,
    );

    try {
      await _repositorioDeAutenticacao.cadastreAsync(
        nome: nome,
        email: email,
        senha: senha,
      );
      state = const EstadoDaSessao(
        situacao: SituacaoDaSessao.naoAutenticada,
      );
      return true;
    } on ExcecaoDaApi catch (excecao) {
      state = const EstadoDaSessao(
        situacao: SituacaoDaSessao.naoAutenticada,
      ).copieCom(mensagemDeErro: excecao.mensagem);
      return false;
    }
  }

  Future<void> encerreSessaoAsync() async {
    try {
      await _repositorioDeAutenticacao.encerreSessaoAsync();
    } finally {
      state = const EstadoDaSessao(
        situacao: SituacaoDaSessao.naoAutenticada,
      );
    }
  }

  void limpeMensagemDeErro() {
    state = state.copieCom(limpeMensagemDeErro: true);
  }

  void _definaSessaoAutenticada(RespostaDeSessao resposta) {
    state = EstadoDaSessao(
      situacao: SituacaoDaSessao.autenticada,
      tokenDeAcesso: resposta.tokenDeAcesso,
      expiraEm: resposta.expiraEm,
    );
  }

  Future<bool> _renoveSessaoInternamenteAsync() async {
    try {
      RespostaDeSessao resposta =
          await _repositorioDeAutenticacao.renoveSessaoAsync();
      _definaSessaoAutenticada(resposta);
      return true;
    } on ExcecaoDaApi {
      state = const EstadoDaSessao(
        situacao: SituacaoDaSessao.naoAutenticada,
      );
      return false;
    }
  }
}
