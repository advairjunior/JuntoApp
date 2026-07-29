import 'package:flutter/material.dart';

abstract final class SombrasDoAplicativo {
  static const List<BoxShadow> baixa = <BoxShadow>[
    BoxShadow(
      color: Color(0x33000000),
      blurRadius: 14,
      offset: Offset(0, 6),
    ),
  ];

  static const List<BoxShadow> elevada = <BoxShadow>[
    BoxShadow(
      color: Color(0x59000000),
      blurRadius: 28,
      offset: Offset(0, 12),
    ),
  ];
}
