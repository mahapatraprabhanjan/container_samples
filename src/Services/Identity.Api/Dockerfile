FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 49489
EXPOSE 44336

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY src/Services/Identity.Api/Identity.Api.csproj src/Services/Identity.Api/
RUN dotnet restore src/Services/Identity.Api/Identity.Api.csproj
COPY . .
WORKDIR /src/src/Services/Identity.Api
RUN dotnet build Identity.Api.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Identity.Api.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Identity.Api.dll"]
