import 'package:flutter/material.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/configuracao/configuracao_do_ambiente.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/imagens/imagem_privada.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';

class FotoDePerfil extends StatelessWidget {
  const FotoDePerfil({
    required this.url,
    required this.iniciais,
    required this.dimensao,
    this.tamanhoDasIniciais,
    super.key,
  });

  final String? url;
  final String iniciais;
  final double dimensao;
  final double? tamanhoDasIniciais;

  @override
  Widget build(BuildContext context) {
    String recurso = ConfiguracaoDoAmbiente.crieUrlAbsoluta(url);

    return Container(
      width: dimensao,
      height: dimensao,
      clipBehavior: Clip.antiAlias,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        color: CoresDoAplicativo.fundoDoCartaoSuave,
        border: Border.all(color: CoresDoAplicativo.verdeEscuro),
      ),
      child: recurso.isEmpty
          ? _Iniciais(
              iniciais: iniciais,
              tamanho: tamanhoDasIniciais,
            )
          : ImagemPrivada(
              recurso: recurso,
              construaSubstituta: (_) => _Iniciais(
                iniciais: iniciais,
                tamanho: tamanhoDasIniciais,
              ),
            ),
    );
  }
}

class _Iniciais extends StatelessWidget {
  const _Iniciais({required this.iniciais, this.tamanho});

  final String iniciais;
  final double? tamanho;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Text(
        iniciais.trim().isEmpty ? '?' : iniciais,
        style: TextStyle(
          fontSize: tamanho,
          fontWeight: FontWeight.w700,
        ),
      ),
    );
  }
}
