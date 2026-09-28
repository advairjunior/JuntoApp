import 'package:flutter_test/flutter_test.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/estado_da_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/navegacao/rotas_do_aplicativo.dart';

void main() {
  test('deve preservar rota do nucleo enquanto restaura a sessao', () {
    String? redirecionamento = redirecioneRota(
      sessao: const EstadoDaSessao.restaurando(),
      enderecoDaRota: Uri.parse('/grupos'),
    );

    Uri enderecoDaInicializacao = Uri.parse(redirecionamento!);

    expect(enderecoDaInicializacao.path, '/inicializacao');
    expect(enderecoDaInicializacao.queryParameters['retorno'], '/grupos');
  });

  test('deve encaminhar rota privada para entrada preservando o retorno', () {
    String? redirecionamento = redirecioneRota(
      sessao: const EstadoDaSessao(
        situacao: SituacaoDaSessao.naoAutenticada,
      ),
      enderecoDaRota: Uri.parse('/pessoas'),
    );

    Uri enderecoDaEntrada = Uri.parse(redirecionamento!);

    expect(enderecoDaEntrada.path, '/entrada');
    expect(enderecoDaEntrada.queryParameters['retorno'], '/pessoas');
  });

  test('deve retomar rota valida depois de restaurar a sessao', () {
    String? redirecionamento = redirecioneRota(
      sessao: _crieSessaoAutenticada(),
      enderecoDaRota: Uri(
        path: '/inicializacao',
        queryParameters: const <String, String>{'retorno': '/perfil'},
      ),
    );

    expect(redirecionamento, '/perfil');
  });

  test('deve descartar retorno para rota removida depois do login', () {
    String? redirecionamento = redirecioneRota(
      sessao: _crieSessaoAutenticada(),
      enderecoDaRota: Uri(
        path: '/entrada',
        queryParameters: const <String, String>{
          'retorno': '/convite/token-antigo',
        },
      ),
    );

    expect(redirecionamento, '/inicio');
  });

  test('deve direcionar rota removida autenticada para o inicio', () {
    String? redirecionamento = redirecioneRota(
      sessao: _crieSessaoAutenticada(),
      enderecoDaRota: Uri.parse('/memorias'),
    );

    expect(redirecionamento, '/inicio');
  });

  test('deve pedir entrada sem preservar uma rota removida', () {
    String? redirecionamento = redirecioneRota(
      sessao: const EstadoDaSessao(
        situacao: SituacaoDaSessao.naoAutenticada,
      ),
      enderecoDaRota: Uri.parse('/notificacoes'),
    );

    expect(redirecionamento, '/entrada');
  });
}

EstadoDaSessao _crieSessaoAutenticada() {
  return EstadoDaSessao(
    situacao: SituacaoDaSessao.autenticada,
    tokenDeAcesso: 'token-de-teste',
    expiraEm: DateTime.now().add(const Duration(minutes: 15)),
  );
}
