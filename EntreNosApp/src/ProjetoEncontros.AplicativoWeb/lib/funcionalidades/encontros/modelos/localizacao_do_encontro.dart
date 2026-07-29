class LocalizacaoDoEncontro {
  const LocalizacaoDoEncontro({
    required this.descricao,
    this.latitude,
    this.longitude,
  });

  factory LocalizacaoDoEncontro.deJson(Map<String, dynamic> json) {
    return LocalizacaoDoEncontro(
      descricao: json['descricao'] as String,
      latitude: (json['latitude'] as num?)?.toDouble(),
      longitude: (json['longitude'] as num?)?.toDouble(),
    );
  }

  final String descricao;
  final double? latitude;
  final double? longitude;

  bool get temCoordenadas => latitude != null && longitude != null;

  Map<String, dynamic> paraJson() {
    return <String, dynamic>{
      'descricao': descricao,
      'latitude': latitude,
      'longitude': longitude,
    };
  }
}
