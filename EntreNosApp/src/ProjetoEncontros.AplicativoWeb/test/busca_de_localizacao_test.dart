import 'package:flutter_test/flutter_test.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/servicos/servico_de_busca_de_localizacao.dart';

void main() {
  test('deve converter resultado de busca em localizacao selecionavel', () {
    ResultadoDaBuscaDeLocalizacao resultado =
        ResultadoDaBuscaDeLocalizacao.deJson(
      <String, dynamic>{
        'descricao':
            'Parque Ibirapuera, Moema, São Paulo, 04002-010, Brasil',
        'latitude': -23.5877126,
        'longitude': -46.6585214,
      },
    );

    expect(resultado.descricao, contains('Parque Ibirapuera'));
    expect(resultado.latitude, -23.5877126);
    expect(resultado.longitude, -46.6585214);
  });
}
