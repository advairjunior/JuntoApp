import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estrutura_responsiva_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/configuracao/configuracao_do_ambiente.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/imagem_privada.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/foto_de_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/midias/audio_privado.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/midias/contrato_do_gravador_de_audio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/midias/provedor_do_gravador_de_audio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/midias/video_privado.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/raios_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/componentes/folha_de_resposta_de_presenca.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/componentes/folha_de_origem_da_imagem.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_detalhado.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/servicos/seletor_de_imagem.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/dados/repositorio_de_memorias_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/estado/controlador_da_pagina_inicial.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/pessoas_frequentes/componentes/perfil_resumido_da_pessoa.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/publicacoes/estado/controlador_dos_momentos_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/publicacoes/estado/estado_dos_momentos_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/publicacoes/modelos/publicacao_do_encontro.dart';

class TelaDeMomentosDoEncontro extends ConsumerStatefulWidget {
  const TelaDeMomentosDoEncontro({
    required this.identificadorDoEncontro,
    this.soliciteRespostaDePresenca = false,
    super.key,
  });

  final String identificadorDoEncontro;
  final bool soliciteRespostaDePresenca;

  @override
  ConsumerState<TelaDeMomentosDoEncontro> createState() =>
      _EstadoDaTelaDeMomentosDoEncontro();
}

