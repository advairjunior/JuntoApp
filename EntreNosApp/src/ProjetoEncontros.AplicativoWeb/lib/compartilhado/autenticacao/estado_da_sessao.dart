enum SituacaoDaSessao {
  restaurando,
  naoAutenticada,
  autenticada,
}

class EstadoDaSessao {
  const EstadoDaSessao({
    required this.situacao,
    this.tokenDeAcesso,
    this.expiraEm,
    this.operacaoEstaEmAndamento = false,
    this.mensagemDeErro,
  });

  const EstadoDaSessao.restaurando()
      : situacao = SituacaoDaSessao.restaurando,
        tokenDeAcesso = null,
        expiraEm = null,
        operacaoEstaEmAndamento = false,
        mensagemDeErro = null;

  final SituacaoDaSessao situacao;
  final String? tokenDeAcesso;
  final DateTime? expiraEm;
  final bool operacaoEstaEmAndamento;
  final String? mensagemDeErro;

  bool get usuarioEstaAutenticado =>
      situacao == SituacaoDaSessao.autenticada &&
      tokenDeAcesso != null &&
      expiraEm != null;

  EstadoDaSessao copieCom({
    SituacaoDaSessao? situacao,
    String? tokenDeAcesso,
    DateTime? expiraEm,
    bool? operacaoEstaEmAndamento,
    String? mensagemDeErro,
    bool limpeMensagemDeErro = false,
  }) {
    return EstadoDaSessao(
      situacao: situacao ?? this.situacao,
      tokenDeAcesso: tokenDeAcesso ?? this.tokenDeAcesso,
      expiraEm: expiraEm ?? this.expiraEm,
      operacaoEstaEmAndamento:
          operacaoEstaEmAndamento ?? this.operacaoEstaEmAndamento,
      mensagemDeErro:
          limpeMensagemDeErro ? null : mensagemDeErro ?? this.mensagemDeErro,
    );
  }
}
