class RespostaDeSessao {
  const RespostaDeSessao({
    required this.tokenDeAcesso,
    required this.expiraEm,
  });

  factory RespostaDeSessao.deJson(Map<String, dynamic> json) {
    return RespostaDeSessao(
      tokenDeAcesso: json['tokenDeAcesso'] as String,
      expiraEm: DateTime.parse(json['expiraEm'] as String),
    );
  }

  final String tokenDeAcesso;
  final DateTime expiraEm;
}
