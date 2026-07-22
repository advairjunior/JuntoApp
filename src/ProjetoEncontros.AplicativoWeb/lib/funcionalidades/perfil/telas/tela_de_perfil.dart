import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/controlador_de_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cabecalho_da_pagina.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/cartao_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/conteudo_responsivo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/estado_vazio.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/componentes/titulo_de_secao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/configuracao/configuracao_do_ambiente.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/foto_de_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/imagem_privada.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/repositorio_de_imagens_privadas.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/raios_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/dados/repositorio_da_pagina_inicial.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/estado/controlador_da_pagina_inicial.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/usuario_atual.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/servicos/seletor_de_imagem.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/perfil/dados/repositorio_do_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/pessoas_frequentes/dados/repositorio_de_pessoas_frequentes.dart';

final provedorDoUsuarioAtualNoPerfil = FutureProvider.autoDispose<UsuarioAtual>(
  (Ref referencia) {
    return referencia
        .watch(provedorDoRepositorioDaPaginaInicial)
        .obtenhaUsuarioAtualAsync();
  },
);

class TelaDePerfil extends ConsumerStatefulWidget {
  const TelaDePerfil({super.key});

  @override
  ConsumerState<TelaDePerfil> createState() => _EstadoDaTelaDePerfil();
}

class _EstadoDaTelaDePerfil extends ConsumerState<TelaDePerfil> {
  bool _estaAlterandoFoto = false;
  bool _estaAlterandoNome = false;
  String? _mensagemDaFoto;
  String? _mensagemDoNome;
  bool _mensagemIndicaErro = false;
  bool _mensagemDoNomeIndicaErro = false;

