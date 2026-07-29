import 'package:flutter_test/flutter_test.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/configuracao/configuracao_do_ambiente.dart';

void main() {
  test('deve usar o mesmo site da aplicacao web no ambiente local', () {
    Uri enderecoDaApi = Uri.parse(ConfiguracaoDoAmbiente.urlDaApi);

    expect(enderecoDaApi.host, 'localhost');
    expect(enderecoDaApi.port, 5281);
  });

  test('deve criar URL absoluta usando a API local', () {
    String endereco = ConfiguracaoDoAmbiente.crieUrlAbsoluta(
      '/arquivos/perfis/foto.png',
    );

    expect(endereco, 'http://localhost:5281/arquivos/perfis/foto.png');
  });
}
