import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/dados/repositorio_de_encontros.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_detalhado.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/servicos/seletor_de_imagem.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/dados/repositorio_de_memorias_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/memoria_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/publicacoes/dados/repositorio_de_publicacoes_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/publicacoes/estado/estado_dos_momentos_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/publicacoes/modelos/publicacao_do_encontro.dart';

final provedorDoControladorDosMomentosDoEncontro =
    StateNotifierProvider.autoDispose.family<
        ControladorDosMomentosDoEncontro,
        EstadoDosMomentosDoEncontro,
        String>((Ref referencia, String identificadorDoEncontro) {
  return ControladorDosMomentosDoEncontro(
    identificadorDoEncontro,
    referencia.watch(provedorDoRepositorioDeEncontros),
    referencia.watch(provedorDoRepositorioDePublicacoesDoEncontro),
    referencia.watch(provedorDoRepositorioDeMemoriasDoEncontro),
  );
});

class ControladorDosMomentosDoEncontro
    extends StateNotifier<EstadoDosMomentosDoEncontro> {
  ControladorDosMomentosDoEncontro(
    this._identificadorDoEncontro,
    this._repositorioDeEncontros,
    this._repositorioDePublicacoes,
    this._repositorioDeMemorias,
  ) : super(const EstadoDosMomentosDoEncontro.carregando()) {
    unawaited(carregueAsync());
  }

  final String _identificadorDoEncontro;
  final IRepositorioDeEncontros _repositorioDeEncontros;
  final IRepositorioDePublicacoesDoEncontro _repositorioDePublicacoes;
  final IRepositorioDeMemoriasDoEncontro _repositorioDeMemorias;

  Future<void> carregueAsync() async {
    EncontroDetalhado? encontroAtual = state.encontro;
    List<PublicacaoDoEncontro> publicacoesAtuais = state.publicacoes;

    if (encontroAtual == null) {
      state = const EstadoDosMomentosDoEncontro.carregando();
    }

    try {
      EncontroDetalhado encontro =
          await _repositorioDeEncontros.obtenhaEncontroAsync(
        _identificadorDoEncontro,
      );
      List<PublicacaoDoEncontro> publicacoes =
          await _repositorioDePublicacoes.listeAsync(
        _identificadorDoEncontro,
      );
      publicacoes.sort(_comparePublicacoes);

      state = EstadoDosMomentosDoEncontro(
        situacao: SituacaoDosMomentosDoEncontro.carregado,
        encontro: encontro,
        publicacoes: publicacoes,
      );
    } on ExcecaoDaApi catch (excecao) {
      if (encontroAtual != null) {
        state = EstadoDosMomentosDoEncontro(
          situacao: SituacaoDosMomentosDoEncontro.carregado,
          encontro: encontroAtual,
          publicacoes: publicacoesAtuais,
          mensagemDeErro: excecao.mensagem,
        );
        return;
      }

      state = EstadoDosMomentosDoEncontro(
        situacao: SituacaoDosMomentosDoEncontro.falhou,
        mensagemDeErro: excecao.mensagem,
      );
    }
  }

  Future<bool> publiqueAsync(String texto) async {
    EncontroDetalhado? encontroAtual = state.encontro;
    String textoNormalizado = texto.trim();

    if (encontroAtual == null ||
        state.estaPublicando ||
        textoNormalizado.isEmpty ||
        textoNormalizado.length > 1000) {
      return false;
    }

    state = EstadoDosMomentosDoEncontro(
      situacao: SituacaoDosMomentosDoEncontro.carregado,
      encontro: encontroAtual,
      publicacoes: state.publicacoes,
      estaPublicando: true,
    );

    try {
      PublicacaoDoEncontro publicacao =
          await _repositorioDePublicacoes.publiqueAsync(
        identificadorDoEncontro: _identificadorDoEncontro,
        texto: textoNormalizado,
      );
      List<PublicacaoDoEncontro> publicacoes = <PublicacaoDoEncontro>[
        ...state.publicacoes.where(
          (PublicacaoDoEncontro item) =>
              item.identificador != publicacao.identificador,
        ),
        publicacao,
      ]..sort(_comparePublicacoes);

      state = EstadoDosMomentosDoEncontro(
        situacao: SituacaoDosMomentosDoEncontro.carregado,
        encontro: encontroAtual,
        publicacoes: publicacoes,
      );
      return true;
    } on ExcecaoDaApi catch (excecao) {
      state = EstadoDosMomentosDoEncontro(
        situacao: SituacaoDosMomentosDoEncontro.carregado,
        encontro: encontroAtual,
        publicacoes: state.publicacoes,
        mensagemDeErro: excecao.mensagem,
      );
      return false;
    }
  }

  Future<bool> respondaPresencaAsync(String situacao) async {
    EncontroDetalhado? encontroAtual = state.encontro;

    if (encontroAtual == null || state.estaAtualizandoPresenca) {
      return false;
    }

    state = EstadoDosMomentosDoEncontro(
      situacao: SituacaoDosMomentosDoEncontro.carregado,
      encontro: encontroAtual,
      publicacoes: state.publicacoes,
      estaAtualizandoPresenca: true,
    );

    try {
      await _repositorioDeEncontros.respondaPresencaAsync(
        identificador: _identificadorDoEncontro,
        situacao: situacao,
      );
      EncontroDetalhado encontroAtualizado =
          await _repositorioDeEncontros.obtenhaEncontroAsync(
        _identificadorDoEncontro,
      );
      state = EstadoDosMomentosDoEncontro(
        situacao: SituacaoDosMomentosDoEncontro.carregado,
        encontro: encontroAtualizado,
        publicacoes: state.publicacoes,
      );
      return true;
    } on ExcecaoDaApi catch (excecao) {
      state = EstadoDosMomentosDoEncontro(
        situacao: SituacaoDosMomentosDoEncontro.carregado,
        encontro: encontroAtual,
        publicacoes: state.publicacoes,
        mensagemDeErro: excecao.mensagem,
      );
      return false;
    }
  }

  Future<bool> publiqueImagemAsync(
    ImagemSelecionada imagem,
    String legenda,
  ) async {
    EncontroDetalhado? encontroAtual = state.encontro;
    String legendaNormalizada = legenda.trim();

    if (encontroAtual == null ||
        state.estaPublicando ||
        legendaNormalizada.length > 280) {
      return false;
    }

    state = EstadoDosMomentosDoEncontro(
      situacao: SituacaoDosMomentosDoEncontro.carregado,
      encontro: encontroAtual,
      publicacoes: state.publicacoes,
      estaPublicando: true,
    );

    try {
      MemoriaDoEncontro memoria =
          await _repositorioDeMemorias.publiqueImagemAsync(
        identificadorDoEncontro: _identificadorDoEncontro,
        nomeDoArquivo: imagem.nome,
        tipoDeConteudo: imagem.tipoDeConteudo,
        conteudo: imagem.conteudo,
        legenda: legendaNormalizada,
      );
      PublicacaoDoEncontro publicacao = memoria.convertaParaPublicacao();
      List<PublicacaoDoEncontro> publicacoes = <PublicacaoDoEncontro>[
        ...state.publicacoes.where(
          (PublicacaoDoEncontro item) =>
              item.identificador != publicacao.identificador,
        ),
        publicacao,
      ]..sort(_comparePublicacoes);

      state = EstadoDosMomentosDoEncontro(
        situacao: SituacaoDosMomentosDoEncontro.carregado,
        encontro: encontroAtual,
        publicacoes: publicacoes,
      );
      return true;
    } on ExcecaoDaApi catch (excecao) {
      state = EstadoDosMomentosDoEncontro(
        situacao: SituacaoDosMomentosDoEncontro.carregado,
        encontro: encontroAtual,
        publicacoes: state.publicacoes,
        mensagemDeErro: excecao.mensagem,
      );
      return false;
    }
  }

  Future<bool> removaMemoriaAsync(String identificadorDaMemoria) async {
    EncontroDetalhado? encontroAtual = state.encontro;

    if (encontroAtual == null || state.estaPublicando) {
      return false;
    }

    state = EstadoDosMomentosDoEncontro(
      situacao: SituacaoDosMomentosDoEncontro.carregado,
      encontro: encontroAtual,
      publicacoes: state.publicacoes,
      estaPublicando: true,
    );

    try {
      await _repositorioDeMemorias.removaAsync(
        identificadorDoEncontro: _identificadorDoEncontro,
        identificadorDaMemoria: identificadorDaMemoria,
      );
      state = EstadoDosMomentosDoEncontro(
        situacao: SituacaoDosMomentosDoEncontro.carregado,
        encontro: encontroAtual,
        publicacoes: state.publicacoes
            .where(
              (PublicacaoDoEncontro item) =>
                  item.identificador != identificadorDaMemoria,
            )
            .toList(),
      );
      return true;
    } on ExcecaoDaApi catch (excecao) {
      state = EstadoDosMomentosDoEncontro(
        situacao: SituacaoDosMomentosDoEncontro.carregado,
        encontro: encontroAtual,
        publicacoes: state.publicacoes,
        mensagemDeErro: excecao.mensagem,
      );
      return false;
    }
  }

  int _comparePublicacoes(
    PublicacaoDoEncontro primeira,
    PublicacaoDoEncontro segunda,
  ) {
    int comparacaoDaData = primeira.publicadoEm.compareTo(segunda.publicadoEm);

    if (comparacaoDaData != 0) {
      return comparacaoDaData;
    }

    return primeira.identificador.compareTo(segunda.identificador);
  }
}
