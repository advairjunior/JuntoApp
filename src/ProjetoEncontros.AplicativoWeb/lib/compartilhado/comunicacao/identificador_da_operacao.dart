import 'dart:math';

String crieIdentificadorDaOperacao() {
  Random gerador = Random.secure();
  List<int> bytes = List<int>.generate(16, (_) => gerador.nextInt(256));

  bytes[6] = (bytes[6] & 0x0f) | 0x40;
  bytes[8] = (bytes[8] & 0x3f) | 0x80;

  String hexadecimal =
      bytes.map((int byte) => byte.toRadixString(16).padLeft(2, '0')).join();

  return '${hexadecimal.substring(0, 8)}-'
      '${hexadecimal.substring(8, 12)}-'
      '${hexadecimal.substring(12, 16)}-'
      '${hexadecimal.substring(16, 20)}-'
      '${hexadecimal.substring(20)}';
}
