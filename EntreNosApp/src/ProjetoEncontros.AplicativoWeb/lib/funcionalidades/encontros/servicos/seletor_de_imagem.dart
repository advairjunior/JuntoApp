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

class MidiaSelecionada {
  const MidiaSelecionada({
    required this.nome,
    required this.tipoDeConteudo,
    required this.conteudo,
  });

  factory MidiaSelecionada.deImagem(ImagemSelecionada imagem) {
    return MidiaSelecionada(
      nome: imagem.nome,
      tipoDeConteudo: imagem.tipoDeConteudo,
      conteudo: imagem.conteudo,
    );
  }

  final String nome;
  final String tipoDeConteudo;
  final Uint8List conteudo;

  bool get ehVideo => tipoDeConteudo.toLowerCase().startsWith('video/');

  bool get ehAudio => tipoDeConteudo.toLowerCase().startsWith('audio/');
}

abstract interface class ISeletorDeImagem {
  Future<ImagemSelecionada?> selecioneAsync();
}

abstract interface class ISeletorDeImagemPorOrigem {
  Future<ImagemSelecionada?> selecionePorOrigemAsync(
    EnumeradorDeOrigemDaImagem origem,
  );
}

abstract interface class ISeletorDeMidias {
  Future<List<MidiaSelecionada>> selecioneMidiasPorOrigemAsync(
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

extension SelecaoDeMidiasPorOrigem on ISeletorDeImagem {
  Future<List<MidiaSelecionada>> selecioneMidiasPorOrigemAsync(
    EnumeradorDeOrigemDaImagem origem,
  ) async {
    ISeletorDeImagem seletor = this;

    if (seletor is ISeletorDeMidias) {
      return (seletor as ISeletorDeMidias)
          .selecioneMidiasPorOrigemAsync(origem);
    }

    ImagemSelecionada? imagem = await selecionePorOrigemAsync(origem);
    return imagem == null
        ? <MidiaSelecionada>[]
        : <MidiaSelecionada>[MidiaSelecionada.deImagem(imagem)];
  }
}

final provedorDoSeletorDeImagem = Provider<ISeletorDeImagem>((Ref referencia) {
  return SeletorDeImagem();
});

class SeletorDeImagem
    implements ISeletorDeImagem, ISeletorDeImagemPorOrigem, ISeletorDeMidias {
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

  @override
  Future<List<MidiaSelecionada>> selecioneMidiasPorOrigemAsync(
    EnumeradorDeOrigemDaImagem origem,
  ) async {
    if (origem == EnumeradorDeOrigemDaImagem.camera) {
      ImagemSelecionada? imagem = await selecionePorOrigemAsync(origem);
      return imagem == null
          ? <MidiaSelecionada>[]
          : <MidiaSelecionada>[MidiaSelecionada.deImagem(imagem)];
    }

    try {
      return await _selecioneArquivosDeMidiaAsync();
    } catch (_) {
      return <MidiaSelecionada>[];
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

  Future<List<MidiaSelecionada>> _selecioneArquivosDeMidiaAsync() async {
    FilePickerResult? resultado = await FilePicker.pickFiles(
      type: FileType.custom,
      allowedExtensions: <String>[
        'jpg',
        'jpeg',
        'png',
        'webp',
        'mp4',
        'mov',
        'webm',
      ],
      allowMultiple: true,
      withData: true,
    );

    if (resultado == null) {
      return <MidiaSelecionada>[];
    }

    return resultado.files
        .where((PlatformFile arquivo) => arquivo.bytes != null)
        .map(
          (PlatformFile arquivo) => MidiaSelecionada(
            nome: arquivo.name,
            tipoDeConteudo: _obtenhaTipoDeConteudo(
              arquivo.extension ?? arquivo.name,
            ),
            conteudo: arquivo.bytes!,
          ),
        )
        .toList();
  }

  String _obtenhaTipoDeConteudo(String nomeOuExtensao) {
    String extensao = nomeOuExtensao.split('.').last.toLowerCase();

    return switch (extensao) {
      'jpg' || 'jpeg' => 'image/jpeg',
      'png' => 'image/png',
      'webp' => 'image/webp',
      'mp4' => 'video/mp4',
      'mov' => 'video/quicktime',
      'webm' => 'video/webm',
      _ => 'application/octet-stream',
    };
  }
}
