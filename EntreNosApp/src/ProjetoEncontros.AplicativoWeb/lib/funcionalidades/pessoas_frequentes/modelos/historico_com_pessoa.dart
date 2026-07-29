import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/memoria_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/midia_da_memoria.dart';

class HistoricoComPessoa {
  const HistoricoComPessoa({
    required this.identificadorDaPessoa,
    required this.nome,
    required this.quantidadeDeEncontrosEmComum,
    required this.quantidadeDeEncontrosRealizadosJuntos,
    required this.proximosEncontros,
    required this.temMaisProximosEncontros,
    required this.estatisticas,
    required this.historico,
    required this.memorias,
    required this.temMaisMemorias,
    this.urlDaFotoDePerfil,
    this.ultimoEncontroEm,
    this.primeiroEncontroEm,
    this.proximoEncontroEm,
    this.diasSemSeVer,
  });

  factory HistoricoComPessoa.deJson(Map<String, dynamic> json) {
    List<dynamic> proximos =
        json['proximosEncontros'] as List<dynamic>? ?? <dynamic>[];
    List<dynamic> memorias = json['memorias'] as List<dynamic>? ?? <dynamic>[];

    return HistoricoComPessoa(
      identificadorDaPessoa: json['identificadorDaPessoa'] as String,
      nome: json['nome'] as String,
      urlDaFotoDePerfil: json['urlDaFotoDePerfil'] as String?,
      quantidadeDeEncontrosEmComum:
          json['quantidadeDeEncontrosEmComum'] as int? ?? 0,
      quantidadeDeEncontrosRealizadosJuntos:
          json['quantidadeDeEncontrosRealizadosJuntos'] as int? ?? 0,
      ultimoEncontroEm: _leiaData(json['ultimoEncontroEm']),
      primeiroEncontroEm: _leiaData(json['primeiroEncontroEm']),
      proximoEncontroEm: _leiaData(json['proximoEncontroEm']),
      diasSemSeVer: json['diasSemSeVer'] as int?,
      proximosEncontros: proximos
          .map(
            (dynamic item) => ProximoEncontroComPessoa.deJson(
              Map<String, dynamic>.from(item as Map<dynamic, dynamic>),
            ),
          )
          .toList(),
      temMaisProximosEncontros:
          json['temMaisProximosEncontros'] as bool? ?? false,
      estatisticas: EstatisticasComPessoa.deJson(
        Map<String, dynamic>.from(
          json['estatisticas'] as Map<dynamic, dynamic>,
        ),
      ),
      historico: PaginaDoHistoricoComPessoa.deJson(
        Map<String, dynamic>.from(
          json['historico'] as Map<dynamic, dynamic>,
        ),
      ),
      memorias: memorias
          .map(
            (dynamic item) => _leiaMemoria(
              Map<String, dynamic>.from(item as Map<dynamic, dynamic>),
            ),
          )
          .toList(),
      temMaisMemorias: json['temMaisMemorias'] as bool? ?? false,
    );
  }

  final String identificadorDaPessoa;
  final String nome;
  final String? urlDaFotoDePerfil;
  final int quantidadeDeEncontrosEmComum;
  final int quantidadeDeEncontrosRealizadosJuntos;
  final DateTime? ultimoEncontroEm;
  final DateTime? primeiroEncontroEm;
  final DateTime? proximoEncontroEm;
  final int? diasSemSeVer;
  final List<ProximoEncontroComPessoa> proximosEncontros;
  final bool temMaisProximosEncontros;
  final EstatisticasComPessoa estatisticas;
  final PaginaDoHistoricoComPessoa historico;
  final List<MemoriaDoEncontro> memorias;
  final bool temMaisMemorias;

  HistoricoComPessoa acrescenteHistorico(HistoricoComPessoa paginaSeguinte) {
    return HistoricoComPessoa(
      identificadorDaPessoa: identificadorDaPessoa,
      nome: nome,
      urlDaFotoDePerfil: urlDaFotoDePerfil,
      quantidadeDeEncontrosEmComum: quantidadeDeEncontrosEmComum,
      quantidadeDeEncontrosRealizadosJuntos:
          quantidadeDeEncontrosRealizadosJuntos,
      ultimoEncontroEm: ultimoEncontroEm,
      primeiroEncontroEm: primeiroEncontroEm,
      proximoEncontroEm: proximoEncontroEm,
      diasSemSeVer: diasSemSeVer,
      proximosEncontros: proximosEncontros,
      temMaisProximosEncontros: temMaisProximosEncontros,
      estatisticas: estatisticas,
      historico: historico.acrescente(paginaSeguinte.historico),
      memorias: memorias,
      temMaisMemorias: temMaisMemorias,
    );
  }

