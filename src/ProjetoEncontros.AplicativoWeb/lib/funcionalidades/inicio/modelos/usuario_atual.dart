class UsuarioAtual {
  const UsuarioAtual({
    required this.identificador,
    required this.nome,
    required this.email,
    this.urlDaFotoDePerfil,
  });

  factory UsuarioAtual.deJson(Map<String, dynamic> json) {
    return UsuarioAtual(
      identificador: json['identificador'] as String,
      nome: json['nome'] as String,
      email: json['email'] as String,
      urlDaFotoDePerfil: json['urlDaFotoDePerfil'] as String?,
    );
  }

  final String identificador;
  final String nome;
  final String email;
  final String? urlDaFotoDePerfil;

  String get primeiroNome => nome.trim().split(RegExp(r'\s+')).first;

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

    return '${partes.first.substring(0, 1)}${partes.last.substring(0, 1)}'
        .toUpperCase();
  }
}