  @override
  Widget build(BuildContext context) {
    AsyncValue<UsuarioAtual> usuarioAtual = ref.watch(
      provedorDoUsuarioAtualNoPerfil,
    );

    return ConteudoResponsivo(
      preenchimento: const EdgeInsets.all(EspacamentosDoAplicativo.padrao),
      filho: usuarioAtual.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (_, __) => _ErroDoPerfil(
          aoTentarNovamente: () => ref.invalidate(
            provedorDoUsuarioAtualNoPerfil,
          ),
        ),
        data: (UsuarioAtual usuario) => _ConteudoDoPerfil(
          usuario: usuario,
          estaAlterandoFoto: _estaAlterandoFoto,
          estaAlterandoNome: _estaAlterandoNome,
          mensagemDaFoto: _mensagemDaFoto,
          mensagemDoNome: _mensagemDoNome,
          mensagemIndicaErro: _mensagemIndicaErro,
          mensagemDoNomeIndicaErro: _mensagemDoNomeIndicaErro,
          aoTocarNaFoto: () => _aoTocarNaFotoAsync(usuario),
          aoEditarNome: () => _abraEditorDoNomeAsync(usuario),
          aoSair: () => ref
              .read(provedorDoControladorDeSessao.notifier)
              .encerreSessaoAsync(),
        ),
      ),
    );
  }

  Future<void> _aoTocarNaFotoAsync(UsuarioAtual usuario) async {
    if (_estaAlterandoFoto) {
      return;
    }

    String recurso = ConfiguracaoDoAmbiente.crieUrlAbsoluta(
      usuario.urlDaFotoDePerfil,
    );

    if (recurso.isEmpty) {
      await _abraOpcoesDaFotoAsync(usuario, false);
      return;
    }

    await showDialog<void>(
      context: context,
      useSafeArea: false,
      builder: (BuildContext contextoDoDialogo) {
        return Dialog.fullscreen(
          backgroundColor: Colors.black,
          child: Scaffold(
            backgroundColor: Colors.black,
            appBar: AppBar(
              backgroundColor: Colors.black,
              foregroundColor: Colors.white,
              title: const Text('Foto de perfil'),
              leading: IconButton(
                tooltip: 'Fechar',
                onPressed: () => contextoDoDialogo.pop(),
                icon: const Icon(Icons.close_rounded),
              ),
              actions: <Widget>[
                IconButton(
                  key: const Key('editar-foto-do-perfil'),
                  tooltip: 'Editar foto de perfil',
                  onPressed: () {
                    contextoDoDialogo.pop();
                    _abraOpcoesDaFotoAsync(usuario, true);
                  },
                  icon: const Icon(Icons.edit_outlined),
                ),
              ],
            ),
            body: InteractiveViewer(
              minScale: 0.8,
              maxScale: 4,
              child: Center(
                child: ImagemPrivada(
                  recurso: recurso,
                  ajuste: BoxFit.contain,
                  construaSubstituta: (_) => const Icon(
                    Icons.broken_image_outlined,
                    color: Colors.white54,
                    size: 52,
                  ),
                ),
              ),
            ),
          ),
        );
      },
    );
  }

  Future<void> _abraOpcoesDaFotoAsync(
    UsuarioAtual usuario,
    bool temFoto,
  ) async {
    String? acao = await showModalBottomSheet<String>(
      context: context,
      backgroundColor: CoresDoAplicativo.fundoDoCartao,
      showDragHandle: true,
      builder: (BuildContext contextoDaFolha) {
        return SafeArea(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(16, 0, 16, 20),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: <Widget>[
                Text(
                  'Foto de perfil',
                  style: Theme.of(context).textTheme.titleLarge,
                ),
                const SizedBox(height: EspacamentosDoAplicativo.medio),
                ListTile(
                  key: const Key('tirar-foto-do-perfil'),
                  leading: const Icon(Icons.camera_alt_outlined),
                  title: const Text('Tirar foto'),
                  onTap: () => contextoDaFolha.pop('camera'),
                ),
                ListTile(
                  key: const Key('escolher-foto-do-perfil'),
                  leading: const Icon(Icons.photo_library_outlined),
                  title: const Text('Escolher da galeria'),
                  onTap: () => contextoDaFolha.pop('galeria'),
                ),
                if (temFoto)
                  ListTile(
                    key: const Key('remover-foto-do-perfil'),
                    leading: const Icon(
                      Icons.no_photography_outlined,
                      color: CoresDoAplicativo.coral,
                    ),
                    title: const Text(
                      'Remover foto',
                      style: TextStyle(color: CoresDoAplicativo.coral),
                    ),
                    onTap: () => contextoDaFolha.pop('remover'),
                  ),
              ],
            ),
          ),
        );
      },
    );

    if (acao == 'camera') {
      await _altereFotoAsync(
        usuario,
        EnumeradorDeOrigemDaImagem.camera,
      );
    } else if (acao == 'galeria') {
      await _altereFotoAsync(
        usuario,
        EnumeradorDeOrigemDaImagem.galeria,
      );
    } else if (acao == 'remover') {
      await _confirmeRemocaoAsync(usuario);
    }
  }

  Future<void> _altereFotoAsync(
    UsuarioAtual usuario,
    EnumeradorDeOrigemDaImagem origem,
  ) async {
    ImagemSelecionada? imagem = await ref
        .read(provedorDoSeletorDeImagem)
        .selecionePorOrigemAsync(origem);

    if (imagem == null || !mounted) {
      return;
    }

    await _executeAlteracaoAsync(
      usuario: usuario,
      acao: () => ref.read(provedorDoRepositorioDoPerfil).altereFotoAsync(
            nomeDoArquivo: imagem.nome,
            tipoDeConteudo: imagem.tipoDeConteudo,
            conteudo: imagem.conteudo,
          ),
      mensagemDeSucesso: 'Foto de perfil atualizada.',
    );
  }

  Future<void> _abraEditorDoNomeAsync(UsuarioAtual usuario) async {
    if (_estaAlterandoNome) {
      return;
    }

    String nomeInformado = usuario.nome;
    GlobalKey<FormState> chaveDoFormulario = GlobalKey<FormState>();
    String? novoNome = await showDialog<String>(
      context: context,
      builder: (BuildContext contextoDoDialogo) => AlertDialog(
        title: const Text('Editar nome'),
        content: Form(
          key: chaveDoFormulario,
          child: TextFormField(
            key: const Key('campo-do-nome-do-perfil'),
            initialValue: usuario.nome,
            autofocus: true,
            maxLength: 120,
            textCapitalization: TextCapitalization.words,
            decoration: const InputDecoration(labelText: 'Nome'),
            validator: (String? valor) {
              if (valor == null || valor.trim().isEmpty) {
                return 'Informe seu nome.';
              }

              return null;
            },
            onChanged: (String valor) => nomeInformado = valor,
            onFieldSubmitted: (String valor) {
              nomeInformado = valor;
              _confirmeNome(
                contextoDoDialogo,
                chaveDoFormulario,
                nomeInformado,
              );
            },
          ),
        ),
        actions: <Widget>[
          TextButton(
            onPressed: () => contextoDoDialogo.pop(),
            child: const Text('Cancelar'),
          ),
          FilledButton(
            key: const Key('salvar-nome-do-perfil'),
            onPressed: () => _confirmeNome(
              contextoDoDialogo,
              chaveDoFormulario,
              nomeInformado,
            ),
            child: const Text('Salvar'),
          ),
        ],
      ),
    );

    if (novoNome == null || novoNome == usuario.nome || !mounted) {
      return;
    }

    setState(() {
      _estaAlterandoNome = true;
      _mensagemDoNome = null;
    });

    try {
      await ref.read(provedorDoRepositorioDoPerfil).altereNomeAsync(novoNome);
      ref.invalidate(provedorDoUsuarioAtualNoPerfil);
      ref.invalidate(provedorDoControladorDaPaginaInicial);
      ref.invalidate(provedorDasPessoasFrequentes);

      if (mounted) {
        setState(() {
          _mensagemDoNome = 'Nome atualizado.';
          _mensagemDoNomeIndicaErro = false;
        });
      }
    } on ExcecaoDaApi catch (excecao) {
      if (mounted) {
        setState(() {
          _mensagemDoNome = excecao.mensagem;
          _mensagemDoNomeIndicaErro = true;
        });
      }
    } catch (_) {
      if (mounted) {
        setState(() {
          _mensagemDoNome = 'Não foi possível alterar seu nome.';
          _mensagemDoNomeIndicaErro = true;
        });
      }
    } finally {
      if (mounted) {
        setState(() {
          _estaAlterandoNome = false;
        });
      }
    }
  }

  void _confirmeNome(
    BuildContext contextoDoDialogo,
    GlobalKey<FormState> chaveDoFormulario,
    String nomeInformado,
  ) {
    if (chaveDoFormulario.currentState?.validate() ?? false) {
      contextoDoDialogo.pop(nomeInformado.trim());
    }
  }

  Future<void> _confirmeRemocaoAsync(UsuarioAtual usuario) async {
    bool confirmou = await showDialog<bool>(
          context: context,
          builder: (BuildContext contextoDoDialogo) => AlertDialog(
            title: const Text('Remover foto de perfil?'),
            content: const Text('Suas iniciais voltarão a ser exibidas.'),
            actions: <Widget>[
              TextButton(
                onPressed: () => contextoDoDialogo.pop(false),
                child: const Text('Manter foto'),
              ),
              FilledButton(
                key: const Key('confirmar-remocao-da-foto-do-perfil'),
                onPressed: () => contextoDoDialogo.pop(true),
                style: FilledButton.styleFrom(
                  backgroundColor: CoresDoAplicativo.coral,
                ),
                child: const Text('Remover foto'),
              ),
            ],
          ),
        ) ??
        false;

    if (confirmou) {
      await _executeAlteracaoAsync(
        usuario: usuario,
        acao: () => ref.read(provedorDoRepositorioDoPerfil).removaFotoAsync(),
        mensagemDeSucesso: 'Foto de perfil removida.',
      );
    }
  }

  Future<void> _executeAlteracaoAsync({
    required UsuarioAtual usuario,
    required Future<UsuarioAtual> Function() acao,
    required String mensagemDeSucesso,
  }) async {
    setState(() {
      _estaAlterandoFoto = true;
      _mensagemDaFoto = null;
    });

    try {
      UsuarioAtual usuarioAtualizado = await acao();
      _invalideImagens(
        usuario.urlDaFotoDePerfil,
        usuarioAtualizado.urlDaFotoDePerfil,
      );
      ref.invalidate(provedorDoUsuarioAtualNoPerfil);
      ref.invalidate(provedorDoControladorDaPaginaInicial);
      ref.invalidate(provedorDasPessoasFrequentes);

      if (mounted) {
        setState(() {
          _mensagemDaFoto = mensagemDeSucesso;
          _mensagemIndicaErro = false;
        });
      }
    } on ExcecaoDaApi catch (excecao) {
      if (mounted) {
        setState(() {
          _mensagemDaFoto = excecao.mensagem;
          _mensagemIndicaErro = true;
        });
      }
    } catch (_) {
      if (mounted) {
        setState(() {
          _mensagemDaFoto = 'Não foi possível atualizar sua foto de perfil.';
          _mensagemIndicaErro = true;
        });
      }
    } finally {
      if (mounted) {
        setState(() {
          _estaAlterandoFoto = false;
        });
      }
    }
  }

  void _invalideImagens(String? recursoAnterior, String? recursoAtual) {
    String urlAnterior = ConfiguracaoDoAmbiente.crieUrlAbsoluta(
      recursoAnterior,
    );
    String urlAtual = ConfiguracaoDoAmbiente.crieUrlAbsoluta(recursoAtual);

    if (urlAnterior.isNotEmpty) {
      ref.invalidate(provedorDosBytesDaImagemPrivada(urlAnterior));
    }

    if (urlAtual.isNotEmpty) {
      ref.invalidate(provedorDosBytesDaImagemPrivada(urlAtual));
    }
  }
}

