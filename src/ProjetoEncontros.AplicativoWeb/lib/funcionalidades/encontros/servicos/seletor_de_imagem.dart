import 'dart:typed_data';

import 'package:file_picker/file_picker.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';

enum EnumeradorDeOrigemDaImagem { camera, galeria }

class ImagemSelecionada {
  const ImagemSelecionada({
    required this.nome,
    required this.tipoDeConteudo,
    required this.conteudo,
  });

  final String nome;
  final String tipoDeConteudo;
  final Uint8List conteudo;
}

abstract interface class ISeletorDeImagem {
  Future<ImagemSelecionada?> selecioneAsync();
}

abstract interface class ISeletorDeImagemPorOrigem {
  Future<ImagemSelecionada?> selecionePorOrigemAsync(
    EnumeradorDeOrigemDaImagem origem,
  );
}

extension SelecaoDeImagemPorOrigem on ISeletorDeImagem {
  Future<ImagemSelecionada?> selecionePorOrigemAsync(
    EnumeradorDeOrigemDaImagem origem,
  ) {
    ISeletorDeImagem seletor = this;

    if (seletor is ISeletorDeImagemPorOrigem) {
      return (seletor as ISeletorDeImagemPorOrigem)
          .selecionePorOrigemAsync(origem);
    }

    return selecioneAsync();
  }
}

final provedorDoSeletorDeImagem = Provider<ISeletorDeImagem>((Ref referencia) {
  return SeletorDeImagem();
});

class SeletorDeImagem implements ISeletorDeImagem, ISeletorDeImagemPorOrigem {
  SeletorDeImagem({ImagePicker? seletorNativo})
      : _seletorNativo = seletorNativo ?? ImagePicker();

  final ImagePicker _seletorNativo;

  @override
  Future<ImagemSelecionada?> selecioneAsync() async {
    return selecionePorOrigemAsync(EnumeradorDeOrigemDaImagem.galeria);
  }

  @override
  Future<ImagemSelecionada?> selecionePorOrigemAsync(
    EnumeradorDeOrigemDaImagem origem,
  ) async {
    try {
      XFile? arquivo = await _seletorNativo.pickImage(
        source: origem == EnumeradorDeOrigemDaImagem.camera
            ? ImageSource.camera
            : ImageSource.gallery,
      );

      if (arquivo == null) {
        return null;
      }

      Uint8List conteudo = await arquivo.readAsBytes();
      return ImagemSelecionada(
        nome: arquivo.name,
        tipoDeConteudo:
            arquivo.mimeType ?? _obtenhaTipoDeConteudo(arquivo.name),
        conteudo: conteudo,
      );
    } catch (_) {
      return _selecioneArquivoAsync();
    }
  }

  Future<ImagemSelecionada?> _selecioneArquivoAsync() async {
    FilePickerResult? resultado = await FilePicker.pickFiles(
      type: FileType.custom,
      allowedExtensions: <String>['jpg', 'jpeg', 'png', 'webp'],
      allowMultiple: false,
      withData: true,
    );

    if (resultado == null) {
      return null;
    }

    PlatformFile arquivo = resultado.files.single;
    Uint8List? conteudo = arquivo.bytes;

    if (conteudo == null) {
      return null;
    }

    return ImagemSelecionada(
      nome: arquivo.name,
      tipoDeConteudo: _obtenhaTipoDeConteudo(
        arquivo.extension ?? arquivo.name,
      ),
      conteudo: conteudo,
    );
  }

  String _obtenhaTipoDeConteudo(String nomeOuExtensao) {
    String extensao = nomeOuExtensao.split('.').last.toLowerCase();

    return switch (extensao) {
      'jpg' || 'jpeg' => 'image/jpeg',
      'png' => 'image/png',
      'webp' => 'image/webp',
      _ => 'application/octet-stream',
    };
  }
}