  HistoricoComPessoa substituaMemorias(HistoricoComPessoa resultado) {
    return HistoricoComPessoa(
      identificadorDaPessoa: identificadorDaPessoa,
      nome: nome,
      urlDaFotoDePerfil: urlDaFotoDePerfil,
      quantidadeDeEncontrosEmComum: quantidadeDeEncontrosEmComum,
      quantidadeDeEncontrosRealizadosJuntos:
          quantidadeDeEncontrosRealizadosJuntos,
      ultimoEncontroEm: ultimoEncontroEm,
      primeiroEncontroEm: primeiroEncontroEm,
      proximoEncontroEm: proximoEncontroEm,
      diasSemSeVer: diasSemSeVer,
      proximosEncontros: proximosEncontros,
      temMaisProximosEncontros: temMaisProximosEncontros,
      estatisticas: estatisticas,
      historico: historico,
      memorias: resultado.memorias,
      temMaisMemorias: resultado.temMaisMemorias,
    );
  }

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

class ProximoEncontroComPessoa {
  const ProximoEncontroComPessoa({
    required this.identificadorDoEncontro,
    required this.titulo,
    required this.inicioEm,
    required this.situacaoDoUsuarioAtual,
    required this.situacaoDaPessoa,
    this.descricao,
    this.local,
    this.tipo,
    this.urlDaImagemDeCapa,
  });

  factory ProximoEncontroComPessoa.deJson(Map<String, dynamic> json) {
    return ProximoEncontroComPessoa(
      identificadorDoEncontro: json['identificadorDoEncontro'] as String,
      titulo: json['titulo'] as String,
      descricao: json['descricao'] as String?,
      local: json['local'] as String?,
      tipo: json['tipo'] as String?,
      urlDaImagemDeCapa: json['urlDaImagemDeCapa'] as String?,
      inicioEm: DateTime.parse(json['inicioEm'] as String).toLocal(),
      situacaoDoUsuarioAtual: json['situacaoDoUsuarioAtual'] as String,
      situacaoDaPessoa: json['situacaoDaPessoa'] as String,
    );
  }

  final String identificadorDoEncontro;
  final String titulo;
  final String? descricao;
  final String? local;
  final String? tipo;
  final String? urlDaImagemDeCapa;
  final DateTime inicioEm;
  final String situacaoDoUsuarioAtual;
  final String situacaoDaPessoa;
}

class EstatisticasComPessoa {
  const EstatisticasComPessoa({
    required this.quantidadeDeEncontrosRealizadosJuntos,
    required this.quantidadeDeEncontrosJuntosNesteAno,
    this.mediaDeDiasEntreEncontros,
    this.maiorIntervaloEmDias,
    this.tipoMaisFrequente,
    this.diaDaSemanaMaisFrequente,
    this.localMaisFrequente,
  });

  factory EstatisticasComPessoa.deJson(Map<String, dynamic> json) {
    return EstatisticasComPessoa(
      quantidadeDeEncontrosRealizadosJuntos:
          json['quantidadeDeEncontrosRealizadosJuntos'] as int? ?? 0,
      quantidadeDeEncontrosJuntosNesteAno:
          json['quantidadeDeEncontrosJuntosNesteAno'] as int? ?? 0,
      mediaDeDiasEntreEncontros:
          (json['mediaDeDiasEntreEncontros'] as num?)?.toDouble(),
      maiorIntervaloEmDias: json['maiorIntervaloEmDias'] as int?,
      tipoMaisFrequente: json['tipoMaisFrequente'] as String?,
      diaDaSemanaMaisFrequente: json['diaDaSemanaMaisFrequente'] as String?,
      localMaisFrequente: json['localMaisFrequente'] as String?,
    );
  }

