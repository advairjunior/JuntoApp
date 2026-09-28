import 'dart:convert';
import 'dart:typed_data';
import 'dart:ui' show SemanticsFlag;

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/repositorio_de_autenticacao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/resposta_de_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/repositorio_de_imagens_privadas.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/instalacao/contrato_do_servico_de_instalacao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/instalacao/servico_de_instalacao.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/servicos/seletor_de_imagem.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/dados/repositorio_da_pagina_inicial.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/encontro_resumo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/usuario_atual.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/perfil/dados/repositorio_do_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/inicializacao/aplicativo.dart';

void main() {
  test('repositorio deve enviar somente celular e PIN na autenticacao',
      () async {
    AdaptadorHttpFalso adaptador = AdaptadorHttpFalso();
    Dio cliente = Dio()..httpClientAdapter = adaptador;
    RepositorioDeAutenticacao repositorio = RepositorioDeAutenticacao(cliente);

    await repositorio.autentiqueAsync(
      numeroDeCelular: '62999998888',
      pin: '123456',
    );

    Map<String, dynamic> dados =
        Map<String, dynamic>.from(adaptador.ultimaRequisicao!.data as Map);
    expect(dados, <String, String>{
      'numeroDeCelular': '62999998888',
      'pin': '123456',
    });
    expect(dados.containsKey('email'), isFalse);
    expect(dados.containsKey('senha'), isFalse);
  });

  test('repositorio deve enviar somente nome celular e PIN no cadastro',
      () async {
    AdaptadorHttpFalso adaptador = AdaptadorHttpFalso();
    Dio cliente = Dio()..httpClientAdapter = adaptador;
    RepositorioDeAutenticacao repositorio = RepositorioDeAutenticacao(cliente);

    await repositorio.cadastreAsync(
      nome: 'Pessoa Teste',
      numeroDeCelular: '62999998888',
      pin: '123456',
    );

    Map<String, dynamic> dados =
        Map<String, dynamic>.from(adaptador.ultimaRequisicao!.data as Map);
    expect(dados, <String, String>{
      'nome': 'Pessoa Teste',
      'numeroDeCelular': '62999998888',
      'pin': '123456',
    });
    expect(dados.containsKey('email'), isFalse);
    expect(dados.containsKey('senha'), isFalse);
  });

  testWidgets('deve abrir a entrada quando nao houver sessao', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso();

    await testador.pumpWidget(_crieAplicativo(repositorio));
    await testador.pumpAndSettle();

    expect(find.text('Juntô'), findsOneWidget);
    expect(find.text('Entre para continuar'), findsOneWidget);
    expect(find.text('Entrar'), findsOneWidget);
  });

  testWidgets('deve autenticar e abrir o novo nucleo', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso();

    await testador.pumpWidget(_crieAplicativo(repositorio));
    await testador.pumpAndSettle();
    await testador.enterText(
      find.widgetWithText(TextFormField, 'Celular'),
      '(62) 99999-8888',
    );
    await testador.enterText(
      find.widgetWithText(TextFormField, 'PIN de 6 dígitos'),
      '123456',
    );

    Finder formularioDoCelular = find.widgetWithText(TextFormField, 'Celular');
    Finder formularioDoPin =
        find.widgetWithText(TextFormField, 'PIN de 6 dígitos');
    TextField campoDoCelular = testador.widget(
      find.descendant(
        of: formularioDoCelular,
        matching: find.byType(TextField),
      ),
    );
    TextField campoDoPin = testador.widget(
      find.descendant(
        of: formularioDoPin,
        matching: find.byType(TextField),
      ),
    );
    expect(campoDoCelular.keyboardType, TextInputType.phone);
    expect(campoDoPin.keyboardType, TextInputType.number);
    expect(campoDoPin.obscureText, isTrue);

    Finder botaoDeEntrada = find.widgetWithText(FilledButton, 'Entrar');
    await testador.ensureVisible(botaoDeEntrada);
    await testador.tap(botaoDeEntrada);
    await testador.pumpAndSettle();

    expect(repositorio.numeroDeCelularDoUltimoLogin, '(62) 99999-8888');
    expect(repositorio.pinDoUltimoLogin, '123456');
    expect(find.text('Seus encontros aparecerão aqui'), findsOneWidget);
    expect(find.text('Café de domingo'), findsNothing);
  });

  testWidgets('deve exigir PIN numerico com seis digitos', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso();

    await testador.pumpWidget(_crieAplicativo(repositorio));
    await testador.pumpAndSettle();
    await testador.enterText(
      find.widgetWithText(TextFormField, 'Celular'),
      '62999998888',
    );
    await testador.enterText(
      find.widgetWithText(TextFormField, 'PIN de 6 dígitos'),
      '12345',
    );
    await testador.tap(find.widgetWithText(FilledButton, 'Entrar'));
    await testador.pumpAndSettle();

    expect(find.text('O PIN deve ter exatamente 6 dígitos.'), findsOneWidget);
    expect(repositorio.numeroDeCelularDoUltimoLogin, isNull);
    expect(find.text('E-mail'), findsNothing);
    expect(find.text('Senha'), findsNothing);
  });

  testWidgets('deve restaurar sessao existente ao iniciar', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);

    await testador.pumpWidget(_crieAplicativo(repositorio));
    await testador.pumpAndSettle();

    expect(find.text('Seus encontros aparecerão aqui'), findsOneWidget);
    expect(find.text('Entre para continuar'), findsNothing);
  });

  testWidgets('deve exibir somente o novo nucleo no dock', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);

    await testador.pumpWidget(_crieAplicativo(repositorio));
    await testador.pumpAndSettle();

    Finder inicio = find.byKey(const Key('dock-início'));
    Finder grupos = find.byKey(const Key('dock-grupos'));
    Finder pessoas = find.byKey(const Key('dock-pessoas'));
    Finder perfil = find.byKey(const Key('dock-perfil'));
    expect(inicio, findsOneWidget);
    expect(grupos, findsOneWidget);
    expect(pessoas, findsOneWidget);
    expect(perfil, findsOneWidget);
    expect(find.text('Memórias'), findsNothing);
    expect(find.text('Notificações'), findsNothing);
    expect(
      testador.getCenter(inicio).dx,
      lessThan(testador.getCenter(grupos).dx),
    );
    expect(
      testador.getCenter(grupos).dx,
      lessThan(testador.getCenter(pessoas).dx),
    );
    expect(
      testador.getCenter(pessoas).dx,
      lessThan(testador.getCenter(perfil).dx),
    );
    expect(
      testador.getSemantics(inicio).hasFlag(SemanticsFlag.isSelected),
      isTrue,
    );

    await testador.tap(find.text('Grupos'));
    await testador.pumpAndSettle();
    expect(find.text('Seus grupos aparecerão aqui'), findsOneWidget);
    expect(
      testador.getSemantics(grupos).hasFlag(SemanticsFlag.isSelected),
      isTrue,
    );

    await testador.tap(find.text('Pessoas'));
    await testador.pumpAndSettle();
    expect(find.text('As pessoas aparecerão aqui'), findsOneWidget);
    expect(
      testador.getSemantics(pessoas).hasFlag(SemanticsFlag.isSelected),
      isTrue,
    );
  });

  testWidgets('deve apresentar os dados privados no perfil', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);

    await testador.pumpWidget(_crieAplicativo(repositorio));
    await testador.pumpAndSettle();
    await testador.tap(find.text('Perfil'));
    await testador.pumpAndSettle();

    expect(find.byKey(const Key('dados-do-perfil')), findsOneWidget);
    expect(find.byKey(const Key('foto-do-usuario-no-perfil')), findsOneWidget);
    expect(find.text('Pessoa Teste'), findsOneWidget);
    expect(find.text('+5562999998888'), findsOneWidget);
    expect(find.text('Notificações'), findsNothing);
  });

  testWidgets('deve alterar o nome exibido no perfil', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDoPerfilFalso perfil = RepositorioDoPerfilFalso();

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDoPerfil: perfil,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Perfil'));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('editar-nome-do-perfil')));
    await testador.pumpAndSettle();
    await testador.enterText(
      find.byKey(const Key('campo-do-nome-do-perfil')),
      'Pessoa Atualizada',
    );
    await testador.tap(find.byKey(const Key('salvar-nome-do-perfil')));
    await testador.pumpAndSettle();

    expect(perfil.ultimoNome, 'Pessoa Atualizada');
    expect(find.text('Nome atualizado.'), findsOneWidget);
  });

  testWidgets('deve adicionar uma foto de perfil', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDaPaginaInicialFalso paginaInicial =
        RepositorioDaPaginaInicialFalso();
    RepositorioDoPerfilFalso perfil = RepositorioDoPerfilFalso(
      aoAtualizarUrl: (String? url) {
        paginaInicial.urlDaFotoDePerfil = url;
      },
    );
    RepositorioDeImagensPrivadasFalso imagens =
        RepositorioDeImagensPrivadasFalso();

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDaPaginaInicial: paginaInicial,
        repositorioDoPerfil: perfil,
        seletorDeImagem: SeletorDeImagemFalso(),
        repositorioDeImagens: imagens,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Perfil'));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('abrir-foto-do-perfil')));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('escolher-foto-do-perfil')));
    await testador.pumpAndSettle();

    expect(perfil.nomeDaUltimaFoto, 'perfil.png');
    expect(paginaInicial.urlDaFotoDePerfil, '/arquivos/usuarios/perfil.png');
    expect(find.text('Foto de perfil atualizada.'), findsOneWidget);
    expect(imagens.quantidadeDeBuscas, greaterThan(0));
  });

  testWidgets('deve orientar a instalacao pelo Safari no iPhone', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    ServicoDeInstalacaoFalso instalacao = ServicoDeInstalacaoFalso(
      situacao: SituacaoDaInstalacao.requerOrientacaoNoIos,
    );

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        servicoDeInstalacao: instalacao,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Perfil'));
    await testador.pumpAndSettle();
    await testador.scrollUntilVisible(
      find.byKey(const Key('instalar-aplicativo')),
      250,
      scrollable: find.byType(Scrollable).last,
    );
    await testador.tap(find.byKey(const Key('instalar-aplicativo')));
    await testador.pumpAndSettle();

    expect(find.text('Instalar o Juntô'), findsOneWidget);
    expect(find.text('Toque em Compartilhar.'), findsOneWidget);
    expect(
      find.text('Escolha “Adicionar à Tela de Início”.'),
      findsOneWidget,
    );
    expect(instalacao.quantidadeDeSolicitacoes, 0);
  });

  testWidgets('deve solicitar e concluir a instalacao nativa', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    ServicoDeInstalacaoFalso instalacao = ServicoDeInstalacaoFalso(
      situacao: SituacaoDaInstalacao.podeSolicitar,
      aceite: true,
    );

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        servicoDeInstalacao: instalacao,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Perfil'));
    await testador.pumpAndSettle();
    await testador.scrollUntilVisible(
      find.byKey(const Key('instalar-aplicativo')),
      250,
      scrollable: find.byType(Scrollable).last,
    );
    await testador.tap(find.byKey(const Key('instalar-aplicativo')));
    await testador.pumpAndSettle();

    expect(instalacao.quantidadeDeSolicitacoes, 1);
    expect(find.byKey(const Key('instalar-aplicativo')), findsNothing);
  });

  testWidgets('deve cadastrar e retornar para entrada', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso();

    await testador.pumpWidget(_crieAplicativo(repositorio));
    await testador.pumpAndSettle();

    Finder acaoDeCadastro = find.text('Ainda não tem conta?  Criar conta');
    await testador.ensureVisible(acaoDeCadastro);
    await testador.tap(acaoDeCadastro);
    await testador.pumpAndSettle();
    await testador.enterText(
      find.widgetWithText(TextFormField, 'Seu nome'),
      'Pessoa Teste',
    );
    await testador.enterText(
      find.widgetWithText(TextFormField, 'Celular'),
      '(64) 99999-7777',
    );
    await testador.enterText(
      find.widgetWithText(TextFormField, 'PIN de 6 dígitos'),
      '654321',
    );
    Finder botaoDeCadastro = find.widgetWithText(FilledButton, 'Criar conta');
    await testador.ensureVisible(botaoDeCadastro);
    await testador.tap(botaoDeCadastro);
    await testador.pumpAndSettle();

    expect(repositorio.numeroDeCelularDoUltimoCadastro, '(64) 99999-7777');
    expect(repositorio.pinDoUltimoCadastro, '654321');
    expect(
      find.text('Conta criada. Agora entre com seus dados.'),
      findsOneWidget,
    );
  });
}

