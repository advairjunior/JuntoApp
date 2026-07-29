import 'package:flutter_test/flutter_test.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/modelos/encontro_detalhado.dart';

void main() {
  test('deve ler a localização fixa retornada pela API', () {
    EncontroDetalhado encontro = EncontroDetalhado.deJson(
      <String, dynamic>{
        'identificador': 'encontro-1',
        'titulo': 'Churrasco',
        'descricao': null,
        'local': 'Casa da Ana',
        'localizacao': <String, dynamic>{
          'descricao': 'Casa da Ana',
          'latitude': -23.55052,
          'longitude': -46.633308,
        },
        'urlDaImagemDeCapa': null,
        'inicioEm': '2027-08-01T22:00:00Z',
        'situacao': 'Planejado',
        'usuarioAtualConfirmouPresenca': false,
        'podeEditar': true,
        'podeCancelar': true,
        'participantes': <dynamic>[],
        'tipo': null,
      },
    );

    expect(encontro.localizacao, isNotNull);
    expect(encontro.localizacao!.descricao, 'Casa da Ana');
    expect(encontro.localizacao!.latitude, -23.55052);
    expect(encontro.localizacao!.longitude, -46.633308);
    expect(encontro.localizacao!.temCoordenadas, isTrue);
  });

  test('deve manter compatibilidade com local textual antigo', () {
    EncontroDetalhado encontro = EncontroDetalhado.deJson(
      <String, dynamic>{
        'identificador': 'encontro-1',
        'titulo': 'Churrasco',
        'descricao': null,
        'local': 'Salão do condomínio',
        'urlDaImagemDeCapa': null,
        'inicioEm': '2027-08-01T22:00:00Z',
        'situacao': 'Planejado',
        'usuarioAtualConfirmouPresenca': false,
        'podeEditar': true,
        'podeCancelar': true,
        'participantes': <dynamic>[],
        'tipo': null,
      },
    );

    expect(encontro.local, 'Salão do condomínio');
    expect(encontro.localizacao, isNull);
  });
}
