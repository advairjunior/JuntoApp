import 'package:flutter/material.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';

abstract final class TipografiaDoAplicativo {
  static const TextStyle tituloGrande = TextStyle(
    color: CoresDoAplicativo.textoPrincipal,
    fontSize: 28,
    fontWeight: FontWeight.w700,
    height: 1.14,
  );

  static const TextStyle tituloMedio = TextStyle(
    color: CoresDoAplicativo.textoPrincipal,
    fontSize: 22,
    fontWeight: FontWeight.w700,
    height: 1.2,
  );

  static const TextStyle tituloDeSecao = TextStyle(
    color: CoresDoAplicativo.textoPrincipal,
    fontSize: 20,
    fontWeight: FontWeight.w700,
    height: 1.2,
  );

  static const TextStyle tituloDeCartao = TextStyle(
    color: CoresDoAplicativo.textoPrincipal,
    fontSize: 18,
    fontWeight: FontWeight.w700,
    height: 1.25,
  );

  static const TextStyle corpo = TextStyle(
    color: CoresDoAplicativo.textoPrincipal,
    fontSize: 15,
    fontWeight: FontWeight.w400,
    height: 1.45,
  );

  static const TextStyle corpoSecundario = TextStyle(
    color: CoresDoAplicativo.textoSecundario,
    fontSize: 14,
    fontWeight: FontWeight.w400,
    height: 1.4,
  );

  static const TextStyle legenda = TextStyle(
    color: CoresDoAplicativo.textoTerciario,
    fontSize: 12,
    fontWeight: FontWeight.w500,
    height: 1.35,
  );

  static const TextStyle acao = TextStyle(
    color: CoresDoAplicativo.textoPrincipal,
    fontSize: 15,
    fontWeight: FontWeight.w600,
    height: 1.2,
  );
}