ProviderScope _crieAplicativo(
  IRepositorioDeAutenticacao repositorio, {
  IRepositorioDaPaginaInicial? repositorioDaPaginaInicial,
  IRepositorioDoPerfil? repositorioDoPerfil,
  ISeletorDeImagem? seletorDeImagem,
  IRepositorioDeImagensPrivadas? repositorioDeImagens,
  IServicoDeInstalacao? servicoDeInstalacao,
}) {
  return ProviderScope(
    overrides: <Override>[
      provedorDoRepositorioDeAutenticacao.overrideWithValue(repositorio),
      provedorDoRepositorioDaPaginaInicial.overrideWithValue(
        repositorioDaPaginaInicial ?? RepositorioDaPaginaInicialFalso(),
      ),
      provedorDoRepositorioDoPerfil.overrideWithValue(
        repositorioDoPerfil ?? RepositorioDoPerfilFalso(),
      ),
      provedorDoSeletorDeImagem.overrideWithValue(
        seletorDeImagem ?? SeletorDeImagemFalso(temImagem: false),
      ),
      provedorDoRepositorioDeImagensPrivadas.overrideWithValue(
        repositorioDeImagens ?? RepositorioDeImagensPrivadasFalso(),
      ),
      if (servicoDeInstalacao != null)
        provedorDoServicoDeInstalacao.overrideWithValue(servicoDeInstalacao),
    ],
    child: const Aplicativo(),
  );
}

