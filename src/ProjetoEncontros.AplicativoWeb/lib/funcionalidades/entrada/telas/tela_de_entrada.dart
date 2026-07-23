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
    super.key,
  });

  final bool cadastroFoiConcluido;

  @override
  ConsumerState<TelaDeEntrada> createState() => _EstadoDaTelaDeEntrada();
}

class _EstadoDaTelaDeEntrada extends ConsumerState<TelaDeEntrada> {
  final GlobalKey<FormState> _chaveDoFormulario = GlobalKey<FormState>();
  final TextEditingController _controladorDoEmail = TextEditingController();
  final TextEditingController _controladorDaSenha = TextEditingController();
  bool _senhaEstaVisivel = false;

  @override
  void dispose() {
    _controladorDoEmail.dispose();
    _controladorDaSenha.dispose();
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
                            controladorDoEmail: _controladorDoEmail,
                            controladorDaSenha: _controladorDaSenha,
                            sessao: sessao,
                            senhaEstaVisivel: _senhaEstaVisivel,
                            cadastroFoiConcluido: widget.cadastroFoiConcluido,
                            aoAlternarVisibilidadeDaSenha: () {
                              setState(() {
                                _senhaEstaVisivel = !_senhaEstaVisivel;
                              });
                            },
                            aoEntrar: _entreAsync,
                            aoCriarConta: _abraCadastro,
                            valideEmail: _valideEmail,
                            valideSenha: _valideSenha,
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
          email: _controladorDoEmail.text.trim(),
          senha: _controladorDaSenha.text,
        );
  }

  void _abraCadastro() {
    ref.read(provedorDoControladorDeSessao.notifier).limpeMensagemDeErro();
    context.go('/cadastro');
  }

  String? _valideEmail(String? email) {
    String valor = email?.trim() ?? '';

    if (valor.isEmpty) {
      return 'Informe seu e-mail.';
    }

    if (!valor.contains('@') || !valor.contains('.')) {
      return 'Informe um e-mail válido.';
    }

    return null;
  }

  String? _valideSenha(String? senha) {
    if (senha == null || senha.isEmpty) {
      return 'Informe sua senha.';
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
    required this.controladorDoEmail,
    required this.controladorDaSenha,
    required this.sessao,
    required this.senhaEstaVisivel,
    required this.cadastroFoiConcluido,
    required this.aoAlternarVisibilidadeDaSenha,
    required this.aoEntrar,
    required this.aoCriarConta,
    required this.valideEmail,
    required this.valideSenha,
  });

  final GlobalKey<FormState> chaveDoFormulario;
  final TextEditingController controladorDoEmail;
  final TextEditingController controladorDaSenha;
  final EstadoDaSessao sessao;
  final bool senhaEstaVisivel;
  final bool cadastroFoiConcluido;
  final VoidCallback aoAlternarVisibilidadeDaSenha;
  final VoidCallback aoEntrar;
  final VoidCallback aoCriarConta;
  final FormFieldValidator<String> valideEmail;
  final FormFieldValidator<String> valideSenha;

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
                  identifier: IdentificadoresSemanticos.entradaEmail,
                  child: TextFormField(
                    controller: controladorDoEmail,
                    enabled: !sessao.operacaoEstaEmAndamento,
                    keyboardType: TextInputType.emailAddress,
                    textInputAction: TextInputAction.next,
                    autofillHints: const <String>[AutofillHints.email],
                    decoration: const InputDecoration(
                      labelText: 'E-mail',
                      prefixIcon: Icon(Icons.mail_outline_rounded),
                    ),
                    validator: valideEmail,
                  ),
                ),
                const SizedBox(height: EspacamentosDoAplicativo.medio),
                Semantics(
                  identifier: IdentificadoresSemanticos.entradaSenha,
                  child: TextFormField(
                    controller: controladorDaSenha,
                    enabled: !sessao.operacaoEstaEmAndamento,
                    obscureText: !senhaEstaVisivel,
                    textInputAction: TextInputAction.done,
                    autofillHints: const <String>[AutofillHints.password],
                    onFieldSubmitted: (_) => aoEntrar(),
                    decoration: InputDecoration(
                      labelText: 'Senha',
                      prefixIcon: const Icon(Icons.lock_outline_rounded),
                      suffixIcon: IconButton(
                        tooltip: senhaEstaVisivel
                            ? 'Ocultar senha'
                            : 'Mostrar senha',
                        onPressed: sessao.operacaoEstaEmAndamento
                            ? null
                            : aoAlternarVisibilidadeDaSenha,
                        icon: Icon(
                          senhaEstaVisivel
                              ? Icons.visibility_off_outlined
                              : Icons.visibility_outlined,
                        ),
                      ),
                    ),
                    validator: valideSenha,
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