class _EstadoDaTelaDeMomentosDoEncontro
    extends ConsumerState<TelaDeMomentosDoEncontro>
    with WidgetsBindingObserver {
  static const Duration _duracaoMaximaDoAudio = Duration(minutes: 2);
  static const List<String> _emojis = <String>[
    '😀',
    '😂',
    '❤️',
    '👏',
    '🎉',
    '🔥',
    '🙏',
    '👍',
  ];

  final TextEditingController _controladorDoTexto = TextEditingController();
  final ScrollController _controladorDaRolagem = ScrollController();
  final FocusNode _focoDoTexto = FocusNode();
  late final IGravadorDeAudio _gravadorDeAudio;
  Timer? _cronometroDoAudio;
  Duration _duracaoDoAudio = Duration.zero;
  AudioGravado? _audioPendente;
  bool _estaSolicitandoMicrofone = false;
  bool _estaGravandoAudio = false;
  bool _estaFinalizandoAudio = false;
  bool _estaEnviandoAudio = false;
  bool _seletorDeEmojiEstaVisivel = false;
  PublicacaoDoEncontro? _publicacaoSendoRespondida;
  int _quantidadeAnteriorDePublicacoes = -1;
  bool _respostaDePresencaFoiSolicitada = false;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    _gravadorDeAudio = ref.read(provedorDoGravadorDeAudio);
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    _cronometroDoAudio?.cancel();
    unawaited(_gravadorDeAudio.canceleAsync());
    _controladorDoTexto.dispose();
    _controladorDaRolagem.dispose();
    _focoDoTexto.dispose();
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState estado) {
    if (estado != AppLifecycleState.resumed && _estaGravandoAudio) {
      unawaited(_canceleGravacaoDeAudioAsync());
    }
  }

  @override
  Widget build(BuildContext context) {
    EstadoDosMomentosDoEncontro estado = ref.watch(
      provedorDoControladorDosMomentosDoEncontro(
        widget.identificadorDoEncontro,
      ),
    );

    if (estado.publicacoes.length != _quantidadeAnteriorDePublicacoes) {
      _quantidadeAnteriorDePublicacoes = estado.publicacoes.length;
      WidgetsBinding.instance.addPostFrameCallback((Duration duracao) {
        _roleParaOFinal();
      });
    }

    if (widget.soliciteRespostaDePresenca &&
        !_respostaDePresencaFoiSolicitada &&
        estado.situacao == SituacaoDosMomentosDoEncontro.carregado &&
        estado.encontro!.situacao.toLowerCase() == 'planejado') {
      _respostaDePresencaFoiSolicitada = true;
      WidgetsBinding.instance.addPostFrameCallback((Duration duracao) {
        _soliciteRespostaDePresencaAsync();
      });
    }

    return Scaffold(
      resizeToAvoidBottomInset: true,
      body: EstruturaResponsivaDoAplicativo(
        filho: DecoratedBox(
          decoration: const BoxDecoration(
            gradient: LinearGradient(
              begin: Alignment.topCenter,
              end: Alignment.bottomCenter,
              colors: <Color>[
                CoresDoAplicativo.fundoSecundario,
                CoresDoAplicativo.fundoPrincipal,
              ],
            ),
          ),
          child: SafeArea(
            child: switch (estado.situacao) {
              SituacaoDosMomentosDoEncontro.carregando =>
                const Center(child: CircularProgressIndicator()),
              SituacaoDosMomentosDoEncontro.falhou => _ErroDosMomentos(
                  mensagem: estado.mensagemDeErro ??
                      'Não foi possível abrir este encontro.',
                  aoVoltar: _volte,
                  aoTentarNovamente: () => ref
                      .read(
                        provedorDoControladorDosMomentosDoEncontro(
                          widget.identificadorDoEncontro,
                        ).notifier,
                      )
                      .carregueAsync(),
                ),
              SituacaoDosMomentosDoEncontro.carregado => Column(
                  children: <Widget>[
                    _CabecalhoDosMomentos(
                      encontro: estado.encontro!,
                      aoVoltar: _volte,
                      aoAbrirInformacoes: _abraInformacoes,
                    ),
                    if (estado.encontro!.situacao.toLowerCase() == 'cancelado')
                      const _AvisoDeEncontroCancelado(),
                    if (estado.mensagemDeErro != null)
                      _MensagemDeErro(
                        mensagem: estado.mensagemDeErro!,
                        aoTentarNovamente: () => ref
                            .read(
                              provedorDoControladorDosMomentosDoEncontro(
                                widget.identificadorDoEncontro,
                              ).notifier,
                            )
                            .carregueAsync(),
                      ),
                    Expanded(
                      child: RefreshIndicator(
                        onRefresh: () => ref
                            .read(
                              provedorDoControladorDosMomentosDoEncontro(
                                widget.identificadorDoEncontro,
                              ).notifier,
                            )
                            .carregueAsync(),
                        child: _ListaDeMomentos(
                          controladorDaRolagem: _controladorDaRolagem,
                          publicacoes: estado.publicacoes,
                          aoRemover: _confirmeRemocaoDaMemoriaAsync,
                          aoResponder: _selecionePublicacaoParaResposta,
                          aoAbrirPerfil: _abraPerfilDaPessoaAsync,
                          respostaPorGestoEstaHabilitada: true,
                        ),
                      ),
                    ),
                    if (estado.encontro!.situacao.toLowerCase() !=
                        'cancelado') ...<Widget>[
                      if (_seletorDeEmojiEstaVisivel)
                        _SeletorDeEmoji(
                          emojis: _emojis,
                          aoSelecionar: _adicioneEmoji,
                        ),
                      _CompositorDeMomento(
                        controladorDoTexto: _controladorDoTexto,
                        focoDoTexto: _focoDoTexto,
                        estaPublicando:
                            estado.estaPublicando || _estaEnviandoAudio,
                        publicacaoSendoRespondida: _publicacaoSendoRespondida,
                        gravacaoEstaDisponivel: _gravadorDeAudio.estaDisponivel,
                        estaSolicitandoMicrofone: _estaSolicitandoMicrofone,
                        estaGravandoAudio: _estaGravandoAudio,
                        estaFinalizandoAudio: _estaFinalizandoAudio,
                        duracaoDoAudio: _duracaoDoAudio,
                        audioPendente: _audioPendente,
                        aoSelecionarImagem: _selecioneMidiasAsync,
                        aoIniciarAudio: _inicieGravacaoDeAudioAsync,
                        aoCancelarAudio: _canceleAudioAsync,
                        aoEnviarAudio: _finalizeEEnvieGravacaoDeAudioAsync,
                        aoCancelarResposta: _canceleResposta,
                        aoAlternarEmojis: () {
                          setState(() {
                            _seletorDeEmojiEstaVisivel =
                                !_seletorDeEmojiEstaVisivel;
                          });
                        },
                        aoPublicar: _publiqueAsync,
                      ),
                    ],
                  ],
                ),
            },
          ),
        ),
      ),
    );
  }

  void _volte() {
    if (context.canPop()) {
      context.pop();
      return;
    }

    context.go('/inicio');
  }

  Future<void> _abraInformacoes() async {
    await context.push<void>(
      '/encontros/${widget.identificadorDoEncontro}/informacoes',
    );

    if (mounted) {
      await ref
          .read(
            provedorDoControladorDosMomentosDoEncontro(
              widget.identificadorDoEncontro,
            ).notifier,
          )
          .carregueAsync();
    }
  }

  Future<void> _soliciteRespostaDePresencaAsync() async {
    if (!mounted) {
      return;
    }

    String? situacao = await mostreFolhaDeRespostaDePresencaAsync(context);

    if (situacao == null || !mounted) {
      return;
    }

    bool respondeu = await ref
        .read(
          provedorDoControladorDosMomentosDoEncontro(
            widget.identificadorDoEncontro,
          ).notifier,
        )
        .respondaPresencaAsync(situacao);

    if (respondeu) {
      await ref
          .read(provedorDoControladorDaPaginaInicial.notifier)
          .carregueAsync();
    }
  }

  void _adicioneEmoji(String emoji) {
    String textoAtual = _controladorDoTexto.text;
    _controladorDoTexto.text =
        textoAtual.trim().isEmpty ? emoji : '${textoAtual.trimRight()} $emoji';
    _controladorDoTexto.selection = TextSelection.collapsed(
      offset: _controladorDoTexto.text.length,
    );
  }

  Future<void> _abraPerfilDaPessoaAsync(
    PublicacaoDoEncontro publicacao,
  ) async {
    await mostrePerfilResumidoDaPessoaAsync(
      context: context,
      pessoa: PessoaDoEncontro(
        identificadorDoUsuario: publicacao.identificadorDoUsuarioAutor,
        nome: publicacao.nomeDoAutor,
        urlDaFotoDePerfil: publicacao.urlDaFotoDePerfilDoAutor,
      ),
      identificadorDoEncontroAtual: widget.identificadorDoEncontro,
    );
  }

  Future<void> _publiqueAsync() async {
    String texto = _controladorDoTexto.text;
    bool campoTinhaFoco = _focoDoTexto.hasFocus;

    if (texto.trim().isEmpty) {
      return;
    }

    if (texto.trim().length > 1000) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text(
            'A publicação não pode ultrapassar 1000 caracteres.',
          ),
        ),
      );
      return;
    }

    ControladorDosMomentosDoEncontro controlador = ref.read(
      provedorDoControladorDosMomentosDoEncontro(
        widget.identificadorDoEncontro,
      ).notifier,
    );
    bool publicou = await controlador.publiqueAsync(
      texto,
      identificadorDaPublicacaoRespondida:
          _publicacaoSendoRespondida?.identificador,
    );

    if (publicou && mounted) {
      _controladorDoTexto.clear();
      setState(() {
        _seletorDeEmojiEstaVisivel = false;
        _publicacaoSendoRespondida = null;
      });
      _roleParaOFinal();

      if (campoTinhaFoco) {
        WidgetsBinding.instance.addPostFrameCallback((Duration _) {
          if (mounted) {
            _focoDoTexto.requestFocus();
          }
        });
      }
    }
  }

  Future<void> _inicieGravacaoDeAudioAsync() async {
    if (_estaSolicitandoMicrofone ||
        _estaGravandoAudio ||
        _estaFinalizandoAudio ||
        !_gravadorDeAudio.estaDisponivel) {
      return;
    }

    setState(() {
      _estaSolicitandoMicrofone = true;
      _seletorDeEmojiEstaVisivel = false;
      _publicacaoSendoRespondida = null;
      _audioPendente = null;
      _duracaoDoAudio = Duration.zero;
    });

    try {
      await _gravadorDeAudio.inicieAsync();

      if (!mounted) {
        await _gravadorDeAudio.canceleAsync();
        return;
      }

      setState(() {
        _estaSolicitandoMicrofone = false;
        _estaGravandoAudio = true;
      });
      _cronometroDoAudio?.cancel();
      _cronometroDoAudio = Timer.periodic(
        const Duration(seconds: 1),
        (Timer cronometro) {
          if (!mounted || !_estaGravandoAudio) {
            cronometro.cancel();
            return;
          }

          Duration novaDuracao = Duration(
            seconds: _duracaoDoAudio.inSeconds + 1,
          );
          setState(() {
            _duracaoDoAudio = novaDuracao;
          });

          if (novaDuracao >= _duracaoMaximaDoAudio) {
            cronometro.cancel();
            unawaited(_finalizeEEnvieGravacaoDeAudioAsync());
          }
        },
      );
    } on Object {
      if (!mounted) {
        return;
      }

      setState(() {
        _estaSolicitandoMicrofone = false;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text(
            'Não foi possível acessar o microfone. Verifique a permissão do navegador.',
          ),
        ),
      );
    }
  }

  Future<void> _finalizeGravacaoDeAudioAsync() async {
    if (!_estaGravandoAudio || _estaFinalizandoAudio) {
      return;
    }

    _cronometroDoAudio?.cancel();
    setState(() {
      _estaGravandoAudio = false;
      _estaFinalizandoAudio = true;
    });

    try {
      AudioGravado? audio = await _gravadorDeAudio.finalizeAsync(
        _duracaoDoAudio,
      );

      if (!mounted) {
        return;
      }

      setState(() {
        _audioPendente = audio;
        _estaFinalizandoAudio = false;
      });

      if (audio == null) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Nenhum áudio foi gravado.')),
        );
      }
    } on Object {
      if (!mounted) {
        return;
      }

      setState(() {
        _estaFinalizandoAudio = false;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Não foi possível finalizar o áudio.')),
      );
    }
  }

  Future<void> _canceleAudioAsync() async {
    if (_estaGravandoAudio || _estaSolicitandoMicrofone) {
      await _canceleGravacaoDeAudioAsync();
      return;
    }

    if (mounted) {
      setState(() {
        _audioPendente = null;
        _duracaoDoAudio = Duration.zero;
      });
    }
  }

  Future<void> _canceleGravacaoDeAudioAsync() async {
    _cronometroDoAudio?.cancel();

    try {
      await _gravadorDeAudio.canceleAsync();
    } on Object {
      // O cancelamento sempre deve devolver a interface ao estado inicial.
    }

    if (!mounted) {
      return;
    }

    setState(() {
      _estaSolicitandoMicrofone = false;
      _estaGravandoAudio = false;
      _estaFinalizandoAudio = false;
      _audioPendente = null;
      _duracaoDoAudio = Duration.zero;
    });
  }

  Future<void> _envieAudioAsync() async {
    AudioGravado? audio = _audioPendente;

    if (audio == null || _estaEnviandoAudio) {
      return;
    }

    setState(() {
      _estaEnviandoAudio = true;
    });
    MidiaSelecionada midia = MidiaSelecionada(
      nome: audio.nomeDoArquivo,
      tipoDeConteudo: audio.tipoDeConteudo,
      conteudo: audio.bytes,
    );
    bool publicou = false;

    try {
      publicou = await ref
          .read(
        provedorDoControladorDosMomentosDoEncontro(
          widget.identificadorDoEncontro,
        ).notifier,
      )
          .publiqueMidiasAsync(<MidiaSelecionada>[midia], '');
    } finally {
      if (mounted) {
        setState(() {
          _estaEnviandoAudio = false;
        });
      }
    }

    if (!mounted) {
      return;
    }

    if (publicou) {
      setState(() {
        _audioPendente = null;
        _duracaoDoAudio = Duration.zero;
      });
      _roleParaOFinal();
      return;
    }

    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('O áudio não foi enviado. Você pode tentar novamente.'),
      ),
    );
  }

  Future<void> _finalizeEEnvieGravacaoDeAudioAsync() async {
    if (_estaFinalizandoAudio || _estaEnviandoAudio) {
      return;
    }

    if (_estaGravandoAudio) {
      await _finalizeGravacaoDeAudioAsync();
    }

    if (_audioPendente != null) {
      await _envieAudioAsync();
    }
  }

  Future<void> _selecioneMidiasAsync() async {
    EnumeradorDeOrigemDaImagem? origem = await escolhaOrigemDaImagemAsync(
      context,
      titulo: 'Fotos e vídeos do encontro',
    );

    if (origem == null || !mounted) {
      return;
    }

    List<MidiaSelecionada> midias = await ref
        .read(provedorDoSeletorDeImagem)
        .selecioneMidiasPorOrigemAsync(origem);

    if (!mounted || midias.isEmpty) {
      return;
    }

    if (midias.length > 10) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Selecione no máximo 10 mídias por publicação.'),
        ),
      );
      return;
    }

    if (midias.any(
      (MidiaSelecionada midia) =>
          midia.conteudo.lengthInBytes > 10 * 1024 * 1024,
    )) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Cada foto ou vídeo pode ter no máximo 10 MB.'),
        ),
      );
      return;
    }

    String legendaInicial = _controladorDoTexto.text;
    bool? publicou = await showDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (BuildContext context) {
        return DialogoDeNovaPublicacao(
          midias: midias,
          legendaInicial: legendaInicial,
          aoPublicar: (String legenda) {
            return ref
                .read(
                  provedorDoControladorDosMomentosDoEncontro(
                    widget.identificadorDoEncontro,
                  ).notifier,
                )
                .publiqueMidiasAsync(midias, legenda);
          },
        );
      },
    );

    if (publicou == true && mounted) {
      _controladorDoTexto.clear();
      setState(() {
        _publicacaoSendoRespondida = null;
        _seletorDeEmojiEstaVisivel = false;
      });
      ref.invalidate(
        provedorDasMemoriasDoEncontro(widget.identificadorDoEncontro),
      );
      _roleParaOFinal();
    }
  }

  void _selecionePublicacaoParaResposta(PublicacaoDoEncontro publicacao) {
    setState(() {
      _publicacaoSendoRespondida = publicacao;
      _seletorDeEmojiEstaVisivel = false;
    });
    _focoDoTexto.requestFocus();
  }

  void _canceleResposta() {
    setState(() {
      _publicacaoSendoRespondida = null;
    });
  }

  Future<bool> _confirmeRemocaoDaMemoriaAsync(
    PublicacaoDoEncontro publicacao,
  ) async {
    bool confirmou = await showDialog<bool>(
          context: context,
          builder: (BuildContext contextoDoDialogo) {
            return AlertDialog(
              title: const Text('Remover foto?'),
              content: const Text(
                'A foto será removida do mural e da galeria deste encontro.',
              ),
              actions: <Widget>[
                TextButton(
                  onPressed: () => Navigator.of(contextoDoDialogo).pop(false),
                  child: const Text('Cancelar'),
                ),
                FilledButton(
                  key: const Key('confirmar-remocao-da-memoria'),
                  onPressed: () => Navigator.of(contextoDoDialogo).pop(true),
                  child: const Text('Remover'),
                ),
              ],
            );
          },
        ) ??
        false;

    if (!confirmou || !mounted) {
      return false;
    }

    bool removeu = await ref
        .read(
          provedorDoControladorDosMomentosDoEncontro(
            widget.identificadorDoEncontro,
          ).notifier,
        )
        .removaMemoriaAsync(publicacao.identificador);

    if (removeu) {
      ref.invalidate(
        provedorDasMemoriasDoEncontro(widget.identificadorDoEncontro),
      );
    }

    return removeu;
  }

  void _roleParaOFinal() {
    if (!_controladorDaRolagem.hasClients) {
      return;
    }

    _controladorDaRolagem.animateTo(
      _controladorDaRolagem.position.maxScrollExtent,
      duration: const Duration(milliseconds: 250),
      curve: Curves.easeOut,
    );
  }
}