class RepositorioDeImagensPrivadasFalso
    implements IRepositorioDeImagensPrivadas {
  int quantidadeDeBuscas = 0;

  @override
  Future<Uint8List?> obtenhaAsync(String recurso) async {
    quantidadeDeBuscas++;
    return null;
  }
}

class ServicoDeInstalacaoFalso implements IServicoDeInstalacao {
  ServicoDeInstalacaoFalso({
    required this.situacao,
    this.aceite = false,
  });

  final SituacaoDaInstalacao situacao;
  final bool aceite;
  int quantidadeDeSolicitacoes = 0;

  @override
  SituacaoDaInstalacao obtenhaSituacao() {
    return situacao;
  }

  @override
  Future<bool> soliciteInstalacaoAsync() async {
    quantidadeDeSolicitacoes++;
    return aceite;
  }
}

class SeletorDeImagemFalso
    implements ISeletorDeImagem, ISeletorDeImagemPorOrigem {
  SeletorDeImagemFalso({this.temImagem = true});

  final bool temImagem;

  @override
  Future<ImagemSelecionada?> selecioneAsync() async {
    return _selecione();
  }

  @override
  Future<ImagemSelecionada?> selecionePorOrigemAsync(
    EnumeradorDeOrigemDaImagem origem,
  ) async {
    return _selecione();
  }

  ImagemSelecionada? _selecione() {
    if (!temImagem) {
      return null;
    }

    return ImagemSelecionada(
      nome: 'perfil.png',
      tipoDeConteudo: 'image/png',
      conteudo: base64Decode(
        'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=',
      ),
    );
  }
}

