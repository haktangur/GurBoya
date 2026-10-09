FROM mcr.microsoft.com/dotnet/sdk:10.0.401@sha256:e70cdb7f80b0348f5cb85f19a8f670fca061f033d57eed12fa003d58b0e06317 AS build
WORKDIR /source
COPY global.json .
COPY src/GurBoya.Web/GurBoya.Web.csproj src/GurBoya.Web/packages.lock.json src/GurBoya.Web/
RUN dotnet restore src/GurBoya.Web --locked-mode
COPY src/ src/
RUN dotnet publish src/GurBoya.Web -c Release --no-restore -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0.9@sha256:7644f992230d35cf230017189d4038c0ae0f7388b13f4f7ae1900a155bafb597
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
USER $APP_UID
ENTRYPOINT ["dotnet", "GurBoya.Web.dll"]
