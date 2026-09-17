FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["src/GiftStore.Contracts/GiftStore.Contracts.csproj", "src/GiftStore.Contracts/"]
COPY ["src/GiftStore.Domain/GiftStore.Domain.csproj", "src/GiftStore.Domain/"]
COPY ["src/GiftStore.Gifticard/GiftStore.Gifticard.csproj", "src/GiftStore.Gifticard/"]
COPY ["src/GiftStore.Payment/GiftStore.Payment.csproj", "src/GiftStore.Payment/"]
COPY ["src/GiftStore.Application/GiftStore.Application.csproj", "src/GiftStore.Application/"]
COPY ["src/GiftStore.Infrastructure/GiftStore.Infrastructure.csproj", "src/GiftStore.Infrastructure/"]
COPY ["src/GiftStore.Cms/GiftStore.Cms.csproj", "src/GiftStore.Cms/"]
COPY ["src/GiftStore.ServiceDefaults/GiftStore.ServiceDefaults.csproj", "src/GiftStore.ServiceDefaults/"]
COPY ["src/GiftStore.Web/GiftStore.Web.csproj", "src/GiftStore.Web/"]
RUN dotnet restore "src/GiftStore.Web/GiftStore.Web.csproj"
COPY . .
WORKDIR "/src/src/GiftStore.Web"
RUN dotnet build "GiftStore.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GiftStore.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GiftStore.Web.dll"]
