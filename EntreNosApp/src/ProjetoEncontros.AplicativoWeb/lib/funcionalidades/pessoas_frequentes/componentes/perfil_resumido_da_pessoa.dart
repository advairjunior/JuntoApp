import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/configuracao/configuracao_do_ambiente.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/foto_de_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/imagem_privada.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/raios_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/dados/repositorio_de_encontros.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/estado/controlador_da_pagina_inicial.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/estado/estado_da_pagina_inicial.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/encontro_resumo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/pessoas_frequentes/dados/repositorio_de_pessoas_frequentes.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/pessoas_frequentes/modelos/pessoa_frequente.dart';

class PessoaDoEncontro {
  const PessoaDoEncontro({
    required this.identificadorDoUsuario,
    required this.nome,
    this.urlDaFotoDePerfil,
  });

  final String identificadorDoUsuario;
  final String nome;
  final String? urlDaFotoDePerfil;

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

enum _AcaoDoPerfilResumido { convidar }

Future<void> mostrePerfilResumidoDaPessoaAsync({
  required BuildContext context,
  required PessoaDoEncontro pessoa,
  String? identificadorDoEncontroAtual,
}) async {
  _AcaoDoPerfilResumido? acao =
      await showModalBottomSheet<_AcaoDoPerfilResumido>(
    context: context,
    isScrollControlled: true,
    backgroundColor: CoresDoAplicativo.fundoDoCartao,
    showDragHandle: true,
    builder: (BuildContext context) {
      return _PerfilResumidoDaPessoa(pessoa: pessoa);
    },
  );

  if (acao != _AcaoDoPerfilResumido.convidar || !context.mounted) {
    return;
  }

  await mostreConviteParaOutroEncontroAsync(
    context: context,
    pessoa: pessoa,
    identificadorDoEncontroAtual: identificadorDoEncontroAtual,
  );
}

Future<void> mostreConviteParaOutroEncontroAsync({
  required BuildContext context,
  required PessoaDoEncontro pessoa,
  String? identificadorDoEncontroAtual,
}) {
  return showModalBottomSheet<void>(
    context: context,
    isScrollControlled: true,
    backgroundColor: CoresDoAplicativo.fundoDoCartao,
    showDragHandle: true,
    builder: (BuildContext context) {
      return _ConviteParaOutroEncontro(
        pessoa: pessoa,
        identificadorDoEncontroAtual: identificadorDoEncontroAtual,
      );
    },
  );
}

class _PerfilResumidoDaPessoa extends ConsumerWidget {
  const _PerfilResumidoDaPessoa({required this.pessoa});

  final PessoaDoEncontro pessoa;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    AsyncValue<List<PessoaFrequente>> pessoas =
        ref.watch(provedorDasPessoasFrequentes);
    PessoaFrequente? pessoaFrequente = _encontrePessoaFrequente(
      pessoas.valueOrNull ?? <PessoaFrequente>[],
      pessoa.identificadorDoUsuario,
    );

