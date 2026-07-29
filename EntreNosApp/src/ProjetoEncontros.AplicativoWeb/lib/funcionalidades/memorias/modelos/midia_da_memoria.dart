class MidiaDaMemoria {
  const MidiaDaMemoria({
    required this.identificador,
    required this.url,
    required this.tipoDeConteudo,
    required this.tamanhoEmBytes,
  });

  factory MidiaDaMemoria.deJson(Map<String, dynamic> json) {
    return MidiaDaMemoria(
      identificador: json['identificador'] as String,
      url: json['url'] as String,
      tipoDeConteudo: json['tipoDeConteudo'] as String,
      tamanhoEmBytes: json['tamanhoEmBytes'] as int,
    );
  }

  final String identificador;
  final String url;
  final String tipoDeConteudo;
  final int tamanhoEmBytes;
}