  final int quantidadeDeEncontrosRealizadosJuntos;
  final int quantidadeDeEncontrosJuntosNesteAno;
  final double? mediaDeDiasEntreEncontros;
  final int? maiorIntervaloEmDias;
  final String? tipoMaisFrequente;
  final String? diaDaSemanaMaisFrequente;
  final String? localMaisFrequente;
}

class PaginaDoHistoricoComPessoa {
  const PaginaDoHistoricoComPessoa({
    required this.pagina,
    required this.tamanho,
    required this.quantidadeTotal,
    required this.temProximaPagina,
    required this.itens,
  });

  factory PaginaDoHistoricoComPessoa.deJson(Map<String, dynamic> json) {
    List<dynamic> itens = json['itens'] as List<dynamic>? ?? <dynamic>[];

    return PaginaDoHistoricoComPessoa(
      pagina: json['pagina'] as int? ?? 1,
      tamanho: json['tamanho'] as int? ?? 10,
      quantidadeTotal: json['quantidadeTotal'] as int? ?? 0,
      temProximaPagina: json['temProximaPagina'] as bool? ?? false,
      itens: itens
          .map(
            (dynamic item) => EncontroDoHistoricoComPessoa.deJson(
              Map<String, dynamic>.from(item as Map<dynamic, dynamic>),
            ),
          )
          .toList(),
    );
  }

  final int pagina;
  final int tamanho;
  final int quantidadeTotal;
  final bool temProximaPagina;
  final List<EncontroDoHistoricoComPessoa> itens;

  PaginaDoHistoricoComPessoa acrescente(
    PaginaDoHistoricoComPessoa paginaSeguinte,
  ) {
    return PaginaDoHistoricoComPessoa(
      pagina: paginaSeguinte.pagina,
      tamanho: paginaSeguinte.tamanho,
      quantidadeTotal: paginaSeguinte.quantidadeTotal,
      temProximaPagina: paginaSeguinte.temProximaPagina,
      itens: <EncontroDoHistoricoComPessoa>[
        ...itens,
        ...paginaSeguinte.itens,
      ],
    );
  }
}

class EncontroDoHistoricoComPessoa {
  const EncontroDoHistoricoComPessoa({
    required this.identificadorDoEncontro,
    required this.titulo,
    required this.inicioEm,
    this.local,
    this.tipo,
    this.urlDaImagemDeCapa,
  });

  factory EncontroDoHistoricoComPessoa.deJson(Map<String, dynamic> json) {
    return EncontroDoHistoricoComPessoa(
      identificadorDoEncontro: json['identificadorDoEncontro'] as String,
      titulo: json['titulo'] as String,
      local: json['local'] as String?,
      tipo: json['tipo'] as String?,
      urlDaImagemDeCapa: json['urlDaImagemDeCapa'] as String?,
      inicioEm: DateTime.parse(json['inicioEm'] as String).toLocal(),
    );
  }

  final String identificadorDoEncontro;
  final String titulo;
  final String? local;
  final String? tipo;
  final String? urlDaImagemDeCapa;
  final DateTime inicioEm;
}

DateTime? _leiaData(dynamic valor) {
  return valor == null ? null : DateTime.parse(valor as String).toLocal();
}

MemoriaDoEncontro _leiaMemoria(Map<String, dynamic> json) {
  List<dynamic> dadosDasMidias =
      json['midias'] as List<dynamic>? ?? <dynamic>[];

  return MemoriaDoEncontro(
    identificador: json['identificadorDaMemoria'] as String,
    identificadorDoEncontro: json['identificadorDoEncontro'] as String,
    identificadorDoUsuarioAutor: json['identificadorDoUsuarioAutor'] as String,
    nomeDoAutor: json['nomeDoAutor'] as String,
    urlDaFotoDePerfilDoAutor: json['urlDaFotoDePerfilDoAutor'] as String?,
    legenda: json['legenda'] as String?,
    criadoEm: DateTime.parse(json['criadaEm'] as String).toLocal(),
    usuarioAtual: json['usuarioAtual'] as bool? ?? false,
    midias: dadosDasMidias.map(
      (dynamic item) {
        Map<String, dynamic> midia =
            Map<String, dynamic>.from(item as Map<dynamic, dynamic>);
        return MidiaDaMemoria(
          identificador: midia['identificadorDaMidia'] as String,
          url: midia['url'] as String,
          tipoDeConteudo: midia['tipoDeConteudo'] as String,
          tamanhoEmBytes: midia['tamanhoEmBytes'] as int,
        );
      },
    ).toList(),
  );
}