class RepositorioDaPaginaInicialFalso implements IRepositorioDaPaginaInicial {
  RepositorioDaPaginaInicialFalso({this.urlDaFotoDePerfil});

  String? urlDaFotoDePerfil;

  @override
  Future<List<EncontroResumo>> listeProximosEncontrosAsync() async {
    return <EncontroResumo>[];
  }

  @override
  Future<UsuarioAtual> obtenhaUsuarioAtualAsync() async {
    return UsuarioAtual(
      identificador: 'usuario-1',
      nome: 'Pessoa Teste',
      numeroDeCelular: '+5562999998888',
      urlDaFotoDePerfil: urlDaFotoDePerfil,
    );
  }
}

class RepositorioDoPerfilFalso
    implements IRepositorioDoPerfil, IRepositorioDeEdicaoDoPerfil {
  RepositorioDoPerfilFalso({this.aoAtualizarUrl});

  final void Function(String? url)? aoAtualizarUrl;
  String? nomeDaUltimaFoto;
  String? ultimoNome;

  @override
  Future<UsuarioAtual> altereNomeAsync(String nome) async {
    ultimoNome = nome;
    return UsuarioAtual(
      identificador: 'usuario-1',
      nome: nome,
      numeroDeCelular: '+5562999998888',
    );
  }

  @override
  Future<UsuarioAtual> altereFotoAsync({
    required String nomeDoArquivo,
    required String tipoDeConteudo,
    required Uint8List conteudo,
  }) async {
    const String url = '/arquivos/usuarios/perfil.png';
    nomeDaUltimaFoto = nomeDoArquivo;
    aoAtualizarUrl?.call(url);

    return const UsuarioAtual(
      identificador: 'usuario-1',
      nome: 'Pessoa Teste',
      numeroDeCelular: '+5562999998888',
      urlDaFotoDePerfil: url,
    );
  }

  @override
  Future<UsuarioAtual> removaFotoAsync() async {
    aoAtualizarUrl?.call(null);
    return const UsuarioAtual(
      identificador: 'usuario-1',
      nome: 'Pessoa Teste',
      numeroDeCelular: '+5562999998888',
    );
  }
}

