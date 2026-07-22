# Junto - Flutter Web/PWA

## Executar preservando a sessao

Na raiz do repositorio, execute:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\inicie-aplicativo-web.ps1
```

Depois abra [http://localhost:5391](http://localhost:5391) no Chrome, Edge ou outro navegador instalado normalmente.

Esse modo usa o perfil normal do navegador e preserva o cookie seguro de renovacao da sessao por ate 30 dias.

Nao use `flutter run -d chrome` para validar persistencia entre execucoes. O Flutter inicia esse dispositivo com um perfil temporario do Chrome e o apaga quando a depuracao termina, removendo tambem seus cookies.

## API local

A API deve estar executando em:

```text
http://localhost:5281
```

O frontend aberto por `127.0.0.1` tambem usa automaticamente esse mesmo host para manter frontend, API e cookie no mesmo site.

## URL de API personalizada

Quando frontend e API estiverem em origens diferentes, informe explicitamente:

```powershell
flutter run -d web-server --web-hostname localhost --web-port 5391 --dart-define=URL_DA_API=https://api.exemplo.com
```

Em producao sem `URL_DA_API`, o frontend utiliza sua propria origem e acessa a API por `/api`.
