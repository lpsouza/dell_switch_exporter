FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 9116

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["dell_switch_exporter.csproj", "./"]
RUN dotnet restore "./dell_switch_exporter.csproj"
COPY . .
RUN dotnet build "dell_switch_exporter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dell_switch_exporter.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "dell_switch_exporter.dll"]
