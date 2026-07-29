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

enum AplicativoDeMapa {
  googleMaps,
  appleMaps,
  waze,
}

abstract interface class IServicoDeLocalizacao {
  Future<CoordenadasDoEncontro> obtenhaPosicaoAtualAsync();

  Future<bool> abraNoMapaAsync({
    required double latitude,
    required double longitude,
    required AplicativoDeMapa aplicativo,
    String? descricao,
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
    required AplicativoDeMapa aplicativo,
    String? descricao,
  }) {
    String latitudeFormatada = latitude.toStringAsFixed(6);
    String longitudeFormatada = longitude.toStringAsFixed(6);
    Uri endereco = crieEnderecoDoMapa(
      aplicativo: aplicativo,
      latitude: latitudeFormatada,
      longitude: longitudeFormatada,
      descricao: descricao,
    );

    return launchUrl(endereco, mode: LaunchMode.externalApplication);
  }
}

Uri crieEnderecoDoMapa({
  required AplicativoDeMapa aplicativo,
  required String latitude,
  required String longitude,
  String? descricao,
}) {
  String destino = '$latitude,$longitude';
  String nomeDoDestino = descricao?.trim().isNotEmpty == true
      ? descricao!.trim()
      : 'Local do encontro';

  return switch (aplicativo) {
    AplicativoDeMapa.googleMaps => Uri.https(
        'www.google.com',
        '/maps/dir/',
        <String, String>{
          'api': '1',
          'destination': destino,
          'travelmode': 'driving',
        },
      ),
    AplicativoDeMapa.appleMaps => Uri.https(
        'maps.apple.com',
        '/',
        <String, String>{
          'daddr': destino,
          'q': nomeDoDestino,
          'dirflg': 'd',
        },
      ),
    AplicativoDeMapa.waze => Uri.https(
        'waze.com',
        '/ul',
        <String, String>{
          'll': destino,
          'navigate': 'yes',
          'utm_source': 'junto',
        },
      ),
  };
}
