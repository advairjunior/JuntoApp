import 'package:flutter/material.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/espacamentos_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/funcionalidades/encontros/servicos/seletor_de_imagem.dart';

Future<EnumeradorDeOrigemDaImagem?> escolhaOrigemDaImagemAsync(
  BuildContext context, {
  String titulo = 'Adicionar foto',
}) {
  return showModalBottomSheet<EnumeradorDeOrigemDaImagem>(
    context: context,
    backgroundColor: CoresDoAplicativo.fundoDoCartao,
    showDragHandle: true,
    builder: (BuildContext contextoDaFolha) {
      return SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(16, 0, 16, 20),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: <Widget>[
              Text(titulo, style: Theme.of(context).textTheme.titleLarge),
              const SizedBox(height: EspacamentosDoAplicativo.medio),
              ListTile(
                key: const Key('tirar-foto-pela-camera'),
                leading: const Icon(Icons.camera_alt_outlined),
                title: const Text('Tirar foto'),
                onTap: () => Navigator.of(contextoDaFolha).pop(
                  EnumeradorDeOrigemDaImagem.camera,
                ),
              ),
              ListTile(
                key: const Key('escolher-foto-da-galeria'),
                leading: const Icon(Icons.photo_library_outlined),
                title: const Text('Escolher da galeria'),
                onTap: () => Navigator.of(contextoDaFolha).pop(
                  EnumeradorDeOrigemDaImagem.galeria,
                ),
              ),
            ],
          ),
        ),
      );
    },
  );
}
