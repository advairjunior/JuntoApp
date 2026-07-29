import 'dart:async';
import 'dart:typed_data';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/dados/repositorio_de_encontros.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/estado/estado_do_detalhe_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_detalhado.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/participante_do_encontro.dart';

final provedorDoControladorDoDetalheDoEncontro = StateNotifierProvider
    .autoDispose
    .family<ControladorDoDetalheDoEncontro, EstadoDoDetalheDoEncontro, String>(
        (Ref referencia, String identificador) {
  return ControladorDoDetalheDoEncontro(
    identificador,
    referencia.watch(provedorDoRepositorioDeEncontros),
  );
});

class ControladorDoDetalheDoEncontro
    extends StateNotifier<EstadoDoDetalheDoEncontro> {
  ControladorDoDetalheDoEncontro(this._identificador, this._repositorio)
      : super(const EstadoDoDetalheDoEncontro.carregando()) {
    unawaited(carregueAsync());
  }

  final String _identificador;
  final IRepositorioDeEncontros _repositorio;

  Future<void> carregueAsync() async {
    state = const EstadoDoDetalheDoEncontro.carregando();

    try {
      EncontroDetalhado encontro =
          await _repositorio.obtenhaEncontroAsync(_identificador);
      state = EstadoDoDetalheDoEncontro(
        situacao: SituacaoDoDetalheDoEncontro.carregado,
        encontro: encontro,
      );
    } on ExcecaoDaApi catch (excecao) {
      state = EstadoDoDetalheDoEncontro(
        situacao: SituacaoDoDetalheDoEncontro.falhou,
        mensagemDeErro: excecao.mensagem,
      );
    }
  }

  Future<bool> respondaPresencaAsync(String situacao) async {
    EncontroDetalhado? encontroAtual = state.encontro;

    if (encontroAtual == null || state.estaAtualizandoPresenca) {
      return false;
    }

    state = EstadoDoDetalheDoEncontro(
      situacao: SituacaoDoDetalheDoEncontro.carregado,
      encontro: encontroAtual,
      estaAtualizandoPresenca: true,
    );

    try {
      String situacaoAtualizada = await _repositorio.respondaPresencaAsync(
        identificador: _identificador,
        situacao: situacao,
      );
      List<ParticipanteDoEncontro> participantesAtualizados = encontroAtual
          .participantes
          .map(
            (ParticipanteDoEncontro participante) => participante.usuarioAtual
                ? ParticipanteDoEncontro(
                    identificadorDoUsuario: participante.identificadorDoUsuario,
                    nome: participante.nome,
                    urlDaFotoDePerfil: participante.urlDaFotoDePerfil,
                    papel: participante.papel,
                    situacao: situacaoAtualizada,
                    usuarioAtual: true,
                  )
                : participante,
          )
          .toList();
      EncontroDetalhado encontroAtualizado =
          encontroAtual.copieComParticipantes(participantesAtualizados);

      state = EstadoDoDetalheDoEncontro(
        situacao: SituacaoDoDetalheDoEncontro.carregado,
        encontro: encontroAtualizado,
      );
      return true;
    } on ExcecaoDaApi catch (excecao) {
      state = EstadoDoDetalheDoEncontro(
        situacao: SituacaoDoDetalheDoEncontro.carregado,
        encontro: encontroAtual,
        mensagemDeErro: excecao.mensagem,
      );
      return false;
    }
  }

  Future<bool> canceleEncontroAsync() async {
    EncontroDetalhado? encontroAtual = state.encontro;

    if (encontroAtual == null || state.estaExecutandoAcaoDoOrganizador) {
      return false;
    }

    state = EstadoDoDetalheDoEncontro(
      situacao: SituacaoDoDetalheDoEncontro.carregado,
      encontro: encontroAtual,
      estaExecutandoAcaoDoOrganizador: true,
    );

    try {
      await _repositorio.canceleEncontroAsync(_identificador);
      await carregueAsync();
      return true;
    } on ExcecaoDaApi catch (excecao) {
      state = EstadoDoDetalheDoEncontro(
        situacao: SituacaoDoDetalheDoEncontro.carregado,
        encontro: encontroAtual,
        mensagemDeErro: excecao.mensagem,
      );
      return false;
    }
  }

  Future<bool> marqueEncontroComoRealizadoAsync() async {
    if (_repositorio is! IRepositorioDeRealizacaoDeEncontro) {
      return false;
    }

    IRepositorioDeRealizacaoDeEncontro repositorioDeRealizacao =
        _repositorio as IRepositorioDeRealizacaoDeEncontro;

    return _executeAcaoDoOrganizadorAsync(
      acao: () => repositorioDeRealizacao.marqueEncontroComoRealizadoAsync(
        _identificador,
      ),
      mensagemDeSucesso: 'Encontro marcado como realizado.',
    );
  }

  Future<bool> altereImagemDeCapaAsync({
    required String nomeDoArquivo,
    required String tipoDeConteudo,
    required Uint8List conteudo,
  }) async {
    return _executeAcaoDoOrganizadorAsync(
      acao: () async {
        await _repositorio.altereImagemDeCapaAsync(
          identificador: _identificador,
          nomeDoArquivo: nomeDoArquivo,
          tipoDeConteudo: tipoDeConteudo,
          conteudo: conteudo,
        );
      },
      mensagemDeSucesso: 'Imagem do encontro atualizada.',
    );
  }

  Future<bool> convidePessoaAsync(String email) async {
    return _executeAcaoDoOrganizadorAsync(
      acao: () => _repositorio.convidePessoaAsync(
        identificador: _identificador,
        email: email,
      ),
      mensagemDeSucesso: 'Convite enviado.',
    );
  }

  Future<bool> alterePapelDoParticipanteAsync({
    required String identificadorDoUsuario,
    required String papel,
  }) async {
    String mensagemDeSucesso = papel.toLowerCase() == 'administrador'
        ? 'Administrador adicionado.'
        : 'Administrador removido.';

    return _executeAcaoDoOrganizadorAsync(
      acao: () => _repositorio.alterePapelDoParticipanteAsync(
        identificador: _identificador,
        identificadorDoUsuario: identificadorDoUsuario,
        papel: papel,
      ),
      mensagemDeSucesso: mensagemDeSucesso,
    );
  }

  Future<bool> removaImagemDeCapaAsync() async {
    return _executeAcaoDoOrganizadorAsync(
      acao: () => _repositorio.removaImagemDeCapaAsync(_identificador),
      mensagemDeSucesso: 'Imagem do encontro removida.',
    );
  }

  Future<bool> _executeAcaoDoOrganizadorAsync({
    required Future<void> Function() acao,
    required String mensagemDeSucesso,
  }) async {
    EncontroDetalhado? encontroAtual = state.encontro;

    if (encontroAtual == null || state.estaExecutandoAcaoDoOrganizador) {
      return false;
    }

    state = EstadoDoDetalheDoEncontro(
      situacao: SituacaoDoDetalheDoEncontro.carregado,
      encontro: encontroAtual,
      estaExecutandoAcaoDoOrganizador: true,
    );

    try {
      await acao();
    } on ExcecaoDaApi catch (excecao) {
      state = EstadoDoDetalheDoEncontro(
        situacao: SituacaoDoDetalheDoEncontro.carregado,
        encontro: encontroAtual,
        mensagemDeErro: excecao.mensagem,
      );
      return false;
    }

    try {
      EncontroDetalhado encontroAtualizado =
          await _repositorio.obtenhaEncontroAsync(_identificador);
      state = EstadoDoDetalheDoEncontro(
        situacao: SituacaoDoDetalheDoEncontro.carregado,
        encontro: encontroAtualizado,
        mensagemDeSucesso: mensagemDeSucesso,
      );
      return true;
    } on ExcecaoDaApi {
      state = EstadoDoDetalheDoEncontro(
        situacao: SituacaoDoDetalheDoEncontro.carregado,
        encontro: encontroAtual,
        mensagemDeSucesso: mensagemDeSucesso,
      );
      return true;
    }
  }
}
