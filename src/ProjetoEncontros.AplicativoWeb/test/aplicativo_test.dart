import 'dart:convert';
import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/repositorio_de_autenticacao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/resposta_de_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/erros/excecao_da_api.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/repositorio_de_imagens_privadas.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/combinados/dados/repositorio_de_combinados.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/combinados/modelos/item_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/dados/repositorio_de_encontros.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_criado.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_detalhado.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/participante_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/servicos/seletor_de_imagem.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/dados/repositorio_da_pagina_inicial.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/encontro_resumo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/usuario_atual.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/dados/repositorio_de_memorias_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/dados/repositorio_da_linha_do_tempo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/item_da_linha_do_tempo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/memoria_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/memorias/modelos/midia_da_memoria.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/notificacoes/dados/repositorio_de_notificacoes.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/notificacoes/modelos/notificacao_do_usuario.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/notificacoes/modelos/preferencia_de_notificacao.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/perfil/dados/repositorio_do_perfil.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/pessoas_frequentes/dados/repositorio_de_pessoas_frequentes.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/pessoas_frequentes/modelos/pessoa_frequente.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/publicacoes/dados/repositorio_de_publicacoes_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/publicacoes/modelos/publicacao_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/inicializacao/aplicativo.dart';

void main() {
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

  testWidgets('deve autenticar e abrir a area privada', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso();

    await testador.pumpWidget(_crieAplicativo(repositorio));
    await testador.pumpAndSettle();

    await testador.enterText(
      find.widgetWithText(TextFormField, 'E-mail'),
      'pessoa@email.com',
    );
    await testador.enterText(
      find.widgetWithText(TextFormField, 'Senha'),
      'senha-segura',
    );
    Finder botaoDeEntrada = find.widgetWithText(FilledButton, 'Entrar');
    await testador.ensureVisible(botaoDeEntrada);
    await testador.pumpAndSettle();
    await testador.tap(botaoDeEntrada);
    await testador.pumpAndSettle();

    expect(repositorio.emailDoUltimoLogin, 'pessoa@email.com');
    expect(find.text('Próximo encontro'), findsOneWidget);
    expect(find.text('Olá, Pessoa'), findsOneWidget);
    expect(find.text('Café de domingo'), findsOneWidget);
  });

  testWidgets('deve restaurar sessao existente ao iniciar', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);

    await testador.pumpWidget(_crieAplicativo(repositorio));
    await testador.pumpAndSettle();

    expect(find.text('Próximo encontro'), findsOneWidget);
    expect(find.text('Entre para continuar'), findsNothing);
  });

  testWidgets('deve apresentar os dados e a foto na tela de perfil', (
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
    expect(find.text('pessoa@email.com'), findsOneWidget);
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

    expect(find.byKey(const Key('escolher-foto-do-perfil')), findsOneWidget);
    await testador.tap(find.byKey(const Key('escolher-foto-do-perfil')));
    await testador.pumpAndSettle();

    expect(perfil.nomeDaUltimaFoto, 'capa.png');
    expect(paginaInicial.urlDaFotoDePerfil, '/arquivos/usuarios/perfil.png');
    expect(find.text('Foto de perfil atualizada.'), findsOneWidget);
    expect(imagens.quantidadeDeBuscas, greaterThan(0));
  });

  testWidgets('deve visualizar e alterar uma foto de perfil existente', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDaPaginaInicialFalso paginaInicial =
        RepositorioDaPaginaInicialFalso(
      urlDaFotoDePerfil: '/arquivos/usuarios/perfil-atual.png',
    );
    RepositorioDoPerfilFalso perfil = RepositorioDoPerfilFalso(
      aoAtualizarUrl: (String? url) {
        paginaInicial.urlDaFotoDePerfil = url;
      },
    );

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDaPaginaInicial: paginaInicial,
        repositorioDoPerfil: perfil,
        seletorDeImagem: SeletorDeImagemFalso(),
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Perfil'));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('abrir-foto-do-perfil')));
    await testador.pumpAndSettle();

    expect(find.byTooltip('Fechar'), findsOneWidget);
    expect(find.byKey(const Key('editar-foto-do-perfil')), findsOneWidget);
    await testador.tap(find.byKey(const Key('editar-foto-do-perfil')));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('escolher-foto-do-perfil')));
    await testador.pumpAndSettle();

    expect(perfil.nomeDaUltimaFoto, 'capa.png');
    expect(find.text('Foto de perfil atualizada.'), findsOneWidget);
  });

  testWidgets('deve listar e filtrar a linha do tempo de memorias', (
    WidgetTester testador,
  ) async {
    await testador.binding.setSurfaceSize(const Size(320, 720));
    addTearDown(() => testador.binding.setSurfaceSize(null));
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDaLinhaDoTempoFalso repositorioDaLinhaDoTempo =
        RepositorioDaLinhaDoTempoFalso();

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDaLinhaDoTempo: repositorioDaLinhaDoTempo,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Memórias'));
    await testador.pumpAndSettle();

    expect(find.text('Noite para lembrar'), findsOneWidget);
    expect(find.byKey(const Key('memoria-encontro-memoria')), findsOneWidget);

    await testador.drag(
      find.byKey(const Key('filtros-da-linha-do-tempo')),
      const Offset(-650, 0),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('filtro-com-memorias')));
    await testador.pumpAndSettle();

    expect(
      repositorioDaLinhaDoTempo.ultimoFiltro,
      FiltroDaLinhaDoTempo.comMemorias,
    );
    await testador.scrollUntilVisible(
      find.byKey(const Key('abrir-galeria-encontro-memoria')),
      160,
      scrollable: find.descendant(
        of: find.byKey(const Key('lista-da-linha-do-tempo')),
        matching: find.byType(Scrollable),
      ),
    );
    await testador.drag(
      find.byKey(const Key('lista-da-linha-do-tempo')),
      const Offset(0, -120),
    );
    await testador.pumpAndSettle();
    await testador.tap(
      find.byKey(const Key('abrir-galeria-encontro-memoria')),
    );
    await testador.pumpAndSettle();
    expect(find.text('Mídias do encontro'), findsOneWidget);
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
    await testador.pumpAndSettle();
    await testador.tap(acaoDeCadastro);
    await testador.pumpAndSettle();

    await testador.enterText(
      find.widgetWithText(TextFormField, 'Seu nome'),
      'Pessoa Teste',
    );
    await testador.enterText(
      find.widgetWithText(TextFormField, 'E-mail'),
      'nova@email.com',
    );
    await testador.enterText(
      find.widgetWithText(TextFormField, 'Senha'),
      'senha-segura',
    );
    Finder botaoDeCadastro = find.widgetWithText(FilledButton, 'Criar conta');
    await testador.ensureVisible(botaoDeCadastro);
    await testador.pumpAndSettle();
    await testador.tap(botaoDeCadastro);
    await testador.pumpAndSettle();

    expect(repositorio.emailDoUltimoCadastro, 'nova@email.com');
    expect(
      find.text('Conta criada. Agora entre com seus dados.'),
      findsOneWidget,
    );
  });

  testWidgets('deve apresentar estado vazio sem encontros', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDaPaginaInicialFalso repositorioDaPaginaInicial =
        RepositorioDaPaginaInicialFalso(encontros: <EncontroResumo>[]);

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDaPaginaInicial: repositorioDaPaginaInicial,
      ),
    );
    await testador.pumpAndSettle();

    expect(find.text('Nenhum encontro marcado'), findsOneWidget);
    expect(find.text('Café de domingo'), findsNothing);
  });

  testWidgets('deve permitir tentar novamente quando a home falhar', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDaPaginaInicialFalso repositorioDaPaginaInicial =
        RepositorioDaPaginaInicialFalso(deveFalhar: true);

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDaPaginaInicial: repositorioDaPaginaInicial,
      ),
    );
    await testador.pumpAndSettle();

    expect(find.text('Falha simulada ao carregar a home.'), findsOneWidget);
    expect(find.text('Tentar novamente'), findsOneWidget);
  });

  testWidgets('deve criar encontro e retornar para a home', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    List<EncontroResumo> encontros = <EncontroResumo>[];
    RepositorioDaPaginaInicialFalso repositorioDaPaginaInicial =
        RepositorioDaPaginaInicialFalso(encontros: encontros);
    RepositorioDeEncontrosFalso repositorioDeEncontros =
        RepositorioDeEncontrosFalso(
      aoCriar: (String titulo, String? local) {
        encontros.add(
          EncontroResumo(
            identificador: 'encontro-criado',
            titulo: titulo,
            local: local,
            inicioEm: DateTime(2026, 7, 20, 19),
            situacao: 'Planejado',
            quantidadeDePresencasConfirmadas: 1,
            usuarioAtualConfirmouPresenca: true,
          ),
        );
      },
    );

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDaPaginaInicial: repositorioDaPaginaInicial,
        repositorioDeEncontros: repositorioDeEncontros,
      ),
    );
    await testador.pumpAndSettle();

    await testador.tap(find.byTooltip('Criar encontro'));
    await testador.pumpAndSettle();
    await testador.enterText(
      find.widgetWithText(TextFormField, 'Título do encontro'),
      'Noite de jogos',
    );
    await testador.enterText(
      find.widgetWithText(TextFormField, 'Nome ou endereço do local'),
      'Casa da Bia',
    );
    await testador.scrollUntilVisible(
      find.byKey(const Key('botao-criar-encontro')),
      300,
      scrollable: find.byType(Scrollable).last,
    );
    Finder botaoDeCriacao = find.byKey(const Key('botao-criar-encontro'));
    await testador.ensureVisible(botaoDeCriacao);
    await testador.pumpAndSettle();
    await testador.tap(botaoDeCriacao);
    await testador.pumpAndSettle();

    expect(repositorioDeEncontros.ultimoTitulo, 'Noite de jogos');
    expect(repositorioDeEncontros.ultimoLocal, 'Casa da Bia');
    expect(repositorioDeEncontros.ultimoTipo, isNull);
    expect(find.text('Olá, Pessoa'), findsOneWidget);
    expect(find.text('Noite de jogos'), findsOneWidget);
  });

  testWidgets('deve criar encontro com tipo opcional', (
    WidgetTester testador,
  ) async {
    await testador.binding.setSurfaceSize(const Size(320, 720));
    addTearDown(() => testador.binding.setSurfaceSize(null));
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeEncontrosFalso repositorioDeEncontros =
        RepositorioDeEncontrosFalso();

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDeEncontros: repositorioDeEncontros,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.byTooltip('Criar encontro'));
    await testador.pumpAndSettle();
    await testador.enterText(
      find.widgetWithText(TextFormField, 'Título do encontro'),
      'Noite de jogos',
    );
    await testador.tap(
      find.byKey(const Key('selecionar-tipo-do-encontro')),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('tipo-do-encontro-jogo')));
    await testador.pumpAndSettle();
    await testador.scrollUntilVisible(
      find.byKey(const Key('botao-criar-encontro')),
      300,
      scrollable: find.byType(Scrollable).first,
    );
    await testador.drag(
      find.byType(Scrollable).first,
      const Offset(0, -180),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('botao-criar-encontro')));
    await testador.pumpAndSettle();

    expect(repositorioDeEncontros.ultimoTipo, 'Jogo');
    expect(find.text('Olá, Pessoa'), findsOneWidget);
  });

  testWidgets('deve mostrar erro quando a criacao do encontro falhar', (
    WidgetTester testador,
  ) async {
    await testador.binding.setSurfaceSize(const Size(800, 800));
    addTearDown(() => testador.binding.setSurfaceSize(null));
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeEncontrosFalso repositorioDeEncontros =
        RepositorioDeEncontrosFalso(deveFalhar: true);

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDeEncontros: repositorioDeEncontros,
      ),
    );
    await testador.pumpAndSettle();

    await testador.tap(find.byTooltip('Criar encontro'));
    await testador.pumpAndSettle();
    await testador.enterText(
      find.widgetWithText(TextFormField, 'Título do encontro'),
      'Encontro indisponível',
    );
    await testador.scrollUntilVisible(
      find.byKey(const Key('botao-criar-encontro')),
      300,
      scrollable: find.byType(Scrollable).last,
    );
    Finder botaoDeCriacao = find.byKey(const Key('botao-criar-encontro'));
    await testador.ensureVisible(botaoDeCriacao);
    await testador.pumpAndSettle();
    await testador.tap(botaoDeCriacao);
    await testador.pumpAndSettle();

    expect(find.text('Não foi possível criar o encontro de teste.'),
        findsOneWidget);
    expect(find.byKey(const Key('botao-criar-encontro')), findsOneWidget);
  });

  testWidgets('deve abrir os momentos e as informacoes pela home', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);

    await testador.pumpWidget(_crieAplicativo(repositorio));
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();

    expect(find.text('Chego com o café.'), findsOneWidget);
    expect(find.text('Até domingo!'), findsOneWidget);
    expect(
      find.textContaining('Pessoa Teste criou o combinado'),
      findsOneWidget,
    );
    expect(
      testador.getTopLeft(find.text('Até domingo!')).dx,
      greaterThan(testador.getTopLeft(find.text('Chego com o café.')).dx),
    );
    expect(find.byKey(const Key('texto-da-nova-publicacao')), findsOneWidget);

    await testador.tap(
      find.byKey(const Key('abrir-informacoes-do-encontro')),
    );
    await testador.pumpAndSettle();

    expect(find.text('Informações do encontro'), findsOneWidget);
    await testador.scrollUntilVisible(
      find.text('Você vai participar'),
      300,
      scrollable: find.descendant(
        of: find.byKey(const Key('lista-dos-detalhes-do-encontro')),
        matching: find.byType(Scrollable),
      ),
    );
    expect(find.text('Você vai participar'), findsOneWidget);
    await testador.drag(
      find.byKey(const Key('lista-dos-detalhes-do-encontro')),
      const Offset(0, -360),
    );
    await testador.pumpAndSettle();
    expect(find.byKey(const Key('abrir-participantes')), findsOneWidget);
    await testador.tap(find.byKey(const Key('abrir-participantes')));
    await testador.pumpAndSettle();

    expect(find.text('Participantes'), findsWidgets);
    expect(find.text('Pessoa Teste (você)'), findsOneWidget);
    expect(find.text('Bia Souza'), findsOneWidget);
  });

  testWidgets('deve filtrar participantes por situacao', (
    WidgetTester testador,
  ) async {
    await testador.binding.setSurfaceSize(const Size(320, 720));
    addTearDown(() => testador.binding.setSurfaceSize(null));

    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);

    await testador.pumpWidget(_crieAplicativo(repositorio));
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();
    await _abraInformacoesDoEncontro(testador);
    await testador.scrollUntilVisible(
      find.byKey(const Key('abrir-participantes')),
      300,
      scrollable: find.byType(Scrollable).last,
    );
    await testador.tap(find.byKey(const Key('abrir-participantes')));
    await testador.pumpAndSettle();

    await testador.drag(
      find.byKey(const Key('lista-de-filtros-de-participantes')),
      const Offset(-120, 0),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('filtro-talvez')));
    await testador.pumpAndSettle();
    expect(find.text('Bia Souza'), findsOneWidget);

    await testador.drag(
      find.byKey(const Key('lista-de-filtros-de-participantes')),
      const Offset(-500, 0),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('filtro-naoVao')));
    await testador.pumpAndSettle();
    expect(find.text('Bia Souza'), findsNothing);
    expect(find.text('Ninguém está nesta situação.'), findsOneWidget);

    await testador.tap(find.text('Ver todos'));
    await testador.pumpAndSettle();
    expect(find.text('Bia Souza'), findsOneWidget);
  });

  testWidgets('deve apresentar estado vazio dos momentos', (
    WidgetTester testador,
  ) async {
    await testador.binding.setSurfaceSize(const Size(320, 720));
    addTearDown(() => testador.binding.setSurfaceSize(null));

    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDePublicacoesDoEncontroFalso repositorioDePublicacoes =
        RepositorioDePublicacoesDoEncontroFalso(
      publicacoesPersonalizadas: <PublicacaoDoEncontro>[],
    );

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDePublicacoes: repositorioDePublicacoes,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();

    expect(find.text('Ainda não há momentos por aqui.'), findsOneWidget);
    expect(find.byKey(const Key('texto-da-nova-publicacao')), findsOneWidget);
  });

  testWidgets('deve permitir tentar novamente quando os momentos falharem', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDePublicacoesDoEncontroFalso repositorioDePublicacoes =
        RepositorioDePublicacoesDoEncontroFalso(deveFalharAoListar: true);

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDePublicacoes: repositorioDePublicacoes,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();

    expect(find.text('Falha simulada ao listar momentos.'), findsOneWidget);
    expect(find.text('Tentar novamente'), findsOneWidget);
    expect(find.text('Chego com o café.'), findsNothing);
  });

  testWidgets('deve publicar no fim da ordem cronologica', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDePublicacoesDoEncontroFalso repositorioDePublicacoes =
        RepositorioDePublicacoesDoEncontroFalso();

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDePublicacoes: repositorioDePublicacoes,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();

    expect(
      testador.getTopLeft(find.text('Chego com o café.')).dy,
      lessThan(testador.getTopLeft(find.text('Até domingo!')).dy),
    );

    await testador.enterText(
      find.byKey(const Key('texto-da-nova-publicacao')),
      'Levo o bolo.',
    );
    await testador.tap(find.byKey(const Key('publicar-momento')));
    await testador.pumpAndSettle();

    expect(repositorioDePublicacoes.ultimoTextoPublicado, 'Levo o bolo.');
    expect(find.text('Levo o bolo.'), findsOneWidget);
    TextField campo = testador.widget<TextField>(
      find.byKey(const Key('texto-da-nova-publicacao')),
    );
    expect(campo.controller?.text, isEmpty);
  });

  testWidgets('deve preservar rascunho e operacao quando a publicacao falhar', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDePublicacoesDoEncontroFalso repositorioDePublicacoes =
        RepositorioDePublicacoesDoEncontroFalso(deveFalharAoPublicar: true);

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDePublicacoes: repositorioDePublicacoes,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();
    await testador.enterText(
      find.byKey(const Key('texto-da-nova-publicacao')),
      'Não quero perder este texto.',
    );
    await testador.tap(find.byKey(const Key('publicar-momento')));
    await testador.pumpAndSettle();

    expect(find.text('Falha simulada ao publicar.'), findsOneWidget);
    TextField campo = testador.widget<TextField>(
      find.byKey(const Key('texto-da-nova-publicacao')),
    );
    expect(campo.controller?.text, 'Não quero perder este texto.');

    repositorioDePublicacoes.deveFalharAoPublicar = false;
    await testador.tap(find.byKey(const Key('publicar-momento')));
    await testador.pumpAndSettle();

    expect(repositorioDePublicacoes.identificadoresDasOperacoes, hasLength(2));
    expect(
      repositorioDePublicacoes.identificadoresDasOperacoes.toSet(),
      hasLength(1),
    );
    expect(find.text('Não quero perder este texto.'), findsOneWidget);
  });

  testWidgets('deve selecionar e publicar foto com legenda no mural', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeMemoriasDoEncontroFalso repositorioDeMemorias =
        RepositorioDeMemoriasDoEncontroFalso();
    SeletorDeImagemFalso seletorDeImagem = SeletorDeImagemFalso();

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDeMemorias: repositorioDeMemorias,
        seletorDeImagem: seletorDeImagem,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('selecionar-foto')));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('tirar-foto-pela-camera')));
    await testador.pumpAndSettle();

    expect(find.byKey(const Key('previa-da-foto')), findsOneWidget);
    expect(seletorDeImagem.ultimaOrigem, EnumeradorDeOrigemDaImagem.camera);

    await testador.enterText(
      find.byKey(const Key('texto-da-nova-publicacao')),
      'Mesa pronta para a resenha',
    );
    await testador.tap(find.byKey(const Key('publicar-momento')));
    await testador.pumpAndSettle();

    expect(repositorioDeMemorias.ultimaLegenda, 'Mesa pronta para a resenha');
    expect(repositorioDeMemorias.ultimoNomeDoArquivo, 'capa.png');
    expect(find.byKey(const Key('previa-da-foto')), findsNothing);
    expect(find.byKey(const Key('abrir-midia-memoria-nova')), findsOneWidget);

    await testador.tap(find.byKey(const Key('abrir-midia-memoria-nova')));
    await testador.pumpAndSettle();
    expect(find.text('Sua foto'), findsOneWidget);

    await testador.tap(find.byKey(const Key('remover-memoria')));
    await testador.pumpAndSettle();
    await testador.tap(
      find.byKey(const Key('confirmar-remocao-da-memoria')),
    );
    await testador.pumpAndSettle();

    expect(repositorioDeMemorias.ultimaMemoriaRemovida, 'memoria-nova');
    expect(find.byKey(const Key('publicacao-memoria-nova')), findsNothing);
  });

  testWidgets('deve abrir a galeria privada pelos detalhes do encontro', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeMemoriasDoEncontroFalso repositorioDeMemorias =
        RepositorioDeMemoriasDoEncontroFalso(comMemoriaInicial: true);

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDeMemorias: repositorioDeMemorias,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();
    await _abraInformacoesDoEncontro(testador);
    await testador.scrollUntilVisible(
      find.byKey(const Key('abrir-midias')),
      300,
      scrollable: find.descendant(
        of: find.byKey(const Key('lista-dos-detalhes-do-encontro')),
        matching: find.byType(Scrollable),
      ),
    );
    await testador.drag(
      find.byKey(const Key('lista-dos-detalhes-do-encontro')),
      const Offset(0, -120),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('abrir-midias')));
    await testador.pumpAndSettle();

    expect(find.text('Mídias do encontro'), findsOneWidget);
    expect(find.byKey(const Key('midia-midia-inicial')), findsOneWidget);

    await testador.tap(find.byKey(const Key('midia-midia-inicial')));
    await testador.pumpAndSettle();
    expect(find.text('Pessoa Teste'), findsWidgets);
  });

  testWidgets('deve alterar a resposta de presenca', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeEncontrosFalso repositorioDeEncontros =
        RepositorioDeEncontrosFalso();

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDeEncontros: repositorioDeEncontros,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();
    await _abraInformacoesDoEncontro(testador);
    await testador.scrollUntilVisible(
      find.byKey(const Key('alterar-presenca')),
      300,
      scrollable: find.byType(Scrollable).last,
    );
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('alterar-presenca')));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('presenca-talvez')));
    await testador.pumpAndSettle();

    expect(repositorioDeEncontros.ultimaSituacaoDePresenca, 'Talvez');
    expect(find.text('Você talvez participe'), findsOneWidget);
  });

  testWidgets('deve editar um encontro como organizador', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeEncontrosFalso repositorioDeEncontros =
        RepositorioDeEncontrosFalso(tipoInicial: 'Amigos');

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDeEncontros: repositorioDeEncontros,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();
    await _abraInformacoesDoEncontro(testador);
    await testador.scrollUntilVisible(
      find.byKey(const Key('editar-encontro')),
      400,
      scrollable: find.byType(Scrollable).last,
    );
    await testador.drag(
      find.byType(Scrollable).last,
      const Offset(0, -100),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('editar-encontro')));
    await testador.pumpAndSettle();
    await testador.enterText(
      find.widgetWithText(TextFormField, 'Título do encontro'),
      'Café atualizado',
    );
    expect(find.text('Amigos'), findsOneWidget);
    await testador.tap(
      find.byKey(const Key('selecionar-tipo-do-encontro')),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('tipo-do-encontro-família')));
    await testador.pumpAndSettle();
    await testador.scrollUntilVisible(
      find.byKey(const Key('botao-salvar-encontro')),
      300,
      scrollable: find.byType(Scrollable).first,
    );
    await testador.drag(
      find.byType(Scrollable).first,
      const Offset(0, -120),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('botao-salvar-encontro')));
    await testador.pumpAndSettle();

    expect(repositorioDeEncontros.ultimoTituloEditado, 'Café atualizado');
    expect(repositorioDeEncontros.ultimoTipoEditado, 'Família');
    await testador.scrollUntilVisible(
      find.text('Café atualizado'),
      -400,
      scrollable: find.byType(Scrollable).last,
    );
    expect(find.text('Café atualizado'), findsOneWidget);
    expect(
        find.byKey(const Key('tipo-do-encontro-no-detalhe')), findsOneWidget);
    expect(find.text('Família'), findsOneWidget);
  });

  testWidgets('deve remover o tipo ao editar um encontro', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeEncontrosFalso repositorioDeEncontros =
        RepositorioDeEncontrosFalso(tipoInicial: 'Amigos');

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDeEncontros: repositorioDeEncontros,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();
    await _abraInformacoesDoEncontro(testador);
    await testador.scrollUntilVisible(
      find.byKey(const Key('editar-encontro')),
      400,
      scrollable: find.byType(Scrollable).last,
    );
    await testador.ensureVisible(find.byKey(const Key('editar-encontro')));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('editar-encontro')));
    await testador.pumpAndSettle();
    expect(find.text('Amigos'), findsOneWidget);
    await testador.tap(
      find.byKey(const Key('remover-tipo-do-encontro')),
    );
    await testador.pumpAndSettle();
    expect(find.text('Escolher tipo (opcional)'), findsOneWidget);
    await testador.scrollUntilVisible(
      find.byKey(const Key('botao-salvar-encontro')),
      300,
      scrollable: find.byType(Scrollable).first,
    );
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('botao-salvar-encontro')));
    await testador.pumpAndSettle();

    expect(repositorioDeEncontros.tipoFoiEditado, isTrue);
    expect(repositorioDeEncontros.ultimoTipoEditado, isNull);
    expect(
      find.byKey(const Key('tipo-do-encontro-no-detalhe')),
      findsNothing,
    );
  });

  testWidgets('deve cancelar um encontro mediante confirmacao', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeEncontrosFalso repositorioDeEncontros =
        RepositorioDeEncontrosFalso();

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDeEncontros: repositorioDeEncontros,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();
    await _abraInformacoesDoEncontro(testador);
    await testador.scrollUntilVisible(
      find.byKey(const Key('cancelar-encontro')),
      400,
      scrollable: find.byType(Scrollable).last,
    );
    await testador.drag(
      find.byType(Scrollable).last,
      const Offset(0, -100),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('cancelar-encontro')));
    await testador.pumpAndSettle();

    expect(find.text('Cancelar encontro?'), findsOneWidget);
    await testador.tap(find.byKey(const Key('confirmar-cancelamento')));
    await testador.pumpAndSettle();

    expect(repositorioDeEncontros.encontroFoiCancelado, isTrue);
    await testador.scrollUntilVisible(
      find.text('Cancelado'),
      -400,
      scrollable: find.byType(Scrollable).last,
    );
    expect(find.text('Cancelado'), findsOneWidget);
    expect(find.byKey(const Key('editar-encontro')), findsNothing);
  });

  testWidgets('nao deve mostrar gestao para participante comum', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeEncontrosFalso repositorioDeEncontros =
        RepositorioDeEncontrosFalso(podeGerenciar: false);

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDeEncontros: repositorioDeEncontros,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();
    await _abraInformacoesDoEncontro(testador);
    await testador.fling(
      find.byType(Scrollable).last,
      const Offset(0, -1200),
      1800,
    );
    await testador.pumpAndSettle();

    expect(find.byKey(const Key('editar-encontro')), findsNothing);
    expect(find.byKey(const Key('cancelar-encontro')), findsNothing);
    expect(find.byKey(const Key('alterar-capa')), findsNothing);
    expect(find.byKey(const Key('remover-capa')), findsNothing);
    expect(find.byKey(const Key('convidar-pessoas')), findsNothing);

    await testador.scrollUntilVisible(
      find.byKey(const Key('abrir-participantes')),
      300,
      scrollable: find.byType(Scrollable).last,
    );
    await testador.tap(find.byKey(const Key('abrir-participantes')));
    await testador.pumpAndSettle();
    expect(find.byKey(const Key('convidar-pessoas')), findsNothing);
  });

  testWidgets('deve alterar a imagem de capa do encontro', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeEncontrosFalso repositorioDeEncontros =
        RepositorioDeEncontrosFalso(
      urlInicialDaImagemDeCapa: '/arquivos/encontros/capa.png',
    );
    RepositorioDeImagensPrivadasFalso repositorioDeImagens =
        RepositorioDeImagensPrivadasFalso();

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDeEncontros: repositorioDeEncontros,
        seletorDeImagem: SeletorDeImagemFalso(),
        repositorioDeImagens: repositorioDeImagens,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();
    await _abraInformacoesDoEncontro(testador);
    await testador.tap(find.byKey(const Key('gerenciar-capa')));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('alterar-capa')));
    await testador.pumpAndSettle();
    await testador.tap(
      find.byKey(const Key('escolher-foto-da-galeria')),
    );
    await testador.pumpAndSettle();

    expect(repositorioDeEncontros.ultimoNomeDaImagem, 'capa.png');
    expect(repositorioDeImagens.quantidadeDeBuscas, greaterThanOrEqualTo(2));
    expect(find.text('Imagem do encontro atualizada.'), findsOneWidget);
  });

  testWidgets('deve convidar uma pessoa por email', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeEncontrosFalso repositorioDeEncontros =
        RepositorioDeEncontrosFalso();

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDeEncontros: repositorioDeEncontros,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();
    await _abraInformacoesDoEncontro(testador);
    await testador.scrollUntilVisible(
      find.byKey(const Key('abrir-participantes')),
      400,
      scrollable: find.byType(Scrollable).last,
    );
    await testador.drag(
      find.byType(Scrollable).last,
      const Offset(0, -120),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('abrir-participantes')));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('convidar-pessoas')));
    await testador.pumpAndSettle();
    await testador.enterText(
      find.byKey(const Key('email-do-convidado')),
      'convidada@email.com',
    );
    await testador.tap(find.byKey(const Key('confirmar-convite')));
    await testador.pumpAndSettle();

    expect(repositorioDeEncontros.ultimoEmailConvidado, 'convidada@email.com');
    expect(find.text('Convite enviado.'), findsOneWidget);
    expect(find.text('Pessoa convidada'), findsOneWidget);
  });

  testWidgets('deve convidar uma pessoa frequente mediante confirmacao', (
    WidgetTester testador,
  ) async {
    await testador.binding.setSurfaceSize(const Size(390, 844));
    addTearDown(() => testador.binding.setSurfaceSize(null));
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeEncontrosFalso repositorioDeEncontros =
        RepositorioDeEncontrosFalso();
    RepositorioDePessoasFrequentesFalso pessoasFrequentes =
        RepositorioDePessoasFrequentesFalso(
      pessoas: <PessoaFrequente>[
        PessoaFrequente(
          identificadorDoUsuario: 'usuario-caio',
          nome: 'Caio Lima',
          quantidadeDeEncontrosEmComum: 4,
          ultimoEncontroEm: DateTime(2026, 7, 10),
        ),
      ],
    );

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDeEncontros: repositorioDeEncontros,
        repositorioDePessoasFrequentes: pessoasFrequentes,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();
    await _abraInformacoesDoEncontro(testador);
    await testador.scrollUntilVisible(
      find.byKey(const Key('abrir-participantes')),
      400,
      scrollable: find.byType(Scrollable).last,
    );
    await testador.tap(find.byKey(const Key('abrir-participantes')));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('convidar-pessoas')));
    await testador.pumpAndSettle();

    expect(find.text('Pessoas frequentes'), findsOneWidget);
    expect(find.text('Caio Lima'), findsOneWidget);
    expect(find.text('4 encontros juntos'), findsOneWidget);
    expect(
      repositorioDeEncontros.ultimoIdentificadorDoUsuarioConvidado,
      isNull,
    );

    await testador.tap(
      find.byKey(const Key('convidar-pessoa-frequente-usuario-caio')),
    );
    await testador.pumpAndSettle();
    expect(find.text('Enviar convite?'), findsOneWidget);
    expect(
      repositorioDeEncontros.ultimoIdentificadorDoUsuarioConvidado,
      isNull,
    );

    await testador.tap(
      find.byKey(const Key('confirmar-convite-de-pessoa-frequente')),
    );
    await testador.pumpAndSettle();

    expect(
      repositorioDeEncontros.ultimoIdentificadorDoUsuarioConvidado,
      'usuario-caio',
    );
    expect(find.byKey(const Key('email-do-convidado')), findsNothing);
  });

  testWidgets('deve preservar o email quando o convite falhar', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeEncontrosFalso repositorioDeEncontros =
        RepositorioDeEncontrosFalso(deveFalharAoConvidar: true);

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDeEncontros: repositorioDeEncontros,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();
    await _abraInformacoesDoEncontro(testador);
    await testador.scrollUntilVisible(
      find.byKey(const Key('abrir-participantes')),
      400,
      scrollable: find.byType(Scrollable).last,
    );
    await testador.drag(
      find.byType(Scrollable).last,
      const Offset(0, -120),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('abrir-participantes')));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('convidar-pessoas')));
    await testador.pumpAndSettle();
    await testador.enterText(
      find.byKey(const Key('email-do-convidado')),
      'convidada@email.com',
    );
    await testador.tap(find.byKey(const Key('confirmar-convite')));
    await testador.pumpAndSettle();

    expect(find.text('Falha simulada ao convidar.'), findsWidgets);
    TextFormField campo = testador.widget<TextFormField>(
      find.byKey(const Key('email-do-convidado')),
    );
    expect(campo.controller?.text, 'convidada@email.com');
    expect(find.byKey(const Key('confirmar-convite')), findsOneWidget);
  });

  testWidgets('deve remover a imagem de capa mediante confirmacao', (
    WidgetTester testador,
  ) async {
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeEncontrosFalso repositorioDeEncontros =
        RepositorioDeEncontrosFalso(
      urlInicialDaImagemDeCapa: '/arquivos/encontros/capa.png',
    );

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDeEncontros: repositorioDeEncontros,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();
    await _abraInformacoesDoEncontro(testador);
    await testador.tap(find.byKey(const Key('visualizar-capa')));
    await testador.pumpAndSettle();
    expect(find.byTooltip('Fechar'), findsOneWidget);
    await testador.tap(find.byTooltip('Fechar'));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('gerenciar-capa')));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('remover-capa')));
    await testador.pumpAndSettle();
    await testador.tap(
      find.byKey(const Key('confirmar-remocao-da-capa')),
    );
    await testador.pumpAndSettle();

    expect(repositorioDeEncontros.imagemDeCapaFoiRemovida, isTrue);
    expect(find.text('Imagem do encontro removida.'), findsOneWidget);
    expect(find.byKey(const Key('remover-capa')), findsNothing);
  });

  testWidgets('deve abrir e marcar notificacao como lida', (
    WidgetTester testador,
  ) async {
    await testador.binding.setSurfaceSize(const Size(320, 720));
    addTearDown(() => testador.binding.setSurfaceSize(null));
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeNotificacoesFalso notificacoes =
        RepositorioDeNotificacoesFalso();

    await testador.pumpWidget(
      _crieAplicativo(repositorio, repositorioDeNotificacoes: notificacoes),
    );
    await testador.pumpAndSettle();

    expect(find.text('1'), findsOneWidget);
    await testador.tap(find.byKey(const Key('abrir-notificacoes')));
    await testador.pumpAndSettle();
    expect(find.text('Encontro alterado'), findsOneWidget);

    await testador.tap(find.byKey(const Key('notificacao-notificacao')));
    await testador.pumpAndSettle();

    expect(notificacoes.identificadorMarcadoComoLido, 'notificacao');
    expect(find.text('Café de domingo'), findsWidgets);
  });

  testWidgets('deve solicitar presenca ao abrir notificacao de convite', (
    WidgetTester testador,
  ) async {
    await testador.binding.setSurfaceSize(const Size(320, 720));
    addTearDown(() => testador.binding.setSurfaceSize(null));
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeNotificacoesFalso notificacoes =
        RepositorioDeNotificacoesFalso(
      tipoDaNotificacao: 'ConviteRecebido',
      tituloDaNotificacao: 'Você recebeu um convite',
    );
    RepositorioDeEncontrosFalso encontros = RepositorioDeEncontrosFalso(
      situacaoInicialDoUsuario: 'Convidado',
    );
    RepositorioDaPaginaInicialFalso paginaInicial =
        RepositorioDaPaginaInicialFalso();

    await testador.pumpWidget(
      _crieAplicativo(
        repositorio,
        repositorioDaPaginaInicial: paginaInicial,
        repositorioDeEncontros: encontros,
        repositorioDeNotificacoes: notificacoes,
      ),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('abrir-notificacoes')));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('notificacao-notificacao')));
    await testador.pumpAndSettle();

    expect(find.text('Você vai participar?'), findsOneWidget);
    expect(find.byKey(const Key('presenca-confirmado')), findsOneWidget);
    expect(find.byKey(const Key('presenca-talvez')), findsOneWidget);
    expect(find.byKey(const Key('presenca-nao-vai')), findsOneWidget);

    await testador.tap(find.byKey(const Key('presenca-talvez')));
    await testador.pumpAndSettle();

    expect(encontros.ultimaSituacaoDePresenca, 'Talvez');
    expect(paginaInicial.quantidadeDeListagens, greaterThanOrEqualTo(2));
    expect(find.text('Você vai participar?'), findsNothing);
    expect(find.text('Café de domingo'), findsWidgets);
  });

  testWidgets('deve salvar preferencias de notificacao', (
    WidgetTester testador,
  ) async {
    await testador.binding.setSurfaceSize(const Size(320, 720));
    addTearDown(() => testador.binding.setSurfaceSize(null));
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeNotificacoesFalso notificacoes =
        RepositorioDeNotificacoesFalso();

    await testador.pumpWidget(
      _crieAplicativo(repositorio, repositorioDeNotificacoes: notificacoes),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Perfil'));
    await testador.pumpAndSettle();
    await testador.scrollUntilVisible(
      find.byKey(const Key('abrir-preferencias-de-notificacao')),
      250,
      scrollable: find.byType(Scrollable).last,
    );
    await testador.drag(
      find.byType(Scrollable).last,
      const Offset(0, -120),
    );
    await testador.pumpAndSettle();
    await testador.tap(
      find.byKey(const Key('abrir-preferencias-de-notificacao')),
    );
    await testador.pumpAndSettle();

    await testador.tap(find.byType(Switch).first);
    await testador.scrollUntilVisible(
      find.byKey(const Key('salvar-preferencias-de-notificacao')),
      250,
      scrollable: find.byType(Scrollable).last,
    );
    await testador.tap(
      find.byKey(const Key('salvar-preferencias-de-notificacao')),
    );
    await testador.pumpAndSettle();

    expect(
      notificacoes.ultimasPreferencias?.notificacoesDeConviteAtivas,
      isFalse,
    );
    expect(find.text('Preferências salvas.'), findsOneWidget);
  });

  testWidgets('deve criar e resolver um combinado', (
    WidgetTester testador,
  ) async {
    await testador.binding.setSurfaceSize(const Size(320, 720));
    addTearDown(() => testador.binding.setSurfaceSize(null));
    RepositorioDeAutenticacaoFalso repositorio =
        RepositorioDeAutenticacaoFalso(sessaoPodeSerRestaurada: true);
    RepositorioDeCombinadosFalso combinados = RepositorioDeCombinadosFalso();
    combinados.deveFalharNaProximaCriacao = true;

    await testador.pumpWidget(
      _crieAplicativo(repositorio, repositorioDeCombinados: combinados),
    );
    await testador.pumpAndSettle();
    await testador.tap(find.text('Café de domingo'));
    await testador.pumpAndSettle();
    await _abraInformacoesDoEncontro(testador);
    await testador.scrollUntilVisible(
      find.byKey(const Key('abrir-combinados')),
      300,
      scrollable: find.descendant(
        of: find.byKey(const Key('lista-dos-detalhes-do-encontro')),
        matching: find.byType(Scrollable),
      ),
    );
    await testador.ensureVisible(find.byKey(const Key('abrir-combinados')));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('abrir-combinados')));
    await testador.pumpAndSettle();
    await testador.tap(find.byKey(const Key('adicionar-combinado')));
    await testador.pumpAndSettle();
    await testador.enterText(
      find.byKey(const Key('descricao-do-combinado')),
      'Levar gelo',
    );
    await testador.tap(find.byKey(const Key('salvar-combinado')));
    await testador.pumpAndSettle();

    expect(find.text('Não foi possível salvar o combinado.'), findsOneWidget);
    await testador.tap(find.byKey(const Key('salvar-combinado')));
    await testador.pumpAndSettle();

    expect(find.text('Levar gelo'), findsOneWidget);
    expect(combinados.identificadoresDasOperacoes, hasLength(2));
    expect(combinados.identificadoresDasOperacoes.toSet(), hasLength(1));
    await testador.tap(find.byTooltip('Marcar como resolvido'));
    await testador.pumpAndSettle();
    expect(combinados.itemFoiResolvido, isTrue);
  });
}

