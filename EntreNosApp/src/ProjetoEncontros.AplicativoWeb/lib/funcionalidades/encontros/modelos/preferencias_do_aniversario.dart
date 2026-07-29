class PreferenciasDoAniversario {
  const PreferenciasDoAniversario({
    this.numeroDoCalcado,
    this.tamanhoDaCamiseta,
    this.tamanhoDaCalca,
    this.sugestoesDePresente,
    this.coisasQueGostariaDeGanhar,
  });

  factory PreferenciasDoAniversario.deJson(Map<String, dynamic> json) {
    return PreferenciasDoAniversario(
      numeroDoCalcado: json['numeroDoCalcado'] as String?,
      tamanhoDaCamiseta: json['tamanhoDaCamiseta'] as String?,
      tamanhoDaCalca: json['tamanhoDaCalca'] as String?,
      sugestoesDePresente: json['sugestoesDePresente'] as String?,
      coisasQueGostariaDeGanhar: json['coisasQueGostariaDeGanhar'] as String?,
    );
  }

  final String? numeroDoCalcado;
  final String? tamanhoDaCamiseta;
  final String? tamanhoDaCalca;
  final String? sugestoesDePresente;
  final String? coisasQueGostariaDeGanhar;

  bool get temAlgumaInformacao {
    return <String?>[
      numeroDoCalcado,
      tamanhoDaCamiseta,
      tamanhoDaCalca,
      sugestoesDePresente,
      coisasQueGostariaDeGanhar,
    ].any((String? valor) => valor != null && valor.trim().isNotEmpty);
  }

  Map<String, dynamic> paraJson() {
    return <String, dynamic>{
      'numeroDoCalcado': numeroDoCalcado,
      'tamanhoDaCamiseta': tamanhoDaCamiseta,
      'tamanhoDaCalca': tamanhoDaCalca,
      'sugestoesDePresente': sugestoesDePresente,
      'coisasQueGostariaDeGanhar': coisasQueGostariaDeGanhar,
    };
  }
}