class _ConteudoDoPerfil extends StatelessWidget {
  const _ConteudoDoPerfil({
    required this.usuario,
    required this.estaAlterandoFoto,
    required this.estaAlterandoNome,
    required this.mensagemIndicaErro,
    required this.mensagemDoNomeIndicaErro,
    required this.aoTocarNaFoto,
    required this.aoEditarNome,
    required this.aoSair,
    this.mensagemDaFoto,
    this.mensagemDoNome,
  });

  final UsuarioAtual usuario;
  final bool estaAlterandoFoto;
  final bool estaAlterandoNome;
  final String? mensagemDaFoto;
  final String? mensagemDoNome;
  final bool mensagemIndicaErro;
  final bool mensagemDoNomeIndicaErro;
  final VoidCallback aoTocarNaFoto;
  final VoidCallback aoEditarNome;
  final Future<void> Function() aoSair;

  @override
  Widget build(BuildContext context) {
    return ListView(
      children: <Widget>[
        const CabecalhoDaPagina(
          titulo: 'Perfil',
          subtitulo: 'Sua conta e preferências do Juntô.',
        ),
        const SizedBox(height: EspacamentosDoAplicativo.grande),
        CartaoDoAplicativo(
          key: const Key('dados-do-perfil'),
          elevado: true,
          preenchimento: const EdgeInsets.symmetric(
            horizontal: EspacamentosDoAplicativo.grande,
            vertical: EspacamentosDoAplicativo.extraGrande,
          ),
          filho: Column(
            children: <Widget>[
              Semantics(
                button: true,
                label: usuario.urlDaFotoDePerfil == null ||
                        usuario.urlDaFotoDePerfil!.trim().isEmpty
                    ? 'Adicionar foto de perfil'
                    : 'Abrir foto de perfil',
                child: InkWell(
                  key: const Key('abrir-foto-do-perfil'),
                  onTap: aoTocarNaFoto,
                  customBorder: const CircleBorder(),
                  child: Stack(
                    clipBehavior: Clip.none,
                    children: <Widget>[
                      FotoDePerfil(
                        key: const Key('foto-do-usuario-no-perfil'),
                        url: usuario.urlDaFotoDePerfil,
                        iniciais: usuario.iniciais,
                        dimensao: 92,
                        tamanhoDasIniciais: 30,
                      ),
                      Positioned(
                        right: -2,
                        bottom: -2,
                        child: Container(
                          width: 32,
                          height: 32,
                          decoration: const BoxDecoration(
                            shape: BoxShape.circle,
                            color: CoresDoAplicativo.verdeDestaque,
                          ),
                          child: estaAlterandoFoto
                              ? const Padding(
                                  padding: EdgeInsets.all(8),
                                  child: CircularProgressIndicator(
                                    strokeWidth: 2,
                                    color: Colors.white,
                                  ),
                                )
                              : const Icon(
                                  Icons.camera_alt_outlined,
                                  size: 17,
                                  color: Colors.white,
                                ),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
              if (mensagemDaFoto != null) ...<Widget>[
                const SizedBox(height: EspacamentosDoAplicativo.medio),
                Text(
                  mensagemDaFoto!,
                  textAlign: TextAlign.center,
                  style: TextStyle(
                    color: mensagemIndicaErro
                        ? CoresDoAplicativo.coral
                        : CoresDoAplicativo.verdeDestaque,
                  ),
                ),
              ],
              const SizedBox(height: EspacamentosDoAplicativo.padrao),
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: <Widget>[
                  Flexible(
                    child: Text(
                      usuario.nome,
                      textAlign: TextAlign.center,
                      style: Theme.of(context).textTheme.headlineSmall,
                    ),
                  ),
                  const SizedBox(width: EspacamentosDoAplicativo.minimo),
                  if (estaAlterandoNome)
                    const SizedBox.square(
                      dimension: 24,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  else
                    IconButton(
                      key: const Key('editar-nome-do-perfil'),
                      tooltip: 'Editar nome',
                      onPressed: aoEditarNome,
                      icon: const Icon(Icons.edit_outlined),
                    ),
                ],
              ),
              if (mensagemDoNome != null)
                Text(
                  mensagemDoNome!,
                  textAlign: TextAlign.center,
                  style: TextStyle(
                    color: mensagemDoNomeIndicaErro
                        ? CoresDoAplicativo.coral
                        : CoresDoAplicativo.verdeDestaque,
                  ),
                ),
              const SizedBox(height: EspacamentosDoAplicativo.minimo),
              Text(
                'Membro do Juntô',
                textAlign: TextAlign.center,
                style: const TextStyle(
                  color: CoresDoAplicativo.textoSecundario,
                ),
              ),
            ],
          ),
        ),
        const SizedBox(height: EspacamentosDoAplicativo.grande),
        const TituloDeSecao(titulo: 'Conta'),
        const SizedBox(height: EspacamentosDoAplicativo.medio),
        CartaoDoAplicativo(
          preenchimento: EdgeInsets.zero,
          filho: Column(
            children: <Widget>[
              _LinhaDoPerfil(
                icone: Icons.mail_outline_rounded,
                titulo: 'E-mail',
                valor: usuario.email,
              ),
              const Divider(height: 1, indent: 56),
              const _LinhaDoPerfil(
                icone: Icons.lock_outline_rounded,
                titulo: 'Privacidade',
                valor: 'Conta e encontros privados',
              ),
            ],
          ),
        ),
        const SizedBox(height: EspacamentosDoAplicativo.grande),
        const TituloDeSecao(titulo: 'Preferências'),
        const SizedBox(height: EspacamentosDoAplicativo.medio),
        CartaoDoAplicativo(
          preenchimento: EdgeInsets.zero,
          filho: _LinhaDoPerfil(
            icone: Icons.notifications_none_rounded,
            titulo: 'Notificações',
            valor: 'Escolha quais avisos deseja receber',
            aoTocar: () => context.push<void>('/perfil/notificacoes'),
          ),
        ),
        const SizedBox(height: EspacamentosDoAplicativo.grande),
        const TituloDeSecao(titulo: 'Sessão'),
        const SizedBox(height: EspacamentosDoAplicativo.medio),
        CartaoDoAplicativo(
          preenchimento: EdgeInsets.zero,
          filho: Semantics(
            button: true,
            label: 'Sair da conta',
            child: InkWell(
              key: const Key('sair-da-conta'),
              onTap: aoSair,
              borderRadius: BorderRadius.circular(RaiosDoAplicativo.grande),
              child: const Padding(
                padding: EdgeInsets.symmetric(
                  horizontal: EspacamentosDoAplicativo.padrao,
                  vertical: EspacamentosDoAplicativo.medio,
                ),
                child: Row(
                  children: <Widget>[
                    SizedBox.square(
                      dimension: 40,
                      child: Icon(
                        Icons.logout_rounded,
                        color: CoresDoAplicativo.perigo,
                      ),
                    ),
                    SizedBox(width: EspacamentosDoAplicativo.pequeno),
                    Expanded(
                      child: Text(
                        'Sair da conta',
                        style: TextStyle(
                          color: CoresDoAplicativo.perigo,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
        const SizedBox(height: EspacamentosDoAplicativo.grande),
      ],
    );
  }
}

class _LinhaDoPerfil extends StatelessWidget {
  const _LinhaDoPerfil({
    required this.icone,
    required this.titulo,
    required this.valor,
    this.aoTocar,
  });

  final IconData icone;
  final String titulo;
  final String valor;
  final VoidCallback? aoTocar;

  @override
  Widget build(BuildContext context) {
    Widget conteudo = Padding(
      padding: const EdgeInsets.symmetric(
        horizontal: EspacamentosDoAplicativo.padrao,
        vertical: EspacamentosDoAplicativo.pequeno,
      ),
      child: ConstrainedBox(
        constraints: const BoxConstraints(minHeight: 48),
        child: Row(
          children: <Widget>[
            Container(
              width: 40,
              height: 40,
              decoration: BoxDecoration(
                color: CoresDoAplicativo.fundoDoCartaoSuave,
                borderRadius: BorderRadius.circular(RaiosDoAplicativo.pequeno),
              ),
              child: Icon(
                icone,
                size: 20,
                color: CoresDoAplicativo.verdeDestaque,
              ),
            ),
            const SizedBox(width: EspacamentosDoAplicativo.medio),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: <Widget>[
                  Text(titulo, style: Theme.of(context).textTheme.bodyLarge),
                  const SizedBox(height: 2),
                  Text(
                    valor,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      color: CoresDoAplicativo.textoTerciario,
                      fontSize: 13,
                    ),
                  ),
                ],
              ),
            ),
            if (aoTocar != null)
              const Icon(
                Icons.chevron_right_rounded,
                color: CoresDoAplicativo.textoTerciario,
              ),
          ],
        ),
      ),
    );

    if (aoTocar == null) {
      return conteudo;
    }

    return InkWell(
      key: const Key('abrir-preferencias-de-notificacao'),
      onTap: aoTocar,
      borderRadius: BorderRadius.circular(RaiosDoAplicativo.grande),
      child: conteudo,
    );
  }
}

class _ErroDoPerfil extends StatelessWidget {
  const _ErroDoPerfil({required this.aoTentarNovamente});

  final VoidCallback aoTentarNovamente;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: CartaoDoAplicativo(
        filho: EstadoVazio(
          icone: Icons.person_off_outlined,
          titulo: 'Não foi possível carregar seu perfil',
          descricao: 'Verifique sua conexão e tente novamente.',
          acao: FilledButton(
            onPressed: aoTentarNovamente,
            child: const Text('Tentar novamente'),
          ),
        ),
      ),
    );
  }
}
