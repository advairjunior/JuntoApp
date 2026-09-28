import 'dart:ui';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/acessibilidade/identificadores_semanticos.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/controlador_de_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/estado_da_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estrutura_responsiva_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/raios_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/sombras_do_aplicativo.dart';

class TelaDeEntrada extends ConsumerStatefulWidget {
  const TelaDeEntrada({
    this.cadastroFoiConcluido = false,
    this.retorno,
    super.key,
  });

  final bool cadastroFoiConcluido;
  final String? retorno;

  @override
  ConsumerState<TelaDeEntrada> createState() => _EstadoDaTelaDeEntrada();
}

class _EstadoDaTelaDeEntrada extends ConsumerState<TelaDeEntrada> {
  final GlobalKey<FormState> _chaveDoFormulario = GlobalKey<FormState>();
  final TextEditingController _controladorDoNumeroDeCelular =
      TextEditingController();
  final TextEditingController _controladorDoPin = TextEditingController();
  bool _pinEstaVisivel = false;

  @override
  void dispose() {
    _controladorDoNumeroDeCelular.dispose();
    _controladorDoPin.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    EstadoDaSessao sessao = ref.watch(provedorDoControladorDeSessao);

    return Scaffold(
      body: EstruturaResponsivaDoAplicativo(
        filho: Stack(
          fit: StackFit.expand,
          children: <Widget>[
            Image.asset(
              'assets/imagens/fundo_da_entrada.png',
              fit: BoxFit.cover,
              alignment: Alignment.topCenter,
            ),
            const DecoratedBox(
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  begin: Alignment.topCenter,
                  end: Alignment.bottomCenter,
                  stops: <double>[0, 0.42, 1],
                  colors: <Color>[
                    Color(0x52000000),
                    Color(0xA6000000),
                    Color(0xF2050D0B),
                  ],
                ),
              ),
            ),
            SafeArea(
              child: LayoutBuilder(
                builder: (BuildContext context, BoxConstraints limites) {
                  return SingleChildScrollView(
                    padding: const EdgeInsets.symmetric(
                      horizontal: EspacamentosDoAplicativo.grande,
                      vertical: EspacamentosDoAplicativo.extraGrande,
                    ),
                    child: ConstrainedBox(
                      constraints: BoxConstraints(
                        minHeight: limites.maxHeight -
                            (EspacamentosDoAplicativo.extraGrande * 2),
                      ),
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: <Widget>[
                          const _MarcaDoAplicativo(),
                          const SizedBox(
                            height: EspacamentosDoAplicativo.grande,
                          ),
                          _FormularioDeEntrada(
                            chaveDoFormulario: _chaveDoFormulario,
                            controladorDoNumeroDeCelular:
                                _controladorDoNumeroDeCelular,
                            controladorDoPin: _controladorDoPin,
                            sessao: sessao,
                            pinEstaVisivel: _pinEstaVisivel,
                            cadastroFoiConcluido: widget.cadastroFoiConcluido,
                            aoAlternarVisibilidadeDoPin: () {
                              setState(() {
                                _pinEstaVisivel = !_pinEstaVisivel;
                              });
                            },
                            aoEntrar: _entreAsync,
                            aoCriarConta: _abraCadastro,
                            valideNumeroDeCelular: _valideNumeroDeCelular,
                            validePin: _validePin,
                          ),
                        ],
                      ),
                    ),
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _entreAsync() async {
    FocusManager.instance.primaryFocus?.unfocus();

    if (!(_chaveDoFormulario.currentState?.validate() ?? false)) {
      return;
    }

    await ref.read(provedorDoControladorDeSessao.notifier).autentiqueAsync(
          numeroDeCelular: _controladorDoNumeroDeCelular.text.trim(),
          pin: _controladorDoPin.text,
        );
  }

  void _abraCadastro() {
    ref.read(provedorDoControladorDeSessao.notifier).limpeMensagemDeErro();
    String? retorno = widget.retorno;

    context.go(
      retorno == null
          ? '/cadastro'
          : '/cadastro?retorno=${Uri.encodeComponent(retorno)}',
    );
  }

  String? _valideNumeroDeCelular(String? numeroDeCelular) {
    String valor = numeroDeCelular?.trim() ?? '';

    if (valor.isEmpty) {
      return 'Informe seu celular.';
    }

    String apenasDigitos = valor.replaceAll(RegExp(r'\D'), '');

    if (apenasDigitos.length != 11 && apenasDigitos.length != 13) {
      return 'Informe um celular com DDD.';
    }

    return null;
  }

  String? _validePin(String? pin) {
    if (pin == null || !RegExp(r'^\d{6}$').hasMatch(pin)) {
      return 'O PIN deve ter exatamente 6 dígitos.';
    }

    return null;
  }
}

class _MarcaDoAplicativo extends StatelessWidget {
  const _MarcaDoAplicativo();

  @override
  Widget build(BuildContext context) {
    return Column(
      children: <Widget>[
        Container(
          width: 92,
          height: 92,
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(
            color: CoresDoAplicativo.fundoElevado.withValues(alpha: 0.9),
            borderRadius: BorderRadius.circular(RaiosDoAplicativo.extraGrande),
            border: Border.all(color: CoresDoAplicativo.bordaSuave),
            boxShadow: SombrasDoAplicativo.elevada,
          ),
          child: ClipRRect(
            borderRadius: BorderRadius.circular(RaiosDoAplicativo.grande),
            child: Image.asset(
              'assets/imagens/logo_junto.png',
              fit: BoxFit.cover,
            ),
          ),
        ),
        const SizedBox(height: EspacamentosDoAplicativo.medio),
        Text('Juntô', style: Theme.of(context).textTheme.headlineLarge),
        const SizedBox(height: EspacamentosDoAplicativo.pequeno),
        const Text.rich(
          TextSpan(
            children: <InlineSpan>[
              TextSpan(
                text: 'Grupos. ',
                style: TextStyle(color: CoresDoAplicativo.verdeDestaque),
              ),
              TextSpan(
                text: 'Encontros. ',
                style: TextStyle(color: CoresDoAplicativo.ambar),
              ),
              TextSpan(
                text: 'Memórias.',
                style: TextStyle(color: CoresDoAplicativo.coral),
              ),
            ],
          ),
          style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
        ),
        const SizedBox(height: EspacamentosDoAplicativo.pequeno),
        const Text(
          'Momentos reais e histórias que ficam.',
          textAlign: TextAlign.center,
          style: TextStyle(color: CoresDoAplicativo.textoSecundario),
        ),
      ],
    );
  }
}

class _FormularioDeEntrada extends StatelessWidget {
  const _FormularioDeEntrada({
    required this.chaveDoFormulario,
    required this.controladorDoNumeroDeCelular,
    required this.controladorDoPin,
    required this.sessao,
    required this.pinEstaVisivel,
    required this.cadastroFoiConcluido,
    required this.aoAlternarVisibilidadeDoPin,
    required this.aoEntrar,
    required this.aoCriarConta,
    required this.valideNumeroDeCelular,
    required this.validePin,
  });

  final GlobalKey<FormState> chaveDoFormulario;
  final TextEditingController controladorDoNumeroDeCelular;
  final TextEditingController controladorDoPin;
  final EstadoDaSessao sessao;
  final bool pinEstaVisivel;
  final bool cadastroFoiConcluido;
  final VoidCallback aoAlternarVisibilidadeDoPin;
  final VoidCallback aoEntrar;
  final VoidCallback aoCriarConta;
  final FormFieldValidator<String> valideNumeroDeCelular;
  final FormFieldValidator<String> validePin;

  @override
  Widget build(BuildContext context) {
    BorderRadius raio = BorderRadius.circular(RaiosDoAplicativo.grande);

    return ClipRRect(
      borderRadius: raio,
      child: BackdropFilter(
        filter: ImageFilter.blur(sigmaX: 18, sigmaY: 18),
        child: Container(
          padding: const EdgeInsets.all(EspacamentosDoAplicativo.grande),
          decoration: BoxDecoration(
            color: CoresDoAplicativo.fundoDoCartao.withValues(alpha: 0.9),
            borderRadius: raio,
            border: Border.all(color: CoresDoAplicativo.bordaSuave),
          ),
          child: Form(
            key: chaveDoFormulario,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: <Widget>[
                Text(
                  'Entre para continuar',
                  textAlign: TextAlign.center,
                  style: Theme.of(context).textTheme.titleLarge,
                ),
                if (cadastroFoiConcluido) ...<Widget>[
                  const SizedBox(height: EspacamentosDoAplicativo.padrao),
                  const Text(
                    'Conta criada. Agora entre com seus dados.',
                    textAlign: TextAlign.center,
                    style: TextStyle(color: CoresDoAplicativo.verdeDestaque),
                  ),
                ],
                const SizedBox(height: EspacamentosDoAplicativo.grande),
                Semantics(
                  identifier: IdentificadoresSemanticos.entradaCelular,
                  child: TextFormField(
                    controller: controladorDoNumeroDeCelular,
                    enabled: !sessao.operacaoEstaEmAndamento,
                    keyboardType: TextInputType.phone,
                    textInputAction: TextInputAction.next,
                    autofillHints: const <String>[
                      AutofillHints.telephoneNumber
                    ],
                    decoration: const InputDecoration(
                      labelText: 'Celular',
                      hintText: '(62) 99999-8888',
                      prefixIcon: Icon(Icons.phone_android_rounded),
                    ),
                    validator: valideNumeroDeCelular,
                  ),
                ),
                const SizedBox(height: EspacamentosDoAplicativo.medio),
                Semantics(
                  identifier: IdentificadoresSemanticos.entradaPin,
                  child: TextFormField(
                    controller: controladorDoPin,
                    enabled: !sessao.operacaoEstaEmAndamento,
                    keyboardType: TextInputType.number,
                    obscureText: !pinEstaVisivel,
                    textInputAction: TextInputAction.done,
                    autofillHints: const <String>[AutofillHints.password],
                    onFieldSubmitted: (_) => aoEntrar(),
                    decoration: InputDecoration(
                      labelText: 'PIN de 6 dígitos',
                      prefixIcon: const Icon(Icons.lock_outline_rounded),
                      suffixIcon: IconButton(
                        tooltip: pinEstaVisivel ? 'Ocultar PIN' : 'Mostrar PIN',
                        onPressed: sessao.operacaoEstaEmAndamento
                            ? null
                            : aoAlternarVisibilidadeDoPin,
                        icon: Icon(
                          pinEstaVisivel
                              ? Icons.visibility_off_outlined
                              : Icons.visibility_outlined,
                        ),
                      ),
                    ),
                    validator: validePin,
                  ),
                ),
                if (sessao.mensagemDeErro != null) ...<Widget>[
                  const SizedBox(height: EspacamentosDoAplicativo.padrao),
                  Text(
                    sessao.mensagemDeErro!,
                    textAlign: TextAlign.center,
                    style: const TextStyle(color: CoresDoAplicativo.coral),
                  ),
                ],
                const SizedBox(height: EspacamentosDoAplicativo.padrao),
                Semantics(
                  identifier: IdentificadoresSemanticos.entradaConfirmar,
                  child: FilledButton(
                    onPressed: sessao.operacaoEstaEmAndamento ? null : aoEntrar,
                    child: sessao.operacaoEstaEmAndamento
                        ? const SizedBox.square(
                            dimension: 22,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : const Text('Entrar'),
                  ),
                ),
                const SizedBox(height: EspacamentosDoAplicativo.medio),
                TextButton(
                  onPressed:
                      sessao.operacaoEstaEmAndamento ? null : aoCriarConta,
                  child: const Text('Ainda não tem conta?  Criar conta'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
