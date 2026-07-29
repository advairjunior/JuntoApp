import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/participante_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/localizacao_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/preferencias_do_aniversario.dart';

class EncontroDetalhado {
  const EncontroDetalhado({
    required this.identificador,
    required this.titulo,
    required this.inicioEm,
    required this.situacao,
    required this.usuarioAtualConfirmouPresenca,
    required this.podeEditar,
    required this.podeCancelar,
    required this.participantes,
    this.descricao,
    this.local,
    this.localizacao,
    this.urlDaImagemDeCapa,
    this.tipo,
    this.preferenciasDoAniversario,
  });

  factory EncontroDetalhado.deJson(Map<String, dynamic> json) {
    List<dynamic> participantesRecebidos =
        json['participantes'] as List<dynamic>? ?? <dynamic>[];

    Map<String, dynamic>? localizacaoRecebida = json['localizacao'] == null
        ? null
        : Map<String, dynamic>.from(
            json['localizacao'] as Map<dynamic, dynamic>,
          );
    Map<String, dynamic>? preferenciasRecebidas =
        json['preferenciasDoAniversario'] == null
            ? null
            : Map<String, dynamic>.from(
                json['preferenciasDoAniversario'] as Map<dynamic, dynamic>,
              );

    return EncontroDetalhado(
      identificador: json['identificador'] as String,
      titulo: json['titulo'] as String,
      descricao: json['descricao'] as String?,
      local: json['local'] as String?,
      localizacao: localizacaoRecebida == null
          ? null
          : LocalizacaoDoEncontro.deJson(localizacaoRecebida),
      urlDaImagemDeCapa: json['urlDaImagemDeCapa'] as String?,
      inicioEm: DateTime.parse(json['inicioEm'] as String).toLocal(),
      situacao: json['situacao'] as String,
      usuarioAtualConfirmouPresenca:
          json['usuarioAtualConfirmouPresenca'] as bool,
      podeEditar: json['podeEditar'] as bool,
      podeCancelar: json['podeCancelar'] as bool,
      participantes: participantesRecebidos
          .map(
            (dynamic participante) => ParticipanteDoEncontro.deJson(
              Map<String, dynamic>.from(
                participante as Map<dynamic, dynamic>,
              ),
            ),
          )
          .toList(),
      tipo: json['tipo'] as String?,
      preferenciasDoAniversario: preferenciasRecebidas == null
          ? null
          : PreferenciasDoAniversario.deJson(preferenciasRecebidas),
    );
  }

  final String identificador;
  final String titulo;
  final String? descricao;
  final String? local;
  final LocalizacaoDoEncontro? localizacao;
  final String? urlDaImagemDeCapa;
  final DateTime inicioEm;
  final String situacao;
  final bool usuarioAtualConfirmouPresenca;
  final bool podeEditar;
  final bool podeCancelar;
  final List<ParticipanteDoEncontro> participantes;
  final String? tipo;
  final PreferenciasDoAniversario? preferenciasDoAniversario;

  ParticipanteDoEncontro? get participanteAtual {
    for (ParticipanteDoEncontro participante in participantes) {
      if (participante.usuarioAtual) {
        return participante;
      }
    }

    return null;
  }

  int get quantidadeDeConfirmados => participantes
      .where(
        (ParticipanteDoEncontro participante) =>
            participante.situacao.toLowerCase() == 'confirmado',
      )
      .length;

  int get quantidadeDeTalvez => participantes
      .where(
        (ParticipanteDoEncontro participante) =>
            participante.situacao.toLowerCase() == 'talvez',
      )
      .length;

  int get quantidadeDeAusentes => participantes
      .where(
        (ParticipanteDoEncontro participante) =>
            participante.situacao.toLowerCase() == 'naovai',
      )
      .length;

  EncontroDetalhado copieComParticipantes(
    List<ParticipanteDoEncontro> novosParticipantes,
  ) {
    return EncontroDetalhado(
      identificador: identificador,
      titulo: titulo,
      descricao: descricao,
      local: local,
      localizacao: localizacao,
      urlDaImagemDeCapa: urlDaImagemDeCapa,
      inicioEm: inicioEm,
      situacao: situacao,
      usuarioAtualConfirmouPresenca: novosParticipantes.any(
        (ParticipanteDoEncontro participante) =>
            participante.usuarioAtual &&
            participante.situacao.toLowerCase() == 'confirmado',
      ),
      podeEditar: podeEditar,
      podeCancelar: podeCancelar,
      participantes: novosParticipantes,
      tipo: tipo,
      preferenciasDoAniversario: preferenciasDoAniversario,
    );
  }
}
