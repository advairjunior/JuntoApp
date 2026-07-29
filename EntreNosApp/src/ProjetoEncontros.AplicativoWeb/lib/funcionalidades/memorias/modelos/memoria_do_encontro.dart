import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/midia_da_memoria.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/publicacoes/modelos/publicacao_do_encontro.dart';

class MemoriaDoEncontro {
  const MemoriaDoEncontro({
    required this.identificador,
    required this.identificadorDoEncontro,
    required this.identificadorDoUsuarioAutor,
    required this.nomeDoAutor,
    required this.criadoEm,
    required this.usuarioAtual,
    required this.midias,
    this.urlDaFotoDePerfilDoAutor,
    this.legenda,
  });

  factory MemoriaDoEncontro.deJson(Map<String, dynamic> json) {
    List<dynamic> dadosDasMidias =
        json['midias'] as List<dynamic>? ?? <dynamic>[];

    return MemoriaDoEncontro(
      identificador: json['identificador'] as String,
      identificadorDoEncontro: json['identificadorDoEncontro'] as String,
      identificadorDoUsuarioAutor:
          json['identificadorDoUsuarioAutor'] as String,
      nomeDoAutor: json['nomeDoAutor'] as String,
      urlDaFotoDePerfilDoAutor: json['urlDaFotoDePerfilDoAutor'] as String?,
      legenda: json['legenda'] as String?,
      criadoEm: DateTime.parse(json['criadoEm'] as String).toLocal(),
      usuarioAtual: json['usuarioAtual'] as bool,
      midias: dadosDasMidias
          .map(
            (dynamic item) => MidiaDaMemoria.deJson(
              Map<String, dynamic>.from(item as Map<dynamic, dynamic>),
            ),
          )
          .toList(),
    );
  }

  final String identificador;
  final String identificadorDoEncontro;
  final String identificadorDoUsuarioAutor;
  final String nomeDoAutor;
  final String? urlDaFotoDePerfilDoAutor;
  final String? legenda;
  final DateTime criadoEm;
  final bool usuarioAtual;
  final List<MidiaDaMemoria> midias;

  PublicacaoDoEncontro convertaParaPublicacao() {
    MidiaDaMemoria? midia = midias.firstOrNull;

    return PublicacaoDoEncontro(
      identificador: identificador,
      identificadorDoEncontro: identificadorDoEncontro,
      identificadorDoUsuarioAutor: identificadorDoUsuarioAutor,
      nomeDoAutor: nomeDoAutor,
      urlDaFotoDePerfilDoAutor: urlDaFotoDePerfilDoAutor,
      texto: legenda,
      urlDaMidia: midia?.url,
      tipoDeConteudoDaMidia: midia?.tipoDeConteudo,
      tamanhoDaMidiaEmBytes: midia?.tamanhoEmBytes,
      publicadoEm: criadoEm,
      ehAtualizacaoDoSistema: false,
      usuarioAtual: usuarioAtual,
    );
  }
}