class _CabecalhoDosMomentos extends StatelessWidget {
  const _CabecalhoDosMomentos({
    required this.encontro,
    required this.aoVoltar,
    required this.aoAbrirInformacoes,
  });

  final EncontroDetalhado encontro;
  final VoidCallback aoVoltar;
  final VoidCallback aoAbrirInformacoes;

  @override
  Widget build(BuildContext context) {
    String complemento = <String>[
      DateFormat('dd/MM/yyyy • HH:mm').format(encontro.inicioEm),
      if (encontro.local != null && encontro.local!.trim().isNotEmpty)
        encontro.local!,
    ].join(' • ');

    return Material(
      color: CoresDoAplicativo.fundoPrincipal.withValues(alpha: 0.94),
      child: DecoratedBox(
        decoration: const BoxDecoration(
          border: Border(
            bottom: BorderSide(color: CoresDoAplicativo.bordaDiscreta),
          ),
        ),
        child: Padding(
          padding: const EdgeInsets.symmetric(
            horizontal: EspacamentosDoAplicativo.pequeno,
            vertical: EspacamentosDoAplicativo.pequeno,
          ),
          child: Row(
            children: <Widget>[
              IconButton(
                tooltip: 'Voltar',
                onPressed: aoVoltar,
                icon: const Icon(Icons.arrow_back_ios_new_rounded),
              ),
              Expanded(
                child: InkWell(
                  key: const Key('abrir-informacoes-do-encontro'),
                  borderRadius: BorderRadius.circular(
                    RaiosDoAplicativo.pequeno,
                  ),
                  onTap: aoAbrirInformacoes,
                  child: Padding(
                    padding: const EdgeInsets.symmetric(vertical: 4),
                    child: Row(
                      children: <Widget>[
                        _MiniaturaDoEncontro(encontro: encontro),
                        const SizedBox(width: EspacamentosDoAplicativo.medio),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: <Widget>[
                              Text(
                                encontro.titulo,
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis,
                                style: Theme.of(context)
                                    .textTheme
                                    .titleMedium
                                    ?.copyWith(fontWeight: FontWeight.w700),
                              ),
                              const SizedBox(height: 2),
                              Text(
                                complemento,
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis,
                                style: const TextStyle(
                                  color: CoresDoAplicativo.textoSecundario,
                                  fontSize: 12,
                                ),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
              IconButton(
                tooltip: 'Informações do encontro',
                onPressed: aoAbrirInformacoes,
                icon: const Icon(Icons.info_outline_rounded),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _MiniaturaDoEncontro extends StatelessWidget {
  const _MiniaturaDoEncontro({required this.encontro});

  final EncontroDetalhado encontro;

  @override
  Widget build(BuildContext context) {
    String url = ConfiguracaoDoAmbiente.crieUrlAbsoluta(
      encontro.urlDaImagemDeCapa,
    );
    double dimensao = MediaQuery.sizeOf(context).width <= 360 ? 40 : 48;

    return ClipRRect(
      borderRadius: BorderRadius.circular(RaiosDoAplicativo.medio),
      child: SizedBox.square(
        dimension: dimensao,
        child: url.isEmpty
            ? const ColoredBox(
                color: CoresDoAplicativo.fundoDoCartaoSuave,
                child: Icon(Icons.people_alt_outlined),
              )
            : ImagemPrivada(
                recurso: url,
                construaSubstituta: (_) => const ColoredBox(
                  color: CoresDoAplicativo.fundoDoCartaoSuave,
                  child: Icon(Icons.people_alt_outlined),
                ),
              ),
      ),
    );
  }
}

class _ListaDeMomentos extends StatelessWidget {
  const _ListaDeMomentos({
    required this.controladorDaRolagem,
    required this.publicacoes,
    required this.aoRemover,
    required this.aoResponder,
    required this.aoAbrirPerfil,
    required this.respostaPorGestoEstaHabilitada,
  });

  final ScrollController controladorDaRolagem;
  final List<PublicacaoDoEncontro> publicacoes;
  final Future<bool> Function(PublicacaoDoEncontro publicacao) aoRemover;
  final ValueChanged<PublicacaoDoEncontro> aoResponder;
  final ValueChanged<PublicacaoDoEncontro> aoAbrirPerfil;
  final bool respostaPorGestoEstaHabilitada;

  @override
  Widget build(BuildContext context) {
    if (publicacoes.isEmpty) {
      return ListView(
        controller: controladorDaRolagem,
        physics: const AlwaysScrollableScrollPhysics(),
        padding: const EdgeInsets.all(EspacamentosDoAplicativo.grande),
        children: const <Widget>[
          SizedBox(height: 96),
          EstadoVazio(
            icone: Icons.forum_outlined,
            titulo: 'Ainda não há momentos por aqui.',
            descricao:
                'Compartilhe uma mensagem ou foto para começar a história deste encontro.',
          ),
        ],
      );
    }

    return ListView.separated(
      controller: controladorDaRolagem,
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.symmetric(
        horizontal: EspacamentosDoAplicativo.padrao,
        vertical: EspacamentosDoAplicativo.padrao,
      ),
      itemCount: publicacoes.length,
      separatorBuilder: (BuildContext context, int indice) =>
          const SizedBox(height: EspacamentosDoAplicativo.pequeno),
      itemBuilder: (BuildContext context, int indice) {
        PublicacaoDoEncontro publicacao = publicacoes[indice];

        if (publicacao.ehAtualizacaoDoSistema) {
          return _AtualizacaoDoSistema(publicacao: publicacao);
        }

        return _MomentoDeParticipante(
          publicacao: publicacao,
          aoRemover: aoRemover,
          aoResponder: aoResponder,
          aoAbrirPerfil: aoAbrirPerfil,
          respostaPorGestoEstaHabilitada: respostaPorGestoEstaHabilitada,
        );
      },
    );
  }
}

class _AtualizacaoDoSistema extends StatelessWidget {
  const _AtualizacaoDoSistema({required this.publicacao});

  final PublicacaoDoEncontro publicacao;

  @override
  Widget build(BuildContext context) {
    return Center(
      key: Key('publicacao-${publicacao.identificador}'),
      child: Container(
        constraints: const BoxConstraints(maxWidth: 420),
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 7),
        decoration: BoxDecoration(
          color: CoresDoAplicativo.fundoSecundario,
          borderRadius: BorderRadius.circular(16),
        ),
        child: Text(
          '${publicacao.texto ?? ''} • ${DateFormat('dd/MM HH:mm').format(publicacao.publicadoEm)}',
          textAlign: TextAlign.center,
          style: const TextStyle(
            color: CoresDoAplicativo.textoTerciario,
            fontSize: 12,
          ),
        ),
      ),
    );
  }
}

class _MomentoDeParticipante extends StatelessWidget {
  const _MomentoDeParticipante({
    required this.publicacao,
    required this.aoRemover,
    required this.aoResponder,
    required this.aoAbrirPerfil,
    required this.respostaPorGestoEstaHabilitada,
  });

  final PublicacaoDoEncontro publicacao;
  final Future<bool> Function(PublicacaoDoEncontro publicacao) aoRemover;
  final ValueChanged<PublicacaoDoEncontro> aoResponder;
  final ValueChanged<PublicacaoDoEncontro> aoAbrirPerfil;
  final bool respostaPorGestoEstaHabilitada;

  bool get _priorizeRespostaPorGesto {
    return defaultTargetPlatform == TargetPlatform.android ||
        defaultTargetPlatform == TargetPlatform.iOS ||
        defaultTargetPlatform == TargetPlatform.fuchsia;
  }

  @override
  Widget build(BuildContext context) {
    Widget balao = Container(
      key: Key('publicacao-${publicacao.identificador}'),
      constraints: const BoxConstraints(maxWidth: 430),
      padding: const EdgeInsets.symmetric(
        horizontal: EspacamentosDoAplicativo.medio,
        vertical: 10,
      ),
      decoration: BoxDecoration(
        color: publicacao.usuarioAtual
            ? CoresDoAplicativo.fundoDaMensagemAtual
            : CoresDoAplicativo.fundoDoCartao,
        borderRadius: BorderRadius.only(
          topLeft: const Radius.circular(RaiosDoAplicativo.medio),
          topRight: const Radius.circular(RaiosDoAplicativo.medio),
          bottomLeft: Radius.circular(
            publicacao.usuarioAtual ? RaiosDoAplicativo.medio : 4,
          ),
          bottomRight: Radius.circular(
            publicacao.usuarioAtual ? 4 : RaiosDoAplicativo.medio,
          ),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: <Widget>[
          Row(
            mainAxisSize: MainAxisSize.min,
            children: <Widget>[
              Flexible(
                child: Text(
                  publicacao.usuarioAtual ? 'Você' : publicacao.nomeDoAutor,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(
                    color: publicacao.usuarioAtual
                        ? CoresDoAplicativo.textoSecundario
                        : CoresDoAplicativo.ambar,
                    fontSize: 14,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
              const SizedBox(width: EspacamentosDoAplicativo.medio),
              Text(
                DateFormat('dd/MM HH:mm').format(publicacao.publicadoEm),
                style: const TextStyle(
                  color: CoresDoAplicativo.textoTerciario,
                  fontSize: 12,
                ),
              ),
              if (!_priorizeRespostaPorGesto) ...<Widget>[
                const SizedBox(width: EspacamentosDoAplicativo.minimo),
                IconButton(
                  key: Key(
                    'responder-publicacao-${publicacao.identificador}',
                  ),
                  tooltip:
                      'Responder à publicação de ${publicacao.nomeDoAutor}',
                  visualDensity: VisualDensity.compact,
                  constraints: const BoxConstraints(
                    minWidth: 48,
                    minHeight: 48,
                  ),
                  onPressed: () => aoResponder(publicacao),
                  icon: const Icon(Icons.reply_rounded, size: 18),
                ),
              ],
            ],
          ),
          if (publicacao.publicacaoRespondida != null) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.minimo),
            _TrechoDaPublicacaoRespondida(
              publicacaoRespondida: publicacao.publicacaoRespondida!,
            ),
          ],
          if (publicacao.texto != null &&
              publicacao.texto!.trim().isNotEmpty) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.minimo),
            Text(publicacao.texto!),
          ],
          if (publicacao.ehImagem) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.pequeno),
            _ImagemDoMomento(
              publicacao: publicacao,
              aoRemover: () => aoRemover(publicacao),
            ),
          ],
          if (publicacao.ehAudio) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.pequeno),
            _AudioDoMomento(
              publicacao: publicacao,
              aoRemover: () => aoRemover(publicacao),
            ),
          ],
        ],
      ),
    );

    return GestureDetector(
      behavior: HitTestBehavior.opaque,
      onLongPress: () => aoResponder(publicacao),
      child: Row(
        mainAxisAlignment: publicacao.usuarioAtual
            ? MainAxisAlignment.end
            : MainAxisAlignment.start,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: <Widget>[
          if (!publicacao.usuarioAtual) ...<Widget>[
            _AvatarDoAutor(
              publicacao: publicacao,
              aoTocar: () => aoAbrirPerfil(publicacao),
            ),
            const SizedBox(width: EspacamentosDoAplicativo.pequeno),
          ],
          Flexible(
            child: _BalaoDeslizavelParaResposta(
              identificadorDaPublicacao: publicacao.identificador,
              estaHabilitado: respostaPorGestoEstaHabilitada,
              aoResponder: () => aoResponder(publicacao),
              filho: balao,
            ),
          ),
        ],
      ),
    );
  }
}

class _BalaoDeslizavelParaResposta extends StatefulWidget {
  const _BalaoDeslizavelParaResposta({
    required this.identificadorDaPublicacao,
    required this.estaHabilitado,
    required this.aoResponder,
    required this.filho,
  });

  final String identificadorDaPublicacao;
  final bool estaHabilitado;
  final VoidCallback aoResponder;
  final Widget filho;

  @override
  State<_BalaoDeslizavelParaResposta> createState() =>
      _EstadoDoBalaoDeslizavelParaResposta();
}

class _EstadoDoBalaoDeslizavelParaResposta
    extends State<_BalaoDeslizavelParaResposta> {
  static const double _deslocamentoMaximo = 72;
  static const double _limiteParaResponder = 48;
  static const double _margemDoGestoDoSistema = 24;
  static const Duration _duracaoDoRetorno = Duration(milliseconds: 160);

  double _deslocamento = 0;
  bool _estaArrastando = false;
  bool _inicioDoGestoEhValido = false;

  @override
  Widget build(BuildContext context) {
    double progresso = (_deslocamento / _limiteParaResponder).clamp(0, 1);

    return Stack(
      clipBehavior: Clip.none,
      alignment: Alignment.centerLeft,
      children: <Widget>[
        IgnorePointer(
          child: Opacity(
            opacity: progresso,
            child: Transform.scale(
              scale: 0.72 + (0.28 * progresso),
              child: const Padding(
                padding: EdgeInsets.only(left: 10),
                child: Icon(
                  Icons.reply_rounded,
                  color: CoresDoAplicativo.azulInteracao,
                  size: 24,
                ),
              ),
            ),
          ),
        ),
        GestureDetector(
          key: Key(
            'deslizar-para-responder-${widget.identificadorDaPublicacao}',
          ),
          behavior: HitTestBehavior.translucent,
          dragStartBehavior: DragStartBehavior.down,
          supportedDevices: const <PointerDeviceKind>{
            PointerDeviceKind.touch,
            PointerDeviceKind.stylus,
          },
          onHorizontalDragStart: widget.estaHabilitado
              ? (DragStartDetails detalhes) {
                  setState(() {
                    _inicioDoGestoEhValido =
                        detalhes.globalPosition.dx >= _margemDoGestoDoSistema;
                    _estaArrastando = _inicioDoGestoEhValido;
                  });
                }
              : null,
          onHorizontalDragUpdate: widget.estaHabilitado
              ? (DragUpdateDetails detalhes) {
                  if (!_inicioDoGestoEhValido) {
                    return;
                  }

                  double deslocamentoAtualizado =
                      _deslocamento + detalhes.delta.dx;
                  setState(() {
                    _deslocamento = deslocamentoAtualizado.clamp(
                      0,
                      _deslocamentoMaximo,
                    );
                  });
                }
              : null,
          onHorizontalDragCancel:
              widget.estaHabilitado ? _retorneParaOrigem : null,
          onHorizontalDragEnd: widget.estaHabilitado
              ? (_) {
                  if (!_inicioDoGestoEhValido) {
                    _retorneParaOrigem();
                    return;
                  }

                  bool deveResponder = _deslocamento >= _limiteParaResponder;
                  _retorneParaOrigem();

                  if (deveResponder) {
                    HapticFeedback.selectionClick().catchError((Object _) {});
                    widget.aoResponder();
                  }
                }
              : null,
          child: AnimatedContainer(
            duration: _estaArrastando || MediaQuery.disableAnimationsOf(context)
                ? Duration.zero
                : _duracaoDoRetorno,
            curve: Curves.easeOutCubic,
            transform: Matrix4.translationValues(_deslocamento, 0, 0),
            child: widget.filho,
          ),
        ),
      ],
    );
  }

  void _retorneParaOrigem() {
    setState(() {
      _estaArrastando = false;
      _inicioDoGestoEhValido = false;
      _deslocamento = 0;
    });
  }
}

class _TrechoDaPublicacaoRespondida extends StatelessWidget {
  const _TrechoDaPublicacaoRespondida({
    required this.publicacaoRespondida,
    this.exibaAcaoDeCancelar = false,
    this.aoCancelar,
  });

  final PublicacaoRespondida publicacaoRespondida;
  final bool exibaAcaoDeCancelar;
  final VoidCallback? aoCancelar;

  @override
  Widget build(BuildContext context) {
    String conteudo = publicacaoRespondida.foiRemovida
        ? 'Mensagem removida'
        : publicacaoRespondida.texto?.trim().isNotEmpty == true
            ? publicacaoRespondida.texto!.trim()
            : publicacaoRespondida.temMidia
                ? 'Mídia'
                : 'Publicação';

    String descricaoSemantica =
        exibaAcaoDeCancelar ? 'Respondendo a' : 'Resposta à publicação de';

    return Semantics(
      label:
          '$descricaoSemantica ${publicacaoRespondida.nomeDoAutor}: $conteudo',
      child: Container(
        key: Key(
          'trecho-publicacao-respondida-${publicacaoRespondida.identificador}',
        ),
        width: double.infinity,
        constraints: const BoxConstraints(minHeight: 54),
        padding: const EdgeInsets.only(left: 10, top: 7, bottom: 7),
        decoration: const BoxDecoration(
          color: CoresDoAplicativo.fundoSecundario,
          border: Border(
            left: BorderSide(
              color: CoresDoAplicativo.azulInteracao,
              width: 3,
            ),
          ),
        ),
        child: Row(
          children: <Widget>[
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                children: <Widget>[
                  Text(
                    publicacaoRespondida.nomeDoAutor,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      color: CoresDoAplicativo.azulInteracao,
                      fontWeight: FontWeight.w700,
                      fontSize: 13,
                    ),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    conteudo,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      color: CoresDoAplicativo.textoSecundario,
                      fontSize: 12,
                    ),
                  ),
                ],
              ),
            ),
            if (exibaAcaoDeCancelar)
              IconButton(
                key: const Key('cancelar-resposta'),
                tooltip: 'Cancelar resposta',
                onPressed: aoCancelar,
                icon: const Icon(Icons.close_rounded, size: 20),
              ),
          ],
        ),
      ),
    );
  }
}

class _AudioDoMomento extends StatelessWidget {
  const _AudioDoMomento({
    required this.publicacao,
    required this.aoRemover,
  });

  final PublicacaoDoEncontro publicacao;
  final Future<bool> Function() aoRemover;

  @override
  Widget build(BuildContext context) {
    String recurso = ConfiguracaoDoAmbiente.crieUrlAbsoluta(
      publicacao.urlDaMidia,
    );

    return Row(
      children: <Widget>[
        const Icon(
          Icons.mic_rounded,
          color: CoresDoAplicativo.azulInteracao,
        ),
        const SizedBox(width: EspacamentosDoAplicativo.pequeno),
        Expanded(
          child: AudioPrivado(
            recurso: recurso,
            tipoDeConteudo: publicacao.tipoDeConteudoDaMidia ?? 'audio/webm',
          ),
        ),
        if (publicacao.usuarioAtual)
          IconButton(
            key: Key('remover-audio-${publicacao.identificador}'),
            tooltip: 'Remover áudio',
            onPressed: () => aoRemover(),
            icon: const Icon(Icons.delete_outline_rounded, size: 20),
          ),
      ],
    );
  }
}

class _ImagemDoMomento extends StatelessWidget {
  const _ImagemDoMomento({
    required this.publicacao,
    required this.aoRemover,
  });

  final PublicacaoDoEncontro publicacao;
  final Future<bool> Function() aoRemover;

  @override
  Widget build(BuildContext context) {
    String recurso = ConfiguracaoDoAmbiente.crieUrlAbsoluta(
      publicacao.urlDaMidia,
    );

    return Semantics(
      button: true,
      label: 'Ampliar foto compartilhada por ${publicacao.nomeDoAutor}',
      child: InkWell(
        key: Key('abrir-midia-${publicacao.identificador}'),
        onTap: () => _abraImagemAmpliada(context),
        borderRadius: BorderRadius.circular(12),
        child: ClipRRect(
          borderRadius: BorderRadius.circular(12),
          child: AspectRatio(
            aspectRatio: 4 / 3,
            child: ImagemPrivada(
              recurso: recurso,
              construaSubstituta: (_) => const ColoredBox(
                color: CoresDoAplicativo.fundoSecundario,
                child: Center(
                  child: Icon(
                    Icons.broken_image_outlined,
                    color: CoresDoAplicativo.textoTerciario,
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  Future<void> _abraImagemAmpliada(BuildContext context) {
    String recurso = ConfiguracaoDoAmbiente.crieUrlAbsoluta(
      publicacao.urlDaMidia,
    );

    return showDialog<void>(
      context: context,
      useSafeArea: false,
      builder: (BuildContext context) {
        return Dialog.fullscreen(
          backgroundColor: Colors.black,
          child: Scaffold(
            backgroundColor: Colors.black,
            appBar: AppBar(
              backgroundColor: Colors.black,
              foregroundColor: Colors.white,
              title: Text(
                publicacao.usuarioAtual ? 'Sua foto' : publicacao.nomeDoAutor,
              ),
              leading: IconButton(
                tooltip: 'Fechar',
                onPressed: () => Navigator.of(context).pop(),
                icon: const Icon(Icons.close_rounded),
              ),
              actions: <Widget>[
                if (publicacao.usuarioAtual)
                  IconButton(
                    key: const Key('remover-memoria'),
                    tooltip: 'Remover foto',
                    onPressed: () async {
                      bool removeu = await aoRemover();

                      if (removeu && context.mounted) {
                        Navigator.of(context).pop();
                      }
                    },
                    icon: const Icon(Icons.delete_outline_rounded),
                  ),
              ],
            ),
            body: _VisualizadorDaImagemDoMomento(
              recurso: recurso,
              legenda: publicacao.texto,
            ),
          ),
        );
      },
    );
  }
}

class _VisualizadorDaImagemDoMomento extends StatefulWidget {
  const _VisualizadorDaImagemDoMomento({
    required this.recurso,
    required this.legenda,
  });

  final String recurso;
  final String? legenda;

  @override
  State<_VisualizadorDaImagemDoMomento> createState() =>
      _EstadoDoVisualizadorDaImagemDoMomento();
}

class _EstadoDoVisualizadorDaImagemDoMomento
    extends State<_VisualizadorDaImagemDoMomento> {
  static const double _distanciaMinimaParaFechar = 110;
  static const double _distanciaMaximaDoArraste = 280;
  static const double _margemParaIdentificarDirecao = 8;

  final TransformationController _controladorDaTransformacao =
      TransformationController();
  int? _ponteiroAtivo;
  Offset? _posicaoInicial;
  double _deslocamentoVertical = 0;
  double _escalaAtual = 1;
  bool _gestoEhVertical = false;
  bool _gestoFoiDescartado = false;
  bool _estaArrastando = false;

  bool get _imagemEstaAmpliada => _escalaAtual > 1.02;

  @override
  void dispose() {
    _controladorDaTransformacao.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    double progressoDoFechamento =
        (_deslocamentoVertical / _distanciaMaximaDoArraste).clamp(0, 1);

    return ColoredBox(
      color: Color.lerp(
        Colors.black,
        Colors.transparent,
        progressoDoFechamento * 0.75,
      )!,
      child: Listener(
        key: const Key('visualizador-da-imagem-do-feed'),
        onPointerDown: _inicieGesto,
        onPointerMove: _acompanheGesto,
        onPointerUp: _finalizeGesto,
        onPointerCancel: _canceleGesto,
        child: AnimatedContainer(
          duration: _estaArrastando
              ? Duration.zero
              : const Duration(milliseconds: 180),
          curve: Curves.easeOutCubic,
          transform: Matrix4.translationValues(0, _deslocamentoVertical, 0),
          child: Column(
            children: <Widget>[
              Expanded(
                child: InteractiveViewer(
                  transformationController: _controladorDaTransformacao,
                  minScale: 1,
                  maxScale: 4,
                  panEnabled: _imagemEstaAmpliada,
                  onInteractionUpdate: (_) {
                    double escala =
                        _controladorDaTransformacao.value.getMaxScaleOnAxis();

                    if ((escala - _escalaAtual).abs() > 0.001) {
                      setState(() {
                        _escalaAtual = escala;
                      });
                    }
                  },
                  child: Center(
                    child: ImagemPrivada(
                      recurso: widget.recurso,
                      ajuste: BoxFit.contain,
                      construaSubstituta: (_) => const Icon(
                        Icons.broken_image_outlined,
                        size: 52,
                        color: Colors.white54,
                      ),
                    ),
                  ),
                ),
              ),
              if (widget.legenda != null && widget.legenda!.trim().isNotEmpty)
                SafeArea(
                  top: false,
                  child: Padding(
                    padding: const EdgeInsets.all(
                      EspacamentosDoAplicativo.padrao,
                    ),
                    child: Text(
                      widget.legenda!,
                      style: const TextStyle(color: Colors.white),
                    ),
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }

  void _inicieGesto(PointerDownEvent evento) {
    if (_imagemEstaAmpliada || _ponteiroAtivo != null) {
      _gestoFoiDescartado = true;
      return;
    }

    _ponteiroAtivo = evento.pointer;
    _posicaoInicial = evento.position;
    _gestoEhVertical = false;
    _gestoFoiDescartado = false;
  }

  void _acompanheGesto(PointerMoveEvent evento) {
    if (evento.pointer != _ponteiroAtivo ||
        _posicaoInicial == null ||
        _imagemEstaAmpliada ||
        _gestoFoiDescartado) {
      return;
    }

    Offset diferenca = evento.position - _posicaoInicial!;

    if (!_gestoEhVertical) {
      if (diferenca.distance < _margemParaIdentificarDirecao) {
        return;
      }

      if (diferenca.dy <= 0 || diferenca.dy.abs() <= diferenca.dx.abs() * 1.2) {
        _gestoFoiDescartado = true;
        return;
      }

      _gestoEhVertical = true;
    }

    setState(() {
      _estaArrastando = true;
      _deslocamentoVertical = diferenca.dy.clamp(0, _distanciaMaximaDoArraste);
    });
  }

  void _finalizeGesto(PointerUpEvent evento) {
    if (evento.pointer != _ponteiroAtivo) {
      return;
    }

    bool deveFechar =
        _gestoEhVertical && _deslocamentoVertical >= _distanciaMinimaParaFechar;
    _limpeGesto();

    if (deveFechar) {
      Navigator.of(context).pop();
      return;
    }

    setState(() {
      _estaArrastando = false;
      _deslocamentoVertical = 0;
    });
  }

  void _canceleGesto(PointerCancelEvent evento) {
    if (evento.pointer != _ponteiroAtivo) {
      return;
    }

    _limpeGesto();
    setState(() {
      _estaArrastando = false;
      _deslocamentoVertical = 0;
    });
  }

  void _limpeGesto() {
    _ponteiroAtivo = null;
    _posicaoInicial = null;
    _gestoEhVertical = false;
    _gestoFoiDescartado = false;
  }
}

class _AvatarDoAutor extends StatelessWidget {
  const _AvatarDoAutor({
    required this.publicacao,
    required this.aoTocar,
  });

  final PublicacaoDoEncontro publicacao;
  final VoidCallback aoTocar;

  @override
  Widget build(BuildContext context) {
    String inicial = publicacao.nomeDoAutor.trim().isEmpty
        ? '?'
        : publicacao.nomeDoAutor.trim()[0].toUpperCase();

    return Semantics(
      button: true,
      label: 'Abrir perfil de ${publicacao.nomeDoAutor}',
      child: Tooltip(
        message: 'Ver perfil de ${publicacao.nomeDoAutor}',
        child: GestureDetector(
          key: Key(
            'abrir-perfil-${publicacao.identificadorDoUsuarioAutor}',
          ),
          behavior: HitTestBehavior.opaque,
          onTap: aoTocar,
          child: SizedBox.square(
            dimension: 48,
            child: Center(
              child: FotoDePerfil(
                key: Key(
                  'foto-do-feed-${publicacao.identificadorDoUsuarioAutor}',
                ),
                url: publicacao.urlDaFotoDePerfilDoAutor,
                iniciais: inicial,
                dimensao: 32,
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _CompositorDeMomento extends StatefulWidget {
  const _CompositorDeMomento({
    required this.controladorDoTexto,
    required this.focoDoTexto,
    required this.estaPublicando,
    required this.publicacaoSendoRespondida,
    required this.gravacaoEstaDisponivel,
    required this.estaSolicitandoMicrofone,
    required this.estaGravandoAudio,
    required this.estaFinalizandoAudio,
    required this.duracaoDoAudio,
    required this.audioPendente,
    required this.aoSelecionarImagem,
    required this.aoIniciarAudio,
    required this.aoCancelarAudio,
    required this.aoEnviarAudio,
    required this.aoCancelarResposta,
    required this.aoAlternarEmojis,
    required this.aoPublicar,
  });

  final TextEditingController controladorDoTexto;
  final FocusNode focoDoTexto;
  final bool estaPublicando;
  final PublicacaoDoEncontro? publicacaoSendoRespondida;
  final bool gravacaoEstaDisponivel;
  final bool estaSolicitandoMicrofone;
  final bool estaGravandoAudio;
  final bool estaFinalizandoAudio;
  final Duration duracaoDoAudio;
  final AudioGravado? audioPendente;
  final VoidCallback aoSelecionarImagem;
  final Future<void> Function() aoIniciarAudio;
  final Future<void> Function() aoCancelarAudio;
  final Future<void> Function() aoEnviarAudio;
  final VoidCallback aoCancelarResposta;
  final VoidCallback aoAlternarEmojis;
  final VoidCallback aoPublicar;

  @override
  State<_CompositorDeMomento> createState() => _EstadoDoCompositorDeMomento();
}

class _EstadoDoCompositorDeMomento extends State<_CompositorDeMomento> {
  static const Duration _tempoParaIniciarGravacao = Duration(
    milliseconds: 450,
  );
  static const double _distanciaParaCancelar = 72;

  Timer? _temporizadorDaPressao;
  int? _identificadorDoPonteiro;
  Offset? _origemDaPressao;
  Future<void>? _inicioDaGravacao;
  bool _gravacaoFoiIniciadaPeloGesto = false;
  bool _cancelamentoFoiSolicitado = false;
  bool _acaoDoGestoFoiConcluida = false;

  TextEditingController get controladorDoTexto => widget.controladorDoTexto;
  FocusNode get focoDoTexto => widget.focoDoTexto;
  bool get estaPublicando => widget.estaPublicando;
  PublicacaoDoEncontro? get publicacaoSendoRespondida =>
      widget.publicacaoSendoRespondida;
  bool get gravacaoEstaDisponivel => widget.gravacaoEstaDisponivel;
  bool get estaSolicitandoMicrofone => widget.estaSolicitandoMicrofone;
  bool get estaGravandoAudio => widget.estaGravandoAudio;
  bool get estaFinalizandoAudio => widget.estaFinalizandoAudio;
  Duration get duracaoDoAudio => widget.duracaoDoAudio;
  AudioGravado? get audioPendente => widget.audioPendente;
  VoidCallback get aoSelecionarImagem => widget.aoSelecionarImagem;
  Future<void> Function() get aoIniciarAudio => widget.aoIniciarAudio;
  Future<void> Function() get aoCancelarAudio => widget.aoCancelarAudio;
  Future<void> Function() get aoEnviarAudio => widget.aoEnviarAudio;
  VoidCallback get aoCancelarResposta => widget.aoCancelarResposta;
  VoidCallback get aoAlternarEmojis => widget.aoAlternarEmojis;
  VoidCallback get aoPublicar => widget.aoPublicar;

  bool get _estaEmComputador {
    return defaultTargetPlatform == TargetPlatform.windows ||
        defaultTargetPlatform == TargetPlatform.macOS ||
        defaultTargetPlatform == TargetPlatform.linux;
  }

  @override
  void dispose() {
    _temporizadorDaPressao?.cancel();
    super.dispose();
  }

  void _pressioneMicrofone(PointerDownEvent evento) {
    if (_estaEmComputador ||
        estaPublicando ||
        _identificadorDoPonteiro != null) {
      return;
    }

    _identificadorDoPonteiro = evento.pointer;
    _origemDaPressao = evento.position;
    _gravacaoFoiIniciadaPeloGesto = false;
    _cancelamentoFoiSolicitado = false;
    _acaoDoGestoFoiConcluida = false;
    _inicioDaGravacao = null;
    _temporizadorDaPressao?.cancel();
    _temporizadorDaPressao = Timer(_tempoParaIniciarGravacao, () {
      if (!mounted || _identificadorDoPonteiro != evento.pointer) {
        return;
      }

      setState(() {
        _gravacaoFoiIniciadaPeloGesto = true;
      });
      _inicioDaGravacao = aoIniciarAudio();
    });
  }

  void _acompanhePonteiro(PointerMoveEvent evento) {
    if (_identificadorDoPonteiro != evento.pointer ||
        _origemDaPressao == null ||
        _cancelamentoFoiSolicitado) {
      return;
    }

    double deslocamentoVertical = evento.position.dy - _origemDaPressao!.dy;
    double deslocamentoHorizontal = evento.position.dx - _origemDaPressao!.dx;

    if (!_gravacaoFoiIniciadaPeloGesto &&
        (deslocamentoVertical.abs() > 18 ||
            deslocamentoHorizontal.abs() > 18)) {
      _reinicieGestoDeAudio();
      return;
    }

    if (deslocamentoHorizontal > -_distanciaParaCancelar) {
      return;
    }

    _temporizadorDaPressao?.cancel();
    setState(() {
      _cancelamentoFoiSolicitado = true;
    });

    if (_gravacaoFoiIniciadaPeloGesto) {
      unawaited(_concluaGestoDeAudioAsync(cancele: true));
    }
  }

  void _soltePonteiro(PointerUpEvent evento) {
    if (_identificadorDoPonteiro != evento.pointer) {
      return;
    }

    _temporizadorDaPressao?.cancel();
    if (!_gravacaoFoiIniciadaPeloGesto) {
      _reinicieGestoDeAudio();
      return;
    }

    unawaited(
      _concluaGestoDeAudioAsync(cancele: _cancelamentoFoiSolicitado),
    );
  }

  void _cancelePonteiro(PointerCancelEvent evento) {
    if (_identificadorDoPonteiro != evento.pointer) {
      return;
    }

    _temporizadorDaPressao?.cancel();
    if (!_gravacaoFoiIniciadaPeloGesto) {
      _reinicieGestoDeAudio();
      return;
    }

    unawaited(_concluaGestoDeAudioAsync(cancele: true));
  }

  Future<void> _concluaGestoDeAudioAsync({required bool cancele}) async {
    if (_acaoDoGestoFoiConcluida) {
      return;
    }

    _acaoDoGestoFoiConcluida = true;
    try {
      await _inicioDaGravacao;
      if (cancele) {
        await aoCancelarAudio();
      } else {
        await aoEnviarAudio();
      }
    } finally {
      if (mounted) {
        setState(_reinicieGestoDeAudio);
      }
    }
  }

  void _reinicieGestoDeAudio() {
    _temporizadorDaPressao?.cancel();
    _temporizadorDaPressao = null;
    _identificadorDoPonteiro = null;
    _origemDaPressao = null;
    _inicioDaGravacao = null;
    _gravacaoFoiIniciadaPeloGesto = false;
    _cancelamentoFoiSolicitado = false;
    _acaoDoGestoFoiConcluida = false;
  }

  KeyEventResult _processeTecla(FocusNode _, KeyEvent evento) {
    TextRange composicaoAtual = controladorDoTexto.value.composing;
    bool temComposicaoEmAndamento =
        composicaoAtual.isValid && !composicaoAtual.isCollapsed;

    if (!_estaEmComputador ||
        evento is! KeyDownEvent ||
        temComposicaoEmAndamento) {
      return KeyEventResult.ignored;
    }

    if (evento.logicalKey == LogicalKeyboardKey.escape &&
        publicacaoSendoRespondida != null) {
      aoCancelarResposta();
      return KeyEventResult.handled;
    }

    if (evento.logicalKey != LogicalKeyboardKey.enter) {
      return KeyEventResult.ignored;
    }

    if (HardwareKeyboard.instance.isShiftPressed) {
      _quebreLinha();
    } else {
      aoPublicar();
    }

    return KeyEventResult.handled;
  }

  void _quebreLinha() {
    TextEditingValue valorAtual = controladorDoTexto.value;
    TextSelection selecaoAtual = valorAtual.selection;
    int inicioDaSelecao =
        selecaoAtual.isValid ? selecaoAtual.start : valorAtual.text.length;
    int fimDaSelecao =
        selecaoAtual.isValid ? selecaoAtual.end : valorAtual.text.length;
    String textoAtualizado = valorAtual.text.replaceRange(
      inicioDaSelecao,
      fimDaSelecao,
      '\n',
    );

    controladorDoTexto.value = TextEditingValue(
      text: textoAtualizado,
      selection: TextSelection.collapsed(offset: inicioDaSelecao + 1),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Listener(
      onPointerMove: _acompanhePonteiro,
      onPointerUp: _soltePonteiro,
      onPointerCancel: _cancelePonteiro,
      child: DecoratedBox(
        decoration: BoxDecoration(
          color: CoresDoAplicativo.fundoElevado,
          border: const Border(
            top: BorderSide(color: CoresDoAplicativo.bordaDiscreta),
          ),
          boxShadow: <BoxShadow>[
            BoxShadow(
              color: Colors.black.withValues(alpha: 0.22),
              blurRadius: 18,
              offset: const Offset(0, -5),
            ),
          ],
        ),
        child: SafeArea(
          top: false,
          child: Padding(
            padding: const EdgeInsets.all(EspacamentosDoAplicativo.pequeno),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: <Widget>[
                if (estaSolicitandoMicrofone ||
                    estaGravandoAudio ||
                    estaFinalizandoAudio)
                  _ControlesDaGravacaoDeAudio(
                    estaSolicitandoMicrofone: estaSolicitandoMicrofone,
                    estaGravando: estaGravandoAudio,
                    estaFinalizando: estaFinalizandoAudio,
                    duracao: duracaoDoAudio,
                    mostreDicaDeDeslize:
                        !_estaEmComputador && _gravacaoFoiIniciadaPeloGesto,
                    cancelamentoFoiSolicitado: _cancelamentoFoiSolicitado,
                    aoCancelar: aoCancelarAudio,
                    aoEnviar: aoEnviarAudio,
                  )
                else if (audioPendente != null)
                  _PreviaDoAudioGravado(
                    audio: audioPendente!,
                    estaPublicando: estaPublicando,
                    aoCancelar: aoCancelarAudio,
                    aoEnviar: aoEnviarAudio,
                  )
                else ...<Widget>[
                  if (publicacaoSendoRespondida != null) ...<Widget>[
                    _TrechoDaPublicacaoRespondida(
                      publicacaoRespondida: PublicacaoRespondida(
                        identificador: publicacaoSendoRespondida!.identificador,
                        nomeDoAutor: publicacaoSendoRespondida!.usuarioAtual
                            ? 'Você'
                            : publicacaoSendoRespondida!.nomeDoAutor,
                        texto: publicacaoSendoRespondida!.texto,
                        temMidia: publicacaoSendoRespondida!.temMidia,
                        foiRemovida: false,
                      ),
                      exibaAcaoDeCancelar: true,
                      aoCancelar: aoCancelarResposta,
                    ),
                    const SizedBox(height: EspacamentosDoAplicativo.pequeno),
                  ],
                  Row(
                    crossAxisAlignment: CrossAxisAlignment.end,
                    children: <Widget>[
                      IconButton(
                        key: const Key('selecionar-foto'),
                        tooltip: 'Adicionar foto',
                        onPressed: estaPublicando ? null : aoSelecionarImagem,
                        icon: const Icon(Icons.add_photo_alternate_outlined),
                      ),
                      IconButton(
                        key: const Key('alternar-emojis'),
                        tooltip: 'Emojis',
                        onPressed: estaPublicando ? null : aoAlternarEmojis,
                        icon: const Icon(Icons.sentiment_satisfied_alt_rounded),
                      ),
                      Expanded(
                        child: Focus(
                          onKeyEvent: _processeTecla,
                          child: TextField(
                            key: const Key('texto-da-nova-publicacao'),
                            controller: controladorDoTexto,
                            focusNode: focoDoTexto,
                            enabled: !estaPublicando,
                            minLines: 1,
                            maxLines: 4,
                            maxLength: 1000,
                            buildCounter: (
                              BuildContext context, {
                              required int currentLength,
                              required bool isFocused,
                              required int? maxLength,
                            }) {
                              return null;
                            },
                            textInputAction: TextInputAction.newline,
                            decoration: InputDecoration(
                              hintText: 'Compartilhe algo com o encontro',
                              filled: true,
                              fillColor: CoresDoAplicativo.fundoDoCartaoSuave,
                              contentPadding: const EdgeInsets.symmetric(
                                horizontal: 14,
                                vertical: 11,
                              ),
                              border: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(
                                  RaiosDoAplicativo.pilula,
                                ),
                                borderSide: BorderSide.none,
                              ),
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(width: EspacamentosDoAplicativo.pequeno),
                      ValueListenableBuilder<TextEditingValue>(
                        valueListenable: controladorDoTexto,
                        builder: (
                          BuildContext context,
                          TextEditingValue valor,
                          Widget? child,
                        ) {
                          bool temTexto = valor.text.trim().isNotEmpty;

                          if (!temTexto && gravacaoEstaDisponivel) {
                            return _BotaoDeMicrofone(
                              estaEmComputador: _estaEmComputador,
                              estaDesabilitado: estaPublicando,
                              aoClicar: aoIniciarAudio,
                              aoPressionar: _pressioneMicrofone,
                            );
                          }

                          return IconButton.filled(
                            key: const Key('publicar-momento'),
                            tooltip: 'Publicar',
                            onPressed:
                                estaPublicando || !temTexto ? null : aoPublicar,
                            icon: estaPublicando
                                ? const SizedBox.square(
                                    dimension: 20,
                                    child: CircularProgressIndicator(
                                      strokeWidth: 2,
                                    ),
                                  )
                                : const Icon(Icons.send_rounded),
                          );
                        },
                      ),
                    ],
                  ),
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _BotaoDeMicrofone extends StatelessWidget {
  const _BotaoDeMicrofone({
    required this.estaEmComputador,
    required this.estaDesabilitado,
    required this.aoClicar,
    required this.aoPressionar,
  });

  final bool estaEmComputador;
  final bool estaDesabilitado;
  final Future<void> Function() aoClicar;
  final void Function(PointerDownEvent evento) aoPressionar;

  @override
  Widget build(BuildContext context) {
    if (estaEmComputador) {
      return IconButton.filled(
        key: const Key('gravar-audio'),
        tooltip: 'Gravar áudio',
        onPressed: estaDesabilitado ? null : aoClicar,
        icon: const Icon(Icons.mic_none_rounded),
      );
    }

    return Semantics(
      button: true,
      label: 'Pressione e segure para gravar áudio',
      child: Listener(
        key: const Key('gravar-audio'),
        onPointerDown: estaDesabilitado ? null : aoPressionar,
        child: Tooltip(
          message: 'Pressione e segure para gravar',
          child: SizedBox.square(
            dimension: 48,
            child: DecoratedBox(
              decoration: BoxDecoration(
                color: estaDesabilitado
                    ? CoresDoAplicativo.bordaDiscreta
                    : CoresDoAplicativo.verdeDestaque,
                shape: BoxShape.circle,
              ),
              child: Icon(
                Icons.mic_none_rounded,
                color: estaDesabilitado
                    ? CoresDoAplicativo.textoSecundario
                    : CoresDoAplicativo.fundoPrincipal,
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _ControlesDaGravacaoDeAudio extends StatelessWidget {
  const _ControlesDaGravacaoDeAudio({
    required this.estaSolicitandoMicrofone,
    required this.estaGravando,
    required this.estaFinalizando,
    required this.duracao,
    required this.mostreDicaDeDeslize,
    required this.cancelamentoFoiSolicitado,
    required this.aoCancelar,
    required this.aoEnviar,
  });

  final bool estaSolicitandoMicrofone;
  final bool estaGravando;
  final bool estaFinalizando;
  final Duration duracao;
  final bool mostreDicaDeDeslize;
  final bool cancelamentoFoiSolicitado;
  final Future<void> Function() aoCancelar;
  final Future<void> Function() aoEnviar;

  @override
  Widget build(BuildContext context) {
    String situacao = estaSolicitandoMicrofone
        ? 'Aguardando permissão do microfone'
        : estaFinalizando
            ? 'Preparando áudio'
            : _formateDuracao(duracao);

    return Row(
      children: <Widget>[
        TextButton(
          key: const Key('cancelar-gravacao-de-audio'),
          onPressed: estaFinalizando ? null : aoCancelar,
          child: const Text('Cancelar'),
        ),
        const SizedBox(width: EspacamentosDoAplicativo.minimo),
        if (estaSolicitandoMicrofone || estaFinalizando)
          const SizedBox.square(
            dimension: 18,
            child: CircularProgressIndicator(strokeWidth: 2),
          )
        else
          const Icon(
            Icons.fiber_manual_record_rounded,
            color: Colors.redAccent,
            size: 18,
          ),
        const SizedBox(width: EspacamentosDoAplicativo.pequeno),
        Expanded(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: <Widget>[
              Text(
                situacao,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
              ),
              if (mostreDicaDeDeslize)
                Text(
                  cancelamentoFoiSolicitado
                      ? 'Gravação cancelada'
                      : 'Deslize para a esquerda para cancelar',
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    color: CoresDoAplicativo.textoSecundario,
                    fontSize: 11,
                  ),
                ),
            ],
          ),
        ),
        IconButton.filled(
          key: const Key('enviar-gravacao-de-audio'),
          tooltip: 'Enviar áudio',
          onPressed: estaGravando ? aoEnviar : null,
          icon: estaFinalizando
              ? const SizedBox.square(
                  dimension: 20,
                  child: CircularProgressIndicator(strokeWidth: 2),
                )
              : const Icon(Icons.send_rounded),
        ),
      ],
    );
  }
}

class _PreviaDoAudioGravado extends StatelessWidget {
  const _PreviaDoAudioGravado({
    required this.audio,
    required this.estaPublicando,
    required this.aoCancelar,
    required this.aoEnviar,
  });

  final AudioGravado audio;
  final bool estaPublicando;
  final VoidCallback aoCancelar;
  final VoidCallback aoEnviar;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: <Widget>[
        IconButton(
          key: const Key('descartar-audio-gravado'),
          tooltip: 'Descartar áudio',
          onPressed: estaPublicando ? null : aoCancelar,
          icon: const Icon(Icons.delete_outline_rounded),
        ),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: <Widget>[
              Text(
                'Áudio gravado · ${_formateDuracao(audio.duracao)}',
                style: const TextStyle(
                  color: CoresDoAplicativo.textoSecundario,
                  fontSize: 12,
                ),
              ),
              const SizedBox(height: EspacamentosDoAplicativo.minimo),
              AudioComBytes(
                bytes: audio.bytes,
                tipoDeConteudo: audio.tipoDeConteudo,
              ),
            ],
          ),
        ),
        const SizedBox(width: EspacamentosDoAplicativo.pequeno),
        IconButton.filled(
          key: const Key('enviar-audio-gravado'),
          tooltip: 'Enviar áudio',
          onPressed: estaPublicando ? null : aoEnviar,
          icon: estaPublicando
              ? const SizedBox.square(
                  dimension: 20,
                  child: CircularProgressIndicator(strokeWidth: 2),
                )
              : const Icon(Icons.send_rounded),
        ),
      ],
    );
  }
}

String _formateDuracao(Duration duracao) {
  int minutos = duracao.inMinutes;
  int segundos = duracao.inSeconds.remainder(60);
  return '$minutos:${segundos.toString().padLeft(2, '0')}';
}

class DialogoDeNovaPublicacao extends StatefulWidget {
  const DialogoDeNovaPublicacao({
    required this.midias,
    required this.legendaInicial,
    required this.aoPublicar,
    super.key,
  });

  final List<MidiaSelecionada> midias;
  final String legendaInicial;
  final Future<bool> Function(String legenda) aoPublicar;

  @override
  State<DialogoDeNovaPublicacao> createState() =>
      _EstadoDoDialogoDeNovaPublicacao();
}

class _EstadoDoDialogoDeNovaPublicacao extends State<DialogoDeNovaPublicacao> {
  late final TextEditingController _controladorDaLegenda;
  final PageController _controladorDasMidias = PageController();
  int _indiceDaMidia = 0;
  bool _estaPublicando = false;

  @override
  void initState() {
    super.initState();
    _controladorDaLegenda = TextEditingController(text: widget.legendaInicial);
  }

  @override
  void dispose() {
    _controladorDaLegenda.dispose();
    _controladorDasMidias.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    bool telaPequena = MediaQuery.sizeOf(context).width < 600;
    Widget conteudo = Material(
      color: CoresDoAplicativo.fundoPrincipal,
      child: SafeArea(
        child: Column(
          children: <Widget>[
            _CabecalhoDaNovaPublicacao(
              estaPublicando: _estaPublicando,
              aoCancelar: () => Navigator.of(context).pop(false),
              aoPublicar: _publiqueAsync,
            ),
            Expanded(
              child: _PreviaDasMidiasSelecionadas(
                midias: widget.midias,
                controlador: _controladorDasMidias,
                indiceAtual: _indiceDaMidia,
                aoMudar: (int indice) {
                  setState(() {
                    _indiceDaMidia = indice;
                  });
                },
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(EspacamentosDoAplicativo.padrao),
              child: TextField(
                key: const Key('legenda-da-publicacao'),
                controller: _controladorDaLegenda,
                enabled: !_estaPublicando,
                minLines: 2,
                maxLines: 5,
                maxLength: 280,
                decoration: const InputDecoration(
                  hintText: 'Escreva uma legenda...',
                ),
              ),
            ),
          ],
        ),
      ),
    );

    if (telaPequena) {
      return Dialog.fullscreen(child: conteudo);
    }

    return Dialog(
      child: SizedBox(
        width: 560,
        height: 700,
        child: conteudo,
      ),
    );
  }

  Future<void> _publiqueAsync() async {
    if (_estaPublicando || _controladorDaLegenda.text.trim().length > 280) {
      return;
    }

    setState(() {
      _estaPublicando = true;
    });
    bool publicou = await widget.aoPublicar(_controladorDaLegenda.text);

    if (!mounted) {
      return;
    }

    if (publicou) {
      Navigator.of(context).pop(true);
      return;
    }

    setState(() {
      _estaPublicando = false;
    });
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(content: Text('Não foi possível publicar as mídias.')),
    );
  }
}

class _CabecalhoDaNovaPublicacao extends StatelessWidget {
  const _CabecalhoDaNovaPublicacao({
    required this.estaPublicando,
    required this.aoCancelar,
    required this.aoPublicar,
  });

  final bool estaPublicando;
  final VoidCallback aoCancelar;
  final VoidCallback aoPublicar;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(
        horizontal: EspacamentosDoAplicativo.pequeno,
        vertical: EspacamentosDoAplicativo.minimo,
      ),
      child: Row(
        children: <Widget>[
          IconButton(
            key: const Key('cancelar-nova-publicacao'),
            tooltip: 'Cancelar',
            onPressed: estaPublicando ? null : aoCancelar,
            icon: const Icon(Icons.close_rounded),
          ),
          const Expanded(
            child: Text(
              'Nova publicação',
              textAlign: TextAlign.center,
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600),
            ),
          ),
          TextButton(
            key: const Key('confirmar-nova-publicacao'),
            onPressed: estaPublicando ? null : aoPublicar,
            child: estaPublicando
                ? const SizedBox.square(
                    dimension: 18,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                : const Text('Publicar'),
          ),
        ],
      ),
    );
  }
}

class _PreviaDasMidiasSelecionadas extends StatelessWidget {
  const _PreviaDasMidiasSelecionadas({
    required this.midias,
    required this.controlador,
    required this.indiceAtual,
    required this.aoMudar,
  });

  final List<MidiaSelecionada> midias;
  final PageController controlador;
  final int indiceAtual;
  final ValueChanged<int> aoMudar;

  @override
  Widget build(BuildContext context) {
    return Stack(
      fit: StackFit.expand,
      children: <Widget>[
        PageView.builder(
          key: const Key('previa-das-midias'),
          controller: controlador,
          itemCount: midias.length,
          onPageChanged: aoMudar,
          itemBuilder: (BuildContext context, int indice) {
            MidiaSelecionada midia = midias[indice];

            if (midia.ehVideo) {
              return VideoComBytes(
                bytes: midia.conteudo,
                tipoDeConteudo: midia.tipoDeConteudo,
                exibaControles: true,
              );
            }

            return Image.memory(
              midia.conteudo,
              key: Key('previa-da-midia-$indice'),
              fit: BoxFit.contain,
              errorBuilder: (_, __, ___) => const Center(
                child: Icon(Icons.broken_image_outlined, size: 52),
              ),
            );
          },
        ),
        if (midias.length > 1)
          Positioned(
            top: EspacamentosDoAplicativo.pequeno,
            right: EspacamentosDoAplicativo.pequeno,
            child: DecoratedBox(
              decoration: BoxDecoration(
                color: Colors.black.withValues(alpha: 0.7),
                borderRadius: BorderRadius.circular(
                  RaiosDoAplicativo.pilula,
                ),
              ),
              child: Padding(
                padding: const EdgeInsets.symmetric(
                  horizontal: EspacamentosDoAplicativo.pequeno,
                  vertical: EspacamentosDoAplicativo.minimo,
                ),
                child: Text(
                  '${indiceAtual + 1}/${midias.length}',
                  style: const TextStyle(color: Colors.white),
                ),
              ),
            ),
          ),
      ],
    );
  }
}

class _SeletorDeEmoji extends StatelessWidget {
  const _SeletorDeEmoji({
    required this.emojis,
    required this.aoSelecionar,
  });

  final List<String> emojis;
  final ValueChanged<String> aoSelecionar;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 54,
      child: ListView.separated(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(
          horizontal: EspacamentosDoAplicativo.padrao,
          vertical: EspacamentosDoAplicativo.minimo,
        ),
        itemCount: emojis.length,
        separatorBuilder: (BuildContext context, int indice) =>
            const SizedBox(width: EspacamentosDoAplicativo.pequeno),
        itemBuilder: (BuildContext context, int indice) {
          String emoji = emojis[indice];

          return IconButton(
            key: Key('emoji-$indice'),
            tooltip: 'Adicionar $emoji',
            onPressed: () => aoSelecionar(emoji),
            icon: Text(emoji, style: const TextStyle(fontSize: 22)),
          );
        },
      ),
    );
  }
}

class _AvisoDeEncontroCancelado extends StatelessWidget {
  const _AvisoDeEncontroCancelado();

  @override
  Widget build(BuildContext context) {
    return const ColoredBox(
      color: Color(0xFF321713),
      child: Padding(
        padding: EdgeInsets.all(EspacamentosDoAplicativo.pequeno),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: <Widget>[
            Icon(Icons.event_busy_outlined, color: CoresDoAplicativo.coral),
            SizedBox(width: EspacamentosDoAplicativo.pequeno),
            Text('Este encontro foi cancelado e está somente para leitura.'),
          ],
        ),
      ),
    );
  }
}

class _MensagemDeErro extends StatelessWidget {
  const _MensagemDeErro({
    required this.mensagem,
    required this.aoTentarNovamente,
  });

  final String mensagem;
  final VoidCallback aoTentarNovamente;

  @override
  Widget build(BuildContext context) {
    return MaterialBanner(
      content: Text(mensagem),
      backgroundColor: const Color(0xFF321713),
      actions: <Widget>[
        TextButton(
          onPressed: aoTentarNovamente,
          child: const Text('Tentar novamente'),
        ),
      ],
    );
  }
}

class _ErroDosMomentos extends StatelessWidget {
  const _ErroDosMomentos({
    required this.mensagem,
    required this.aoVoltar,
    required this.aoTentarNovamente,
  });

  final String mensagem;
  final VoidCallback aoVoltar;
  final VoidCallback aoTentarNovamente;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(EspacamentosDoAplicativo.grande),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: <Widget>[
            const Icon(
              Icons.cloud_off_outlined,
              size: 42,
              color: CoresDoAplicativo.coral,
            ),
            const SizedBox(height: EspacamentosDoAplicativo.medio),
            Text(mensagem, textAlign: TextAlign.center),
            const SizedBox(height: EspacamentosDoAplicativo.padrao),
            FilledButton(
              onPressed: aoTentarNovamente,
              child: const Text('Tentar novamente'),
            ),
            TextButton(onPressed: aoVoltar, child: const Text('Voltar')),
          ],
        ),
      ),
    );
  }
}