class RepositorioDeAutenticacaoFalso implements IRepositorioDeAutenticacao {
  RepositorioDeAutenticacaoFalso({
    this.sessaoPodeSerRestaurada = false,
  });

  final bool sessaoPodeSerRestaurada;
  String? numeroDeCelularDoUltimoLogin;
  String? pinDoUltimoLogin;
  String? numeroDeCelularDoUltimoCadastro;
  String? pinDoUltimoCadastro;

  @override
  Future<RespostaDeSessao> autentiqueAsync({
    required String numeroDeCelular,
    required String pin,
  }) async {
    numeroDeCelularDoUltimoLogin = numeroDeCelular;
    pinDoUltimoLogin = pin;
    return _crieRespostaDeSessao();
  }

  @override
  Future<void> cadastreAsync({
    required String nome,
    required String numeroDeCelular,
    required String pin,
  }) async {
    numeroDeCelularDoUltimoCadastro = numeroDeCelular;
    pinDoUltimoCadastro = pin;
  }

  @override
  Future<void> encerreSessaoAsync() async {}

  @override
  Future<RespostaDeSessao> renoveSessaoAsync() async {
    if (!sessaoPodeSerRestaurada) {
      throw const ExcecaoDaApi(
        codigoHttp: 401,
        mensagem: 'Sessao ausente.',
      );
    }

    return _crieRespostaDeSessao();
  }

  RespostaDeSessao _crieRespostaDeSessao() {
    return RespostaDeSessao(
      tokenDeAcesso: 'token-de-teste',
      expiraEm: DateTime.now().add(const Duration(minutes: 15)),
    );
  }
}

class AdaptadorHttpFalso implements HttpClientAdapter {
  RequestOptions? ultimaRequisicao;

  @override
  void close({bool force = false}) {}

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    ultimaRequisicao = options;

    if (options.path.endsWith('/navegador/login')) {
      return ResponseBody.fromString(
        jsonEncode(<String, dynamic>{
          'tokenDeAcesso': 'token-de-teste',
          'expiraEm': '2026-09-28T13:00:00Z',
        }),
        200,
        headers: <String, List<String>>{
          Headers.contentTypeHeader: <String>[Headers.jsonContentType],
        },
      );
    }

    return ResponseBody.fromString('', 201);
  }
}
