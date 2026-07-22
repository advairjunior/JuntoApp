import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:geolocator/geolocator.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/conteudo_responsivo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/dados/repositorio_de_encontros.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/componentes/seletor_de_localizacao_no_mapa.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_detalhado.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/servicos/servico_de_localizacao.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/estado/controlador_da_pagina_inicial.dart';

const List<_OpcaoDeTipoDoEncontro> _opcoesDeTipoDoEncontro =
    <_OpcaoDeTipoDoEncontro>[
  _OpcaoDeTipoDoEncontro('Amigos', Icons.groups_outlined),
  _OpcaoDeTipoDoEncontro('Família', Icons.home_outlined),
  _OpcaoDeTipoDoEncontro('Churrasco', Icons.outdoor_grill_outlined),
  _OpcaoDeTipoDoEncontro('Futebol', Icons.sports_soccer_outlined),
  _OpcaoDeTipoDoEncontro('Igreja', Icons.church_outlined),
  _OpcaoDeTipoDoEncontro('Música', Icons.music_note_outlined),
  _OpcaoDeTipoDoEncontro('Estudo', Icons.menu_book_outlined),
  _OpcaoDeTipoDoEncontro('Jogo', Icons.sports_esports_outlined),
  _OpcaoDeTipoDoEncontro('Viagem', Icons.luggage_outlined),
];

class TelaDeCriacaoDeEncontro extends ConsumerStatefulWidget {
  const TelaDeCriacaoDeEncontro({
    this.identificadorDoEncontro,
    super.key,
  });

  final String? identificadorDoEncontro;

  @override
  ConsumerState<TelaDeCriacaoDeEncontro> createState() =>
      _EstadoDaTelaDeCriacaoDeEncontro();
}

