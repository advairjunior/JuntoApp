import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/pessoa_marcada_na_midia.dart';

class MidiaDaMemoria {
  const MidiaDaMemoria({
    required this.identificador,
    required this.url,
    required this.tipoDeConteudo,
    required this.tamanhoEmBytes,
    this.pessoasMarcadas = const <PessoaMarcadaNaMidia>[],
  });

  factory MidiaDaMemoria.deJson(Map<String, dynamic> json) {
    List<dynamic> pessoas =
        json['pessoasMarcadas'] as List<dynamic>? ?? <dynamic>[];

    return MidiaDaMemoria(
      identificador:
          (json['identificador'] ?? json['identificadorDaMidia']) as String,
      url: json['url'] as String,
      tipoDeConteudo: json['tipoDeConteudo'] as String,
      tamanhoEmBytes: json['tamanhoEmBytes'] as int,
      pessoasMarcadas: pessoas
          .map(
            (dynamic pessoa) => PessoaMarcadaNaMidia.deJson(
              Map<String, dynamic>.from(pessoa as Map<dynamic, dynamic>),
            ),
          )
          .toList(),
    );
  }

  final String identificador;
  final String url;
  final String tipoDeConteudo;
  final int tamanhoEmBytes;
  final List<PessoaMarcadaNaMidia> pessoasMarcadas;

  MidiaDaMemoria copieComPessoasMarcadas(
    List<PessoaMarcadaNaMidia> novasPessoasMarcadas,
  ) {
    return MidiaDaMemoria(
      identificador: identificador,
      url: url,
      tipoDeConteudo: tipoDeConteudo,
      tamanhoEmBytes: tamanhoEmBytes,
      pessoasMarcadas: novasPessoasMarcadas,
    );
  }
}
