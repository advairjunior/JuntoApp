class ConvitePorLinkCriado {
  const ConvitePorLinkCriado({
    required this.token,
    required this.expiraEm,
  });

  factory ConvitePorLinkCriado.deJson(Map<String, dynamic> json) {
    return ConvitePorLinkCriado(
      token: json['token'] as String,
      expiraEm: DateTime.parse(json['expiraEm'] as String).toLocal(),
    );
  }

  final String token;
  final DateTime expiraEm;
}

class ConvitePorLinkDetalhado {
  const ConvitePorLinkDetalhado({
    required this.identificadorDoEncontro,
    required this.titulo,
    required this.inicioEm,
    this.tipo,
  });

  factory ConvitePorLinkDetalhado.deJson(Map<String, dynamic> json) {
    return ConvitePorLinkDetalhado(
      identificadorDoEncontro: json['identificadorDoEncontro'] as String,
      titulo: json['titulo'] as String,
      inicioEm: DateTime.parse(json['inicioEm'] as String).toLocal(),
      tipo: json['tipo'] as String?,
    );
  }

  final String identificadorDoEncontro;
  final String titulo;
  final DateTime inicioEm;
  final String? tipo;
}

class AceiteDoConvitePorLink {
  const AceiteDoConvitePorLink({
    required this.identificadorDoEncontro,
    required this.situacao,
  });

  factory AceiteDoConvitePorLink.deJson(Map<String, dynamic> json) {
    return AceiteDoConvitePorLink(
      identificadorDoEncontro: json['identificadorDoEncontro'] as String,
      situacao: json['situacao'] as String,
    );
  }

  final String identificadorDoEncontro;
  final String situacao;
}
