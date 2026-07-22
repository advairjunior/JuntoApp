import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:geolocator/geolocator.dart';
import 'package:url_launcher/url_launcher.dart';

class CoordenadasDoEncontro {
  const CoordenadasDoEncontro({
    required this.latitude,
    required this.longitude,
    this.descricao,
  });

  final double latitude;
  final double longitude;
  final String? descricao;
}

abstract interface class IServicoDeLocalizacao {
  Future<CoordenadasDoEncontro> obtenhaPosicaoAtualAsync();

  Future<bool> abraNoMapaAsync({
    required double latitude,
    required double longitude,
  });
}

final provedorDoServicoDeLocalizacao = Provider<IServicoDeLocalizacao>(
  (Ref referencia) => ServicoDeLocalizacao(),
);

class ServicoDeLocalizacao implements IServicoDeLocalizacao {
  @override
  Future<CoordenadasDoEncontro> obtenhaPosicaoAtualAsync() async {
    Position posicao = await Geolocator.getCurrentPosition(
      locationSettings: const LocationSettings(
        accuracy: LocationAccuracy.high,
        timeLimit: Duration(seconds: 20),
      ),
    );

    return CoordenadasDoEncontro(
      latitude: posicao.latitude,
      longitude: posicao.longitude,
    );
  }

  @override
  Future<bool> abraNoMapaAsync({
    required double latitude,
    required double longitude,
  }) {
    String latitudeFormatada = latitude.toStringAsFixed(6);
    String longitudeFormatada = longitude.toStringAsFixed(6);
    Uri endereco = Uri.parse(
      'https://www.openstreetmap.org/?mlat=$latitudeFormatada&mlon=$longitudeFormatada#map=18/$latitudeFormatada/$longitudeFormatada',
    );

    return launchUrl(endereco, mode: LaunchMode.externalApplication);
  }
}
