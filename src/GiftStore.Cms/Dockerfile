FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 5050
ENV ASPNETCORE_URLS=http://+:5050

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["src/GiftStore.Contracts/GiftStore.Contracts.csproj", "src/GiftStore.Contracts/"]
COPY ["src/GiftStore.Cms/GiftStore.Cms.csproj", "src/GiftStore.Cms/"]
RUN dotnet restore "src/GiftStore.Cms/GiftStore.Cms.csproj"
COPY . .
WORKDIR "/src/src/GiftStore.Cms"
RUN dotnet build "GiftStore.Cms.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GiftStore.Cms.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GiftStore.Cms.dll"]
