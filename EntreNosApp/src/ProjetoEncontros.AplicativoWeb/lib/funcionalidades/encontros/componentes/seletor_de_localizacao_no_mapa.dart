import 'dart:math' as math;

import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:latlong2/latlong.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/servicos/servico_de_localizacao.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/servicos/servico_de_busca_de_localizacao.dart';
import 'package:url_launcher/url_launcher.dart';

Future<CoordenadasDoEncontro?> abraSeletorDeLocalizacaoNoMapaAsync(
  BuildContext context, {
  String termoInicial = '',
  double? latitudeInicial,
  double? longitudeInicial,
}) {
  return showModalBottomSheet<CoordenadasDoEncontro>(
    context: context,
    isScrollControlled: true,
    backgroundColor: CoresDoAplicativo.fundoDoCartao,
    builder: (BuildContext context) {
      return _SeletorDeLocalizacaoNoMapa(
        termoInicial: termoInicial,
        latitudeInicial: latitudeInicial,
        longitudeInicial: longitudeInicial,
      );
    },
  );
}

class _SeletorDeLocalizacaoNoMapa extends ConsumerStatefulWidget {
  const _SeletorDeLocalizacaoNoMapa({
    required this.termoInicial,
    this.latitudeInicial,
    this.longitudeInicial,
  });

  final String termoInicial;
  final double? latitudeInicial;
  final double? longitudeInicial;

  @override
  ConsumerState<_SeletorDeLocalizacaoNoMapa> createState() =>
      _EstadoDoSeletorDeLocalizacaoNoMapa();
}

