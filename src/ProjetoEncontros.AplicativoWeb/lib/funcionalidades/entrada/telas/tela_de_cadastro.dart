import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/controlador_de_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/estado_da_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/conteudo_responsivo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cartao_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';

class TelaDeCadastro extends ConsumerStatefulWidget {
  const TelaDeCadastro({
    this.retorno,
    super.key,
  });

  final String? retorno;

  @override
  ConsumerState<TelaDeCadastro> createState() => _EstadoDaTelaDeCadastro();
}

class _EstadoDaTelaDeCadastro extends ConsumerState<TelaDeCadastro> {
  final GlobalKey<FormState> _chaveDoFormulario = GlobalKey<FormState>();
  final TextEditingController _controladorDoNome = TextEditingController();
  final TextEditingController _controladorDoEmail = TextEditingController();
  final TextEditingController _controladorDaSenha = TextEditingController();
  bool _senhaEstaVisivel = false;

  @override
  void dispose() {
    _controladorDoNome.dispose();
    _controladorDoEmail.dispose();
    _controladorDaSenha.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    EstadoDaSessao sessao = ref.watch(provedorDoControladorDeSessao);

    return Scaffold(
      appBar: AppBar(
        backgroundColor: CoresDoAplicativo.fundoPrincipal,
        leading: IconButton(
          tooltip: 'Voltar',
          onPressed: sessao.operacaoEstaEmAndamento
              ? null
              : () => context.go(_crieRotaDeEntrada()),
          icon: const Icon(Icons.arrow_back_ios_new_rounded),
        ),
        title: const Text('Criar conta'),
      ),
      body: SafeArea(
        child: DecoratedBox(
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
          child: ConteudoResponsivo(
            filho: SingleChildScrollView(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: <Widget>[
                  Align(
                    child: ClipRRect(
                      borderRadius: BorderRadius.circular(20),
                      child: Image.asset(
                        'assets/imagens/logo_junto.png',
                        width: 72,
                        height: 72,
                        fit: BoxFit.cover,
                      ),
                    ),
                  ),
                  const SizedBox(height: EspacamentosDoAplicativo.padrao),
                  Text(
                    'Seu espaço no Juntô',
                    textAlign: TextAlign.center,
                    style: Theme.of(context).textTheme.headlineSmall,
                  ),
                  const SizedBox(height: EspacamentosDoAplicativo.pequeno),
                  const Text(
                    'Crie sua conta para organizar encontros privados com quem importa.',
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      color: CoresDoAplicativo.textoSecundario,
                    ),
                  ),
                  const SizedBox(
                    height: EspacamentosDoAplicativo.extraGrande,
                  ),
                  CartaoDoAplicativo(
                    elevado: true,
                    preenchimento: const EdgeInsets.all(
                      EspacamentosDoAplicativo.grande,
                    ),
                    filho: Form(
                      key: _chaveDoFormulario,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: <Widget>[
                          TextFormField(
                            controller: _controladorDoNome,
                            enabled: !sessao.operacaoEstaEmAndamento,
                            textInputAction: TextInputAction.next,
                            autofillHints: const <String>[AutofillHints.name],
                            decoration: const InputDecoration(
                              hintText: 'Seu nome',
                              prefixIcon: Icon(Icons.person_outline_rounded),
                            ),
                            validator: (String? nome) {
                              if (nome == null || nome.trim().isEmpty) {
                                return 'Informe seu nome.';
                              }

                              return null;
                            },
                          ),
                          const SizedBox(
                            height: EspacamentosDoAplicativo.medio,
                          ),
                          TextFormField(
                            controller: _controladorDoEmail,
                            enabled: !sessao.operacaoEstaEmAndamento,
                            keyboardType: TextInputType.emailAddress,
                            textInputAction: TextInputAction.next,
                            autofillHints: const <String>[AutofillHints.email],
                            decoration: const InputDecoration(
                              hintText: 'E-mail',
                              prefixIcon: Icon(Icons.mail_outline_rounded),
                            ),
                            validator: (String? email) {
                              String valor = email?.trim() ?? '';

                              if (valor.isEmpty) {
                                return 'Informe seu e-mail.';
                              }

                              if (!valor.contains('@') ||
                                  !valor.contains('.')) {
                                return 'Informe um e-mail válido.';
                              }

                              return null;
                            },
                          ),
                          const SizedBox(
                            height: EspacamentosDoAplicativo.medio,
                          ),
                          TextFormField(
                            controller: _controladorDaSenha,
                            enabled: !sessao.operacaoEstaEmAndamento,
                            obscureText: !_senhaEstaVisivel,
                            textInputAction: TextInputAction.done,
                            autofillHints: const <String>[
                              AutofillHints.newPassword,
                            ],
                            onFieldSubmitted: (_) => _cadastreAsync(),
                            decoration: InputDecoration(
                              hintText: 'Senha',
                              prefixIcon: const Icon(
                                Icons.lock_outline_rounded,
                              ),
                              suffixIcon: IconButton(
                                tooltip: _senhaEstaVisivel
                                    ? 'Ocultar senha'
                                    : 'Mostrar senha',
                                onPressed: sessao.operacaoEstaEmAndamento
                                    ? null
                                    : () {
                                        setState(() {
                                          _senhaEstaVisivel =
                                              !_senhaEstaVisivel;
                                        });
                                      },
                                icon: Icon(
                                  _senhaEstaVisivel
                                      ? Icons.visibility_off_outlined
                                      : Icons.visibility_outlined,
                                ),
                              ),
                            ),
                            validator: (String? senha) {
                              if (senha == null || senha.length < 8) {
                                return 'Use pelo menos 8 caracteres.';
                              }

                              if (senha.length > 100) {
                                return 'A senha deve ter no máximo 100 caracteres.';
                              }

                              return null;
                            },
                          ),
                          if (sessao.mensagemDeErro != null) ...<Widget>[
                            const SizedBox(
                              height: EspacamentosDoAplicativo.padrao,
                            ),
                            Text(
                              sessao.mensagemDeErro!,
                              textAlign: TextAlign.center,
                              style: const TextStyle(
                                color: CoresDoAplicativo.coral,
                              ),
                            ),
                          ],
                          const SizedBox(
                            height: EspacamentosDoAplicativo.grande,
                          ),
                          FilledButton(
                            onPressed: sessao.operacaoEstaEmAndamento
                                ? null
                                : _cadastreAsync,
                            child: sessao.operacaoEstaEmAndamento
                                ? const SizedBox.square(
                                    dimension: 22,
                                    child: CircularProgressIndicator(
                                      strokeWidth: 2,
                                    ),
                                  )
                                : const Text('Criar conta'),
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Future<void> _cadastreAsync() async {
    FocusManager.instance.primaryFocus?.unfocus();

    if (!(_chaveDoFormulario.currentState?.validate() ?? false)) {
      return;
    }

    bool cadastroFoiConcluido =
        await ref.read(provedorDoControladorDeSessao.notifier).cadastreAsync(
              nome: _controladorDoNome.text.trim(),
              email: _controladorDoEmail.text.trim(),
              senha: _controladorDaSenha.text,
            );

    if (cadastroFoiConcluido && mounted) {
      context.go(_crieRotaDeEntrada(cadastroFoiConcluido: true));
    }
  }

  String _crieRotaDeEntrada({bool cadastroFoiConcluido = false}) {
    Map<String, String> parametros = <String, String>{};

    if (cadastroFoiConcluido) {
      parametros['cadastro'] = 'concluido';
    }

    if (widget.retorno != null) {
      parametros['retorno'] = widget.retorno!;
    }

    return Uri(path: '/entrada', queryParameters: parametros).toString();
  }
}
