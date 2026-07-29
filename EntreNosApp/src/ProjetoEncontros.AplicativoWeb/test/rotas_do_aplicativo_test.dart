import 'package:flutter_test/flutter_test.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/autenticacao/estado_da_sessao.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/navegacao/rotas_do_aplicativo.dart';

void main() {
  const String caminhoDoConvite = '/convite/token-compartilhado';

  test('deve preservar convite enquanto restaura a sessao', () {
    String? redirecionamento = redirecioneRota(
      sessao: const EstadoDaSessao.restaurando(),
      enderecoDaRota: Uri.parse(caminhoDoConvite),
    );

    Uri enderecoDaInicializacao = Uri.parse(redirecionamento!);

    expect(enderecoDaInicializacao.path, '/inicializacao');
    expect(
      enderecoDaInicializacao.queryParameters['retorno'],
      caminhoDoConvite,
    );
  });

  test('deve encaminhar convite para entrada preservando o retorno', () {
    String? redirecionamento = redirecioneRota(
      sessao: const EstadoDaSessao(
        situacao: SituacaoDaSessao.naoAutenticada,
      ),
      enderecoDaRota: Uri.parse(caminhoDoConvite),
    );

    Uri enderecoDaEntrada = Uri.parse(redirecionamento!);

    expect(enderecoDaEntrada.path, '/entrada');
    expect(enderecoDaEntrada.queryParameters['retorno'], caminhoDoConvite);
  });

  test('deve retomar convite depois de restaurar a sessao', () {
    EstadoDaSessao sessaoAutenticada = EstadoDaSessao(
      situacao: SituacaoDaSessao.autenticada,
      tokenDeAcesso: 'token-de-teste',
      expiraEm: DateTime.now().add(const Duration(minutes: 15)),
    );
    String? redirecionamento = redirecioneRota(
      sessao: sessaoAutenticada,
      enderecoDaRota: Uri(
        path: '/inicializacao',
        queryParameters: const <String, String>{
          'retorno': caminhoDoConvite,
        },
      ),
    );

    expect(redirecionamento, caminhoDoConvite);
  });

  test('deve preservar convite na entrada sem sessao restaurada', () {
    String? redirecionamento = redirecioneRota(
      sessao: const EstadoDaSessao(
        situacao: SituacaoDaSessao.naoAutenticada,
      ),
      enderecoDaRota: Uri(
        path: '/inicializacao',
        queryParameters: const <String, String>{
          'retorno': caminhoDoConvite,
        },
      ),
    );

    Uri enderecoDaEntrada = Uri.parse(redirecionamento!);

    expect(enderecoDaEntrada.path, '/entrada');
    expect(enderecoDaEntrada.queryParameters['retorno'], caminhoDoConvite);
  });

  test('deve preservar convite ao restaurar sessao durante a entrada', () {
    String? redirecionamento = redirecioneRota(
      sessao: const EstadoDaSessao.restaurando(),
      enderecoDaRota: Uri(
        path: '/entrada',
        queryParameters: const <String, String>{
          'retorno': caminhoDoConvite,
        },
      ),
    );

    Uri enderecoDaInicializacao = Uri.parse(redirecionamento!);

    expect(enderecoDaInicializacao.path, '/inicializacao');
    expect(
      enderecoDaInicializacao.queryParameters['retorno'],
      caminhoDoConvite,
    );
  });

  test('deve retomar convite depois do login', () {
    EstadoDaSessao sessaoAutenticada = EstadoDaSessao(
      situacao: SituacaoDaSessao.autenticada,
      tokenDeAcesso: 'token-de-teste',
      expiraEm: DateTime.now().add(const Duration(minutes: 15)),
    );
    String? redirecionamento = redirecioneRota(
      sessao: sessaoAutenticada,
      enderecoDaRota: Uri(
        path: '/entrada',
        queryParameters: const <String, String>{
          'retorno': caminhoDoConvite,
        },
      ),
    );

    expect(redirecionamento, caminhoDoConvite);
  });
}
