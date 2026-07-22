import 'package:flutter/material.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/cores_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/raios_do_aplicativo.dart';
import 'package:projeto_encontros_aplicativo_web/compartilhado/tema/tipografia_do_aplicativo.dart';

abstract final class TemaDoAplicativo {
  static ThemeData get escuro {
    ColorScheme esquemaDeCores = const ColorScheme.dark(
      primary: CoresDoAplicativo.verdeDestaque,
      onPrimary: CoresDoAplicativo.fundoPrincipal,
      secondary: CoresDoAplicativo.ambar,
      onSecondary: CoresDoAplicativo.fundoPrincipal,
      error: CoresDoAplicativo.perigo,
      onError: CoresDoAplicativo.textoPrincipal,
      surface: CoresDoAplicativo.fundoDoCartao,
      onSurface: CoresDoAplicativo.textoPrincipal,
      outline: CoresDoAplicativo.bordaSuave,
      outlineVariant: CoresDoAplicativo.bordaDiscreta,
    );
    OutlineInputBorder bordaDoCampo = OutlineInputBorder(
      borderRadius: BorderRadius.circular(RaiosDoAplicativo.medio),
      borderSide: const BorderSide(color: CoresDoAplicativo.bordaDiscreta),
    );

    return ThemeData(
      useMaterial3: true,
      brightness: Brightness.dark,
      colorScheme: esquemaDeCores,
      scaffoldBackgroundColor: CoresDoAplicativo.fundoPrincipal,
      canvasColor: CoresDoAplicativo.fundoSecundario,
      dividerColor: CoresDoAplicativo.bordaDiscreta,
      splashFactory: InkSparkle.splashFactory,
      textTheme: const TextTheme(
        headlineLarge: TipografiaDoAplicativo.tituloGrande,
        headlineMedium: TipografiaDoAplicativo.tituloMedio,
        headlineSmall: TipografiaDoAplicativo.tituloDeSecao,
        titleLarge: TipografiaDoAplicativo.tituloDeSecao,
        titleMedium: TipografiaDoAplicativo.tituloDeCartao,
        bodyLarge: TipografiaDoAplicativo.corpo,
        bodyMedium: TipografiaDoAplicativo.corpoSecundario,
        bodySmall: TipografiaDoAplicativo.legenda,
        labelLarge: TipografiaDoAplicativo.acao,
        labelMedium: TipografiaDoAplicativo.legenda,
      ),
      appBarTheme: const AppBarTheme(
        elevation: 0,
        scrolledUnderElevation: 0,
        centerTitle: false,
        backgroundColor: CoresDoAplicativo.fundoPrincipal,
        foregroundColor: CoresDoAplicativo.textoPrincipal,
        surfaceTintColor: CoresDoAplicativo.transparente,
        titleTextStyle: TipografiaDoAplicativo.tituloDeCartao,
      ),
      cardTheme: CardThemeData(
        elevation: 0,
        color: CoresDoAplicativo.fundoDoCartao,
        surfaceTintColor: CoresDoAplicativo.transparente,
        margin: EdgeInsets.zero,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(RaiosDoAplicativo.grande),
          side: const BorderSide(color: CoresDoAplicativo.bordaDiscreta),
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: CoresDoAplicativo.fundoElevado,
        contentPadding: const EdgeInsets.symmetric(
          horizontal: 18,
          vertical: 16,
        ),
        hintStyle: TipografiaDoAplicativo.corpoSecundario.copyWith(
          color: CoresDoAplicativo.textoTerciario,
        ),
        prefixIconColor: CoresDoAplicativo.verdeDestaque,
        suffixIconColor: CoresDoAplicativo.textoSecundario,
        border: bordaDoCampo,
        enabledBorder: bordaDoCampo,
        focusedBorder: bordaDoCampo.copyWith(
          borderSide: const BorderSide(
            color: CoresDoAplicativo.verdeDestaque,
            width: 1.4,
          ),
        ),
        errorBorder: bordaDoCampo.copyWith(
          borderSide: const BorderSide(color: CoresDoAplicativo.perigo),
        ),
        focusedErrorBorder: bordaDoCampo.copyWith(
          borderSide: const BorderSide(
            color: CoresDoAplicativo.perigo,
            width: 1.4,
          ),
        ),
      ),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          minimumSize: const Size.fromHeight(52),
          backgroundColor: CoresDoAplicativo.verdeDestaque,
          foregroundColor: CoresDoAplicativo.fundoPrincipal,
          disabledBackgroundColor: CoresDoAplicativo.fundoDoCartaoSuave,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(RaiosDoAplicativo.medio),
          ),
          textStyle: TipografiaDoAplicativo.acao,
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          minimumSize: const Size.fromHeight(50),
          foregroundColor: CoresDoAplicativo.textoPrincipal,
          side: const BorderSide(color: CoresDoAplicativo.bordaSuave),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(RaiosDoAplicativo.medio),
          ),
          textStyle: TipografiaDoAplicativo.acao,
        ),
      ),
      textButtonTheme: TextButtonThemeData(
        style: TextButton.styleFrom(
          foregroundColor: CoresDoAplicativo.verdeDestaque,
          textStyle: TipografiaDoAplicativo.acao,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(RaiosDoAplicativo.pequeno),
          ),
        ),
      ),
      iconButtonTheme: IconButtonThemeData(
        style: IconButton.styleFrom(
          foregroundColor: CoresDoAplicativo.textoSecundario,
          highlightColor: CoresDoAplicativo.fundoDoCartaoSuave,
        ),
      ),
      chipTheme: ChipThemeData(
        backgroundColor: CoresDoAplicativo.fundoDoCartao,
        selectedColor: CoresDoAplicativo.verdeEscuro,
        side: const BorderSide(color: CoresDoAplicativo.bordaDiscreta),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(RaiosDoAplicativo.pilula),
        ),
        labelStyle: TipografiaDoAplicativo.legenda.copyWith(
          color: CoresDoAplicativo.textoSecundario,
        ),
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 5),
      ),
      navigationBarTheme: NavigationBarThemeData(
        height: 64,
        elevation: 0,
        backgroundColor: CoresDoAplicativo.transparente,
        indicatorColor: CoresDoAplicativo.fundoDoCartaoSuave,
        labelBehavior: NavigationDestinationLabelBehavior.alwaysShow,
        iconTheme: WidgetStateProperty.resolveWith<IconThemeData>(
          (Set<WidgetState> estados) {
            return IconThemeData(
              size: 22,
              color: estados.contains(WidgetState.selected)
                  ? CoresDoAplicativo.verdeDestaque
                  : CoresDoAplicativo.textoTerciario,
            );
          },
        ),
        labelTextStyle: WidgetStateProperty.resolveWith<TextStyle>(
          (Set<WidgetState> estados) {
            return TipografiaDoAplicativo.legenda.copyWith(
              color: estados.contains(WidgetState.selected)
                  ? CoresDoAplicativo.verdeDestaque
                  : CoresDoAplicativo.textoTerciario,
              fontWeight: estados.contains(WidgetState.selected)
                  ? FontWeight.w600
                  : FontWeight.w500,
            );
          },
        ),
      ),
      bottomSheetTheme: const BottomSheetThemeData(
        backgroundColor: CoresDoAplicativo.fundoElevado,
        modalBackgroundColor: CoresDoAplicativo.fundoElevado,
        surfaceTintColor: CoresDoAplicativo.transparente,
        showDragHandle: true,
        dragHandleColor: CoresDoAplicativo.bordaSuave,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(
            top: Radius.circular(RaiosDoAplicativo.extraGrande),
          ),
        ),
      ),
      dialogTheme: DialogThemeData(
        elevation: 0,
        backgroundColor: CoresDoAplicativo.fundoElevado,
        surfaceTintColor: CoresDoAplicativo.transparente,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(RaiosDoAplicativo.grande),
          side: const BorderSide(color: CoresDoAplicativo.bordaDiscreta),
        ),
      ),
      snackBarTheme: SnackBarThemeData(
        behavior: SnackBarBehavior.floating,
        backgroundColor: CoresDoAplicativo.fundoElevado,
        contentTextStyle: TipografiaDoAplicativo.corpo,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(RaiosDoAplicativo.medio),
        ),
      ),
      progressIndicatorTheme: const ProgressIndicatorThemeData(
        color: CoresDoAplicativo.verdeDestaque,
        linearTrackColor: CoresDoAplicativo.fundoDoCartaoSuave,
      ),
    );
  }
}
