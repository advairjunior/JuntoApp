class PessoaFrequente {
  const PessoaFrequente({
    required this.identificadorDoUsuario,
    required this.nome,
    required this.quantidadeDeEncontrosEmComum,
    required this.ultimoEncontroEm,
    this.urlDaFotoDePerfil,
  });

  factory PessoaFrequente.deJson(Map<String, dynamic> json) {
    return PessoaFrequente(
      identificadorDoUsuario: json['identificadorDoUsuario'] as String,
      nome: json['nome'] as String,
      urlDaFotoDePerfil: json['urlDaFotoDePerfil'] as String?,
      quantidadeDeEncontrosEmComum:
          json['quantidadeDeEncontrosEmComum'] as int? ?? 0,
      ultimoEncontroEm:
          DateTime.parse(json['ultimoEncontroEm'] as String).toLocal(),
    );
  }

  final String identificadorDoUsuario;
  final String nome;
  final String? urlDaFotoDePerfil;
  final int quantidadeDeEncontrosEmComum;
  final DateTime ultimoEncontroEm;

  String get iniciais {
    List<String> partes = nome
        .trim()
        .split(RegExp(r'\s+'))
        .where((String parte) => parte.isNotEmpty)
        .toList();

    if (partes.isEmpty) {
      return '?';
    }

    if (partes.length == 1) {
      return partes.first.substring(0, 1).toUpperCase();
    }

    return '${partes.first[0]}${partes.last[0]}'.toUpperCase();
  }

  String get textoDaRecorrencia {
    if (quantidadeDeEncontrosEmComum == 1) {
      return '1 encontro juntos';
    }

    return '$quantidadeDeEncontrosEmComum encontros juntos';
  }
}
