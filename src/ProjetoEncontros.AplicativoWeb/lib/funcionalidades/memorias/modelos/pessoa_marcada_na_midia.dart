class PessoaMarcadaNaMidia {
  const PessoaMarcadaNaMidia({
    required this.identificadorDoUsuario,
    required this.nome,
    this.urlDaFotoDePerfil,
  });

  factory PessoaMarcadaNaMidia.deJson(Map<String, dynamic> json) {
    return PessoaMarcadaNaMidia(
      identificadorDoUsuario: json['identificadorDoUsuario'] as String,
      nome: json['nome'] as String,
      urlDaFotoDePerfil: json['urlDaFotoDePerfil'] as String?,
    );
  }

  final String identificadorDoUsuario;
  final String nome;
  final String? urlDaFotoDePerfil;

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
}