class _EstadoDaTelaDeCriacaoDeEncontro
    extends ConsumerState<TelaDeCriacaoDeEncontro> {
  final GlobalKey<FormState> _chaveDoFormulario = GlobalKey<FormState>();
  final TextEditingController _controladorDoTitulo = TextEditingController();
  final TextEditingController _controladorDoLocal = TextEditingController();
  final TextEditingController _controladorDaDescricao = TextEditingController();

  late DateTime _data;
  late TimeOfDay _horario;
  bool _estaSalvando = false;
  bool _estaCarregando = false;
  bool _carregamentoFalhou = false;
  bool _estaObtendoLocalizacao = false;
  String? _mensagemDeErro;
  String? _mensagemDaLocalizacao;
  String? _tipo;
  double? _latitude;
  double? _longitude;

  bool get _estaEditando => widget.identificadorDoEncontro != null;

  @override
  void initState() {
    super.initState();
    DateTime amanha = DateTime.now().add(const Duration(days: 1));
    _data = DateTime(amanha.year, amanha.month, amanha.day);
    _horario = const TimeOfDay(hour: 19, minute: 0);

    if (_estaEditando) {
      unawaited(_carregueEncontroAsync());
    }
  }

  @override
  void dispose() {
    _controladorDoTitulo.dispose();
    _controladorDoLocal.dispose();
    _controladorDaDescricao.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    if (_estaCarregando) {
      return const Scaffold(
        body: SafeArea(
          child: Center(child: CircularProgressIndicator()),
        ),
      );
    }

    if (_carregamentoFalhou) {
      return Scaffold(
        body: SafeArea(
          child: ConteudoResponsivo(
            filho: Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: <Widget>[
                  const Icon(
                    Icons.edit_off_outlined,
                    size: 48,
                    color: CoresDoAplicativo.coral,
                  ),
                  const SizedBox(height: EspacamentosDoAplicativo.medio),
                  Text(
                    _mensagemDeErro ?? 'Não foi possível carregar o encontro.',
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: EspacamentosDoAplicativo.padrao),
                  FilledButton.icon(
                    onPressed: _carregueEncontroAsync,
                    icon: const Icon(Icons.refresh_rounded),
                    label: const Text('Tentar novamente'),
                  ),
                  TextButton(
                    onPressed: () => context.pop(),
                    child: const Text('Voltar'),
                  ),
                ],
              ),
            ),
          ),
        ),
      );
    }

    return Scaffold(
      body: SafeArea(
        child: ConteudoResponsivo(
          filho: Form(
            key: _chaveDoFormulario,
            child: ListView(
              children: <Widget>[
                _Cabecalho(
                  estaEditando: _estaEditando,
                  aoVoltar: () {
                    if (context.canPop()) {
                      context.pop();
                      return;
                    }

                    context.go('/inicio');
                  },
                ),
                const SizedBox(height: EspacamentosDoAplicativo.grande),
                Container(
                  padding:
                      const EdgeInsets.all(EspacamentosDoAplicativo.grande),
                  decoration: BoxDecoration(
                    color: CoresDoAplicativo.fundoDoCartao,
                    borderRadius: BorderRadius.circular(20),
                    border: Border.all(color: CoresDoAplicativo.bordaSuave),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: <Widget>[
                      Text(
                        _estaEditando
                            ? 'Atualize o que mudou'
                            : 'Combine um momento real',
                        style: Theme.of(context).textTheme.titleLarge,
                      ),
                      const SizedBox(height: EspacamentosDoAplicativo.minimo),
                      Text(
                        _estaEditando
                            ? 'Mantenha as informações claras para todos os participantes.'
                            : 'Comece pelo essencial. Os detalhes podem continuar simples.',
                        style: const TextStyle(
                          color: CoresDoAplicativo.textoSecundario,
                        ),
                      ),
                      const SizedBox(
                        height: EspacamentosDoAplicativo.grande,
                      ),
                      TextFormField(
                        controller: _controladorDoTitulo,
                        autofocus: !_estaEditando,
                        maxLength: 120,
                        textInputAction: TextInputAction.next,
                        decoration: const InputDecoration(
                          labelText: 'Título do encontro',
                          prefixIcon: Icon(Icons.celebration_outlined),
                        ),
                        onChanged: _aoAlterarDescricaoDoLocal,
                        validator: (String? titulo) {
                          if (titulo == null || titulo.trim().isEmpty) {
                            return 'Informe um título para o encontro.';
                          }

                          return null;
                        },
                      ),
                      const SizedBox(height: EspacamentosDoAplicativo.medio),
                      _SeletorDoTipoDoEncontro(
                        tipo: _tipo,
                        aoSelecionar: _selecioneTipoAsync,
                        aoRemover: () {
                          setState(() {
                            _tipo = null;
                          });
                        },
                      ),
                      const SizedBox(height: EspacamentosDoAplicativo.medio),
                      Row(
                        children: <Widget>[
                          Expanded(
                            child: _SeletorDeDataOuHorario(
                              key: const Key('seletor-de-data'),
                              icone: Icons.calendar_today_outlined,
                              rotulo: 'Data',
                              valor: DateFormat('dd/MM/yyyy').format(_data),
                              aoPressionar: _selecioneDataAsync,
                            ),
                          ),
                          const SizedBox(
                            width: EspacamentosDoAplicativo.medio,
                          ),
                          Expanded(
                            child: _SeletorDeDataOuHorario(
                              key: const Key('seletor-de-horario'),
                              icone: Icons.schedule_rounded,
                              rotulo: 'Horário',
                              valor: _horario.format(context),
                              aoPressionar: _selecioneHorarioAsync,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: EspacamentosDoAplicativo.medio),
                      TextFormField(
                        controller: _controladorDoLocal,
                        maxLength: 200,
                        textInputAction: TextInputAction.next,
                        decoration: const InputDecoration(
                          labelText: 'Nome ou endereço do local',
                          prefixIcon: Icon(Icons.location_on_outlined),
                        ),
                        validator: (String? local) {
                          if (_latitude != null &&
                              (local == null || local.trim().isEmpty)) {
                            return 'Descreva o local que será mostrado aos convidados.';
                          }

                          return null;
                        },
                      ),
                      _ControleDaLocalizacao(
                        temCoordenadas:
                            _latitude != null && _longitude != null,
                        podeAdicionar:
                            _controladorDoLocal.text.trim().isNotEmpty,
                        estaObtendo: _estaObtendoLocalizacao,
                        aoObter: _obtenhaLocalizacaoAtualAsync,
                        aoSelecionarNoMapa: _selecioneLocalizacaoNoMapaAsync,
                        aoRemover: _removaCoordenadas,
                        mensagem: _mensagemDaLocalizacao,
                      ),
                      const SizedBox(height: EspacamentosDoAplicativo.medio),
                      TextFormField(
                        controller: _controladorDaDescricao,
                        maxLength: 500,
                        minLines: 3,
                        maxLines: 5,
                        decoration: const InputDecoration(
                          labelText: 'Descrição opcional',
                          alignLabelWithHint: true,
                          prefixIcon: Icon(Icons.notes_rounded),
                        ),
                      ),
                      if (_mensagemDeErro != null) ...<Widget>[
                        const SizedBox(height: EspacamentosDoAplicativo.medio),
                        Text(
                          _mensagemDeErro!,
                          textAlign: TextAlign.center,
                          style: const TextStyle(
                            color: CoresDoAplicativo.coral,
                          ),
                        ),
                      ],
                      const SizedBox(height: EspacamentosDoAplicativo.grande),
                      FilledButton.icon(
                        key: Key(
                          _estaEditando
                              ? 'botao-salvar-encontro'
                              : 'botao-criar-encontro',
                        ),
                        onPressed: _estaSalvando || _estaObtendoLocalizacao
                            ? null
                            : _salveAsync,
                        icon: _estaSalvando
                            ? const SizedBox.square(
                                dimension: 20,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                ),
                              )
                            : const Icon(Icons.check_rounded),
                        label: Text(
                          _estaSalvando
                              ? (_estaEditando ? 'Salvando...' : 'Criando...')
                              : (_estaEditando
                                  ? 'Salvar alterações'
                                  : 'Criar encontro'),
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: EspacamentosDoAplicativo.grande),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Future<void> _selecioneDataAsync() async {
    DateTime? dataSelecionada = await showDatePicker(
      context: context,
      initialDate: _data,
      firstDate: DateTime.now(),
      lastDate: DateTime.now().add(const Duration(days: 3650)),
      helpText: 'Escolha a data do encontro',
    );

    if (dataSelecionada != null) {
      setState(() {
        _data = dataSelecionada;
      });
    }
  }

  Future<void> _selecioneHorarioAsync() async {
    TimeOfDay? horarioSelecionado = await showTimePicker(
      context: context,
      initialTime: _horario,
      helpText: 'Escolha o horário do encontro',
    );

    if (horarioSelecionado != null) {
      setState(() {
        _horario = horarioSelecionado;
      });
    }
  }

  Future<void> _salveAsync() async {
    FocusScope.of(context).unfocus();

    if (!_chaveDoFormulario.currentState!.validate()) {
      return;
    }

    DateTime inicioEm = DateTime(
      _data.year,
      _data.month,
      _data.day,
      _horario.hour,
      _horario.minute,
    );

    if (!_estaEditando && !inicioEm.isAfter(DateTime.now())) {
      setState(() {
        _mensagemDeErro = 'Escolha uma data e um horário futuros.';
      });
      return;
    }

    setState(() {
      _estaSalvando = true;
      _mensagemDeErro = null;
    });

    try {
      IRepositorioDeEncontros repositorio =
          ref.read(provedorDoRepositorioDeEncontros);

      if (_estaEditando) {
        await repositorio.editeEncontroAsync(
          identificador: widget.identificadorDoEncontro!,
          titulo: _controladorDoTitulo.text.trim(),
          descricao: _normalizeTextoOpcional(_controladorDaDescricao.text),
          local: _normalizeTextoOpcional(_controladorDoLocal.text),
          latitude: _latitude,
          longitude: _longitude,
          inicioEm: inicioEm,
          tipo: _tipo,
        );
      } else {
        await repositorio.crieEncontroAsync(
          titulo: _controladorDoTitulo.text.trim(),
          descricao: _normalizeTextoOpcional(_controladorDaDescricao.text),
          local: _normalizeTextoOpcional(_controladorDoLocal.text),
          latitude: _latitude,
          longitude: _longitude,
          inicioEm: inicioEm,
          tipo: _tipo,
        );
      }

      if (mounted) {
        if (_estaEditando) {
          context.pop(true);
        } else {
          await ref
              .read(provedorDoControladorDaPaginaInicial.notifier)
              .carregueAsync();

          if (mounted) {
            context.go('/inicio');
          }
        }
      }
    } on ExcecaoDaApi catch (excecao) {
      if (mounted) {
        setState(() {
          _mensagemDeErro = excecao.mensagem;
        });
      }
    } finally {
      if (mounted) {
        setState(() {
          _estaSalvando = false;
        });
      }
    }
  }

  Future<void> _selecioneTipoAsync() async {
    String? tipoSelecionado = await showModalBottomSheet<String>(
      context: context,
      backgroundColor: CoresDoAplicativo.fundoDoCartao,
      showDragHandle: true,
      isScrollControlled: true,
      builder: (BuildContext contextoDaFolha) {
        return SafeArea(
          child: SingleChildScrollView(
            padding: const EdgeInsets.fromLTRB(20, 0, 20, 20),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: <Widget>[
                Text(
                  'Qual é o tipo deste encontro?',
                  style: Theme.of(context).textTheme.titleLarge,
                ),
                const SizedBox(height: EspacamentosDoAplicativo.padrao),
                GridView.builder(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  itemCount: _opcoesDeTipoDoEncontro.length,
                  gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount: 2,
                    crossAxisSpacing: EspacamentosDoAplicativo.pequeno,
                    mainAxisSpacing: EspacamentosDoAplicativo.pequeno,
                    childAspectRatio: 2.55,
                  ),
                  itemBuilder: (BuildContext context, int indice) {
                    _OpcaoDeTipoDoEncontro opcao =
                        _opcoesDeTipoDoEncontro[indice];
                    bool opcaoEstaSelecionada = opcao.nome == _tipo;

                    return OutlinedButton.icon(
                      key: Key(
                        'tipo-do-encontro-${opcao.nome.toLowerCase()}',
                      ),
                      onPressed: () => contextoDaFolha.pop(opcao.nome),
                      style: OutlinedButton.styleFrom(
                        alignment: Alignment.centerLeft,
                        backgroundColor: opcaoEstaSelecionada
                            ? CoresDoAplicativo.verdeDestaque
                                .withValues(alpha: 0.14)
                            : null,
                        side: BorderSide(
                          color: opcaoEstaSelecionada
                              ? CoresDoAplicativo.verdeDestaque
                              : CoresDoAplicativo.bordaSuave,
                        ),
                      ),
                      icon: Icon(opcao.icone, size: 20),
                      label: Text(
                        opcao.nome,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                    );
                  },
                ),
                const SizedBox(height: EspacamentosDoAplicativo.medio),
                TextButton(
                  key: const Key('continuar-sem-tipo'),
                  onPressed: () => contextoDaFolha.pop(''),
                  child: const Text('Continuar sem tipo'),
                ),
              ],
            ),
          ),
        );
      },
    );

    if (tipoSelecionado != null && mounted) {
      setState(() {
        _tipo = tipoSelecionado.isEmpty ? null : tipoSelecionado;
      });
    }
  }

  Future<void> _obtenhaLocalizacaoAtualAsync() async {
    setState(() {
      _estaObtendoLocalizacao = true;
      _mensagemDaLocalizacao = null;
    });

    try {
      CoordenadasDoEncontro coordenadas = await ref
          .read(provedorDoServicoDeLocalizacao)
          .obtenhaPosicaoAtualAsync();

      if (!mounted) {
        return;
      }

      setState(() {
        _latitude = coordenadas.latitude;
        _longitude = coordenadas.longitude;
        _mensagemDaLocalizacao = null;
      });
    } on PermissionDeniedException {
      if (mounted) {
        setState(() {
          _mensagemDaLocalizacao =
              'Permissão negada. Autorize a localização no navegador ou escolha o ponto no mapa.';
        });
      }
    } on LocationServiceDisabledException {
      if (mounted) {
        setState(() {
          _mensagemDaLocalizacao =
              'A localização do aparelho está desativada. Ative-a ou escolha o ponto no mapa.';
        });
      }
    } on TimeoutException {
      if (mounted) {
        setState(() {
          _mensagemDaLocalizacao =
              'A localização demorou demais. Tente novamente ou escolha o ponto no mapa.';
        });
      }
    } on Object {
      if (mounted) {
        setState(() {
          _mensagemDaLocalizacao =
              'Não foi possível obter a posição atual. Escolha o ponto no mapa ou mantenha somente o texto.';
        });
      }
    } finally {
      if (mounted) {
        setState(() {
          _estaObtendoLocalizacao = false;
        });
      }
    }
  }

  Future<void> _selecioneLocalizacaoNoMapaAsync() async {
    CoordenadasDoEncontro? coordenadas =
        await abraSeletorDeLocalizacaoNoMapaAsync(
      context,
      termoInicial: _controladorDoLocal.text,
      latitudeInicial: _latitude,
      longitudeInicial: _longitude,
    );

    if (coordenadas == null || !mounted) {
      return;
    }

    setState(() {
      if (coordenadas.descricao != null) {
        _controladorDoLocal.text = coordenadas.descricao!;
      }
      _latitude = coordenadas.latitude;
      _longitude = coordenadas.longitude;
      _mensagemDaLocalizacao = null;
    });
  }

  void _aoAlterarDescricaoDoLocal(String valor) {
    setState(() {
      if (_latitude != null || _longitude != null) {
        _latitude = null;
        _longitude = null;
      }
    });
  }

  void _removaCoordenadas() {
    setState(() {
      _latitude = null;
      _longitude = null;
      _mensagemDaLocalizacao = null;
    });
  }

  Future<void> _carregueEncontroAsync() async {
    setState(() {
      _estaCarregando = true;
      _carregamentoFalhou = false;
      _mensagemDeErro = null;
    });

    try {
      EncontroDetalhado encontro = await ref
          .read(provedorDoRepositorioDeEncontros)
          .obtenhaEncontroAsync(widget.identificadorDoEncontro!);

      if (!mounted) {
        return;
      }

      setState(() {
        _controladorDoTitulo.text = encontro.titulo;
        _controladorDoLocal.text = encontro.local ?? '';
        _latitude = encontro.localizacao?.latitude;
        _longitude = encontro.localizacao?.longitude;
        _controladorDaDescricao.text = encontro.descricao ?? '';
        _data = DateTime(
          encontro.inicioEm.year,
          encontro.inicioEm.month,
          encontro.inicioEm.day,
        );
        _horario = TimeOfDay.fromDateTime(encontro.inicioEm);
        _tipo = encontro.tipo;
      });
    } on ExcecaoDaApi catch (excecao) {
      if (mounted) {
        setState(() {
          _mensagemDeErro = excecao.mensagem;
          _carregamentoFalhou = true;
        });
      }
    } finally {
      if (mounted) {
        setState(() {
          _estaCarregando = false;
        });
      }
    }
  }

  String? _normalizeTextoOpcional(String texto) {
    String textoNormalizado = texto.trim();
    return textoNormalizado.isEmpty ? null : textoNormalizado;
  }
}

class _OpcaoDeTipoDoEncontro {
  const _OpcaoDeTipoDoEncontro(this.nome, this.icone);

  final String nome;
  final IconData icone;
}

class _SeletorDoTipoDoEncontro extends StatelessWidget {
  const _SeletorDoTipoDoEncontro({
    required this.tipo,
    required this.aoSelecionar,
    required this.aoRemover,
  });

  final String? tipo;
  final VoidCallback aoSelecionar;
  final VoidCallback aoRemover;

  @override
  Widget build(BuildContext context) {
    bool temTipo = tipo != null && tipo!.trim().isNotEmpty;

    return Semantics(
      label: temTipo
          ? 'Tipo de encontro, opcional, $tipo'
          : 'Tipo de encontro, opcional, não selecionado',
      button: true,
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          key: const Key('selecionar-tipo-do-encontro'),
          onTap: aoSelecionar,
          borderRadius: BorderRadius.circular(16),
          child: InputDecorator(
            decoration: const InputDecoration(
              labelText: 'Tipo de encontro',
              prefixIcon: Icon(Icons.category_outlined),
            ),
            child: Row(
              children: <Widget>[
                Expanded(
                  child: Text(
                    temTipo ? tipo! : 'Escolher tipo (opcional)',
                    style: TextStyle(
                      color: temTipo
                          ? CoresDoAplicativo.textoPrincipal
                          : CoresDoAplicativo.textoSecundario,
                    ),
                  ),
                ),
                if (temTipo)
                  IconButton(
                    key: const Key('remover-tipo-do-encontro'),
                    tooltip: 'Remover tipo',
                    onPressed: aoRemover,
                    icon: const Icon(Icons.close_rounded),
                  )
                else
                  const Icon(Icons.expand_more_rounded),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _ControleDaLocalizacao extends StatelessWidget {
  const _ControleDaLocalizacao({
    required this.temCoordenadas,
    required this.podeAdicionar,
    required this.estaObtendo,
    required this.aoObter,
    required this.aoSelecionarNoMapa,
    required this.aoRemover,
    this.mensagem,
  });

  final bool temCoordenadas;
  final bool podeAdicionar;
  final bool estaObtendo;
  final VoidCallback aoObter;
  final VoidCallback aoSelecionarNoMapa;
  final VoidCallback aoRemover;
  final String? mensagem;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(top: EspacamentosDoAplicativo.pequeno),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: <Widget>[
          if (temCoordenadas)
            Container(
              padding: const EdgeInsets.symmetric(
                horizontal: EspacamentosDoAplicativo.medio,
                vertical: EspacamentosDoAplicativo.pequeno,
              ),
              decoration: BoxDecoration(
                color: CoresDoAplicativo.verdeDestaque.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(14),
                border: Border.all(
                  color:
                      CoresDoAplicativo.verdeDestaque.withValues(alpha: 0.35),
                ),
              ),
              child: Column(
                children: <Widget>[
                  Row(
                    children: <Widget>[
                  const Icon(
                    Icons.location_on_rounded,
                    color: CoresDoAplicativo.verdeDestaque,
                    size: 20,
                  ),
                  const SizedBox(width: EspacamentosDoAplicativo.pequeno),
                  const Expanded(
                    child: Text('Ponto fixo adicionado ao encontro'),
                  ),
                  IconButton(
                    tooltip: 'Remover ponto do mapa',
                    onPressed: aoRemover,
                    icon: const Icon(Icons.close_rounded),
                  ),
                    ],
                  ),
                  Align(
                    alignment: Alignment.centerLeft,
                    child: TextButton.icon(
                      key: const Key('conferir-ponto-no-mapa'),
                      onPressed: aoSelecionarNoMapa,
                      icon: const Icon(Icons.map_outlined),
                      label: const Text('Conferir ou alterar no mapa'),
                    ),
                  ),
                ],
              ),
            )
          else
            Wrap(
              spacing: EspacamentosDoAplicativo.pequeno,
              runSpacing: EspacamentosDoAplicativo.pequeno,
              children: <Widget>[
                OutlinedButton.icon(
                  key: const Key('selecionar-ponto-no-mapa'),
                  onPressed: podeAdicionar ? aoSelecionarNoMapa : null,
                  icon: const Icon(Icons.search_rounded),
                  label: const Text('Buscar endereço ou local'),
                ),
                OutlinedButton.icon(
                  key: const Key('usar-localizacao-atual'),
                  onPressed: estaObtendo || !podeAdicionar ? null : aoObter,
                  icon: estaObtendo
                      ? const SizedBox.square(
                          dimension: 18,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Icon(Icons.my_location_rounded),
                  label: Text(
                    estaObtendo
                        ? 'Obtendo localização...'
                        : 'Usar posição atual',
                  ),
                ),
              ],
            ),
          if (!podeAdicionar && !temCoordenadas) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.minimo),
            const Text(
              'Informe primeiro o nome ou endereço do local.',
              style: TextStyle(color: CoresDoAplicativo.textoSecundario),
            ),
          ],
          if (mensagem != null) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.pequeno),
            Text(
              mensagem!,
              style: const TextStyle(
                color: CoresDoAplicativo.coral,
                fontSize: 12,
                height: 1.3,
              ),
            ),
          ],
          const SizedBox(height: EspacamentosDoAplicativo.minimo),
          const Text(
            'A posição é capturada uma única vez e ficará visível aos convidados deste encontro.',
            style: TextStyle(
              color: CoresDoAplicativo.textoTerciario,
              fontSize: 12,
              height: 1.3,
            ),
          ),
        ],
      ),
    );
  }
}

class _Cabecalho extends StatelessWidget {
  const _Cabecalho({
    required this.estaEditando,
    required this.aoVoltar,
  });

  final bool estaEditando;
  final VoidCallback aoVoltar;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: <Widget>[
        IconButton.filledTonal(
          onPressed: aoVoltar,
          tooltip: 'Voltar',
          icon: const Icon(Icons.arrow_back_rounded),
        ),
        const SizedBox(width: EspacamentosDoAplicativo.medio),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: <Widget>[
              const Text(
                'Encontro',
                style: TextStyle(color: CoresDoAplicativo.textoSecundario),
              ),
              Text(
                estaEditando ? 'Editar encontro' : 'Novo encontro',
                style: Theme.of(context).textTheme.headlineSmall,
              ),
            ],
          ),
        ),
      ],
    );
  }
}

class _SeletorDeDataOuHorario extends StatelessWidget {
  const _SeletorDeDataOuHorario({
    required this.icone,
    required this.rotulo,
    required this.valor,
    required this.aoPressionar,
    super.key,
  });

  final IconData icone;
  final String rotulo;
  final String valor;
  final VoidCallback aoPressionar;

  @override
  Widget build(BuildContext context) {
    return OutlinedButton(
      onPressed: aoPressionar,
      style: OutlinedButton.styleFrom(
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 14),
        alignment: Alignment.centerLeft,
        side: const BorderSide(color: CoresDoAplicativo.bordaSuave),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(16),
        ),
      ),
      child: Row(
        children: <Widget>[
          Icon(icone, size: 20, color: CoresDoAplicativo.verdeDestaque),
          const SizedBox(width: EspacamentosDoAplicativo.pequeno),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: <Widget>[
                Text(
                  rotulo,
                  style: const TextStyle(
                    color: CoresDoAplicativo.textoTerciario,
                    fontSize: 11,
                  ),
                ),
                Text(
                  valor,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    color: CoresDoAplicativo.textoPrincipal,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
