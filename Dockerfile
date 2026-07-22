FROM ghcr.io/cirruslabs/flutter:3.27.3 AS compilacao-flutter

WORKDIR /fonte

COPY src/ProjetoEncontros.AplicativoWeb/pubspec.yaml ./
COPY src/ProjetoEncontros.AplicativoWeb/pubspec.lock ./
RUN flutter pub get

COPY src/ProjetoEncontros.AplicativoWeb/ ./
RUN flutter analyze && \
    flutter test && \
    flutter build web --release

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS compilacao-api

WORKDIR /fonte

COPY global.json ./
COPY ProjetoEncontros.sln ./
COPY src/ProjetoEncontros.Api/ProjetoEncontros.Api.csproj src/ProjetoEncontros.Api/
COPY src/ProjetoEncontros.Aplicacao/ProjetoEncontros.Aplicacao.csproj src/ProjetoEncontros.Aplicacao/
COPY src/ProjetoEncontros.Dominio/ProjetoEncontros.Dominio.csproj src/ProjetoEncontros.Dominio/
COPY src/ProjetoEncontros.Infraestrutura/ProjetoEncontros.Infraestrutura.csproj src/ProjetoEncontros.Infraestrutura/
RUN dotnet restore src/ProjetoEncontros.Api/ProjetoEncontros.Api.csproj

COPY src/ src/
RUN dotnet publish src/ProjetoEncontros.Api/ProjetoEncontros.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /publicacao \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS execucao

WORKDIR /aplicacao

RUN apt-get update && \
    apt-get install --yes --no-install-recommends libgssapi-krb5-2 && \
    rm -rf /var/lib/apt/lists/* && \
    mkdir -p /aplicacao/dados/arquivos/perfis && \
    chown -R app:app /aplicacao

COPY --from=compilacao-api /publicacao ./
COPY --from=compilacao-flutter /fonte/build/web ./aplicativo-web

USER app

ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "ProjetoEncontros.Api.dll"]