    return SafeArea(
      top: false,
      child: Padding(
        padding: const EdgeInsets.fromLTRB(
          EspacamentosDoAplicativo.grande,
          EspacamentosDoAplicativo.pequeno,
          EspacamentosDoAplicativo.grande,
          EspacamentosDoAplicativo.grande,
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: <Widget>[
            Semantics(
              button: pessoa.urlDaFotoDePerfil?.trim().isNotEmpty ?? false,
              label: 'Ampliar foto de ${pessoa.nome}',
              child: InkResponse(
                key: Key(
                  'ampliar-foto-do-perfil-${pessoa.identificadorDoUsuario}',
                ),
                radius: 58,
                onTap: pessoa.urlDaFotoDePerfil?.trim().isNotEmpty ?? false
                    ? () => mostreFotoDaPessoaAmpliadaAsync(context, pessoa)
                    : null,
                child: FotoDePerfil(
                  url: pessoa.urlDaFotoDePerfil,
                  iniciais: pessoa.iniciais,
                  dimensao: 104,
                  tamanhoDasIniciais: 34,
                ),
              ),
            ),
            const SizedBox(height: EspacamentosDoAplicativo.medio),
            Text(
              pessoa.nome,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                    fontWeight: FontWeight.w700,
                  ),
            ),
            const SizedBox(height: EspacamentosDoAplicativo.pequeno),
            AnimatedSwitcher(
              duration: const Duration(milliseconds: 180),
              child: pessoas.isLoading
                  ? const SizedBox(
                      key: Key('carregando-convivencia'),
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : Text(
                      pessoaFrequente?.textoDaRecorrencia ??
                          'Primeiro encontro registrado de vocês',
                      key: const Key('resumo-da-convivencia'),
                      textAlign: TextAlign.center,
                      style: const TextStyle(
                        color: CoresDoAplicativo.textoSecundario,
                      ),
                    ),
            ),
            if (pessoaFrequente != null) ...<Widget>[
              const SizedBox(height: EspacamentosDoAplicativo.minimo),
              Text(
                'Último encontro em '
                '${DateFormat('dd/MM/yyyy').format(pessoaFrequente.ultimoEncontroEm)}',
                style: const TextStyle(
                  color: CoresDoAplicativo.textoTerciario,
                  fontSize: 13,
                ),
              ),
            ],
            const SizedBox(height: EspacamentosDoAplicativo.grande),
            SizedBox(
              width: double.infinity,
              child: FilledButton.icon(
                key: const Key('convidar-para-outro-encontro'),
                onPressed: () => Navigator.of(context).pop(
                  _AcaoDoPerfilResumido.convidar,
                ),
                icon: const Icon(Icons.person_add_alt_1_outlined),
                label: const Text('Convidar para outro encontro'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  PessoaFrequente? _encontrePessoaFrequente(
    List<PessoaFrequente> pessoas,
    String identificadorDoUsuario,
  ) {
    for (PessoaFrequente pessoa in pessoas) {
      if (pessoa.identificadorDoUsuario == identificadorDoUsuario) {
        return pessoa;
      }
    }

    return null;
  }
}

class _ConviteParaOutroEncontro extends ConsumerStatefulWidget {
  const _ConviteParaOutroEncontro({
    required this.pessoa,
    required this.identificadorDoEncontroAtual,
  });

  final PessoaDoEncontro pessoa;
  final String? identificadorDoEncontroAtual;

  @override
  ConsumerState<_ConviteParaOutroEncontro> createState() =>
      _EstadoDoConviteParaOutroEncontro();
}

class _EstadoDoConviteParaOutroEncontro
    extends ConsumerState<_ConviteParaOutroEncontro> {
  String? _identificadorEmEnvio;
  String? _mensagemDeErro;

  @override
  Widget build(BuildContext context) {
    EstadoDaPaginaInicial estado =
        ref.watch(provedorDoControladorDaPaginaInicial);
    List<EncontroResumo> encontros = estado.encontros
        .where(
          (EncontroResumo encontro) =>
              (widget.identificadorDoEncontroAtual == null ||
                  encontro.identificador !=
                      widget.identificadorDoEncontroAtual) &&
              encontro.situacao.toLowerCase() != 'cancelado',
        )
        .toList();

    return SafeArea(
      top: false,
      child: ConstrainedBox(
        constraints: BoxConstraints(
          maxHeight: MediaQuery.sizeOf(context).height * 0.72,
        ),
        child: Padding(
          padding: const EdgeInsets.fromLTRB(
            EspacamentosDoAplicativo.grande,
            EspacamentosDoAplicativo.pequeno,
            EspacamentosDoAplicativo.grande,
            EspacamentosDoAplicativo.grande,
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: <Widget>[
              Text(
                'Convidar ${widget.pessoa.nome}',
                style: Theme.of(context).textTheme.titleLarge?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
              ),
              const SizedBox(height: EspacamentosDoAplicativo.minimo),
              const Text(
                'Escolha um dos seus próximos encontros.',
                style: TextStyle(color: CoresDoAplicativo.textoSecundario),
              ),
              const SizedBox(height: EspacamentosDoAplicativo.medio),
              if (_mensagemDeErro != null) ...<Widget>[
                Text(
                  _mensagemDeErro!,
                  key: const Key('erro-ao-convidar-pessoa'),
                  style: const TextStyle(color: CoresDoAplicativo.coral),
                ),
                const SizedBox(height: EspacamentosDoAplicativo.medio),
              ],
              Flexible(child: _construaConteudo(estado, encontros)),
            ],
          ),
        ),
      ),
    );
  }

  Widget _construaConteudo(
    EstadoDaPaginaInicial estado,
    List<EncontroResumo> encontros,
  ) {
    if (estado.situacao == SituacaoDaPaginaInicial.carregando) {
      return const Center(child: CircularProgressIndicator());
    }

    if (estado.situacao == SituacaoDaPaginaInicial.falhou) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: <Widget>[
            Text(
              estado.mensagemDeErro ??
                  'Não foi possível carregar seus encontros.',
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: EspacamentosDoAplicativo.medio),
            TextButton(
              onPressed: () => ref
                  .read(provedorDoControladorDaPaginaInicial.notifier)
                  .carregueAsync(),
              child: const Text('Tentar novamente'),
            ),
          ],
        ),
      );
    }

    if (encontros.isEmpty) {
      return const Center(
        child: Padding(
          padding: EdgeInsets.symmetric(
            vertical: EspacamentosDoAplicativo.grande,
          ),
          child: Text(
            'Nenhum outro encontro disponível para convite.',
            textAlign: TextAlign.center,
            style: TextStyle(color: CoresDoAplicativo.textoSecundario),
          ),
        ),
      );
    }

    return ListView.separated(
      shrinkWrap: true,
      itemCount: encontros.length,
      separatorBuilder: (_, __) =>
          const SizedBox(height: EspacamentosDoAplicativo.pequeno),
      itemBuilder: (BuildContext context, int indice) {
        EncontroResumo encontro = encontros[indice];
        bool estaEnviando = _identificadorEmEnvio == encontro.identificador;

        return Material(
          color: CoresDoAplicativo.fundoDoCartaoSuave,
          borderRadius: BorderRadius.circular(RaiosDoAplicativo.medio),
          child: ListTile(
            key: Key('convidar-para-encontro-${encontro.identificador}'),
            enabled: _identificadorEmEnvio == null,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(RaiosDoAplicativo.medio),
            ),
            leading: const Icon(Icons.event_outlined),
            title: Text(encontro.titulo),
            subtitle: Text(
              DateFormat('dd/MM/yyyy • HH:mm').format(encontro.inicioEm),
            ),
            trailing: estaEnviando
                ? const SizedBox.square(
                    dimension: 20,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                : const Icon(Icons.chevron_right_rounded),
            onTap: () => _convideAsync(encontro),
          ),
        );
      },
    );
  }

  Future<void> _convideAsync(EncontroResumo encontro) async {
    setState(() {
      _identificadorEmEnvio = encontro.identificador;
      _mensagemDeErro = null;
    });

    try {
      await ref
          .read(provedorDoRepositorioDeEncontros)
          .convidePessoaFrequenteAsync(
            identificador: encontro.identificador,
            identificadorDoUsuario: widget.pessoa.identificadorDoUsuario,
          );

      if (!mounted) {
        return;
      }

      Navigator.of(context).pop();
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            '${widget.pessoa.nome} foi convidado(a) para ${encontro.titulo}.',
          ),
        ),
      );
    } on ExcecaoDaApi catch (excecao) {
      if (mounted) {
        setState(() {
          _mensagemDeErro = excecao.mensagem;
          _identificadorEmEnvio = null;
        });
      }
    } catch (_) {
      if (mounted) {
        setState(() {
          _mensagemDeErro = 'Não foi possível enviar o convite.';
          _identificadorEmEnvio = null;
        });
      }
    }
  }
}

Future<void> mostreFotoDaPessoaAmpliadaAsync(
  BuildContext context,
  PessoaDoEncontro pessoa,
) async {
  String recurso =
      ConfiguracaoDoAmbiente.crieUrlAbsoluta(pessoa.urlDaFotoDePerfil);

  if (recurso.isEmpty) {
    return;
  }

  await showDialog<void>(
    context: context,
    useSafeArea: false,
    builder: (BuildContext context) {
      return Dialog.fullscreen(
        key: Key(
          'foto-ampliada-do-perfil-${pessoa.identificadorDoUsuario}',
        ),
        backgroundColor: Colors.black,
        child: Stack(
          children: <Widget>[
            Positioned.fill(
              child: InteractiveViewer(
                minScale: 1,
                maxScale: 4,
                child: Center(
                  child: ImagemPrivada(
                    recurso: recurso,
                    ajuste: BoxFit.contain,
                    construaSubstituta: (_) => FotoDePerfil(
                      url: null,
                      iniciais: pessoa.iniciais,
                      dimensao: 120,
                      tamanhoDasIniciais: 38,
                    ),
                  ),
                ),
              ),
            ),
            SafeArea(
              child: Align(
                alignment: Alignment.topRight,
                child: Padding(
                  padding: const EdgeInsets.all(
                    EspacamentosDoAplicativo.padrao,
                  ),
                  child: IconButton.filled(
                    tooltip: 'Fechar',
                    onPressed: () => Navigator.of(context).pop(),
                    icon: const Icon(Icons.close_rounded),
                  ),
                ),
              ),
            ),
          ],
        ),
      );
    },
  );
}
