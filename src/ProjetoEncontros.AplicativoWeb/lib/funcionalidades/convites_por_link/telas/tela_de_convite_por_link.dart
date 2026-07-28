import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cartao_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/conteudo_responsivo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/convites_por_link/dados/repositorio_de_convites_por_link.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/convites_por_link/modelos/convite_por_link.dart';

class TelaDeConvitePorLink extends ConsumerStatefulWidget {
  const TelaDeConvitePorLink({
    required this.token,
    super.key,
  });

  final String token;

  @override
  ConsumerState<TelaDeConvitePorLink> createState() =>
      _EstadoDaTelaDeConvitePorLink();
}

class _EstadoDaTelaDeConvitePorLink
    extends ConsumerState<TelaDeConvitePorLink> {
  ConvitePorLinkDetalhado? _convite;
  String? _mensagemDeErro;
  bool _estaCarregando = true;
  bool _estaAceitando = false;

  @override
  void initState() {
    super.initState();
    Future<void>.microtask(_carregueAsync);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        leading: IconButton(
          tooltip: 'Voltar',
          onPressed: () => context.go('/inicio'),
          icon: const Icon(Icons.close_rounded),
        ),
        title: const Text('Convite'),
      ),
      body: SafeArea(
        child: ConteudoResponsivo(
          filho: Center(
            child: _estaCarregando
                ? const CircularProgressIndicator()
                : _convite == null
                    ? _ErroDoConvite(
                        mensagem: _mensagemDeErro ??
                            'Este convite não está mais disponível.',
                        aoTentarNovamente: _carregueAsync,
                      )
                    : _ConteudoDoConvite(
                        convite: _convite!,
                        estaAceitando: _estaAceitando,
                        mensagemDeErro: _mensagemDeErro,
                        aoAceitar: _aceiteAsync,
                      ),
          ),
        ),
      ),
    );
  }

  Future<void> _carregueAsync() async {
    setState(() {
      _estaCarregando = true;
      _mensagemDeErro = null;
    });

    try {
      ConvitePorLinkDetalhado convite = await ref
          .read(provedorDoRepositorioDeConvitesPorLink)
          .consulteAsync(widget.token);

      if (!mounted) {
        return;
      }

      setState(() {
        _convite = convite;
        _estaCarregando = false;
      });
    } on ExcecaoDaApi catch (excecao) {
      _registreFalha(excecao.mensagem);
    } catch (_) {
      _registreFalha('Este convite não está mais disponível.');
    }
  }

  Future<void> _aceiteAsync() async {
    if (_estaAceitando) {
      return;
    }

    setState(() {
      _estaAceitando = true;
      _mensagemDeErro = null;
    });

    try {
      AceiteDoConvitePorLink aceite = await ref
          .read(provedorDoRepositorioDeConvitesPorLink)
          .aceiteAsync(widget.token);

      if (!mounted) {
        return;
      }

      context.go('/encontros/${aceite.identificadorDoEncontro}');
    } on ExcecaoDaApi catch (excecao) {
      _registreFalhaNoAceite(excecao.mensagem);
    } catch (_) {
      _registreFalhaNoAceite('Não foi possível entrar neste encontro.');
    }
  }

  void _registreFalha(String mensagem) {
    if (!mounted) {
      return;
    }

    setState(() {
      _convite = null;
      _estaCarregando = false;
      _mensagemDeErro = mensagem;
    });
  }

  void _registreFalhaNoAceite(String mensagem) {
    if (!mounted) {
      return;
    }

    setState(() {
      _estaAceitando = false;
      _mensagemDeErro = mensagem;
    });
  }
}

class _ConteudoDoConvite extends StatelessWidget {
  const _ConteudoDoConvite({
    required this.convite,
    required this.estaAceitando,
    required this.mensagemDeErro,
    required this.aoAceitar,
  });

  final ConvitePorLinkDetalhado convite;
  final bool estaAceitando;
  final String? mensagemDeErro;
  final VoidCallback aoAceitar;

  @override
  Widget build(BuildContext context) {
    String data = DateFormat(
      "EEEE, dd 'de' MMMM 'às' HH:mm",
      'pt_BR',
    ).format(convite.inicioEm);

    return CartaoDoAplicativo(
      elevado: true,
      preenchimento: const EdgeInsets.all(EspacamentosDoAplicativo.grande),
      filho: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: <Widget>[
          const Icon(
            Icons.celebration_outlined,
            size: 48,
            color: CoresDoAplicativo.verdeDestaque,
          ),
          const SizedBox(height: EspacamentosDoAplicativo.padrao),
          const Text(
            'Você recebeu um convite',
            textAlign: TextAlign.center,
            style: TextStyle(fontSize: 16),
          ),
          const SizedBox(height: EspacamentosDoAplicativo.pequeno),
          Text(
            convite.titulo,
            textAlign: TextAlign.center,
            style: Theme.of(context).textTheme.headlineSmall,
          ),
          const SizedBox(height: EspacamentosDoAplicativo.padrao),
          Text(
            data,
            textAlign: TextAlign.center,
            style: const TextStyle(
              color: CoresDoAplicativo.textoSecundario,
            ),
          ),
          if (convite.tipo != null && convite.tipo!.trim().isNotEmpty) ...[
            const SizedBox(height: EspacamentosDoAplicativo.pequeno),
            Text(
              convite.tipo!,
              textAlign: TextAlign.center,
              style: const TextStyle(color: CoresDoAplicativo.ambar),
            ),
          ],
          if (mensagemDeErro != null) ...<Widget>[
            const SizedBox(height: EspacamentosDoAplicativo.padrao),
            Text(
              mensagemDeErro!,
              textAlign: TextAlign.center,
              style: const TextStyle(color: CoresDoAplicativo.coral),
            ),
          ],
          const SizedBox(height: EspacamentosDoAplicativo.grande),
          FilledButton.icon(
            key: const Key('aceitar-convite-por-link'),
            onPressed: estaAceitando ? null : aoAceitar,
            icon: estaAceitando
                ? const SizedBox.square(
                    dimension: 18,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                : const Icon(Icons.login_rounded),
            label: const Text('Entrar no encontro'),
          ),
          const SizedBox(height: EspacamentosDoAplicativo.pequeno),
          const Text(
            'Ao entrar, as pessoas do encontro poderão ver seu nome e sua resposta de presença.',
            textAlign: TextAlign.center,
            style: TextStyle(
              color: CoresDoAplicativo.textoTerciario,
              fontSize: 12,
            ),
          ),
        ],
      ),
    );
  }
}

class _ErroDoConvite extends StatelessWidget {
  const _ErroDoConvite({
    required this.mensagem,
    required this.aoTentarNovamente,
  });

  final String mensagem;
  final VoidCallback aoTentarNovamente;

  @override
  Widget build(BuildContext context) {
    return CartaoDoAplicativo(
      preenchimento: const EdgeInsets.all(EspacamentosDoAplicativo.grande),
      filho: Column(
        mainAxisSize: MainAxisSize.min,
        children: <Widget>[
          const Icon(
            Icons.link_off_rounded,
            size: 44,
            color: CoresDoAplicativo.coral,
          ),
          const SizedBox(height: EspacamentosDoAplicativo.padrao),
          Text(
            mensagem,
            textAlign: TextAlign.center,
            style: Theme.of(context).textTheme.titleMedium,
          ),
          const SizedBox(height: EspacamentosDoAplicativo.padrao),
          OutlinedButton(
            onPressed: aoTentarNovamente,
            child: const Text('Tentar novamente'),
          ),
        ],
      ),
    );
  }
}