class _EstadoDoSeletorDeLocalizacaoNoMapa
    extends ConsumerState<_SeletorDeLocalizacaoNoMapa> {
  final MapController _controladorDoMapa = MapController();
  LatLng? _pontoSelecionado;
  late final TextEditingController _controladorDaBusca;
  List<ResultadoDaBuscaDeLocalizacao> _resultados =
      <ResultadoDaBuscaDeLocalizacao>[];
  String? _descricaoSelecionada;
  String? _mensagemDaBusca;
  bool _estaBuscando = false;

  @override
  void initState() {
    super.initState();
    _controladorDaBusca = TextEditingController(text: widget.termoInicial);

    if (widget.latitudeInicial != null && widget.longitudeInicial != null) {
      _pontoSelecionado = LatLng(
        widget.latitudeInicial!,
        widget.longitudeInicial!,
      );
    }
  }

  @override
  void dispose() {
    _controladorDaBusca.dispose();
    _controladorDoMapa.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    LatLng centroInicial = _pontoSelecionado ?? const LatLng(-14.235, -51.9253);
    double altura = math.min(MediaQuery.sizeOf(context).height * 0.88, 720);

    return SafeArea(
      child: SizedBox(
        height: altura,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: <Widget>[
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 8, 12, 12),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: <Widget>[
                  Row(
                    children: <Widget>[
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: <Widget>[
                            Text(
                              'Buscar localização',
                              style: Theme.of(context).textTheme.titleLarge,
                            ),
                            const Text(
                              'Digite o nome do lugar, rua ou endereço em Goiânia.',
                              style: TextStyle(
                                color: CoresDoAplicativo.textoSecundario,
                              ),
                            ),
                          ],
                        ),
                      ),
                      IconButton(
                        tooltip: 'Fechar mapa',
                        onPressed: () => Navigator.of(context).pop(),
                        icon: const Icon(Icons.close_rounded),
                      ),
                    ],
                  ),
                  const SizedBox(height: EspacamentosDoAplicativo.medio),
                  TextField(
                    key: const Key('campo-de-busca-da-localizacao'),
                    controller: _controladorDaBusca,
                    textInputAction: TextInputAction.search,
                    onSubmitted: (_) => _busqueAsync(),
                    decoration: InputDecoration(
                      labelText: 'Local ou endereço em Goiânia',
                      prefixIcon: const Icon(Icons.search_rounded),
                      suffixIcon: IconButton(
                        key: const Key('buscar-localizacao'),
                        tooltip: 'Buscar localização',
                        onPressed: _estaBuscando ? null : _busqueAsync,
                        icon: _estaBuscando
                            ? const SizedBox.square(
                                dimension: 18,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                ),
                              )
                            : const Icon(Icons.arrow_forward_rounded),
                      ),
                    ),
                  ),
                  if (_mensagemDaBusca != null) ...<Widget>[
                    const SizedBox(height: EspacamentosDoAplicativo.pequeno),
                    Text(
                      _mensagemDaBusca!,
                      style: const TextStyle(
                        color: CoresDoAplicativo.coral,
                        fontSize: 12,
                      ),
                    ),
                  ],
                  if (_resultados.isNotEmpty) ...<Widget>[
                    const SizedBox(height: EspacamentosDoAplicativo.pequeno),
                    ConstrainedBox(
                      constraints: const BoxConstraints(maxHeight: 190),
                      child: ListView.separated(
                        shrinkWrap: true,
                        itemCount: _resultados.length,
                        separatorBuilder: (_, __) => const Divider(height: 1),
                        itemBuilder: (BuildContext context, int indice) {
                          ResultadoDaBuscaDeLocalizacao resultado =
                              _resultados[indice];

                          return ListTile(
                            key: Key('resultado-da-localizacao-$indice'),
                            dense: true,
                            leading: const Icon(Icons.location_on_outlined),
                            title: Text(
                              resultado.descricao,
                              maxLines: 2,
                              overflow: TextOverflow.ellipsis,
                            ),
                            onTap: () => _selecioneResultado(resultado),
                          );
                        },
                      ),
                    ),
                  ],
                ],
              ),
            ),
            Expanded(
              child: FlutterMap(
                mapController: _controladorDoMapa,
                options: MapOptions(
                  initialCenter: centroInicial,
                  initialZoom: _pontoSelecionado == null ? 3.5 : 16,
                  minZoom: 2,
                  maxZoom: 19,
                  onTap: (TapPosition posicaoDoToque, LatLng ponto) {
                    setState(() {
                      _pontoSelecionado = ponto;
                      _resultados = <ResultadoDaBuscaDeLocalizacao>[];
                    });
                  },
                ),
                children: <Widget>[
                  TileLayer(
                    urlTemplate:
                        'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                    userAgentPackageName: 'br.com.junto.aplicativo',
                  ),
                  if (_pontoSelecionado != null)
                    MarkerLayer(
                      markers: <Marker>[
                        Marker(
                          point: _pontoSelecionado!,
                          width: 48,
                          height: 48,
                          alignment: Alignment.topCenter,
                          child: const Icon(
                            Icons.location_on_rounded,
                            size: 44,
                            color: CoresDoAplicativo.coral,
                          ),
                        ),
                      ],
                    ),
                  RichAttributionWidget(
                    attributions: <SourceAttribution>[
                      TextSourceAttribution(
                        'OpenStreetMap contributors',
                        onTap: () => launchUrl(
                          Uri.parse('https://www.openstreetmap.org/copyright'),
                          mode: LaunchMode.externalApplication,
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(EspacamentosDoAplicativo.padrao),
              child: FilledButton.icon(
                key: const Key('confirmar-ponto-no-mapa'),
                onPressed: _pontoSelecionado == null
                    ? null
                    : () {
                        Navigator.of(context).pop(
                          CoordenadasDoEncontro(
                            latitude: _pontoSelecionado!.latitude,
                            longitude: _pontoSelecionado!.longitude,
                            descricao: _descricaoSelecionada,
                          ),
                        );
                      },
                icon: const Icon(Icons.check_rounded),
                label: const Text('Usar este ponto'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _busqueAsync() async {
    String termo = _controladorDaBusca.text.trim();

    if (termo.length < 3) {
      setState(() {
        _mensagemDaBusca = 'Digite ao menos 3 caracteres para buscar.';
        _resultados = <ResultadoDaBuscaDeLocalizacao>[];
      });
      return;
    }

    setState(() {
      _estaBuscando = true;
      _mensagemDaBusca = null;
      _resultados = <ResultadoDaBuscaDeLocalizacao>[];
    });

    try {
      List<ResultadoDaBuscaDeLocalizacao> resultados = await ref
          .read(provedorDoServicoDeBuscaDeLocalizacao)
          .busqueAsync(termo);

      if (!mounted) {
        return;
      }

      setState(() {
        _resultados = resultados;
        _mensagemDaBusca = resultados.isEmpty
            ? 'Nenhum local encontrado em Goiânia. Tente o endereço completo ou marque o ponto no mapa.'
            : null;
      });
    } on Object {
      if (mounted) {
        setState(() {
          _mensagemDaBusca =
              'Não foi possível buscar agora. Tente novamente ou marque o ponto no mapa.';
        });
      }
    } finally {
      if (mounted) {
        setState(() {
          _estaBuscando = false;
        });
      }
    }
  }

  void _selecioneResultado(ResultadoDaBuscaDeLocalizacao resultado) {
    LatLng ponto = LatLng(resultado.latitude, resultado.longitude);

    setState(() {
      _pontoSelecionado = ponto;
      _descricaoSelecionada = resultado.descricao;
      _controladorDaBusca.text = resultado.descricao;
      _resultados = <ResultadoDaBuscaDeLocalizacao>[];
      _mensagemDaBusca = null;
    });
    _controladorDoMapa.move(ponto, 16);
  }
}
