import 'dart:typed_data';

import 'package:flutter_test/flutter_test.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/dados/repositorio_de_encontros.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/estado/controlador_do_detalhe_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_criado.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_detalhado.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/participante_do_encontro.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/dados/repositorio_da_pagina_inicial.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/estado/controlador_da_pagina_inicial.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/convite_do_encontro_resumo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/encontro_resumo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/inicio/modelos/usuario_atual.dart';

void main() {
  test('marca encontro como realizado e recarrega o detalhe', () async {
    _RepositorioDeEncontrosFalso repositorio = _RepositorioDeEncontrosFalso();
    ControladorDoDetalheDoEncontro controlador =
        ControladorDoDetalheDoEncontro('encontro-1', repositorio);

    await controlador.carregueAsync();
    bool realizou = await controlador.marqueEncontroComoRealizadoAsync();

    expect(realizou, isTrue);
    expect(controlador.state.encontro?.situacao, 'Realizado');
    expect(controlador.state.mensagemDeSucesso,
        'Encontro marcado como realizado.');
  });

  test('responde convite e atualiza a pagina inicial', () async {
    _RepositorioDeEncontrosFalso encontros = _RepositorioDeEncontrosFalso();
    _RepositorioDaPaginaInicialFalso paginaInicial =
        _RepositorioDaPaginaInicialFalso(encontros);
    ControladorDaPaginaInicial controlador =
        ControladorDaPaginaInicial(paginaInicial, encontros);

    await controlador.carregueAsync();
    expect(controlador.state.convitesPendentes, hasLength(1));

    bool respondeu = await controlador.respondaConviteAsync(
      identificadorDoEncontro: 'encontro-1',
      situacao: 'Talvez',
    );

    expect(respondeu, isTrue);
    expect(encontros.ultimaSituacao, 'Talvez');
    expect(controlador.state.convitesPendentes, isEmpty);
  });
}

class _RepositorioDaPaginaInicialFalso
    implements
        IRepositorioDaPaginaInicial,
        IRepositorioDeConvitesDaPaginaInicial {
  _RepositorioDaPaginaInicialFalso(this._repositorioDeEncontros);

  final _RepositorioDeEncontrosFalso _repositorioDeEncontros;

  @override
  Future<List<ConviteDoEncontroResumo>> listeConvitesPendentesAsync() async {
    if (_repositorioDeEncontros.ultimaSituacao != null) {
      return <ConviteDoEncontroResumo>[];
    }

    return <ConviteDoEncontroResumo>[
      ConviteDoEncontroResumo(
        identificadorDoEncontro: 'encontro-1',
        titulo: 'Café de domingo',
        inicioEm: DateTime(2026, 7, 19, 16),
        situacao: 'Convidado',
        convidadoEm: DateTime(2026, 7, 16),
      ),
    ];
  }

  @override
  Future<List<EncontroResumo>> listeProximosEncontrosAsync() async {
    return <EncontroResumo>[];
  }

  @override
  Future<UsuarioAtual> obtenhaUsuarioAtualAsync() async {
    return const UsuarioAtual(
      identificador: 'usuario-1',
      nome: 'Pessoa Teste',
      email: 'pessoa@teste.com',
    );
  }
}

class _RepositorioDeEncontrosFalso
    implements IRepositorioDeEncontros, IRepositorioDeRealizacaoDeEncontro {
  bool realizado = false;
  String? ultimaSituacao;

  @override
  Future<void> marqueEncontroComoRealizadoAsync(String identificador) async {
    realizado = true;
  }

  @override
  Future<EncontroDetalhado> obtenhaEncontroAsync(String identificador) async {
    return EncontroDetalhado(
      identificador: identificador,
      titulo: 'Café de domingo',
      inicioEm: DateTime(2026, 7, 19, 16),
      situacao: realizado ? 'Realizado' : 'Planejado',
      usuarioAtualConfirmouPresenca: true,
      podeEditar: !realizado,
      podeCancelar: !realizado,
      participantes: const <ParticipanteDoEncontro>[],
    );
  }

  @override
  Future<String> respondaPresencaAsync({
    required String identificador,
    required String situacao,
  }) async {
    ultimaSituacao = situacao;
    return situacao;
  }

  @override
  Future<String?> altereImagemDeCapaAsync({
    required String identificador,
    required String nomeDoArquivo,
    required String tipoDeConteudo,
    required Uint8List conteudo,
  }) =>
      throw UnimplementedError();

  @override
  Future<void> canceleEncontroAsync(String identificador) =>
      throw UnimplementedError();

  @override
  Future<void> convidePessoaAsync({
    required String identificador,
    required String email,
  }) =>
      throw UnimplementedError();

  @override
  Future<void> convidePessoaFrequenteAsync({
    required String identificador,
    required String identificadorDoUsuario,
  }) =>
      throw UnimplementedError();

  @override
  Future<EncontroCriado> crieEncontroAsync({
    required String titulo,
    required DateTime inicioEm,
    String? descricao,
    String? local,
    double? latitude,
    double? longitude,
    String? tipo,
  }) =>
      throw UnimplementedError();

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
  }) =>
      throw UnimplementedError();

  @override
  Future<void> removaImagemDeCapaAsync(String identificador) =>
      throw UnimplementedError();
}