Future<void> _abraInformacoesDoEncontro(WidgetTester testador) async {
  await testador.tap(
    find.byKey(const Key('abrir-informacoes-do-encontro')),
  );
  await testador.pumpAndSettle();
}

ProviderScope _crieAplicativo(
  IRepositorioDeAutenticacao repositorio, {
  IRepositorioDaPaginaInicial? repositorioDaPaginaInicial,
  IRepositorioDeEncontros? repositorioDeEncontros,
  IRepositorioDePublicacoesDoEncontro? repositorioDePublicacoes,
  IRepositorioDeMemoriasDoEncontro? repositorioDeMemorias,
  IRepositorioDaLinhaDoTempo? repositorioDaLinhaDoTempo,
  IRepositorioDeNotificacoes? repositorioDeNotificacoes,
  IRepositorioDeCombinados? repositorioDeCombinados,
  IRepositorioDePessoasFrequentes? repositorioDePessoasFrequentes,
  IRepositorioDoPerfil? repositorioDoPerfil,
  ISeletorDeImagem? seletorDeImagem,
  IRepositorioDeImagensPrivadas? repositorioDeImagens,
}) {
  return ProviderScope(
    overrides: [
      provedorDoRepositorioDeAutenticacao.overrideWithValue(repositorio),
      provedorDoRepositorioDaPaginaInicial.overrideWithValue(
        repositorioDaPaginaInicial ?? RepositorioDaPaginaInicialFalso(),
      ),
      provedorDoRepositorioDeEncontros.overrideWithValue(
        repositorioDeEncontros ?? RepositorioDeEncontrosFalso(),
      ),
      provedorDoRepositorioDePublicacoesDoEncontro.overrideWithValue(
        repositorioDePublicacoes ?? RepositorioDePublicacoesDoEncontroFalso(),
      ),
      provedorDoRepositorioDeMemoriasDoEncontro.overrideWithValue(
        repositorioDeMemorias ?? RepositorioDeMemoriasDoEncontroFalso(),
      ),
      if (repositorioDaLinhaDoTempo != null)
        provedorDoRepositorioDaLinhaDoTempo.overrideWithValue(
          repositorioDaLinhaDoTempo,
        ),
      provedorDoRepositorioDeNotificacoes.overrideWithValue(
        repositorioDeNotificacoes ?? RepositorioDeNotificacoesFalso(),
      ),
      provedorDoRepositorioDeCombinados.overrideWithValue(
        repositorioDeCombinados ?? RepositorioDeCombinadosFalso(),
      ),
      provedorDoRepositorioDePessoasFrequentes.overrideWithValue(
        repositorioDePessoasFrequentes ?? RepositorioDePessoasFrequentesFalso(),
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

class RepositorioDeNotificacoesFalso implements IRepositorioDeNotificacoes {
  RepositorioDeNotificacoesFalso({
    this.tipoDaNotificacao = 'EncontroAlterado',
    this.tituloDaNotificacao = 'Encontro alterado',
  });

  final String tipoDaNotificacao;
  final String tituloDaNotificacao;
  String? identificadorMarcadoComoLido;
  PreferenciaDeNotificacao? ultimasPreferencias;

  @override
  Future<ListaDeNotificacoes> listeAsync() async {
    return ListaDeNotificacoes(
      quantidadeNaoLida: identificadorMarcadoComoLido == null ? 1 : 0,
      notificacoes: <NotificacaoDoUsuario>[
        NotificacaoDoUsuario(
          identificador: 'notificacao',
          tipo: tipoDaNotificacao,
          titulo: tituloDaNotificacao,
          mensagem: 'O horário do encontro mudou.',
          identificadorDoEncontro: 'encontro',
          situacao: identificadorMarcadoComoLido == null ? 'NaoLida' : 'Lida',
          criadaEm: DateTime(2026, 8, 20, 18),
          lidaEm: identificadorMarcadoComoLido == null
              ? null
              : DateTime(2026, 8, 20, 18, 1),
        ),
      ],
    );
  }

  @override
  Future<void> marqueComoLidaAsync(String identificadorDaNotificacao) async {
    identificadorMarcadoComoLido = identificadorDaNotificacao;
  }

  @override
  Future<PreferenciaDeNotificacao> obtenhaPreferenciasAsync() async {
    return ultimasPreferencias ??
        const PreferenciaDeNotificacao(
          notificacoesDeConviteAtivas: true,
          lembretesDeEncontroAtivos: true,
          notificacoesDeAlteracaoAtivas: true,
          notificacoesDeCombinadosAtivas: true,
        );
  }

  @override
  Future<PreferenciaDeNotificacao> atualizePreferenciasAsync(
    PreferenciaDeNotificacao preferencias,
  ) async {
    ultimasPreferencias = preferencias;
    return preferencias;
  }
}

class RepositorioDeCombinadosFalso implements IRepositorioDeCombinados {
  final List<ItemDoEncontro> _itens = <ItemDoEncontro>[];
  bool itemFoiResolvido = false;
  bool deveFalharNaProximaCriacao = false;
  final List<String> identificadoresDasOperacoes = <String>[];

  @override
  Future<List<ItemDoEncontro>> listeAsync(
      String identificadorDoEncontro) async {
    return List<ItemDoEncontro>.from(_itens);
  }

  @override
  Future<void> crieAsync({
    required String identificadorDoEncontro,
    required String descricao,
    required String identificadorDaOperacao,
    String? identificadorDoResponsavel,
  }) async {
    identificadoresDasOperacoes.add(identificadorDaOperacao);

    if (deveFalharNaProximaCriacao) {
      deveFalharNaProximaCriacao = false;
      throw Exception('Falha simulada ao criar combinado.');
    }

    _itens.add(
      ItemDoEncontro(
        identificador: 'item',
        identificadorDoEncontro: identificadorDoEncontro,
        descricao: descricao,
        situacao: 'Pendente',
        identificadorDoUsuarioQueCriou: 'usuario',
        identificadorDoUsuarioResponsavel: identificadorDoResponsavel,
        usuarioAtualEhResponsavel: false,
        criadoEm: DateTime(2026, 8, 20),
        atualizadoEm: DateTime(2026, 8, 20),
      ),
    );
  }

  @override
  Future<void> editeAsync({
    required String identificadorDoEncontro,
    required String identificadorDoItem,
    required String descricao,
    String? identificadorDoResponsavel,
  }) async {}

  @override
  Future<void> removaAsync({
    required String identificadorDoEncontro,
    required String identificadorDoItem,
  }) async {
    _itens.removeWhere(
      (ItemDoEncontro item) => item.identificador == identificadorDoItem,
    );
  }

  @override
  Future<void> altereSituacaoAsync({
    required String identificadorDoEncontro,
    required String identificadorDoItem,
    required bool resolva,
  }) async {
    itemFoiResolvido = resolva;
  }
}

class RepositorioDaLinhaDoTempoFalso implements IRepositorioDaLinhaDoTempo {
  FiltroDaLinhaDoTempo? ultimoFiltro;

  @override
  Future<LinhaDoTempo> listeAsync(FiltroDaLinhaDoTempo filtro) async {
    ultimoFiltro = filtro;

    return LinhaDoTempo(
      filtro: filtro.rotulo,
      itens: <ItemDaLinhaDoTempo>[
        ItemDaLinhaDoTempo(
          identificadorDoEncontro: 'encontro-memoria',
          titulo: 'Noite para lembrar',
          descricao: 'Um encontro que virou história.',
          local: 'Casa da turma',
          inicio: DateTime(2026, 6, 20, 20),
          situacao: 'Realizado',
          quantidadeDeParticipantes: 8,
          quantidadeDeMemorias: 4,
          quantidadeDePublicacoes: 12,
          nomesDosParticipantesEmDestaque: const <String>[
            'Pessoa',
            'Bia',
          ],
        ),
      ],
    );
  }
}

class RepositorioDeMemoriasDoEncontroFalso
    implements IRepositorioDeMemoriasDoEncontro {
  RepositorioDeMemoriasDoEncontroFalso({this.comMemoriaInicial = false});

  final bool comMemoriaInicial;
  String? ultimaLegenda;
  String? ultimoNomeDoArquivo;
  String? ultimaMemoriaRemovida;

  @override
  Future<void> removaAsync({
    required String identificadorDoEncontro,
    required String identificadorDaMemoria,
  }) async {
    ultimaMemoriaRemovida = identificadorDaMemoria;
  }

  @override
  Future<List<MemoriaDoEncontro>> listeAsync(
    String identificadorDoEncontro,
  ) async {
    if (!comMemoriaInicial) {
      return <MemoriaDoEncontro>[];
    }

    return <MemoriaDoEncontro>[
      _crieMemoria(
        identificadorDoEncontro: identificadorDoEncontro,
        identificador: 'memoria-inicial',
        identificadorDaMidia: 'midia-inicial',
        legenda: 'Uma noite para lembrar',
      ),
    ];
  }

  @override
  Future<MemoriaDoEncontro> publiqueImagemAsync({
    required String identificadorDoEncontro,
    required String nomeDoArquivo,
    required String tipoDeConteudo,
    required Uint8List conteudo,
    String? legenda,
  }) async {
    ultimaLegenda = legenda;
    ultimoNomeDoArquivo = nomeDoArquivo;

    return _crieMemoria(
      identificadorDoEncontro: identificadorDoEncontro,
      identificador: 'memoria-nova',
      identificadorDaMidia: 'midia-nova',
      legenda: legenda,
    );
  }

  MemoriaDoEncontro _crieMemoria({
    required String identificadorDoEncontro,
    required String identificador,
    required String identificadorDaMidia,
    String? legenda,
  }) {
    return MemoriaDoEncontro(
      identificador: identificador,
      identificadorDoEncontro: identificadorDoEncontro,
      identificadorDoUsuarioAutor: 'usuario-1',
      nomeDoAutor: 'Pessoa Teste',
      legenda: legenda,
      criadoEm: DateTime(2026, 7, 18, 12),
      usuarioAtual: true,
      midias: <MidiaDaMemoria>[
        MidiaDaMemoria(
          identificador: identificadorDaMidia,
          url:
              '/api/encontros/$identificadorDoEncontro/memorias/$identificador/midias/$identificadorDaMidia/conteudo',
          tipoDeConteudo: 'image/png',
          tamanhoEmBytes: 3,
        ),
      ],
    );
  }
}

class RepositorioDePublicacoesDoEncontroFalso
    implements IRepositorioDePublicacoesDoEncontro {
  RepositorioDePublicacoesDoEncontroFalso({
    this.deveFalharAoPublicar = false,
    this.deveFalharAoListar = false,
    this.publicacoesPersonalizadas,
  });

  bool deveFalharAoPublicar;
  final bool deveFalharAoListar;
  final List<PublicacaoDoEncontro>? publicacoesPersonalizadas;
  String? ultimoTextoPublicado;
  final List<String> identificadoresDasOperacoes = <String>[];

  @override
  Future<List<PublicacaoDoEncontro>> listeAsync(
    String identificadorDoEncontro,
  ) async {
    if (deveFalharAoListar) {
      throw const ExcecaoDaApi(
        codigoHttp: 500,
        mensagem: 'Falha simulada ao listar momentos.',
      );
    }

    if (publicacoesPersonalizadas != null) {
      return List<PublicacaoDoEncontro>.from(publicacoesPersonalizadas!);
    }

    return <PublicacaoDoEncontro>[
      PublicacaoDoEncontro(
        identificador: 'publicacao-2',
        identificadorDoEncontro: identificadorDoEncontro,
        identificadorDoUsuarioAutor: 'usuario-1',
        nomeDoAutor: 'Pessoa Teste',
        texto: 'Até domingo!',
        publicadoEm: DateTime(2026, 7, 18, 10),
        ehAtualizacaoDoSistema: false,
        usuarioAtual: true,
      ),
      PublicacaoDoEncontro(
        identificador: 'publicacao-1',
        identificadorDoEncontro: identificadorDoEncontro,
        identificadorDoUsuarioAutor: 'usuario-2',
        nomeDoAutor: 'Bia Souza',
        texto: 'Chego com o café.',
        publicadoEm: DateTime(2026, 7, 18, 9),
        ehAtualizacaoDoSistema: false,
        usuarioAtual: false,
      ),
      PublicacaoDoEncontro(
        identificador: 'publicacao-sistema',
        identificadorDoEncontro: identificadorDoEncontro,
        identificadorDoUsuarioAutor: 'usuario-1',
        nomeDoAutor: 'Pessoa Teste',
        texto: 'Pessoa Teste criou o combinado "Levar bolo"',
        publicadoEm: DateTime(2026, 7, 18, 9, 30),
        ehAtualizacaoDoSistema: true,
        usuarioAtual: true,
      ),
    ];
  }

  @override
  Future<PublicacaoDoEncontro> publiqueAsync({
    required String identificadorDoEncontro,
    required String texto,
    required String identificadorDaOperacao,
  }) async {
    ultimoTextoPublicado = texto;
    identificadoresDasOperacoes.add(identificadorDaOperacao);

    if (deveFalharAoPublicar) {
      throw const ExcecaoDaApi(
        codigoHttp: 500,
        mensagem: 'Falha simulada ao publicar.',
      );
    }

    return PublicacaoDoEncontro(
      identificador: 'publicacao-nova',
      identificadorDoEncontro: identificadorDoEncontro,
      identificadorDoUsuarioAutor: 'usuario-1',
      nomeDoAutor: 'Pessoa Teste',
      texto: texto,
      publicadoEm: DateTime(2026, 7, 18, 11),
      ehAtualizacaoDoSistema: false,
      usuarioAtual: true,
    );
  }
}

class SeletorDeImagemFalso
    implements ISeletorDeImagem, ISeletorDeImagemPorOrigem {
  SeletorDeImagemFalso({this.temImagem = true});

  final bool temImagem;
  EnumeradorDeOrigemDaImagem? ultimaOrigem;

  @override
  Future<ImagemSelecionada?> selecionePorOrigemAsync(
    EnumeradorDeOrigemDaImagem origem,
  ) async {
    ultimaOrigem = origem;
    return selecioneAsync();
  }

  @override
  Future<ImagemSelecionada?> selecioneAsync() async {
    if (!temImagem) {
      return null;
    }

    return ImagemSelecionada(
      nome: 'capa.png',
      tipoDeConteudo: 'image/png',
      conteudo: base64Decode(
        'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=',
      ),
    );
  }
}

class RepositorioDeEncontrosFalso implements IRepositorioDeEncontros {
  RepositorioDeEncontrosFalso({
    this.deveFalhar = false,
    this.podeGerenciar = true,
    this.deveFalharAoConvidar = false,
    this.tipoInicial,
    this.aoCriar,
    this.situacaoInicialDoUsuario = 'Confirmado',
    String? urlInicialDaImagemDeCapa,
  }) : urlDaImagemDeCapa = urlInicialDaImagemDeCapa;

  final bool deveFalhar;
  final bool podeGerenciar;
  final bool deveFalharAoConvidar;
  final String? tipoInicial;
  final void Function(String titulo, String? local)? aoCriar;
  final String situacaoInicialDoUsuario;
  String? ultimoTitulo;
  String? ultimoLocal;
  String? ultimoTipo;
  String? ultimaSituacaoDePresenca;
  String? ultimoTituloEditado;
  String? ultimoTipoEditado;
  bool tipoFoiEditado = false;
  bool encontroFoiCancelado = false;
  String? ultimoNomeDaImagem;
  String? ultimoEmailConvidado;
  String? ultimoIdentificadorDoUsuarioConvidado;
  String? urlDaImagemDeCapa;
  bool imagemDeCapaFoiRemovida = false;

  @override
  Future<EncontroCriado> crieEncontroAsync({
    required String titulo,
    required DateTime inicioEm,
    String? descricao,
    String? local,
    double? latitude,
    double? longitude,
    String? tipo,
  }) async {
    if (deveFalhar) {
      throw const ExcecaoDaApi(
        codigoHttp: 500,
        mensagem: 'Não foi possível criar o encontro de teste.',
      );
    }

    ultimoTitulo = titulo;
    ultimoLocal = local;
    ultimoTipo = tipo;
    aoCriar?.call(titulo, local);

    return EncontroCriado(
      identificador: 'encontro-criado',
      titulo: titulo,
      descricao: descricao,
      local: local,
      inicioEm: inicioEm,
      situacao: 'Planejado',
      tipo: tipo,
    );
  }

  @override
  Future<EncontroDetalhado> obtenhaEncontroAsync(String identificador) async {
    return EncontroDetalhado(
      identificador: identificador,
      titulo: ultimoTituloEditado ?? 'Café de domingo',
      descricao: 'Um encontro para colocar a conversa em dia.',
      local: 'Casa da Ana',
      urlDaImagemDeCapa: urlDaImagemDeCapa,
      tipo: tipoFoiEditado ? ultimoTipoEditado : tipoInicial,
      inicioEm: DateTime(2026, 7, 19, 16),
      situacao: encontroFoiCancelado ? 'Cancelado' : 'Planejado',
      usuarioAtualConfirmouPresenca:
          (ultimaSituacaoDePresenca ?? situacaoInicialDoUsuario)
                  .toLowerCase() ==
              'confirmado',
      podeEditar: podeGerenciar && !encontroFoiCancelado,
      podeCancelar: podeGerenciar && !encontroFoiCancelado,
      participantes: <ParticipanteDoEncontro>[
        ParticipanteDoEncontro(
          identificadorDoUsuario: 'usuario-1',
          nome: 'Pessoa Teste',
          papel: 'Organizador',
          situacao: ultimaSituacaoDePresenca ?? situacaoInicialDoUsuario,
          usuarioAtual: true,
        ),
        const ParticipanteDoEncontro(
          identificadorDoUsuario: 'usuario-2',
          nome: 'Bia Souza',
          papel: 'Convidado',
          situacao: 'Talvez',
          usuarioAtual: false,
        ),
        if (ultimoEmailConvidado != null)
          const ParticipanteDoEncontro(
            identificadorDoUsuario: 'usuario-convidado',
            nome: 'Pessoa convidada',
            papel: 'Convidado',
            situacao: 'Convidado',
            usuarioAtual: false,
          ),
      ],
    );
  }

  @override
  Future<String> respondaPresencaAsync({
    required String identificador,
    required String situacao,
  }) async {
    ultimaSituacaoDePresenca = situacao;
    return situacao;
  }

  @override
  Future<void> editeEncontroAsync({
    required String identificador,
    required String titulo,
    required DateTime inicioEm,
    String? descricao,
    String? local,
    double? latitude,
    double? longitude,
    String? tipo,
  }) async {
    ultimoTituloEditado = titulo;
    ultimoTipoEditado = tipo;
    tipoFoiEditado = true;
  }

  @override
  Future<void> canceleEncontroAsync(String identificador) async {
    encontroFoiCancelado = true;
  }

  @override
  Future<String?> altereImagemDeCapaAsync({
    required String identificador,
    required String nomeDoArquivo,
    required String tipoDeConteudo,
    required Uint8List conteudo,
  }) async {
    ultimoNomeDaImagem = nomeDoArquivo;
    urlDaImagemDeCapa = '/arquivos/encontros/capa.png';
    return urlDaImagemDeCapa;
  }

  @override
  Future<void> convidePessoaAsync({
    required String identificador,
    required String email,
  }) async {
    if (deveFalharAoConvidar) {
      throw const ExcecaoDaApi(
        codigoHttp: 500,
        mensagem: 'Falha simulada ao convidar.',
      );
    }

    ultimoEmailConvidado = email;
  }

  @override
  Future<void> convidePessoaFrequenteAsync({
    required String identificador,
    required String identificadorDoUsuario,
  }) async {
    if (deveFalharAoConvidar) {
      throw const ExcecaoDaApi(
        codigoHttp: 500,
        mensagem: 'Falha simulada ao convidar.',
      );
    }

    ultimoIdentificadorDoUsuarioConvidado = identificadorDoUsuario;
  }

  @override
  Future<void> removaImagemDeCapaAsync(String identificador) async {
    imagemDeCapaFoiRemovida = true;
    urlDaImagemDeCapa = null;
  }
}

class RepositorioDePessoasFrequentesFalso
    implements IRepositorioDePessoasFrequentes {
  RepositorioDePessoasFrequentesFalso({
    this.pessoas = const <PessoaFrequente>[],
  });

  final List<PessoaFrequente> pessoas;

  @override
  Future<List<PessoaFrequente>> listeAsync() async {
    return pessoas;
  }
}

class RepositorioDaPaginaInicialFalso implements IRepositorioDaPaginaInicial {
  RepositorioDaPaginaInicialFalso({
    List<EncontroResumo>? encontros,
    this.deveFalhar = false,
    this.urlDaFotoDePerfil,
  }) : encontros = encontros ??
            <EncontroResumo>[
              EncontroResumo(
                identificador: 'encontro-1',
                titulo: 'Café de domingo',
                local: 'Casa da Ana',
                inicioEm: DateTime(2026, 7, 19, 16),
                situacao: 'Planejado',
                quantidadeDePresencasConfirmadas: 3,
                usuarioAtualConfirmouPresenca: true,
              ),
            ];

  final List<EncontroResumo> encontros;
  final bool deveFalhar;
  String? urlDaFotoDePerfil;
  int quantidadeDeListagens = 0;

  @override
  Future<List<EncontroResumo>> listeProximosEncontrosAsync() async {
    quantidadeDeListagens++;
    return encontros;
  }

  @override
  Future<UsuarioAtual> obtenhaUsuarioAtualAsync() async {
    if (deveFalhar) {
      throw const ExcecaoDaApi(
        codigoHttp: 500,
        mensagem: 'Falha simulada ao carregar a home.',
      );
    }

    return UsuarioAtual(
      identificador: 'usuario-1',
      nome: 'Pessoa Teste',
      email: 'pessoa@email.com',
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
  bool fotoFoiRemovida = false;

  @override
  Future<UsuarioAtual> altereNomeAsync(String nome) async {
    ultimoNome = nome;
    return UsuarioAtual(
      identificador: 'usuario-1',
      nome: nome,
      email: 'pessoa@email.com',
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
      email: 'pessoa@email.com',
      urlDaFotoDePerfil: url,
    );
  }

  @override
  Future<UsuarioAtual> removaFotoAsync() async {
    fotoFoiRemovida = true;
    aoAtualizarUrl?.call(null);

    return const UsuarioAtual(
      identificador: 'usuario-1',
      nome: 'Pessoa Teste',
      email: 'pessoa@email.com',
    );
  }
}

class RepositorioDeAutenticacaoFalso implements IRepositorioDeAutenticacao {
  RepositorioDeAutenticacaoFalso({
    this.sessaoPodeSerRestaurada = false,
  });

  final bool sessaoPodeSerRestaurada;
  String? emailDoUltimoLogin;
  String? emailDoUltimoCadastro;

  @override
  Future<RespostaDeSessao> autentiqueAsync({
    required String email,
    required String senha,
  }) async {
    emailDoUltimoLogin = email;
    return _crieRespostaDeSessao();
  }

  @override
  Future<void> cadastreAsync({
    required String nome,
    required String email,
    required String senha,
  }) async {
    emailDoUltimoCadastro = email;
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
